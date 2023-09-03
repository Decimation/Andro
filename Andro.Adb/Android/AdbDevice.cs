using JetBrains.Annotations;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Andro.Adb.Utilities;
using Andro.Adb.Utilities;

[assembly: InternalsVisibleTo("Andro")]
[assembly: InternalsVisibleTo("UnitTest")]

namespace Andro.Adb.Android;

public class AdbDevice : ITransportFactory
{
	public string? Serial { get; }

	private ITransportFactory m_factory;

	internal AdbDevice(string? serial, ITransportFactory f)
	{
		Serial = serial;
		m_factory = f;
	}

	public async ValueTask<Transport> GetTransport()
	{
		var t = await m_factory.GetTransport();

		try
		{
			await SendAsync(t, Serial == null ? "host:transport-any" : $"host:transport:{Serial}");
		}
		catch (Exception e)
		{
			t.Dispose();
			throw new AdbException(message: null, innerException: e);
		}

		return t;
	}

	public async Task<AdbDeviceState> GetStateAsync()
	{
		using var t = await m_factory.GetTransport();

		await SendAsync(t, Serial == null ? "host:get-state" : $"host-serial:{Serial}:get-state");
		return AdbHelper.ConvertState(await t.ReadStringAsync());
	}

	public async Task<List<string>> list(string rp)
	{
		using var t = await GetTransport();
		var t2 = await t.startSync();
		await t2.Send("LIST", rp);
		var x = await t2.ReadString(Transport.SZ_LEN);

		return default;
	}

	private async Task SendAsync(Transport t, string c)
	{
		await t.SendAsync(c);
		await t.VerifyAsync();
	}

	public async Task<AdbFilterInputStream> ShellAsync(string cmd, IEnumerable<string>? args = null)
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
		return new AdbFilterInputStream(t.NetworkStream);
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