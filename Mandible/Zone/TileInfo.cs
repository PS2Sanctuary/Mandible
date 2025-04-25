using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Zone;

/// <summary>
/// Represents a TileInfo definition of the <see cref="Zone"/> class.
/// </summary>
public class TileInfo
{
    /// <summary>
    /// Gets the serialized size of a <see cref="TileInfo"/>.
    /// </summary>
    public const int Size = sizeof(uint) // QuadCount
        + sizeof(float) // Width
        + sizeof(float) // Height
        + sizeof(uint); // VertexCount

    /// <summary>
    /// Gets or sets the quad count.
    /// </summary>
    public uint QuadCount { get; set; }

    /// <summary>
    /// Gets or sets the width of the zone tiles.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the zone tiles.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Gets or sets the vertex count.
    /// </summary>
    public uint VertexCount { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataOffsets"/> class.
    /// </summary>
    /// <param name="quadCount">The quad count.</param>
    /// <param name="width">The width of the zone tiles.</param>
    /// <param name="height">The height of the zone tiles.</param>
    /// <param name="vertexCount">The vertex count.</param>
    public TileInfo(uint quadCount, float width, float height, uint vertexCount)
    {
        QuadCount = quadCount;
        Width = width;
        Height = height;
        VertexCount = vertexCount;
    }

    /// <summary>
    /// Reads a <see cref="TileInfo"/> instance from a <see cref="BinaryPrimitiveReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="DataOffsets"/> instance.</returns>
    public static TileInfo Read(ref BinaryPrimitiveReader reader)
    {
        uint quadCount = reader.ReadUInt32LE();
        float width = reader.ReadSingleLE();
        float height = reader.ReadSingleLE();
        uint vertexCount = reader.ReadUInt32LE();

        return new TileInfo(quadCount, width, height, vertexCount);
    }

    /// <summary>
    /// Writes this <see cref="TileInfo"/> instance to a <see cref="BinaryPrimitiveWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryPrimitiveWriter writer)
    {
        if (Size > writer.RemainingLength)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        writer.WriteUInt32LE(QuadCount);
        writer.WriteSingleLE(Width);
        writer.WriteSingleLE(Height);
        writer.WriteUInt32LE(VertexCount);
    }
}
