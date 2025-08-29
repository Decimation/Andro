#region Global usings

global using R1 = Andro.Lib.Properties.Resources;
global using SFM = JetBrains.Annotations.StringFormatMethodAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using CA = JetBrains.Annotations.ContractAnnotationAttribute;
global using AC = JetBrains.Annotations.AssertionConditionAttribute;
global using ACT = JetBrains.Annotations.AssertionConditionType;

#endregion

#nullable disable

using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Andro.Lib.Properties;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Andro.Lib.Diagnostics;
using Novus.Memory;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0079

[assembly: InternalsVisibleTo("Andro")]
[assembly: InternalsVisibleTo("UnitTest")]
[assembly: InternalsVisibleTo("Test")]


namespace Andro.Lib.Daemon;

public class Transport : IDisposable
{

#region

	public const string HOST_DEFAULT = "localhost";

	public const int PORT_DEFAULT = 5037;

#endregion

#region

	public const int SZ_LEN = sizeof(uint);

	public const string S_OKAY = "OKAY";

	public const string S_FAIL = "FAIL";

#endregion

#region

	public StreamWriter Writer { get; }

	public StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; }

	public TcpClient Tcp { get; }

	public static Encoding Encoding { get; } = Encoding.UTF8;

	public bool IsAlive => Tcp.Connected;

#endregion

	public Transport(string host, int port)
	{
		Tcp = new TcpClient(host, port) { };

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream, Encoding);
		Reader = new StreamReader(NetworkStream, Encoding, false);

		// Writer.AutoFlush = true;

		/*var sock = new Socket(SocketType.Stream, ProtocolType.Tcp) { };
		await sock.ConnectAsync("localhost", 5037);*/
	}

	static Transport()
	{
		_transports = new ConcurrentDictionary<string, Transport>();
	}

	private static readonly ConcurrentDictionary<string, Transport> _transports;

	

	public ValueTask<int> SendAsync(string s, CancellationToken t = default)
	{
		var rg2 = GetPayload(s);

		return Tcp.Client.SendAsync(rg2, t);
	}

	/*public async Task<SyncTransport> StartSyncAsync()
	{
		await SendAsync("sync:");
		await VerifyAsync();
		return new SyncTransport(Reader, Writer);
	}*/

	public async ValueTask<T> ReadAsync<T>(CancellationToken t = default) where T : struct
	{
		var size    = Mem.SizeOf<T>();
		var buffers = new byte[size];

		var s = await Tcp.Client.ReceiveAsync(buffers, t);

		if (s != size) {
			throw new AdbException($"Received {s} expected {size}");
		}

		var val = MemoryMarshal.Read<T>(buffers);

		/*
		if (BinaryPrimitives.ReverseEndianness()) {

		}
		*/

		return val;

		// var val = BinaryPrimitives.ReverseEndianness();
		// return val;
	}

	public async ValueTask<string> ReadStringAsync(CancellationToken ct = default)
	{
		var s  = await ReadStringAsync(SZ_LEN, ct);
		var l2 = int.Parse(s, NumberStyles.HexNumber);
		return await ReadStringAsync(l2, ct);
	}

	public async ValueTask<byte[]> ReadAsync(int l, CancellationToken ct = default)
	{
		var buf = new byte[l];
		var l2  = await Tcp.Client.ReceiveAsync(buf, ct);

		if (l != l2) {
			throw new AdbException($"Received {l2} expected {l}");
		}

		return buf;
	}

	public async ValueTask<string> ReadStringAsync(int l, CancellationToken ct = default)
	{
		var buf = await ReadAsync(l, ct);

		var s = Encoding.GetString(buf);

		return s;
	}


	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<Device[]> GetDevicesAsync(CancellationToken t = default)
	{
		// NOTE: host:devices closes connection after
		await SendAsync(R1.Cmd_Devices, t);
		await VerifyAsync(t:t);
		var s = await ReadStringAsync(t);
		return ParseDevices(s);
	}

	public async ValueTask<string> TrackDevicesAsync(CancellationToken t = default)
	{
		await SendAsync(R1.Cmd_TrackDevices, t);
		await VerifyAsync(t: t);
		var s = await ReadStringAsync(t);
		return s;
	}

	public async ValueTask<string> GetVersionAsync()
	{
		// NOTE: no verification
		await SendAsync(R1.Cmd_Version);
		return await ReadStringAsync(SZ_LEN);
	}

	[CA($"{nameof(throws)}: false => halt")]
	public async ValueTask<AdbResponse> VerifyAsync([AC(ACT.IS_FALSE)] bool throws = true,
	                                                CancellationToken t = default)
	{
		var res = await ReadStringAsync(SZ_LEN, t);

		string msg = res;
		bool?  b   = null;

		switch (res) {
			case S_OKAY:
				b = true;
				break;

			case S_FAIL:
				msg = await ReadStringAsync(t);

				if (throws) {
					throw new AdbException(msg);
				}

				b = false;
				break;
		}

		return new AdbResponse(b, msg);
	}

	public async ValueTask SetTransport([CBN] string serial = null)
	{
		await SendAsync(serial == null ? R1.Cmd_HostTransportAny : $"host:transport:{serial}");
		await VerifyAsync();
	}

	public async ValueTask<string> ShellAsync(string cmd, IEnumerable<string> args = null, CancellationToken ct = default)
	{
		// TODO
		args ??= [];
		var cmd2 = $"{cmd} {string.Join(' ', args.Select(AdbHelper.Escape))}";

		Trace.WriteLine($">> {cmd2}", nameof(ShellAsync));

		await SendAsync($"{R1.Cmd_Shell}{cmd2}");
		return await Reader.ReadToEndAsync(ct);
	}

	public async ValueTask<AdbDeviceState> GetStateAsync([CBN] string serial = null)
	{
		await SendAsync(serial == null ? R1.Cmd_HostGetState : $"host-serial:{serial}:get-state");
		var s = await ReadStringAsync();
		return AdbHelper.ConvertState(s);
	}

	public static Device[] ParseDevices(string body)
	{
		var lines   = body.Split(Environment.NewLine);
		var devices = new Device[lines.Length];
		int i       = 0;

		foreach (string s in lines) {
			var parts = s.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 1) {
				devices[i++] = new Device(parts[0]);
			}
		}

		return devices;
	}

	public static byte[] GetPayload(string s)
	{
		var bc  = Encoding.GetByteCount(s);
		var str = $"{bc:x4}{s}";
		return Encoding.GetBytes(str);
	}

	public void Dispose()
	{
		Trace.WriteLine("Disposing");
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}

	public const string DIR_SDCARD = "sdcard/";

}