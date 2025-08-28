namespace Andro.Adb.Android;

public class AdbConnection : ITransportFactory
{

	public string Host { get; }

	public int Port { get; }

	public AdbConnection(string host, int port)
	{
		Host = host;
		Port = port;
	}

	public AdbConnection() : this(Transport.DEFAULT_HOST, Transport.DEFAULT_PORT) { }

	public ValueTask<Transport> GetTransport()
	{
		return ValueTask.FromResult(new Transport(Host, Port));
	}

}