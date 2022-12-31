using JetBrains.Annotations;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Andro")]
[assembly: InternalsVisibleTo("UnitTest")]
namespace Andro.Lib.Android;

public class AdbDevice
{
	public string? Serial { get; }

	private ITransportFactory m_factory;

	internal AdbDevice(string? serial, ITransportFactory f)
	{
		Serial    = serial;
		m_factory = f;
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

	internal static State ConvertState(String type)
	{
		if (string.IsNullOrWhiteSpace(type)) {
			return State.Unknown;
		}

		var s = Enum.GetValues<State>()
			.FirstOrDefault(r => type.Equals(r.ToString(), StringComparison.InvariantCultureIgnoreCase));

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

	public async Task<Transport> GetTransport()
	{
		var t = await m_factory.GetTransport();

		try {
			await SendAsync(t, Serial == null ? "host:transport-any" : $"host:transport:{Serial}");
		}
		catch (Exception e) {
			t.Dispose();
			throw;
		}

		return t;
	}

	public async Task<State> GetState()
	{
		using var t = await m_factory.GetTransport();

		await SendAsync(t, Serial == null ? "host:get-state" : $"host-serial:{Serial}:get-state");
		return ConvertState(await t.ReadStringAsync());
	}

	private async Task SendAsync(Transport t, string c)
	{
		await t.SendAsync(c);
		await t.VerifyAsync();
	}

	public async Task<AdbFilterInputStream> ShellAsync(string cmd, IEnumerable<string>? args = null)
	{
		args ??= Enumerable.Empty<string>();
		var cmd2 = $"{cmd} {String.Join(' ', args.Select(AdbHelper.Escape))}";
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