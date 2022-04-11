using System;
using System.Collections.ObjectModel;

namespace Mandible.Exceptions;

/// <summary>
/// Represents an exception that is thrown when the
/// magic bytes of an input file are not recognised.
/// </summary>
public class UnrecognisedMagicException : Exception
{
    /// <summary>
    /// Gets the magic bytes that were expected.
    /// </summary>
    public ReadOnlyCollection<byte> ExpectedMagicBytes { get; }

    /// <summary>
    /// Gets the magic bytes that were actually read.
    /// </summary>
    public ReadOnlyCollection<byte> ActualMagicBytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnrecognisedMagicException"/> class.
    /// </summary>
    /// <param name="expectedMagicBytes">The expected magic bytes.</param>
    /// <param name="actualMagicBytes">The actual magic bytes.</param>
    public UnrecognisedMagicException(byte[] expectedMagicBytes, byte[] actualMagicBytes)
    {
        ExpectedMagicBytes = Array.AsReadOnly(expectedMagicBytes);
        ActualMagicBytes = Array.AsReadOnly(actualMagicBytes);
    }
}
