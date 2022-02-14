using System;
using System.Diagnostics;
using System.Text;
using Andro.Utilities;
using Kantan.Utilities;

#pragma warning disable IDE0079

namespace Andro.Android;

public struct AdbCommandResult : IDisposable
{
	public Process Process { get; }

	public string[] StandardOutput { get; private set; }

	public string[] StandardError { get; private set; }

	public AdbCommand AdbCommand { get; }

	public AdbCommandResult(AdbCommand cmd)
	{
		Process        = cmd.RunShell();
		AdbCommand     = cmd;
		StandardError  = null;
		StandardOutput = null;
	}

	public void Start()
	{
		Trace.WriteLine($"Start {AdbCommand.FullCommand}");
		Process.Start();

		StandardOutput = Process.StandardOutput.ReadAllLines();
		StandardError  = Process.StandardError.ReadAllLines();
	}


	public void Dispose()
	{

		Trace.WriteLine($"Dispose {AdbCommand.FullCommand}");
		Process.WaitForExit();

		GC.SuppressFinalize(this);
	}


	public override string ToString()
	{
		var sb = new StringBuilder();

		sb.AppendFormat($"{AdbCommand.FullCommand}");

		sb.AppendRangeSafe(StandardOutput, "Standard output:\n");
		sb.AppendRangeSafe(StandardError, "Standard error:\n");

		return sb.ToString();
	}
}