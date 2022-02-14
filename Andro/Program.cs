using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Andro.Android;
using Andro.App;
using Andro.Properties;
using Andro.Utilities;
using JetBrains.Annotations;
using Kantan.Cli;
using Kantan.Collections;
using Kantan.Text;
using Microsoft.VisualBasic;
using Novus;
using Novus.OS;
using FileSystem = Novus.OS.FileSystem;
using Strings = Kantan.Text.Strings;

// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable SuspiciousTypeConversion.Global

// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo
#pragma warning disable IDE0060

namespace Andro;

/*
 *
 */
public static class Program
{
	public const string PULL_ALL    = "/pull-all";
	public const string PUSH_ALL    = "/push-all";
	public const string PUSH_FOLDER = "/push-folder";
	public const string FSIZE       = "/fsize";
	public const string DSIZE       = "/dsize";

	public const string APP_SENDTO = "/sendto";
	public const string APP_CTX    = "/ctx";

	public const string OP_ADD = "add";
	public const string OP_RM  = "rm";


	public static void Main(string[] args)
	{
#if TEST
		if (!args.Any()) {
			args = new[] { APP_SENDTO, OP_ADD, APP_CTX, OP_ADD };
			// args = new[] { PULL_ALL, "sdcard/dcim/snapchat", @"C:\users\deci\downloads" };
			// args = new[] { PULL_ALL, "sdcard/dcim/snapchat" };

		}
#endif
		RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);
		/*
		 * Setup
		 */

		Console.Title = Resources.Name;
		
		/*
		 *
		 */


		object data = ReadArguments(args);

		switch (data) {
			case null:
			{
				string input;

				while ((input = Console.ReadLine()) != null) {
					if (input == "exit") {
						break;
					}

					var rg  = input.Split(' ');
					var obj = ReadArguments(rg);
					Console.WriteLine(obj);
				}

				return;
			}
			// Console.WriteLine($">> {data}".AddColor(Color.PaleGreen));
			case AdbCommand[] commands:
			{
				foreach (AdbCommand adbCommand in commands) {
					Console.WriteLine(adbCommand);
				}

				break;
			}
		}

		ConsoleManager.WaitForInput();


	}
	
	[CBN]
	private static object ReadArguments(string[] args)
	{
		if (args == null || !args.Any()) {
			return null;
		}
#if DEBUG
		Console.WriteLine($">> {args.QuickJoin()}");
#endif
		Trace.WriteLine($">> {args.QuickJoin()}");

		IEnumerator<string> argEnumerator = args.GetEnumerator().Cast<string>();

		AdbDevice device = AdbDevice.First;

		Console.WriteLine($"{device.ToString().AddColor(Color.Aquamarine)}");

		while (argEnumerator.MoveNext()) {
			string cmd = argEnumerator.Current;


			switch (cmd) {
				case APP_SENDTO:
					HandleOption(argEnumerator, AppIntegration.HandleSendToMenu);
					break;
				case APP_CTX:
					HandleOption(argEnumerator, AppIntegration.HandleContextMenu);
					break;
				case PUSH_ALL:
					var localFiles = args.Skip(1).ToArray();

					return device.PushAll(localFiles);
				case PULL_ALL:
					// args = args.Skip(1).ToArray();

					string remFolder  = argEnumerator.MoveAndGet();
					string destFolder = Environment.CurrentDirectory;

					if (argEnumerator.MoveNext()) {
						destFolder = argEnumerator.Current;
					}

					return device.PullAll(remFolder, destFolder);
				case FSIZE:
					string file = argEnumerator.MoveAndGet();

					argEnumerator.MoveNext();
					return device.GetFileSize(file);
				case DSIZE:
					string folder = argEnumerator.MoveAndGet();

					argEnumerator.MoveNext();
					return device.GetFolderSize(folder);
				case PUSH_FOLDER:
					string dir  = argEnumerator.MoveAndGet();
					string rdir = argEnumerator.MoveAndGet();

					argEnumerator.MoveNext();
					device.PushFolder(dir, rdir);
					break;

				default:
					break;
			}
		}

		return null;
	}

	private static void HandleOption(IEnumerator<string> argEnumerator, Action<bool> f)
	{
		string op = argEnumerator.MoveAndGet();

		switch (op) {
			case OP_ADD:
				f(true);
				break;
			case OP_RM:
				f(false);
				break;
		}

		// argEnumerator.MoveNext();
	}
}