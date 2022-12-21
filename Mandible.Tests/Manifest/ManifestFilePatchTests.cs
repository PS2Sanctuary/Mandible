using Mandible.Manifest;
using System;
using System.IO;
using System.Xml;
using Xunit;

namespace Mandible.Tests.Manifest;

public class ManifestFilePatchTests
{
    [Fact]
    public void TestDeserialize()
    {
        XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <patch sourceUncompressedSize="256688832" sourceCrc="2391518192" sourceTimestamp="1665009966" patchCompressedSize="11939882" patchUncompressedSize="12037117" />
            """
        );
        ManifestFilePatch patch = ManifestFilePatch.DeserializeFromXml(reader);

        Assert.Equal(256688832, patch.SourceUncompressedSize);
        Assert.Equal(2391518192, patch.SourceCrc);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1665009966), patch.SourceTimestamp);
        Assert.Equal(11939882, patch.PatchCompressedSize);
        Assert.Equal(12037117, patch.PatchUncompressedSize);
    }

    private static XmlReader GetXmlReader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings() { Async = true });
        reader.Read();
        return reader;
    }
}
