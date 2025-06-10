using System.Diagnostics;
using System.Runtime.CompilerServices;
using Andro.Adb.Diagnostics;

[assembly: InternalsVisibleTo("Andro")]
[assembly: InternalsVisibleTo("UnitTest")]

namespace Andro.Adb.Android;

public class AdbDevice : ITransportFactory
{
	public const string SDCARD = "sdcard/";

	public string? Serial { get; }

	private ITransportFactory m_factory;

	internal AdbDevice(string? serial, ITransportFactory f)
	{
		Serial    = serial;
		m_factory = f;
	}

	public async ValueTask<Transport> GetTransport()
	{
		var t = await m_factory.GetTransport();

		try {
			await SendAsync(t, Serial == null ? "host:transport-any" : $"host:transport:{Serial}");
		}
		catch (Exception e) {
			t.Dispose();
			throw new AdbException(message: null, innerException: e);
		}

		return t;
	}

	public async ValueTask<AdbDeviceState> GetStateAsync()
	{
		using var t = await m_factory.GetTransport();

		await SendAsync(t, Serial == null ? "host:get-state" : $"host-serial:{Serial}:get-state");
		return AdbHelper.ConvertState(await t.ReadStringAsync());
	}

	private async Task SendAsync(Transport t, string c)
	{
		await t.SendAsync(c);
		await t.VerifyAsync();
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

	public async ValueTask<string> ShellAsync(string cmd, IEnumerable<string>? args = null)
	{
		args ??= Enumerable.Empty<string>();
		var cmd2 = $"{cmd} {string.Join(' ', args.Select(AdbHelper.Escape))}";
		Trace.WriteLine($">> {cmd2}", nameof(ShellAsync));

		// await SendAsync($"{R.Cmd_Shell}{cmd2}");
		// await VerifyAsync();

		// var l = await Reader.ReadLineAsync();
		// return l;

		// var output = await Reader.ReadToEndAsync();
		// return output;
		var t = await GetTransport();
		await SendAsync(t, $"{R.Cmd_Shell}{cmd2}");
		return await t.Reader.ReadToEndAsync();
	}

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