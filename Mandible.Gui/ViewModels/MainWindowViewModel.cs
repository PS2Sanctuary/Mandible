using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Mandible.Gui.Models.Pack;
using Mandible.Gui.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mandible.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IStorageProvider _storageProvider;
    private readonly PackManagerService _packManager;

    public ObservableCollection<BasePackInfo> AssetPacks { get; } = [];

    public MainWindowViewModel()
    {
        // Default ctor provided for design-time context
        _storageProvider = null!;
        _packManager = null!;
    }

    public MainWindowViewModel(IStorageProvider storageProvider, PackManagerService packManager)
    {
        _storageProvider = storageProvider;
        _packManager = packManager;
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
        {
            string? filePath = file.TryGetLocalPath();
            if (string.IsNullOrEmpty(filePath))
            {
                // TODO: Message box or snackbar
                continue;
            }

            BasePackInfo? packInfo = await _packManager.AddPack(file.TryGetLocalPath()!);
            if (packInfo is null)
            {
                // TODO: Message box or snackbar
                continue;
            }

            AssetPacks.Add(packInfo);
        }
    }
}
