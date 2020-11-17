using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

namespace Andro
{
	public class CommandOperation : IDisposable
	{
		public Process Process { get; }

		public string OperationCommand { get; }


		public string[] StandardOutput { get; private set; }

		public string[] StandardError { get; private set; }


		public CommandOperation(string cmd)
		{
			Process          = Operations.RunCommand(cmd);
			OperationCommand = cmd;
		}

		public void Start()
		{
			Global.WriteDebug("Start {0}", OperationCommand);
			Process.Start();

			StandardOutput = Command.ReadAllLines(Process.StandardOutput);
			StandardError  = Command.ReadAllLines(Process.StandardError);
		}


		public void Dispose()
		{

			Global.WriteDebug("Dispose {0}", OperationCommand);
			Process.WaitForExit();
		}

		public static CommandOperation Run(string c)
		{
			var op = new CommandOperation(c);

			op.Start();

			return op;
		}


		public override string ToString()
		{
			var sb = new StringBuilder();

			

			sb.AppendCond(StandardOutput, "Stdout");
			sb.AppendCond(StandardError, "Stderr");
			
			return base.ToString();
		}
	}
}