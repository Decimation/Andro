using System.Runtime.CompilerServices;
using System.Text;
using Andro.Adb.Android;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
#pragma warning disable IDE0049

[assembly: InternalsVisibleTo("Andro")]
[assembly: InternalsVisibleTo("Test")]


namespace Andro.Adb;

public static class AdbHelper
{

	public static Encoding Encoding { get; set; } = Encoding.UTF8;

	public static string GetPayload(string s, out byte[] rg, out byte[] rg2)
	{
		rg = Encoding.GetBytes(s);
		var cm = $"{rg.Length:x4}{s}";
		rg2 = Encoding.GetBytes(cm);
		return cm;
	}

	public static string Escape(string e)
	{
		return e.Replace(" ", "' '");
	}


	internal static AdbDeviceState ConvertState(string type)
	{
		if (string.IsNullOrWhiteSpace(type)) {
			return AdbDeviceState.Unknown;
		}

		return Enum.Parse<AdbDeviceState>(type, true);

		/*return type switch
		{
			"device"       => State.Device,
			"offline"      => State.Offline,
			"bootloader"   => State.BootLoader,
			"recovery"     => State.Recovery,
			"unauthorized" => State.Unauthorized,
			"authorizing"  => State.Authorizing,
			"connecting"   => State.Connecting,
			"sideload"     => State.Sideload,
			"rescue"       => State.Rescue,
			_              => State.Unknown
		};*/
	}

}