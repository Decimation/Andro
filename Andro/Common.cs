using System;
using System.Collections.Generic;
using System.Text;

namespace Andro
{
	internal static class Common
	{
		public static StringBuilder AppendCond<T>(this StringBuilder sb, T[] rg, string sx)
		{
			if (rg?.Length > 0)
			{
				sb.Append($"{sx}\n");

				foreach (T s in rg)
				{
					sb.AppendFormat(">> {0}", s.ToString());
				}
			}

			return sb;
		}
	}
}
