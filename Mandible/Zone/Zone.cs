using Mandible.Abstractions;
using Mandible.Exceptions;
using Mandible.Util;
using System;
using System.Collections.Generic;

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
    /// Gets or sets the tiling information.
    /// </summary>
    public TileInfo TileInfo { get; set; }

    /// <summary>
    /// Gets or sets the chunk information.
    /// </summary>
    public ChunkInfo ChunkInfo { get; set; }

    /// <summary>
    /// Gets or sets the eco data.
    /// </summary>
    public List<Eco> Ecos { get; set; }

    /// <summary>
    /// Gets or sets the flora data.
    /// </summary>
    public List<Flora> Florae { get; set; }

    /// <summary>
    /// Gets or sets the invisible wall data.
    /// </summary>
    public List<InvisibleWall> InvisibleWalls { get; set; }

    /// <summary>
    /// Gets the objects used by the zone.
    /// </summary>
    public List<RuntimeObject> Objects { get; set; }

    /// <summary>
    /// Gets the lights used in the zone.
    /// </summary>
    public List<Light> Lights { get; set; }

    /// <summary>
    /// Unknown.
    /// </summary>
    public byte[] UnknownValue1 { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Zone"/> class.
    /// </summary>
    /// <param name="version">The version represented by the instance.</param>
    /// <param name="tileInfo">The tiling information.</param>
    /// <param name="chunkInfo">The chunk information.</param>
    /// <param name="ecos">The eco data.</param>
    /// <param name="florae">The flora data.</param>
    /// <param name="invisibleWalls">The invisible wall data.</param>
    /// <param name="objects">The objects used by the zone.</param>
    /// <param name="lights">The lights used in the zone.</param>
    /// <param name="unknownValue1">Unknown.</param>
    public Zone
    (
        ZoneVersion version,
        TileInfo tileInfo,
        ChunkInfo chunkInfo,
        List<Eco> ecos,
        List<Flora> florae,
        List<InvisibleWall> invisibleWalls,
        List<RuntimeObject> objects,
        List<Light> lights,
        byte[] unknownValue1
    )
    {
        Version = version;
        TileInfo = tileInfo;
        ChunkInfo = chunkInfo;
        Ecos = ecos;
        Florae = florae;
        InvisibleWalls = invisibleWalls;
        Objects = objects;
        Lights = lights;
        UnknownValue1 = unknownValue1;
    }

    /// <summary>
    /// Reads a <see cref="Zone"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/></param>
    /// <returns>A <see cref="Zone"/> instance.</returns>
    /// <exception cref="UnrecognisedMagicException">Thrown if the buffer does not represent a zone asset.</exception>
    public static Zone Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        BinaryReader reader = new(buffer);

        if (buffer.IndexOf(MAGIC.Span) != 0)
            throw new UnrecognisedMagicException(buffer[..MAGIC.Length].ToArray(), MAGIC.ToArray());
        reader.Advance(MAGIC.Length);

        ZoneVersion version = (ZoneVersion)reader.ReadUInt32LE();
        DataOffsets.Read(ref reader);
        TileInfo tileInfo = TileInfo.Read(ref reader);
        ChunkInfo chunkInfo = ChunkInfo.Read(ref reader);

        uint ecosCount = reader.ReadUInt32LE();
        List<Eco> ecos = new();
        for (int i = 0; i < ecosCount; i++)
            ecos.Add(Eco.Read(ref reader));

        uint floraeCount = reader.ReadUInt32LE();
        List<Flora> florae = new();
        for (int i = 0; i < floraeCount; i++)
            florae.Add(Flora.Read(ref reader));

        uint invisibleWallsCount = reader.ReadUInt32LE();
        List<InvisibleWall> invisibleWalls = new();
        for (int i = 0; i < invisibleWallsCount; i++)
            invisibleWalls.Add(InvisibleWall.Read(ref reader));

        uint objectsCount = reader.ReadUInt32LE();
        List<RuntimeObject> objects = new();
        for (int i = 0; i < objectsCount; i++)
            objects.Add(RuntimeObject.Read(ref reader, version));

        uint lightsCount = reader.ReadUInt32LE();
        List<Light> lights = new();
        for (int i = 0; i < lightsCount; i++)
            lights.Add(Light.Read(ref reader));

        uint unknownValue1Count = reader.ReadUInt32LE();
        byte[] unknownValue1 = reader.ReadBytes((int)unknownValue1Count).ToArray();

        amountRead = reader.Consumed;
        return new Zone(version, tileInfo, chunkInfo, ecos, florae, invisibleWalls, objects, lights, unknownValue1);
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
    {
        int size = MAGIC.Length
            + sizeof(ZoneVersion)
            + DataOffsets.Size
            + TileInfo.Size
            + ChunkInfo.Size;

        size += sizeof(uint); // Ecos.Count
        foreach (Eco eco in Ecos)
            size += eco.GetSize();

        size += sizeof(uint); // Florae.Count
        foreach (Flora flora in Florae)
            size += flora.GetSize();

        size += sizeof(uint) // InvisibleWalls.Count
            + InvisibleWall.Size * InvisibleWalls.Count;

        size += sizeof(uint); // Objects.Count
        foreach (RuntimeObject obj in Objects)
            size += obj.GetSize(Version);

        size += sizeof(uint); // Lights.Count
        foreach (Light light in Lights)
            size += light.GetSize();

        size += sizeof(uint) // UnknownValue1.Count
            + UnknownValue1.Length;

        return size;
    }

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        BinaryWriter writer = new(buffer);
        writer.WriteBytes(MAGIC.Span);
        writer.WriteUInt32LE((uint)Version);
        writer.Advance(DataOffsets.Size);
        TileInfo.Write(ref writer);
        ChunkInfo.Write(ref writer);

        uint ecosOffset = (uint)writer.Written;
        writer.WriteUInt32LE((uint)Ecos.Count);
        foreach (Eco eco in Ecos)
            eco.Write(ref writer);

        uint floraeOffset = (uint)writer.Written;
        writer.WriteUInt32LE((uint)Florae.Count);
        foreach (Flora flora in Florae)
            flora.Write(ref writer);

        uint invisibleWallsOffset = (uint)writer.Written;
        writer.WriteUInt32LE((uint)InvisibleWalls.Count); // InvisibleWalls.Count
        foreach (InvisibleWall wall in InvisibleWalls)
            wall.Write(ref writer);

        uint objectsOffset = (uint)writer.Written;
        writer.WriteUInt32LE((uint)Objects.Count);
        foreach (RuntimeObject obj in Objects)
            obj.Write(ref writer, Version);

        uint lightsOffset = (uint)writer.Written;
        writer.WriteUInt32LE((uint)Lights.Count);
        foreach (Light light in Lights)
            light.Write(ref writer);

        uint unknownValue1Offset = (uint)writer.Written;
        writer.WriteUInt32LE((uint)UnknownValue1.Length);
        writer.WriteBytes(UnknownValue1);

        DataOffsets offsets = new
        (
            ecosOffset,
            floraeOffset,
            invisibleWallsOffset,
            objectsOffset,
            lightsOffset,
            unknownValue1Offset
        );

        BinaryWriter offsetsWriter = new(buffer);
        offsetsWriter.Advance(MAGIC.Length + sizeof(ZoneVersion));
        offsets.Write(ref offsetsWriter);

        return writer.Written;
    }
}
