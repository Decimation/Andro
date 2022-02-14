using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Andro.Android;
using Andro.Diagnostics;
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
	public const  string PUSH_ALL    = "/push-all";
	private const string FSIZE       = "/fsize";
	private const string PUSH_FOLDER = "/push-folder";

	private const string APP_SENDTO = "/sendto";
	private const string APP_CTX    = "/ctx";

	private const string OP_ADD = "add";
	private const string OP_RM  = "rm";


	public static void Main(string[] args)
	{
#if TEST
		if (!args.Any()) {
			args = new[] { APP_SENDTO, OP_ADD, APP_CTX, OP_ADD };
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

		Console.WriteLine($">> {data}".AddColor(Color.PaleGreen));

		if (data is true) {
			ConsoleManager.WaitForInput();
		}


	}

	[CanBeNull]
	private static object ReadFromArguments(string[] args)
	{
		if (args == null || !args.Any()) {
			return null;
		}
#if DEBUG
		Console.WriteLine($">> {args.QuickJoin()}");
#endif
		Trace.WriteLine($">> {args.QuickJoin()}");

		var argEnumerator = args.GetEnumerator().Cast<string>();

		var device = Device.First;

		Console.WriteLine($"{device.ToString().AddColor(Color.Aquamarine)}");

		while (argEnumerator.MoveNext()) {
			string argValue = argEnumerator.Current;

			// todo: structure

			switch (argValue) {
				case APP_SENDTO:
					HandleOption(argEnumerator, AppIntegration.HandleSendToMenu);
					break;
				case APP_CTX:
					HandleOption(argEnumerator, AppIntegration.HandleContextMenu);
					break;
				case PUSH_ALL:
					args = args.Skip(1).ToArray();
					device.PushAll(args);
					return true;
				case FSIZE:
					argEnumerator.MoveNext();
					var file = argEnumerator.Current;

					argEnumerator.MoveNext();
					return device.GetFileSize(file);
				case PUSH_FOLDER:
					argEnumerator.MoveNext();
					var dir = argEnumerator.Current;

					argEnumerator.MoveNext();
					var rdir = argEnumerator.Current;

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
		argEnumerator.MoveNext();
		var op = argEnumerator.Current;

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