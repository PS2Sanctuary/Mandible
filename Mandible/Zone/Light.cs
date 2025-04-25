using BinaryPrimitiveHelpers;
using Mandible.Common;
using Mandible.Exceptions;
using System;

namespace Mandible.Zone;

/// <summary>
/// Enumerates the types of lights.
/// </summary>
public enum LightType : ushort
{
    /// <summary>
    /// Indicates the light data is for a point light.
    /// </summary>
    Point = 1,

    /// <summary>
    /// Indicates the light data is for a spot light.
    /// </summary>
    Spot = 2
}

/// <summary>
/// Defines the version of the light data.
/// </summary>
public enum LightDataVersion : ushort
{
    /// <summary>
    /// The default light data version.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Indicates the light data contains extended information for the PlanetSide Arena variant of ForgeLight.
    /// </summary>
    PlanetSideArenaExtended = 1
}

/// <summary>
/// Represents light data that is consumed by the ForgeLight engine.
/// </summary>
public class Light
{
    /// <summary>
    /// The name of the light.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The identifying name of the color of the light.
    /// </summary>
    public string ColorName { get; set; }

    /// <summary>
    /// The type of the light.
    /// </summary>
    public LightType Type { get; set; }

    /// <summary>
    /// The version of light data represented by this instance.
    /// </summary>
    public LightDataVersion DataVersion { get; set; }

    /// <summary>
    /// UnknownValue2.
    /// </summary>
    public bool UnknownValue2 { get; set; }

    /// <summary>
    /// The position of the light in the world.
    /// </summary>
    public Vector4 Translation { get; set; }

    /// <summary>
    /// The direction in which the light is pointing.
    /// </summary>
    public Vector4 Rotation { get; set; }

    /// <summary>
    /// The range (in meters?) of the light.
    /// </summary>
    public float Range { get; set; }

    /// <summary>
    /// Unknown.
    /// </summary>
    public float InnerRange { get; set; }

    /// <summary>
    /// The color of the light.
    /// </summary>
    public ColorARGB Color { get; set; }

    /// <summary>
    /// UnknownValue3.
    /// </summary>
    public uint UnknownValue3 { get; set; }

    /// <summary>
    /// UnknownValue4.
    /// </summary>
    public byte UnknownValue4 { get; set; }

    /// <summary>
    /// UnknownValue5.
    /// </summary>
    public Vector4 UnknownValue5 { get; set; }

    /// <summary>
    /// UnknownValue6.
    /// </summary>
    public string UnknownValue6 { get; set; }

    /// <summary>
    /// The ID of the light object.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Light"/> class.
    /// </summary>
    public Light
    (
        string name,
        string colorName,
        LightType type,
        LightDataVersion dataVersion,
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
        DataVersion = dataVersion;
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

    /// <summary>
    /// Reads a <see cref="Light"/> instance from a <see cref="BinaryPrimitiveReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="Light"/> instance.</returns>
    public static Light Read(ref BinaryPrimitiveReader reader)
    {
        string name = reader.ReadStringNullTerminated();
        string colorName = reader.ReadStringNullTerminated();
        LightType type = (LightType)reader.ReadUInt16LE();
        LightDataVersion unknownValue1 = (LightDataVersion)reader.ReadUInt16LE();
        bool unknownValue2 = reader.ReadBool();
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
        if (unknownValue1 is LightDataVersion.PlanetSideArenaExtended)
        {
            unknownValue7 = reader.ReadSingleLE();
            unknownValue8 = reader.ReadUInt32LE();
            unknownValue9 = reader.ReadBool();
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

    /// <summary>
    /// Gets the number of bytes that this <see cref="Light"/> will use when stored within a pack.
    /// </summary>
    /// <returns>The size in bytes of this <see cref="Light"/>.</returns>
    public int GetSize()
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

        if (DataVersion is LightDataVersion.PlanetSideArenaExtended)
        {
            size += sizeof(float); // UnknownValue7;
            size += sizeof(uint); // UnknownValue8;
            size += sizeof(bool); // UnknownValue9;
        }

        return size;
    }

    /// <summary>
    /// Writes this <see cref="Light"/> instance to a <see cref="BinaryPrimitiveWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryPrimitiveWriter writer)
    {
        int requiredSize = GetSize();
        if (requiredSize > writer.RemainingLength)
            throw new InvalidBufferSizeException(requiredSize, writer.RemainingLength);

        writer.WriteStringNullTerminated(Name);
        writer.WriteStringNullTerminated(ColorName);
        writer.WriteUInt16LE((ushort)Type);
        writer.WriteUInt16LE((ushort)DataVersion);
        writer.WriteBool(UnknownValue2);
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

        if (DataVersion is not LightDataVersion.PlanetSideArenaExtended)
            return;

        if (UnknownValue7 is null || UnknownValue8 is null || UnknownValue9 is null)
        {
            throw new InvalidOperationException
            (
                $"None of UnknownValue7, UnknownValue8 and UnknownValue9 may be null when serializing as {DataVersion}"
            );
        }

        writer.WriteSingleLE(UnknownValue7.Value);
        writer.WriteUInt32LE(UnknownValue8.Value);
        writer.WriteBool(UnknownValue9.Value);
    }
}
