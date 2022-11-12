using Mandible.Abstractions;
using Mandible.Exceptions;
using MemoryReaders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mandible.Dma;

/*
struct dmat
{
    char magic[4];
    unsigned int version;
    unsigned int textures_length;
    char textures[textures_length];
    unsigned int material_count;
    material materials[material_count];
};
*/

/// <summary>
/// Represents material information.
/// </summary>
public class Dmat : IBufferWritable
{
    /// <summary>
    /// Gets the DMAT version supported by this class.
    /// </summary>
    public const int SUPPORTED_VERSION = 1;

    /// <summary>
    /// Gets the magic bytes that indicate
    /// </summary>
    public static readonly ReadOnlyMemory<byte> Magic = Encoding.ASCII.GetBytes("DMAT");

    /// <summary>
    /// Gets the file names of the textures to be applied to the materials.
    /// </summary>
    public IReadOnlyList<string> TextureFileNames { get; }

    /// <summary>
    /// Gets the materials.
    /// </summary>
    public IReadOnlyList<Material> Materials { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Dmat"/> class.
    /// </summary>
    /// <param name="textureFileNames">The texture file names.</param>
    /// <param name="materials">The materials.</param>
    public Dmat(IReadOnlyList<string> textureFileNames, IReadOnlyList<Material> materials)
    {
        TextureFileNames = textureFileNames;
        Materials = materials;
    }

    /// <summary>
    /// Reads a <see cref="Dmat"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/>.</param>
    /// <returns>A <see cref="Dmat"/> instance.</returns>
    public static Dmat Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        if (buffer.IndexOf(Magic.Span) != 0)
            throw new UnrecognisedMagicException(buffer[..Magic.Length].ToArray(), Magic.ToArray());
        int offset = Magic.Length;

        uint version = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);
        if (version != SUPPORTED_VERSION)
            throw new UnsupportedVersionException(1, version);

        uint texturesCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);

        List<string> textureFileNames = new();
        SpanReader<byte> reader = new(buffer[offset..]);

        for (int i = 0; i < texturesCount; i++)
        {
            bool readName = reader.TryReadTo(out ReadOnlySpan<byte> fileNameBytes, 0);
            if (!readName)
                break;

            string textureFileName = Encoding.ASCII.GetString(fileNameBytes);
            offset += fileNameBytes.Length;
            textureFileNames.Add(textureFileName);
        }

        uint materialCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);

        List<Material> materials = new();
        for (int i = 0; i < materialCount; i++)
        {
            Material material = Material.Read(buffer[offset..], out int matAmountRead);
            materials.Add(material);
            offset += matAmountRead;
        }

        amountRead = offset;
        return new Dmat(textureFileNames, materials);
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => Magic.Length
           + sizeof(uint) // Version
           + sizeof(uint) // TextureFileNamesCount
           + TextureFileNames.Sum(t => t.Length + 1) // name + null terminator
           + sizeof(uint) // MaterialsCount
           + Materials.Sum(m => m.GetRequiredBufferSize());

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        Magic.Span.CopyTo(buffer);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..], SUPPORTED_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[8..], (uint)TextureFileNames.Count);

        int offset = 12;
        foreach (string textureFileName in TextureFileNames)
        {
            byte[] tfnBytes = Encoding.ASCII.GetBytes(textureFileName);
            tfnBytes.AsSpan().CopyTo(buffer[offset..]);

            offset += tfnBytes.Length + 1;
            buffer[offset - 1] = 0;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(buffer[offset..], (uint)Materials.Count);
        offset += sizeof(uint);

        foreach (Material material in Materials)
            offset += material.Write(buffer[offset..]);

        return offset;
    }
}
