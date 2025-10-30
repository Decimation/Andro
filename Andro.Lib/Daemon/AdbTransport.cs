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
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Andro.Lib.Diagnostics;
using Novus.Memory;
using JetBrains.Annotations;

// ReSharper disable AsyncApostle.ConfigureAwaitHighlighting

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0079

[assembly: InternalsVisibleTo("Andro")]
[assembly: InternalsVisibleTo("UnitTest")]
[assembly: InternalsVisibleTo("Test")]


namespace Andro.Lib.Daemon;

public class AdbTransport : IDisposable
{

	private static readonly ILogger _logger = AdbHelper.LoggerFactoryInt.CreateLogger("Andro.Lib");

#region Response Codes

	public const string S_OKAY = "OKAY";
	public const string S_FAIL = "FAIL";

#endregion

#region

	public const int SZ_LEN = sizeof(uint);

	public const string DIR_SDCARD   = "sdcard/";
	public const string HOST_DEFAULT = "localhost";
	public const int    PORT_DEFAULT = 5037;

#endregion

#region

	public StreamWriter Writer { get; }

	public StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; }

	public TcpClient Tcp { get; }

#endregion

	public AdbTransport(string host = HOST_DEFAULT, int port = PORT_DEFAULT)
	{
		Tcp = new TcpClient(host, port);

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream, AdbHelper.Encoding);
		Reader = new StreamReader(NetworkStream, AdbHelper.Encoding, false);

		// Writer.AutoFlush = true;

		/*var sock = new Socket(SocketType.Stream, ProtocolType.Tcp) { };
		await sock.ConnectAsync("localhost", 5037);*/
	}


	public ValueTask<int> SendAsync(string s, CancellationToken t = default)
	{
		var rg2 = AdbHelper.GetPayload(s);
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

		var len = await Tcp.Client.ReceiveAsync(buffers, t);

		_logger.LogDebug("Reading type {Type} ({Size}) received {Len}", typeof(T).Name, size, len);

		if (len != size) {
			throw new AdbException($"Received {len} expected {size}");
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

	public async ValueTask<byte[]> ReadAsync(int l, CancellationToken ct = default)
	{
		var buf = new byte[l];
		var l2  = await Tcp.Client.ReceiveAsync(buf, ct);

		if (l != l2) {
			throw new AdbException($"Received {l2} expected {l}");
		}

		_logger.LogDebug("Read {Len} received {Len2}", l, l2);
		return buf;
	}

	public async Task<string> ReadEncodedStringAsync(int len = SZ_LEN, CancellationToken ct = default)
	{
		var s  = await ReadStringAsync(len, ct);
		var l2 = int.Parse(s, NumberStyles.HexNumber);
		return await ReadStringAsync(l2, ct);
	}

	public async Task<string> ReadStringAsync(int l, CancellationToken ct = default)
	{
		var buf = await ReadAsync(l, ct);
		var s   = AdbHelper.Encoding.GetString(buf);

		return s;
	}


	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<AdbDevice[]> GetDevicesAsync(CancellationToken t = default)
	{
		// NOTE: host:devices closes connection after
		await SendAsync(R1.Cmd_Devices, t);
		await VerifyAsync(t: t);

		var s = await ReadEncodedStringAsync(ct: t);

		var devices = AdbHelper.ParseDevices(s);
		var rg      = new AdbDevice[devices.Length];

		for (int i = 0; i < devices.Length; i++) {
			rg[i] = new AdbDevice(this, devices[i]);
		}

		return rg;
	}


	/*
	public async Task<T> SendCommandAsync<T>(string cmd, Func<int, CancellationToken, Task<T>> getVal, CancellationToken state,
	                                         bool verify = true)
	{
		var len = await SendAsync(cmd, state);

		if (verify) {
			await VerifyAsync(t: state);
		}

		return await getVal(len, state);
	}
	*/


	public async Task<string> TrackDevicesAsync(CancellationToken t = default)
	{
		await SendAsync(R1.Cmd_TrackDevices, t);
		await VerifyAsync(t: t);
		return await ReadEncodedStringAsync(ct: t);
	}

	public async Task<string> GetVersionAsync(CancellationToken t = default)
	{
		// NOTE: no verification
		await SendAsync(R1.Cmd_Version, t);
		return await ReadEncodedStringAsync(ct: t);
	}

	[CA($"{nameof(throws)}: false => halt")]
	public async Task<AdbResponse> VerifyAsync([AC(ACT.IS_FALSE)] bool throws = true, CancellationToken t = default)
	{
		var res = await ReadStringAsync(SZ_LEN, t);

		string msg = res;
		bool?  b   = null;

		switch (res) {
			case S_OKAY:
				b = true;
				break;

			case S_FAIL:
				msg = await ReadEncodedStringAsync(ct: t);

				if (throws) {
					throw new AdbException(msg);
				}

				b = false;
				break;
		}

		return new AdbResponse(b, msg);
	}

	public async ValueTask<string> ShellAsync(string cmd, IEnumerable<string> args = null, CancellationToken ct = default)
	{
		// TODO
		args ??= [];

		var subSet = String.Join(' ', args.Select(AdbHelper.Escape));
		var cmd2   = String.Join(' ', cmd, subSet);

		_logger.LogTrace("Sending shell command {Cmd}", cmd2);

		await SendAsync($"{R1.Cmd_Shell}{cmd2}", ct);
		return await Reader.ReadToEndAsync(ct);
	}

	public async ValueTask SetTransport([CBN] string serial)
	{
		await SendAsync(String.IsNullOrWhiteSpace(serial) ? R1.Cmd_HostTransportAny : $"host:transport:{serial}");
		await VerifyAsync();
	}

	public async ValueTask<AdbDeviceState> GetStateAsync([CBN] string serial)
	{
		await SendAsync(String.IsNullOrWhiteSpace(serial) ? R1.Cmd_HostGetState : $"host-serial:{serial}:get-state");
		var s = await ReadEncodedStringAsync();
		return AdbHelper.ParseState(s);
	}

	public void Dispose()
	{
		_logger.LogDebug("Disposing {Host}", Tcp);
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}

}