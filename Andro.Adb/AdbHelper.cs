﻿using System.Text;
using Andro.Adb.Android;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
#pragma warning disable IDE0049

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

	private static readonly AdbDeviceState[] s_deviceStatesValues = Enum.GetValues<AdbDeviceState>();

	internal static AdbDeviceState ConvertState(string type)
	{
		if (string.IsNullOrWhiteSpace(type)) {
			return AdbDeviceState.Unknown;
		}

		var s = s_deviceStatesValues.FirstOrDefault(
			r => type.Equals(r.ToString(), StringComparison.InvariantCultureIgnoreCase));

		return s;

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