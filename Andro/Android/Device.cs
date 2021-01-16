#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Andro.Android.IO;
using Andro.Core;
using Andro.Diagnostics;
using JetBrains.Annotations;
using Novus;
using Novus.Win32;
using static Andro.Android.IO.AllCommands;
using static Andro.Android.IO.CommandResult;

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

namespace Andro.Android
{
	public enum ConnectionMode
	{
		USB,
		TCPIP,
		UNKNOWN
	}

	public class Device
	{
		public string DeviceName { get; }

		public ConnectionMode Mode { get; }

		public Device() : this(FirstAvailableDevice) { }

		public Device(string deviceName, ConnectionMode mode = ConnectionMode.UNKNOWN)
		{
			DeviceName = deviceName;

			Mode = mode == ConnectionMode.UNKNOWN ? ResolveConnectionMode(deviceName) : mode;

			GuardAdb.AssertDeviceAvailable(AvailableDevices, deviceName);
		}

		private static ConnectionMode ResolveConnectionMode(string deviceName)
		{
			var isIPAddr = IPAddress.TryParse(deviceName, out var ip);

			return isIPAddr ? ConnectionMode.TCPIP : ConnectionMode.USB;
		}

		public static string FirstAvailableDevice
		{
			get
			{
				var d = AvailableDevices.FirstOrDefault();

				if (d == null) {
					throw new AdbException();
				}

				return d;
			}
		}

		public static string[] AvailableDevices
		{
			get
			{
				using var proc = Commands.RunCommand(CMD_DEVICES);


				// Skip first line
				var stdOut = proc.StandardOutput
					.Skip(1)
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Select(s => s.Split('\t')[0])
					.ToArray();

				return stdOut;
			}
		}


		public static Device SetConnectionMode(ConnectionMode mode)
		{
			//

			var packet = mode switch
			{
				ConnectionMode.USB   => new CommandPacket(CMD_USB),
				ConnectionMode.TCPIP => new CommandPacket(CMD_TCPIP, CommandPacket.TCPIP),
				_                    => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
			};

			Commands.RunCommand(packet);

			/*
			 * Wait a bit for the device to connect
			 */

			Thread.Sleep(TimeSpan.FromSeconds(3));


			//

			var devices = AvailableDevices;

			GuardAdb.AssertSingleDevice(devices);

			var deviceName = devices.First();

			var device = new Device(deviceName, mode);

			return device;
		}

		public int GetFileSize(string remoteFile)
		{
			EnsureDevice();

			var packet = new CommandPacket(CommandScope.AdbShell, CMD_WC, $"\"{remoteFile}\"");

			using var cmd = Commands.RunCommand(packet);

			if (cmd.StandardError != null && cmd.StandardError.Any(s => s.Contains("No such file"))) {
				return Native.INVALID;
			}

			if (cmd.StandardOutput.Any()) {

				var output = cmd.StandardOutput.First().Split(' ');

				// var lines  = int.Parse(output[0]);
				// var words  = int.Parse(output[1]);
				var bytes = int.Parse(output[2]);
				// var file   = output[3];


				return bytes;
			}

			return Native.INVALID;
		}

		public bool FileExists(string remoteFile)
		{
			EnsureDevice();

			var packet = new CommandPacket(CommandScope.AdbShell, CMD_WC, $"\"{remoteFile}\"");

			using var cmd = Commands.RunCommand(packet);

			var fs = GetFileSize(remoteFile);

			return fs != Native.INVALID;
		}

		public void Remove(string remoteFile)
		{
			EnsureDevice();

			var packet = new CommandPacket(CommandScope.AdbShell, CMD_RM, $"-f \"{remoteFile}\"");

			using var cmd = Commands.RunCommand(packet);
		}

		public string Pull(string remoteFile, string localDestFolder)
		{
			EnsureDevice();

			var fileName = Path.GetFileName(remoteFile);

			var destFileName = Path.Combine(localDestFolder, fileName);

			var packet = new CommandPacket(CMD_PULL, $"\"{remoteFile}\" \"{destFileName}\"");

			using var cmd = Commands.RunCommand(packet);

			return destFileName;
		}

		public string[] GetFiles(string remoteFolder)
		{
			EnsureDevice();

			var packet = new CommandPacket(CommandScope.AdbShell, CMD_LS, $"-p \"{remoteFolder}\" | grep -v /");

			using var cmd = Commands.RunCommand(packet);

			var output = cmd.StandardOutput;

			return output;
		}

		/// <summary>
		/// Ensures only <see cref="DeviceName"/> is connected
		/// </summary>
		public void EnsureDevice() => EnsureDevice(DeviceName);

		/// <summary>
		/// Ensures only <paramref name="name"/> is connected
		/// </summary>
		public static void EnsureDevice(string name)
		{
			Debug.WriteLine($"Ensuring devices {name}");

			var otherDevices = AvailableDevices.Where(n => n != name).ToArray();

			foreach (string otherDevice in otherDevices) {
				Debug.WriteLine($"Disconnecting {otherDevice}");

				var       packet = new CommandPacket(CommandScope.Adb, $"-s {otherDevice} disconnect");
				using var cmd    = Commands.RunCommand(packet);
			}
		}

		// public static void RunWithDevice(string n, Action fn)
		// {
		// 	EnsureDevice(n);
		// 	fn();
		// }

		public void Push(string localSrcFile, string remoteDestFolder)
		{
			EnsureDevice();

			var packet = new CommandPacket(CMD_PUSH, $"\"{localSrcFile}\" \"{remoteDestFolder}\"");
			
			
			using var cmd = Commands.RunCommand(packet);
			
		}


		public void PushAll(string localSrcFolder, string remoteDestFolder)
		{
			EnsureDevice();

			var files = Directory.GetFiles(localSrcFolder);

			foreach (string file in files) {
				var fi = new FileInfo(file);

				Global_Andro.Write("Push {0} -> {1}", fi.Name, remoteDestFolder);

				Push(file, remoteDestFolder);
				//Console.ReadLine();
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendFormat($"Name: {DeviceName}\n")
				.AppendFormat($"Mode: {Mode}\n");

			return sb.ToString();
		}
	}
}