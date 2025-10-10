using System.Diagnostics;
using System.Runtime.CompilerServices;
using Andro.Lib.Diagnostics;

namespace Andro.Lib.Daemon;

public class AdbDevice
{

	public string? Serial { get; }


	internal AdbDevice(string? serial)
	{
		Serial = serial;
	}

	/*public async Task<Transport> SyncPrep(string p, string c)
	{
		var t = await GetTransport();
		await SendAsync(t, "sync:");
		var rg = AdbHelper.GetPayload(c, out var rg1, out var rg2);

		// await t.Writer.WriteAsync(($"{c}{BinaryPrimitives.ReverseEndianness(p.Length):x4}{p}"));

		// await t.SendAsync($"{rg}{p}");
		await t.NetworkStream.WriteAsync(AdbHelper.Encoding.GetBytes(c));
		var buffer = AdbHelper.Encoding.GetBytes(p);
		var bl     = buffer.Length;
		await t.NetworkStream.WriteAsync(BitConverter.GetBytes(bl));
		await t.NetworkStream.WriteAsync(buffer);
		t.NetworkStream.Flush();
		return t;
	}*/

	public override string ToString()
	{
		return $"{Serial}";
	}

}

public enum AdbDeviceState
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