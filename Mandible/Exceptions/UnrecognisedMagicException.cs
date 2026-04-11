using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a buffer does not contain the expected magic bytes.
/// </summary>
public class UnrecognisedMagicException : Exception
{
    /// <summary>
    /// Gets the magic bytes that were expected.
    /// </summary>
    public ReadOnlyMemory<byte> ExpectedMagicBytes { get; }

    /// <summary>
    /// Gets the magic bytes that were actually read.
    /// </summary>
    public ReadOnlyMemory<byte> ActualMagicBytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnrecognisedMagicException"/> class.
    /// </summary>
    /// <param name="expectedMagicBytes">The expected magic bytes.</param>
    /// <param name="actualMagicBytes">The actual magic bytes.</param>
    public UnrecognisedMagicException(ReadOnlyMemory<byte> expectedMagicBytes, ReadOnlyMemory<byte> actualMagicBytes)
    {
        ExpectedMagicBytes = expectedMagicBytes;
        ActualMagicBytes = actualMagicBytes;
    }

    /// <summary>
    /// Throws an <see cref="UnrecognisedMagicException"/> if the given magic bytes are not present at the start of the
    /// <paramref name="buffer"/>.
    /// </summary>
    /// <param name="magicBytes">The magic bytes to detect.</param>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="UnrecognisedMagicException">
    /// Thrown if the given magic bytes are not present at the start of the <paramref name="buffer"/>.
    /// </exception>
    [StackTraceHidden]
    public static void ThrowIfNotAtStart(ReadOnlySpan<byte> magicBytes, ReadOnlySpan<byte> buffer)
    {
        if (!buffer.StartsWith(magicBytes))
            ThrowHelper(magicBytes, buffer);
    }

    [DoesNotReturn, StackTraceHidden]
    private static void ThrowHelper(ReadOnlySpan<byte> magicBytes, ReadOnlySpan<byte> buffer)
    {
        int bufLen = Math.Min(magicBytes.Length, buffer.Length);
        throw new UnrecognisedMagicException(magicBytes.ToArray(), buffer[..bufLen].ToArray());
    }
}
