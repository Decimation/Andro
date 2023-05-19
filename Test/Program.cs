namespace Test;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var k = await Andro.Lib.Kde.KdeConnect.Init();
		
		Console.WriteLine(k);

		foreach (var v in await k.Send(new []{ "C:\\Users\\Deci\\Downloads\\chloe_oc_by_sciamano240_dfxf0ot.png" })) {
			Console.WriteLine(v);
		}
	}
}