global using R1 = Andro.Adb.Properties.Resources;
global using R2 = Andro.Resources;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using Andro.Adb.Properties;
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

	public static string ExeLocation => FileSystem.FindExecutableLocation(R1.NameExe);

	internal const string STRING_FORMAT_ARG = "str";

	internal const string DEBUG_COND = "DEBUG";

	public static bool IsContextMenuAdded
	{
		get
		{
			var reg = Registry.CurrentUser.OpenSubKey(R2.Reg_Shell);
			return reg != null;
		}
	}

	public static bool? HandleContextMenu(bool? b = null)
	{
		b ??= Registry.CurrentUser.OpenSubKey(R2.Reg_Shell) == null;

		if (b.Value) {
			RegistryKey shell    = null;
			RegistryKey main     = null;
			RegistryKey mainCmd  = null;
			RegistryKey first    = null;
			RegistryKey firstCmd = null;
			RegistryKey snd    = null;
			RegistryKey sndCmd = null;

			string      fullPath = ExeLocation;

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
				first?.SetValue(null, "sdcard/");

				firstCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_First_Cmd);
				firstCmd?.SetValue(null, $"\"{fullPath}\" {R2.Arg_Push} \"%1\" sdcard/");

				snd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Snd);
				snd?.SetValue(null, "Clipboard");
				sndCmd = Registry.CurrentUser.CreateSubKey(R2.Reg_Shell_Snd_Cmd);
				sndCmd?.SetValue(null, $"\"{fullPath}\" {R2.Arg_Clipboard} \"%1\"");

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
			var shell = Registry.CurrentUser.OpenSubKey(R2.Reg_Shell);

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

		Debug.WriteLine($"{ExeLocation}");

		var sendToFile = Path.Combine(sendTo, R1.NameShortcut);
		b ??= !File.Exists(sendToFile);

		switch (b) {
			case true:
				// string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

				var link = (IShellLink) new ShellLink();

				// setup shortcut information
				// link.SetDescription("My Description");
				link.SetPath(ExeLocation);
				link.SetArguments(R2.Arg_PushAll);

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