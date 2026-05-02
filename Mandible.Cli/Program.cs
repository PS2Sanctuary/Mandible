using ConsoleAppFramework;
using Mandible.Abstractions.Manifest;
using Mandible.Cli.Commands;
using Mandible.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Net.Http;
using ZLogger;

namespace Mandible.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        IAnsiConsole console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Out = new AnsiConsoleOutput(System.Console.Error),
        });

        ConsoleApp.ConsoleAppBuilder app = ConsoleApp.Create()
            .ConfigureLogging(x =>
            {
                x.ClearProviders();
                x.AddZLoggerConsole(opts => opts.UsePlainTextFormatter(formatter =>
                {
                    formatter.SetPrefixFormatter
                    (
                        $"[{0:datetime} {1:short}] [{2}|{3}] ",
                        (in MessageTemplate template, in LogInfo info)
                            => template.Format(info.Timestamp, info.LogLevel, info.Category, info.MemberName)
                    );
                }));
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IAnsiConsole>(console);
                services.AddTransient<HttpClient>();
                services.AddSingleton<IManifestService, ManifestService>();
            });

        app.Add<DownloadCommands>("download");
        app.Add<ImageCommands>("image");
        app.Add<IndexCommands>("index");
        app.Add<NamelistCommands>("namelist");
        app.Add<PackCommands>("pack");
        app.Add<UnpackCommands>("unpack");
        app.Add<ZoneFileCommands>("zone");

        app.Run(args);
    }
}
