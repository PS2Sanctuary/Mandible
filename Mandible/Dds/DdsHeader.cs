using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using System;
using System.Diagnostics;

namespace Mandible.Dds;

/// <summary>
/// Describes a DDS file header.
/// </summary>
/// <remarks>
/// <seealso href="https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-header"/>.
/// </remarks>
public struct DdsHeader : IBufferSerializable<DdsHeader>
{
    public const int SIZE = 124;

    /// <summary>
    /// The size of the structure.
    /// </summary>
    public uint Size = SIZE;

    /// <summary>
    /// Flags to indicate which members contain valid data.
    /// </summary>
    public DdsHeaderFlags Flags;

    /// <summary>
    /// Surface height (in pixels).
    /// </summary>
    public uint Height;

    /// <summary>
    /// Surface width (in pixels).
    /// </summary>
    public uint Width;

    /// <summary>
    /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top
    /// level texture for a compressed texture.
    /// </summary>
    /// <remarks>
    /// For information about how to compute the pitch, see the DDS File Layout section of the
    /// <see href="https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide#dds-file-layout">
    /// Programming Guide for DDS.</see>
    /// </remarks>
    public uint PitchOrLinearSize;

    /// <summary>
    /// Depth of a volume texture (in pixels), otherwise unused.
    /// </summary>
    /// <remarks>
    /// Only if <see cref="DdsHeaderFlags.DDS_HEADER_FLAGS_VOLUME"/> is set in <see cref="Flags"/>.
    /// </remarks>
    public uint Depth;

    /// <summary>
    /// Number of mipmap levels, otherwise unused.
    /// </summary>
    /// <remarks>
    /// Only if <see cref="DdsHeaderFlags.DDS_HEADER_FLAGS_MIPMAP"/> is set in <see cref="Flags"/>.
    /// </remarks>
    public uint MipMapCount;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint[] Reserved1;

    /// <summary>
    /// The pixel format.
    /// </summary>
    public DdsPixelFormat PixelFormat;

    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    public DdsSurfaceFlags Caps;

    /// <summary>
    /// Additional detail about the surfaces stored.
    /// </summary>
    public DdsCubemapFlags Caps2;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint Caps3;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint Caps4;

    /// <summary>
    /// Unused.
    /// </summary>
    public uint Reserved2;

    public DdsHeader()
    {
        Size = SIZE;
        Reserved1 = new uint[11];
    }

    /// <inheritdoc />
    public static DdsHeader Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        BinaryPrimitiveReader reader = new(buffer);

        uint size = reader.ReadUInt32LE();
        DdsHeaderFlags flags = (DdsHeaderFlags)reader.ReadUInt32LE();
        uint height = reader.ReadUInt32LE();
        uint width = reader.ReadUInt32LE();
        uint pitchOrLinearSize = reader.ReadUInt32LE();
        uint depth = reader.ReadUInt32LE();
        uint mipmapCount = reader.ReadUInt32LE();
        uint[] reserved1 = new uint[11];
        for (int i = 0; i < 11; i++)
            reserved1[i] = reader.ReadUInt32LE();
        DdsPixelFormat pixelFormat = DdsPixelFormat.Deserialize(ref reader);
        DdsSurfaceFlags caps = (DdsSurfaceFlags)reader.ReadUInt32LE();
        DdsCubemapFlags caps2 = (DdsCubemapFlags)reader.ReadUInt32LE();
        uint caps3 = reader.ReadUInt32LE();
        uint caps4 = reader.ReadUInt32LE();
        uint reserved2 = reader.ReadUInt32LE();

        amountRead = reader.Offset;
        return new DdsHeader
        {
            Size = size,
            Flags = flags,
            Height = height,
            Width = width,
            PitchOrLinearSize = pitchOrLinearSize,
            Depth = depth,
            MipMapCount = mipmapCount,
            Reserved1 = reserved1,
            PixelFormat = pixelFormat,
            Caps = caps,
            Caps2 = caps2,
            Caps3 = caps3,
            Caps4 = caps4,
            Reserved2 = reserved2
        };
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => SIZE;

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        BinaryPrimitiveWriter writer = new(buffer);

        writer.WriteUInt32LE(Size);
        writer.WriteUInt32LE((uint)Flags);
        writer.WriteUInt32LE(Height);
        writer.WriteUInt32LE(Width);
        writer.WriteUInt32LE(PitchOrLinearSize);
        writer.WriteUInt32LE(Depth);
        writer.WriteUInt32LE(MipMapCount);
        foreach (uint val in Reserved1)
            writer.WriteUInt32LE(val);
        PixelFormat.Serialize(ref writer);
        writer.WriteUInt32LE((uint)Caps);
        writer.WriteUInt32LE((uint)Caps2);
        writer.WriteUInt32LE(Caps3);
        writer.WriteUInt32LE(Caps4);
        writer.WriteUInt32LE(Reserved2);

        Debug.Assert(writer.Offset == SIZE);

        return writer.Offset;
    }
}
