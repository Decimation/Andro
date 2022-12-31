namespace Andro.Lib.Android;

public interface ITransportFactory
{
	public Task<AdbTransport> GetTransport();
}