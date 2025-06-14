﻿
// ReSharper disable UnusedMember.Global

namespace Andro.Adb.Diagnostics;

public sealed class AdbException : Exception
{
	public AdbException() { }
	public AdbException(string? message, Exception? innerException) : base(message, innerException) { }
	public AdbException(string? message) : base(message) { }
}