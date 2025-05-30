using Novus.Streams;

namespace Andro.Adb.Android;

/*public class AdbFilterInputStream : FilterInputStream
{
	//todo

	public AdbFilterInputStream(InputStream s) : base(s) { }

	public new Stream BaseStream => base.BaseStream;

	public override int Read()
	{
		var b1 = base.Read();

		if (b1 == 0x0D)
		{
			base.Mark(1);
			var b2 = base.Read();

			if (b2 == 0x0A)
			{
				return b2;
			}

			base.Reset();
		}

		return b1;
	}

	public override int Read(byte[] buffer) => Read(buffer, 0, buffer.Length);

	public override int Read(byte[] buffer, int offset, int length)
	{
		int n = 0;

		for (int i = 0; i < length; i++)
		{
			int b = Read();
			if (b == -1) return n == 0 ? -1 : n;
			buffer[offset + n] = (byte)b;
			n++;

			// Return as soon as no more data is available (and at least one byte was read)
			if (Available() <= 0)
			{
				//todo?
				return n;
			}
		}

		return n;
	}
}*/