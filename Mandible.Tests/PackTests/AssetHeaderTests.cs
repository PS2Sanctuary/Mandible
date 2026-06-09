using Mandible.Pack;
using System;
using System.Threading.Tasks;

namespace Mandible.Tests.PackTests;

public class AssetHeaderTests
{
    private static readonly AssetHeader EXPECTED_HEADER = new(1, "1", 2, 3, 4);
    private static readonly byte[] EXPECTED_BYTES =
    [
        0x00, 0x00, 0x00, 0x01, // Name length
        (byte)'1', // Name
        0x00, 0x00, 0x00, 0x02, // Data offset
        0x00, 0x00, 0x00, 0x03, // Data length
        0x00, 0x00, 0x00, 0x04 // Checksum
    ];

    [Test]
    public async Task TestDeserialise()
    {
        await Assert.That(AssetHeader.TryDeserialize(EXPECTED_BYTES, out AssetHeader? header)).IsTrue();

        await Assert.That(header!.Checksum).IsEqualTo(EXPECTED_HEADER.Checksum);
        await Assert.That(header.DataLength).IsEqualTo(EXPECTED_HEADER.DataLength);
        await Assert.That(header.DataOffset).IsEqualTo(EXPECTED_HEADER.DataOffset);
        await Assert.That(header.Name).IsEqualTo(EXPECTED_HEADER.Name);
    }

    [Test]
    public async Task TestSerialise()
    {
        Memory<byte> bytes = new byte[AssetHeader.GetSize(EXPECTED_HEADER.Name)];
        EXPECTED_HEADER.Serialize(bytes.Span);
        await Assert.That(bytes).IsEquivalentTo(EXPECTED_BYTES);
    }
}
