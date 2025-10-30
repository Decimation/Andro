using Andro.Kde;
using Andro.Lib.Daemon;
using Novus.Win32;

namespace Test;

public static class Program
{

	public static async Task<int> Main(string[] args)
	{
		using var adbTransport = new AdbTransport();
		var       devices      = await adbTransport.GetDevicesAsync();

		foreach (AdbDevice adbDevice in devices) {
			Console.WriteLine(adbDevice);
		}

		var device = devices.First();
		Console.WriteLine(await adbTransport.GetVersionAsync());

		return 0;
	}

	/*
	private static async Task Test2()
	{
		a:
		var d  = new AdbConnection();
		var t2 = await d.GetDevicesAsync();
		var dd = t2[0];

		/*var l = await dd.ShellAsync( "ls",new[]{ "sdcard/Download" });
		Console.WriteLine(l);#1#

		await dd.SyncPrep("sdcard/Download", "LIST");
		Console.ReadKey();
		goto a;
	}
	*/

	private static async Task Test1()
	{
		var k = await KdeConnect.InitAsync();

		Console.WriteLine(k);
		Clipboard.Open();

		var f = Clipboard.GetDragQueryList();

		var strings = new List<string>
		{
			@"C:\Users\Deci\Pictures\Epic anime\3e91c02d7dba20f9aeda7b4c7823b632.png"
		};
		strings.AddRange(f);

		foreach (var v in await k.SendAsync(strings)) {
			Console.WriteLine(v);
		}
	}

}