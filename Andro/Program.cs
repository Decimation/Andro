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
	private static AdbDevice _device;

	public const string PULL_ALL    = "/pull-all";
	public const string PUSH_ALL    = "/push-all";
	public const string PUSH_FOLDER = "/push-folder";
	public const string FSIZE       = "/fsize";
	public const string DSIZE       = "/dsize";

	public const string APP_SENDTO = "/sendto";
	public const string APP_CTX    = "/ctx";

	public const string OP_ADD = "add";
	public const string OP_RM  = "rm";


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

		object data = await ReadArguments(args);

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

				break;
			}
			case AdbCommand c:
				Console.WriteLine(c);
				break;
			case AdbCommand[] commands:
			{
				foreach (AdbCommand cmd in commands) {
					Console.WriteLine(cmd);
				}

				break;
			}
		}

		ConsoleManager.WaitForInput();

	}

	[CBN]
	private static async Task<object> ReadArguments(string[] args)
	{
		if (args == null || !args.Any()) {
			return null;
		}
#if DEBUG
		Console.WriteLine($">> {args.QuickJoin()}");
#endif
		Trace.WriteLine($">> {args.QuickJoin()}");

		var argEnum = args.GetEnumerator().Cast<string>();

		Console.WriteLine($"{_device.ToString().AddColor(Color.Aquamarine)}");

		while (argEnum.MoveNext()) {
			string current = argEnum.Current;


			switch (current) {
				case APP_SENDTO:
					HandleOption(argEnum, AppIntegration.HandleSendToMenu);
					break;
				case APP_CTX:
					HandleOption(argEnum, AppIntegration.HandleContextMenu);
					break;
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
					break;
				case "/push":
					string localSrcFile = argEnum.MoveAndGet();
					string remoteDest   = argEnum.MoveAndGet();

					argEnum.MoveNext();


					var async = _device.PushAsync(localSrcFile, remoteDest);

					ThreadPool.QueueUserWorkItem((c) =>
					{
						while (!async.IsCompleted) {
							/*var s=_device.GetFileSize(Path.Combine(remoteDest, Path.GetFileName(localSrcFile))
							                        .Replace('\\', '/'));*/
							// Console.Write($"\r{s}");
							if (async.IsCompleted)
							{
								break;
							}
							for (int i = 0; i <= 3; i++) {
								Console.Write($"\r{new string('.', i)}");

								if (async.IsCompleted) {
									break;
								}
								// Thread.Sleep(TimeSpan.FromSeconds(i)/3);
								Thread.Sleep(i*100);


							}
						}
					});
					return await @async;
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