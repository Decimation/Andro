using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Andro.Android;
using Andro.App;
using Andro.Properties;
using Kantan.Text;
using Novus.Memory;

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

#if DEBUG
		// KillAdb();
#endif
		RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);

		/*
		 * Setup
		 */

		Console.Title = Resources.Name;

		/*
		 *
		 */

		string s = null;

		var d = new AdbDevice();

		s = await d.GetDevicesAsync();
		Console.WriteLine(s);
		d.Dispose();
		d = new AdbDevice();

		await d.SendAsync($"host:transport-any");
		await d.Verify();

		var bytes = await d.ShellAsync("ls", new []{ "sdcard/pictures/Reaction images/" });
		Console.WriteLine(bytes);
		Console.WriteLine(d.IsAlive);
		
		var async = await d.ShellAsync("echo hi");
		Console.WriteLine(async);
	}

	private static void KillAdb()
	{
		foreach (var v in Process.GetProcessesByName("adb")) {
			v.Kill(true);
			Console.WriteLine($"Killed {v.Id}");
		}
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