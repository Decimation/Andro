// Author: Deci | Project: Andro | Name: IntegrationCommand.cs
// Date: 2025/05/30 @ 03:05:00

using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using Andro.Adb.Android;
using Andro.App;
using Andro.Comm;
using Microsoft.Win32;
using Novus.Win32.Structures.User32;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Andro.Commands;

public class IntegrationCommand : AsyncCommand<IntegrationCommandSettings>
{

	public static bool? HandleContextMenu(bool? b = null)
	{
		b ??= TryGetContextMenuSubKey() == null;

		if (b.Value) {
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
				first?.SetValue(null, AdbDevice.SDCARD);

				firstCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_First_Cmd);
				firstCmd?.SetValue(null, $"\"{fullPath}\" {R2.Arg_Push} \"%1\" {AdbDevice.SDCARD}");

				snd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Snd);
				snd?.SetValue(null, "Clipboard");
				sndCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Snd_Cmd);
				sndCmd?.SetValue(null, $"\"{fullPath}\" {R2.Arg_Clipboard} \"%1\"");

				return true;

			}
			catch (Exception ex) {
				Debug.WriteLine($"{ex.Message}");
				return false;
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
			var shell = TryGetContextMenuSubKey();

			if (shell != null) {
				shell.Close();
				Registry.CurrentUser.DeleteSubKeyTree(R2.Reg_Shell);
				return false;
			}

		}

		return null;
	}

	public static bool? HandleSendToMenu(bool? b = null)
	{

		var sendTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		                          @"AppData\Roaming\Microsoft\Windows\SendTo");

		Debug.WriteLine($"{AppIntegration.ExeLocation}");

		var sendToFile = Path.Combine(sendTo, R1.NameShortcut);
		b ??= !File.Exists(sendToFile);

		switch (b) {
			case true:
				// string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

				var link = (IShellLink) new ShellLink();

				// setup shortcut information
				// link.SetDescription("My Description");
				link.SetPath(AppIntegration.ExeLocation);
				link.SetArguments(AndroPipeData.SendToDataSerialized);
				link.SetShowCmd((int) ShowCommands.SW_HIDE);

				// save it
				var file = (IPersistFile) link;

				// string       desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				file.Save(sendToFile, false);
				return true;

			case false:
				var pp = sendToFile;
				File.Delete(pp);
				return false;

		}

		return null;
	}

#region Overrides of AsyncCommand<IntegrationCommandSettings>

	public override async Task<int> ExecuteAsync(CommandContext context, IntegrationCommandSettings settings)
	{
		if (settings.ContextMenu.HasValue) { }
	}

#endregion

	// public static bool IsContextMenuAdded => Registry.CurrentUser.GetSubKeyNames().Any(k => k == R2.Reg_Shell);

	public static bool TryGetContextMenuSubKey(out RegistryKey reg)
	{
		reg = Registry.CurrentUser.OpenSubKey(R2.Reg_Shell, RegistryRights.ReadKey);

		return reg != null;
	}

}

public class IntegrationCommandSettings : CommandSettings
{

	[CommandOption("--send-to")]
	public bool? SendTo { get; set; }

	[CommandOption("--ctx-menu")]
	public bool? ContextMenu { get; set; }

	public override ValidationResult Validate()
	{
		//todo

		// return (SendTo ^ ContextMenu);
		return ValidationResult.Success();
	}

}