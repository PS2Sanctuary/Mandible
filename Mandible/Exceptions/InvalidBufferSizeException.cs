using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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

    /// <summary>
    /// Throws a <see cref="InvalidBufferSizeException"/> if the <see cref="actualSize"/> is less than the
    /// <see cref="minimumSize"/>.
    /// </summary>
    /// <param name="minimumSize">The minimum length that the buffer must be.</param>
    /// <param name="actualSize">The actual length of the buffer.</param>
    [StackTraceHidden]
    public static void ThrowIfLessThan(int minimumSize, int actualSize)
    {
        if (actualSize < minimumSize)
            ThrowHelper(minimumSize, actualSize);
    }

    [DoesNotReturn, StackTraceHidden]
    public static void ThrowHelper(int requiredSize, int actualSize)
        => throw new InvalidBufferSizeException(requiredSize, actualSize);
}
