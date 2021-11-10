using Mandible.Pack;
using System;
using System.Collections.Generic;
using Xunit;

namespace Mandible.Tests.PackTests;

public class PackChunkHeaderTests
{
    private static readonly PackChunkHeader EXPECTED_HEADER;
    private static readonly byte[] EXPECTED_BYTES = new byte[]
    {
        0x00, 0x00, 0x00, 0x01, // Next chunk header
        0x00, 0x00, 0x00, 0x02, // Asset count
        // Asset headers
        0x00, 0x00, 0x00, 0x01, // Name length
        (byte)'1', // Name
        0x00, 0x00, 0x00, 0x02, // Data offset
        0x00, 0x00, 0x00, 0x03, // Data length
        0x00, 0x00, 0x00, 0x04, // Checksum
        0x00, 0x00, 0x00, 0x01, // Name length
        (byte)'2', // Name
        0x00, 0x00, 0x00, 0x02, // Data offset
        0x00, 0x00, 0x00, 0x03, // Data length
        0x00, 0x00, 0x00, 0x04 // Checksum
    };

    static PackChunkHeaderTests()
    {
        List<AssetHeader> assetHeaders = new()
        {
            new AssetHeader("1", 2, 3, 4),
            new AssetHeader("2", 2, 3, 4)
        };
        EXPECTED_HEADER = new PackChunkHeader(1, assetHeaders);
    }

    [Fact]
    public void TestDeserialise()
    {
        PackChunkHeader header = PackChunkHeader.Deserialize(EXPECTED_BYTES);

        Assert.Equal(EXPECTED_HEADER.NextChunkOffset, header.NextChunkOffset);
        Assert.Equal(EXPECTED_HEADER.AssetCount, header.AssetCount);

        for (int i = 0; i < header.AssetCount; i++)
            Assert.Equal(EXPECTED_HEADER.AssetHeaders[i].Name, header.AssetHeaders[i].Name);
    }

    [Fact]
    public void TestSerialise()
    {
        Span<byte> bytes = stackalloc byte[PackChunkHeader.GetSize(EXPECTED_HEADER.AssetHeaders)];
        EXPECTED_HEADER.Serialize(bytes);

        for (int i = 0; i < bytes.Length; i++)
            Assert.Equal(EXPECTED_BYTES[i], bytes[i]);
    }
}
