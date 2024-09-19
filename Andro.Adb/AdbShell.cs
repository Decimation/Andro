// ReSharper disable RedundantUsingDirective.Global
// Read S Andro.Adb AdbShell.cs
// 2023-09-03 @ 4:24 PM
// global using R1 = Andro.Adb.Properties.Resources;


global using R = Andro.Adb.Properties.Resources;
global using SFM = JetBrains.Annotations.StringFormatMethodAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
using System.Runtime.CompilerServices;

using System.Text;
using CliWrap;
// ReSharper disable InconsistentNaming

namespace Andro.Adb;

public static class AdbShell
{

	public static readonly Command Cmd_adb = Cli.Wrap(R.Adb);

	public static Command BuildPush(string src, string dest, PipeTarget stdOut, PipeTarget stdErr)
	{
		// var sb = new StringBuilder();

		var r = Cmd_adb.WithArguments($"push \"{src}\" \"{dest}\"")
			.WithStandardOutputPipe(stdOut)
			.WithStandardErrorPipe(stdErr);

		return r;

	}


}