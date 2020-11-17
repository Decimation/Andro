using System.Text;

namespace Andro.Core
{
	internal static class Common
	{
		public static StringBuilder AppendRangeSafe<T>(this StringBuilder sb, T[] rg, string sx)
		{
			if (rg?.Length > 0)
			{
				sb.Append($"\n{sx}\n");

				foreach (T s in rg)
				{
					sb.AppendFormat(">> {0}", s.ToString());
				}
			}

			return sb;
		}
	}
}
