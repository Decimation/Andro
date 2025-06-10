#nullable disable
namespace Andro.Adb.Android;

public readonly record struct AdbResponse
{

	[CBN]
	public string Message { get; }

	public bool? Ok { get; }

	public AdbResponse(bool? ok, [CBN] string message)
	{
		Ok      = ok;
		Message = message;
	}

	public AdbResponse() { }
}