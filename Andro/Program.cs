using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Andro.Adb.Android;
using Andro.App;
using Andro.Kde;
using Andro.Adb.Properties;
using Kantan.Collections;
using Kantan.Text;
using Microsoft.Extensions.Hosting;
using Novus.Streams;
using Spectre.Console;
using System.IO.Pipes;
using Novus.Utilities;

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
	private const char CTRL_Z = '\x1A';

	static Program()
	{
		RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);

	}

	public static async Task<int> Main(string[] args)
	{
#if TEST
#endif

#if DEBUG
#endif

		Trace.WriteLine($"{args.QuickJoin()}");

		await parseArgs(args);

		if (mutex.WaitOne(TimeSpan.Zero, true)) {
			// This instance acquired the mutex, it's the first instance.
			// Continue with your application logic here.

			using IHost h = Host.CreateDefaultBuilder()
				.ConfigureHostOptions((a, b) =>
				{
					a.HostingEnvironment.ApplicationName = R1.Name;
				})
				.ConfigureLogging((a, b) => { })
				.Build();

			/*
			 * Setup
			 */

			Console.Title = R1.Name;

			// ...
			OnPipeMessage += s =>
			{
				Console.WriteLine($"pi: {s}");
			};

			// Release the mutex when your application is done.
			var ht = h.RunAsync();
			mutex.ReleaseMutex();

			StartServer();

			await ht;
			return 0;
		}
		else {
			// Another instance is already running.
			// You can choose to intercept data here or take other actions.
			Console.WriteLine($"Another instance is already running. {args.QuickJoin()}");
			// Console.ReadKey();
			SendMessage(args);

		}

		return 0;
	}

	static async Task parseArgs(string[] args)
	{
		var e = args.GetEnumerator();

		while (e.MoveNext()) {
			var v = (string) e.Current;

			if (v == R2.Arg_CtxMenu) {
				AppIntegration.HandleContextMenu(!AppIntegration.IsContextMenuAdded);
				continue;
			}

		}
	}

	static Mutex mutex = new Mutex(true, SingleGuid);

	/// <summary>
	/// This identifier must be unique for each application.
	/// </summary>
	public const string SingleGuid = "{910e8c27-ab31-4043-9c5d-1382707e6c93}";

	public const string IPC_PIPE_NAME = "SIPC";

	public const char ARGS_DELIM = '\0';

	public static NamedPipeServerStream PipeServer { get; private set; }

	public static Thread PipeThread { get; private set; }

	private static void SendMessage(string[] e)
	{

		using (var pipe = new NamedPipeClientStream(".", IPC_PIPE_NAME, PipeDirection.Out))
		using (var stream = new StreamWriter(pipe)) {
			pipe.Connect();

			foreach (var s in e) {
				stream.WriteLine(s);
			}

			stream.Write($"{ARGS_DELIM}{ProcessHelper.GetParent().Id}");
		}
	}

	public delegate void PipeMessageCallback(string s);

	public static event PipeMessageCallback OnPipeMessage;

	private static void StartServer()
	{
		PipeServer = new NamedPipeServerStream(IPC_PIPE_NAME, PipeDirection.In);

		PipeThread = new Thread(() =>
		{
			while (true) {
				PipeServer.WaitForConnection();
				var sr = new StreamReader(PipeServer);

				while (!sr.EndOfStream) {
					var v = sr.ReadLine();
					OnPipeMessage?.Invoke(v);
				}

				PipeServer.Disconnect();
			}
		})
		{
			IsBackground = true
		};
		PipeThread.Start();
	}
}