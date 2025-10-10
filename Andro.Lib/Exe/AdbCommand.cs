// ReSharper disable RedundantUsingDirective.Global
// Read S Andro.Lib AdbShell.cs
// 2023-09-03 @ 4:24 PM
// global using R1 = Andro.Lib.Properties.Resources;


using System.IO.Pipelines;
using CliWrap;

// ReSharper disable InconsistentNaming

namespace Andro.Lib.Exe;

public static class AdbCommand
{

	public static readonly Command CommandBase = Cli.Wrap(R1.Adb);

	public static Command Push(string src, string dest, PipeTarget stdOut, PipeTarget stdErr)
		=> CommandBase.WithArguments([R1.Cmd_Push, src, dest], true)
			.WithStandardOutputPipe(stdOut)
			.WithStandardErrorPipe(stdErr);

	public static Command Pull(string src, string dest, PipeTarget stdOut, PipeTarget stdErr)
		=> CommandBase.WithArguments([R1.Cmd_Pull, src, dest], true)
			.WithStandardOutputPipe(stdOut)
			.WithStandardErrorPipe(stdErr);

}