using Mandible.Pack;
using System;
using Xunit;

namespace Mandible.Tests.PackTests;

public class PackChunkHeaderTests
{
    private static readonly PackChunkHeader EXPECTED_HEADER;
    private static readonly byte[] EXPECTED_BYTES = new byte[]
    {
        0x00, 0x00, 0x00, 0x01, // Next chunk header
        0x00, 0x00, 0x00, 0x02, // Asset count
    };

    static PackChunkHeaderTests()
    {
        EXPECTED_HEADER = new PackChunkHeader(1, 2);
    }

    [Fact]
    public void TestDeserialise()
    {
        PackChunkHeader header = PackChunkHeader.Deserialize(EXPECTED_BYTES);

        Assert.Equal(EXPECTED_HEADER.NextChunkOffset, header.NextChunkOffset);
        Assert.Equal(EXPECTED_HEADER.AssetCount, header.AssetCount);
    }

    [Fact]
    public void TestSerialise()
    {
        Span<byte> bytes = stackalloc byte[PackChunkHeader.Size];
        EXPECTED_HEADER.Serialize(bytes);

        for (int i = 0; i < bytes.Length; i++)
            Assert.Equal(EXPECTED_BYTES[i], bytes[i]);
    }
}
