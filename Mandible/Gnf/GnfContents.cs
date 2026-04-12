using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;
using System;
using System.Collections.Generic;

namespace Mandible.Gnf;

/// <summary>
/// Represents the contents block of a GNF image file.
/// </summary>
/// <param name="Version">The version of the file.</param>
/// <param name="AlignmentShift">Left-shift <c>1</c> by this value to get the byte alignment of the file.</param>
/// <param name="Reserved">Reserved.</param>
/// <param name="Textures">The textures contained in the file.</param>
public record GnfContents
(
    byte Version,
    byte AlignmentShift,
    byte Reserved,
    IReadOnlyList<GnfTextureHeader> Textures
) : IBinarySerializable<GnfContents>
{
    /// <summary>
    /// The minimum size in bytes of a serialized GNF contents structure. Additional size is required to store texture
    /// headers.
    /// </summary>
    public const int MINIMUM_SIZE = sizeof(byte) // Version
        + sizeof(byte) // NumTextures
        + sizeof(byte) // Alignment
        + sizeof(byte) // Reserved
        + sizeof(uint); // StreamSize

    /// <inheritdoc />
    public static GnfContents Deserialize(ref BinaryPrimitiveReader reader)
    {
        InvalidBufferSizeException.ThrowIfLessThan(MINIMUM_SIZE, reader.RemainingLength);

        byte version = reader.ReadByte();
        UnsupportedVersionException<byte>.ThrowIfMismatch(2, version);

        byte numTextures = reader.ReadByte();
        byte alignmentShift = reader.ReadByte();
        byte reserved = reader.ReadByte();
        reader.Seek(sizeof(uint)); // StreamSize
        GnfTextureHeader[] textures = new GnfTextureHeader[numTextures];

        for (int i = 0; i < numTextures; i++)
            textures[i] = GnfTextureHeader.Deserialize(ref reader);

        return new GnfContents(version, alignmentShift, reserved, textures);
    }

    /// <inheritdoc />
    public int GetSerializedSize()
        => MINIMUM_SIZE + Textures.Count * GnfTextureHeader.SIZE;

    /// <inheritdoc />
    public void Serialize(ref BinaryPrimitiveWriter writer)
    {
        InvalidBufferSizeException.ThrowIfLessThan(GetSerializedSize(), writer.RemainingLength);

        if (Textures.Count > byte.MaxValue)
            throw new InvalidOperationException($"Only {byte.MaxValue} textures are supported in a single file");

        writer.WriteByte(Version);
        writer.WriteByte((byte)Textures.Count);
        writer.WriteByte(AlignmentShift);
        writer.WriteByte(Reserved);
        writer.WriteUInt32LE(0); // TODO: Calculate the stream size

        foreach (GnfTextureHeader texture in Textures)
            texture.Serialize(ref writer);
    }
}
