// Author: Deci | Project: Andro | Name: AndroPipeData.cs
// Date: 2024/09/19 @ 17:09:16

using System.Text.Json;
using System.Text.Json.Serialization;
using Novus.Utilities;
using Novus.Win32;

namespace Andro.Comm;

public class AndroPipeData
{

	public string[] Data { get; }

	public int Pid { get; }

	[JsonConstructor]
	public AndroPipeData(string[] data, int pid)
	{
		Data = data;
		Pid  = pid;
	}

	#region 

	internal static readonly AndroPipeData SendToData = new([R2.Arg_PushAll], Native.ERROR_SV);

	internal static readonly string SendToDataSerialized = JsonSerializer.Serialize(SendToData);

	#endregion

	public static AndroPipeData FromArgs(string[] data)
	{
		var parent = ProcessHelper.GetParent();
		int pid = parent?.Id ?? Native.ERROR_SV;

		return new AndroPipeData(data, pid);
	}

	public override string ToString()
	{
		return $"{Data.Length} from {Pid}";
	}

	public const char MSG_DELIM = '\0';

}