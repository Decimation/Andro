namespace Andro.Lib.Daemon;

public class AdbDevice : IDisposable
{

	public string? Serial { get; }

	public AdbTransport Transport { get; }

	public AdbDevice(AdbTransport transport, string? serial = null)
	{
		Serial    = serial;
		Transport = transport;
	}

	public bool IsDefault => String.IsNullOrWhiteSpace(Serial);

	static AdbDevice() { }

	// public static implicit operator string?(AdbDevice device) => device.Serial;

	// public static implicit operator string?(AdbDevice device) => device.Serial;


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

	public ValueTask<AdbDeviceState> GetStateAsync() => Transport.GetStateAsync(Serial);

	public override string ToString()
	{
		return $"{Serial} : {Transport}";
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Transport.Dispose();
	}

}