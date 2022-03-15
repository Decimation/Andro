using System;
using System.Diagnostics;
using System.Text;
using Andro.App;
using Andro.Utilities;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Text;
using Kantan.Utilities;
using Novus.OS;

#pragma warning disable IDE0079


#nullable disable
namespace Andro.Android;

public class AdbCommandOp
{
	public object[] Cmd { get; init; }

	public string Alias { get; init; }

	public AdbCommandOp() { }
}

[DebuggerDisplay("{StandardOutput} {StandardError} {BuiltCommand}")]
public class AdbCommand : IDisposable
{
	public string Value { get; }


	public string BuiltCommand { get; private set; }

	public bool IsBuilt => Process != null;

	public Process Process { get; internal set; }

	public string StandardOutput { get; internal set; }

	public string StandardError { get; internal set; }

	public string format;

	public List<object> Args { get; set; }


	[SFM(AppIntegration.STRING_FORMAT_ARG)]
	public AdbCommand(string s = AdbDevice.ADB, string fmt = null)
	{
		/*var args2 = new List<object>(args);
		args2.Insert(0, s);
		args = args2.ToArray();*/
		Process        = new Process() { };
		StandardError  = null;
		StandardOutput = null;
		format         = fmt;
		Value          = s;
	}

	public static readonly AdbCommand find = new(AdbDevice.ADB_SHELL, fmt: "find");

	public static readonly AdbCommand rm = new(AdbDevice.ADB_SHELL, "rm -f \"{0}\"");

	public override string ToString()
	{
		return $"{BuiltCommand} | {StandardOutput} | {StandardError} | {Success}";
	}

	public void Dispose()
	{
		// Trace.WriteLine($"Dispose {BuiltCommand}");

		Process?.WaitForExit();

		Process        = null;
		StandardError  = null;
		StandardOutput = null;
		GC.SuppressFinalize(this);
	}

	[CanBeNull]
	internal Predicate<AdbCommand> SuccessPredicate { get; set; }

	public bool? Success => SuccessPredicate?.Invoke(this);

	public void Start()
	{
		// Trace.WriteLine($"Start {BuiltCommand}");

		Process.Start();

		StandardOutput = Process.StandardOutput.ReadToEnd().Trim();
		StandardError  = Process.StandardError.ReadToEnd().Trim();
	}

	[MURV]
	public AdbCommand Build(bool start = true, string[] args2 = null, params object[] args)
	{
		var sb = new StringBuilder();

		sb.Append($"{Value} ");
		sb.AppendFormat(format, args);

		if (args2 is { }) {
			sb.Append(' ');

			foreach (string s in args2) {
				sb.Append($"{s} ");
			}

		}

		var cmdStr = sb.ToString();


		BuiltCommand = cmdStr;

		Process = Command.Shell(BuiltCommand);

		if (start) {
			Start();
		}

		Trace.WriteLine($"{BuiltCommand}");

		return this;
	}

	public static AdbCommand wc =
		new(AdbDevice.ADB_SHELL, "wc \"{0}\"")
		{
			SuccessPredicate = cmd =>
			{
				return !(cmd.StandardError != null &&
				         cmd.StandardError.Split('\n').Any(s => s.Contains("No such file")));
			}
		};


	public static AdbCommand pull(string remoteFile, [CBN] string destFileName)
	{
		return new("pull", $"\"{remoteFile}\"" + (destFileName == null ? String.Empty : $" \"{destFileName}\""))
		{
			SuccessPredicate = cmd => !(cmd.StandardError != null && cmd.StandardError.Contains("adb: error"))
		};
	}

	public static readonly AdbCommand devices = new(AdbDevice.ADB, "devices");

	public static readonly AdbCommand usb = new(AdbDevice.ADB, "usb");

	public static readonly AdbCommand tcpip = new(AdbDevice.ADB, "tcpip");

	public static AdbCommand push(string localSrcFile, string remoteDestFolder)
		=> new("push", $"\"{localSrcFile}\" \"{remoteDestFolder}\"");

	public static AdbCommand disconnect(string otherDevice) => new(AdbDevice.ADB, $"-s {otherDevice} disconnect");
}