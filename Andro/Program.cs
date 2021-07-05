using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Andro.Android;
using Andro.Core;
using Andro.Diagnostics;
using JetBrains.Annotations;
using Novus;
using SimpleCore.Cli;
using SimpleCore.Utilities;

// ReSharper disable IdentifierTypo

// ReSharper disable StringLiteralTypo
#pragma warning disable IDE0060

#nullable enable
namespace Andro
{
	/*
	 *
	 */
	public static class Program
	{
		public static void Main(string[] args)
		{
#if DEBUG
			if (!args.Any()) {
				args = new[] {"ctx", "rm"};
			}
#endif
			/*
			 * Setup
			 */

			Console.Title = Info.NAME;
			NConsole.Init();
			NConsole.Write(Info.NAME);

			/*
			 *
			 */

			var data = ReadFromArguments(args);

			Console.WriteLine(">> {0}", data);
			

			NConsole.WaitForInput();
		}

		private static object? ReadFromArguments(string[] args)
		{
			//var args = Environment.GetCommandLineArgs()
			//                      .Skip(1)
			//                      .ToArray();

			//args = args.Skip(1).ToArray();

			Debug.WriteLine(args.QuickJoin(StringConstants.SPACE.ToString()));


			if (!args.Any()) {
				return null;
			}

			var       argQueue      = new Queue<string>(args);
			using var argEnumerator = argQueue.GetEnumerator();

			var d = new Device();

			Console.WriteLine(d);

			while (argEnumerator.MoveNext()) {
				string argValue = argEnumerator.Current;

				// todo: structure

				switch (argValue) {
					case "push":
						argEnumerator.MoveNext();
						var f = argEnumerator.Current;

						argEnumerator.MoveNext();
						var df = argEnumerator.Current;

						argEnumerator.MoveNext();

						d.Push(f, df);

						break;
					case "fsize":
						argEnumerator.MoveNext();
						var file = argEnumerator.Current;

						argEnumerator.MoveNext();

						return d.GetFileSize(file);

					case "ctx":
						argEnumerator.MoveNext();
						var op = argEnumerator.Current;

						if (op == "add") {
							Util.Add();
						}

						if (op == "rm") {
							Util.Remove();

						}

						argEnumerator.MoveNext();
						break;
					case "pushall":
						argEnumerator.MoveNext();
						var dir = argEnumerator.Current;

						argEnumerator.MoveNext();
						var rdir = argEnumerator.Current;

						argEnumerator.MoveNext();

						d.PushAll(dir, rdir);

						break;
					default:
						break;
				}
			}

			return null;
		}
	}
}