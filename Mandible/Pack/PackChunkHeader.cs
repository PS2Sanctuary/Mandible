using System;
using System.Buffers.Binary;

namespace Mandible.Pack;

/// <summary>
/// Represents a chunk header used in the pack file format.
/// </summary>
public class PackChunkHeader
{
    /// <summary>
    /// Gets the size of a pack chunk header when stored within a pack.
    /// </summary>
    public const int Size = 8;

    /// <summary>
    /// Gets the offset of the next chunk into the pack.
    /// </summary>
    public uint NextChunkOffset { get; }

    /// <summary>
    /// Gets the number of assets contained in this chunk.
    /// </summary>
    public uint AssetCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackChunkHeader"/> class.
    /// </summary>
    /// <param name="nextChunkOffset">The offset into the pack of the next chunk.</param>
    /// <param name="assetCount">The number of assets in this chunk.</param>
    public PackChunkHeader(uint nextChunkOffset, uint assetCount)
    {
        NextChunkOffset = nextChunkOffset;
        AssetCount = assetCount;
    }

    /// <summary>
    /// Serializes this <see cref="PackChunkHeader"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"Buffer must be at least {Size} bytes", nameof(buffer));

        BinaryPrimitives.WriteUInt32BigEndian(buffer[..4], NextChunkOffset);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[4..8], AssetCount);
    }

    /// <summary>
    /// Deserializes a buffer to a <see cref="PackChunkHeader"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>An <see cref="PackChunkHeader"/>.</returns>
    public static PackChunkHeader Deserialize(ReadOnlySpan<byte> buffer)
    {
        uint nextChunkOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer[..4]);
        uint assetCount = BinaryPrimitives.ReadUInt32BigEndian(buffer[4..8]);

        return new PackChunkHeader(nextChunkOffset, assetCount);
    }
}
