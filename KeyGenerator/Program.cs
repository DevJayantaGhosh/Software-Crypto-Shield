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

            config.AddBranch<GenerateOptions>("generate", g =>
            {
                g.SetDescription("Generate RSA or ECDSA key pairs");

                g.SetDefaultCommand<GenerateCommand>();

                g.AddCommand<GenerateRsaCommand>("rsa")
                 .WithDescription("RSA keys (default: size=2048, out=keys/)")
                 .WithExample("generate rsa")
                 .WithExample("generate rsa -p MySecretPassword")
                 .WithExample("generate rsa -s 4096 -o ./mykeys -v")
                 .WithExample("generate rsa", "-s", "4096", "-o", "./prod-keys", "-p", "MySecretPassword", "--verbose");

                g.AddCommand<GenerateEcdsaCommand>("ecdsa")
                 .WithDescription("ECDSA keys (default: P-256, out=keys/)")
                 .WithExample("generate ecdsa")
                 .WithExample("generate ecdsa -p MySecretPassword")
                 .WithExample("generate ecdsa", "--curve", "P-384")
                 .WithExample("generate ecdsa", "-p", "MySecret", "-o", "./keys")
                 .WithExample("generate ecdsa --curve P-384 -o ./mykeys -j")
                 .WithExample("generate ecdsa", "--curve", "P-521", "-o", "./prod-keys", "-p", "MySecretPassword", "--json");
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
            // Basics
            .AddRow("[cyan]--help / -h[/]", "[dim]Show this help[/]")
            .AddRow("[cyan]--version / -v[/]", "[dim]Show version[/]")

            // RSA Examples
            .AddRow("[cyan]generate rsa[/]", "[dim]RSA Default (2048, Plain, ./keys)[/]")
            .AddRow("[cyan]generate rsa -p MySecret[/]", "[dim]RSA Encrypted[/]")
            .AddRow("[cyan]generate rsa -s 4096 -o ./mykeys -p MyPass -v[/]", "[dim]RSA Full (4096, Encrypted, Custom Dir, Verbose)[/]")

            // ECDSA Examples
            .AddRow("[cyan]generate ecdsa[/]", "[dim]ECDSA Default (P-256, Plain, ./keys)[/]")
            .AddRow("[cyan]generate ecdsa -p MySecret[/]", "[dim]ECDSA Encrypted[/]")
            .AddRow("[cyan]generate ecdsa -c P-384 -o ./mykeys -p MyPass -j[/]", "[dim]ECDSA Full (P-384, Encrypted, Custom Dir, JSON)[/]");

        AnsiConsole.MarkupLine("[bold yellow]Available Commands:[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[bold green]Type command or 'exit'[/]");
    }
}
