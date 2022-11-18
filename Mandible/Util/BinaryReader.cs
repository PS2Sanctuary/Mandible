using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mandible.Util;

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

    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        ReadOnlySpan<byte> value = Span.Slice(Consumed, length);
        Consumed += length;
        return value;
    }

    public ushort ReadUInt16LE()
    {
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(Span[Consumed..]);
        Consumed += sizeof(ushort);
        return value;
    }

    public short ReadInt16LE()
    {
        short value = BinaryPrimitives.ReadInt16LittleEndian(Span[Consumed..]);
        Consumed += sizeof(short);
        return value;
    }

    public uint ReadUInt32LE()
    {
        uint value = BinaryPrimitives.ReadUInt32LittleEndian(Span[Consumed..]);
        Consumed += sizeof(uint);
        return value;
    }

    public int ReadInt32LE()
    {
        int value = BinaryPrimitives.ReadInt32LittleEndian(Span[Consumed..]);
        Consumed += sizeof(int);
        return value;
    }

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

    public string ReadString(int length, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        string value = encoding.GetString(Span.Slice(Consumed, length));
        Consumed += length;

        return value;
    }
}
