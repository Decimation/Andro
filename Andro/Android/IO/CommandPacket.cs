using System;
using System.Text;
using JetBrains.Annotations;
using static Andro.Diagnostics.Global_Andro;
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

#nullable enable
namespace Andro.Android.IO
{
	public enum CommandScope
	{
		/// <summary>
		/// <see cref="CommandPacket.ADB"/>
		/// </summary>
		Adb,

		/// <summary>
		/// <see cref="CommandPacket.ADB_SHELL"/>
		/// </summary>
		AdbShell
	}

	public readonly struct CommandPacket
	{
		public string Command { get; }

		public CommandScope Scope { get;  }

		public string FullCommand { get;  }


		public CommandPacket(string command) : this(CommandScope.Adb, command) { }


		[StringFormatMethod(STRING_FORMAT_ARG)]
		public CommandPacket(string command, string? str = null, params object[] args)
			: this(CommandScope.Adb, command, str, args) { }


		[StringFormatMethod(STRING_FORMAT_ARG)]
		public CommandPacket(CommandScope scope, string command, string? str = null, params object[] args)
		{
			Command = command;
			Scope   = scope;


			//

			var sb = new StringBuilder();

			sb.Append(Command);

			if (str != null) {

				sb.Append(" ");

				sb.AppendFormat(str, args);
			}

			var cmdStr = sb.ToString();

			//
			var strScope = CommandScopeToString(scope);
			FullCommand = $"{strScope} {cmdStr}";
		}

		public static implicit operator CommandPacket(string command)
		{
			var p = new CommandPacket(command);

			return p;
		}

		private static string CommandScopeToString(CommandScope scope)
		{
			return scope switch
			{
				CommandScope.Adb      => ADB,
				CommandScope.AdbShell => ADB_SHELL,
				_                     => throw new ArgumentException()
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
	}
}