using System.ServiceModel;
using System.Buffers.Binary;
using System.Collections.Concurrent;
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Andro.Adb;
using Novus.Utilities;
using System.Threading;
using Novus.Win32;

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
	private static bool _createdNew;
	private const  char CTRL_Z = '\x1A';
	private static int  inter  = 0;

	static Program()
	{
		RuntimeHelpers.RunClassConstructor(typeof(AppIntegration).TypeHandle);

		OnPipeMessage += async s =>
		{
			// Console.WriteLine($"pi: {s}");

			if (s[0] == ARGS_DELIM) {
				int pid = int.Parse(s[1..^1]);
				Debug.WriteLine("full msg");
				inter++;

				var args = m_pipe.ToArray();
				Array.Reverse(args);
				await parseArgs(args);
				m_pipe.Clear();
			}
			else {
				m_pipe.Add(s);

			}

			AnsiConsole.Clear();
			AnsiConsole.Write(new FigletText("Andro"));
			AnsiConsole.WriteLine($"{m_pipe.Count} msg | {inter}");
		};

		AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
		{
			Console.WriteLine($"{sender} {args.ExceptionObject}");
		};
	}

	public static  AdbcDevice            Device { get; } = new AdbcDevice();
	private static ConcurrentBag<string> m_pipe = new ConcurrentBag<string>();

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	const int SW_HIDE = 0;

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

		var b = mutex.WaitOne(TimeSpan.Zero, true);

		if (b) {
			try {

				await parseArgs(args);
				AnsiConsole.Clear();
				AnsiConsole.Write(new FigletText("Andro"));
				AnsiConsole.WriteLine($"{m_pipe.Count} msg");
				StartServer();

				await Task.Delay(-1);
			}
			finally {
				mutex.ReleaseMutex();
			}
		}
		else {
			AnsiConsole.WriteLine($">> {args.Length} to process");
			SendMessage(args);
		}

		return 0;
	}

	class SingleGlobalInstance : IDisposable
	{
		//edit by user "jitbit" - renamed private fields to "_"
		public bool _hasHandle = false;
		Mutex       _mutex;

		private void InitMutex()
		{
			string appGuid = ((GuidAttribute) Assembly.GetExecutingAssembly()
					                 .GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
			string mutexId = string.Format("Global\\{{{0}}}", appGuid);
			_mutex = new Mutex(false, mutexId);

			var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
			                                            MutexRights.FullControl, AccessControlType.Allow);
			var securitySettings = new MutexSecurity();
			securitySettings.AddAccessRule(allowEveryoneRule);
			_mutex.SetAccessControl(securitySettings);
		}

		public SingleGlobalInstance(int timeOut)
		{
			InitMutex();

			try {
				if (timeOut < 0)
					_hasHandle = _mutex.WaitOne(Timeout.Infinite, false);
				else
					_hasHandle = _mutex.WaitOne(timeOut, false);

				if (_hasHandle == false)
					throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
			}
			catch (AbandonedMutexException) {
				_hasHandle = true;
			}
		}

		public void Dispose()
		{
			if (_mutex != null) {
				if (_hasHandle)
					_mutex.ReleaseMutex();
				_mutex.Close();
			}
		}
	}

	private static Semaphore semaphore;
	static         Mutex     mutex = new Mutex(true, "{E70EAF8B-2A56-45F1-8EF2-8F6968A4B20E}");

	static async Task parseArgs(string[] args)
	{
		var e = args.GetEnumerator();

		while (e.MoveNext()) {
			var v = (string) e.Current;

			if (v == R2.Arg_CtxMenu) {
				AppIntegration.HandleContextMenu(!AppIntegration.IsContextMenuAdded);
				continue;
			}

			if (v == R2.Arg_Push) {
				var f = (string) e.MoveAndGet();
				var d = (string) e.MoveAndGet();
				d ??= "sdcard/";
				f =   f.CleanString();
				d =   d.CleanString();
				var x = await Device.Push(f, d);

				continue;
			}

			if (v == R2.Arg_Clipboard) {
				var d = (string) e.MoveAndGet();

				Clipboard.Open();
				var f = Clipboard.GetDragQueryList();

				await Parallel.ForEachAsync(f, async (s, token) =>
				{
					var x = await Device.Push(s, "sdcard/");
				});

				Clipboard.Close();

				continue;
			}
		}
	}

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

			stream.Write(ARGS_DELIM);
			stream.Write(ProcessHelper.GetParent().Id);
			stream.Write('\0');
			stream.WriteLine();
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

				// OnPipeMessage?.Invoke(null);

				PipeServer.Disconnect();
			}
		})
		{
			IsBackground = true
		};
		PipeThread.Start();
	}
}