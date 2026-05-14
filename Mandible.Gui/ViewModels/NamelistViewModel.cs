using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mandible.Gui.ViewModels;

public partial class NamelistViewModel : ViewModelBase
{
    private readonly IStorageProvider _storageProvider;

    [ObservableProperty]
    public partial string? OutputPath { get; set; }

    public ObservableCollection<string> PackDirectories { get; } = [];
    public ObservableCollection<string> ExistingNamelists { get; } = [];

    public NamelistViewModel()
    {
        // Default ctor provided for design-time context
        _storageProvider = null!;
    }

    public NamelistViewModel(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    [RelayCommand]
    public async Task BrowseOutputPath()
    {
        IStorageFile? result = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Select Output Namelist Path",
            FileTypeChoices = [new FilePickerFileType("Namelist Files") { Patterns = ["*.txt"] }],
            DefaultExtension = "txt"
        });

        if (result is not null)
            OutputPath = result.TryGetLocalPath();
    }

    [RelayCommand]
    public async Task AddPackDirectory()
    {
        IReadOnlyList<IStorageFolder> results = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Pack2 Directories",
            AllowMultiple = true
        });

        foreach (IStorageFolder folder in results)
        {
            string? path = folder.TryGetLocalPath();
            if (path is not null && !PackDirectories.Contains(path))
                PackDirectories.Add(path);
        }
    }

    [RelayCommand]
    public void RemovePackDirectory(string path)
    {
        PackDirectories.Remove(path);
    }

    [RelayCommand]
    public async Task AddNamelist()
    {
        IReadOnlyList<IStorageFile> results = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Existing Namelists",
            AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("Namelist Files") { Patterns = ["*.txt"] }]
        });

        foreach (IStorageFile file in results)
        {
            string? path = file.TryGetLocalPath();
            if (path is not null && !ExistingNamelists.Contains(path))
                ExistingNamelists.Add(path);
        }
    }

    [RelayCommand]
    public void RemoveNamelist(string path)
    {
        ExistingNamelists.Remove(path);
    }

    [RelayCommand]
    public async Task ScrapeNamelist()
    {
        if (string.IsNullOrEmpty(OutputPath) || PackDirectories.Count == 0)
            return;

        List<string> pack2Files = PackDirectories
            .SelectMany(d => Directory.EnumerateFiles(d, "*.pack2", SearchOption.AllDirectories))
            .ToList();

        if (pack2Files.Count == 0)
            return;

        Namelist? existingNl = null;
        if (File.Exists(OutputPath))
            existingNl = await Namelist.FromFileAsync(OutputPath);

        Namelist extractedNamelist = await NameExtractor.ExtractAsync(pack2Files, existingNl);
        await using FileStream nlOut = new(OutputPath, FileMode.Create);
        await extractedNamelist.WriteAsync(nlOut);
    }

    [RelayCommand]
    public async Task MergeNamelists()
    {
        if (string.IsNullOrEmpty(OutputPath))
            return;

        Namelist mergedNamelist = new();

        if (File.Exists(OutputPath))
            mergedNamelist.Append(await Namelist.FromFileAsync(OutputPath));

        foreach (string path in ExistingNamelists)
        {
            if (File.Exists(path))
                mergedNamelist.Append(await Namelist.FromFileAsync(path));
        }

        await using FileStream nlOut = new(OutputPath, FileMode.Create);
        await mergedNamelist.WriteAsync(nlOut);
    }

    [RelayCommand]
    public async Task TrimNamelists()
    {
        if (string.IsNullOrEmpty(OutputPath) || PackDirectories.Count == 0)
            return;

        List<string> pack2Files = PackDirectories
            .SelectMany(d => Directory.EnumerateFiles(d, "*.pack2", SearchOption.AllDirectories))
            .ToList();

        if (pack2Files.Count == 0)
            return;

        Namelist combinedNl = new();
        if (File.Exists(OutputPath))
            combinedNl.Append(await Namelist.FromFileAsync(OutputPath));

        foreach (string path in ExistingNamelists)
        {
            if (File.Exists(path))
                combinedNl.Append(await Namelist.FromFileAsync(path));
        }

        HashSet<ulong> knownHashes = [];
        foreach (string path in pack2Files)
        {
            using RandomAccessDataReaderService dr = new(path);
            using Pack2Reader pr = new(dr);

            IReadOnlyList<Asset2Header> headers = await pr.ReadAssetHeadersAsync();
            foreach (Asset2Header element in headers)
                knownHashes.Add(element.NameHash);
        }

        Namelist outputNl = new();
        foreach ((ulong hash, string name) in combinedNl.Map)
        {
            if (knownHashes.Contains(hash))
                outputNl.Append(hash, name);
        }

        await using FileStream nlOut = new(OutputPath, FileMode.Create);
        await outputNl.WriteAsync(nlOut);
    }
}
