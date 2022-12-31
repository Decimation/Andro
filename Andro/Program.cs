using System.Buffers.Binary;
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
		Console.WriteLine(await dev.GetState());
		await h.RunAsync();
	}

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