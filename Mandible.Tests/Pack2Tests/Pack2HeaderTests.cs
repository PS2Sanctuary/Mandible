using Mandible.Pack2;
using System;
using System.Threading.Tasks;

namespace Mandible.Tests.Pack2Tests;

public class Pack2HeaderTests
{
    private static readonly Pack2Header EXPECTED_HEADER;
    private static readonly byte[] EXPECTED_BYTES =
    [
        0x50, 0x41, 0x4b, // PAK
        0x04, // Version
        0x01, 0x00, 0x00, 0x00, // Asset count
        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Length
        0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Asset map offset
        0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unknown
        0xff // TRUNCATED 'Checksum'
    ];

    static Pack2HeaderTests()
    {
        // Allocate space for the checksum, which we don't have in our hardcoded EXPECTED_BYTES
        byte[] bytes = new byte[Pack2Header.Size];
        Buffer.BlockCopy(EXPECTED_BYTES, 0, bytes, 0, EXPECTED_BYTES.Length);
        EXPECTED_BYTES = bytes;

        byte[] checksumBytes = new byte[128];
        checksumBytes[0] = 0xff;
        EXPECTED_HEADER = new Pack2Header(1, 2, 3, checksumBytes, 4);
    }

    [Test]
    public async Task TestDeserialise()
    {
        Pack2Header header = Pack2Header.Deserialize(EXPECTED_BYTES);

        await Assert.That(header.Version).IsEqualTo(EXPECTED_HEADER.Version);
        await Assert.That(header.AssetCount).IsEqualTo(EXPECTED_HEADER.AssetCount);
        await Assert.That(header.Length).IsEqualTo(EXPECTED_HEADER.Length);
        await Assert.That(header.AssetMapOffset).IsEqualTo(EXPECTED_HEADER.AssetMapOffset);
        await Assert.That(header.Unknown).IsEqualTo(EXPECTED_HEADER.Unknown);
        await Assert.That(header.Checksum).IsEquivalentTo(EXPECTED_HEADER.Checksum);
    }

    [Test]
    public async Task TestSerialise()
    {
        Memory<byte> bytes = new byte[Pack2Header.Size];
        EXPECTED_HEADER.Serialize(bytes.Span);
        await Assert.That(bytes).IsEquivalentTo(EXPECTED_BYTES);
    }
}
