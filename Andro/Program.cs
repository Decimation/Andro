using System;

namespace Andro
{
	/*
	 *
	 */
	public static class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			var d = new Device("192.168.1.234:5555");

			foreach (string device in Device.AvailableDevices) {
				Console.WriteLine(device);
			}

			Console.WriteLine(Device.AvailableDevices.Length);


			d.Push(@"C:\Users\Deci\Downloads\unnamed.jpg", "sdcard/");
		}
	}
}
