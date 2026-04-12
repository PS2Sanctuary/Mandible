using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;
using System;

namespace Mandible.Gnf;

/// <summary>
/// Represents the header of a GNF image file.
/// </summary>
/// <param name="ContentsSize">The size of the <see cref="GnfContents"/> data.</param>
public readonly record struct GnfHeader(uint ContentsSize) : IBinarySerializable<GnfHeader>
{
    /// <summary>
    /// Gets the size in bytes of a serialized GNF Header structure.
    /// </summary>
    public static readonly int SIZE = GnfImage.MAGIC.Length
        + sizeof(uint); // ContentsSize

    /// <inheritdoc />
    public static GnfHeader Deserialize(ref BinaryPrimitiveReader reader)
    {
        InvalidBufferSizeException.ThrowIfLessThan(SIZE, reader.RemainingLength);

        ReadOnlySpan<byte> magic = reader.ReadBytes(GnfImage.MAGIC.Length);
        UnrecognisedMagicException.ThrowIfNotAtStart(GnfImage.MAGIC.Span, magic);

        uint contentsSize = reader.ReadUInt32LE();
        return new GnfHeader(contentsSize);
    }

    /// <inheritdoc />
    public int GetSerializedSize()
        => SIZE;

    /// <inheritdoc />
    public void Serialize(ref BinaryPrimitiveWriter writer)
    {
        InvalidBufferSizeException.ThrowIfLessThan(SIZE, writer.RemainingLength);

        writer.WriteBytes(GnfImage.MAGIC.Span);
        writer.WriteUInt32LE(ContentsSize);
    }
}
