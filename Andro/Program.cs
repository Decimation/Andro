using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Xml;
using Andro.Android;
using Andro.Diagnostics;
using Andro.Properties;
using Andro.Utilities;
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

namespace Andro;

/*
 *
 */
public static class Program
{
	public const string ADB_PUSH   = "/adb-push";
	public const string ADD_SENDTO = "/sendto";
	public const string ADD_CTX    = "/ctx";

	private const string ADD = "add";
	private const string RM  = "rm";

	public static string AppExe;

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

		/*
		 * Setup
		 */

		Console.Title = Resources.Name;

		/*
		 *
		 */

		var data = ReadFromArguments(args);

		Trace.WriteLine($">> {data}");

		if (data is true) {
			ConsoleManager.WaitForInput();

		}
	}

	[CanBeNull]
	private static object ReadFromArguments(string[] args)
	{

		// Debug.WriteLine(args.QuickJoin(Strings.Constants.SPACE.ToString()));

		if (args == null) {
			return null;

		}

		if (!args.Any()) {

			return null;
		}

		Trace.WriteLine($"{args.QuickJoin()}");

		var argQueue = new Queue<string>(args);

		using var argEnumerator = argQueue.GetEnumerator();

		var d = Device.First;

		Console.WriteLine(d);

		while (argEnumerator.MoveNext()) {
			string argValue = argEnumerator.Current;
			string op       = null;
			// todo: structure

			switch (argValue) {
				case ADD_SENDTO:
					argEnumerator.MoveNext();
					op = argEnumerator.Current;

					HandleOption(op, argEnumerator, AppIntegration.HandleSendTo);
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
					return true;
				case "fsize":
					argEnumerator.MoveNext();
					var file = argEnumerator.Current;

					argEnumerator.MoveNext();

					return d.GetFileSize(file);

				case ADD_CTX:
					argEnumerator.MoveNext();
					op = argEnumerator.Current;

					HandleOption(op, argEnumerator, AppIntegration.HandleCtx);
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

	private static void HandleOption(string op, Queue<string>.Enumerator argEnumerator, Action<bool> f)
	{
		switch (op) {
			case ADD:
				f(true);
				break;
			case RM:
				f(false);
				break;
		}

		argEnumerator.MoveNext();
	}
}