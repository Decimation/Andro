using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Novus.Win32;

// ReSharper disable UnusedMember.Global

namespace Andro
{
	public enum ConnectionMode
	{
		USB,
		TCPIP,
		UNKNOWN
	}

	public class Device
	{
		public string DeviceName { get; }

		public ConnectionMode Mode { get; }

		public const string TCPIP = "5555";

		public const string ADB = "adb";


		public Device(string deviceName, ConnectionMode mode = ConnectionMode.UNKNOWN)
		{
			DeviceName = deviceName;

			Mode = mode == ConnectionMode.UNKNOWN ? Guess(deviceName) : mode;

			GuardAdb.AssertDeviceAvailable(AvailableDevices, deviceName);
		}

		private static ConnectionMode Guess(string deviceName)
		{
			bool tcpIp = deviceName.Contains(TCPIP);

			return tcpIp ? ConnectionMode.TCPIP : ConnectionMode.USB;
		}


		public static string[] AvailableDevices
		{
			get
			{
				using var proc = CommandOperation.Run("devices");


				// Skip first line
				var stdOut = proc.StandardOutput
					.Skip(1)
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Select(s => s.Split('\t')[0])
					.ToArray();

				return stdOut;
			}
		}

		public static Device Connect(ConnectionMode mode)
		{
			switch (mode) {

				case ConnectionMode.USB:
					var c = CommandOperation.Run($"usb");
					break;
				case ConnectionMode.TCPIP:
					var c2 = CommandOperation.Run($"tcpip {TCPIP}");
					break;
				case ConnectionMode.UNKNOWN:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}

			var devices = AvailableDevices;

			GuardAdb.AssertSingleDevice(devices);

			var deviceName = devices.First();

			var device = new Device(deviceName, mode);

			return device;
		}

		public void Push(string srcFile, string destFolder)
		{
			using var cmd = CommandOperation.Run($"push \"{srcFile}\" \"{destFolder}\"");

			Global.Write(cmd);
		}

		public void PushAll(string srcFolder, string destFolder)
		{
			var files = Directory.GetFiles(srcFolder);


			foreach (string file in files) {
				Push(file, destFolder);
				//Console.ReadLine();
			}

		}
	}
}