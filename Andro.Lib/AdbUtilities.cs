using Andro.Lib.Daemon;
using Microsoft.Extensions.Logging;
using System.Text;


// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess
#pragma warning disable IDE0049

namespace Andro.Lib;

public static class AdbUtilities
{

	internal static readonly ILoggerFactory LoggerFactoryInt;

	static AdbUtilities()
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