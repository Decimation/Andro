﻿using System.Collections.Concurrent;
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

public class AdbDevice : IDisposable
{
	#region

	public const int SZ_LEN = sizeof(uint);

	public const string HOST_DEFAULT = "localhost";

	public const int PORT_DEFAULT = 5037;

	#endregion

	public StreamWriter Writer { get; }

	public StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; }

	public TcpClient Tcp { get; }

	public bool IsAlive => Tcp.Connected;

	public AdbDevice(string host = HOST_DEFAULT, int port = PORT_DEFAULT)
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

		var s = Encoding.UTF8.GetString(buf);

		return s;
	}

	/// <remarks>Connection terminates after command</remarks>
	public async ValueTask<string> GetDevicesAsync()
	{
		// NOTE: host:devices closes connection after
		await SendAsync(Resources.Cmd_Devices);
		await VerifyAsync();
		var s = await ReadStringAsync();
		return s;
	}

	public async ValueTask<string> TrackDevicesAsync()
	{
		await SendAsync(Resources.Cmd_TrackDevices);
		await VerifyAsync();
		var s = await ReadStringAsync();
		return s;
	}

	public async ValueTask<string> GetVersionAsync()
	{
		// NOTE: no verification
		await SendAsync(Resources.Cmd_Version);
		return await ReadStringAsync(SZ_LEN);
	}

	public async ValueTask<bool?> VerifyAsync(Predicate<string> f = null)
	{
		var res = await ReadStringAsync(SZ_LEN);

		string msg = res;

		switch (res) {
			case "OKAY":
				return true;
			default:
				/*msg = await ReadStringAsync();

				if (throws) {
					throw new AdbException(msg);
				}*/
				return f?.Invoke(res);

				break;
		}

		return null;
	}

	public async Task ConnectTransport()
	{
		await SendAsync(Resources.Cmd_HostTransport_Any);
		await VerifyAsync();
	}

	public async Task<string> ShellAsync(string cmd, [CanBeNull] IEnumerable<string> args = null)
	{
		args ??= Enumerable.Empty<string>();
		var cmd2 = $"{cmd} {String.Join(' ', args.Select(AdbHelper.Escape))}";
		Trace.WriteLine($">> {cmd2}", nameof(ShellAsync));

		await SendAsync($"{R.Cmd_Shell}{cmd2}");
		await VerifyAsync();

		// var l = await Reader.ReadLineAsync();
		// return l;

		var output = await Reader.ReadToEndAsync();
		return output;
	}

	public void Dispose()
	{
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}
}