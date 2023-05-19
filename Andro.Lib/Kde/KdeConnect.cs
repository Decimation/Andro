using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CliWrap;

namespace Andro.Lib.Kde;

public class KdeConnect
{
	public static readonly Command Kde = CliWrap.Cli.Wrap("kdeconnect-cli");

	public string Device { get; private set; }

	public static async Task<KdeConnect> Init(CancellationToken ct = default)
	{

		var buf  = new StringBuilder();
		var buf2 = new StringBuilder();

		var r = await Kde.WithStandardOutputPipe(PipeTarget.ToStringBuilder(buf))
			        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(buf2))
			        .WithArguments("-l")
			        .ExecuteAsync(ct);

		var d = buf.ToString().Trim().Split(' ')[2];

		return new KdeConnect()
		{
			Device = d
		};
	}

	public async Task<string[]> Send(string[] f, CancellationToken ct = default)
	{
		var cb = new ConcurrentBag<string>();

		await Parallel.ForEachAsync(f, ct, async (s, token) =>
		{
			var buf  = new StringBuilder();
			var buf2 = new StringBuilder();

			var r = await Kde.WithStandardOutputPipe(PipeTarget.ToStringBuilder(buf))
				        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(buf2))
				        .WithArguments($"-d {Device} --share \"{s}\"")
				        .ExecuteAsync(ct, token);
			cb.Add(buf.ToString());
			return;
		});

		return cb.ToArray();
	}
}