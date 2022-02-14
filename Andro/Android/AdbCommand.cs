using System;
using System.Diagnostics;
using System.Text;
using Andro.Utilities;
using JetBrains.Annotations;

#pragma warning disable IDE0079


#nullable enable
namespace Andro.Android;

public enum CommandScope
{
	/// <summary>
	/// <see cref="AdbCommand.ADB"/>
	/// </summary>
	Adb,

	/// <summary>
	/// <see cref="AdbCommand.ADB_SHELL"/>
	/// </summary>
	AdbShell
}

public readonly struct AdbCommand
{
	
	public string Command { get; }

	public CommandScope Scope { get;  }

	public string FullCommand { get;  }


	public AdbCommand(string command) : this(CommandScope.Adb, command) { }


	[StringFormatMethod(AppIntegration.STRING_FORMAT_ARG)]
	public AdbCommand(string command, string? str = null, params object[] args)
		: this(CommandScope.Adb, command, str, args) { }


	[StringFormatMethod(AppIntegration.STRING_FORMAT_ARG)]
	public AdbCommand(CommandScope scope, string command, string? str = null, params object[] args)
	{
		Command = command;
		Scope   = scope;


		//

		var sb = new StringBuilder();

		sb.Append(Command);

		if (str != null) {

			sb.Append(' ');

			sb.AppendFormat(str, args);
		}

		var cmdStr = sb.ToString();

		//
		var strScope = CommandScopeToString(scope);
		FullCommand = $"{strScope} {cmdStr}";
	}

	public static implicit operator AdbCommand(string command)
	{
		var p = new AdbCommand(command);

		return p;
	}

	private static string CommandScopeToString(CommandScope scope)
	{
		return scope switch
		{
			CommandScope.Adb      => ADB,
			CommandScope.AdbShell => ADB_SHELL,
			_                     => throw new ArgumentOutOfRangeException(nameof(scope))
		};
	}

	public const string ADB = "adb";

	public const string ADB_SHELL = "adb shell";

	public const string TCPIP = "5555";


	public override string ToString()
	{
		var sb = new StringBuilder();

		sb.AppendFormat("({0}): {1}", Scope, FullCommand);

		return sb.ToString();
	}

	public AdbCommandResult Run()
	{
		var op = new AdbCommandResult(this);

		op.Start();

		return op;
	}

	public Process RunShell()
	{
		var proc = Novus.OS.Command.Shell(FullCommand);

		return proc;
	}

	public static class Commands
	{
		public const string CMD_USB     = "usb";
		public const string CMD_DEVICES = "devices";
		public const string CMD_TCPIP   = "tcpip";
		public const string CMD_PUSH    = "push";
		public const string CMD_PULL    = "pull";
		public const string CMD_RM      = "rm";
		public const string CMD_WC      = "wc";
		public const string CMD_LS      = "ls";
	}
}