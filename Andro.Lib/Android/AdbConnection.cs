using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andro.Lib.Android;

public class AdbConnection : ITransportFactory
{
	public string Host { get; }
	public int    Port { get; }

	private AdbConnection(string host, int port)
	{
		Host = host;
		Port = port;
	}

	public AdbConnection() : this(DEFAULT_HOST, DEFAULT_PORT) { }

	public async Task<AdbTransport> GetTransport()
	{
		return new AdbTransport(Host, Port);
	}

	public const string DEFAULT_HOST = "localhost";

	public const int DEFAULT_PORT = 5037;

	public async Task<AdbDevice[]> get()
	{
		using var t = await GetTransport();
		await t.SendAsync("host:devices");
		await t.VerifyAsync();
		var b = await t.ReadStringAsync();
		return parse(b);
	}

	internal AdbDevice[] parse(string b)
	{
		var l       = b.Split(Environment.NewLine);
		var devices = new AdbDevice[l.Length];
		int i       = 0;

		foreach (string s in l) {
			var p = s.Split('\t');

			if (p.Length > 1) {
				devices[i++] = new AdbDevice(p[0], this);
			}
		}

		return devices;
	}
}