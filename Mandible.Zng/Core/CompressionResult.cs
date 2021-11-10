namespace Mandible.Zng.Core;

/// <summary>
/// Enumerates the possible results of a compression operation.
/// </summary>
public enum CompressionResult
{
    /// <summary>
    /// The version of the underlying library is not the same as what the caller is expecting.
    /// </summary>
    VersionError = -6,

    /// <summary>
    /// No progress is possible or there is not enough room in the
    /// output buffer when <c>..FlushMethod.Finish</c> is used.
    /// This error is not fatal, and inflation can continue with more input and/or more output space.
    /// </summary>
    BufferError = -5,

    /// <summary>
    /// Not enough memory to complete the operation.
    /// </summary>
    MemoryError = -4,

    /// <summary>
    /// Inflate: The input stream is corrupt (doesn't conform to the zlib format,
    /// or incorrect check value, in which case see <see cref="ZngStream.ErrorMessage"/>).
    /// </summary>
    DataError = -3,

    /// <summary>
    /// The <see cref="ZngStream"/> structure is inconsistent (e.g. <see cref="ZngStream.NextIn"/> or <see cref="ZngStream.NextIn"/> are null).
    /// </summary>
    StreamError = -2,

    ErrNo = -1,

    /// <summary>
    /// Deflate/Inflate: Progress has been made. Supply more input, or more output space.
    /// Otherwise: The operation completed successfully.
    /// </summary>
    OK = 0,

    /// <summary>
    /// All input has been consumed and all output produced successfully.
    /// </summary>
    StreamEnd = 1,

    /// <summary>
    /// Inflate: A preset dictionary is needed at this point.
    /// </summary>
    NeedsDictionary = 2
}
