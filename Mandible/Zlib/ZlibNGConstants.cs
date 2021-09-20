namespace Mandible.Zlib
{
    public enum ZlibFlushType
    {
        NoFlush = 0,
        PartialFlush = 1,
        SyncFlush = 2,
        FullFlush = 3
    }

    public enum CompressionResult
    {
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

    internal static class ZlibNGConstants
    {
        public const int Z_OK = 0;
        public const int Z_STREAM_END = 1;
    }
}
