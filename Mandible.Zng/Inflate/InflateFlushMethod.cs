namespace Mandible.Zng.Inflate
{
    /// <summary>
    /// Enumerates the possible flush methods that can be used with an inflate operation.
    /// </summary>
    public enum InflateFlushMethod
    {
        NoFlush = 0,

        /// <summary>
        /// Flushes as much data as possible to the output buffer.
        /// </summary>
        SyncFlush = 2,

        /// <summary>
        /// Indicates that the input buffer contains the entire sequence, allowing optimisations to be applied.
        /// Data will be inflated in one operation, so ensure the output buffer is large enough when using this method.
        /// </summary>
        Finish = 4,

        /// <summary>
        /// Requests that the inflate operation stops if and when it reaches the next deflate block boundary.
        /// Note that when decoding the zlib format this method will cause the inflate operation to stop immediately after the header.
        /// </summary>
        Block = 5,

        /// <summary>
        /// Behaves like <see cref="InflateFlushMethod.Block"/>, but also returns when the end of each deflate
        /// block header is reach, before any actual data in that block is decoded.
        /// </summary>
        Trees = 6
    }
}
