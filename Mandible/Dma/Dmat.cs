using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Common;
using Mandible.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mandible.Dma;

/// <summary>
/// Represents material information.
/// </summary>
/// <param name="TextureFileNames">The file names of the textures to be applied to the materials.</param>
/// <param name="Materials">The materials.</param>
public record Dmat
(
    IReadOnlyList<string> TextureFileNames,
    IReadOnlyList<Material> Materials
) : IBufferSerializable<Dmat>
{
    private static readonly ReadOnlyMemory<byte> MAGIC = FileIdentifiers.Magics[FileType.MaterialInfo];

    /// <summary>
    /// Gets the DMAT version supported by this class.
    /// </summary>
    public const int SUPPORTED_VERSION = 1;

    /// <inheritdoc />
    public static Dmat Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        UnrecognisedMagicException.ThrowIfNotAtStart(MAGIC.Span, buffer);

        BinaryPrimitiveReader reader = new(buffer);
        reader.Seek(MAGIC.Length);

        uint version = reader.ReadUInt32LE();
        if (version != SUPPORTED_VERSION)
            throw new UnsupportedVersionException<uint>(1, version);

        uint texturesBlockLen = reader.ReadUInt32LE();
        int texBlockStartOffset = reader.Offset;
        List<string> textureFileNames = [];

        while (reader.Offset - texBlockStartOffset < texturesBlockLen)
            textureFileNames.Add(reader.ReadStringNullTerminated());

        uint materialCount = reader.ReadUInt32LE();

        List<Material> materials = [];
        for (int i = 0; i < materialCount; i++)
        {
            Material material = Material.Deserialize(ref reader);
            materials.Add(material);
        }

        amountRead = reader.Offset;
        return new Dmat(textureFileNames, materials);
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => MAGIC.Length
           + sizeof(uint) // Version
           + sizeof(uint) // TexturesBlockLen
           + TextureFileNames.Sum(t => t.Length + 1) // name + null terminator
           + sizeof(uint) // MaterialsCount
           + Materials.Sum(m => m.GetSerializedSize());

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        InvalidBufferSizeException.ThrowIfLessThan(GetRequiredBufferSize(), buffer.Length);

        BinaryPrimitiveWriter writer = new(buffer);
        writer.WriteBytes(MAGIC.Span);
        writer.WriteUInt32LE(SUPPORTED_VERSION);
        writer.WriteUInt32LE((uint)TextureFileNames.Sum(t => t.Length + 1));

        foreach (string textureFileName in TextureFileNames)
            writer.WriteStringNullTerminated(textureFileName);

        writer.WriteUInt32LE((uint)Materials.Count);

        foreach (Material material in Materials)
            material.Serialize(ref writer);

        return writer.Offset;
    }
}
