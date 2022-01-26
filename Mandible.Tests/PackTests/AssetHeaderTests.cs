using Mandible.Pack;
using System;
using Xunit;

namespace Mandible.Tests.PackTests;

public class AssetHeaderTests
{
    private static readonly AssetHeader EXPECTED_HEADER = new(1, "1", 2, 3, 4);
    private static readonly byte[] EXPECTED_BYTES = new byte[]
    {
        0x00, 0x00, 0x00, 0x01, // Name length
        (byte)'1', // Name
        0x00, 0x00, 0x00, 0x02, // Data offset
        0x00, 0x00, 0x00, 0x03, // Data length
        0x00, 0x00, 0x00, 0x04 // Checksum
    };

    [Fact]
    public void TestDeserialise()
    {
        Assert.True(AssetHeader.TryDeserialize(EXPECTED_BYTES, out AssetHeader? header));

        Assert.Equal(EXPECTED_HEADER.Checksum, header!.Checksum);
        Assert.Equal(EXPECTED_HEADER.DataLength, header.DataLength);
        Assert.Equal(EXPECTED_HEADER.DataOffset, header.DataOffset);
        Assert.Equal(EXPECTED_HEADER.Name, header.Name);
    }

    [Fact]
    public void TestSerialise()
    {
        Span<byte> bytes = stackalloc byte[AssetHeader.GetSize(EXPECTED_HEADER.Name)];
        EXPECTED_HEADER.Serialize(bytes);

        for (int i = 0; i < bytes.Length; i++)
            Assert.Equal(EXPECTED_BYTES[i], bytes[i]);
    }
}
