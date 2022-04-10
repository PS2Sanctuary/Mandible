using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Mandible.Pack;

/// <summary>
/// Represents an asset header used in the pack file format.
/// </summary>
public class AssetHeader
{
    /// <summary>
    /// Gets the length of the asset <see cref="Name"/>.
    /// </summary>
    public uint NameLength { get; }

    /// <summary>
    /// Gets the name of the asset.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the byte offset into the pack at which the asset data is stored.
    /// </summary>
    public uint DataOffset { get; }

    /// <summary>
    /// Gets the length of the packed data.
    /// </summary>
    public uint DataLength { get; }

    /// <summary>
    /// Gets a CRC-32 checksum of the packed data.
    /// </summary>
    public uint Checksum { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetHeader"/> class.
    /// </summary>
    /// <param name="nameLength">The length of the asset's name.</param>
    /// <param name="name">The name of the asset.</param>
    /// <param name="dataOffset">The byte offset into the pack at which the asset data is stored.</param>
    /// <param name="dataLength">The length of the packed data.</param>
    /// <param name="checksum">The CRC-32 checksum of the packed data.</param>
    public AssetHeader
    (
        uint nameLength,
        string name,
        uint dataOffset,
        uint dataLength,
        uint checksum
    )
    {
        NameLength = nameLength;
        Name = name;
        DataOffset = dataOffset;
        DataLength = dataLength;
        Checksum = checksum;
    }

    /// <summary>
    /// Gets the number of bytes that this <see cref="AssetHeader"/> will use when stored within a pack.
    /// </summary>
    /// <returns>The size in bytes of this <see cref="AssetHeader"/>.</returns>
    public int GetSize()
        => GetSize(Name);

    /// <summary>
    /// Gets the number of bytes that an <see cref="AssetHeader"/> will use when stored within a pack.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    /// <returns>The size in bytes of an <see cref="AssetHeader"/>.</returns>
    public static int GetSize(string name)
        => 16 + name.Length;

    /// <summary>
    /// Attempts to deserialize a buffer to a <see cref="AssetHeader"/> instance.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="header">The header.</param>
    /// <returns>A value indicating whether or no the deserialization was successful. Failure occurs because the buffer was too small.</returns>
    public static bool TryDeserialize(ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out AssetHeader? header)
    {
        header = null;

        if (buffer.Length < 16)
            return false;

        int index = 0;

        uint nameLength = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
        if (buffer.Length < 16 + nameLength)
            return false;

        string name = Encoding.ASCII.GetString(buffer[index..(index += (int)nameLength)]);
        uint assetOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
        uint dataLength = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
        uint checksum = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..]);

        header = new AssetHeader(nameLength, name, assetOffset, dataLength, checksum);
        return true;
    }

    /// <summary>
    /// Serializes this <see cref="PackChunkHeader"/> to a byte buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
    public void Serialize(Span<byte> buffer)
    {
        if (buffer.Length < GetSize())
            throw new ArgumentException($"Buffer must be at least {16 + Name.Length} bytes", nameof(buffer));

        int index = 0;

        BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], NameLength);

        foreach (char value in Name)
            buffer[index++] = (byte)value;

        BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], DataOffset);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], DataLength);
        BinaryPrimitives.WriteUInt32BigEndian(buffer[index..], Checksum);
    }
}
