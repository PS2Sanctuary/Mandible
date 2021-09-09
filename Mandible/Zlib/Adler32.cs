﻿namespace Mandible.Zlib
{
    /// <summary>
	/// <para>
    /// Computes Adler32 checksum for a stream of data. An Adler32
    /// checksum is not as reliable as a CRC32 checksum, but a lot faster to
    /// compute.
	/// </para>
    ///
	/// <para>
    /// The specification for Adler32 may be found in RFC 1950 - ZLIB Compressed Data Format Specification version 3.3.
    /// From that document:
    ///
	/// <para>
	/// "ADLER32 (Adler-32 checksum)
	/// This contains a checksum value of the uncompressed data
	/// (excluding any dictionary data) computed according to Adler-32
	/// algorithm. This algorithm is a 32-bit extension and improvement
	/// of the Fletcher algorithm, used in the ITU-T X.224 / ISO 8073
	/// standard.
	/// </para>
	///
	/// <para>
	/// "Adler-32 is composed of two sums accumulated per byte: s1 is
	/// the sum of all bytes, s2 is the sum of all s1 values. Both sums
	/// are done modulo 65521. s1 is initialized to 1, s2 to zero.  The
    /// Adler-32 checksum is stored as s2*65536 + s1 in most-
    /// significant-byte first (network) order."
	/// </para>
    ///
    ///  "8.2. The Adler-32 algorithm
    ///
    ///    The Adler-32 algorithm is much faster than the CRC32 algorithm yet
    ///    still provides an extremely low probability of undetected errors.
    ///
    ///    The modulo on unsigned long accumulators can be delayed for 5552
    ///    bytes, so the modulo operation time is negligible.  If the bytes
    ///    are a, b, c, the second sum is 3a + 2b + c + 3, and so is position
    ///    and order sensitive, unlike the first sum, which is just a
    ///    checksum.  That 65521 is prime is important to avoid a possible
    ///    large class of two-byte errors that leave the check unchanged.
    ///    (The Fletcher checksum uses 255, which is not prime and which also
    ///    makes the Fletcher check insensitive to single byte changes 0 -
    ///    255.)
    ///
    ///    The sum s1 is initialized to 1 instead of zero to make the length
    ///    of the sequence part of s2, so that the length does not have to be
    ///    checked separately. (Any sequence of zeroes has a Fletcher
    ///    checksum of zero.)"
	/// </para>
    /// </summary>
    /// <see cref="ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream"/>
    /// <see cref="ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream"/>
    public sealed class Adler32
	{
		#region Instance Fields

		/// <summary>
		/// largest prime smaller than 65536
		/// </summary>
		private const uint BASE = 65521;

		/// <summary>
		/// The CRC data checksum so far.
		/// </summary>
		private uint _checkValue;

		#endregion Instance Fields

		/// <summary>
		/// Initialise a default instance of <see cref="Adler32"></see>
		/// </summary>
		public Adler32()
		{
			Reset();
		}

		/// <summary>
		/// Resets the Adler32 data checksum as if no update was ever called.
		/// </summary>
		public void Reset()
		{
			_checkValue = 1;
		}

		/// <summary>
		/// Returns the Adler32 data checksum computed so far.
		/// </summary>
		public long Value => _checkValue;

		/// <summary>
		/// Updates the checksum with the byte b.
		/// </summary>
		/// <param name="bval">
		/// The data value to add. The high byte of the int is ignored.
		/// </param>
		public void Update(int bval)
		{
			// We could make a length 1 byte array and call update again, but I
			// would rather not have that overhead
			uint s1 = _checkValue & 0xFFFF;
			uint s2 = _checkValue >> 16;

			s1 = (s1 + ((uint)bval & 0xFF)) % BASE;
			s2 = (s1 + s2) % BASE;

			_checkValue = (s2 << 16) + s1;
		}

		public void Update(ReadOnlySpan<byte> segment)
        {
			//(By Per Bothner)
			uint s1 = _checkValue & 0xFFFF;
			uint s2 = _checkValue >> 16;
			var count = segment.Length;
			var offset = 0;

			while (count > 0)
			{
				// We can defer the modulo operation:
				// s1 maximally grows from 65521 to 65521 + 255 * 3800
				// s2 maximally grows by 3800 * median(s1) = 2090079800 < 2^31
				int n = 3800;
				if (n > count)
					n = count;

				count -= n;
				while (--n >= 0)
				{
					s1 += (uint)(segment[offset++] & 0xff);
					s2 += + s1;
				}
				s1 %= BASE;
				s2 %= BASE;
			}

			_checkValue = (s2 << 16) | s1;
		}
	}
}