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
		return res.HasValue ? pred(res.Value) ? Clr_Success : Clr_Error : Clr_Unknown;
	}


	internal static readonly Style Clr_Success = new(Color.SeaGreen1, decoration: Decoration.None);

	internal static readonly Style Clr_Unknown = new(Color.Yellow3, decoration: Decoration.None);

	internal static readonly Style Clr_Error = new(Color.Red, decoration: Decoration.None);

	internal static readonly FigletText _nameFiglet = new(R1.Name);

}