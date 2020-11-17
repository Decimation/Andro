using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Novus.Win32;

namespace Andro
{
	public static class Operations
	{
		public static Process RunCommand(string cmd)
		{
			var proc = Command.Shell($"{Device.ADB} {cmd}");

			return proc;
		}

		
	}
}
