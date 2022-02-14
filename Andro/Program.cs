using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
	private static AdbDevice _device;

	public const string PULL_ALL    = "/pull-all";
	public const string PUSH_ALL    = "/push-all";
	public const string PUSH_FOLDER = "/push-folder";
	public const string FSIZE       = "/fsize";
	public const string DSIZE       = "/dsize";
	public const string PUSH        = "/push";

	public const string APP_SENDTO = "/sendto";
	public const string APP_CTX    = "/ctx";

	public const string OP_ADD = "add";
	public const string OP_RM  = "rm";

	private const char   CTRL_Z = '\x1A';
	private const string EXIT   = "exit";


	public static async Task Main(string[] args)
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

		_device = AdbDevice.First;

		Console.WriteLine($">> {_device.ToString().AddColor(Color.Aquamarine)}");

		object data;
		string input;
		data = await ReadArguments(args)!;
		Print(data);

		while ((input = Console.ReadLine()) != null) {
			input = input.Trim();


			var inputArgs = input.Split(' ');
			data = await ReadArguments(inputArgs)!;
			Print(data);
			continue;
		}


		// ConsoleManager.WaitForInput();
	}

	private static void Print(object data)
	{
		switch (data) {
			case AdbCommand c:
				Trace.WriteLine(c);
				break;
			case AdbCommand[] commands:
			{
				foreach (AdbCommand cmd in commands) {
					Trace.WriteLine(cmd);
				}

				break;
			}
			default:
				Console.WriteLine(data);
				break;
		}
	}

	[CBN]
	private static async Task<object> ReadArguments(string[] args)
	{
		if (args == null || !args.Any()) {
			return null;
		}
#if DEBUG
		Console.WriteLine($">> {args.QuickJoin()}".AddColor(Color.Beige));
#endif
		Trace.WriteLine($">> {args.QuickJoin()}");

		var argEnum = args.GetEnumerator().Cast<string>();

		while (argEnum.MoveNext()) {
			string current = argEnum.Current;


			switch (current) {
				case EXIT:
					goto default;
				case APP_SENDTO:
					return HandleOption(argEnum, AppIntegration.HandleSendToMenu);
				case APP_CTX:
					return HandleOption(argEnum, AppIntegration.HandleContextMenu);
				case PUSH_ALL:
					var localFiles = args.Skip(1).ToArray();

					return _device.PushAll(localFiles);
				case PULL_ALL:
					// args = args.Skip(1).ToArray();

					string remFolder  = argEnum.MoveAndGet();
					string destFolder = Environment.CurrentDirectory;

					if (argEnum.MoveNext()) {
						destFolder = argEnum.Current;
					}

					return _device.PullAll(remFolder, destFolder);
				case FSIZE:
					string file = argEnum.MoveAndGet();

					argEnum.MoveNext();
					return _device.GetFileSize(file);
				case DSIZE:
					string folder = argEnum.MoveAndGet();

					argEnum.MoveNext();
					return _device.GetFolderSize(folder);
				case PUSH_FOLDER:
					string dir  = argEnum.MoveAndGet();
					string rdir = argEnum.MoveAndGet();

					argEnum.MoveNext();
					return _device.PushFolder(dir, rdir);
				case PUSH:
					string localSrcFile = argEnum.MoveAndGet();
					string remoteDest   = argEnum.MoveAndGet();

					argEnum.MoveNext();

					var pushTask = _device.PushAsync(localSrcFile, remoteDest);

					ThreadPool.QueueUserWorkItem((c) =>
					{
						do {
							if (pushTask.IsCompleted) {
								break;
							}

							for (int i = 0; i <= 3; i++) {
								Console.Write($"\r{new string('.', i)}");

								if (pushTask.IsCompleted) {
									break;
								}

								Thread.Sleep(i * 100);
							}
						} while (!pushTask.IsCompleted);
					});
					return await pushTask;

				case { } when current.Contains(CTRL_Z):
				default:

					break;
			}
		}

		return null;
	}

	private static bool? HandleOption(IEnumerator<string> argEnumerator, Func<bool?, bool?> f)
	{
		string op = argEnumerator.MoveAndGet();

		switch (op) {
			case null:
				return f(null);

			case OP_ADD:
				return f(true);
				break;
			case OP_RM:
				return f(false);
				break;
		}

		return null;

		// argEnumerator.MoveNext();
	}
}