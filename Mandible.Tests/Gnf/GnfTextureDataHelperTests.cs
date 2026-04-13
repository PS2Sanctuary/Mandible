using Mandible.Gnf;
using Xunit;

namespace Mandible.Tests.Gnf;

public class GnfTextureDataHelperTests
{
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

        (int StartOffset, int Length)[] offsets = GnfTextureDataHelper.GetMipmapOffsets(texHead);
        Assert.Equal(10, offsets.Length);

        Assert.Equal(0, offsets[0].StartOffset);
        Assert.Equal(131072, offsets[0].Length);

        Assert.Equal(131072, offsets[1].StartOffset);
        Assert.Equal(32768, offsets[1].Length);

        Assert.Equal(163480, offsets[2].StartOffset);
        Assert.Equal(8192, offsets[2].Length);
    }
}
