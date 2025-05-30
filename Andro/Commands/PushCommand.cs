// Author: Deci | Project: Andro | Name: PushCommand.cs
// Date: 2025/05/30 @ 02:05:15

using System.ComponentModel;
using System.Text;
using Andro.Adb;
using Andro.Adb.Android;
using CliWrap;
using Kantan.Text;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class PushCommand : AsyncCommand<PushCommandOptions>
{

	public override async Task<int> ExecuteAsync(CommandContext context, PushCommandOptions settings)
	{
		var sb  = new StringBuilder();
		var sb2 = new StringBuilder();

		var cmd = AdbShell.BuildPush(settings.Source, settings.Destination,
		                             PipeTarget.ToStringBuilder(sb),
		                             PipeTarget.ToStringBuilder(sb2));

		var x = await cmd.ExecuteAsync();

		if (x.IsSuccess) {
			AnsiConsole.WriteLine($"{x} : {sb}");
		}

		return x.ExitCode;
	}

}

public class PushCommandOptions : CommandSettings
{

	[CommandOption("--source")]
	public string Source { get; set; }

	[DefaultValue(AdbDevice.SDCARD)]
	[CommandOption("--destination")]
	public string Destination { get; set; }

	public override ValidationResult Validate()
	{
		return base.Validate();
	}

}