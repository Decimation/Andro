using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text.Json;
using Andro.App;
using Microsoft.Extensions.Logging;

namespace Andro.IPC;

public static class AndroPipeManager
{

	internal static int Inter = 0;

	/*
	 * TODO: maybe channels or concurrentqueue
	 */
	
	internal static ConcurrentBag<string> PipeBag = new();


	private const int SW_HIDE = 0;

	public const string IPC_PIPE_NAME = "SIPC";

	public static NamedPipeServerStream PipeServer { get; private set; }

	public static Thread PipeThread { get; private set; }

	public delegate void PipeMessageCallback(AndroPipeData s);

	public static event PipeMessageCallback OnPipeMessage;

	private static readonly ILogger s_logger;

	static AndroPipeManager()
	{
		s_logger = AppIntegration.LoggerFactoryInt.CreateLogger(nameof(AndroPipeManager));
	}

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

			// var sr = new StreamReader(PipeServer);

			if (PipeServer.CanRead) {
				var data = JsonSerializer.Deserialize<AndroPipeData>(PipeServer, JsonSerializerOptions.Default);
				OnPipeMessage?.Invoke(data);
			}

			/*while (!sr.EndOfStream) {
				// var line = sr.ReadLine();
				OnPipeMessage?.Invoke(line);
			}*/

			// OnPipeMessage?.Invoke(null);

			PipeServer.Disconnect();
		}
	}

	public static void SendMessage(AndroPipeData data)
	{
		using var pipe = new NamedPipeClientStream(".", IPC_PIPE_NAME, PipeDirection.Out);

		using var stream = new StreamWriter(pipe);

		pipe.Connect();

		/*foreach (var s in msg) {
					stream.WriteLine(s);
				}

				stream.Write(MSG_DELIM);
				stream.Write(ProcessHelper.GetParent().Id);
				stream.Write(MSG_DELIM);
				stream.WriteLine();*/

		var dataSerialized = JsonSerializer.Serialize(data, JsonSerializerOptions.Default);
		stream.Write(dataSerialized);

	}

}