using Mandible.Exceptions;
using System;
using System.Buffers.Binary;

namespace Mandible.Pack2;

/// <summary>
/// 
/// </summary>
/// <param name="AssetCount">Gets the number of assets stored in the pack.</param>
/// <param name="Length">Gets the length of the pack, in bytes.</param>
/// <param name="AssetMapOffset">Gets the offset into the pack of the asset map, in bytes.</param>
/// <param name="Unknown">An unknown value that is always set to 256.</param>
/// <param name="Version">Gets the version represented by this <see cref="Pack2Header"/> object.</param>
/// <param name="Checksum">A value assumed to be a checksum. How it is calculated is unknown.</param>
public record Pack2Header
(
    uint AssetCount,
    ulong Length,
    ulong AssetMapOffset,
    ReadOnlyMemory<byte> Checksum,
    byte Version = 1,
    ulong Unknown = 256
)
{
    /// <summary>
    /// Gets the magic identifier of a pack2 file.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC_BYTES = "PAK"u8.ToArray();

    /// <summary>
    /// Gets the size of a <see cref="Pack2Header"/> as stored within a pack.
    /// </summary>
    public static readonly int Size = MAGIC_BYTES.Length
        + sizeof(byte) // Version
        + sizeof(uint) // AssetCount
        + sizeof(ulong) // Length
        + sizeof(ulong) // AssetMapOffset
        + sizeof(ulong) // Unknown
        + 128; // Checksum

    /// <summary>
    /// Serializes this <see cref="Pack2Header"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"Buffer must be at least {Size} bytes", nameof(buffer));

        // Write the magic bytes
        MAGIC_BYTES.Span.CopyTo(buffer);
        buffer[3] = Version;

        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..8], AssetCount);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[8..16], Length);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[16..24], AssetMapOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(buffer[24..32], Unknown);
        Checksum.Span.CopyTo(buffer[32..]);
    }

    /// <summary>
    /// Deserializes a buffer to a <see cref="Pack2Header"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>An <see cref="Pack2Header"/>.</returns>
    /// <exception cref="UnrecognisedMagicException">
    /// Thrown if the <paramref name="buffer"/> contains an invalid magic.
    /// </exception>
    public static Pack2Header Deserialize(ReadOnlySpan<byte> buffer)
    {
        int offset = 0;

        foreach (byte value in MAGIC_BYTES.Span)
        {
            if (buffer[offset++] != value)
                throw new UnrecognisedMagicException(MAGIC_BYTES.ToArray(), buffer[..MAGIC_BYTES.Length].ToArray());
        }

        byte version = buffer[offset++];

        uint assetCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);

        ulong length = BinaryPrimitives.ReadUInt64LittleEndian(buffer[offset..]);
        offset += sizeof(ulong);

        ulong assetMapOffset = BinaryPrimitives.ReadUInt64LittleEndian(buffer[offset..]);
        offset += sizeof(ulong);

        ulong unknown = BinaryPrimitives.ReadUInt64LittleEndian(buffer[offset..]);
        offset += sizeof(ulong);

        return new Pack2Header(assetCount, length, assetMapOffset, buffer[offset..].ToArray(), version, unknown);
    }
}
