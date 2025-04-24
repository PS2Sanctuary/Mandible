using BinaryPrimitiveHelpers;
using Mandible.Exceptions;
using System.Collections.Generic;

namespace Mandible.Zone;

public class EcoLayer
{
    public float Density { get; set; }
    public FloatRange Scale { get; set; }
    public float SlopePeak { get; set; }
    public float SlopeExtent { get; set; }
    public FloatRange Elevation { get; set; }
    public byte MinAlpha { get; set; }
    public string FloraName { get; set; }
    public List<EcoTint> Tints { get; set; }

    public EcoLayer
    (
        float density,
        FloatRange scale,
        float slopePeak,
        float slopeExtent,
        FloatRange elevation,
        byte minAlpha,
        string floraName,
        List<EcoTint> tints
    )
    {
        Density = density;
        Scale = scale;
        SlopePeak = slopePeak;
        SlopeExtent = slopeExtent;
        Elevation = elevation;
        MinAlpha = minAlpha;
        FloraName = floraName;
        Tints = tints;
    }

    public static EcoLayer Read(ref BinaryReader reader)
    {
        float density = reader.ReadSingleLE();
        FloatRange scale = FloatRange.Read(ref reader);
        float slopePeak = reader.ReadSingleLE();
        float slopeExtent = reader.ReadSingleLE();
        FloatRange elevation = FloatRange.Read(ref reader);
        byte minAlpha = reader.ReadByte();
        string floraName = reader.ReadStringNullTerminated();

        uint tintsCount = reader.ReadUInt32LE();
        List<EcoTint> tints = new();
        for (int i = 0; i < tintsCount; i++)
            tints.Add(EcoTint.Read(ref reader));

        return new EcoLayer(density, scale, slopePeak, slopeExtent, elevation, minAlpha, floraName, tints);
    }

    public int GetSize()
        => sizeof(float) // Density
            + FloatRange.Size // Scale
            + sizeof(float) // SlopePeak
            + sizeof(float) // SlopeExtent
            + FloatRange.Size // Elevation
            + sizeof(byte) // MinAlpha
            + FloraName.Length + 1
            + sizeof(uint) // Tints.Count
            + EcoTint.Size * Tints.Count;

    public void Write(ref BinaryWriter writer)
    {
        int requiredSize = GetSize();
        if (requiredSize > writer.RemainingLength)
            throw new InvalidBufferSizeException(requiredSize, writer.RemainingLength);

        writer.WriteSingleLE(Density);
        Scale.Write(ref writer);
        writer.WriteSingleLE(SlopePeak);
        writer.WriteSingleLE(SlopeExtent);
        Elevation.Write(ref writer);
        writer.WriteByte(MinAlpha);
        writer.WriteStringNullTerminated(FloraName);

        writer.WriteUInt32LE((uint)Tints.Count);
        foreach (EcoTint tint in Tints)
            tint.Write(ref writer);
    }
}
