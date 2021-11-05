namespace Mandible.Zng.Deflate
{
    /// <summary>
    /// Enumerates the levels of compression that can be employed.
    /// </summary>
    public enum CompressionLevel
    {
        /// <summary>
        /// A default compromise between speed and compression.
        /// </summary>
        Default = -1,

        /// <summary>
        /// No compression is used.
        /// </summary>
        None = 0,

        BestSpeed = 1,
        BestCompression = 9
    }
}
