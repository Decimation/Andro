using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Andro.Utilities;
using JetBrains.Annotations;
using Kantan.Numeric;
using Kantan.Text;
#nullable disable
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Andro.Properties;
using Kantan.Collections;
using Novus.Win32;
using Kantan.Diagnostics;
using Kantan.Utilities;
using Novus.OS;
using Novus.Utilities;

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

namespace Andro.Android;

public enum AdbConnectionMode
{
	USB,
	TCPIP,
	UNKNOWN
}

public class AdbDevice : IDisposable
{
	public const int SZ_LEN = sizeof(uint);

	public StreamWriter Writer { get; }

	public StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; private set; }

	public TcpClient Tcp { get; private set; }

	public bool IsAlive => Tcp.Connected;

	public AdbDevice(string host = "localhost", int port = 5037)
	{
		Tcp = new TcpClient(host, port);

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream);
		Reader = new StreamReader(NetworkStream);

		Writer.AutoFlush = true;
	}

	public async Task SendAsync(string s, CancellationToken? t = null)
	{
		t ??= CancellationToken.None;
		string s2 = GetPayload(s, out byte[] rg, out var rg2);
		await NetworkStream.WriteAsync(rg2, t.Value);
		await NetworkStream.FlushAsync(t.Value);

		return;
	}

	private static string GetPayload(string s, out byte[] rg, out byte[] rg2)
	{
		rg = Encoding.UTF8.GetBytes(s);
		var cm = $"{rg.Length:x4}{s}";
		rg2 = Encoding.UTF8.GetBytes(cm);
		return cm;
	}

	public async Task<string> ReadStringAsync()
	{
		var l  = await ReadStringAsync(SZ_LEN);
		var l2 = int.Parse(l, NumberStyles.HexNumber);
		return await ReadStringAsync(l2);

	}

	public async Task<string> ReadStringAsync(int l)
	{
		var buf = new byte[l];
		var l2  = await NetworkStream.ReadAsync(buf);

		var s = Encoding.UTF8.GetString(buf);

		return s;
	}

	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<string> GetDevicesAsync()
	{
		// NOTE: host:devices closes connection after
		await SendAsync(Resources.Cmd_Devices);
		await Verify();
		var s = await ReadStringAsync();
		return s;
	}

	public async ValueTask<string> GetVersionAsync()
	{
		// NOTE: no verification
		await SendAsync(Resources.Cmd_Version);
		return await ReadStringAsync(SZ_LEN);
	}

	public async Task<string> Verify(Action<string> f = null)
	{
		var res = await ReadStringAsync(SZ_LEN);

		string msg = res;

		switch (res) {
			case "OKAY":
				break;
			default:
				f?.Invoke(res);
				/*msg = await ReadStringAsync();

				if (throws) {
					throw new AdbException(msg);
				}*/

				break;
		}

		return msg;
	}

	public async Task<string> ShellAsync(string cmd, IEnumerable<string> args = null)
	{
		args ??= Enumerable.Empty<string>();
		var cmd2 = $"{cmd} {String.Join(' ', args.Select(Escape))}";
		Trace.WriteLine($">> {cmd2}", nameof(ShellAsync));

		await SendAsync($"shell:{cmd2}");
		await Verify();
		
		// var l = await Reader.ReadLineAsync();
		// return l;
		var output = await Reader.ReadToEndAsync();
		return output;
	}

	public static string Escape(string e)
	{
		return e.Replace(" ", "' '");
	}

	public void Dispose()
	{
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}
}