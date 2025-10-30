// Author: Deci | Project: Andro | Name: PushAllCommand.cs
// Date: 2025/05/30 @ 02:05:00

using System.Text;
using Andro.Lib.Daemon;
using Andro.Lib.Exe;
using CliWrap;
using Kantan.Text;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class PushAllCommand : AsyncCommand
{

	public override async Task<int> ExecuteAsync(CommandContext context,CancellationToken ct)
	{
		var files = context.Arguments;

		var progress = AnsiConsole.Progress()
			.Columns(new TaskDescriptionColumn(),
			         new ProgressBarColumn(),
			         new PercentageColumn(),
			         new SpinnerColumn())
			.AutoRefresh(true);

		var progTask = progress.StartAsync(async ctx =>
		{
			var sendTask = ctx.AddTask("Send", false, files.Count);

			int n = 0;

			/*
			var prg = new Progress<string>(handler)
				{ };
			*/
			sendTask.StartTask();

			await Parallel.ForEachAsync(files, async (s, token) =>
			{
				var sb  = new StringBuilder();
				var sb2 = new StringBuilder();

				var dest = AdbTransport.DIR_SDCARD;

				var cmd = AdbCommand.Push(s, dest,
				                               PipeTarget.ToStringBuilder(sb),
				                               PipeTarget.ToStringBuilder(sb2));

				var desc     = $"{s} {Strings.Constants.ARROW_RIGHT} {dest}";
				var fileTask = ctx.AddTask(desc, false);
				fileTask.IsIndeterminate = true;
				fileTask.StartTask();

				// fileTask.Increment(50D);
				var result = await cmd.ExecuteAsync(token);


				if (result.IsSuccess) {
					n++;
					sendTask.Increment(n);
					fileTask.Description = $"{desc} {Strings.Constants.HEAVY_CHECK_MARK}";
				}

				fileTask.IsIndeterminate = false;
				fileTask.Increment(100D);

				// fileTask.Increment(50D);
				fileTask.StopTask();

			});
			sendTask.StopTask();

		});
		await progTask;

		return 0;
	}

}