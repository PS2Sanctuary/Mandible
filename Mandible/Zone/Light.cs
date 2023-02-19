using Mandible.Common;
using Mandible.Exceptions;
using Mandible.Util;
using System;

namespace Mandible.Zone;

public enum LightType : ushort
{
    Point = 1,
    Spot = 2
}

public class Light
{
    public string Name { get; set; }
    public string ColorName { get; set; }
    public LightType Type { get; set; }
    public ushort UnknownValue1 { get; set; }
    public bool UnknownValue2 { get; set; }
    public Vector4 Translation { get; set; }
    public Vector4 Rotation { get; set; }
    public float Range { get; set; }
    public float InnerRange { get; set; }
    public ColorARGB Color { get; set; }
    public uint UnknownValue3 { get; set; }
    public byte UnknownValue4 { get; set; }
    public Vector4 UnknownValue5 { get; set; }
    public string UnknownValue6 { get; set; }
    public uint Id { get; set; }

    /// <summary>
    /// Unknown. Only present in PSA zones.
    /// </summary>
    public float? UnknownValue7 { get; set; }

    /// <summary>
    /// Unknown. Only present in PSA zones.
    /// </summary>
    public uint? UnknownValue8 { get; set; }

    /// <summary>
    /// Unknown. Only present in PSA zones.
    /// </summary>
    public bool? UnknownValue9 { get; set; }

    public Light
    (
        string name,
        string colorName,
        LightType type,
        ushort unknownValue1,
        bool unknownValue2,
        Vector4 translation,
        Vector4 rotation,
        float range,
        float innerRange,
        ColorARGB color,
        uint unknownValue3,
        byte unknownValue4,
        Vector4 unknownValue5,
        string unknownValue6,
        uint id
    )
    {
        Name = name;
        ColorName = colorName;
        Type = type;
        UnknownValue1 = unknownValue1;
        UnknownValue2 = unknownValue2;
        Translation = translation;
        Rotation = rotation;
        Range = range;
        InnerRange = innerRange;
        Color = color;
        UnknownValue3 = unknownValue3;
        UnknownValue4 = unknownValue4;
        UnknownValue5 = unknownValue5;
        UnknownValue6 = unknownValue6;
        Id = id;
    }

    public static Light Read(ref BinaryReader reader, ZoneVersion version)
    {
        string name = reader.ReadStringNullTerminated();
        string colorName = reader.ReadStringNullTerminated();
        LightType type = (LightType)reader.ReadUInt16LE();
        ushort unknownValue1 = reader.ReadUInt16LE();
        bool unknownValue2 = reader.ReadBoolean();
        Vector4 translation = Vector4.Read(ref reader);
        Vector4 rotation = Vector4.Read(ref reader);
        float range = reader.ReadSingleLE();
        float innerRange = reader.ReadSingleLE();
        ColorARGB color = ColorARGB.Read(ref reader);
        uint unknownValue3 = reader.ReadUInt32LE();
        byte unknownValue4 = reader.ReadByte();
        Vector4 unknownValue5 = Vector4.Read(ref reader);
        string unknownValue6 = reader.ReadStringNullTerminated();
        uint id = reader.ReadUInt32LE();

        float? unknownValue7 = null;
        uint? unknownValue8 = null;
        bool? unknownValue9 = null;
        if (version is ZoneVersion.V1_PSA)
        {
            unknownValue7 = reader.ReadSingleLE();
            unknownValue8 = reader.ReadUInt32LE();
            unknownValue9 = reader.ReadBoolean();
        }

        return new Light
        (
            name,
            colorName,
            type,
            unknownValue1,
            unknownValue2,
            translation,
            rotation,
            range,
            innerRange,
            color,
            unknownValue3,
            unknownValue4,
            unknownValue5,
            unknownValue6,
            id
        )
        {
            UnknownValue7 = unknownValue7,
            UnknownValue8 = unknownValue8,
            UnknownValue9 = unknownValue9,
        };
    }

    public int GetSize(ZoneVersion version)
    {
        int size = Name.Length + 1
            + ColorName.Length + 1
            + sizeof(LightType) // Type
            + sizeof(ushort) // UnknownValue1
            + sizeof(bool) // UnknownValue2
            + Vector4.Size // Translation
            + Vector4.Size // Rotation
            + sizeof(float) // Range
            + sizeof(float) // InnerRange
            + ColorARGB.Size // Color
            + sizeof(uint) // UnknownValue3
            + sizeof(byte) // UnknownValue4
            + Vector4.Size // UnknownValue5
            + UnknownValue6.Length + 1
            + sizeof(uint); // Id

        if (version is ZoneVersion.V1_PSA)
        {
            size += sizeof(float); // UnknownValue7;
            size += sizeof(uint); // UnknownValue8;
            size += sizeof(bool); // UnknownValue9;
        }

        return size;
    }

    public void Write(ref BinaryWriter writer, ZoneVersion version)
    {
        int requiredSize = GetSize(version);
        if (requiredSize > writer.Remaining)
            throw new InvalidBufferSizeException(requiredSize, writer.Remaining);

        writer.WriteStringNullTerminated(Name);
        writer.WriteStringNullTerminated(ColorName);
        writer.WriteUInt16LE((ushort)Type);
        writer.WriteUInt16LE(UnknownValue1);
        writer.WriteBoolean(UnknownValue2);
        Translation.Write(ref writer);
        Rotation.Write(ref writer);
        writer.WriteSingleLE(Range);
        writer.WriteSingleLE(InnerRange);
        Color.Write(ref writer);
        writer.WriteUInt32LE(UnknownValue3);
        writer.WriteByte(UnknownValue4);
        UnknownValue5.Write(ref writer);
        writer.WriteStringNullTerminated(UnknownValue6);
        writer.WriteUInt32LE(Id);

        if (version is ZoneVersion.V1_PSA)
        {
            if (UnknownValue7 is null || UnknownValue8 is null || UnknownValue9 is null)
            {
                throw new InvalidOperationException
                (
                    $"None of UnknownValue7, UnknownValue8 and UnknownValue9 may be null when serializing as {version}"
                );
            }

            writer.WriteSingleLE(UnknownValue7.Value);
            writer.WriteUInt32LE(UnknownValue8.Value);
            writer.WriteBoolean(UnknownValue9.Value);
        }
    }
}
