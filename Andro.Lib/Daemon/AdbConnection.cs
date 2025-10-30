#region Global usings

global using ICBN = JetBrains.Annotations.ItemCanBeNullAttribute;
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
using Novus.Runtime;
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

public class AdbConnection : IDisposable
{

	private static readonly ILogger _logger = AdbUtilities.LoggerFactoryInt.CreateLogger("Andro.Lib");

#region Response Codes

	public const string STATUS_OKAY = "OKAY";
	public const string STATUS_FAIL = "FAIL";

#endregion

#region

	public const int SZ_LEN = sizeof(uint);

	public const string DIR_SDCARD = "sdcard/";

	public const string SERVER_HOST = "localhost";
	public const int    SERVER_PORT = 5037;

#endregion

#region

	public StreamWriter Writer { get; }

	public StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; }

	public TcpClient Tcp { get; }

#endregion

	public AdbConnection(string host = SERVER_HOST, int port = SERVER_PORT)
	{
		Tcp = new TcpClient(host, port)
			{ };

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream, AdbUtilities.Encoding, leaveOpen: false);
		Reader = new StreamReader(NetworkStream, AdbUtilities.Encoding, false);

		// Writer.AutoFlush = true;

		/*var sock = new Socket(SocketType.Stream, ProtocolType.Tcp) { };
		await sock.ConnectAsync("localhost", 5037);*/
	}


#region

	public Task SendAsync(string s, CancellationToken ct = default)
	{
		var rg2 = AdbUtilities.GetPayload(s);
		return NetworkStream.WriteAsync(rg2, 0, rg2.Length, ct);
	}

	public async ValueTask<T> ReceiveAsync<T>(CancellationToken ct = default) where T : struct
	{
		var size    = Mem.SizeOf<T>();
		var buffers = new byte[size];

		var len = await NetworkStream.ReadAsync(buffers, 0, buffers.Length, ct);

		// var len = await Tcp.Client.ReceiveAsync(buffers, ct);

		_logger.LogDebug("Reading type {Type} ({Size}) received {Len}", typeof(T).Name, size, len);

		if (len != size) {
			throw new AdbException($"Received {len} expected {size}");
		}

		var val = MemoryMarshal.Read<T>(buffers);

		return val;
	}

	public ValueTask Connect(CancellationToken ct = default)
	{
		return Tcp.Client.ConnectAsync(SERVER_HOST, SERVER_PORT, ct);

	}

	public async ValueTask<byte[]> ReceiveAsync(int l, CancellationToken ct = default)
	{
		var buf = new byte[l];

		var l2 = await NetworkStream.ReadAsync(buf, 0, buf.Length, ct);

		// var l2 = await Tcp.Client.ReceiveAsync(buf, ct);

		if (l != l2) {
			throw new AdbException($"Received {l2} expected {l}");
		}

		_logger.LogDebug("Read {Len} received {Len2}", l, l2);
		return buf;
	}

	public async ValueTask<string> ReadEncodedStringAsync(CancellationToken ct = default)
	{
		var s  = await ReadStringAsync(SZ_LEN, ct);
		var l2 = Int32.Parse(s, NumberStyles.HexNumber);
		return await ReadStringAsync(l2, ct);
	}

	public async ValueTask<string> ReadStringAsync(int l, CancellationToken ct = default)
	{
		var buf = await ReceiveAsync(l, ct);
		var s   = AdbUtilities.Encoding.GetString(buf);
		return s;
	}

#endregion


#region

	[ICBN]
	[CA($"canbenull <= {nameof(throws)}:false")]
	public async Task<string> VerifyResponseStatusAsync(bool throws = true, CancellationToken ct = default)
	{
		string resMsg = await ReadResponseStatusAsync(ct);

		if (throws && resMsg != null) {
			throw new AdbException(resMsg);
		}

		return resMsg != null ? (throws ? throw new AdbException(resMsg) : resMsg) : null;
	}


	/// <returns>
	/// <see cref="STATUS_OKAY"/>: <c>null</c> <br />
	/// <see cref="STATUS_FAIL"/>: Error message
	/// </returns>
	[ICBN]
	private async ValueTask<string> ReadResponseStatusAsync(CancellationToken ct = default)
	{
		var res = await ReadStringAsync(SZ_LEN, ct);

		return res switch
		{
			STATUS_OKAY => null,
			STATUS_FAIL => await ReadEncodedStringAsync(ct: ct),
			_           => null
		};

	}

#endregion


	public async ValueTask<string> ShellAsync(string cmd, IEnumerable<string> args = null, CancellationToken ct = default)
	{
		// TODO
		args ??= [];

		var subSet = String.Join(' ', args.Select(AdbUtilities.Escape));
		var cmd2   = String.Join(' ', cmd, subSet);

		_logger.LogTrace("Sending shell command {Cmd}", cmd2);

		await SendAsync($"{R1.Cmd_Shell}{cmd2}", ct);
		return await Reader.ReadToEndAsync(ct);
	}

#region

	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<AdbDevice[]> GetDevicesAsync(CancellationToken ct = default)
	{

		// NOTE: host:devices closes connection after
		await SendAsync(R1.Cmd_Devices, ct);
		await VerifyResponseStatusAsync(ct: ct);

		var body = await ReadEncodedStringAsync(ct: ct);

		var devices    = AdbDevice.ParseDevices(body);
		var adbDevices = new AdbDevice[devices.Length];

		for (int i = 0; i < devices.Length; i++) {
			adbDevices[i] = new AdbDevice(devices[i]);
		}

		// await Tcp.Client.DisconnectAsync(true, ct);
		// await Tcp.Client.ConnectAsync(SERVER_HOST, SERVER_PORT, ct);

		return adbDevices;
	}


	public async ValueTask<string> TrackDevicesAsync(CancellationToken ct = default)
	{
		await SendAsync(R1.Cmd_TrackDevices, ct);
		await VerifyResponseStatusAsync(ct: ct);
		return await ReadEncodedStringAsync(ct: ct);
	}

	public async ValueTask SetHostTransportAsync([CBN] AdbDevice device = null)
	{
		await SendAsync(device == null ? R1.Cmd_HostTransportAny : $"host:transport:{device}");
		await VerifyResponseStatusAsync();
	}

	public async ValueTask<AdbDeviceState> GetHostStateAsync([CBN] AdbDevice device = null)
	{
		await SendAsync(device == null ? R1.Cmd_HostGetState : $"host-serial:{device}:get-state");
		var s = await ReadEncodedStringAsync();
		return AdbUtilities.ParseState(s);
	}

#endregion

	public async ValueTask<int> GetVersionAsync(CancellationToken ct = default)
	{
		// NOTE: no verification
		await SendAsync(R1.Cmd_Version, ct);
		await VerifyResponseStatusAsync(ct: ct);
		var v = await ReadEncodedStringAsync(ct: ct);
		return Int32.Parse(v, NumberStyles.HexNumber);
	}

	public void Dispose()
	{
		_logger.LogDebug("Disposing {Host}", Tcp);
		Tcp.Dispose();
	}

}