using Novus.Streams;
using JetBrains.Annotations;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;

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

	public async Task<AdbFilter> ShellAsync(string cmd, [CanBeNull] IEnumerable<string> args = null)
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
		return new AdbFilter(t.NetworkStream);
	}

	public class AdbFilter : FilterInputStream
	{
		public AdbFilter(InputStream s) : base(s) { }
		
		public new Stream BaseStream => base.BaseStream;
		
		public override int Read()
		{
			var b1 = base.Read();

			if (b1 == 0x0D) {
				base.Mark(1);
				var b2 = base.Read();

				if (b2 == 0x0A) {
					return b2;
				}

				base.Reset();
			}

			return b1;
		}

		public override int Read(byte[] buffer) => Read(buffer, 0, buffer.Length);

		public override int Read(byte[] buffer, int offset, int length)
		{
			int n = 0;

			for (int i = 0; i < length; i++) {
				int b = Read();
				if (b == -1) return n == 0 ? -1 : n;
				buffer[offset + n] = (byte) b;
				n++;

				// Return as soon as no more data is available (and at least one byte was read)
				if (Available() <= 0) {
					//todo?
					return n;
				}
			}

			return n;
		}
	}

	public override string ToString()
	{
		return $"{Serial}";
	}
}