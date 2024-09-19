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
		RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);

		AndroPipe.OnPipeMessage += async s =>
		{
			// Console.WriteLine($"pi: {s}");

			// TODO: WTF JUST SERIALIZE THE DATA IN A STRUCTURED WAY !!!!!!!!!!!!!!!!

			if (s[0] == AndroPipe.MSG_DELIM) {
				int pid = int.Parse(s[1..^1]);
				Debug.WriteLine("full msg");
				AndroPipe.Inter++;

				var args = AndroPipe.PipeBag.ToArray();
				Array.Reverse(args);
				await ParseArgsAsync(args);
				AndroPipe.PipeBag.Clear();
			}
			else {
				AndroPipe.PipeBag.Add(s);

			}

			/*AnsiConsole.Clear();
			AnsiConsole.Write(new FigletText("Andro"));
			AnsiConsole.WriteLine($"{AndroPipe.PipeBag.Count} msg | {AndroPipe.Inter}");*/
		};

		Console.CancelKeyPress += (sender, args) =>
		{
			Debug.WriteLine($"{sender} {args} ctrl-c");

			Cts.Cancel();
		};

		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			AnsiConsole.WriteLine($"{sender} {args.ExceptionObject}");
		};
	}

	public static readonly CancellationTokenSource Cts = new();

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

		var b = _mutex.WaitOne(TimeSpan.Zero, true);

		if (b) {


			try {

				await ParseArgsAsync(args);

				AnsiConsole.Clear();
				AnsiConsole.Write(new FigletText("Andro"));
				AnsiConsole.WriteLine($"{AndroPipe.PipeBag.Count} msg");
				AndroPipe.StartServer();

				await Task.Delay(-1, Cts.Token);

			}
			finally {
				_mutex.ReleaseMutex();
			}
		}
		else {
			AnsiConsole.WriteLine($">> {args.Length} to process");
			AndroPipe.SendMessage(args);
		}

		return 0;
	}



	internal static async Task ParseArgsAsync(string[] args)
	{
		var e = args.GetEnumerator();

		while (e.MoveNext()) {
			var v = (string) e.Current;

			if (v == R2.Arg_CtxMenu) {
				var res = AppIntegration.HandleContextMenu(!AppIntegration.IsContextMenuAdded);
				var sty = AppShell.GetStyleForNullable(res);
				AnsiConsole.Write(new Text($"Context menu integration: {res}", sty));
				continue;
			}

			

			if (v == R2.Arg_SendTo) {
				var res = AppIntegration.HandleSendToMenu();

				var sty = AppShell.GetStyleForNullable(res);
				AnsiConsole.Write(new Text($"Send-to integration: {res}", sty));

				continue;
			}

			if (v == R2.Arg_Push) {
				var f = (string) e.MoveAndGet();
				var d = (string) e.MoveAndGet();
				d ??= AdbDevice.SDCARD;
				f =   f.CleanString();
				d =   d.CleanString();

				var sb  = new StringBuilder();
				var sb2 = new StringBuilder();

				var cmd = AdbShell.BuildPush(f, d,
				                             PipeTarget.ToStringBuilder(sb),
				                             PipeTarget.ToStringBuilder(sb2));

				var x = await cmd.ExecuteAsync();

				if (x.IsSuccess) {
					AnsiConsole.WriteLine($"{x} : {sb}");
				}

				continue;
			}

			if (v == R2.Arg_Clipboard) {
				var d = (string) e.MoveAndGet();
				Debug.WriteLine($"clipboard arg mag : {d}");
				Clipboard.Open();
				var cbDragQuery = Clipboard.GetDragQueryList();

				await Parallel.ForEachAsync(cbDragQuery, async (s, token) =>
				{
					var sb  = new StringBuilder();
					var sb2 = new StringBuilder();

					var cmd = AdbShell.BuildPush(s, AdbDevice.SDCARD,
					                             PipeTarget.ToStringBuilder(sb),
					                             PipeTarget.ToStringBuilder(sb2));

					var x = await cmd.ExecuteAsync(token);

					if (x.IsSuccess) {
						// ...
					}
				});

				Clipboard.Close();

				continue;
			}

			if (v == R2.Arg_PushAll) {
				// Debugger.Break();

				var filesIdx = Array.IndexOf(args, R2.Arg_PushAll, 0) + 1;
				var files    = args[filesIdx..];

				var progress = AnsiConsole.Progress().Columns(new ProgressColumn[]
				{
					new TaskDescriptionColumn(), // Task description
					new ProgressBarColumn(),     // Progress bar
					new PercentageColumn(),      // Percentage
					new SpinnerColumn(),         // Spinner
				}).AutoRefresh(true);

				var t = progress.StartAsync(async (context) =>
				{
					var pt = context.AddTask("Send", false, files.Length);

					int n = 0;

					/*
					var prg = new Progress<string>(handler)
						{ };
					*/
					pt.StartTask();

					await Parallel.ForEachAsync(files, async (s, token) =>
					{
						var sb  = new StringBuilder();
						var sb2 = new StringBuilder();

						var dest = AdbDevice.SDCARD;

						var cmd = AdbShell.BuildPush(s, dest,
						                             PipeTarget.ToStringBuilder(sb),
						                             PipeTarget.ToStringBuilder(sb2));

						var desc     = $"{s} {Strings.Constants.ARROW_RIGHT} {dest}";
						var fileTask = context.AddTask(desc, false);
						fileTask.IsIndeterminate = true;
						fileTask.StartTask();

						// fileTask.Increment(50D);
						var result = await cmd.ExecuteAsync(token);


						if (result.IsSuccess) {
							n++;
							pt.Increment(n);
							fileTask.Description = $"{desc} {Strings.Constants.HEAVY_CHECK_MARK}";
						}

						fileTask.IsIndeterminate = false;
						fileTask.Increment(100D);

						// fileTask.Increment(50D);
						fileTask.StopTask();

					});
					pt.StopTask();

					return;
				});
				await t;
				continue;
			}
		}
	}

	// public static AdbShell Device { get; } = new AdbShell();

	private const char CTRL_Z = '\x1A';

	private static readonly Mutex _mutex = new(true, "{E70EAF8B-2A56-45F1-8EF2-8F6968A4B20E}");

}