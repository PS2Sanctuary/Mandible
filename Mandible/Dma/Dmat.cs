using Mandible.Abstractions;
using Mandible.Exceptions;
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
    /// Gets the magic bytes that indicate
    /// </summary>
    public static readonly ReadOnlyMemory<byte> Magic = new[] { (byte)'D', (byte)'M', (byte)'A', (byte)'T' };

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
    /// <returns>A <see cref="Dmat"/> instance.</returns>
    public static Dmat Read(ReadOnlySpan<byte> buffer)
    {
        for (int i = 0; i < Magic.Length; i++)
        {
            if (buffer[i] != Magic.Span[i])
                throw new UnrecognisedMagicException(buffer[..4].ToArray(), Magic.ToArray());
        }

        uint version = BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..]);
        if (version != 1)
            throw new UnsupportedVersionException(1, version);

        uint texturesCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[8..]);
        int offset = 12;
        List<string> textureFileNames = new();

        for (int i = 0; i < texturesCount; i++)
        {
            int length = 1;
            while (buffer[offset] != 0)
            {
                length++;
                offset++;
            }

            string textureFileName = Encoding.ASCII.GetString(buffer[(offset - length)..offset]);
            textureFileNames.Add(textureFileName);
            offset++;
        }

        uint materialCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        List<Material> materials = new();

        for (int i = 0; i < materialCount; i++)
        {
            Material material = Material.Read(buffer[offset..]);
            materials.Add(material);
            offset += material.GetRequiredBufferSize();
        }

        return new Dmat(textureFileNames, materials);
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => 4 // magic
           + sizeof(uint)
           + sizeof(uint)
           + TextureFileNames.Sum(t => t.Length + 1) // name + null terminator
           + sizeof(uint)
           + Materials.Sum(m => m.GetRequiredBufferSize());

    /// <inheritdoc />
    public void Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        Magic.Span.CopyTo(buffer);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..], 1);
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
        {
            material.Write(buffer[offset..]);
            offset += material.GetRequiredBufferSize();
        }
    }
}
