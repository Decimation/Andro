using System;
using System.Diagnostics;
using System.Text;
using Andro.Core;
using Andro.Diagnostics;
using JetBrains.Annotations;
using Novus.Win32;

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

namespace Andro.Android.IO
{
	public class CommandResult : IDisposable
	{
		public Process Process { get; }

		public string[] StandardOutput { get; private set; }

		public string[] StandardError { get; private set; }

		public CommandPacket CommandPacket { get; }

		public CommandResult(CommandPacket cmd)
		{
			Process     = Commands.RunShellCommand(cmd);
			CommandPacket = cmd;
		}

		public void Start()
		{
			Global.WriteDebug("Start {0}", CommandPacket.FullCommand);
			Process.Start();

			StandardOutput = Command.ReadAllLines(Process.StandardOutput);
			StandardError  = Command.ReadAllLines(Process.StandardError);
		}


		public void Dispose()
		{

			Global.WriteDebug("Dispose {0}", CommandPacket.FullCommand);
			Process.WaitForExit();
		}


		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendFormat($"{CommandPacket.FullCommand}");

			sb.AppendRangeSafe(StandardOutput, "Standard output:\n");
			sb.AppendRangeSafe(StandardError, "Standard error:\n");

			return sb.ToString();
		}
	}
}