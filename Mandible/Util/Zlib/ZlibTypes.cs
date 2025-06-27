namespace Mandible.Util.Zlib;

// ReSharper disable EnumUnderlyingTypeIsInt

public static class ZlibConstants
{
    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p>ZLib's <code>windowBits</code> parameter is the base two logarithm of the window size (the size of the
    /// history buffer). It should be in the range 8..15 for this version of the library. Larger values of this
    /// parameter result in better compression at the expense of memory usage. The default value is 15 if deflateInit is
    /// used instead.<br /></p>
    /// <strong>Note</strong>:
    /// <code>windowBits</code> can also be -8..-15 for raw deflate (i.e. no zlib header is written). In this case,
    /// -windowBits determines the window size. <code>Deflate</code> will then generate raw deflate data with no ZLib
    /// header or trailer, and will not compute an adler32 check value.<br />
    /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>).</p>
    /// </summary>
    public const int Deflate_DefaultWindowBits = -15;

    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p>ZLib's <code>windowBits</code> parameter is the base two logarithm of the window size (the size of the
    /// history buffer). It should be in the range 8..15 for this version of the library. Larger values of this
    /// parameter result in better compression at the expense of memory usage. The default value is 15 if deflateInit is
    /// used instead.<br /></p>
    /// </summary>
    public const int ZLib_DefaultWindowBits = 15;

    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p>The <code>memLevel</code> parameter specifies how much memory should be allocated for the internal
    /// compression state. <code>memLevel</code> = 1 uses minimum memory but is slow and reduces compression ratio;
    /// <code>memLevel</code> = 9 uses maximum memory for optimal speed. The default value is 8.</p>
    /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.)</p>
    /// </summary>
    public const int Deflate_DefaultMemLevel = 8;

    public const int Deflate_NoCompressionMemLevel = 7;
}

public enum ZlibFlushCode : int
{
    NoFlush = 0,
    SyncFlush = 2,
    Finish = 4,
    Block = 5
}

public enum ZlibErrorCode : int
{
    Ok = 0,
    StreamEnd = 1,
    StreamError = -2,
    DataError = -3,
    MemError = -4,
    BufError = -5,
    VersionError = -6
}

/// <summary>
/// <p><strong>From the ZLib manual:</strong></p>
/// <p><code>CompressionStrategy</code> is used to tune the compression algorithm.<br />
/// Use the value <code>DefaultStrategy</code> for normal data, <code>Filtered</code> for data produced by a filter
/// (or predictor), <code>HuffmanOnly</code> to force Huffman encoding only (no string match), or <code>Rle</code> to
/// limit match distances to one (run-length encoding). Filtered data consists mostly of small values with a somewhat
/// random distribution. In this case, the compression algorithm is tuned to compress them better. The effect of
/// <code>Filtered</code> is to force more Huffman coding and less string matching; it is somewhat intermediate between
/// <code>DefaultStrategy</code> and <code>HuffmanOnly</code>. <code>Rle</code> is designed to be almost as fast as
/// <code>HuffmanOnly</code>, but give better compression for PNG image data. The strategy parameter only affects the
/// compression ratio but not the correctness of the compressed output even if it is not set appropriately.
/// <code>Fixed</code> prevents the use of dynamic Huffman codes, allowing for a simpler decoder for special applications.</p>
///
/// <p>We have investigated compression scenarios for a bunch of different frequently occurring compression data and
/// found that in all cases we investigated so far, <code>DefaultStrategy</code> provided best results</p>.
/// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>).</p>
/// </summary>
public enum ZlibCompressionStrategy : int
{
    DefaultStrategy = 0,
    Filtered = 1,
    HuffmanOnly = 2,
    RunLengthEncoding = 3,
    Fixed = 4
}

/// <summary>
/// In version 1.2.3, ZLib provides on the <code>Deflated</code>-<code>CompressionMethod</code>.
/// </summary>
public enum ZlibCompressionMethod : int
{
    Deflated = 8
}

/// <summary>
/// ZLib can accept any integer value between 0 and 9 (inclusive) as a valid compression level parameter:
/// 1 gives best speed, 9 gives best compression, 0 gives no compression at all (the input data is simply copied a block
/// at a time). <see cref="ZlibCompressionLevel.DefaultCompression" /> = -1 requests a default compromise between speed
/// and compression (currently equivalent to level 6).
/// </summary>
/// <remarks>
/// <para><strong>How to choose a compression level:</strong><br />
/// The names <see cref="NoCompression" />, <see cref="BestSpeed" />, <see cref="DefaultCompression" />,
/// <see cref="BestCompression" /> are taken over from the corresponding ZLib definitions, which map to our public
/// NoCompression, Fastest, Optimal, and SmallestSize respectively.</para>
/// <em>Optimal Compression:</em>
/// <code>
/// ZlibCompressionLevel compressionLevel = ZLibNative.CompressionLevel.DefaultCompression;
/// int windowBits = 15;  // or -15 if no headers required
/// int memLevel = 8;
/// ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;
/// </code>
///
/// <em>Fastest compression:</em>
/// <code>
/// ZlibCompressionLevel compressionLevel = ZLibNative.CompressionLevel.BestSpeed;
/// int windowBits = 15;  // or -15 if no headers required
/// int memLevel = 8;
/// ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;
/// </code>
///
/// <em>No compression (even faster, useful for data that cannot be compressed such some image formats):</em>
/// <code>
/// ZlibCompressionLevel compressionLevel = ZLibNative.CompressionLevel.NoCompression;
/// int windowBits = 15;  // or -15 if no headers required
/// int memLevel = 7;
/// ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;
/// </code>
///
/// <em>Smallest Size Compression:</em>
/// <code>
/// ZlibCompressionLevel compressionLevel = ZLibNative.CompressionLevel.BestCompression;
/// int windowBits = 15;  // or -15 if no headers required
/// int memLevel = 8;
/// ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;
/// </code>
/// </remarks>
public enum ZlibCompressionLevel : int
{
    NoCompression = 0,
    BestSpeed = 1,
    DefaultCompression = -1,
    BestCompression = 9
}
