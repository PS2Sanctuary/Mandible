using CommandDotNet;
using CommandDotNet.IoC.MicrosoftDependencyInjection;
using CommandDotNet.Spectre;
using Mandible.Abstractions.Manifest;
using Mandible.Cli.Commands;
using Mandible.Manifest;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mandible.Cli;

public class Program
{
    //[Subcommand]
    public DownloadCommands? DownloadCommands { get; set; }

    [Subcommand]
    public IndexCommands? IndexCommands { get; set; }

    [Subcommand]
    public NamelistCommands? NamelistCommands { get; set; }

    [Subcommand]
    public UnpackCommands? UnpackCommands { get; set; }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            ServiceCollection services = new();
            services.AddTransient<HttpClient>();
            services.AddSingleton<IManifestService, ManifestService>();

            return await new AppRunner<Program>()
                .UseDefaultMiddleware()
                .UseSpectreAnsiConsole()
                //.UseMicrosoftDependencyInjection(services.BuildServiceProvider())
                .RunAsync(args);
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
    }
}
