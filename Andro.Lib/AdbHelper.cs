using Andro.Lib.Daemon;
using Microsoft.Extensions.Logging;
using System.Text;


// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
#pragma warning disable IDE0049

namespace Andro.Lib;

public static class AdbHelper
{

	internal static readonly ILoggerFactory LoggerFactoryInt;

	static AdbHelper()
	{
		LoggerFactoryInt = LoggerFactory.Create(builder =>
		{
			builder.AddDebug();
			builder.AddTraceSource("TRACE");
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Debug);
		});
	}

	public static string Escape(string e)
	{
		return e.Replace(" ", "' '");
	}


	internal static AdbDeviceState ParseState(string type)
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

	public static byte[] GetPayload(string s)
	{
		var bc  = Encoding.GetByteCount(s);
		var str = $"{bc:x4}{s}";
		return Encoding.GetBytes(str);
	}

	public static Encoding Encoding { get; } = Encoding.UTF8;

	public static string[] ParseDevices(string body)
	{
		var lines   = body.Split(Environment.NewLine);
		var devices = new string[lines.Length];
		int i       = 0;

		foreach (string s in lines) {
			var parts = s.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 1) {
				devices[i++] = parts[0];
			}
		}

		return devices;
	}

}

public enum AdbDeviceState
{

	Unknown = 0,
	Offline,
	Device,
	Recovery,
	BootLoader,
	Unauthorized,
	Authorizing,
	Sideload,
	Connecting,
	Rescue

}