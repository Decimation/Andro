﻿using Andro.Kde;
using Andro.Adb.Android;
using Novus.Win32;

namespace Test;

public static class Program
{
	public static async Task Main(string[] args)
	{
		await Test1();
	}

	private static async Task Test2()
	{
		var d  = new AdbConnection();
		var t2 = await d.GetDevicesAsync();
		var dd = t2[0];

		var l = await dd.list("sdcard/Download");

		foreach (string s in l) {
			Console.WriteLine(s);
		}
	}

	private static async Task Test1()
	{
		var k = await KdeConnect.InitAsync();

		Console.WriteLine(k);
		Clipboard.Open();

		var f= Clipboard.GetDragQueryList();

		var strings = new List<string>()
		{
			@"C:\Users\Deci\Pictures\Epic anime\3e91c02d7dba20f9aeda7b4c7823b632.png"
		};
		strings.AddRange(f);

		foreach (var v in await k.SendAsync(strings)) {
			Console.WriteLine(v);
		}
	}
}