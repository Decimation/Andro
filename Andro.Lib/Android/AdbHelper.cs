using System.Text;

namespace Andro.Lib.Android;

public static class AdbHelper
{
	public static readonly Encoding Encoding = Encoding.UTF8;

	public static string GetPayload(string s, out byte[] rg, out byte[] rg2)
	{
		rg = Encoding.GetBytes(s);
		var cm = $"{rg.Length:x4}{s}";
		rg2 = Encoding.GetBytes(cm);
		return cm;
	}

	public static string Escape(string e)
	{
		return e.Replace(" ", "' '");
	}
}