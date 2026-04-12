namespace Mandible.Gnf;

public enum GnmChannel : byte
{
    Constant0 = 0x0,
    Constant1 = 0x1,
    X = 0x4,
    Y = 0x5,
    Z = 0x6,
    W = 0x7
};

public enum GnmImageDataFormat : byte
{
    FORMAT_INVALID = 0x0,
    FORMAT_8 = 0x1,
    FORMAT_16 = 0x2,
    FORMAT_8_8 = 0x3,
    FORMAT_32 = 0x4,
    FORMAT_16_16 = 0x5,
    FORMAT_10_11_11 = 0x6,
    FORMAT_11_11_10 = 0x7,
    FORMAT_10_10_10_2 = 0x8,
    FORMAT_2_10_10_10 = 0x9,
    FORMAT_8_8_8_8 = 0xa,
    FORMAT_32_32 = 0xb,
    FORMAT_16_16_16_16 = 0xc,
    FORMAT_32_32_32 = 0xd,
    FORMAT_32_32_32_32 = 0xe,
    FORMAT_5_6_5 = 0x10,
    FORMAT_1_5_5_5 = 0x11,
    FORMAT_5_5_5_1 = 0x12,
    FORMAT_4_4_4_4 = 0x13,
    FORMAT_8_24 = 0x14,
    FORMAT_24_8 = 0x15,
    FORMAT_X24_8_32 = 0x16,
    FORMAT_GB_GR = 0x20,
    FORMAT_BG_RG = 0x21,
    FORMAT_5_9_9_9 = 0x22,
    FORMAT_BC1 = 0x23,
    FORMAT_BC2 = 0x24,
    FORMAT_BC3 = 0x25,
    FORMAT_BC4 = 0x26,
    FORMAT_BC5 = 0x27,
    FORMAT_BC6 = 0x28,
    FORMAT_BC7 = 0x29,
    FORMAT_FMASK8_S2_F1 = 0x2c,
    FORMAT_FMASK8_S4_F1 = 0x2d,
    FORMAT_FMASK8_S8_F1 = 0x2e,
    FORMAT_FMASK8_S2_F2 = 0x2f,
    FORMAT_FMASK8_S4_F2 = 0x30,
    FORMAT_FMASK8_S4_F4 = 0x31,
    FORMAT_FMASK16_S16_F1 = 0x32,
    FORMAT_FMASK16_S8_F2 = 0x33,
    FORMAT_FMASK32_S16_F2 = 0x34,
    FORMAT_FMASK32_S8_F4 = 0x35,
    FORMAT_FMASK32_S8_F8 = 0x36,
    FORMAT_FMASK64_S16_F4 = 0x37,
    FORMAT_FMASK64_S16_F8 = 0x38,
    FORMAT_4_4 = 0x39,
    FORMAT_6_5_5 = 0x3a,
    FORMAT_1 = 0x3b,
    FORMAT_1_REVERSED = 0x3c,
    FORMAT_32_AS_8 = 0x3d,
    FORMAT_32_AS_8_8 = 0x3e,
    FORMAT_32_AS_32_32_32_32 = 0x3f
}

public enum GnmImageNumberFormat : byte
{
    UNORM = 0x0,
    SNORM = 0x1,
    USCALED = 0x2,
    SSCALED = 0x3,
    UINT = 0x4,
    SINT = 0x5,
    SNORM_OGL = 0x6,
    FLOAT = 0x7,
    SRGB = 0x9,
    UBNORM = 0xa,
    UBNORM_OGL = 0xb,
    UBINT = 0xc,
    UBSCALED = 0xd
};

public enum GnmTextureType : byte
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

public enum GnmTileMode : byte
{
    DEPTH_2D_THIN_64 = 0x0,
    DEPTH_2D_THIN_128 = 0x1,
    DEPTH_2D_THIN_256 = 0x2,
    DEPTH_2D_THIN_512 = 0x3,
    DEPTH_2D_THIN_1K = 0x4,
    DEPTH_1D_THIN = 0x5,
    DEPTH_2D_THIN_PRT_256 = 0x6,
    DEPTH_2D_THIN_PRT_1K = 0x7,

    DISPLAY_LINEAR_ALIGNED = 0x8,
    DISPLAY_1D_THIN = 0x9,
    DISPLAY_2D_THIN = 0xa,
    DISPLAY_THIN_PRT = 0xb,
    DISPLAY_2D_THIN_PRT = 0xc,

    THIN_1D_THIN = 0xd,
    THIN_2D_THIN = 0xe,
    THIN_3D_THIN = 0xf,
    THIN_THIN_PRT = 0x10,
    THIN_2D_THIN_PRT = 0x11,
    THIN_3D_THIN_PRT = 0x12,

    THICK_1D_THICK = 0x13,
    THICK_2D_THICK = 0x14,
    THICK_3D_THICK = 0x15,
    THICK_THICK_PRT = 0x16,
    THICK_2D_THICK_PRT = 0x17,
    THICK_3D_THICK_PRT = 0x18,
    THICK_2D_XTHICK = 0x19,
    THICK_3D_XTHICK = 0x1a,

    DISPLAY_LINEAR_GENERAL = 0x1f,
};
