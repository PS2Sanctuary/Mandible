namespace Mandible.Zng.Deflate;

/// <summary>
/// Enumerates the possible flush methods that can be used with a deflate operation.
/// </summary>
public enum DeflateFlushMethod
{
    /// <summary>
    /// Allows the algorithm to decide how much data to accumulate before producing output.
    /// This maximizes the compression level that is achieved.
    /// </summary>
    NoFlush = 0,

    /// <summary>
    /// Flushes all pending output to the output buffer, but doesn't align it to a byte boundary.
    /// </summary>
    PartialFlush = 1,

    /// <summary>
    /// All pending output is flushed to the output buffer and aligned on a byte boundary.
    /// </summary>
    SyncFlush = 2,

    /// <summary>
    /// Indicates that the input buffer contains the entire sequence to be deflated, allowing optimisations to be applied.
    /// </summary>
    Finish = 4,

    Block = 5,
    FullFlush = 3,
}
