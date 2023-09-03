using System.Collections.Concurrent;
using System.Text;
using CliWrap;

namespace Andro.Kde;

public class KdeConnect
{
	public static readonly Command Kde = CliWrap.Cli.Wrap("kdeconnect-cli");

	public string Device { get; private set; }

	public static async Task<KdeConnect> InitAsync(CancellationToken ct = default)
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

	public async Task<string[]> SendAsync(IEnumerable<string> f, IProgress<string>? p = null, CancellationToken ct = default)
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
			var v = buf.ToString();
			cb.Add(v);
			p?.Report(v);
			return;
		});

		return cb.ToArray();
	}
}