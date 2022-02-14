using System;
using System.Diagnostics;
using System.Text;
using Andro.App;
using Andro.Utilities;
using JetBrains.Annotations;
using Kantan.Utilities;
using Novus.OS;

#pragma warning disable IDE0079


#nullable disable
namespace Andro.Android;

public enum AdbCommandScope
{
	/// <summary>
	/// <see cref="AdbCommands.ADB"/>
	/// </summary>
	Adb,

	/// <summary>
	/// <see cref="AdbCommands.ADB_SHELL"/>
	/// </summary>
	AdbShell
}

public class AdbCommand : IDisposable
{
	public string Value { get; }

	public AdbCommandScope Scope { get; }

	public string BuiltCommand { get; }

	public bool IsBuilt => Process != null;

	public Process Process { get; internal set; }

	public string[] StandardOutput { get; internal set; }

	public string[] StandardError { get; internal set; }


	[SFM(AppIntegration.STRING_FORMAT_ARG)]
	public AdbCommand(string command, [CBN] string str = null, params object[] args)
		: this(AdbCommandScope.Adb, command, str, args) { }

	[SFM(AppIntegration.STRING_FORMAT_ARG)]
	public AdbCommand(AdbCommandScope scope, string command, [CBN] string str = null, params object[] args)
	{
		Value          = command;
		Scope          = scope;
		Process        = new Process() { };
		StandardError  = null;
		StandardOutput = null;

		//

		var sb = new StringBuilder();

		sb.Append(Value);

		if (str != null) {
			sb.Append(' ')
			  .AppendFormat(str, args);
		}

		var cmdStr = sb.ToString();

		//
		var strScope = scope switch
		{
			AdbCommandScope.Adb      => AdbCommands.ADB,
			AdbCommandScope.AdbShell => AdbCommands.ADB_SHELL,

			_ => throw new ArgumentOutOfRangeException(nameof(scope))
		};

		BuiltCommand = $"{strScope} {cmdStr}";
	}

	public override string ToString()
	{
		return $"{nameof(Scope)}: {Scope}, " +
		       $"{nameof(BuiltCommand)}: {BuiltCommand}, " +
		       $"{nameof(IsBuilt)}: {IsBuilt}, " +
		       $"{nameof(StandardOutput)}: {StandardOutput}, " +
		       $"{nameof(StandardError)}: {StandardError}, " +
		       $"{nameof(Success)}: {Success}";
	}

	public void Dispose()
	{
		Trace.WriteLine($"Dispose {BuiltCommand}");

		Process.WaitForExit();

		Process        = null;
		StandardError  = null;
		StandardOutput = null;
		GC.SuppressFinalize(this);
	}

	[CanBeNull]
	internal Predicate<AdbCommand> SuccessPredicate { get; set; }

	public bool? Success => SuccessPredicate?.Invoke(this);

	public void Start()
	{
		Trace.WriteLine($"Start {BuiltCommand}");

		Process.Start();

		StandardOutput = Process.StandardOutput.ReadAllLines();
		StandardError  = Process.StandardError.ReadAllLines();
	}

	[MURV]
	public AdbCommand Build(bool start = true)
	{
		Process = Command.Shell(BuiltCommand);

		if (start) {
			// Process.Start();
			Start();
		}


		return this;
	}
}