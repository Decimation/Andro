using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Andro.Utilities;
using JetBrains.Annotations;
using Kantan.Cli;
using Kantan.Numeric;
using Kantan.Text;
#nullable disable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Kantan.Collections;
using Novus.OS.Win32;
using Kantan.Diagnostics;
using Kantan.Utilities;
using Novus.OS;
using Novus.Utilities;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0079

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

#pragma warning disable HAA0101

namespace Andro.Android;

public enum AdbConnectionMode
{
	USB,
	TCPIP,
	UNKNOWN
}

public class AdbDevice : IDisposable
{
	private const int          SZ_LEN = sizeof(uint);
	private       StreamWriter Writer { get; }

	private StreamReader Reader { get; }

	public NetworkStream NetworkStream { get; private set; }

	public TcpClient Tcp { get; private set; }

	public AdbDevice()
	{
		Tcp = new TcpClient("localhost", 5037);

		NetworkStream = Tcp.GetStream();

		Writer = new StreamWriter(NetworkStream);
		Reader = new StreamReader(NetworkStream);

		Writer.AutoFlush = true;
	}

	public async Task Send(string s, CancellationToken? t = default)
	{
		t ??= CancellationToken.None;

		var rg  = Encoding.UTF8.GetBytes(s);
		var cm  = $"{rg.Length:x4}{s}";
		var cm2 = Encoding.UTF8.GetBytes(cm);

		await NetworkStream.WriteAsync(cm2, t.Value);
		await NetworkStream.FlushAsync(t.Value);

		return;
	}

	public async Task<string> ReadStringAsync()
	{
		var l  = await ReadStringAsync(SZ_LEN);
		var l2 = int.Parse(l, NumberStyles.HexNumber);
		return await ReadStringAsync(l2);

	}

	public async Task<string> Verify(bool throws = true)
	{
		var res = await ReadStringAsync(SZ_LEN);

		string msg = res;

		switch (res) {
			case "OKAY":
				break;
			default:
				msg = await ReadStringAsync();

				if (throws) {
					throw new AdbException(msg);
				}

				break;
		}

		return msg;
	}

	public async Task<string> ReadStringAsync(int l)
	{
		var buf = new byte[l];
		var l2 = await NetworkStream.ReadAsync(buf);
		// await NetworkStream.ReadFullyAsync(buf);

		var s = Encoding.UTF8.GetString(buf, 0, l);

		return s;
	}

	#region IDisposable

	public void Dispose()
	{
		Tcp.Dispose();
		Reader.Dispose();
		Writer.Dispose();
		NetworkStream.Dispose();
	}

	#endregion
}