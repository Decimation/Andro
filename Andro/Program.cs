using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Xml;
using Andro.Android;
using Andro.Com;
using Andro.Core;
using Andro.Diagnostics;
using Andro.Properties;
using JetBrains.Annotations;
using Kantan.Cli;
using Kantan.Text;
using Microsoft.VisualBasic;
using Novus;
using Novus.OS;
using FileSystem = Novus.OS.FileSystem;
using Strings = Kantan.Text.Strings;

// ReSharper disable SuspiciousTypeConversion.Global

// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo
#pragma warning disable IDE0060

namespace Andro
{
	/*
	 *
	 */
	public static class Program
	{
		private const  string ADB_PUSH   = "/adb-push";
		private const  string ADD_SENDTO = "add-sendto";
		private static string AppExe;

		static Program()
		{
			var mainModule = Process.GetCurrentProcess().MainModule;
			Debug.Assert(mainModule != null);
			AppExe = mainModule.FileName;

		}

		public static void Main(string[] args)
		{
#if TEST
			if (!args.Any()) {
				args = new[] {"ctx", "rm"};
			}
#endif

			Console.WriteLine($"{args?.QuickJoin()}");

			if (args != null && args.Any()) {
				switch (args.First()) {
					case ADD_SENDTO:
						AddSendTo();
						break;
					case ADB_PUSH:
						args = args.Skip(1).ToArray();

						var plr = Parallel.For(0, args.Length, (i, pls) =>
						{
							// var rx=Command.Run("adb", $"push {s} sdcard/");
							// rx.Start();

							var device = Device.First;

							var destFolder = "sdcard/";

							var cr = device.Push($"{args[i]}", destFolder);


							Console.WriteLine(cr.StandardOutput.QuickJoin("\n"));
							// Console.WriteLine(rx.StandardOutput.ReadLine());
						});

						break;
				}
			}


			/*
			 * Setup
			 */

			Console.Title = Resources.Name;


			/*
			 *
			 */

			var data = ReadFromArguments(args);

			Console.WriteLine(">> {0}", data);


			ConsoleManager.WaitForInput();
		}

		private static void AddSendTo()
		{

			var sendTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
			                          @"AppData\Roaming\Microsoft\Windows\SendTo");

			Debug.WriteLine($"{AppExe}");

			// string location = System.Reflection.Assembly.GetExecutingAssembly().Location;

			var link = (IShellLink) new ShellLink();

			// setup shortcut information
			// link.SetDescription("My Description");
			link.SetPath(AppExe);
			link.SetArguments(ADB_PUSH);

			// save it
			var file = (IPersistFile) link;
			// string       desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			file.Save(Path.Combine(sendTo, Resources.NameShortcut), false);
		}

		[CanBeNull]
		private static object ReadFromArguments(string[] args)
		{
			//var args = Environment.GetCommandLineArgs()
			//                      .Skip(1)
			//                      .ToArray();

			//args = args.Skip(1).ToArray();

			Debug.WriteLine(args.QuickJoin(Strings.Constants.SPACE.ToString()));


			if (!args.Any()) {
				return null;
			}

			var argQueue = new Queue<string>(args);

			using var argEnumerator = argQueue.GetEnumerator();

			var d = Device.First;

			Console.WriteLine(d);

			while (argEnumerator.MoveNext()) {
				string argValue = argEnumerator.Current;

				// todo: structure

				switch (argValue) {
					case "push":
						argEnumerator.MoveNext();
						var f = argEnumerator.Current;

						argEnumerator.MoveNext();
						var df = argEnumerator.Current;

						argEnumerator.MoveNext();

						d.Push(f, df);

						break;
					case "fsize":
						argEnumerator.MoveNext();
						var file = argEnumerator.Current;

						argEnumerator.MoveNext();

						return d.GetFileSize(file);

					case "ctx":
						argEnumerator.MoveNext();
						var op = argEnumerator.Current;

						if (op == "add") {
							AppIntegration.Add();
						}

						if (op == "rm") {
							AppIntegration.Remove();

						}

						argEnumerator.MoveNext();
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