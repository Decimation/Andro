using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andro.Android;

public static class AdbHelper
{
	public static string GetPayload(string s, out byte[] rg, out byte[] rg2)
	{
		rg = Encoding.UTF8.GetBytes(s);
		var cm = $"{rg.Length:x4}{s}";
		rg2 = Encoding.UTF8.GetBytes(cm);
		return cm;
	}

	public static string Escape(string e)
	{
		return e.Replace(" ", "' '");
	}
}