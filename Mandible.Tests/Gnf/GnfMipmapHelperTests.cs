using Mandible.Gnf;
using Xunit;

namespace Mandible.Tests.Gnf;

public class GnfMipmapHelperTests
{
    [Fact]
    public void TestCalculateMipmapCount()
    {
        Assert.Equal(10, GnfMipmapHelper.CalculateMipmapCount(512, 256));
    }

    [Fact]
    public void TestGetMipmapSizes()
    {
        GnfTextureHeader texHead = new()
        {
            BaseAddress = 0,
            MipAlignmentShift = 8,
            BaseMipLevel = 0,
            LastMipLevel = 9,
            DataFormat = GnmImageDataFormat.FORMAT_BC3,
            Width = 512,
            Height = 256
        };

        (int Width, int Height)[] sizes = GnfMipmapHelper.GetMipmapSizes(texHead);
        Assert.Equal(10, sizes.Length);

        Assert.Equal(512, sizes[0].Width);
        Assert.Equal(256, sizes[0].Height);
        Assert.Equal(256, sizes[1].Width);
        Assert.Equal(128, sizes[1].Height);
        Assert.Equal(128, sizes[2].Width);
        Assert.Equal(64, sizes[2].Height);
        Assert.Equal(64, sizes[3].Width);
        Assert.Equal(32, sizes[3].Height);
        Assert.Equal(32, sizes[4].Width);
        Assert.Equal(16, sizes[4].Height);
        Assert.Equal(16, sizes[5].Width);
        Assert.Equal(8, sizes[5].Height);
        Assert.Equal(8, sizes[6].Width);
        Assert.Equal(4, sizes[6].Height);
        Assert.Equal(4, sizes[7].Width);
        Assert.Equal(2, sizes[7].Height);
        Assert.Equal(2, sizes[8].Width);
        Assert.Equal(1, sizes[8].Height);
        Assert.Equal(1, sizes[9].Width);
        Assert.Equal(1, sizes[9].Height);
    }

    [Fact]
    public void TestGetMipmapOffsets()
    {
        GnfTextureHeader texHead = new()
        {
            BaseAddress = 0,
            MipAlignmentShift = 8,
            BaseMipLevel = 0,
            LastMipLevel = 9,
            DataFormat = GnmImageDataFormat.FORMAT_BC3,
            Width = 512,
            Height = 256
        };

        (int StartOffset, int Length)[] offsets = GnfMipmapHelper.GetMipmapOffsets(texHead);
        Assert.Equal(10, offsets.Length);

        Assert.Equal(0, offsets[0].StartOffset);
        Assert.Equal(131072, offsets[0].Length);
        Assert.Equal(131072, offsets[1].StartOffset);
        Assert.Equal(32768, offsets[1].Length);
        Assert.Equal(163840, offsets[2].StartOffset);
        Assert.Equal(8192, offsets[2].Length);
        Assert.Equal(172032, offsets[3].StartOffset);
        Assert.Equal(2048, offsets[3].Length);
        Assert.Equal(174080, offsets[4].StartOffset);
        Assert.Equal(512, offsets[4].Length);
        Assert.Equal(175104, offsets[5].StartOffset);
        Assert.Equal(128, offsets[5].Length);
        Assert.Equal(176128, offsets[6].StartOffset);
        Assert.Equal(32, offsets[6].Length);
        Assert.Equal(177152, offsets[7].StartOffset);
        Assert.Equal(16, offsets[7].Length);
        Assert.Equal(178176, offsets[8].StartOffset);
        Assert.Equal(16, offsets[8].Length);
        Assert.Equal(179200, offsets[9].StartOffset);
        Assert.Equal(16, offsets[9].Length);
    }
}
