#nullable disable
using Andro;

namespace Andro.Adb.Android;

public record AdbResponse
{
	public string Message { get; internal init; }
	public bool? Ok { get; internal init; }
}