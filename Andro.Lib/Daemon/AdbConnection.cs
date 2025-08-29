namespace Andro.Lib.Daemon;

public class AdbConnection
{

	public string Host { get; }

	public int Port { get; }

	public AdbConnection(string host, int port)
	{
		Host = host;
		Port = port;
	}

	public AdbConnection() : this(Transport.HOST_DEFAULT, Transport.PORT_DEFAULT) { }
}