// ReSharper disable UnusedMember.Global

using JetBrains.Annotations;

namespace Andro.Adb.Diagnostics;

public sealed class AdbException : Exception
{

	public AdbException() { }

	public AdbException(string? message, Exception? innerException) : base(message, innerException) { }

	public AdbException(string? message = null) : base(message) { }

	[AssertionMethod]
	public static void AssertSize(int act, int expect, string? msg = null)
	{
		if (act != expect) {
			throw new AdbException($"Expected size: {expect}, actual: {act}");
		}
	}

}