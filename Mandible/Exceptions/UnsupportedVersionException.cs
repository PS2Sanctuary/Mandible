using System;

namespace Mandible.Exceptions;

/// <summary>
/// Represents an exception that is thrown when the
/// version of an input file is unsupported.
/// </summary>
public class UnsupportedVersionException : Exception
{
    /// <summary>
    /// Gets the expected version number.
    /// </summary>
    public uint ExpectedVersion { get; }

    /// <summary>
    /// Gets the actual version number that was read.
    /// </summary>
    public uint ActualVersion { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedVersionException"/>.
    /// </summary>
    /// <param name="expectedVersion">The expected version.</param>
    /// <param name="actualVersion">The actual version.</param>
    public UnsupportedVersionException(uint expectedVersion, uint actualVersion)
    {
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}
