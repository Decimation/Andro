using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andro.Lib.Android;

public class SyncTransport
{
	private StreamReader m_reader;
	private StreamWriter m_writer;

	public SyncTransport(StreamReader reader, StreamWriter writer)
	{
		m_reader = reader;
		m_writer = writer;
	}

	public async Task Send(String cmd, string name)
	{
		if (cmd.Length != Transport.SZ_LEN) {
			throw new ArgumentException();
		}

		await m_writer.WriteAsync(cmd);
	}

	public async ValueTask<string> ReadString(int l)
	{
		var buf = new byte[l];
		await m_reader.BaseStream.ReadAsync(buf);
		return AdbHelper.Encoding.GetString(buf);
	}
}