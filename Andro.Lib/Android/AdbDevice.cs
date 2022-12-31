using System.Net.Sockets;

namespace Andro.Lib.Android;

public class AdbDevice
{
	public string? Serial { get; }

	public ITransportFactory Factory { get; }

	internal AdbDevice(string? serial, ITransportFactory f)
	{
		Serial  = serial;
		Factory = f;
	}

	public enum State
	{
		Unknown,
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

	private static State ConvertState(String type)
	{
		switch (type) {
			case "device":       return State.Device;
			case "offline":      return State.Offline;
			case "bootloader":   return State.BootLoader;
			case "recovery":     return State.Recovery;
			case "unauthorized": return State.Unauthorized;
			case "authorizing":  return State.Authorizing;
			case "connecting":   return State.Connecting;
			case "sideload":     return State.Sideload;
			case "rescue":       return State.Rescue;
			default:             return State.Unknown;
		}
	}

	public async Task<AdbTransport> GetTransport()
	{
		var t = await Factory.GetTransport();

		try {
			await SendAsync(t, Serial == null ? "host:transport-any" : "host:transport:" + Serial);
		}
		catch (Exception e) {
			t.Dispose();
			throw;
		}

		return t;
	}

	public async Task<State> GetState()
	{
		using var t = await Factory.GetTransport();

		await SendAsync(t, Serial == null ? "host:get-state" : "host-serial:" + Serial + ":get-state");
		return ConvertState(await t.ReadStringAsync());
	}

	private async Task SendAsync(AdbTransport t, string c)
	{
		await t.SendAsync(c);
		await t.VerifyAsync();
	}

	public override string ToString()
	{
		return $"{Serial}";
	}
}