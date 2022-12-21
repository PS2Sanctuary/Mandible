using Mandible.Manifest;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace Mandible.Tests.Manifest;

public class FolderTests
{
    [Fact]
    public async Task TestDeserialize_DownloadPriority()
    {
        XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <folder downloadPriority="30">
                <file name="Uninstaller.exe" compressedSize="138583" uncompressedSize="314784" crc="1220914910" timestamp="1329165695" os="windows" sha="b3a478c93557146432e67f17477628ed26e3e830" />
                <file name="Installer.exe" compressedSize="138583" uncompressedSize="314784" crc="1220914910" timestamp="1329165695" os="windows" sha="b3a478c93557146432e67f17477628ed26e3e830" />
            </folder>
            """
        );
        Folder folder = await Folder.DeserializeFromXmlAsync(reader);

        Assert.Equal(30, folder.DownloadPriority);
        Assert.Empty(folder.Children);
        Assert.Null(folder.Name);
        Assert.Equal(2, folder.Files.Count);
        Assert.Equal("Installer.exe", folder.Files[1].Name);
    }

    [Fact]
    public async Task TestDeserialize_Nested()
    {
        XmlReader reader = GetXmlReader
        (
            // lang=xml
            """
            <folder name="CommonData">
                <folder name="Collision">
                    <file name="1StoryRoomSmaller.cdt" delete="yes" />
                </folder>
                <folder name="Assets">
                    <file name="2StoryRoomSmaller.cdt" delete="yes" />
                </folder>
            </folder>
            """
        );
        Folder folder = await Folder.DeserializeFromXmlAsync(reader);

        Assert.Equal("CommonData", folder.Name);
        Assert.Null(folder.DownloadPriority);
        Assert.Empty(folder.Files);
        Assert.Equal(2, folder.Children.Count);

        Assert.Equal("Assets", folder.Children[1].Name);
        Assert.NotEmpty(folder.Children[1].Files);
        Assert.Equal("2StoryRoomSmaller.cdt", folder.Children[1].Files[0].Name);
    }

    private static XmlReader GetXmlReader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { Async = true });
        reader.Read();
        return reader;
    }
}
