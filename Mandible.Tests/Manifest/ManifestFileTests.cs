using Mandible.Manifest;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Tests.Manifest;

public class ManifestFileTests
{
    [Test]
    public async Task TestDeserialize_FileToDeleteAsync()
    {
        using XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <file name="White.tga" delete="yes" />
            """
        );
        ManifestFile file = await ManifestFile.DeserializeFromXmlAsync(reader);

        await Assert.That(file.Name).IsEqualTo("White.tga");
        await Assert.That(file.Delete).IsTrue();
        await Assert.That(file.CompressedSize).IsNull();
        await Assert.That(file.Timestamp).IsNull();
        await Assert.That(file.Executable).IsNull();
        await Assert.That(file.Patches).IsEmpty();
    }

    [Test]
    public async Task TestDeserialize_AllValuesAsync()
    {
        using XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <file name="Uninstaller.exe" compressedSize="138583" uncompressedSize="314784" crc="1220914910" timestamp="1329165695" os="windows" sha="b3a478c93557146432e67f17477628ed26e3e830" executable="true" />
            """
        );
        ManifestFile file = await ManifestFile.DeserializeFromXmlAsync(reader);

        await Assert.That(file.Name).IsEqualTo("Uninstaller.exe");
        await Assert.That(file.CompressedSize).IsEqualTo(138583);
        await Assert.That(file.UncompressedSize).IsEqualTo(314784);
        await Assert.That(file.Crc).IsEqualTo((uint)1220914910);
        await Assert.That(file.Timestamp).IsEqualTo(DateTimeOffset.FromUnixTimeSeconds(1329165695));
        await Assert.That(file.OS).IsEqualTo("windows");
        await Assert.That(file.Sha).IsEqualTo("b3a478c93557146432e67f17477628ed26e3e830");
        await Assert.That(file.Executable).IsTrue();
        await Assert.That(file.Patches).IsEmpty();
    }

    [Test]
    public async Task TestDeserialize_WithPatchesAsync()
    {
        using XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <file name="assets_x64_0.pack2" compressedSize="260038849" uncompressedSize="260038720" crc="2304049464" timestamp="1670383787" sha="2f2fd7a277a140a4ef7b0fb38413ff80918b9c08">
                <patch sourceUncompressedSize="256688832" sourceCrc="2391518192" sourceTimestamp="1665009966" patchCompressedSize="11939882" patchUncompressedSize="12037117" />
                <patch sourceUncompressedSize="259248288" sourceCrc="1762535410" sourceTimestamp="1668643298" patchCompressedSize="1190224" patchUncompressedSize="1208669" />
                <patch sourceUncompressedSize="260038752" sourceCrc="3357894315" sourceTimestamp="1669775172" patchCompressedSize="13294" patchUncompressedSize="13253" />
            </file>
            """
        );
        ManifestFile file = await ManifestFile.DeserializeFromXmlAsync(reader);

        await Assert.That(file.Patches.Count).IsEqualTo(3);
        await Assert.That(file.Patches[2].SourceCrc).IsEqualTo(3357894315);
    }

    private static XmlReader GetXmlReader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { Async = true });
        reader.Read();
        return reader;
    }
}
