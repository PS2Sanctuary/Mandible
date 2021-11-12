using Mandible.Pack2;
using System;
using Xunit;

namespace Mandible.Tests.Pack2Tests;

public class Asset2HeaderTests
{
    private static readonly Asset2Header EXPECTED_HEADER = new(1, 2, 3, Asset2ZipDefinition.Zipped, 4);
    private static readonly byte[] EXPECTED_BYTES = new byte[]
    {
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Name hash
            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Data offset
            0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Data size
            0x01, 0x00, 0x00, 0x00, // Zip status
            0x04, 0x00, 0x00, 0x00 // Data hash
    };

    [Fact]
    public void TestDeserialise()
    {
        Asset2Header header = Asset2Header.Deserialize(EXPECTED_BYTES);
        Assert.Equal(EXPECTED_HEADER, header);
    }

    [Fact]
    public void TestSerialise()
    {
        Span<byte> bytes = stackalloc byte[Asset2Header.Size];
        EXPECTED_HEADER.Serialize(bytes);

        for (int i = 0; i < bytes.Length; i++)
            Assert.Equal(EXPECTED_BYTES[i], bytes[i]);
    }
}
