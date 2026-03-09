using SoftwareVerifier.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using SoftwareVerifier.Commands;

namespace SoftwareVerifier;

public class Program
{
    /// <summary>
    /// Holds the --publickeystring value extracted before Spectre.Console.Cli parsing.
    /// Spectre's parser chokes on values starting with "-----" (interprets as malformed --option).
    /// </summary>
    public static string? ExtractedPublicKeyString { get; set; }

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
            var argsArray = ParseArguments(input);
            var exitCode = await RunCommand(argsArray);

            if (exitCode != 0)
                AnsiConsole.MarkupLine($"[yellow]Exit: {exitCode}[/]");
        }
    }

    private const string AppVersion = "1.0.0";

    /// <summary>
    /// Extracts a named option and its value from the args array before Spectre parses them.
    /// This prevents Spectre.Console.Cli from misinterpreting PEM strings (which start with "-----")
    /// as malformed long option names.
    /// </summary>
    static string? ExtractOptionValue(ref string[] args, string optionName)
    {
        var argsList = new List<string>(args);
        for (int i = 0; i < argsList.Count; i++)
        {
            if (argsList[i].Equals(optionName, StringComparison.OrdinalIgnoreCase))
            {
                string? value = null;
                if (i + 1 < argsList.Count)
                {
                    value = argsList[i + 1];
                    argsList.RemoveAt(i + 1);
                }
                argsList.RemoveAt(i);
                args = argsList.ToArray();
                return value;
            }
            // Also handle --option=VALUE syntax
            if (argsList[i].StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                string value = argsList[i].Substring(optionName.Length + 1);
                argsList.RemoveAt(i);
                args = argsList.ToArray();
                return value;
            }
        }
        return null;
    }

    static async Task<int> RunCommand(string[] args)
    {
        // Reset extracted value for each command invocation
        ExtractedPublicKeyString = null;

        // Handle --version manually (long-form only) to avoid -v conflicting with --verbose
        // Works at any position: "verify --version", "--version", etc.
        if (args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine($"[bold green]cryptoshield-verifier[/] [yellow]{AppVersion}[/]");
            return 0;
        }

        // Pre-extract --publickeystring before Spectre parses args
        // (PEM values start with "-----" which Spectre misinterprets as a --option)
        ExtractedPublicKeyString = ExtractOptionValue(ref args, "--publickeystring");

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