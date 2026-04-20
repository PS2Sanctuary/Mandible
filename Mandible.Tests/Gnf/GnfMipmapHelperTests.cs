using Mandible.Gnf;
using Xunit;

namespace Mandible.Tests.Gnf;

public class GnfMipmapHelperTests
{
    [Fact]
    public void TestCalculateMipmapCount()
    {
        Assert.Equal(10, GnfMipmapHelper.CalculateMipmapCount(512, 256));
        Assert.Equal(11, GnfMipmapHelper.CalculateMipmapCount(512, 256, 1024));
    }

    [Fact]
    public void TestGetMipmapSize2D()
    {
        (int Width, int Height) size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 0);
        Assert.Equal(512, size.Width);
        Assert.Equal(256, size.Height);

        size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 1);
        Assert.Equal(256, size.Width);
        Assert.Equal(128, size.Height);

        size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 4);
        Assert.Equal(32, size.Width);
        Assert.Equal(16, size.Height);

        size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 9);
        Assert.Equal(1, size.Width);
        Assert.Equal(1, size.Height);
    }

    [Fact]
    public void TestGetMipmapSize3D()
    {
        (int Width, int Height, int Depth) size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 0);
        Assert.Equal(512, size.Width);
        Assert.Equal(256, size.Height);
        Assert.Equal(1024, size.Depth);

        size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 1);
        Assert.Equal(256, size.Width);
        Assert.Equal(128, size.Height);
        Assert.Equal(512, size.Depth);

        size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 4);
        Assert.Equal(32, size.Width);
        Assert.Equal(16, size.Height);
        Assert.Equal(64, size.Depth);

        size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 10);
        Assert.Equal(1, size.Width);
        Assert.Equal(1, size.Height);
        Assert.Equal(1, size.Depth);
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
