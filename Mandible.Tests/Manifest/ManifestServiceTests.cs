using Mandible.Manifest;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Mandible.Tests.Manifest;

public class ManifestServiceTests
{
    // This test needs to be refactored to use mocking.
    // For now though, it's a useful debugging tool.
    //[Fact]
    public async Task TestGetDigestAsync()
    {
        ManifestService ms = new(NullLogger<ManifestService>.Instance, new HttpClient());
        Digest digest = await ms.GetDigestAsync("http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/livenext/planetside2-livecommon.sha.soe.txt");

        ManifestFile? FindFile(string fileName, Folder searchRoot)
        {
            foreach (ManifestFile file in searchRoot.Files)
            {
                if (file.Name == fileName)
                    return file;
            }

            foreach (Folder child in searchRoot.Children)
            {
                ManifestFile? childFile = FindFile(fileName, child);
                if (childFile is not null)
                    return childFile;
            }

            return null;
        }

        ManifestFile? usLocaleFile = null;
        foreach (Folder folder in digest.Folders)
        {
            usLocaleFile = FindFile("en_us_data.dat", folder);
            if (usLocaleFile is not null)
            {
                Stream fileData = await ms.GetFileDataAsync(digest, usLocaleFile);
            }
        }

        Assert.NotNull(usLocaleFile);
    }
}
