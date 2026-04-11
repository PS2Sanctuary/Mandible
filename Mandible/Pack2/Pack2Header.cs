using BinaryPrimitiveHelpers;
using Mandible.Common;
using Mandible.Exceptions;
using System;

namespace Mandible.Pack2;

/// <summary>
/// Represents a PACK2 header.
/// </summary>
/// <param name="AssetCount">Gets the number of assets stored in the pack.</param>
/// <param name="Length">Gets the length of the pack, in bytes.</param>
/// <param name="AssetMapOffset">Gets the offset into the pack of the asset map, in bytes.</param>
/// <param name="Checksum">A 128-byte value assumed to be a checksum. How it is calculated is unknown.</param>
/// <param name="Version">Gets the version represented by this <see cref="Pack2Header"/> object.</param>
/// <param name="Unknown">An unknown value that is always set to 256.</param>
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
    public const int CHECKSUM_LENGTH = 128;

    /// <summary>
    /// Gets the magic identifier of a pack2 file.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC_BYTES = FileIdentifiers.Magics[FileType.Pack2];

    /// <summary>
    /// Gets the size of a <see cref="Pack2Header"/> as stored within a pack.
    /// </summary>
    public static readonly int Size = MAGIC_BYTES.Length
        + sizeof(byte) // Version
        + sizeof(uint) // AssetCount
        + sizeof(ulong) // Length
        + sizeof(ulong) // AssetMapOffset
        + sizeof(ulong) // Unknown
        + CHECKSUM_LENGTH;

    /// <summary>
    /// Serializes this <see cref="Pack2Header"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < Size)
            throw new ArgumentException($"Buffer must be at least {Size} bytes", nameof(buffer));

        BinaryPrimitiveWriter writer = new(buffer);
        writer.WriteBytes(MAGIC_BYTES.Span);
        writer.WriteByte(Version);
        writer.WriteUInt32LE(AssetCount);
        writer.WriteUInt64LE(Length);
        writer.WriteUInt64LE(AssetMapOffset);
        writer.WriteUInt64LE(Unknown);
        writer.WriteBytes(Checksum.Span);
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
        UnrecognisedMagicException.ThrowIfNotAtStart(MAGIC_BYTES.Span, buffer);

        BinaryPrimitiveReader reader = new(buffer);
        reader.Seek(MAGIC_BYTES.Length);
        byte version = reader.ReadByte();
        uint assetCount = reader.ReadUInt32LE();
        ulong length = reader.ReadUInt64LE();
        ulong assetMapOffset = reader.ReadUInt64LE();
        ulong unknown = reader.ReadUInt64LE();
        ReadOnlySpan<byte> checksum = reader.ReadBytes(CHECKSUM_LENGTH);

        return new Pack2Header(assetCount, length, assetMapOffset, checksum.ToArray(), version, unknown);
    }
}
