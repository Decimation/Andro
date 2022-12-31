using System.Text;

namespace Andro.Lib.Android;

public static class AdbHelper
{
	public static readonly Encoding Encoding = Encoding.UTF8;

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

	private static readonly AdbDeviceState[] DeviceStatesValues = Enum.GetValues<AdbDeviceState>();

	internal static AdbDeviceState ConvertState(string type)
	{
		if (String.IsNullOrWhiteSpace(type)) {
			return AdbDeviceState.Unknown;
		}

		var s = DeviceStatesValues.FirstOrDefault(
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