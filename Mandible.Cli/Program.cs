using ConsoleAppFramework;
using Mandible.Abstractions.Manifest;
using Mandible.Cli.Commands;
using Mandible.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Spectre.Console;
using System.Net.Http;

namespace Mandible.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create()
            .ConfigureLogging(x =>
            {
                x.ClearProviders();
                x.AddSerilog();
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
                services.AddTransient<HttpClient>();
                services.AddSingleton<IManifestService, ManifestService>();
            });

        app.Add<DownloadCommands>("download");
        app.Add<IndexCommands>("index");
        app.Add<NamelistCommands>("namelist");
        app.Add<PackCommands>("pack");
        app.Add<UnpackCommands>("unpack");

        app.Run(args);
    }
}
