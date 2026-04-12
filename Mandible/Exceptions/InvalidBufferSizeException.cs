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
    public int RequiredSize { get; }

    /// <summary>
    /// Gets the actual buffer size.
    /// </summary>
    public int ActualSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidBufferSizeException"/> class.
    /// </summary>
    /// <param name="requiredSize">The required buffer size.</param>
    /// <param name="actualSize">The actual buffer size.</param>
    /// <param name="message">An message describing the exception circumstances.</param>
    public InvalidBufferSizeException(int requiredSize, int actualSize, string? message = null)
        : base(message ?? $"The buffer length was expected to be {requiredSize}, but instead it was {actualSize}")
    {
        RequiredSize = requiredSize;
        ActualSize = actualSize;
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
        if (actualSize >= minimumSize)
            return;

        ThrowHelper
        (
            minimumSize,
            actualSize,
            $"The buffer length is {actualSize}, but is must be at least {minimumSize}"
        );
    }

    [DoesNotReturn, StackTraceHidden]
    private static void ThrowHelper(int requiredSize, int actualSize, string message)
        => throw new InvalidBufferSizeException(requiredSize, actualSize, message);
}
