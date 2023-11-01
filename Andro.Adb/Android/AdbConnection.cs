using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andro.Adb.Android;

public class AdbConnection : ITransportFactory
{

	public string Host { get; }

	public int Port { get; }

	private AdbConnection(string host, int port)
	{
		Host = host;
		Port = port;
	}

	public AdbConnection() : this(DEFAULT_HOST, DEFAULT_PORT) { }

	public ValueTask<Transport> GetTransport()
	{
		return ValueTask.FromResult(new Transport(Host, Port));
	}

	public const string DEFAULT_HOST = "localhost";

	public const int DEFAULT_PORT = 5037;

	public async Task<AdbDevice[]> GetDevicesAsync()
	{
		var t = await GetTransport();
		await t.SendAsync("host:devices");
		await t.VerifyAsync();
		var b = await t.ReadStringAsync();
		return ParseDevices(b);
	}

	internal AdbDevice[] ParseDevices(string body)
	{
		var lines   = body.Split(Environment.NewLine);
		var devices = new AdbDevice[lines.Length];
		int i       = 0;

		foreach (string s in lines) {
			var parts = s.Split('\t');

			if (parts.Length > 1) {
				devices[i++] = new AdbDevice(parts[0], this);
			}
		}

		return devices;
	}

}