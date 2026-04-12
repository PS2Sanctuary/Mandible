using System;

namespace Mandible.Abstractions;

/// <summary>
/// Represents an object that can be written to a byte buffer.
/// </summary>
/// <typeparam name="T">The type of the object.</typeparam>
public interface IBufferSerializable<out T>
{
    /// <summary>
    /// Reads an instance of this object from the given buffer.
    /// </summary>
    /// <param name="buffer">The buffer to read the object from.</param>
    /// <param name="amountRead">The number of bytes consumed from the buffer.</param>
    /// <returns>The read object.</returns>
    static abstract T Read(ReadOnlySpan<byte> buffer, out int amountRead);

    /// <summary>
    /// Gets the size in bytes of this object when written to a buffer.
    /// </summary>
    /// <returns>The size in bytes of this object when stored in a buffer.</returns>
    int GetRequiredBufferSize();

    /// <summary>
    /// Writes this object to a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>The amount of data written to the buffer.</returns>
    int Write(Span<byte> buffer);
}
