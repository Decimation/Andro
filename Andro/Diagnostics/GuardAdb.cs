﻿using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Andro.Diagnostics
{
	internal static class GuardAdb
	{
		[DebuggerHidden]
		[AssertionMethod]
		internal static void AssertSingleDevice(string[] devices)
		{
			if (devices.Length > 1) {
				throw new AdbException("More than 1 device connected");
			}
		}

		[DebuggerHidden]
		[AssertionMethod]
		internal static void AssertDeviceAvailable(string[] devices, string deviceName)
		{
			if (!devices.Contains(deviceName)) {
				throw new AdbException("Device is not connected");
			}
		}
	}
}