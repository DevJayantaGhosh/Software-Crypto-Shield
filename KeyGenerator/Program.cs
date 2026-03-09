using Errors;
using KeyGenerator.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using KeyGenerator.Commands;
using KeyGenerator.Models;

namespace KeyGenerator;

public class Program
{
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

            var argsArray = ParseArguments(input);
            var exitCode = await RunCommand(argsArray);

            if (exitCode != ExitCodes.Success)
                AnsiConsole.MarkupLine($"[yellow]Exit: {exitCode}[/]");
        }
    }

    private const string AppVersion = "1.0.0";

    static async Task<int> RunCommand(string[] args)
    {
        // Handle --version manually (long-form only) to avoid -v conflicting with --verbose
        // Works at any position: "generate --version", "--version", etc.
        if (args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine($"[bold green]cryptoshield-keygen[/] [yellow]{AppVersion}[/]");
            return ExitCodes.Success;
        }

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("cryptoshield-keygen");

            config.AddBranch<GenerateOptions>("generate", g =>
            {
                g.SetDescription("Generate RSA or ECDSA key pairs");

                g.SetDefaultCommand<GenerateCommand>();

                g.AddCommand<GenerateRsaCommand>("rsa")
                 .WithDescription("RSA keys (default: size=2048, out=keys/)")
                 .WithExample("generate rsa")
                 .WithExample("generate rsa -p MySecretPassword")
                 .WithExample("generate rsa -s 4096 -o ./mykeys -v")
                 .WithExample("generate rsa", "-s", "4096", "-o", "./prod-keys", "-p", "MySecretPassword", "--verbose")
                 .WithExample("generate rsa --keystring")
                 .WithExample("generate rsa", "-s", "4096", "--keystring", "--json")
                 .WithExample("generate rsa", "-s", "4096", "-p", "MySecretPassword", "--keystring", "--json");

                g.AddCommand<GenerateEcdsaCommand>("ecdsa")
                 .WithDescription("ECDSA keys (default: P-256, out=keys/)")
                 .WithExample("generate ecdsa")
                 .WithExample("generate ecdsa -p MySecretPassword")
                 .WithExample("generate ecdsa", "--curve", "P-384")
                 .WithExample("generate ecdsa", "-p", "MySecret", "-o", "./keys")
                 .WithExample("generate ecdsa --curve P-384 -o ./mykeys -j")
                 .WithExample("generate ecdsa", "--curve", "P-521", "-o", "./prod-keys", "-p", "MySecretPassword", "--json")
                 .WithExample("generate ecdsa --keystring")
                 .WithExample("generate ecdsa", "--curve", "P-384", "--keystring", "--json")
                 .WithExample("generate ecdsa", "--curve", "P-384", "-p", "MySecretPassword", "--keystring", "--json");
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

    static string[] ParseArguments(string input)
    {
        var args = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inDoubleQuotes = false;
        bool inSingleQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"' && !inSingleQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
                continue;
            }

            if (c == '\'' && !inDoubleQuotes)
            {
                inSingleQuotes = !inSingleQuotes;
                continue;
            }

            if (c == ' ' && !inDoubleQuotes && !inSingleQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
            args.Add(current.ToString());

        return args.ToArray();
    }

    static void ShowInteractiveHelp()
    {
        var table = new Table()
            .AddColumns("Command", "Description")
            // Basics
            .AddRow("[cyan]--help / -h[/]", "[dim]Show this help[/]")
            .AddRow("[cyan]--version[/]", "[dim]Show version[/]")

            // RSA Examples
            .AddRow("[cyan]generate rsa[/]", "[dim]RSA Default (2048, Plain, ./keys)[/]")
            .AddRow("[cyan]generate rsa -p MySecret[/]", "[dim]RSA Encrypted[/]")
            .AddRow("[cyan]generate rsa -s 4096 -o ./mykeys -p MyPass -v[/]", "[dim]RSA Full (4096, Encrypted, Custom Dir, Verbose)[/]")
            .AddRow("[cyan]generate rsa --keystring[/]", "[dim]RSA KeyString (output keys to stdout, no files)[/]")
            .AddRow("[cyan]generate rsa --keystring -j[/]", "[dim]RSA KeyString JSON (cloud/API mode)[/]")
            .AddRow("[cyan]generate rsa -s 4096 -p MyPass --keystring -j[/]", "[dim]RSA KeyString (4096, Encrypted, JSON)[/]")

            // ECDSA Examples
            .AddRow("[cyan]generate ecdsa[/]", "[dim]ECDSA Default (P-256, Plain, ./keys)[/]")
            .AddRow("[cyan]generate ecdsa -p MySecret[/]", "[dim]ECDSA Encrypted[/]")
            .AddRow("[cyan]generate ecdsa -c P-384 -o ./mykeys -p MyPass -j[/]", "[dim]ECDSA Full (P-384, Encrypted, Custom Dir, JSON)[/]")
            .AddRow("[cyan]generate ecdsa --keystring[/]", "[dim]ECDSA KeyString (output keys to stdout, no files)[/]")
            .AddRow("[cyan]generate ecdsa --keystring -j[/]", "[dim]ECDSA KeyString JSON (cloud/API mode)[/]")
            .AddRow("[cyan]generate ecdsa -c P-384 -p MyPass --keystring -j[/]", "[dim]ECDSA KeyString (P-384, Encrypted, JSON)[/]");

        AnsiConsole.MarkupLine("[bold yellow]Available Commands:[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[bold green]Type command or 'exit'[/]");
    }
}
