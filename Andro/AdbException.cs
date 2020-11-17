using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
// ReSharper disable UnusedMember.Global
#nullable enable
namespace Andro
{
	public sealed class AdbException : Exception
	{
		public AdbException() { }

		public AdbException(string? message) : base(message) { }
	}
}
