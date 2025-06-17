using ConsoleAppFramework;
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

/// <summary>
/// Downloads PlanetSide 2 assets from the DBG CDN.
/// </summary>
public class DownloadCommands
{
    private static readonly Dictionary<PS2Environment, string> ManifestUrls = new()
    {
        { PS2Environment.Live, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-livecommon/livenext/planetside2-livecommon.sha.soe.txt" },
        { PS2Environment.Test, "http://manifest.patch.daybreakgames.com/patch/sha/manifest/planetside2/planetside2-testcommon/livenext/planetside2-testcommon.sha.soe.txt" }
    };

    private readonly IAnsiConsole _console;
    private readonly IManifestService _manifestService;

    public DownloadCommands
    (
        IAnsiConsole console,
        IManifestService manifestService
    )
    {
        _console = console;
        _manifestService = manifestService;
    }

    /// <summary>
    /// Downloads every asset pack (*.pack2).
    /// </summary>
    /// <param name="outputDirectory">The directory to download the packets into.</param>
    /// <param name="environment">-e, The environment from which to select assets.</param>
    /// <param name="force">-f, Force overwrite of existing pack files.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    [Command("packs")]
    public async Task ExtractAsync
    (
        [Argument] string outputDirectory,
        PS2Environment environment = PS2Environment.Live,
        bool force = false,
        CancellationToken ct = default
    )
    {
        if (!CommandUtils.CheckOutputDirectory(_console, outputDirectory))
            return;

        Digest digest = await _manifestService.GetDigestAsync(ManifestUrls[environment], ct);
        foreach (Folder folder in digest.Folders)
            await DownloadFolder(digest, folder, outputDirectory, force, ct);
    }

    private async Task DownloadFolder
    (
        Digest digest,
        Folder folder,
        string outputDirectory,
        bool force,
        CancellationToken ct
    )
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
                    byte[] sha1 = await SHA1.HashDataAsync(inputFs, ct);

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

            await using Stream s = await _manifestService.GetFileDataAsync(digest, file, ct);
            await using FileStream fs = new(outputPath, FileMode.Create);
            await s.CopyToAsync(fs, ct);
        }

        foreach (Folder child in folder.Children)
            await DownloadFolder(digest, child, outputDirectory, force, ct);
    }
}
