using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mandible.Dds;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct DdsPixelFormat
{
    public static readonly DdsPixelFormat DXT1 = FromFourCC("DXT1"u8);
    public static readonly DdsPixelFormat DXT2 = FromFourCC("DXT2"u8);
    public static readonly DdsPixelFormat DXT3 = FromFourCC("DXT3"u8);
    public static readonly DdsPixelFormat DXT4 = FromFourCC("DXT4"u8);
    public static readonly DdsPixelFormat DXT5 = FromFourCC("DXT5"u8);

    public uint Size;
    public DdsPixelFormatFlags Flags;
    public uint FourCC;
    public uint RgbBitCount;
    public uint RBitMask;
    public uint GBitMask;
    public uint BBitMask;
    public uint ABitMask;

    private static uint TextToFourCC(ReadOnlySpan<byte> text)
    {
        Debug.Assert(text.Length == 4);
        fixed (byte* ptr = text)
            return *(uint*)ptr;
    }

    private static DdsPixelFormat FromFourCC(ReadOnlySpan<byte> fourCC)
        => new()
        {
            Size = (uint)sizeof(DdsPixelFormat),
            Flags = DdsPixelFormatFlags.DDS_FOURCC,
            FourCC = TextToFourCC(fourCC),
            RgbBitCount = 0,
            RBitMask = 0,
            GBitMask = 0,
            BBitMask = 0,
            ABitMask = 0
        };
}
