using System;
using System.Diagnostics;
using System.Text;
using Andro.Core;
using Kantan.Utilities;

#pragma warning disable IDE0079

namespace Andro.IO
{
	public class CommandResult : IDisposable
	{
		public Process Process { get; }

		public string[] StandardOutput { get; private set; }

		public string[] StandardError { get; private set; }

		public CommandMessage CommandMessage { get; }

		public CommandResult(CommandMessage cmd)
		{
			Process       = cmd.RunShell();
			CommandMessage = cmd;
		}

		public void Start()
		{
			Trace.WriteLine($"Start {CommandMessage.FullCommand}");
			Process.Start();

			StandardOutput = Process.StandardOutput.ReadAllLines();
			StandardError  = Process.StandardError.ReadAllLines();
		}


		public void Dispose()
		{

			Trace.WriteLine($"Dispose {CommandMessage.FullCommand}");
			Process.WaitForExit();
			GC.SuppressFinalize(this);
		}


		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendFormat($"{CommandMessage.FullCommand}");

			sb.AppendRangeSafe(StandardOutput, "Standard output:\n");
			sb.AppendRangeSafe(StandardError, "Standard error:\n");

			return sb.ToString();
		}
	}
}