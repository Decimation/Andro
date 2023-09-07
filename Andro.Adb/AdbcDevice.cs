// Read S Andro.Adb AdbcDevice.cs
// 2023-09-03 @ 4:24 PM

using System.Text;
using CliWrap;

namespace Andro.Adb;

public class AdbcDevice
{
	public string Id { get; }

	public static Command Adbc = CliWrap.Cli.Wrap("adb");

	public static async Task<AdbcDevice> Get()
	{
		return new AdbcDevice(); //todo
	}

	public async Task<string> Push(string f, string d)
	{
		var sb = new StringBuilder();

		var r = await Adbc.WithArguments($"push \"{f}\" \"{d}\"")
			        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb))
			        .ExecuteAsync();

		return sb.ToString();
	}
}