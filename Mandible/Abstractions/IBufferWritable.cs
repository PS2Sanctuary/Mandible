using System;

namespace Mandible.Abstractions;

/// <summary>
/// Represents an object that can be written to a byte buffer.
/// </summary>
public interface IBufferWritable
{
    /// <summary>
    /// Gets the required buffer size in order
    /// to write this object to a buffer.
    /// </summary>
    /// <returns>The required buffer size in bytes.</returns>
    int GetRequiredBufferSize();

    /// <summary>
    /// Writes this object to a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="Mandible.Exceptions.InvalidBufferSizeException">
    /// Thrown when the <paramref name="buffer"/> is too small.
    /// </exception>
    void Write(Span<byte> buffer);
}
