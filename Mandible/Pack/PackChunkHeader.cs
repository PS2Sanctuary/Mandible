using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace Mandible.Pack;

/// <summary>
/// Represents a chunk header used in .pack files.
/// </summary>
public class PackChunkHeader
{
    /// <summary>
    /// Gets the offset of the next chunk into the pack.
    /// </summary>
    public uint NextChunkOffset { get; }

    /// <summary>
    /// Gets the number of assets contained in this chunk.
    /// </summary>
    public uint AssetCount { get; }

    public IReadOnlyList<AssetHeader> AssetHeaders { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PackChunkHeader"/> class.
    /// </summary>
    /// <param name="nextChunkOffset">The offset into the pack of the next chunk.</param>
    /// <param name="assetHeaders">The asset headers contained within this chunk.</param>
    public PackChunkHeader(uint nextChunkOffset, IReadOnlyList<AssetHeader> assetHeaders)
    {
        NextChunkOffset = nextChunkOffset;
        AssetCount = (uint)assetHeaders.Count;
        AssetHeaders = assetHeaders;
    }

    /// <summary>
    /// Gets the number of bytes that this <see cref="PackChunkHeader"/> will use when stored within a pack.
    /// </summary>
    /// <returns>The size in bytes of this <see cref="PackChunkHeader"/>.</returns>
    public int GetSize()
        => 16 + AssetHeaders.Sum(h => h.GetSize());

    /// <summary>
    /// Serializes this <see cref="PackChunkHeader"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < GetSize())
            throw new ArgumentException($"Buffer must be at least {GetSize()} bytes", nameof(buffer));

        int index = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], NextChunkOffset);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], AssetCount);

        foreach (AssetHeader header in AssetHeaders)
            header.Serialize(buffer[index..(index += header.GetSize())]);
    }

    /// <summary>
    /// Deserializes a buffer to a <see cref="PackChunkHeader"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>An <see cref="PackChunkHeader"/>.</returns>
    public static PackChunkHeader Deserialize(ReadOnlySpan<byte> buffer)
    {
        int index = 0;

        uint nextChunkOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
        uint assetCount = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);

        List<AssetHeader> assetHeaders = new();
        for (int i = 0; i < assetCount; i++)
        {
            AssetHeader header = AssetHeader.Deserialize(buffer[index..]);
            index += header.GetSize();
        }

        return new PackChunkHeader(nextChunkOffset, assetHeaders);
    }
}
