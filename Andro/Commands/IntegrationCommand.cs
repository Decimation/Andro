// Author: Deci | Project: Andro | Name: IntegrationCommand.cs
// Date: 2025/05/30 @ 03:05:00

using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using Andro.App;
using Andro.IPC;
using Andro.Lib.Daemon;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Novus.Win32.Structures.User32;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class IntegrationCommand : AsyncCommand<IntegrationCommandSettings>
{

	private static readonly ILogger s_logger = AppIntegration.LoggerFactoryInt.CreateLogger(nameof(IntegrationCommand));

	[SupportedOSPlatform(AppIntegration.OS_WIN)]
	public override async Task<int> ExecuteAsync(CommandContext context, IntegrationCommandSettings settings)
	{
		bool? ok = null;

		var contextMenu = settings.ContextMenu;

		ok = HandleContextMenu(contextMenu);
		s_logger.LogDebug("Context menu: {CtxMenu} -> {CtxMenu2}", contextMenu, ok);

		if (contextMenu) { }

		var sendTo = settings.SendTo;

		ok = HandleSendToMenu(sendTo);
		s_logger.LogDebug("Send to: {SendTo} -> {SendTo2}", sendTo,ok);

		if (sendTo) { }

		// int res = ok.HasValue ? ok.Value ? 0 : -1 : -1;

		return 0;

	}

	[SupportedOSPlatform(AppIntegration.OS_WIN)]
	public static bool? HandleContextMenu(bool b)
	{
		// b ??= TryGetContextMenuSubKey(out RegistryKey reg) == null;
		bool? res = null;

		if (b) {
			RegistryKey shell    = null;
			RegistryKey main     = null;
			RegistryKey mainCmd  = null;
			RegistryKey first    = null;
			RegistryKey firstCmd = null;
			RegistryKey snd      = null;
			RegistryKey sndCmd   = null;

			string fullPath = AppIntegration.ExeLocation;

			//Computer\HKEY_CURRENT_USER\SOFTWARE\Classes\*\shell\atop

			try {

				shell = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell);

				if (shell != null) {
					shell.SetValue("MUIVerb", R1.Name);
					shell.SetValue("Icon", $"\"{fullPath}\"");
					shell.SetValue("subcommands", string.Empty);
				}

				main = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Main);

				if (main != null) {
					main.SetValue(null, "Main action");
					main.SetValue("CommandFlags", 0x00000040, RegistryValueKind.DWord);
				}

				mainCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Main_Cmd);
				mainCmd?.SetValue(null, $"\"{fullPath}\" \"%1\"");

				first = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_First);
				first?.SetValue(null, AdbTransport.DIR_SDCARD);

				firstCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_First_Cmd);
				firstCmd?.SetValue(null, $"\"{fullPath}\" {R2.Arg_Push} \"%1\" {AdbTransport.DIR_SDCARD}");

				snd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Snd);
				snd?.SetValue(null, "Clipboard");
				sndCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Snd_Cmd);
				sndCmd?.SetValue(null, $"\"{fullPath}\" {R2.Arg_Clipboard} \"%1\"");

				res = true;

			}
			catch (Exception ex) {
				Debug.WriteLine($"{ex.Message}");
				res = null;
			}
			finally {
				shell?.Close();
				main?.Close();
				mainCmd?.Close();
				first?.Close();
				firstCmd?.Close();
			}
		}
		else {
			var ok = TryGetContextMenuSubKey(out RegistryKey shell);

			if (ok) {
				shell.Close();
				Registry.CurrentUser.DeleteSubKeyTree(R2.Reg_Shell);
				res = false;
			}
		}

		return res;
	}

	[SupportedOSPlatform(AppIntegration.OS_WIN)]
	public static bool? HandleSendToMenu(bool b)
	{
		bool? res = null;

		var sendTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		                          @"AppData\Roaming\Microsoft\Windows\SendTo");

		s_logger.LogDebug("{ExeLoc}", AppIntegration.ExeLocation);
		var sendToFile = Path.Combine(sendTo, R1.Name_Shortcut);

		// b ??= !File.Exists(sendToFile);

		switch (b) {
			case true:
				// string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

				var link = (IShellLink) new ShellLink();

				// setup shortcut information
				// link.SetDescription("My Description");
				link.SetPath(AppIntegration.ExeLocation);
				// link.SetArguments(AndroPipeData.SendToDataSerialized);
				link.SetArguments(R2.Arg_PushAll);
				link.SetShowCmd((int) ShowCommands.SW_HIDE);

				// save it
				var file = (IPersistFile) link;

				// string       desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				file.Save(sendToFile, false);
				res = true;
				break;

			case false:
				var pp = sendToFile;
				File.Delete(pp);
				res = false;
				break;

		}

		return res;
	}

	// public static bool IsContextMenuAdded => Registry.CurrentUser.GetSubKeyNames().Any(k => k == R2.Reg_Shell);

	[SupportedOSPlatform(AppIntegration.OS_WIN)]
	public static bool TryGetContextMenuSubKey(out RegistryKey reg)
	{
		reg = Registry.CurrentUser.OpenSubKey(R2.Reg_Shell, RegistryRights.ReadKey);

		return reg != null;
	}

}

public class IntegrationCommandSettings : CommandSettings
{

	[CommandOption("--send-to")]
	public bool SendTo { get; set; }

	[CommandOption("--ctx-menu")]
	public bool ContextMenu { get; set; }

	public override ValidationResult Validate()
	{
		//todo

		// return (SendTo ^ ContextMenu);

		var vr = ValidationResult.Success();


		return vr;
	}

}