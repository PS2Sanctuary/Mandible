using Mandible.Common;
using Mandible.Exceptions;
using Mandible.Util;
using System;

namespace Mandible.Zone;

/// <summary>
/// Represents an object instance of the <see cref="Zone"/> class.
/// Used to define the in-world presence of a parent <see cref="RuntimeObject"/>.
/// </summary>
public class ObjectInstance
{
    /// <summary>
    /// Gets or sets the translation of the instance.
    /// </summary>
    public Vector4 Translation { get; set; }

    /// <summary>
    /// Gets or sets the rotation of the instance.
    /// </summary>
    public Vector4 Rotation { get; set; }

    /// <summary>
    /// Gets or sets the scale of the instance.
    /// </summary>
    public Vector4 Scale { get; set; }

    /// <summary>
    /// Unknown.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Unknown.
    /// </summary>
    public byte UnknownValue2 { get; set; }

    /// <summary>
    /// Unknown. This byte is not present in v1 zone assets.
    /// </summary>
    public byte? UnknownValue3 { get; set; }

    /// <summary>
    /// Unknown.
    /// </summary>
    public float UnknownValue4 { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectInstance"/> class.
    /// </summary>
    /// <param name="translation">The translation of the instance.</param>
    /// <param name="rotation">The rotation of the instance.</param>
    /// <param name="scale">The scale of the instance.</param>
    /// <param name="id"></param>
    /// <param name="unknownValue2"></param>
    /// <param name="unknownValue3"></param>
    /// <param name="unknownValue4"></param>
    public ObjectInstance
    (
        Vector4 translation,
        Vector4 rotation,
        Vector4 scale,
        uint id,
        byte unknownValue2,
        byte? unknownValue3,
        float unknownValue4
    )
    {
        Translation = translation;
        Rotation = rotation;
        Scale = scale;
        Id = id;
        UnknownValue2 = unknownValue2;
        UnknownValue3 = unknownValue3;
        UnknownValue4 = unknownValue4;
    }

    /// <summary>
    /// Reads a <see cref="ObjectInstance"/> instance from a <see cref="BinaryReader"/>
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="version">The zone version that the instance is being read from.</param>
    /// <returns>An <see cref="ObjectInstance"/> instance.</returns>
    public static ObjectInstance Read(ref BinaryReader reader, ZoneVersion version)
    {
        Vector4 translation = Vector4.Read(ref reader);
        Vector4 rotation = Vector4.Read(ref reader);
        Vector4 scale = Vector4.Read(ref reader);
        uint unknownValue1 = reader.ReadUInt32LE();
        byte unknownValue2 = reader.ReadByte();
        byte? unknownValue3 = null;
        if (version is ZoneVersion.V2)
            unknownValue3 = reader.ReadByte();
        float unknownValue4 = reader.ReadSingleLE();

        return new ObjectInstance
        (
            translation,
            rotation,
            scale,
            unknownValue1,
            unknownValue2,
            unknownValue3,
            unknownValue4
        );
    }

    /// <summary>
    /// Gets the size of a serialized <see cref="ObjectInstance"/>.
    /// </summary>
    /// <param name="version">The zone version that is being serialized.</param>
    /// <returns>The size in bytes.</returns>
    public static int GetSize(ZoneVersion version)
        => Vector4.Size // Translation
            + Vector4.Size // Rotation
            + Vector4.Size // Scale
            + sizeof(uint) // UnknownValue1
            + sizeof(byte) // UnknownValue2
            + (version is ZoneVersion.V2 ? sizeof(byte) : 0) // UnknownValue3
            + sizeof(float); // UnknownValue4

    /// <summary>
    /// Writes this <see cref="ObjectInstance"/> to a <see cref="BinaryWriter"/>
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="version">The zone version to serialize as.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the writer does not have enough remaining space.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this instance has not been correctly initialized for the requested version.
    /// </exception>
    public void Write(ref BinaryWriter writer, ZoneVersion version)
    {
        int requiredSize = GetSize(version);
        if (requiredSize > writer.Remaining)
            throw new InvalidBufferSizeException(requiredSize, writer.Remaining);

        Translation.Write(ref writer);
        Rotation.Write(ref writer);
        Scale.Write(ref writer);
        writer.WriteUInt32LE(Id);
        writer.WriteByte(UnknownValue2);

        if (version is ZoneVersion.V2)
        {
            byte value = UnknownValue3
                ?? throw new InvalidOperationException($"Must set UnknownValue3 when writing as {version}");
            writer.WriteByte(value);
        }

        writer.WriteSingleLE(UnknownValue4);
    }
}
