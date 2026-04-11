#pragma endian little

using byte = u8;
using uint = u32;

enum GnmChannel : u8
{
    GNM_CHAN_CONSTANT0 = 0x0,
    GNM_CHAN_CONSTANT1 = 0x1,
    GNM_CHAN_X = 0x4,
    GNM_CHAN_Y = 0x5,
    GNM_CHAN_Z = 0x6,
    GNM_CHAN_W = 0x7
};

enum GnmImageFormat : u8
{
    GNM_IMG_DATA_FORMAT_INVALID = 0x0,
    GNM_IMG_DATA_FORMAT_8 = 0x1,
    GNM_IMG_DATA_FORMAT_16 = 0x2,
    GNM_IMG_DATA_FORMAT_8_8 = 0x3,
    GNM_IMG_DATA_FORMAT_32 = 0x4,
    GNM_IMG_DATA_FORMAT_16_16 = 0x5,
    GNM_IMG_DATA_FORMAT_10_11_11 = 0x6,
    GNM_IMG_DATA_FORMAT_11_11_10 = 0x7,
    GNM_IMG_DATA_FORMAT_10_10_10_2 = 0x8,
    GNM_IMG_DATA_FORMAT_2_10_10_10 = 0x9,
    GNM_IMG_DATA_FORMAT_8_8_8_8 = 0xa,
    GNM_IMG_DATA_FORMAT_32_32 = 0xb,
    GNM_IMG_DATA_FORMAT_16_16_16_16 = 0xc,
    GNM_IMG_DATA_FORMAT_32_32_32 = 0xd,
    GNM_IMG_DATA_FORMAT_32_32_32_32 = 0xe,
    GNM_IMG_DATA_FORMAT_5_6_5 = 0x10,
    GNM_IMG_DATA_FORMAT_1_5_5_5 = 0x11,
    GNM_IMG_DATA_FORMAT_5_5_5_1 = 0x12,
    GNM_IMG_DATA_FORMAT_4_4_4_4 = 0x13,
    GNM_IMG_DATA_FORMAT_8_24 = 0x14,
    GNM_IMG_DATA_FORMAT_24_8 = 0x15,
    GNM_IMG_DATA_FORMAT_X24_8_32 = 0x16,
    GNM_IMG_DATA_FORMAT_GB_GR = 0x20,
    GNM_IMG_DATA_FORMAT_BG_RG = 0x21,
    GNM_IMG_DATA_FORMAT_5_9_9_9 = 0x22,
    GNM_IMG_DATA_FORMAT_BC1 = 0x23,
    GNM_IMG_DATA_FORMAT_BC2 = 0x24,
    GNM_IMG_DATA_FORMAT_BC3 = 0x25,
    GNM_IMG_DATA_FORMAT_BC4 = 0x26,
    GNM_IMG_DATA_FORMAT_BC5 = 0x27,
    GNM_IMG_DATA_FORMAT_BC6 = 0x28,
    GNM_IMG_DATA_FORMAT_BC7 = 0x29,
    GNM_IMG_DATA_FORMAT_FMASK8_S2_F1 = 0x2c,
    GNM_IMG_DATA_FORMAT_FMASK8_S4_F1 = 0x2d,
    GNM_IMG_DATA_FORMAT_FMASK8_S8_F1 = 0x2e,
    GNM_IMG_DATA_FORMAT_FMASK8_S2_F2 = 0x2f,
    GNM_IMG_DATA_FORMAT_FMASK8_S4_F2 = 0x30,
    GNM_IMG_DATA_FORMAT_FMASK8_S4_F4 = 0x31,
    GNM_IMG_DATA_FORMAT_FMASK16_S16_F1 = 0x32,
    GNM_IMG_DATA_FORMAT_FMASK16_S8_F2 = 0x33,
    GNM_IMG_DATA_FORMAT_FMASK32_S16_F2 = 0x34,
    GNM_IMG_DATA_FORMAT_FMASK32_S8_F4 = 0x35,
    GNM_IMG_DATA_FORMAT_FMASK32_S8_F8 = 0x36,
    GNM_IMG_DATA_FORMAT_FMASK64_S16_F4 = 0x37,
    GNM_IMG_DATA_FORMAT_FMASK64_S16_F8 = 0x38,
    GNM_IMG_DATA_FORMAT_4_4 = 0x39,
    GNM_IMG_DATA_FORMAT_6_5_5 = 0x3a,
    GNM_IMG_DATA_FORMAT_1 = 0x3b,
    GNM_IMG_DATA_FORMAT_1_REVERSED = 0x3c,
    GNM_IMG_DATA_FORMAT_32_AS_8 = 0x3d,
    GNM_IMG_DATA_FORMAT_32_AS_8_8 = 0x3e,
    GNM_IMG_DATA_FORMAT_32_AS_32_32_32_32 = 0x3f
};

enum GnmImgNumFormat : u8
{
    GNM_IMG_NUM_FORMAT_UNORM = 0x0,
    GNM_IMG_NUM_FORMAT_SNORM = 0x1,
    GNM_IMG_NUM_FORMAT_USCALED = 0x2,
    GNM_IMG_NUM_FORMAT_SSCALED = 0x3,
    GNM_IMG_NUM_FORMAT_UINT = 0x4,
    GNM_IMG_NUM_FORMAT_SINT = 0x5,
    GNM_IMG_NUM_FORMAT_SNORM_OGL = 0x6,
    GNM_IMG_NUM_FORMAT_FLOAT = 0x7,
    GNM_IMG_NUM_FORMAT_SRGB = 0x9,
    GNM_IMG_NUM_FORMAT_UBNORM = 0xa,
    GNM_IMG_NUM_FORMAT_UBNORM_OGL = 0xb,
    GNM_IMG_NUM_FORMAT_UBINT = 0xc,
    GNM_IMG_NUM_FORMAT_UBSCALED = 0xd
};

enum GnmTextureType : u8
{
    GNM_TEXTURE_1D = 0x8,
    GNM_TEXTURE_2D = 0x9,
    GNM_TEXTURE_3D = 0xa,
    GNM_TEXTURE_CUBEMAP = 0xb,
    GNM_TEXTURE_1D_ARRAY = 0xc,
    GNM_TEXTURE_2D_ARRAY = 0xd,
    GNM_TEXTURE_2D_MSAA = 0xe,
    GNM_TEXTURE_2D_ARRAY_MSAA = 0xf,
};

enum GnmTileMode : u8
{
    GNM_TM_DEPTH_2D_THIN_64 = 0x0,
    GNM_TM_DEPTH_2D_THIN_128 = 0x1,
    GNM_TM_DEPTH_2D_THIN_256 = 0x2,
    GNM_TM_DEPTH_2D_THIN_512 = 0x3,
    GNM_TM_DEPTH_2D_THIN_1K = 0x4,
    GNM_TM_DEPTH_1D_THIN = 0x5,
    GNM_TM_DEPTH_2D_THIN_PRT_256 = 0x6,
    GNM_TM_DEPTH_2D_THIN_PRT_1K = 0x7,

    GNM_TM_DISPLAY_LINEAR_ALIGNED = 0x8,
    GNM_TM_DISPLAY_1D_THIN = 0x9,
    GNM_TM_DISPLAY_2D_THIN = 0xa,
    GNM_TM_DISPLAY_THIN_PRT = 0xb,
    GNM_TM_DISPLAY_2D_THIN_PRT = 0xc,

    GNM_TM_THIN_1D_THIN = 0xd,
    GNM_TM_THIN_2D_THIN = 0xe,
    GNM_TM_THIN_3D_THIN = 0xf,
    GNM_TM_THIN_THIN_PRT = 0x10,
    GNM_TM_THIN_2D_THIN_PRT = 0x11,
    GNM_TM_THIN_3D_THIN_PRT = 0x12,

    GNM_TM_THICK_1D_THICK = 0x13,
    GNM_TM_THICK_2D_THICK = 0x14,
    GNM_TM_THICK_3D_THICK = 0x15,
    GNM_TM_THICK_THICK_PRT = 0x16,
    GNM_TM_THICK_2D_THICK_PRT = 0x17,
    GNM_TM_THICK_3D_THICK_PRT = 0x18,
    GNM_TM_THICK_2D_XTHICK = 0x19,
    GNM_TM_THICK_3D_XTHICK = 0x1a,

    GNM_TM_DISPLAY_LINEAR_GENERAL = 0x1f,
};

struct GnfUserData
{
    char Magic[4]; // "USER", not guaranteed to be written
    uint DataSize;
    byte Data[DataSize];
};

bitfield GnfTextureHeader
{
    // -- Register 0 --
    // Offset by sizeof(GnfHeader) + GnfHeader#ContentsSize
    // Generally 256-byte aligned, but confirm with `BaseAddressAlignment`
    uint BaseAddress;
    
    // -- Register 1 --
    // Left-shift `1` by this value to get the texture offset alignment
    // (i.e. baseAddressAlignment = 1 << BaseAddressAlignmentShift)
    BaseAddressAlignmentShift : 6;
    // 0x0 - no coherency
    // 0x1 - GPU coherent
    // 0x2 - System coherent
    // 0x3 - Both GPU and System coherent
    MTypeL2 : 2;
    // Minimum level of detail
    MinLod : 12;
    // Surface format
    GnmDataFormat : 6;
    // Channel type
    GnmImgNumFormat : 4;
    // Memory type, controls cache behaviour. If enabled, uses L1 cache LRU
    MType0 : 2;
    
    // -- Register 2 --
    Width : 14;
    Height : 14;
    Perfmod : 3;
    Interlaced : 1;
    
    // -- Register 3 --
    // Channels X to W
    GnmChannel_X : 3;
    GnmChannel_Y : 3;
    GnmChannel_Z : 3;
    GnmChannel_W : 3;
    // Base mip level
    BaseMipLevel : 4;
    // Last mip level, or number of fragments / samples for MSAA
    LastMipLevel : 4;
    GnmTileMode : 5;
    // If true, memory footprint is padded to pow2 dimensions
    Pow2Pad : 1;
    // If true, texture is read-only
    MType2 : 1;
    Atc : 1;
    GnmTextureType : 4;
    
    // -- Register 4 --
    Depth : 13;
    Pitch : 14;
    Unused : 5;
    
    // -- Register 5 --
    BaseArray : 13;
    LastArray : 13;
    Unused2 : 6;
    
    // -- Register 6 --
    MinLodWarn : 12;
    CounterBankId : 8;
    LodHdwcnten : 1;
    // GFX8-only attributes
    CompressionEn : 1;
    // If true, the alpha is on the most significant bit
    AlphaIsOnMsb : 1;
    ColorTransform : 1;
    // NEO-only attribute
    AltTileMode : 1;
    Unused3 : 7;
    
    // -- Register 7 --
    uint TextureSize;
};

struct GnfContents
{
    byte Version;
    byte NumTextures;
    byte Alignment;
    byte Reserved;
    uint StreamSize; // The length of the entire file
    GnfTextureHeader Textures[NumTextures];
};

struct GnfHeader
{
    char Magic[4]; // "GNF "
    uint ContentsSize;
};

struct Gnf
{
    GnfHeader Header;
    GnfContents Contents;
    GnfUserData UserData;
};

Gnf main @ 0x00;