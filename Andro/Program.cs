using Andro.App;
using Andro.Commands;
using Andro.IPC;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;

// using Andro.Kde;


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

		s_logger = AppIntegration.LoggerFactoryInt.CreateLogger(nameof(Program));
	}

	private static readonly CancellationTokenSource _cts = new();

	private const char CTRL_Z = '\x1A';

	private static readonly Mutex _mutex = new(true, "{E70EAF8B-2A56-45F1-8EF2-8F6968A4B20E}");

	private static readonly ILogger s_logger;

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


		AndroPipeManager.OnPipeMessage += async s =>
		{
			//
			s_logger.LogDebug("{Message}", s);
		};

		Console.CancelKeyPress += (sender, args) =>
		{
			//
			s_logger.LogDebug("{Sender} {Args}", sender, args);
			_cts.Cancel();
		};

		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			var exception = args.ExceptionObject as Exception;
			AnsiConsole.WriteException(exception);
			s_logger.LogError(exception, "Error");
		};

		Console.Title = R1.Name;

		var cmdApp = new CommandApp();
		// cmdApp.SetDefaultCommand<>()

		cmdApp.Configure(cfg =>
		{
			cfg.PropagateExceptions();
			cfg.SetHelpProvider(new CustomHelpProvider(cfg.Settings));
			
			cfg.AddCommand<IntegrationCommand>(R2.Arg_Integration);
			cfg.AddCommand<ClipboardCommand>(R2.Arg_Clipboard);
			cfg.AddCommand<PushCommand>(R2.Arg_Push);
			cfg.AddCommand<PushAllCommand>(R2.Arg_PushAll);

		});

		var b = _mutex.WaitOne(TimeSpan.Zero, true);

		int res = 0;

		if (b) {


			try {

				res = await cmdApp.RunAsync(args);

				/*AnsiConsole.Clear(); //todo
				AnsiConsole.Write(AppInterface._nameFiglet);
				AnsiConsole.WriteLine($"{AndroPipeManager.PipeBag.Count} msg");
				AndroPipeManager.StartServer();

				await Task.Delay(Timeout.Infinite, _cts.Token);*/

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

		return res;
	}

}