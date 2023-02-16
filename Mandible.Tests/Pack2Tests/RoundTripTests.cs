using Mandible.Pack2;
using Mandible.Services;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Mandible.Tests.Pack2Tests;

public class RoundTripTests
{
    private const string TEST_PACK_PATH = "Data\\Sanctuary_x64_0.pack2";
    private const string OUTPUT_PATH = TEST_PACK_PATH + ".out";

    [Fact]
    public async Task TestRoundTrip()
    {
        using RandomAccessDataReaderService dr = new(TEST_PACK_PATH);
        RandomAccessDataWriterService dw = new(OUTPUT_PATH, FileMode.Create);

        using Pack2Reader reader = new(dr);
        await using Pack2Writer writer = new(dw);

        IReadOnlyList<Asset2Header> assets = await reader.ReadAssetHeadersAsync();
        foreach (Asset2Header header in assets)
        {
            IMemoryOwner<byte> assetData = await reader.ReadAssetDataAsync(header);
            await writer.WriteAssetAsync(header.NameHash, assetData.Memory, header.ZipStatus, header.DataHash);
        }

        await writer.CloseAsync();

        dw.Dispose();

        using RandomAccessDataReaderService dr2 = new(OUTPUT_PATH);
        using Pack2Reader reader2 = new(dr2);

        // This should not throw
        await reader2.ValidateAsync();

        Pack2Header expectedHeader = await reader.ReadHeaderAsync();
        Pack2Header actualHeader = await reader2.ReadHeaderAsync();
        Assert.Equal(expectedHeader.AssetCount, actualHeader.AssetCount);

        IReadOnlyList<Asset2Header> actualAssets = await reader2.ReadAssetHeadersAsync();
        Assert.Equal(assets.Count, actualAssets.Count);

        for (int i = 0; i < assets.Count; i++)
        {
            Asset2Header expectedAH = assets[i];
            Asset2Header actualAH = actualAssets[i];

            Assert.Equal(expectedAH.DataHash, actualAH.DataHash);
            Assert.Equal(expectedAH.NameHash, actualAH.NameHash);
            Assert.Equal(expectedAH.ZipStatus, actualAH.ZipStatus);

            if (expectedAH.ZipStatus is Asset2ZipDefinition.Unzipped or Asset2ZipDefinition.UnzippedAlternate)
                Assert.Equal(expectedAH.DataSize, actualAH.DataSize);

            using IMemoryOwner<byte> expectedData = await reader.ReadAssetDataAsync(expectedAH);
            using IMemoryOwner<byte> actualData = await reader2.ReadAssetDataAsync(actualAH);

            Assert.Equal(expectedData.Memory.Length, actualData.Memory.Length);
            for (int j = 0; j < expectedData.Memory.Length; j++)
                Assert.Equal(expectedData.Memory.Span[j], actualData.Memory.Span[j]);
        }
    }
}
