// ReSharper disable RedundantUsingDirective.Global
// Read S Andro.Adb AdbShell.cs
// 2023-09-03 @ 4:24 PM
// global using R1 = Andro.Adb.Properties.Resources;



using CliWrap;

// ReSharper disable InconsistentNaming

namespace Andro.Lib.Exe;

public static class AdbCommand
{

	public static readonly Command CommandBase = Cli.Wrap(R1.Adb);

	public static Command BuildPush(string src, string dest, PipeTarget stdOut, PipeTarget stdErr)
	{
		var r = CommandBase.WithArguments([R1.Cmd_Push, src, dest], true)
			.WithStandardOutputPipe(stdOut)
			.WithStandardErrorPipe(stdErr);

		return r;

	}

}