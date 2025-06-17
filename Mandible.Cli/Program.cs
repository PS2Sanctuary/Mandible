using ConsoleAppFramework;
using Mandible.Abstractions.Manifest;
using Mandible.Cli.Commands;
using Mandible.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.Net.Http;

namespace Mandible.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
                services.AddTransient<HttpClient>();
                services.AddSingleton<IManifestService, ManifestService>();
            });

        app.Add<DownloadCommands>("download");
        app.Add<IndexCommands>("index");
        app.Add<NamelistCommands>("namelist");
        app.Add<UnpackCommands>("unpack");

        app.Run(args);
    }
}
