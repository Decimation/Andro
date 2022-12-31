#nullable disable
namespace Andro.Lib.Android;

public record AdbResponse
{
	public string Message { get; internal init; }
	public bool?  Ok      { get; internal init; }
}