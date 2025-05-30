global using R1 = Andro.Adb.Properties.Resources;
global using R2 = Andro.Properties.Resources;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using Andro.Adb.Android;
using Andro.Adb.Properties;
using Andro.Comm;
using Microsoft.Win32;
using Novus.OS;
using Novus.Win32;
using Novus.Win32.Structures.User32;

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

}