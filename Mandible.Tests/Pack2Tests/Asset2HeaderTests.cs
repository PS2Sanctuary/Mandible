using Mandible.Pack2;
using System;
using System.Threading.Tasks;

namespace Mandible.Tests.Pack2Tests;

public class Asset2HeaderTests
{
    private static readonly Asset2Header EXPECTED_HEADER = new(1, 2, 3, Asset2ZipDefinition.ZippedAlternate, 4);
    private static readonly byte[] EXPECTED_BYTES =
    [
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Name hash
            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Data offset
            0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Data size
            0x01, 0x00, 0x00, 0x00, // Zip status
            0x04, 0x00, 0x00, 0x00 // Data hash
    ];

    [Test]
    public async Task TestDeserialise()
    {
        Asset2Header header = Asset2Header.Deserialize(EXPECTED_BYTES);
        await Assert.That(header.NameHash).IsEqualTo(EXPECTED_HEADER.NameHash);
        await Assert.That(header.DataOffset).IsEqualTo(EXPECTED_HEADER.DataOffset);
        await Assert.That(header.DataSize).IsEqualTo(EXPECTED_HEADER.DataSize);
        await Assert.That(header.ZipStatus).IsEqualTo(EXPECTED_HEADER.ZipStatus);
        await Assert.That(header.DataHash).IsEqualTo(EXPECTED_HEADER.DataHash);
    }

    [Test]
    public async Task TestSerialise()
    {
        Memory<byte> bytes = new byte[Asset2Header.Size];
        EXPECTED_HEADER.Serialize(bytes.Span);
        await Assert.That(bytes).IsEquivalentTo(EXPECTED_BYTES);
    }
}
