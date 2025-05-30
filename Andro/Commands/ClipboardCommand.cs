// Author: Deci | Project: Andro | Name: ClipboardCommand.cs
// Date: 2025/05/30 @ 02:05:37

using System.Diagnostics;
using System.Text;
using Andro.Adb;
using Andro.Adb.Android;
using CliWrap;
using Novus.Win32;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class ClipboardCommand : AsyncCommand
{

	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		var d = context.Arguments;

		Debug.WriteLine($"clipboard arg mag : {d}");
		Clipboard.Open();
		var cbDragQuery = Clipboard.GetDragQueryList();

		await Parallel.ForEachAsync(cbDragQuery, async (s, token) =>
		{
			var sb  = new StringBuilder();
			var sb2 = new StringBuilder();

			var cmd = AdbShell.BuildPush(s, AdbDevice.SDCARD,
			                             PipeTarget.ToStringBuilder(sb),
			                             PipeTarget.ToStringBuilder(sb2));

			var x = await cmd.ExecuteAsync(token);

			if (x.IsSuccess) {
				// ...
			}
		});

		Clipboard.Close();
	}

}