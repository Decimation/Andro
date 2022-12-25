using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using Andro.Lib.Properties;
using Microsoft.Win32;
using Novus.OS;

#pragma warning disable CA1416
namespace Andro.App;

public static class AppIntegration
{
	static AppIntegration() { }
	
	/*
	 * HKEY_CLASSES_ROOT is an alias, a merging, of two other locations:
	 *		HKEY_CURRENT_USER\Software\Classes
	 *		HKEY_LOCAL_MACHINE\Software\Classes
	 */

	public static string ExeLocation => FileSystem.FindExecutableLocation(Resources.NameExe)!;

	internal const string STRING_FORMAT_ARG = "str";

	internal const string DEBUG_COND = "DEBUG";

	public static bool? HandleContextMenu(bool? b = null)
	{
		b ??= Registry.CurrentUser.OpenSubKey(Resources.Reg_Shell) == null;

		if (b.Value) {
			RegistryKey shell    = null;
			RegistryKey main     = null;
			RegistryKey mainCmd  = null;
			RegistryKey first    = null;
			RegistryKey firstCmd = null;

			string fullPath = ExeLocation;

			//Computer\HKEY_CURRENT_USER\SOFTWARE\Classes\*\shell\atop

			try {

				shell = Registry.CurrentUser.CreateSubKey(Resources.Reg_Shell);

				if (shell != null) {
					shell.SetValue("MUIVerb", Resources.Name);
					shell.SetValue("Icon", $"\"{fullPath}\"");
					shell.SetValue("subcommands", string.Empty);
				}

				main = Registry.CurrentUser.CreateSubKey(Resources.Reg_Shell_Main);

				if (main != null) {
					main.SetValue(null, "Main action");
					main.SetValue("CommandFlags", 0x00000040, RegistryValueKind.DWord);
				}

				mainCmd = Registry.CurrentUser.CreateSubKey(Resources.Reg_Shell_Main_Cmd);
				mainCmd?.SetValue(null, $"\"{fullPath}\" \"%1\"");

				first = Registry.CurrentUser.CreateSubKey(Resources.Reg_Shell_First);
				first?.SetValue(null, "sdcard/");

				firstCmd = Registry.CurrentUser.CreateSubKey(Resources.Reg_Shell_First_Cmd);
				firstCmd?.SetValue(null, $"\"{fullPath}\" {Program.PUSH} \"%1\" sdcard/");
				return true;

			}
			catch (Exception ex) {
				Debug.WriteLine($"{ex.Message}");
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
			var shell = Registry.CurrentUser.OpenSubKey(Resources.Reg_Shell);

			if (shell != null) {
				shell.Close();
				Registry.CurrentUser.DeleteSubKeyTree(Resources.Reg_Shell);
				return false;
			}

		}

		return null;
	}

	public static bool? HandleSendToMenu(bool? b= null)
	{

		var sendTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		                          @"AppData\Roaming\Microsoft\Windows\SendTo");

		Debug.WriteLine($"{AppIntegration.ExeLocation}");

		var sendToFile = Path.Combine(sendTo, Resources.NameShortcut);
		b ??= !File.Exists(sendToFile);
		switch (b) {
			case true:
				// string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

				var link = (IShellLink) new ShellLink();

				// setup shortcut information
				// link.SetDescription("My Description");
				link.SetPath(AppIntegration.ExeLocation);
				link.SetArguments(Program.PUSH_ALL);

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
}