using System.Diagnostics.CodeAnalysis;
// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

// ReSharper disable InconsistentNaming

namespace Andro.Android;

public static class AdbCommands
{
	public static AdbCommand ls(string s)
	{
		return new(AdbCommandScope.AdbShell, "ls", $"-p \"{s}\" | grep -v /")
			{ };
	}

	public static AdbCommand remove(string remoteFile) => new(AdbCommandScope.AdbShell, "rm", $"-f \"{remoteFile}\"");

	public static AdbCommand wc(string remoteFile)
	{
		return new(AdbCommandScope.AdbShell, "wc", $"\"{remoteFile}\"")
		{
			SuccessPredicate = cmd =>
			{
				return !(cmd.StandardError != null &&
				         cmd.StandardError.Any(s => s.Contains("No such file")));
			}
		};
	}

	public static AdbCommand pull(string remoteFile, [CBN] string destFileName)
	{
		return new("pull", $"\"{remoteFile}\"" + (destFileName == null ? String.Empty : $" \"{destFileName}\""))
		{
			SuccessPredicate = cmd => !(cmd.StandardError != null && cmd.StandardError.Contains("adb: error"))
		};
	}

	public static AdbCommand devices() => new("devices");

	public static AdbCommand usb() => new("usb");

	public static AdbCommand tcpip() => new("tcpip");

	public static AdbCommand push(string localSrcFile, string remoteDestFolder)
		=> new("push", $"\"{localSrcFile}\" \"{remoteDestFolder}\"");

	public static AdbCommand disconnect(string otherDevice) => new(AdbCommandScope.Adb, $"-s {otherDevice} disconnect");

	public const string ADB       = "adb";
	public const string ADB_SHELL = "adb shell";
	public const string TCPIP     = "5555";
}