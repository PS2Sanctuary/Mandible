﻿using Mandible.Util;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2;

/// <summary>
/// Represents a mapping of CRC-64 file name hashes to the original file names.
/// </summary>
public class Namelist
{
    /// <summary>
    /// Gets the CRC-64 hash of the namelist file name ( {NAMELIST} ).
    /// </summary>
    private const ulong NamelistFileNameHash = 4699449473529019696;

    private readonly Dictionary<ulong, string> _hashedNamePairs;

    /// <summary>
    /// Gets the namelist.
    /// </summary>
    public IReadOnlyDictionary<ulong, string> Map => _hashedNamePairs;

    /// <summary>
    /// Initializes a new instance of the <see cref="Namelist"/> class.
    /// </summary>
    public Namelist()
    {
        _hashedNamePairs = new Dictionary<ulong, string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Namelist"/> class.
    /// </summary>
    /// <param name="existing">An existing namelist to copy into this <see cref="Namelist"/> instance.</param>
    public Namelist(IReadOnlyDictionary<ulong, string> existing)
    {
        _hashedNamePairs = new Dictionary<ulong, string>(existing);
    }

    /// <summary>
    /// Constructs a <see cref="Namelist"/> from a file.
    /// The file must contain purely names, separated by a newline.
    /// </summary>
    /// <param name="filePath">The path to the namelist file.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The constructed namelist.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the namelist file could not be found.</exception>
    public static async Task<Namelist> FromFileAsync(string filePath, CancellationToken ct = default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Unable to locate the namelist file.", filePath);

        Namelist nl = new();
        await using FileStream masterFS = new(filePath, FileMode.Open);
        await nl.Append(masterFS, ct: ct).ConfigureAwait(false);

        return nl;
    }

    /// <summary>
    /// Gets a name.
    /// </summary>
    /// <param name="hash">The CRC-64 hash of the name.</param>
    /// <returns>The name, or null if it doesn't exist in this <see cref="Namelist"/>.</returns>
    public string? Get(ulong hash)
    {
        if (_hashedNamePairs.ContainsKey(hash))
            return _hashedNamePairs[hash];

        return null;
    }

    public void Append(ulong hash, string name)
    {
        if (_hashedNamePairs.ContainsKey(hash))
            return;

        _hashedNamePairs.Add(hash, name);
    }

    public void Append(string name)
    {
        ulong hash = PackCrc64.Calculate(name);
        Append(hash, name);
    }

    public async Task Append(IEnumerable<string> names, CancellationToken ct = default)
        => await Task.Run
            (
                () =>
                {
                    foreach (string name in names)
                    {
                        if (ct.IsCancellationRequested)
                            throw new TaskCanceledException();

                        Append(name);
                    }
                },
                ct
            ).ConfigureAwait(false);

    public async Task Append(Stream stream, int endPosition = -1, CancellationToken ct = default)
    {
        using StreamReader sr = new(stream, null, true, -1, true);
        List<string> names = new();

        while (!sr.EndOfStream && (stream.Position < endPosition || endPosition < 0))
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            string? name = await sr.ReadLineAsync().ConfigureAwait(false);
            if (name is null)
                continue;

            names.Add(name);
        }

        await Append(names, ct).ConfigureAwait(false);
        names.Clear();
    }

    public async Task Append(Pack2Reader reader, CancellationToken ct = default)
    {
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        Asset2Header? namelistHeader = null;
        foreach (Asset2Header asset in assetHeaders)
        {
            if (asset.NameHash == NamelistFileNameHash)
            {
                namelistHeader = asset;
                break;
            }
        }

        if (namelistHeader is null)
            return;

        (IMemoryOwner<byte> data, int length) = await reader.ReadAssetDataAsync(namelistHeader.Value, ct).ConfigureAwait(false);
        byte[] dataArray = ArrayPool<byte>.Shared.Rent(length);
        data.Memory[..length].CopyTo(dataArray);

        await using MemoryStream ms = new(dataArray);
        await Append(ms, length, ct).ConfigureAwait(false);
        ArrayPool<byte>.Shared.Return(dataArray);
    }

    public async Task WriteAsync(Stream outputStream, CancellationToken ct = default)
    {
        using StreamWriter sw = new(outputStream, null, -1, true);
        string[] names = _hashedNamePairs.Values.OrderBy(s => s).ToArray();

        foreach (string name in names)
        {
            if (ct.IsCancellationRequested)
                throw new TaskCanceledException();

            await sw.WriteLineAsync(name).ConfigureAwait(false);
        }
    }
}
