using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Mandible.Gui.Services;
using Mandible.Gui.ViewModels;
using Mandible.Gui.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
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
        SetupLogger(services);
        RegisterViewComponents(services);
        RegisterServices(services);

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

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<PackManagerService>();
    }

    private static void SetupLogger(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(builder => builder.AddSerilog());
    }
}
