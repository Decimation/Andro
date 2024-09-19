// Read S Andro.Adb AdbShell.cs
// 2023-09-03 @ 4:24 PM

using System.Text;
using CliWrap;

namespace Andro.Adb;

public static class AdbShell
{

	public static readonly Command adb = CliWrap.Cli.Wrap(R.Adb);

	public static Command BuildPush(string src, string dest, PipeTarget stdOut, PipeTarget stdErr)
	{
		// var sb = new StringBuilder();

		var r = adb.WithArguments($"push \"{src}\" \"{dest}\"")
			.WithStandardOutputPipe(stdOut)
			.WithStandardErrorPipe(stdErr);

		return r;

	}


}