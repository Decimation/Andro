namespace Andro.Lib.Daemon;

public class AdbDevice
{

	public string? Serial { get; }


	public AdbDevice(string? serial = null)
	{
		Serial    = serial;
	}

	public bool IsDefault => String.IsNullOrWhiteSpace(Serial);


	public static implicit operator string?(AdbDevice device) => device.Serial;

	// public static implicit operator string?(AdbDevice device) => device.Serial;


	/*public async Task<Transport> SyncPrep(string p, string c)
	{
		var t = await GetTransport();
		await SendAsync(t, "sync:");
		var rg = AdbUtilities.GetPayload(c, out var rg1, out var rg2);

		// await t.Writer.WriteAsync(($"{c}{BinaryPrimitives.ReverseEndianness(p.Length):x4}{p}"));

		// await t.SendAsync($"{rg}{p}");
		await t.NetworkStream.WriteAsync(AdbUtilities.Encoding.GetBytes(c));
		var buffer = AdbUtilities.Encoding.GetBytes(p);
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