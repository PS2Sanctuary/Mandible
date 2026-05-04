using Mandible.Common;
using Mandible.Gui.Models.Pack;
using Mandible.Pack;
using Mandible.Pack2;
using Mandible.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Gui.Services;

public class PackManagerService
{
    private readonly ILogger<PackManagerService> _logger;

    private readonly List<BasePackInfo> _packInfos = [];

    public PackManagerService(ILogger<PackManagerService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<BasePackInfo> GetLoadedPacks()
        => _packInfos;

    public async ValueTask<BasePackInfo?> AddPack(string path, CancellationToken ct = default)
    {
        FileType packType = FileIdentifiers.InferFileType(Path.GetExtension(path));
        _logger.LogInformation
        (
            "Attempting to load pack of type {FileType} and with name {FileName}",
            packType,
            Path.GetFileName(path)
        );

        try
        {
            BasePackInfo packInfo = packType switch
            {
                FileType.Pack1 => await IndexPack1(path, ct),
                FileType.Pack2 => await IndexPack2(path, ct),
                _ => throw new ArgumentException("The given file does not appear to be an asset pack")
            };
            _packInfos.Add(packInfo);
            return packInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index pack");
            return null;
        }
    }

    private static async ValueTask<Pack1Info> IndexPack1(string path, CancellationToken ct)
    {
        using RandomAccessDataReaderService radrs = new(path);
        PackReader reader = new(radrs);
        IReadOnlyList<AssetHeader> assets = await reader.ReadAssetHeadersAsync(ct);

        return new Pack1Info(assets, path);
    }

    private static async ValueTask<Pack2Info> IndexPack2(string path, CancellationToken ct)
    {
        using RandomAccessDataReaderService radrs = new(path);
        using Pack2Reader reader = new(radrs);

        Pack2Header header = await reader.ReadHeaderAsync(ct);
        IReadOnlyList<Asset2Header> assets = await reader.ReadAssetHeadersAsync(ct);

        return new Pack2Info(header, assets, path);
    }
}
