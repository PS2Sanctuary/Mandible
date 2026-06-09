using Mandible.Gnf;
using System.Threading.Tasks;

namespace Mandible.Tests.Gnf;

public class GnfMipmapHelperTests
{
    [Test]
    public async Task TestCalculateMipmapCount()
    {
        await Assert.That(GnfMipmapHelper.CalculateMipmapCount(512, 256)).IsEqualTo(10);
        await Assert.That(GnfMipmapHelper.CalculateMipmapCount(512, 256, 1024)).IsEqualTo(11);
    }

    [Test]
    public async Task TestGetMipmapSize2D()
    {
        (int Width, int Height) size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 0);
        await Assert.That(size.Width).IsEqualTo(512);
        await Assert.That(size.Height).IsEqualTo(256);

        size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 1);
        await Assert.That(size.Width).IsEqualTo(256);
        await Assert.That(size.Height).IsEqualTo(128);

        size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 4);
        await Assert.That(size.Width).IsEqualTo(32);
        await Assert.That(size.Height).IsEqualTo(16);

        size = GnfMipmapHelper.GetMipmapSize2D(512, 256, 9);
        await Assert.That(size.Width).IsEqualTo(1);
        await Assert.That(size.Height).IsEqualTo(1);
    }

    [Test]
    public async Task TestGetMipmapSize3D()
    {
        (int Width, int Height, int Depth) size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 0);
        await Assert.That(size.Width).IsEqualTo(512);
        await Assert.That(size.Height).IsEqualTo(256);
        await Assert.That(size.Depth).IsEqualTo(1024);

        size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 1);
        await Assert.That(size.Width).IsEqualTo(256);
        await Assert.That(size.Height).IsEqualTo(128);
        await Assert.That(size.Depth).IsEqualTo(512);

        size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 4);
        await Assert.That(size.Width).IsEqualTo(32);
        await Assert.That(size.Height).IsEqualTo(16);
        await Assert.That(size.Depth).IsEqualTo(64);

        size = GnfMipmapHelper.GetMipmapSize3D(512, 256, 1024, 10);
        await Assert.That(size.Width).IsEqualTo(1);
        await Assert.That(size.Height).IsEqualTo(1);
        await Assert.That(size.Depth).IsEqualTo(1);
    }

    [Test]
    public async Task TestGetMipmapOffsets()
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
        await Assert.That(offsets.Length).IsEqualTo(10);

        await Assert.That(offsets[0].StartOffset).IsEqualTo(0);
        await Assert.That(offsets[0].Length).IsEqualTo(131072);
        await Assert.That(offsets[1].StartOffset).IsEqualTo(131072);
        await Assert.That(offsets[1].Length).IsEqualTo(32768);
        await Assert.That(offsets[2].StartOffset).IsEqualTo(163840);
        await Assert.That(offsets[2].Length).IsEqualTo(8192);
        await Assert.That(offsets[3].StartOffset).IsEqualTo(172032);
        await Assert.That(offsets[3].Length).IsEqualTo(2048);
        await Assert.That(offsets[4].StartOffset).IsEqualTo(174080);
        await Assert.That(offsets[4].Length).IsEqualTo(512);
        await Assert.That(offsets[5].StartOffset).IsEqualTo(175104);
        await Assert.That(offsets[5].Length).IsEqualTo(128);
        await Assert.That(offsets[6].StartOffset).IsEqualTo(176128);
        await Assert.That(offsets[6].Length).IsEqualTo(32);
        await Assert.That(offsets[7].StartOffset).IsEqualTo(177152);
        await Assert.That(offsets[7].Length).IsEqualTo(16);
        await Assert.That(offsets[8].StartOffset).IsEqualTo(178176);
        await Assert.That(offsets[8].Length).IsEqualTo(16);
        await Assert.That(offsets[9].StartOffset).IsEqualTo(179200);
        await Assert.That(offsets[9].Length).IsEqualTo(16);
    }
}
