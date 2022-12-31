namespace Andro.Lib.Android;

public interface ITransportFactory
{
	public Task<Transport> GetTransport();
}