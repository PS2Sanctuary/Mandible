# Mandible

[![Nuget | carlst99.Mandible](https://img.shields.io/nuget/v/carlst99.Mandible?label=carlst99.Mandible)](https://www.nuget.org/packages/carlst99.Mandible)

Mandible eases the process of working with the packed assets of a game that uses the
[ForgeLight Engine](https://en.wikipedia.org/wiki/Daybreak_Game_Company#ForgeLight_engine).

Mandible currently supports the following formats (R = read, W = write):

- `.dma/DMAT` (R/W)
- Locale data (R/W)
- `.pack` (R)
- `.pack2/PAK` (R/W)
- `.zone/ZONE` (R/W)

Additional features include:

- A `pack2` namelist extraction utility.
- Parsing and downloading of digests and files served by the Daybreak Manifest CDN.

Documentation and ImHex patterns on some of the file formats that Mandible supports can be found in the [docs](docs)
folder.

A command-line tool is also distributed with Mandible, which offers commands to:

- Extract assets from `pack` and `pack2` files.
- Create `pack2` files.
- Generate namelists from `pack2` files.
- Generate an index of assets within pack files.

**This package is unofficial and is not affiliated with the developers of the ForgeLight engine or its derived games in
any way.**

## Installation

Mandible is available as [NuGet package](https://www.nuget.org/packages/carlst99.Mandible):

```sh
# Visual Studio Package Manager
Install-Package carlst99.Mandible
# dotnet console
dotnet add package carlst99.Mandible
```

The command-line tool can be found in the [latest release](https://github.com/PS2Sanctuary/Mandible/releases/latest).

## Usage

### The `IDataReaderService`

Components that need to read data from an IO source, such as a pack reader, use the `IDataReaderService` interface.
Mandible currently has two implementations of this interface:

- The `RandomAccessDataReaderService` uses .NET 6's `RandomAccess` APIs to read data from a file. It is recommended that
you use this implementation when possible due to its increased performance, as pack reading is not often sequential.

- The `StreamDataReaderService` reads data from any backing `Stream`. The backing stream must be seekable.

### Reading Pack Files

Mandible supports both the `pack` and `pack2` format, by way of the following interfaces and their default implementation:

- `IPackReader`; `PackReader`
- `IPack2Reader`; `Pack2Reader`

Both interfaces provide the means to read the pack headers and the asset data. The `Pack2Reader` will also unzip any
compressed assets.

Here is a sample method for reading all the assets from a `pack2` file. The process is very similar for a `pack` file,
albeit without the need for a namelist.

This example is modified and minified from what can be found in the
[IPack2ReaderExtensions](Mandible/Pack2/IPack2ReaderExtensions.cs) class.

```csharp
public static async Task ExportAllAsync
(
    string packFilePath,
    string outputPath,
    Namelist? namelist,
    CancellationToken ct = default
)
{
    await using RandomAccessDataReaderService dataReader = new(packFilePath);
    using Pack2Reader reader = new(dataReader);

    IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

    foreach (Asset2Header assetHeader in assetHeaders)
    {
        string? fileName = null;
        namelist?.TryGet(assetHeader.NameHash, out fileName);

        using SafeFileHandle outputHandle = File.OpenHandle
        (
            Path.Combine(outputPath, fileName ?? assetHeader.NameHash.ToString())
        );

        using MemoryOwner<byte> data = await reader.ReadAssetDataAsync(assetHeader, ct).ConfigureAwait(false);
        await RandomAccess.WriteAsync(outputHandle, data.Memory, 0, ct).ConfigureAwait(false);
    }
}
```

### Namelists

The `pack2` format does not store asset names, instead opting for a CRC-64 "Jones" hash of the original file name. This
means that an external namelist is required in order to export assets with a sane name. The external list should contain
a list of names, separated by `LF` characters.

Mandible provides the `Namelist` class to help with this. It provides various overloads of an `Append` method to insert
names at runtime, the `WriteAsync` method to export a correctly-formatted list to a stream and a static `FromFileAsync`
which is self-explanatory.

```csharp
Namelist nl = await Namelist.FromFileAsync("master-namelist.txt");
nl.Append("my_fantastic_name.dds");

await using FileStream nlOut = new("new-namelist.txt", FileMode.Create);
await nl.WriteAsync(nlOut);
```

#### Extracting a Namelist

The `NameExtractor` class can be used to extract plaintext file names from a directory of `pack2` files.
It will also make guesses by constructing known name patterns - but this does mean that some of the generated names will
not exist as actual files.

When extracting a namelist, it is useful to note that some game distributions will include a namelist file
(`{NAMELIST}`, no file extension) in the packs themselves. For example, PlanetSide 2's test distribution. If you can
extract your names from these sets of packs, you will likely get a much more complete namelist.
