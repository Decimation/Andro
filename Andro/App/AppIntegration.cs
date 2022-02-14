using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using Andro.Properties;
using Kantan.Cli;
using Microsoft.Win32;
using Novus.OS;

#pragma warning disable CA1416
namespace Andro.App;

public static class AppIntegration
{
	static AppIntegration() { }


	private const string REG_SHELL = @"SOFTWARE\Classes\*\shell\Andro";

	private const string REG_SHELL_MAIN     = @"SOFTWARE\Classes\*\shell\Andro\shell\Main";
	private const string REG_SHELL_MAIN_CMD = @"SOFTWARE\Classes\*\shell\Andro\shell\Main\command";

	private const string REG_SHELL_FIRST     = @"SOFTWARE\Classes\*\shell\Andro\shell\First";
	private const string REG_SHELL_FIRST_CMD = @"SOFTWARE\Classes\*\shell\Andro\shell\First\command";


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
		b ??= Registry.CurrentUser.OpenSubKey(REG_SHELL) == null;

		if (b.Value) {
			RegistryKey shell    = null;
			RegistryKey main     = null;
			RegistryKey mainCmd  = null;
			RegistryKey first    = null;
			RegistryKey firstCmd = null;


			string fullPath = ExeLocation;

			//Computer\HKEY_CURRENT_USER\SOFTWARE\Classes\*\shell\atop

			try {

				shell = Registry.CurrentUser.CreateSubKey(REG_SHELL);

				if (shell != null) {
					shell.SetValue("MUIVerb", Resources.Name);
					shell.SetValue("Icon", $"\"{fullPath}\"");
					shell.SetValue("subcommands", string.Empty);
				}


				main = Registry.CurrentUser.CreateSubKey(REG_SHELL_MAIN);

				if (main != null) {
					main.SetValue(null, "Main action");
					main.SetValue("CommandFlags", 0x00000040, RegistryValueKind.DWord);
				}

				mainCmd = Registry.CurrentUser.CreateSubKey(REG_SHELL_MAIN_CMD);
				mainCmd?.SetValue(null, $"\"{fullPath}\" \"%1\"");


				first = Registry.CurrentUser.CreateSubKey(REG_SHELL_FIRST);
				first?.SetValue(null, "sdcard/");


				firstCmd = Registry.CurrentUser.CreateSubKey(REG_SHELL_FIRST_CMD);
				firstCmd?.SetValue(null, $"\"{fullPath}\" {Program.PUSH} \"%1\" sdcard/");
				return true;

			}
			catch (Exception ex) {
				ConsoleManager.Write($"{ex.Message}");
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
			var shell = Registry.CurrentUser.OpenSubKey(REG_SHELL);

			if (shell != null) {
				shell.Close();
				Registry.CurrentUser.DeleteSubKeyTree(REG_SHELL);
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
				break;
			case false:
				var pp = sendToFile;
				File.Delete(pp);
				return false;

				break;

		}

		return null;
	}
}