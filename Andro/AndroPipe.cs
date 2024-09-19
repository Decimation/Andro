using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Novus.Utilities;

namespace Andro;

internal static class AndroPipe
{

	public static int Inter = 0;

	/*
	 * TODO: maybe channels or concurrentqueue
	 */
	internal static ConcurrentBag<string> PipeBag = new();

	private const int SW_HIDE = 0;

	/// <summary>
	/// This identifier must be unique for each application.
	/// </summary>
	public const string SingleGuid = "{910e8c27-ab31-4043-9c5d-1382707e6c93}";

	public const string IPC_PIPE_NAME = "SIPC";

	public static NamedPipeServerStream PipeServer { get; private set; }

	public static Thread PipeThread { get; private set; }

	public delegate void PipeMessageCallback(string s);

	public static event PipeMessageCallback OnPipeMessage;


	internal static void StartServer()
	{
		PipeServer = new NamedPipeServerStream(IPC_PIPE_NAME, PipeDirection.In);

		PipeThread = new Thread(PipeRoutine)
		{
			IsBackground = true
		};

		PipeThread.Start();
	}

	private static void PipeRoutine()
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
	}

	public static void SendMessage(string[] msg)
	{

		using (var pipe = new NamedPipeClientStream(".", IPC_PIPE_NAME, PipeDirection.Out))
			using (var stream = new StreamWriter(pipe)) {
				pipe.Connect();

				foreach (var s in msg) {
					stream.WriteLine(s);
				}

				stream.Write(MSG_DELIM);
				stream.Write(ProcessHelper.GetParent().Id);
				stream.Write(MSG_DELIM);
				stream.WriteLine();
			}
	}

	public const char MSG_DELIM = '\0';

}