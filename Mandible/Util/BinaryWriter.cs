using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mandible.Util;

/// <summary>
/// A utility for writing binary data to a <see cref="Span{T}"/>.
/// </summary>
public ref struct BinaryWriter
{
    /// <summary>
    /// Gets the underlying <see cref="Span{T}"/> of the writer.
    /// </summary>
    public readonly Span<byte> Span;

    /// <summary>
    /// Gets the number of bytes that this <see cref="BinaryWriter"/>
    /// has written to the underlying <see cref="Span"/>.
    /// </summary>
    public int Written { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the writer has
    /// reached the end of the <see cref="Span"/>.
    /// </summary>
    public readonly bool End => Written >= Span.Length;

    /// <summary>
    /// Gets the number of remaining bytes
    /// in the reader's <see cref="Span"/>.
    /// </summary>
    public readonly int Remaining => Span.Length - Written;

    /// <summary>
    /// Creates a <see cref="BinaryWriter"/> over the given <see cref="Span{T}"/>
    /// </summary>
    /// <param name="span">The <see cref="Span{T}"/> to write to.</param>
    public BinaryWriter(Span<byte> span)
    {
        Span = span;
        Written = 0;
    }

    /// <summary>
    /// Advances the writer by the given number of items.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="count"/> would move the writer past the end,
    /// it will simply move to the end of the <see cref="Span"/>.
    /// </remarks>
    /// <param name="count">The number of items to move ahead by.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="count"/> is negative.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must not be negative");

        if (count > Remaining)
            Written = Span.Length;
        else
            Written += count;
    }

    /// <summary>
    /// Writes the given bytes to the underlying span.
    /// </summary>
    /// <param name="data">The bytes to write.</param>
    public void WriteBytes(ReadOnlySpan<byte> data)
    {
        data.CopyTo(Span[Written..]);
        Written += data.Length;
    }

    /// <summary>
    /// Writes a byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    public void WriteByte(byte value)
        => Span[Written++] = value;

    /// <summary>
    /// Writes an unsigned 16-bit value in little endian.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteUInt16LE(ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(Span[Written..], value);
        Written += sizeof(ushort);
    }

    /// <summary>
    /// Writes a signed 16-bit value in little endian.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteInt16LE(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(Span[Written..], value);
        Written += sizeof(short);
    }

    /// <summary>
    /// Writes an unsigned 32-bit value in little endian.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteUInt32LE(uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(Span[Written..], value);
        Written += sizeof(uint);
    }

    /// <summary>
    /// Writes a signed 32-bit value in little endian.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteInt32LE(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(Span[Written..], value);
        Written += sizeof(int);
    }

    /// <summary>
    /// Writes a 32-bit floating point value in little endian.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteSingleLE(float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(Span[Written..], value);
        Written += sizeof(float);
    }

    /// <summary>
    /// Writes a 32-bit floating point value in big endian.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteSingleBE(float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(Span[Written..], value);
        Written += sizeof(float);
    }

    /// <summary>
    /// Writes a null-terminated string.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="encoding">The encoding to use. Defaults to <see cref="Encoding.ASCII"/>.</param>
    public void WriteStringNullTerminated(string value, Encoding? encoding = null)
    {
        WriteString(value, encoding);
        Span[Written++] = 0;
    }

    /// <summary>
    /// Writes a string.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="encoding">The encoding to use. Defaults to <see cref="Encoding.ASCII"/>.</param>
    public void WriteString(string value, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        int byteCount = encoding.GetByteCount(value);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);

        encoding.GetBytes(value, buffer);
        buffer[..byteCount].CopyTo(Span[Written..]);

        Written += byteCount;
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
