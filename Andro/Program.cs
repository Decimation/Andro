using System;
using Andro.Android;

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

			var n = "192.168.1.234:5555";

			
			foreach (string device in Device.AvailableDevices) {
				Console.WriteLine(device);
			}
			var d = new Device();
			Console.WriteLine(d);

			var f  = @"C:\Users\Deci\Downloads\unnamed.jpg";
			var f2 = "sdcard/unnamed.jpg";

			d.Remove(f2);
			
			
		}
	}
}
