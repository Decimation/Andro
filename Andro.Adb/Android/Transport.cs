#nullable disable
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using Andro.Adb.Diagnostics;
using Novus.Memory;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0079

/*
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0602 // Delegate on struct instance caused a boxing allocation
#pragma warning disable HAA0603 // Delegate allocation from a method group
#pragma warning disable HAA0604 // Delegate allocation from a method group

#pragma warning disable HAA0501 // Explicit new array type allocation
#pragma warning disable HAA0502 // Explicit new reference type allocation
#pragma warning disable HAA0503 // Explicit new reference type allocation
#pragma warning disable HAA0504 // Implicit new array creation allocation
#pragma warning disable HAA0505 // Initializer reference type allocation
#pragma warning disable HAA0506 // Let clause induced allocation

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0302 // Display class allocation to capture closure
#pragma warning disable HAA0303 // Lambda or anonymous method in a generic method allocates a delegate instance

#pragma warning disable HAA0101*/

namespace Andro.Adb.Android;

public class Transport : IDisposable
{

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

	public bool IsAlive => Tcp.Connected;

#endregion

	public Transport(string host = DEFAULT_HOST, int port = DEFAULT_PORT)
	{
		Tcp = new TcpClient(host, port);

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream);
		Reader = new StreamReader(NetworkStream);

		// Writer.AutoFlush = true;

		/*var sock = new Socket(SocketType.Stream, ProtocolType.Tcp) { };
		await sock.ConnectAsync("localhost", 5037);*/
	}

	public async Task SendAsync(string s, CancellationToken t = default)
	{
		string s2 = AdbHelper.GetPayload(s, out byte[] rg, out var rg2);

		// await NetworkStream.WriteAsync(rg2, t.Value);
		// await NetworkStream.FlushAsync(t.Value);

		await Tcp.Client.SendAsync(rg2, t);

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

		var s = AdbHelper.Encoding.GetString(buf);

		return s;
	}

	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<string> GetDevicesAsync(CancellationToken t = default)
	{
		// NOTE: host:devices closes connection after
		await SendAsync(R.Cmd_Devices, t);
		await VerifyAsync(t: t);
		var s = await ReadStringAsync(t);
		return s;
	}

	public async ValueTask<string> TrackDevicesAsync(CancellationToken t = default)
	{
		await SendAsync(R.Cmd_TrackDevices, t);
		await VerifyAsync(t: t);
		var s = await ReadStringAsync(t);
		return s;
	}

	public async ValueTask<string> GetVersionAsync()
	{
		// NOTE: no verification
		await SendAsync(R.Cmd_Version);
		return await ReadStringAsync(SZ_LEN);
	}

	public async ValueTask<AdbResponse> VerifyAsync(bool throws = true, CancellationToken t = default)
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

	public void Dispose()
	{
		Trace.WriteLine("Disposing");
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}

	public const string DEFAULT_HOST = "localhost";

	public const int DEFAULT_PORT = 5037;

	public static Device[] ParseDevices(string body)
	{
		var lines   = body.Split(Environment.NewLine);
		var devices = new Device[lines.Length];
		int i       = 0;

		foreach (string s in lines) {
			var parts = s.Split('\t');

			if (parts.Length > 1) {
				devices[i++] = new Device(parts[0], this);
			}
		}

		return devices;
	}

	public async Task<Device[]> GetDevicesAsync()
	{
		await SendAsync("host:devices");
		await VerifyAsync();
		var b = await ReadStringAsync();
		return ParseDevices(b);
	}

}