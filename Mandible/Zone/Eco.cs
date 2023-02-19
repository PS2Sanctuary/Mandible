using Mandible.Exceptions;
using Mandible.Util;
using System.Collections.Generic;

namespace Mandible.Zone;

public class Eco
{
    public uint Index { get; set; }
    public TextureInfo TextureInfo { get; set; }
    public List<EcoLayer> Layers { get; set; }

    public Eco(uint index, TextureInfo textureInfo, List<EcoLayer> layers)
    {
        Index = index;
        TextureInfo = textureInfo;
        Layers = layers;
    }

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

    public int GetSize()
    {
        int size = sizeof(uint) // Index
            + TextureInfo.GetSize();

        size += sizeof(uint); // Layers.Count
        foreach (EcoLayer layer in Layers)
            size += layer.GetSize();

        return size;
    }

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
