using Mandible.Pack2;
using System;
using Xunit;

namespace Mandible.Tests.Pack2Tests;

public class Pack2HeaderTests
{
    private static readonly Pack2Header EXPECTED_HEADER;
    private static readonly byte[] EXPECTED_BYTES =
    {
            0x50, 0x41, 0x4b, // PAK
            0x04, // Version
            0x01, 0x00, 0x00, 0x00, // Asset count
            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Length
            0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Asset map offset
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unknown
            0xff // 'Checksum'
    };

    static Pack2HeaderTests()
    {
        byte[] bytes = new byte[Pack2Header.Size];
        Buffer.BlockCopy(EXPECTED_BYTES, 0, bytes, 0, EXPECTED_BYTES.Length);
        EXPECTED_BYTES = bytes;

        byte[] checksumBytes = new byte[128];
        checksumBytes[0] = 0xff;
        EXPECTED_HEADER = new Pack2Header(1, 2, 3, checksumBytes, 4);
    }

    [Fact]
    public void TestDeserialise()
    {
        Pack2Header header = Pack2Header.Deserialize(EXPECTED_BYTES);

        Assert.Equal(EXPECTED_HEADER.Version, header.Version);
        Assert.Equal(EXPECTED_HEADER.AssetCount, header.AssetCount);
        Assert.Equal(EXPECTED_HEADER.Length, header.Length);
        Assert.Equal(EXPECTED_HEADER.AssetMapOffset, header.AssetMapOffset);
        Assert.Equal(EXPECTED_HEADER.Unknown, header.Unknown);

        Assert.Equal(EXPECTED_HEADER.Checksum.Length, header.Checksum.Length);
        for (int i = 0; i < header.Checksum.Length; i++)
            Assert.Equal(EXPECTED_HEADER.Checksum.Span[i], header.Checksum.Span[i]);
    }

    [Fact]
    public void TestSerialise()
    {
        Span<byte> bytes = stackalloc byte[Pack2Header.Size];
        EXPECTED_HEADER.Serialize(bytes);

        for (int i = 0; i < bytes.Length; i++)
            Assert.Equal(EXPECTED_BYTES[i], bytes[i]);
    }
}
