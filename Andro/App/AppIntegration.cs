global using R1 = Andro.Lib.Properties.Resources;
global using R2 = Andro.Properties.Resources;
using Microsoft.Extensions.Logging;
using Novus.OS;

#pragma warning disable CA1416
namespace Andro.App;

public static class AppIntegration
{

	internal static readonly ILoggerFactory LoggerFactoryInt;

	static AppIntegration()
	{
		ExeLocation = FileSystem.FindLocation(R1.Name_Exe);

		LoggerFactoryInt = LoggerFactory.Create(builder =>
		{
			builder.AddDebug();
			builder.AddTraceSource(TRACE_COND);
			builder.AddConsole();
			builder.SetMinimumLevel(LogLevel.Trace);
		});
	}

	/*
	 * HKEY_CLASSES_ROOT is an alias, a merging, of two other locations:
	 *		HKEY_CURRENT_USER\Software\Classes
	 *		HKEY_LOCAL_MACHINE\Software\Classes
	 */

	public static readonly string ExeLocation;

	internal const string STRING_FORMAT_ARG = "str";

	internal const string DEBUG_COND = "DEBUG";

	internal const string TRACE_COND = "TRACE";

	internal const string OS_WIN = "windows";

}