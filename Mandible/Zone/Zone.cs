using Mandible.Abstractions;
using Mandible.Exceptions;
using Mandible.Util;
using System;

namespace Mandible.Zone;

/// <summary>
/// Represents a zone asset.
/// </summary>
public class Zone : IBufferWritable
{
    /// <summary>
    /// Gets the magic identifier of a zone file.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC = new[] { (byte)'Z', (byte)'O', (byte)'N', (byte)'E' };

    /// <summary>
    /// Gets or sets the version of the zone asset.
    /// </summary>
    public ZoneVersion Version { get; set; }

    /// <summary>
    /// TODO: Temporary, shouldn't expose to user.
    /// </summary>
    public DataOffsets Offsets { get; set; }

    /// <summary>
    /// Gets or sets the tiling information.
    /// </summary>
    public TileInfo TileInfo { get; set; }

    /// <summary>
    /// Gets or sets the chunk information.
    /// </summary>
    public ChunkInfo ChunkInfo { get; set; }

    public Zone
    (
        DataOffsets offsets,
        TileInfo tileInfo,
        ChunkInfo chunkInfo
    )
    {
        Offsets = offsets;
        TileInfo = tileInfo;
        ChunkInfo = chunkInfo;
    }

    /// <summary>
    /// Reads a <see cref="Zone"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/></param>
    /// <returns>A <see cref="Zone"/> instance.</returns>
    /// <exception cref="UnrecognisedMagicException">Thrown if the buffer does not represent a zone asset.</exception>
    public Zone Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        BinaryReader reader = new(buffer);

        if (buffer.IndexOf(MAGIC.Span) != 0)
            throw new UnrecognisedMagicException(buffer[..MAGIC.Length].ToArray(), MAGIC.ToArray());
        reader.Advance(MAGIC.Length);

        Version = (ZoneVersion)reader.ReadUInt32LE();
        DataOffsets offsets = DataOffsets.Read(ref reader);
        TileInfo tileInfo = TileInfo.Read(ref reader);
        ChunkInfo chunkInfo = ChunkInfo.Read(ref reader);

        amountRead = reader.Consumed;
        return new Zone(offsets, tileInfo, chunkInfo);
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => MAGIC.Length
            + sizeof(ZoneVersion)
            + DataOffsets.Size
            + TileInfo.Size
            + ChunkInfo.Size;

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        BinaryWriter writer = new(buffer);
        writer.WriteBytes(MAGIC.Span);
        writer.WriteUInt32LE((uint)Version);
        // TODO: Come back and write the data offsets
        TileInfo.Write(ref writer);
        ChunkInfo.Write(ref writer);

        return writer.Written;
    }
}
