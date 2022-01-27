using CommandDotNet;
using CommandDotNet.Spectre;
using Mandible.Cli.Commands;
using System.Threading.Tasks;

namespace Mandible.Cli;

public class Program
{
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
            return await new AppRunner<Program>()
                .UseDefaultMiddleware()
                .UseSpectreAnsiConsole()
                .RunAsync(args);
        }
        catch (TaskCanceledException)
        {
            return 0;
        }
    }
}
