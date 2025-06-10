﻿// Author: Deci | Project: Andro | Name: ClipboardCommand.cs
// Date: 2025/05/30 @ 02:05:37

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using Andro.Adb;
using Andro.Adb.Android;
using Andro.App;
using CliWrap;
using Microsoft.Extensions.Logging;
using Novus.Win32;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class ClipboardCommand : AsyncCommand
{

	private static readonly ILogger s_logger = AppIntegration.LoggerFactoryInt.CreateLogger(nameof(ClipboardCommand));

	[SupportedOSPlatform(AppIntegration.OS_WIN)]
	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		var d = context.Arguments;

		s_logger.LogDebug("Clipboard {Args}", d);

		Clipboard.Open();
		var cbDragQuery = Clipboard.GetDragQueryList();

		await Parallel.ForEachAsync(cbDragQuery, async (s, token) =>
		{
			var sb  = new StringBuilder();
			var sb2 = new StringBuilder();

			var cmd = AdbCommand.BuildPush(s, AdbDevice.SDCARD,
			                               PipeTarget.ToStringBuilder(sb),
			                               PipeTarget.ToStringBuilder(sb2));

			var x = await cmd.ExecuteAsync(token);

			if (x.IsSuccess) {
				// ...
			}
		});

		Clipboard.Close();

		return 0;
	}

}