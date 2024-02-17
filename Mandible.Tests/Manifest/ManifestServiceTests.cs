using Mandible.Manifest;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mandible.Tests.Manifest;

public class ManifestServiceTests
{
    [Fact]
    public async Task TestGetDigestAsync()
    {
        const string digestEndpoint = "https://my.url";

        ManifestService ms = GetManifestService(out Mock<HttpMessageHandler> handler);
        Digest digest = await ms.GetDigestAsync(digestEndpoint, CancellationToken.None);

        Uri expectedUri = new(digestEndpoint + ".txt");
        handler.Protected()
            .Verify
            (
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>
                (
                    req => req.Method == HttpMethod.Get
                        && req.RequestUri == expectedUri
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        Assert.Equal(159, digest.DigestBuilderVersion);
        Assert.Single(digest.Folders);
    }

    [Fact]
    public async Task TestGetFileDataAsync()
    {
        Digest digest = new
        (
            1,
            "test",
            new Uri("https://my.url"),
            string.Empty,
            null,
            12,
            2,
            null,
            "local",
            new Uri("http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs"),
            DateTimeOffset.Now,
            "lzma",
            new [] { "pls.patch.daybreakgames.com", "antonica.patch.daybreakgames.com" },
            Array.Empty<Uri>(),
            Array.Empty<Folder>()
        );
        ManifestFile file = new("manifest.xml", 2, 1, 0, DateTimeOffset.Now, null, "abcdefgh", null, null, Array.Empty<ManifestFilePatch>());

        ManifestService ms = GetManifestService(out Mock<HttpMessageHandler> handler);
        Stream fileData = await ms.GetFileDataAsync(digest, file, CancellationToken.None);

        Uri expectedUri = new("http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs/ab/cde/fgh");
        handler.Protected()
            .Verify
            (
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>
                (
                    req => req.Method == HttpMethod.Get
                           && req.RequestUri == expectedUri
                ),
                ItExpr.IsAny<CancellationToken>()
            );

        Assert.True(fileData.CanSeek);
        Assert.Equal(0, fileData.Position);
    }

    private static ManifestService GetManifestService(out Mock<HttpMessageHandler> handlerMock)
    {
        handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>
            (
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync
            (
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(DIGEST_DATA)
                }
            )
            .Verifiable();

        return new ManifestService
        (
            NullLogger<ManifestService>.Instance,
            new HttpClient(handlerMock.Object)
        );
    }

    // lang=xml
    private const string DIGEST_DATA =
        """
        <?xml version="1.0" encoding="UTF-8" standalone="yes" ?>
        <digest digestBuilderVersion="159" productName="PlanetSide 2" defaultServerFolder="http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs" publisher="Sony Online Entertainment" iconPath="PlanetSide2_x64_BE.exe" packageSizeKB="91361" fileCount="17" launchPath="PlanetSide2_x64_BE.exe" defaultLocalFolder="planetside2.sha.zs" shaAssetURL="http://pls.patch.daybreakgames.com/patch/sha/planetside2/planetside2.sha.zs" timestamp="1670987222" compressionType="lzma">
            <fallback host="pls.patch.daybreakgames.com" />
            <fallback host="antonica.patch.daybreakgames.com" />
            <fallback host="faydwer.patch.daybreakgames.com" />
            <fallback host="odus.patch.daybreakgames.com" />
            <externalDigest url="http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/live/planetside2-livecommon.sha.soe" />
            <folder>
                <folder downloadPriority="30">
                    <file name="Uninstaller.exe" compressedSize="138583" uncompressedSize="314784" crc="1220914910" timestamp="1329165695" os="windows" sha="b3a478c93557146432e67f17477628ed26e3e830" />
                </folder>
                <folder downloadPriority="20">
                    <file name="!if.Is32Bit.dll" delete="yes" />
                    <file name="!if.Is64Bit.dll" delete="yes" />
                </folder>
                <folder name="BattlEye">
                    <folder name="Privacy">
                        <file name="en-US.txt" compressedSize="956" uncompressedSize="1642" crc="2584310690" timestamp="1530058733" sha="d4ea14634c6be122a10eb545c606b537ae8fba76" />
                    </folder>
                    <file name="BEClient_x64.dll" compressedSize="4779779" uncompressedSize="5235208" crc="3842604886" timestamp="1603155004" sha="c326072fffee0e03620edde87056d25893bcb878" />
                    <file name="BELauncher.ini" compressedSize="228" uncompressedSize="99" crc="649779607" timestamp="1530058733" sha="4b90f78b3c90ccd6f4a9bd17f3f08ce0f11d1829" />
                    <file name="BEService_x64.exe" compressedSize="8491836" uncompressedSize="8885112" crc="1849419850" timestamp="1652821294" sha="dd538445bde20b32382ff316401d3a5523d97eea" />
                </folder>
                <folder name="UI">
                    <file name="ScriptsBase.bin" delete="yes" />
                    <file name="ScriptsBase_x64.bin" compressedSize="93831" uncompressedSize="533513" crc="387709840" timestamp="1670980607" sha="3f58705350851fcae7d8792c96f9ff8b1cbbc315" />
                </folder>
                <file name="awesomium_1_7.dll" delete="yes" />
                <file name="awesomium_d.dll" delete="yes" />
                <file name="d3dx9_31.dll" delete="yes" />
                <file name="D3DX9_40.dll" delete="yes" />
                <file name="PlanetSide2.exe" delete="yes" />
                <file name="PlanetSide2_x64.exe" compressedSize="56331944" uncompressedSize="77861672" crc="1267351344" timestamp="1670979469" executable="true" sha="7132438aefc83942886f1e176cb5bf59c1dd8eec" />
                <file name="PlanetSide2_x64_BE.exe" compressedSize="509725" uncompressedSize="722440" crc="79162161" timestamp="1619568776" executable="true" sha="d49796b1322b3fd87f2d9a74bd866dda22a089f8" />
                <file name="PlanetSide2_x86.exe" delete="yes" />
            </folder>
        </digest>
        """;
}
