namespace Andro.Adb.Android;

public interface ITransportFactory
{
	public ValueTask<Transport> GetTransport();
}