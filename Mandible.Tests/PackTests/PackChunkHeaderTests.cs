using Mandible.Pack;
using System;
using System.Threading.Tasks;

namespace Mandible.Tests.PackTests;

public class PackChunkHeaderTests
{
    private static readonly PackChunkHeader EXPECTED_HEADER;
    private static readonly byte[] EXPECTED_BYTES =
    [
        0x00, 0x00, 0x00, 0x01, // Next chunk header
        0x00, 0x00, 0x00, 0x02 // Asset count
    ];

    static PackChunkHeaderTests()
    {
        EXPECTED_HEADER = new PackChunkHeader(1, 2);
    }

    [Test]
    public async Task TestDeserialise()
    {
        PackChunkHeader header = PackChunkHeader.Deserialize(EXPECTED_BYTES);

        await Assert.That(header.NextChunkOffset).IsEqualTo(EXPECTED_HEADER.NextChunkOffset);
        await Assert.That(header.AssetCount).IsEqualTo(EXPECTED_HEADER.AssetCount);
    }

    [Test]
    public async Task TestSerialise()
    {
        Memory<byte> bytes = new byte[PackChunkHeader.Size];
        EXPECTED_HEADER.Serialize(bytes.Span);
        await Assert.That(bytes).IsEquivalentTo(EXPECTED_BYTES);
    }
}
