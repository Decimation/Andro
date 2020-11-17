using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace Andro
{
	internal static class Global
	{
		private const string STRING_FORMAT_ARG = "msg";

		private const string DEBUG_COND = "DEBUG";


		[StringFormatMethod(STRING_FORMAT_ARG)]
		internal static void WriteArray(string[] rg)
		{
			foreach (string s in rg) {
				Write(s);
			}
		}

		[StringFormatMethod(STRING_FORMAT_ARG)]
		[Conditional(DEBUG_COND)]
		internal static void WriteDebug(string msg, params object[] args)
		{
			var str = string.Format(msg, args);

			Debug.WriteLine($">> {str}");
		}


		internal static void Write(object o)
		{

			Console.WriteLine($">> {o}");
		}

		[StringFormatMethod(STRING_FORMAT_ARG)]
		internal static void Write(string msg, params object[] args)
		{
			var str = string.Format(msg, args);

			Console.WriteLine($">> {str}");
		}
	}
}