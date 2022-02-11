using System;

// ReSharper disable UnusedMember.Global
#nullable enable
namespace Andro.Diagnostics;

public sealed class AdbException : Exception
{
	public AdbException() { }

	public AdbException(string? message) : base(message) { }
}