using SoftwareSigner.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using SoftwareSigner.Commands;

namespace SoftwareSigner;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 1. DIRECT EXECUTION MODE (e.g., "SoftwareSigner.exe sign -c ./bin -k key.pem")
        if (args.Length > 0)
        {
            return await RunCommand(args);
        }

        // 2. Interactive Mode
        // Show Banner and Help ONCE at startup
        AppBannerPrinter.Print();
        ShowInteractiveHelp();

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold magenta]cryptoshield-signer> [/]")
                    .AllowEmpty()
            );

            // Handle Exit
            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[bold green]Goodbye![/]");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(input)) continue;

            // Run Command
            var argsArray = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var exitCode = await RunCommand(argsArray);

            if (exitCode != 0)
                AnsiConsole.MarkupLine($"[yellow]Exit: {exitCode}[/]");
        }
    }

    static async Task<int> RunCommand(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("cryptoshield-signer");
            config.SetApplicationVersion("1.0.0");

            config.AddCommand<SignCommand>("sign")
                .WithDescription("Sign a file or recursive folder content")
                .WithExample("sign -c ./build -k ./private.pem -o release.sig");
        });

        if (args.Length == 0) return 0;

        try
        {
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    static void ShowInteractiveHelp()
    {
        var table = new Table()
            .AddColumns("Command", "Description")
            .AddRow("[cyan]--help / -h[/]", "[dim]Show this help[/]")
            .AddRow("[cyan]--version / -v[/]", "[dim]Show version[/]")
            .AddRow("[cyan]sign --help[/]", "[dim]Show signing options[/]")
            .AddRow("[cyan]sign -c <path> -k <key>[/]", "[dim]Basic signing[/]")
            .AddRow("[cyan]sign -c ./bin -k key.pem -o bin.sig[/]", "[dim]Full example[/]");

        AnsiConsole.MarkupLine("[bold yellow]Available Commands:[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[bold green]Type command or 'exit'[/]");
    }
}
