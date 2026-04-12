using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Exceptions;

/// <summary>
/// Represents an exception that is thrown when the
/// version of an input file is unsupported.
/// </summary>
public class UnsupportedVersionException<T> : Exception
    where T : notnull
{
    /// <summary>
    /// Gets the expected version number.
    /// </summary>
    public T ExpectedVersion { get; }

    /// <summary>
    /// Gets the actual version number that was read.
    /// </summary>
    public T ActualVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedVersionException{T}"/>.
    /// </summary>
    /// <param name="expectedVersion">The expected version.</param>
    /// <param name="actualVersion">The actual version.</param>
    public UnsupportedVersionException(T expectedVersion, T actualVersion)
        : base($"The expected version was {expectedVersion}, but version {actualVersion} was found")
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    /// <summary>
    /// Throws an <see cref="UnsupportedVersionException{T}"/> if the given versions do not match.
    /// </summary>
    /// <param name="expected">The expected version.</param>
    /// <param name="actual">The actual version.</param>
    [StackTraceHidden]
    public static void ThrowIfMismatch(T expected, T actual)
    {
        if (!expected.Equals(actual))
            ThrowHelper(expected, actual);
    }

    /// <summary>
    /// Throws an <see cref="UnsupportedVersionException{T}"/>.
    /// </summary>
    /// <param name="expected">The expected version.</param>
    /// <param name="actual">The actual version.</param>
    /// <exception cref="UnsupportedVersionException{T}"></exception>
    [DoesNotReturn, StackTraceHidden]
    private static void ThrowHelper(T expected, T actual)
        => throw new UnsupportedVersionException<T>(expected, actual);
}
