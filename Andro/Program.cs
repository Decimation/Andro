using System.ServiceModel;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Andro.Adb.Android;
using Andro.App;

// using Andro.Kde;
using Andro.Adb.Properties;
using Kantan.Collections;
using Kantan.Text;
using Microsoft.Extensions.Hosting;
using Novus.Streams;
using Spectre.Console;
using System.IO.Pipes;
using System.Text;
using Andro.Adb;
using Novus.Utilities;
using System.Threading;
using CliWrap;
using Novus.Win32;
using Andro.Kde;
using Andro.Comm;
using Spectre.Console.Cli;
using Andro.Commands;


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

	static Program()
	{
		// RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);

		AndroPipeManager.OnPipeMessage += async s =>
		{
			// TODO: WTF JUST SERIALIZE THE DATA IN A STRUCTURED WAY !!!!!!!!!!!!!!!!

			// TODO: Coming back to this project after 1 year of inactivity, and my lack of (self) documentation
			//		 has come back to bite me

			Debug.WriteLine($"{nameof(AndroPipeManager.OnPipeMessage)} :: {s}");
			
			await ParseArgsAsync(s.Data);

			/*if (s[0] == AndroPipeManager.MSG_DELIM) {
				int pid = int.Parse(s[1..^1]);
				Debug.WriteLine("full msg");
				AndroPipeManager.Inter++;

				var args = AndroPipeManager.PipeBag.ToArray();
				Array.Reverse(args);
				await ParseArgsAsync(args);
				AndroPipeManager.PipeBag.Clear();
			}
			else {
				AndroPipeManager.PipeBag.Add(s);

			}*/

			/*AnsiConsole.Clear();
			AnsiConsole.Write(new FigletText("Andro"));
			AnsiConsole.WriteLine($"{AndroPipeManager.PipeBag.Count} msg | {AndroPipeManager.Inter}");*/
		};

		Console.CancelKeyPress += (sender, args) =>
		{
			Debug.WriteLine($"{sender} {args} ctrl-c");

			Cts.Cancel();
		};

		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			AnsiConsole.WriteException(args.ExceptionObject as Exception, ExceptionFormats.Default);
		};
	}

	public static readonly CancellationTokenSource Cts = new();

	private const char CTRL_Z = '\x1A';

	private static readonly Mutex _mutex = new(true, "{E70EAF8B-2A56-45F1-8EF2-8F6968A4B20E}");

	public static async Task<int> Main(string[] args)
	{

#if TEST
#endif
#if DEBUG
#endif

		/*
		using IHost h = Host.CreateDefaultBuilder()
			.ConfigureHostOptions((a, b) =>
			{
				a.HostingEnvironment.ApplicationName = R1.Name;
			})
			.ConfigureLogging((a, b) => { })
			.Build();
			*/

		Console.Title = R1.Name;

		var cmdApp = new CommandApp();

		cmdApp.Configure(cfg =>
		{
			cfg.AddCommand<IntegrationCommand>(R2.Arg_Integration);
			cfg.AddCommand<ClipboardCommand>(R2.Arg_Clipboard);
			cfg.AddCommand<PushCommand>(R2.Arg_Push);
			cfg.AddCommand<PushAllCommand>(R2.Arg_PushAll);

		});

		var b = _mutex.WaitOne(TimeSpan.Zero, true);

		if (b) {


			try {

				await ParseArgsAsync(args);

				AnsiConsole.Clear();
				AnsiConsole.Write(AppInterface._nameFiglet);
				AnsiConsole.WriteLine($"{AndroPipeManager.PipeBag.Count} msg");
				AndroPipeManager.StartServer();

				await Task.Delay(-1, Cts.Token);

			}
			finally {
				_mutex.ReleaseMutex();
			}
		}
		else {
			var data = AndroPipeData.FromArgs(args);
			AnsiConsole.WriteLine($">> {data} to process");
			AndroPipeManager.SendMessage(data);
		}

		return 0;
	}
	

	

	// public static AdbShell Device { get; } = new AdbShell();

}