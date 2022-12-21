using Mandible.Manifest;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Mandible.Tests.Manifest;

public class ManifestFileTests
{
    [Fact]
    public async Task TestDeserialize_FileToDeleteAsync()
    {
        XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <file name="White.tga" delete="yes" />
            """
        );
        ManifestFile file = await ManifestFile.DeserializeFromXmlAsync(reader);

        Assert.Equal("White.tga", file.Name);
        Assert.True(file.Delete);
        Assert.Null(file.CompressedSize);
        Assert.Null(file.Timestamp);
        Assert.Null(file.Executable);
        Assert.Empty(file.Patches);
    }

    [Fact]
    public async Task TestDeserialize_AllValuesAsync()
    {
        XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <file name="Uninstaller.exe" compressedSize="138583" uncompressedSize="314784" crc="1220914910" timestamp="1329165695" os="windows" sha="b3a478c93557146432e67f17477628ed26e3e830" executable="true" />
            """
        );
        ManifestFile file = await ManifestFile.DeserializeFromXmlAsync(reader);

        Assert.Equal("Uninstaller.exe", file.Name);
        Assert.Equal(138583, file.CompressedSize);
        Assert.Equal(314784, file.UncompressedSize);
        Assert.Equal((uint)1220914910, file.Crc);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1329165695), file.Timestamp);
        Assert.Equal("windows", file.OS);
        Assert.Equal("b3a478c93557146432e67f17477628ed26e3e830", file.Sha);
        Assert.Equal(true, file.Executable);
        Assert.Empty(file.Patches);
    }

    [Fact]
    public async Task TestDeserialize_WithPatchesAsync()
    {
        XmlReader reader = GetXmlReader
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

        Assert.Equal(3, file.Patches.Count);
        Assert.Equal(3357894315, file.Patches[2].SourceCrc);
    }

    private static XmlReader GetXmlReader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings() { Async = true });
        reader.Read();
        return reader;
    }
}
