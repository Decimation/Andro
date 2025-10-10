namespace Andro.Lib.Daemon;

public class AdbConnection
{

	public string Host { get; }

	public int Port { get; }

	public AdbConnection(string host = HOST_DEFAULT, int port = PORT_DEFAULT)
	{
		Host = host;
		Port = port;
	}

	public const string HOST_DEFAULT = "localhost";

	public const int PORT_DEFAULT = 5037;

}