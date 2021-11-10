using Mandible.Pack2;
using System;
using Xunit;

namespace Mandible.Tests.Pack2Tests;

public class Asset2HeaderTests
{
    private static readonly Asset2Header EXPECTED_HEADER = new(1, 2, 3, AssetZipDefinition.Zipped, 4);
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
        Assert.Equal(header, EXPECTED_HEADER);
    }

    [Fact]
    public void TestSerialise()
    {
        Span<byte> bytes = stackalloc byte[Asset2Header.Size];
        EXPECTED_HEADER.Serialize(bytes);

        Assert.Equal(bytes.ToArray(), EXPECTED_BYTES);
    }
}
