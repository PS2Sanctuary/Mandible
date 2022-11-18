using Mandible.Abstractions;
using Mandible.Exceptions;
using Mandible.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
) : IBufferWritable
{
    private static readonly ReadOnlyMemory<byte> MAGIC = Encoding.ASCII.GetBytes("DMAT");

    /// <summary>
    /// Gets the DMAT version supported by this class.
    /// </summary>
    public const int SUPPORTED_VERSION = 1;

    /// <summary>
    /// Reads a <see cref="Dmat"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/>.</param>
    /// <returns>A <see cref="Dmat"/> instance.</returns>
    public static Dmat Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        if (buffer.IndexOf(MAGIC.Span) != 0)
            throw new UnrecognisedMagicException(buffer[..MAGIC.Length].ToArray(), MAGIC.ToArray());
        BinaryReader reader = new(buffer);
        reader.Advance(MAGIC.Length);

        uint version = reader.ReadUInt32LE();
        if (version != SUPPORTED_VERSION)
            throw new UnsupportedVersionException(1, version);

        uint texturesBlockLen = reader.ReadUInt32LE();
        int texBlockStartOffset = reader.Consumed;
        List<string> textureFileNames = new();

        while (reader.Consumed - texBlockStartOffset < texturesBlockLen)
            textureFileNames.Add(reader.ReadStringNullTerminated());

        uint materialCount = reader.ReadUInt32LE();

        List<Material> materials = new();
        for (int i = 0; i < materialCount; i++)
        {
            Material material = Material.Read(buffer[reader.Consumed..], out int matAmountRead);
            materials.Add(material);
            reader.Advance(matAmountRead);
        }

        amountRead = reader.Consumed;
        return new Dmat(textureFileNames, materials);
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => MAGIC.Length
           + sizeof(uint) // Version
           + sizeof(uint) // TexturesBlockLen
           + TextureFileNames.Sum(t => t.Length + 1) // name + null terminator
           + sizeof(uint) // MaterialsCount
           + Materials.Sum(m => m.GetRequiredBufferSize());

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        BinaryWriter writer = new(buffer);
        writer.WriteBytes(MAGIC.Span);
        writer.WriteUInt32LE(SUPPORTED_VERSION);
        writer.WriteUInt32LE((uint)TextureFileNames.Sum(t => t.Length + 1));

        foreach (string textureFileName in TextureFileNames)
            writer.WriteStringNullTerminated(textureFileName);

        writer.WriteUInt32LE((uint)Materials.Count);

        foreach (Material material in Materials)
        {
            int matAmountWritten = material.Write(buffer[writer.Written..]);
            writer.Advance(matAmountWritten);
        }

        return writer.Written;
    }
}
