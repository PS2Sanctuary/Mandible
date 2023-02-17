using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mandible.Util;

/// <summary>
/// A utility for reading binary data from a <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public ref struct BinaryReader
{
    /// <summary>
    /// Gets the underlying <see cref="ReadOnlySpan{T}"/> for the reader.
    /// </summary>
    public readonly ReadOnlySpan<byte> Span;

    /// <summary>
    /// Gets the number of bytes that this <see cref="BinaryReader"/>
    /// has consumed from the underlying <see cref="Span"/>.
    /// </summary>
    public int Consumed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the reader has
    /// reached the end of the <see cref="Span"/>.
    /// </summary>
    public readonly bool End => Consumed >= Span.Length;

    /// <summary>
    /// Gets the number of remaining bytes
    /// in the reader's <see cref="Span"/>.
    /// </summary>
    public readonly int Remaining => Span.Length - Consumed;

    /// <summary>
    /// Creates a <see cref="BinaryReader"/> over the given <see cref="ReadOnlySpan{T}"/>
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to read.</param>
    public BinaryReader(ReadOnlySpan<byte> span)
    {
        Span = span;
        Consumed = 0;
    }

    /// <summary>
    /// Forms a slice out of the current <see cref="BinaryReader"/>,
    /// beginning at the current <see cref="Consumed"/>.
    /// </summary>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>
    /// A <see cref="BinaryReader"/> backed by a slice of the current <see cref="Span"/>.
    /// </returns>
    public BinaryReader Slice(int length)
        => new(Span.Slice(Consumed, length));

    /// <summary>
    /// Forms a slice out of the current <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="start">The index to begin the slice at.</param>
    /// <param name="length">The desired length of the slice.</param>
    /// <returns>
    /// A <see cref="BinaryReader"/> backed by a slice of the current <see cref="Span"/>.
    /// </returns>
    public BinaryReader Slice(int start, int length)
        => new(Span.Slice(start, length));

    /// <summary>
    /// Advances the reader by the given number of items.
    /// </summary>
    /// <remarks>
    /// If the <paramref name="count"/> would move the reader past the end,
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
            Consumed = Span.Length;
        else
            Consumed += count;
    }

    /// <summary>
    /// Reads a span of bytes from the current position of the reader.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> over the length of bytes to be read.</returns>
    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        ReadOnlySpan<byte> value = Span.Slice(Consumed, length);
        Consumed += length;
        return value;
    }

    /// <summary>
    /// Reads a byte value.
    /// </summary>
    /// <returns>The value.</returns>
    public byte ReadByte()
        => Span[Consumed++];

    /// <summary>
    /// Reads an unsigned 16-bit integer in little endian.
    /// </summary>
    /// <returns>The value.</returns>
    public ushort ReadUInt16LE()
    {
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(Span[Consumed..]);
        Consumed += sizeof(ushort);
        return value;
    }

    /// <summary>
    /// Reads a signed 16-bit integer in little endian.
    /// </summary>
    /// <returns>The value.</returns>
    public short ReadInt16LE()
    {
        short value = BinaryPrimitives.ReadInt16LittleEndian(Span[Consumed..]);
        Consumed += sizeof(short);
        return value;
    }

    /// <summary>
    /// Reads an unsigned 32-bit integer in little endian.
    /// </summary>
    /// <returns>The value.</returns>
    public uint ReadUInt32LE()
    {
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(Span[Consumed..]);
        Consumed += sizeof(uint);
        return value;
    }

    /// <summary>
    /// Reads a signed 32-bit integer in little endian.
    /// </summary>
    /// <returns>The value.</returns>
    public int ReadInt32LE()
    {
        int value = BinaryPrimitives.ReadInt32LittleEndian(Span[Consumed..]);
        Consumed += sizeof(int);
        return value;
    }

    /// <summary>
    /// Reads a 32-bit floating point value in little endian.
    /// </summary>
    /// <returns>The value.</returns>
    public float ReadSingleLE()
    {
        float value = BinaryPrimitives.ReadSingleLittleEndian(Span[Consumed..]);
        Consumed += sizeof(float);
        return value;
    }

    /// <summary>
    /// Reads a 32-bit floating point value in big endian.
    /// </summary>
    /// <returns>The value.</returns>
    public float ReadSingleBE()
    {
        float value = BinaryPrimitives.ReadSingleBigEndian(Span[Consumed..]);
        Consumed += sizeof(float);
        return value;
    }

    /// <summary>
    /// Reads a null-terminated string.
    /// </summary>
    /// <param name="encoding">The encoding of the string. Defaults to <see cref="Encoding.ASCII"/></param>
    /// <returns>The value.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a null terminator does not exist in the underlying span.
    /// </exception>
    public string ReadStringNullTerminated(Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        int terminatorIndex = Span[Consumed..].IndexOf((byte)0);
        if (terminatorIndex == -1)
            throw new InvalidOperationException("Null-terminator not found");

        string value = encoding.GetString(Span.Slice(Consumed, terminatorIndex));
        Consumed += terminatorIndex + 1;

        return value;
    }

    /// <summary>
    /// Reads a string.
    /// </summary>
    /// <param name="length">The number of bytes consumed by the string.</param>
    /// <param name="encoding">The encoding of the string. Defaults to <see cref="Encoding.ASCII"/></param>
    /// <returns>The value.</returns>
    public string ReadString(int length, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        string value = encoding.GetString(Span.Slice(Consumed, length));
        Consumed += length;

        return value;
    }
}
