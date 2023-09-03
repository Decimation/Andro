
// ReSharper disable UnusedMember.Global
#nullable enable
using Andro;

namespace Andro.Adb.Utilities;

public sealed class AdbException : Exception
{
	public AdbException() { }
	public AdbException(string? message, Exception? innerException) : base(message, innerException) { }
	public AdbException(string? message) : base(message) { }
}