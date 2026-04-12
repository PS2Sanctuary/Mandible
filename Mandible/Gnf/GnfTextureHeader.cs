using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;

namespace Mandible.Gnf;

public enum GnfTextureMemoryCoherency : byte
{
    NoCoherency = 0,
    GpuCoherent = 1,
    SystemCoherent = 2,
    GpuAndSystemCoherent = 3
}

/// <summary>
/// Represents a GNF texture header structure.
/// </summary>
/// <param name="BaseAddress">
/// The base address of the texture data in the file. This value must be offset by
/// <see cref="GnfHeader">sizeof(GnfHeader)</see> + <see cref="GnfHeader.ContentsSize"/>, and then ensure it is
/// aligned by <see cref="BaseAddressAlignmentShift"/>.
/// </param>
/// <param name="BaseAddressAlignmentShift">
/// Left-shift <c>1</c> by this value to get the alignment of the texture data.
/// </param>
/// <param name="MemoryCoherency">The memory coherency.</param>
/// <param name="MinLod">The minimum level of detail.</param>
/// <param name="DataFormat">The texture data / surface format.</param>
/// <param name="NumberFormat">The type of number used to store the texture data.</param>
/// <param name="MType0">Memory type, controls cache behaviour. If enabled, uses L1 cache LRU.</param>
/// <param name="Width">The width of the texture in pixels.</param>
/// <param name="Height">The height of the texture in pixels.</param>
/// <param name="Perfmod"></param>
/// <param name="Interlaced">Whether the texture is interlaced.</param>
/// <param name="ChannelX">Which texture channel to map to X.</param>
/// <param name="ChannelY">Which texture channel to map to Y.</param>
/// <param name="ChannelZ">Which texture channel to map to Z.</param>
/// <param name="ChannelW">Which texture channel to map to W.</param>
/// <param name="BaseMipLevel">The base mip level.</param>
/// <param name="LastMipLevel">Either the last mip level, or number of fragments / samples for MSAA.</param>
/// <param name="TileMode">The tile mode.</param>
/// <param name="Pow2Pad">If <c>true</c>, the memory footprint is padded to pow2 dimensions.</param>
/// <param name="IsReadOnly">If true, the texture is read-only.</param>
/// <param name="Atc"></param>
/// <param name="TextureType">The type of the texture.</param>
/// <param name="Depth"></param>
/// <param name="Pitch"></param>
/// <param name="BaseArray"></param>
/// <param name="LastArray"></param>
/// <param name="MinLodWarn"></param>
/// <param name="CounterBankId"></param>
/// <param name="LodHdwcnten"></param>
/// <param name="CompressionEn">GFX-8 only.</param>
/// <param name="AlphaIsOnMsb">If <c>true</c>, the alpha is on the most significant bit.</param>
/// <param name="ColorTransform"></param>
/// <param name="AltTileMode">NEO-only attribute.</param>
/// <param name="TextureSize">The length in bytes of the texture data.</param>
public readonly record struct GnfTextureHeader
(
    uint BaseAddress,
    byte BaseAddressAlignmentShift,
    GnfTextureMemoryCoherency MemoryCoherency,
    ushort MinLod,
    GnmImageDataFormat DataFormat,
    GnmImageNumberFormat NumberFormat,
    byte MType0,
    ushort Width,
    ushort Height,
    byte Perfmod,
    bool Interlaced,
    GnmChannel ChannelX,
    GnmChannel ChannelY,
    GnmChannel ChannelZ,
    GnmChannel ChannelW,
    byte BaseMipLevel,
    byte LastMipLevel,
    GnmTileMode TileMode,
    bool Pow2Pad,
    bool IsReadOnly,
    bool Atc,
    GnmTextureType TextureType,
    ushort Depth,
    ushort Pitch,
    ushort BaseArray,
    ushort LastArray,
    ushort MinLodWarn,
    byte CounterBankId,
    bool LodHdwcnten,
    bool CompressionEn,
    bool AlphaIsOnMsb,
    bool ColorTransform,
    bool AltTileMode,
    uint TextureSize
) : IBinarySerializable<GnfTextureHeader>
{
    // Texture headers are designed to be stored in seven registers.
    public const int SIZE = 7 * sizeof(uint);

    public static GnfTextureHeader Deserialize(ref BinaryPrimitiveReader reader)
    {
        InvalidBufferSizeException.ThrowIfLessThan(SIZE, reader.RemainingLength);

        uint baseAddress = reader.ReadUInt32LE(); // register 0

        uint register = reader.ReadUInt32LE(); // register 1
        byte baseAddressAlignmentShift = ReadAndShiftByte(ref register, 6);
        byte memoryCoherency = ReadAndShiftByte(ref register, 2);
        ushort minLod = ReadAndShiftUInt16(ref register, 12);
        byte dataFormat = ReadAndShiftByte(ref register, 6);
        byte imgNumFormat = ReadAndShiftByte(ref register, 4);
        byte mType0 = ReadAndShiftByte(ref register, 2);

        register = reader.ReadUInt32LE(); // register 2
        ushort width = ReadAndShiftUInt16(ref register, 14);
        ushort height = ReadAndShiftUInt16(ref register, 14);
        byte perfmod = ReadAndShiftByte(ref register, 3);
        byte interlaced = ReadAndShiftByte(ref register, 1);

        register = reader.ReadUInt32LE(); // register 3
        byte channelX = ReadAndShiftByte(ref register, 3);
        byte channelY = ReadAndShiftByte(ref register, 3);
        byte channelZ = ReadAndShiftByte(ref register, 3);
        byte channelW = ReadAndShiftByte(ref register, 3);
        byte baseMipLevel = ReadAndShiftByte(ref register, 4);
        byte lastMipLevel = ReadAndShiftByte(ref register, 4);
        byte tileMode = ReadAndShiftByte(ref register, 5);
        byte pow2Pad = ReadAndShiftByte(ref register, 1);
        byte readOnly = ReadAndShiftByte(ref register, 1);
        byte atc = ReadAndShiftByte(ref register, 1);
        byte textureType = ReadAndShiftByte(ref register, 4);

        register = reader.ReadUInt32LE(); // register 4
        ushort depth = ReadAndShiftUInt16(ref register, 13);
        ushort pitch = ReadAndShiftUInt16(ref register, 14);

        register = reader.ReadUInt32LE(); // register 5
        ushort baseArray = ReadAndShiftUInt16(ref register, 13);
        ushort lastArray = ReadAndShiftUInt16(ref register, 13);

        register = reader.ReadUInt32LE(); // register 6
        ushort minLodWarn = ReadAndShiftUInt16(ref register, 12);
        byte counterBankId = ReadAndShiftByte(ref register, 8);
        byte lodHdwcnten = ReadAndShiftByte(ref register, 1);
        byte compressionEn = ReadAndShiftByte(ref register, 1);
        byte alphaIsOnMsb = ReadAndShiftByte(ref register, 1);
        byte colorTransform = ReadAndShiftByte(ref register, 1);
        byte altTileMode = ReadAndShiftByte(ref register, 1);

        uint textureSize = reader.ReadUInt32LE(); // register 7

        return new GnfTextureHeader
        (
            baseAddress,
            baseAddressAlignmentShift,
            (GnfTextureMemoryCoherency)memoryCoherency,
            minLod,
            (GnmImageDataFormat)dataFormat,
            (GnmImageNumberFormat)imgNumFormat,
            mType0,
            width,
            height,
            perfmod,
            interlaced == 1,
            (GnmChannel)channelX,
            (GnmChannel)channelY,
            (GnmChannel)channelZ,
            (GnmChannel)channelW,
            baseMipLevel,
            lastMipLevel,
            (GnmTileMode)tileMode,
            pow2Pad == 1,
            readOnly == 1,
            atc == 1,
            (GnmTextureType)textureType,
            depth,
            pitch,
            baseArray,
            lastArray,
            minLodWarn,
            counterBankId,
            lodHdwcnten == 1,
            compressionEn == 1,
            alphaIsOnMsb == 1,
            colorTransform == 1,
            altTileMode == 1,
            textureSize
        );

        byte ReadAndShiftByte(ref uint register, int bitLength)
        {
            byte val = (byte)(register & bitLength);
            register >>= bitLength;
            return val;
        }

        ushort ReadAndShiftUInt16(ref uint register, int bitLength)
        {
            ushort val = (ushort)(register & bitLength);
            register >>= bitLength;
            return val;
        }
    }

    public int GetSerializedSize()
        => SIZE;

    public void Serialize(ref BinaryPrimitiveWriter writer)
        => throw new System.NotImplementedException();
}
