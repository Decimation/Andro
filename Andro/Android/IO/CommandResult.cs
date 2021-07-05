using System;
using System.Diagnostics;
using System.Text;
using Andro.Core;
using Andro.Diagnostics;
using JetBrains.Annotations;
using Novus.Win32;
using SimpleCore.Utilities;

#pragma warning disable IDE0079

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
			Trace.WriteLine($"Start {CommandPacket.FullCommand}");
			Process.Start();

			StandardOutput = Process.StandardOutput.ReadAllLines();
			StandardError  = Process.StandardError.ReadAllLines();
		}


		public void Dispose()
		{

			Trace.WriteLine($"Dispose {CommandPacket.FullCommand}");
			Process.WaitForExit();
			GC.SuppressFinalize(this);
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