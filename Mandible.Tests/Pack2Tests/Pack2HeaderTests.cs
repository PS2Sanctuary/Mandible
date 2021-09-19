using Mandible.Pack2;
using System;
using Xunit;

namespace Mandible.Tests.Pack2Tests
{
    public class Pack2HeaderTests
    {
        private static readonly Pack2Header EXPECTED_HEADER = new(1, 2, 3, new byte[] { 0xff }, 4, 256);
        private static readonly byte[] EXPECTED_BYTES = new byte[]
        {
            0x50, 0x41, 0x4b, // PAK
            0x04, // Version
            0x01, 0x00, 0x00, 0x00, // Asset count
            0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Length
            0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Asset map offset
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unknown
            0xff // 'Checksum'
        };

        [Fact]
        public void TestDeserialise()
        {
            Pack2Header header = Pack2Header.Deserialise(EXPECTED_BYTES);

            Assert.Equal(header.Magic, EXPECTED_HEADER.Magic);
            Assert.Equal(header.Version, EXPECTED_HEADER.Version);
            Assert.Equal(header.AssetCount, EXPECTED_HEADER.AssetCount);
            Assert.Equal(header.Length, EXPECTED_HEADER.Length);
            Assert.Equal(header.AssetMapOffset, EXPECTED_HEADER.AssetMapOffset);
            Assert.Equal(header.Unknown, EXPECTED_HEADER.Unknown);
            Assert.Equal(header.Checksum, EXPECTED_HEADER.Checksum);
        }

        [Fact]
        public void TestSerialise()
        {
            ReadOnlySpan<byte> bytes = EXPECTED_HEADER.Serialise();
            Assert.Equal(bytes.ToArray(), EXPECTED_BYTES);
        }
    }
}
