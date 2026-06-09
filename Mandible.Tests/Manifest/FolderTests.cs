using Mandible.Manifest;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Mandible.Tests.Manifest;

public class FolderTests
{
    [Test]
    public async Task TestDeserialize_DownloadPriority()
    {
        using XmlReader reader = GetXmlReader
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

        await Assert.That(folder.DownloadPriority).IsEqualTo(30);
        await Assert.That(folder.Children).IsEmpty();
        await Assert.That(folder.Name).IsNull();
        await Assert.That(folder.Files.Count).IsEqualTo(2);
        await Assert.That(folder.Files[1].Name).IsEqualTo("Installer.exe");
    }

    [Test]
    public async Task TestDeserialize_Nested()
    {
        using XmlReader reader = GetXmlReader
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

        await Assert.That(folder.Name).IsEqualTo("CommonData");
        await Assert.That(folder.DownloadPriority).IsNull();
        await Assert.That(folder.Files).IsEmpty();
        await Assert.That(folder.Children.Count).IsEqualTo(2);

        await Assert.That(folder.Children[1].Name).IsEqualTo("Assets");
        await Assert.That(folder.Children[1].Files).IsNotEmpty();
        await Assert.That(folder.Children[1].Files[0].Name).IsEqualTo("2StoryRoomSmaller.cdt");
    }

    private static XmlReader GetXmlReader(string xml)
    {
        XmlReader reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { Async = true });
        reader.Read();
        return reader;
    }
}
