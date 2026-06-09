using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Pack2;
using Mandible.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mandible.Tests.Pack2Tests;

public class RoundTripTests
{
    private static readonly string TEST_PACK_PATH = Path.Combine("Data", "Sanctuary_x64_0.pack2");
    private static readonly string OUTPUT_PATH = TEST_PACK_PATH + ".out";

    [Test]
    public async Task TestRoundTrip()
    {
        using RandomAccessDataReaderService dr = new(TEST_PACK_PATH);
        RandomAccessDataWriterService dw = new(OUTPUT_PATH, FileMode.Create);

        using Pack2Reader reader = new(dr);
        await using Pack2Writer writer = new(dw);

        IReadOnlyList<Asset2Header> assets = await reader.ReadAssetHeadersAsync();
        foreach (Asset2Header header in assets)
        {
            MemoryOwner<byte> assetData = await reader.ReadAssetDataAsync(header);
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
        await Assert.That(actualHeader.AssetCount).IsEqualTo(expectedHeader.AssetCount);

        IReadOnlyList<Asset2Header> actualAssets = await reader2.ReadAssetHeadersAsync();
        await Assert.That(actualAssets.Count).IsEqualTo(assets.Count);

        for (int i = 0; i < assets.Count; i++)
        {
            Asset2Header expectedAH = assets[i];
            Asset2Header actualAH = actualAssets[i];

            await Assert.That(actualAH.DataHash).IsEqualTo(expectedAH.DataHash);
            await Assert.That(actualAH.NameHash).IsEqualTo(expectedAH.NameHash);
            await Assert.That(actualAH.ZipStatus).IsEqualTo(expectedAH.ZipStatus);

            if (expectedAH.ZipStatus is Asset2ZipDefinition.Unzipped or Asset2ZipDefinition.UnzippedAlternate)
                await Assert.That(actualAH.DataSize).IsEqualTo(expectedAH.DataSize);

            using MemoryOwner<byte> expectedData = await reader.ReadAssetDataAsync(expectedAH);
            using MemoryOwner<byte> actualData = await reader2.ReadAssetDataAsync(actualAH);

            await Assert.That(actualData.Memory.Length).IsEqualTo(expectedData.Memory.Length);
            await Assert.That(actualData.Memory).IsEquivalentTo(expectedData.Memory);
        }
    }
}
