using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Andro.Android;
using Andro.Diagnostics;
using JetBrains.Annotations;
using Novus;
using SimpleCore.Utilities;

#nullable enable
namespace Andro
{
	/*
	 *
	 */
	public static class Program
	{
		public static void Main(string[] args)
		{

			//Global.DumpDependencies();
			//Console.ReadLine();
			Console.WriteLine("Hello World!");

			/*var n = "192.168.1.234:5555";

			foreach (string device in Device.AvailableDevices) {
				Console.WriteLine(device);
			}

			var d = new Device();
			Console.WriteLine(d);

			var f  = @"C:\Users\Deci\Downloads\unnamed.jpg";
			var f2 = "sdcard/unnamed.jpg";

			d.Remove(f2);*/

			var data = ReadFromArguments();

			Console.WriteLine(">> {0}", data);
		}

		private static object? ReadFromArguments()
		{
			var args = Environment.GetCommandLineArgs()
				.Skip(1)
				.ToArray();

			Debug.WriteLine(args.QuickJoin(" "));

			bool noArgs = args.Length == 0;

			if (noArgs) {

				return null;
			}

			var       argQueue      = new Queue<string>(args);
			using var argEnumerator = argQueue.GetEnumerator();

			var d = new Device(Device.FirstAvailableDevice);

			while (argEnumerator.MoveNext()) {
				string argValue = argEnumerator.Current;

				// todo: structure

				switch (argValue) {
					case "fsize":
						argEnumerator.MoveNext();
						var file = argEnumerator.Current;

						argEnumerator.MoveNext();

						return d.GetFileSize(file);

						break;

					case "pushall":
						argEnumerator.MoveNext();
						var dir = argEnumerator.Current;

						argEnumerator.MoveNext();
						var rdir = argEnumerator.Current;

						argEnumerator.MoveNext();

						d.PushAll(dir, rdir);

						break;
					default:
						break;
				}
			}

			return null;
		}
	}
}