using Mandible.Exceptions;
using Mandible.Util;
using System.Collections.Generic;

namespace Mandible.Zone;

/// <summary>
/// Represents an Eco definition of the <see cref="Zone"/> class.
/// </summary>
public class Eco
{
    /// <summary>
    /// The index of the eco.
    /// </summary>
    public uint Index { get; set; }

    /// <summary>
    /// The texture used by the eco.
    /// </summary>
    public TextureInfo TextureInfo { get; set; }

    /// <summary>
    /// The layers used by the eco.
    /// </summary>
    public List<EcoLayer> Layers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Eco"/> class.
    /// </summary>
    /// <param name="index">The index of the eco.</param>
    /// <param name="textureInfo">The texture used by the eco.</param>
    /// <param name="layers">The layers used by the eco.</param>
    public Eco(uint index, TextureInfo textureInfo, List<EcoLayer> layers)
    {
        Index = index;
        TextureInfo = textureInfo;
        Layers = layers;
    }

    /// <summary>
    /// Reads a <see cref="Eco"/> instance from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="Eco"/> instance.</returns>
    public static Eco Read(ref BinaryReader reader)
    {
        uint index = reader.ReadUInt32LE();
        TextureInfo textureInfo = TextureInfo.Read(ref reader);

        uint layersCount = reader.ReadUInt32LE();
        List<EcoLayer> layers = new();
        for (int i = 0; i < layersCount; i++)
            layers.Add(EcoLayer.Read(ref reader));

        return new Eco(index, textureInfo, layers);
    }

    /// <summary>
    /// Gets the serialized size of this <see cref="Eco"/>.
    /// </summary>
    public int GetSize()
    {
        int size = sizeof(uint) // Index
            + TextureInfo.GetSize();

        size += sizeof(uint); // Layers.Count
        foreach (EcoLayer layer in Layers)
            size += layer.GetSize();

        return size;
    }

    /// <summary>
    /// Writes this <see cref="Eco"/> instance to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryWriter writer)
    {
        int requiredSize = GetSize();
        if (requiredSize > writer.Remaining)
            throw new InvalidBufferSizeException(requiredSize, writer.Remaining);

        writer.WriteUInt32LE(Index);
        TextureInfo.Write(ref writer);

        writer.WriteUInt32LE((uint)Layers.Count);
        foreach (EcoLayer layer in Layers)
            layer.Write(ref writer);
    }
}
