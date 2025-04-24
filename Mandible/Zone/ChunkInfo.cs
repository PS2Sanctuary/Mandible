using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Zone;

/// <summary>
/// Represents a ChunkInfo definition of the <see cref="Zone"/> class.
/// </summary>
public class ChunkInfo
{
    /// <summary>
    /// Gets the serialized size of a <see cref="ChunkInfo"/>.
    /// </summary>
    public const int Size = sizeof(uint) // TileCount
        + sizeof(int) // StartX
        + sizeof(int) // StartY
        + sizeof(uint) // CountX
        + sizeof(uint); // CountY

    /// <summary>
    /// Gets or sets the tile count.
    /// </summary>
    public uint TileCount { get; set; }

    /// <summary>
    /// Gets or sets the starting X-coordinate.
    /// </summary>
    public int StartX { get; set; }

    /// <summary>
    /// Gets or sets the starting Y-coordinate.
    /// </summary>
    public int StartY { get; set; }

    /// <summary>
    /// Gets or sets the X count.
    /// </summary>
    public uint CountX { get; set; }

    /// <summary>
    /// Gets or sets the Y count.
    /// </summary>
    public uint CountY { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataOffsets"/> class.
    /// </summary>
    /// <param name="tileCount">The tile count.</param>
    /// <param name="startX">The starting X-coordinate.</param>
    /// <param name="startY">The starting Y-coordinate.</param>
    /// <param name="countX">The X count.</param>
    /// <param name="countY">The Y count.</param>
    public ChunkInfo(uint tileCount, int startX, int startY, uint countX, uint countY)
    {
        TileCount = tileCount;
        StartX = startX;
        StartY = startY;
        CountX = countX;
        CountY = countY;
    }

    /// <summary>
    /// Reads a <see cref="ChunkInfo"/> instance from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="ChunkInfo"/> instance.</returns>
    public static ChunkInfo Read(ref BinaryReader reader)
    {
        uint tileCount = reader.ReadUInt32LE();
        int startX = reader.ReadInt32LE();
        int startY = reader.ReadInt32LE();
        uint countX = reader.ReadUInt32LE();
        uint countY = reader.ReadUInt32LE();

        return new ChunkInfo(tileCount, startX, startY, countX, countY);
    }

    /// <summary>
    /// Writes this <see cref="ChunkInfo"/> instance to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryWriter writer)
    {
        if (Size > writer.RemainingLength)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        writer.WriteUInt32LE(TileCount);
        writer.WriteInt32LE(StartX);
        writer.WriteInt32LE(StartY);
        writer.WriteUInt32LE(CountX);
        writer.WriteUInt32LE(CountY);
    }
}
