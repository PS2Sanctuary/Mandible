using System;

namespace Mandible.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a
/// buffer is not the correct size for a given operation.
/// </summary>
public class InvalidBufferSizeException : Exception
{
    /// <summary>
    /// Gets the required buffer size.
    /// </summary>
    public int RequiredBufferSize { get; }

    /// <summary>
    /// Gets the actual buffer size.
    /// </summary>
    public int ActualBufferSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBufferSizeException"/> class.
    /// </summary>
    /// <param name="requiredBufferSize">The required buffer size.</param>
    /// <param name="actualBufferSize">The actual buffer size.</param>
    public InvalidBufferSizeException(int requiredBufferSize, int actualBufferSize)
    {
        RequiredBufferSize = requiredBufferSize;
        ActualBufferSize = actualBufferSize;
    }
}
