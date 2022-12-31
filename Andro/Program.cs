﻿using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using Andro.Lib.Android;
using Andro.App;
using Andro.Lib.Properties;
using Microsoft.Extensions.Hosting;
using Novus.Streams;

// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable SuspiciousTypeConversion.Global

// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo
#pragma warning disable IDE0060

namespace Andro;

/*
https://github.com/vidstige/jadb/blob/master/src/se/vidstige/jadb/JadbDevice.java#L60
https://github.com/vidstige/jadb/blob/master/src/se/vidstige/jadb/JadbConnection.java
https://github.com/vidstige/jadb/blob/master/src/se/vidstige/jadb/Transport.java
https://github.com/vidstige/jadb/blob/master/src/se/vidstige/jadb/SyncTransport.java
https://github.com/vidstige/jadb/tree/master/src/se/vidstige/jadb
https://github.com/vidstige/jadb/blob/master/src/se/vidstige/jadb/AdbFilterInputStream.java

 */
public static class Program
{
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

		}
#endif

#if DEBUG

#endif

		using IHost h = Host.CreateDefaultBuilder()
			.ConfigureHostOptions((a, b) =>
			{
				a.HostingEnvironment.ApplicationName = Resources.Name;
			})
			.ConfigureLogging((a, b) => { })
			.Build();

		RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);

		/*
		 * Setup
		 */

		Console.Title = Resources.Name;

		/*
		 *
		 */

		string s = null;

		// var d = new AdbTransport();

		// s = await d.TrackDevicesAsync();
		// s = await d.TrackDevicesAsync();
		// Console.WriteLine(s);
		// await d.ConnectTransport();
		// await d.VerifyAsync();

		var d1 = new AdbConnection();

		var                 devices    = await d1.GetDevicesAsync();
		var                 dev        = devices.First();
		var o = (await dev.ShellAsync("echo butt"));

		var nr = new StreamReader(o);
		Console.WriteLine(await nr.ReadToEndAsync());
		
		// d.Dispose();
		// d = new AdbDevice();

		/*var payload = AdbHelper.GetPayload("host:track-devices", out var rg, out var rg2);

		await d.Tcp.Client.SendAsync(rg2);
		var buffer = MemoryPool<byte>.Shared.Rent(8192);
		await d.Tcp.Client.ReceiveAsync(buffer.Memory);
		Console.WriteLine(Encoding.UTF8.GetString(buffer.Memory.Span));*/
		// var bytes = await d.ShellAsync("echo", new[] { "butt"});

		/*var bytes = await d.ShellAsync("ls", new[] { "-lR", "sdcard/pictures/" });
		Console.WriteLine(bytes);
		Console.WriteLine(d.IsAlive);
		// await d.SendAsync("sync:list sdcard/pictures/");
		Console.WriteLine(d.NetworkStream.DataAvailable);*/
	
		await h.RunAsync();
	}

	/*[CBN]
	private static async Task<object> ReadArguments(string[] args)
	{
		if (args == null || !args.Any()) {
			return null;
		}

#if DEBUG
		Console.WriteLine($">> {args.QuickJoin()}".AddColor(Color.Beige));
#endif
		Trace.WriteLine($">> {args.QuickJoin()}");

		// var argEnum = args.GetEnumerator().Cast<string>();

		for (int i = 0; i < args.Length; i++) {

			var current = args[i];

			switch (current) {
				case "sh":
					string input = null;

					var ps = AdbDevice.GetShell(AdbDevice.ADB_SHELL);

					while ((input = Console.ReadLine()) != null) {
						input = input.Trim();

						ps.StandardInput.WriteLine(input);
						ps.StandardInput.Flush();
						Trace.WriteLine($">>{input}");
						string b = null;

						ps.ErrorDataReceived += (sender, eventArgs) =>
						{
							Console.Error.WriteLine(eventArgs.Data);
						};

						ps.OutputDataReceived += (sender, eventArgs) =>
						{
							Console.WriteLine(eventArgs.Data);
						};

						if (input == "exit") {
							break;
						}
					}

					break;
				case "gi":
					// return _device.GetItems(args[++i..]);
					var c = AdbCommand.find.Build(args2: args[++i..]);
					return c;
				case EXIT:
					goto default;
				case APP_SENDTO:

					return HandleOption(args[++i], AppIntegration.HandleSendToMenu);
				case APP_CTX:

					return HandleOption(args[++i], AppIntegration.HandleContextMenu);
				/*case PUSH_ALL:

					var localFiles = args[++i..];

					return _device.PushAll(localFiles);
				case PULL_ALL:

					string remFolder = args[++i];

					var ssb = args.TryIndex(++i, out var destFolder);
					destFolder ??= Environment.CurrentDirectory;
					Console.WriteLine($"{remFolder} {Strings.Constants.ARROW_RIGHT} {destFolder}");
					return _device.PullAll(remFolder, destFolder);#1#
				case FSIZE:
					string file = args[++i];
					return _device.GetFileSize(file);
				case DSIZE:
					// string folder = args[++i];
					// return _device.GetFolderSize(folder);
					return _device.GetFolderSize(args[++i..].QuickJoin(" "));
				/*case PUSH_FOLDER:
					string dir  = args[++i];
					string rdir = args[++i];

					i++;
					return _device.PushFolder(dir, rdir);#1#
				case PUSH:
					string localSrcFile = args[++i];
					string remoteDest   = args[++i];

					i++;

					var pushTask = _device.PushAsync(localSrcFile, remoteDest);

					ThreadPool.QueueUserWorkItem(c =>
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
	*/

	private static bool? HandleOption(string op, Func<bool?, bool?> f)
	{

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