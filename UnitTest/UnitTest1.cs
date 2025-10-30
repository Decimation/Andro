global using Assert = NUnit.Framework.Legacy.ClassicAssert;
using System.Diagnostics;
using Andro.Lib;
using Andro.Lib.Daemon;
using NUnit.Framework;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace UnitTest;

[TestFixture]
public class Tests
{

	[SetUp]
	public void Setup()
	{
		Trace.Listeners.Add(new ConsoleTraceListener());
	}

	[Test]
	public async Task Test2()
	{
		Assert.That(AdbUtilities.ParseState(null), Is.EqualTo(AdbDeviceState.Unknown));

		var c = new AdbConnection();
		var d = await c.GetDevicesAsync();
		Assert.True(d.Any());
		var d1 = d.First();
		TestContext.WriteLine($"{d1}");
		var state = await c.GetHostStateAsync(d1);
		Assert.That(state, Is.EqualTo(AdbDeviceState.Device));
	}

	[Test]
	[TestCase("echo", new[] { "hi" }, "hi")]
	public async Task Test1(string cmd, string[] args, string o2)
	{
		var d  = new AdbConnection();
		var d1 = (await d.GetDevicesAsync()).First();
		TestContext.WriteLine($"{d1.Serial}");
		var o = await d.ShellAsync(cmd, args);

		// var sr = new StreamReader(r);
		// var o  = (await sr.ReadToEndAsync()).Trim().Trim('\n');
		Assert.That(o, Is.EqualTo(o2));
	}

}