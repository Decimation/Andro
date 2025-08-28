// Author: Deci | Project: Andro | Name: AppInterface.cs
// Date: 2024/09/19 @ 14:09:02

using Spectre.Console;

// ReSharper disable InconsistentNaming

namespace Andro.App;

internal static class AppInterface
{

	internal static Style GetStyleForNullable(bool? res)
		=> GetStyleForNullable(res, static b => b);

	internal static Style GetStyleForNullable<T>(T? res, Predicate<T> pred) where T : struct
	{
		return res.HasValue ? pred(res.Value) ? Sty_Success : Sty_Error : Sty_Unknown;
	}


	internal static readonly Style Sty_Success = new(Color.SeaGreen1, decoration: Decoration.None);

	internal static readonly Style Sty_Unknown = new(Color.Yellow3, decoration: Decoration.None);

	internal static readonly Style Sty_Error = new(Color.Red, decoration: Decoration.None);

	internal static readonly FigletText Fig_Name = new(R1.Name);

}