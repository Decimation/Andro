using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andro.Core;
using Andro.Properties;
using Microsoft.Win32;
using Novus.OS.Win32;
using Kantan.Cli;
using Novus.OS;

#pragma warning disable CA1416
namespace Andro.Android
{
	public static class AppIntegration
	{
		private const string REG_SHELL = "SOFTWARE\\Classes\\*\\shell\\Andro";

		private const string REG_SHELL_MAIN     = "SOFTWARE\\Classes\\*\\shell\\Andro\\shell\\Main";
		private const string REG_SHELL_MAIN_CMD = "SOFTWARE\\Classes\\*\\shell\\Andro\\shell\\Main\\command";

		private const string REG_SHELL_FIRST     = "SOFTWARE\\Classes\\*\\shell\\Andro\\shell\\First";
		private const string REG_SHELL_FIRST_CMD = "SOFTWARE\\Classes\\*\\shell\\Andro\\shell\\First\\command";


		/*
		 * HKEY_CLASSES_ROOT is an alias, a merging, of two other locations:
		 *		HKEY_CURRENT_USER\Software\Classes
		 *		HKEY_LOCAL_MACHINE\Software\Classes
		 */


		public static string ExeLocation => FileSystem.FindExecutableLocation(Resources.NameExe)!;

		public static void Remove()
		{
			var shell = Registry.CurrentUser.OpenSubKey(REG_SHELL);

			if (shell != null) {
				shell.Close();
				Registry.CurrentUser.DeleteSubKeyTree(REG_SHELL);
			}
		}

		public static bool Add()
		{
			RegistryKey shell    = null;
			RegistryKey main     = null;
			RegistryKey mainCmd  = null;
			RegistryKey first    = null;
			RegistryKey firstCmd = null;


			string fullPath = ExeLocation;

			//Computer\HKEY_CURRENT_USER\SOFTWARE\Classes\*\shell\atop

			try {

				shell = Registry.CurrentUser.CreateSubKey(REG_SHELL);
				shell?.SetValue("MUIVerb", Resources.Name);
				shell?.SetValue("Icon", $"\"{fullPath}\"");
				shell?.SetValue("subcommands", string.Empty);


				main = Registry.CurrentUser.CreateSubKey(REG_SHELL_MAIN);
				main?.SetValue(null, "Main action");
				main?.SetValue("CommandFlags", 0x00000040, RegistryValueKind.DWord);


				mainCmd = Registry.CurrentUser.CreateSubKey(REG_SHELL_MAIN_CMD);
				mainCmd?.SetValue(null, $"\"{fullPath}\" \"%1\"");


				first = Registry.CurrentUser.CreateSubKey(REG_SHELL_FIRST);
				first?.SetValue(null, "sdcard/");


				firstCmd = Registry.CurrentUser.CreateSubKey(REG_SHELL_FIRST_CMD);
				firstCmd?.SetValue(null, $"\"{fullPath}\" push \"%1\" sdcard/");

			}
			catch (Exception ex) {
				ConsoleManager.Write($"{ex.Message}");
				return false;
			}
			finally {
				shell?.Close();
				main?.Close();
				mainCmd?.Close();
				first?.Close();
				firstCmd?.Close();
			}

			return true;
		}

		internal const string STRING_FORMAT_ARG = "str";

		internal const string DEBUG_COND = "DEBUG";
	}
}