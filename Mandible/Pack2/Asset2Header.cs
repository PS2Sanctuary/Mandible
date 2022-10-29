using System;
using System.Buffers.Binary;

namespace Mandible.Pack2;

/// <summary>
/// Represents an asset header used in the pack2 file format.
/// </summary>
/// <param name="NameHash">Gets the CRC-64 hash of the uppercase file name.</param>
/// <param name="DataOffset">Gets the offset into the pack of the asset data, in bytes.</param>
/// <param name="DataSize">Gets the size in bytes of the packed asset data.</param>
/// <param name="ZipStatus">Gets a value indicating whether the packed asset has been compressed.</param>
/// <param name="DataHash">Gets the CRC-32 hash of the asset data.</param>
public record Asset2Header
(
    ulong NameHash,
    ulong DataOffset,
    ulong DataSize,
    Asset2ZipDefinition ZipStatus,
    uint DataHash
)
{
    /// <summary>
    /// Gets the size of an <see cref="Asset2Header"/> as stored within a pack.
    /// </summary>
    public const int Size = sizeof(ulong) // NameHash
        + sizeof(ulong) // DataOffset
        + sizeof(ulong) // DataSize
        + sizeof(Asset2ZipDefinition) // ZipStatus
        + sizeof(uint); // DataHash

    /// <summary>
    /// Serializes this <see cref="Asset2Header"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"Buffer must be at least {Size} bytes", nameof(buffer));

        BinaryPrimitives.WriteUInt64LittleEndian(buffer[..8], NameHash);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[8..16], DataOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[16..24], DataSize);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[24..28], (uint)ZipStatus);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[28..32], DataHash);
    }

    /// <summary>
    /// Deserializes a buffer to an <see cref="Asset2Header"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>An <see cref="Asset2Header"/>.</returns>
    public static Asset2Header Deserialize(ReadOnlySpan<byte> buffer)
    {
        ulong nameHash = BinaryPrimitives.ReadUInt64LittleEndian(buffer[..8]);
        ulong dataOffset = BinaryPrimitives.ReadUInt64LittleEndian(buffer[8..16]);
        ulong dataSize = BinaryPrimitives.ReadUInt64LittleEndian(buffer[16..24]);
        Asset2ZipDefinition isZipped = (Asset2ZipDefinition)BinaryPrimitives.ReadUInt32LittleEndian(buffer[24..28]);
        uint dataHash = BinaryPrimitives.ReadUInt32LittleEndian(buffer[28..32]);

        return new Asset2Header(nameHash, dataOffset, dataSize, isZipped, dataHash);
    }
}
