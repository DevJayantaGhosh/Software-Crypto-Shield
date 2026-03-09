using SoftwareVerifier.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using SoftwareVerifier.Commands;

namespace SoftwareVerifier;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // 1. DIRECT EXECUTION MODE (e.g., "SoftwareVerifier.exe verify -c ./bin -k key.pem")
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
                new TextPrompt<string>("[bold cyan]cryptoshield-verifier> [/]")
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

    private const string AppVersion = "1.0.0";

    static async Task<int> RunCommand(string[] args)
    {
        // Handle --version manually (long-form only) to avoid -v conflicting with --verbose
        // Works at any position: "verify --version", "--version", etc.
        if (args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine($"[bold green]cryptoshield-verifier[/] [yellow]{AppVersion}[/]");
            return 0;
        }

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("cryptoshield-verifier");

            config.AddCommand<VerifyCommand>("verify")
                .WithDescription("Verify digital signature for file or folder")
                // Example 1: Basic (file-based)
                .WithExample("verify", "-c", "./build", "-k", "./public.pem", "-s", "release.sig")
                // Example 2: Public key string + signature file
                .WithExample("verify", "-c", "./build", "--publickeystring", "\"-----BEGIN PUBLIC KEY-----...-----END PUBLIC KEY-----\"", "-s", "release.sig")
                // Example 3: Public key file + signature string
                .WithExample("verify", "-c", "./build", "-k", "./public.pem", "--signaturestring", "\"BASE64_SIGNATURE_STRING\"")
                // Example 4: Both strings (full cloud/API mode)
                .WithExample("verify", "-c", "./build", "--publickeystring", "\"-----BEGIN PUBLIC KEY-----...-----END PUBLIC KEY-----\"", "--signaturestring", "\"BASE64_SIGNATURE_STRING\"");
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
            .AddRow("[cyan]--version[/]", "[dim]Show version[/]")
            .AddRow("[cyan]verify --help[/]", "[dim]Show verification options[/]")
            .AddRow("[cyan]verify -c <path> -k <pub> -s <sig>[/]", "[dim]Basic verification (files)[/]")
            .AddRow("[cyan]verify -c <path> --publickeystring \"PEM...\" -s <sig>[/]", "[dim]Verify with key string + sig file[/]")
            .AddRow("[cyan]verify -c <path> -k <pub> --signaturestring \"B64...\"[/]", "[dim]Verify with key file + sig string[/]")
            .AddRow("[cyan]verify -c <path> --publickeystring \"PEM...\" --signaturestring \"B64...\"[/]", "[dim]Full cloud/API mode (all strings)[/]");

        AnsiConsole.MarkupLine("[bold yellow]Available Commands:[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[bold green]Type command or 'exit'[/]");
    }
}