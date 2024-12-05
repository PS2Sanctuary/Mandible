using CommandDotNet;
using Mandible.Abstractions.Manifest;
using Mandible.Cli.Objects;
using Mandible.Cli.Util;
using Mandible.Manifest;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

[Command("download", Description = "Downloads PlanetSide 2 assets from the DBG CDN.")]
public class DownloadCommands
{
    private static readonly Dictionary<PS2Environment, string> ManifestUrls = new()
    {
        { PS2Environment.Live, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/livenext/planetside2-livecommon.sha.soe.txt" },
        { PS2Environment.Test, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-testcommon/livenext/planetside2-testcommon.sha.soe.txt" }
    };

    private readonly IAnsiConsole _console;
    private readonly CancellationToken _ct;
    private readonly IManifestService _manifestService;

    public DownloadCommands
    (
        IAnsiConsole console,
        IManifestService manifestService,
        CancellationToken ct
    )
    {
        _console = console;
        _manifestService = manifestService;
        _ct = ct;
    }

    [Command("packs", Description = "Downloads every asset pack (*.pack2).")]
    public async Task ExtractAsync
    (
        [Operand(Description = "The directory to download the packets into.")]
        string outputDirectory,

        [Option('e', Description = "The environment from which to select assets.")]
        PS2Environment environment = PS2Environment.Live,

        [Option('f', Description = "Force overwrite of existing pack files.")]
        bool force = false
    )
    {
        if (!CommandUtils.CheckOutputDirectory(_console, outputDirectory))
            return;

        Digest digest = await _manifestService.GetDigestAsync(ManifestUrls[environment], _ct);
        foreach (Folder folder in digest.Folders)
            await DownloadFolder(digest, folder, outputDirectory, force);
    }

    private async Task DownloadFolder(Digest digest, Folder folder, string outputDirectory, bool force)
    {
        foreach (ManifestFile file in folder.Files)
        {
            if (!file.Name.EndsWith(".pack2"))
                continue;

            string outputPath = Path.Combine(outputDirectory, file.Name);
            if (File.Exists(outputPath) && !force)
            {
                // Check the SHA hash to see if we need to re-download the file
                if (file.Sha is not null)
                {
                    await using FileStream inputFs = new(outputPath, FileMode.Open);
                    byte[] sha1 = await SHA1.HashDataAsync(inputFs, _ct);

                    if (Convert.ToHexString(sha1).Equals(file.Sha, StringComparison.OrdinalIgnoreCase))
                    {
                        _console.WriteLine($"Skipping {file.Name} as the same version already exists");
                        continue;
                    }
                }

                if (!_console.Confirm($"{file.Name} already exists. Overwrite? [y/N]", false))
                    continue;
            }

            AnsiConsole.WriteLine($"Downloading {file.Name}...");

            await using Stream s = await _manifestService.GetFileDataAsync(digest, file, _ct);
            await using FileStream fs = new(outputPath, FileMode.Create);
            await s.CopyToAsync(fs, _ct);
        }

        foreach (Folder child in folder.Children)
            await DownloadFolder(digest, child, outputDirectory, force);
    }
}
