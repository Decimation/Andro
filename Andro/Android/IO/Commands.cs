using System.Diagnostics;
using Novus.Win32;

namespace Andro.Android.IO
{
	/// <summary>
	/// <list type="bullet">
	/// <item>
	/// <description><see cref="Commands"/></description>
	/// </item>
	/// <item>
	/// <description><see cref="AllCommands"/></description>
	/// </item>
	/// <item>
	/// <description><see cref="CommandPacket"/></description>
	/// </item>
	/// <item>
	/// <description><see cref="CommandResult"/></description>
	/// </item>
	/// 
	/// </list>
	/// </summary>
	public static class Commands
	{
		public static CommandResult RunCommand(CommandPacket packet)
		{
			var op = new CommandResult(packet);

			op.Start();

			return op;
		}

		public static Process RunShellCommand(CommandPacket packet)
		{
			var proc = Command.Shell(packet.FullCommand);

			return proc;
		}
	}
}