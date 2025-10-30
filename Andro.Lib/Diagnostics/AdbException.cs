// ReSharper disable UnusedMember.Global

namespace Andro.Lib.Diagnostics;

public sealed class AdbException : Exception
{

	public AdbException() { }

	public AdbException(string? message, Exception? innerException) : base(message, innerException) { }

	public AdbException(string? message = null) : base(message) { }
}