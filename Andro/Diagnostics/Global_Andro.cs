using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Andro.Diagnostics
{
	internal static class Global_Andro
	{
		public const string STRING_FORMAT_ARG = "str";

		public const string DEBUG_COND = "DEBUG";


		[StringFormatMethod(STRING_FORMAT_ARG)]
		internal static void WriteArray(string[] rg)
		{
			foreach (string s in rg) {
				Write(s);
			}
		}

		[StringFormatMethod(STRING_FORMAT_ARG)]
		[Conditional(DEBUG_COND)]
		internal static void WriteDebug(string str, params object[] args)
		{
			var str2 = string.Format(str, args);

			Debug.WriteLine($">> {str2}");
		}


		internal static void Write(object o)
		{
			Console.WriteLine($">> {o}");
		}

		[StringFormatMethod(STRING_FORMAT_ARG)]
		internal static void Write(string str, params object[] args)
		{
			var str2 = string.Format(str, args);

			Console.WriteLine($">> {str2}");
		}
	}
}