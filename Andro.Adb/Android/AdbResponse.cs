#nullable disable
using Andro;

namespace Andro.Adb.Android;

public readonly record struct AdbResponse
{

	[CBN]
	public string Message { get; }

	public bool? Ok { get; }

	public AdbResponse() { }
}