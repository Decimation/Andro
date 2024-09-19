using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andro.Adb.Android;

public class SyncTransport : IDisposable
{

	public StreamReader Reader { get; }

	public StreamWriter Writer { get; }

	public SyncTransport(StreamReader reader, StreamWriter writer)
	{
		Reader = reader;
		Writer = writer;
	}

	public Task SendAsync(string cmd, string name)
	{
		if (cmd.Length != Transport.SZ_LEN) {
			throw new ArgumentException();
		}

		return Writer.WriteAsync(cmd);
	}

	public async ValueTask<string> ReadStringAsync(int l)
	{
		var buf = new byte[l];
		var res = await Reader.BaseStream.ReadAsync(buf);

		return AdbHelper.Encoding.GetString(buf, 0, res);
	}

	public void Dispose()
	{
		Reader.Dispose();
		Writer.Dispose();
	}

}