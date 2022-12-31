#nullable disable
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Andro.Lib.Properties;
using Andro.Lib.Utilities;
using JetBrains.Annotations;

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

namespace Andro.Lib.Android;

public class Transport : IDisposable
{
	#region

	public const int SZ_LEN = sizeof(uint);

	#endregion

	#region

	public StreamWriter Writer { get; }

	public StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; }

	public TcpClient Tcp { get; }

	public bool IsAlive => Tcp.Connected;

	#endregion

	public Transport(string host = AdbConnection.DEFAULT_HOST, int port = AdbConnection.DEFAULT_PORT)
	{
		Tcp = new TcpClient(host, port);

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream);
		Reader = new StreamReader(NetworkStream);

		Writer.AutoFlush = true;

		/*var sock = new Socket(SocketType.Stream, ProtocolType.Tcp) { };
		await sock.ConnectAsync("localhost", 5037);*/
	}

	public async Task SendAsync(string s, CancellationToken? t = null)
	{
		t ??= CancellationToken.None;
		string s2 = AdbHelper.GetPayload(s, out byte[] rg, out var rg2);

		// await NetworkStream.WriteAsync(rg2, t.Value);
		// await NetworkStream.FlushAsync(t.Value);

		await Tcp.Client.SendAsync(rg2, t.Value);

		return;
	}

	public async Task<SyncTransport> startSync()
	{
		await SendAsync("sync:");
		await VerifyAsync();
		return new SyncTransport(Reader, Writer);
	}

	public async Task<int> ReadInt()
	{
		var buffers = new byte[sizeof(int)];
		var s       = await Tcp.Client.ReceiveAsync(buffers);
		var val     = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(buffers));
		return val;
	}

	public async Task<string> ReadStringAsync()
	{
		var l  = await ReadStringAsync(SZ_LEN);
		var l2 = Int32.Parse(l, NumberStyles.HexNumber);
		return await ReadStringAsync(l2);

	}

	public async Task<string> ReadStringAsync(int l)
	{
		var buf = new byte[l];
		// var l2  = await NetworkStream.ReadAsync(buf);

		var l2 = await Tcp.Client.ReceiveAsync(buf);

		var s = AdbHelper.Encoding.GetString(buf);

		return s;
	}

	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<string> GetDevicesAsync()
	{
		// NOTE: host:devices closes connection after
		await SendAsync(R.Cmd_Devices);
		await VerifyAsync();
		var s = await ReadStringAsync();
		return s;
	}

	public async ValueTask<string> TrackDevicesAsync()
	{
		await SendAsync(R.Cmd_TrackDevices);
		await VerifyAsync();
		var s = await ReadStringAsync();
		return s;
	}

	public async ValueTask<string> GetVersionAsync()
	{
		// NOTE: no verification
		await SendAsync(R.Cmd_Version);
		return await ReadStringAsync(SZ_LEN);
	}

	public async ValueTask<AdbResponse> VerifyAsync(Predicate<string> f = null, bool throws = true)
	{
		var res = await ReadStringAsync(SZ_LEN);

		string msg = res;
		bool?  b   = null;

		switch (res) {
			case "OKAY":
				b = true;
				break;
			case "FAIL":
				msg = await ReadStringAsync();

				if (throws) {
					throw new AdbException(msg);
				}

				b = false;
				break;
			default:
				/*msg = await ReadStringAsync();

				if (throws) {
					throw new AdbException(msg);
				}*/
				b = f?.Invoke(res);
				break;
		}

		return new AdbResponse()
		{
			Message = msg,
			Ok      = b
		};
	}

	public void Dispose()
	{
		Trace.WriteLine($"Disposing");
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}
}