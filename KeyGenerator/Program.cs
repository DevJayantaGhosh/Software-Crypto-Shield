using Errors;
using KeyGenerator.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using KeyGenerator.Commands;
using KeyGenerator.Models;

namespace KeyGenerator;

public class Program
{
    private static bool _bannerShown = false;

    public static async Task<int> Main(string[] args)
    {
        // 1. DIRECT EXECUTION MODE
        if (args.Length > 0)
        {
            return await RunCommand(args);
        }

        // 2. Interactive Mode
        AppBannerPrinter.Print();
        ShowInteractiveHelp();

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold cyan]cryptoshield-keygen> [/]")
                    .AllowEmpty()
            );

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[bold green]Goodbye![/]");
                return ExitCodes.Success;
            }

            if (string.IsNullOrWhiteSpace(input)) continue;

            var argsArray = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var exitCode = await RunCommand(argsArray);

            if (exitCode != ExitCodes.Success)
                AnsiConsole.MarkupLine($"[yellow]Exit: {exitCode}[/]");
        }
    }

    static async Task<int> RunCommand(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("cryptoshield-keygen");
            config.SetApplicationVersion("1.0.0");

            // Use AddBranch<GenerateOptions> and SetDefaultCommand
            config.AddBranch<GenerateOptions>("generate", g =>
            {
                g.SetDescription("Generate RSA or ECDSA key pairs");

                g.SetDefaultCommand<GenerateCommand>();

                g.AddCommand<GenerateRsaCommand>("rsa")
                 .WithDescription("RSA keys (default: size=2048, out=keys/)")
                 .WithExample("rsa -s 4096 -o ./mykeys -v");

                g.AddCommand<GenerateEcdsaCommand>("ecdsa")
                 .WithDescription("ECDSA keys (default: P-256, out=keys/)")
                 .WithExample("ecdsa --curve P-384 -o ./mykeys -j");
            });
        });

        if (args.Length == 0) return ExitCodes.Success;

        try
        {
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return ExitCodes.Unexpected;
        }
    }

    static void ShowInteractiveHelp()
    {
        var table = new Table()
            .AddColumns("Command", "Description")
            .AddRow("[cyan]--help / -h[/]", "[dim]Show this help[/]")
            .AddRow("[cyan]--version / -v[/]", "[dim]Show version[/]")
            .AddRow("[cyan]generate rsa[/]", "[dim]RSA (default: 2048, keys/)[/]")
            .AddRow("[cyan]generate rsa --help[/]", "[dim]Find other options[/]")
            .AddRow("[cyan]generate rsa -s 4096 -o ./mykeys[/]", "[dim]Full command example[/]")
            .AddRow("[cyan]generate ecdsa[/]", "[dim]ECDSA (default: P-256, keys/)[/]")
            .AddRow("[cyan]generate ecdsa --help[/]", "[dim]Find other options[/]")
            .AddRow("[cyan]generate ecdsa -c P-384 -o ./mykeys[/]", "[dim]Full command example[/]");

        AnsiConsole.MarkupLine("[bold yellow]Available Commands:[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[bold green]Type command or 'exit'[/]");
    }
}
