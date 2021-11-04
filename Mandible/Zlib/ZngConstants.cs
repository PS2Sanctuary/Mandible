namespace Mandible.Zlib
{
    public enum FlushType
    {
        NoFlush = 0,

        /// <summary>
        /// Deflate only. Flushes all pending output to the output buffer, but doesn't align it to a byte boundary.
        /// </summary>
        PartialFlush = 1,

        /// <summary>
        /// Inflate: Flushes as much data as possible to the output buffer.
        /// Deflate: all pending output is flushed to the output buffer and aligned on a byte boundary.
        /// </summary>
        SyncFlush = 2,

        FullFlush = 3,

        /// <summary>
        /// Inflate: Indicates that the input buffer contains the entire sequence, allowing optimisations to be applied.
        /// </summary>
        Finish = 4,

        Block = 5,
        Trees = 6
    }

    public enum CompressionResult
    {
        /// <summary>
        /// The version of the underlying library was not the same as what the caller was expecting.
        /// </summary>
        VersionError = -6,

        BufferError = -5,
        MemoryError = -4,
        DataError = -3,
        StreamError = -2,
        ErrNo = -1,
        OK = 0,
        StreamEnd = 1,
        NeedsDictionary = 2
    }

    public enum CompressionLevel
    {
        Default = -1,
        None = 0,
        BestSpeed = 1,
        BestCompression = 9
    }

    public enum CompressionStrategy
    {
        Default = 0,
        Filtered = 1,
        HuffmanOnly = 2,
        RLE = 3,
        Fixed = 4
    }

    public enum DeflateDataType
    {
        Binary = 0,
        Text = 1,
        Unknown = 2
    }
}
