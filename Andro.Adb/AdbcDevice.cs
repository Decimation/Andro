// Read S Andro.Adb AdbcDevice.cs
// 2023-09-03 @ 4:24 PM

using System.Text;
using CliWrap;

namespace Andro.Adb;

public class AdbcDevice
{

	public static Command Adbc = CliWrap.Cli.Wrap("adb");

	public static Task<CommandResult> Push(string f, string d, PipeTarget stdOut, CancellationToken ct = default)
	{
		// var sb = new StringBuilder();

		var r = Adbc.WithArguments($"push \"{f}\" \"{d}\"")
			.WithStandardOutputPipe(stdOut)
			.ExecuteAsync(ct);

		return r;

	}

}