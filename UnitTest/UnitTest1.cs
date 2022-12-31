using System.Diagnostics;
using Andro.Lib.Android;

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
		Assert.That(AdbHelper.ConvertState(null!), Is.EqualTo(AdbDeviceState.Unknown));

		var c = new AdbConnection();
		var d = await c.GetDevicesAsync();
		Assert.True(d.Any());
		var d1 = d.First();
		TestContext.WriteLine($"{d1.Serial}");
		Assert.That(await d1.GetStateAsync(), Is.EqualTo(AdbDeviceState.Device));
	}

	[Test]
	[TestCase("echo", new[] { "hi" }, "hi")]
	public async Task Test1(string cmd, string[] args, string o2)
	{
		var c = new AdbConnection();
		var d = await c.GetDevicesAsync();
		Assert.True(d.Any());
		var d1 = d.First();
		TestContext.WriteLine($"{d1.Serial}");
		var r  = await d1.ShellAsync(cmd, args);
		var sr = new StreamReader(r);
		var o  = (await sr.ReadToEndAsync()).Trim().Trim('\n');
		Assert.That(o, Is.EqualTo(o2));
	}
}