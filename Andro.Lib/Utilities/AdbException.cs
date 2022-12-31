
// ReSharper disable UnusedMember.Global
#nullable enable
namespace Andro.Lib.Utilities;

public sealed class AdbException : Exception
{
	public AdbException() { }
	public AdbException(string? message, Exception? innerException) : base(message, innerException) { }
	public AdbException(string? message) : base(message) { }
}