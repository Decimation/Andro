// Author: Deci | Project: Andro | Name: MutexCommand.cs
// Date: 2025/06/11 @ 14:06:15

using Andro.IPC;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class MutexCommand : AsyncCommand
{

#region Overrides of AsyncCommand

	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		AndroPipeManager.StartServer();

		return 0;
	}

#endregion

}