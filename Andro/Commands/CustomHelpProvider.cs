// Author: Deci | Project: Andro | Name: CustomHelpProvider.cs
// Date: 2025/06/10 @ 12:06:09

using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Andro.Commands;

public class CustomHelpProvider : HelpProvider
{

	public CustomHelpProvider(ICommandAppSettings settings)
		: base(settings) { }

	public override IEnumerable<IRenderable> GetUsage(ICommandModel model, ICommandInfo? command)
	{
		var usage = base.GetUsage(model, command);
		return usage;
	}

	/*public override IEnumerable<IRenderable> GetExamples(ICommandModel model, ICommandInfo? command)
	{
	        return
	        [
	                
	        ];

	        return base.GetExamples(model, command);
	}*/

	public override IEnumerable<IRenderable> GetDescription(ICommandModel model, ICommandInfo? command)
	{
		return new[]
		{
			Text.NewLine,
			new Text("DESCRIPTION:", new Style(Color.Yellow, decoration: Decoration.Bold)), Text.NewLine,
			new Text($"    Homepage: {R2.Url_Repo}", new Style(link: R2.Url_Repo)), Text.NewLine,
			new Text($"    Wiki: {R2.Url_Wiki}", new Style(link: R2.Url_Wiki)), Text.NewLine,
			Text.NewLine,
			Text.NewLine,
		};
	}

	public override IEnumerable<IRenderable> GetFooter(ICommandModel model, ICommandInfo? command)
	{
		return base.GetFooter(model, command);
	}

	/*public override IEnumerable<IRenderable> GetHeader(ICommandModel model, ICommandInfo? command)
	{

	        switch (command) {
	                case null:

	                        break;
	        }

	        return [Text.Empty];
	}*/

}