using System;
using System.Buffers.Binary;

namespace Mandible.Pack2;

public readonly struct Asset2Header
{
    /// <summary>
    /// Gets the size of an <see cref="Asset2Header"/> as stored within a pack.
    /// </summary>
    public const int Size = 32;

    /// <summary>
    /// Gets the CRC-64 hash of the uppercase file name.
    /// </summary>
    public readonly ulong NameHash;

    /// <summary>
    /// Gets the offset into the pack of the asset data, in bytes.
    /// </summary>
    public readonly ulong DataOffset;

    /// <summary>
    /// Gets the size in bytes of the packed asset data.
    /// </summary>
    public readonly ulong DataSize;

    /// <summary>
    /// Gets a value indicating whether the packed asset has been compressed.
    /// </summary>
    public readonly AssetZipDefinition ZipStatus;

    /// <summary>
    /// Gets the CRC-32 hash of the asset data.
    /// </summary>
    public readonly uint DataHash;

    /// <summary>
    /// Initialises a new instance of the <see cref="Asset2Header"/> struct.
    /// </summary>
    /// <param name="nameHash">The CRC-64 hash of the uppercase file name.</param>
    /// <param name="dataOffset">The offset into the pack of the asset data, in bytes.</param>
    /// <param name="dataSize">The size in bytes of the packed asset data.</param>
    /// <param name="isZipped">A value indicating whether the packet asset has been compressed.</param>
    /// <param name="dataHash">The CRC-32 hash of the asset data.</param>
    public Asset2Header(ulong nameHash, ulong dataOffset, ulong dataSize, AssetZipDefinition isZipped, uint dataHash)
    {
        NameHash = nameHash;
        DataOffset = dataOffset;
        DataSize = dataSize;
        ZipStatus = isZipped;
        DataHash = dataHash;
    }

    /// <summary>
    /// Serializes this <see cref="Asset2Header"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"Buffer must be at least {Size} bytes", nameof(buffer));

        BinaryPrimitives.WriteUInt64LittleEndian(buffer[0..8], NameHash);
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
        ulong nameHash = BinaryPrimitives.ReadUInt64LittleEndian(buffer[0..8]);
        ulong dataOffset = BinaryPrimitives.ReadUInt64LittleEndian(buffer[8..16]);
        ulong dataSize = BinaryPrimitives.ReadUInt64LittleEndian(buffer[16..24]);
        AssetZipDefinition isZipped = (AssetZipDefinition)BinaryPrimitives.ReadUInt32LittleEndian(buffer[24..28]);
        uint dataHash = BinaryPrimitives.ReadUInt32LittleEndian(buffer[28..32]);

        return new Asset2Header(nameHash, dataOffset, dataSize, isZipped, dataHash);
    }
}
