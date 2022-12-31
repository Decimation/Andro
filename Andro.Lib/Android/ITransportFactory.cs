namespace Andro.Lib.Android;

public interface ITransportFactory
{
	public ValueTask<Transport> GetTransport();
}