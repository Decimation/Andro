using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Andro.Utilities;
using JetBrains.Annotations;
using Kantan.Cli;
using Kantan.Numeric;
using Kantan.Text;
#nullable disable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Kantan.Collections;
using Novus.OS.Win32;
using Kantan.Diagnostics;
using Kantan.Utilities;
using Novus.OS;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0079

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0602 // Delegate on struct instance caused a boxing allocation
#pragma warning disable HAA0603 // Delegate allocation from a method group
#pragma warning disable HAA0604 // Delegate allocation from a method group

#pragma warning disable HAA0501 // Explicit new array type allocation
#pragma warning disable HAA0502 // Explicit new reference type allocation
#pragma warning disable HAA0503 // Explicit new reference type allocation
#pragma warning disable HAA0504 // Implicit new array creation allocation
#pragma warning disable HAA0505 // Initializer reference type allocation
#pragma warning disable HAA0506 // Let clause induced allocation

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0302 // Display class allocation to capture closure
#pragma warning disable HAA0303 // Lambda or anonymous method in a generic method allocates a delegate instance

#pragma warning disable HAA0101

namespace Andro.Android;

public enum AdbConnectionMode
{
	USB,
	TCPIP,
	UNKNOWN
}

public class AdbDevice
{
	public string Name { get; }

	public AdbConnectionMode Mode { get; }

	static AdbDevice() { }

	public AdbDevice(string name, AdbConnectionMode mode = AdbConnectionMode.UNKNOWN)
	{
		Name = name;

		Mode = mode == AdbConnectionMode.UNKNOWN ? ResolveConnectionMode(name) : mode;

		Require.Assert(AvailableDeviceNames.Contains(name));
	}

	public static AdbDevice First => FirstName is { } ? new AdbDevice(FirstName) : throw new AdbException();

	[CanBeNull]
	public static string FirstName
	{
		get
		{
			var first = AvailableDeviceNames;
			return first.FirstOrDefault();
		}
	}

	public static string[] AvailableDeviceNames
	{
		get
		{
			var proc = AdbCommand.devices.Build();
			proc.Start();
			// Skip first line

			var stdOut = proc.StandardOutput.Trim().Split(OUT_SPLIT).Skip<string>(1)
			                 .Where(s => !String.IsNullOrWhiteSpace(s))
			                 .Select(s => s.Split('\t')[0])
			                 .ToArray();

			return stdOut;
		}
	}

	public AdbCommand GetItems(string folder)
	{
		EnsureDevice();

		var packet = AdbCommand.find.Build(args2: new[] { folder });

		return packet;
	}

	public int GetFileSize(string remoteFile, Process p = null)
	{
		EnsureDevice();

		var args = $"wc \"{remoteFile}\""; //todo

		p ??= GetShell($"{ADB_SHELL} {args}");

		var input = TextWriter.Synchronized(p.StandardInput);

		input.WriteLine(args);
		input.Flush();

		string s = null, s2 = null;

		var reader  = TextReader.Synchronized(p.StandardOutput);
		var reader2 = TextReader.Synchronized(p.StandardError);

		// StreamReader reader = p.StandardOutput;
		// var reader2 = p.StandardError;
		s = reader.ReadToEnd();

		if (String.IsNullOrWhiteSpace(s) || s.Contains("Is a directory")) {
			return 0;
		}

		var output = s.Split(' ');

		var b = Int32.TryParse(output[2], out var bytes);

		return bytes;

	}

	public static Process GetShell(string args)
	{
		var p = Command.Shell(args);
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.RedirectStandardError = true;
		p.Start();
		return p;
	}

	public int GetFolderSize(string f)
	{
		EnsureDevice();

		using var cmd = GetItems(f);

		var files = cmd.StandardOutput.Split(OUT_SPLIT)[1..];

		for (int i = 0; i < files.Length; i++) {
			files[i] = esca(files[i]);
		}

		var cb = 0;

		var sw = Stopwatch.StartNew();
		var ps = GetShell(ADB_SHELL);

		var c = 0;

		Parallel.For(0, files.Length, (i, plr) =>
		{
			// var i2   = MathHelper.Wrap(i, ps.Length);
			var size = GetFileSize(files[i]);

			cb += size;
			c++;

			Console.Write($"{Strings.Constants.ClearLine}{cb} - {((double) c / files.Length):Pd/d}");
		});

		sw.Stop();
		Console.WriteLine($"\n{sw.Elapsed.TotalSeconds}");
		return cb;
	}

	private static string esca(string s)
	{
		return s.Replace(" ", "\\ ");
	}

	public bool FileExists(string remoteFile)
	{
		EnsureDevice();

		using var packet = AdbCommand.wc.Build(args: remoteFile);

		var fs = GetFileSize(remoteFile);

		return fs != Native.INVALID;
	}

	public AdbCommand RemoveFile(string remoteFile)
	{
		EnsureDevice();

		using var cmd = AdbCommand.rm.Build(args: remoteFile);

		return cmd;
	}

	/// <summary>
	/// Ensures only <see cref="Name"/> is connected
	/// </summary>
	public bool IsConnected()
	{
		if (!_ensure) return true;

		return IsConnected(Name);
	}

	public void EnsureDevice()
	{
		Require.Assert<AdbException>(IsConnected());
	}

	/// <summary>
	/// Ensures only <paramref name="name"/> is connected
	/// </summary>
	public static bool IsConnected(string name)
	{
		return AvailableDeviceNames.Contains(name);
	}

	public AdbCommand[] RunIOParallel(Func<string, string, AdbCommand> transfer, string[] files, string dest,
	                                  Func<string, long> getSize = null)
	{
		var cb1 = 0L;
		var len = files.Length;
		var bag = new ConcurrentBag<AdbCommand>();
		var sw  = Stopwatch.StartNew();

		_ensure = false;

		getSize ??= _ => 0;

		var err = 0;

		var plr = Parallel.For(0, len, (i, pls) =>
		{
			var file = files[i];
			var cmd  = transfer(file, dest);

			bag.Add(cmd);

			if (cmd.Success.HasValue && !cmd.Success.Value)
				err++;
			else
				cb1 += getSize(file);

			Console.Write($"{Strings.Constants.ClearLine}{bag.Count}/{len} | {err} | " +
			              $"{MathHelper.GetByteUnit(cb1)} | " +
			              $"{sw.Elapsed.TotalSeconds:F3} sec ");
		});

		Console.WriteLine();
		sw.Stop();
		_ensure = true;
		return bag.ToArray();
	}

	public AdbCommand Pull(string remoteFile, string localDestFolder)
	{
		EnsureDevice();

		var destFileName = localDestFolder;
		var fileName     = Path.GetFileName(remoteFile);
		destFileName = Path.Combine(destFileName, fileName);

		var packet = AdbCommand.pull(remoteFile, destFileName);

		var cmd = packet.Build(false);
		cmd.Process.StartInfo.WorkingDirectory = localDestFolder;
		cmd.Start();

		return cmd;
	}

	public AdbCommand Push(string localSrcFile, string remoteDestFolder)
	{
		EnsureDevice();

		var packet = AdbCommand.push(localSrcFile, remoteDestFolder);

		var cmd = packet.Build();

		return cmd;
	}

	public Task<AdbCommand> PushAsync(string localSrcFile, string remoteDestFolder)
	{
		return Task.Run(() => Push(localSrcFile, remoteDestFolder));
	}

	public AdbCommand[] PushFolder(string localSrcFolder, string remoteDestFolder)
	{
		EnsureDevice();

		var files = Directory.GetFiles(localSrcFolder);

		return PushAll(files, remoteDestFolder);
	}

	public AdbCommand[] PullAll(string remFolder, string destFolder)
	{
		EnsureDevice();

		using var result = GetItems(remFolder);

		return RunIOParallel(Pull, result.StandardOutput.Split(OUT_SPLIT), destFolder, s =>
		{
			var fileInfo = new FileInfo(Path.Combine(destFolder, Path.GetFileName(s)));
			return fileInfo.Length;
		});
	}

	public AdbCommand[] PushAll(string[] files, string destFolder = SDCARD)
	{
		EnsureDevice();
		return RunIOParallel(Push, files, destFolder);
	}

	public override string ToString()
	{
		var sb = new StringBuilder();

		sb.AppendFormat($"Name: {Name}| Mode: {Mode}");

		return sb.ToString();
	}

	private const string SDCARD = "sdcard/";

	private const char OUT_SPLIT = '\n';

	private static bool _ensure;

	public const string ADB       = "adb";
	public const string ADB_SHELL = "adb shell";
	public const string TCPIP     = "5555";

	private static AdbConnectionMode ResolveConnectionMode(string deviceName)
	{
		var isIPAddr = IPAddress.TryParse(deviceName, out _);

		return isIPAddr ? AdbConnectionMode.TCPIP : AdbConnectionMode.USB;
	}

	public static AdbDevice SetConnectionMode(AdbConnectionMode mode)
	{
		//

		var packet = mode switch
		{
			AdbConnectionMode.USB   => AdbCommand.usb.Build(),
			AdbConnectionMode.TCPIP => AdbCommand.tcpip.Build(),

			_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
		};

		/*
		 * Wait a bit for the device to connect
		 */

		Thread.Sleep(TimeSpan.FromSeconds(3));

		//

		var devices = AvailableDeviceNames;

		var deviceName = devices.First();

		var device = new AdbDevice(deviceName, mode);

		return device;
	}
}