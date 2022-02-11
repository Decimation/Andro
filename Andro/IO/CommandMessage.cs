using System;
using System.Diagnostics;
using System.Text;
using Andro.Android;
using JetBrains.Annotations;

#pragma warning disable IDE0079


#nullable enable
namespace Andro.IO
{
	public enum CommandScope
	{
		/// <summary>
		/// <see cref="CommandMessage.ADB"/>
		/// </summary>
		Adb,

		/// <summary>
		/// <see cref="CommandMessage.ADB_SHELL"/>
		/// </summary>
		AdbShell
	}

	public readonly struct CommandMessage
	{
		public string Command { get; }

		public CommandScope Scope { get;  }

		public string FullCommand { get;  }


		public CommandMessage(string command) : this(CommandScope.Adb, command) { }


		[StringFormatMethod(AppIntegration.STRING_FORMAT_ARG)]
		public CommandMessage(string command, string? str = null, params object[] args)
			: this(CommandScope.Adb, command, str, args) { }


		[StringFormatMethod(AppIntegration.STRING_FORMAT_ARG)]
		public CommandMessage(CommandScope scope, string command, string? str = null, params object[] args)
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

		public static implicit operator CommandMessage(string command)
		{
			var p = new CommandMessage(command);

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

		public CommandResult Run()
		{
			var op = new CommandResult(this);

			op.Start();

			return op;
		}

		public Process RunShell()
		{
			var proc = Novus.OS.Command.Shell(FullCommand);

			return proc;
		}
	}
}