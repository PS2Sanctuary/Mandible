using Mandible.Manifest;
using System;
using System.IO;
using System.Xml;
using System.Threading.Tasks;

namespace Mandible.Tests.Manifest;

public class ManifestFilePatchTests
{
    [Test]
    public async Task TestDeserialize()
    {
        using XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <patch sourceUncompressedSize="256688832" sourceCrc="2391518192" sourceTimestamp="1665009966" patchCompressedSize="11939882" patchUncompressedSize="12037117" />
            """
        );
        ManifestFilePatch patch = ManifestFilePatch.DeserializeFromXml(reader);

        await Assert.That(patch.SourceUncompressedSize).IsEqualTo(256688832);
        await Assert.That(patch.SourceCrc).IsEqualTo(2391518192);
        await Assert.That(patch.SourceTimestamp).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(1665009966));
        await Assert.That(patch.PatchCompressedSize).IsEqualTo(11939882);
        await Assert.That(patch.PatchUncompressedSize).IsEqualTo(12037117);
    }

    private static XmlReader GetXmlReader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { Async = true });
        reader.Read();
        return reader;
    }
}