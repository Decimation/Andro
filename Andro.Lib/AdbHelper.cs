using System.Runtime.CompilerServices;
using Andro.Lib.Daemon;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
#pragma warning disable IDE0049

namespace Andro.Lib;

public static class AdbHelper
{

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