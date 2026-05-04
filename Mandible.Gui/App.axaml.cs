using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Mandible.Gui.ViewModels;
using Mandible.Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mandible.Gui;

public partial class App : Application
{
    private MainWindow? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceCollection services = new();
        RegisterViewComponents(services);

        services.AddTransient<IStorageProvider>(_ => _mainWindow!.StorageProvider);

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Must construct window so that DI against top level succeeds when retrieving view models
            // ReSharper disable once UseObjectOrCollectionInitializer
            _mainWindow = new MainWindow();
            _mainWindow.DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = _mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterViewComponents(IServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
    }
}
