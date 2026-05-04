using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mandible.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IStorageProvider _storageProvider;

    public ObservableCollection<string> AssetPacks { get; } = [];

    public MainWindowViewModel()
    {
        // Default ctor provided for design-time context
        _storageProvider = null!;
    }

    public MainWindowViewModel(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    [RelayCommand]
    public async Task OpenPack()
    {
        IReadOnlyList<IStorageFile> results = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            FileTypeFilter = [
                new FilePickerFileType("ForgeLight Asset Packs") {Patterns = ["*.pack", "*.pack2"]}
            ],
            Title = "Select Pack / Pack2 Files"
        });

        foreach (IStorageFile file in results)
            AssetPacks.Add(file.Path.ToString());
    }
}
