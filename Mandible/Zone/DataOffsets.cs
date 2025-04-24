using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Zone;

/// <summary>
/// Represents a DataOffsets definition of the <see cref="Zone"/> class.
/// </summary>
public class DataOffsets
{
    /// <summary>
    /// Gets the serialized size of a <see cref="DataOffsets"/>.
    /// </summary>
    public const int Size = sizeof(uint) // Ecos
        + sizeof(uint) // Floras
        + sizeof(uint) // InvisibleWalls
        + sizeof(uint) // Objects
        + sizeof(uint) // Lights
        + sizeof(uint); // Unknown

    /// <summary>
    /// Gets or sets the offset into the zone data at which the eco data begins.
    /// </summary>
    public uint Ecos { get; set; }

    /// <summary>
    /// Gets or sets the offset into the zone data at which the flora data begins.
    /// </summary>
    public uint Floras { get; set; }

    /// <summary>
    /// Gets or sets the offset into the zone data at which the invisible wall data begins.
    /// </summary>
    public uint InvisibleWalls { get; set; }

    /// <summary>
    /// Gets or sets the offset into the zone data at which the object data begins.
    /// </summary>
    public uint Objects { get; set; }

    /// <summary>
    /// Gets or sets the offset into the zone data at which the light data begins.
    /// </summary>
    public uint Lights { get; set; }

    /// <summary>
    /// Gets or sets the offset into the zone data at which the unknown data begins.
    /// </summary>
    public uint Unknown { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataOffsets"/> class.
    /// </summary>
    /// <param name="ecos">The offset into the zone data at which the eco data begins.</param>
    /// <param name="floras">The offset into the zone data at which the flora data begins.</param>
    /// <param name="invisibleWalls">The offset into the zone data at which the invisible wall data begins.</param>
    /// <param name="objects">The offset into the zone data at which the object data begins.</param>
    /// <param name="lights">The offset into the zone data at which the light data begins.</param>
    /// <param name="unknown">The offset into the zone data at which the unknown data begins.</param>
    public DataOffsets(uint ecos, uint floras, uint invisibleWalls, uint objects, uint lights, uint unknown)
    {
        Ecos = ecos;
        Floras = floras;
        InvisibleWalls = invisibleWalls;
        Objects = objects;
        Lights = lights;
        Unknown = unknown;
    }

    /// <summary>
    /// Reads a <see cref="DataOffsets"/> instance from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="DataOffsets"/> instance.</returns>
    public static DataOffsets Read(ref BinaryReader reader)
    {
        uint ecos = reader.ReadUInt32LE();
        uint floras = reader.ReadUInt32LE();
        uint invisibleWalls = reader.ReadUInt32LE();
        uint objects = reader.ReadUInt32LE();
        uint lights = reader.ReadUInt32LE();
        uint unknown = reader.ReadUInt32LE();

        return new DataOffsets(ecos, floras, invisibleWalls, objects, lights, unknown);
    }

    /// <summary>
    /// Writes this <see cref="DataOffsets"/> instance to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryWriter writer)
    {
        if (Size > writer.RemainingLength)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        writer.WriteUInt32LE(Ecos);
        writer.WriteUInt32LE(Floras);
        writer.WriteUInt32LE(InvisibleWalls);
        writer.WriteUInt32LE(Objects);
        writer.WriteUInt32LE(Lights);
        writer.WriteUInt32LE(Unknown);
    }
}
