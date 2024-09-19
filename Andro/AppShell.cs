// Author: Deci | Project: Andro | Name: AppShell.cs
// Date: 2024/09/19 @ 14:09:02

using Spectre.Console;

namespace Andro;

public static class AppShell
{

	internal static Style GetStyleForNullable(bool? res)
	{
		return res.HasValue ? (res.Value ? Clr_Success : Clr_Error) : Clr_Unknown;
	}

	internal static         Style Clr_Success = new Style(Color.SeaGreen1, decoration: Decoration.None);

	internal static         Style Clr_Unknown = new Style(Color.Yellow3, decoration: Decoration.None);

	internal static         Style Clr_Error   = new Style(Color.Red, decoration: Decoration.None);

}