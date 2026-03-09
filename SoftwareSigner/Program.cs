using SoftwareSigner.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using SoftwareSigner.Commands;

namespace SoftwareSigner;

public class Program
{
    /// <summary>
    /// Holds the --privatekeystring value extracted before Spectre.Console.Cli parsing.
    /// Spectre's parser chokes on values starting with "-----" (interprets as malformed --option).
    /// </summary>
    public static string? ExtractedPrivateKeyString { get; set; }

    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0)
        {
            return await RunCommand(args);
        }

        AppBannerPrinter.Print();
        ShowInteractiveHelp();

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold magenta]cryptoshield-signer> [/]")
                    .AllowEmpty()
            );

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[bold green]Goodbye![/]");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(input)) continue;

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
    /// Returns the extracted value (or null), and outputs the cleaned args array.
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
            // Also handle --privatekeystring=VALUE syntax
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
        ExtractedPrivateKeyString = null;

        // Handle --version manually (long-form only) to avoid -v conflicting with --verbose
        // Works at any position: "sign --version", "--version", etc.
        if (args.Any(a => a.Equals("--version", StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine($"[bold green]cryptoshield-signer[/] [yellow]{AppVersion}[/]");
            return 0;
        }

        // Pre-extract --privatekeystring before Spectre parses args
        // (PEM values start with "-----" which Spectre misinterprets as a --option)
        ExtractedPrivateKeyString = ExtractOptionValue(ref args, "--privatekeystring");

        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("cryptoshield-signer");

            config.AddCommand<SignCommand>("sign")
                .WithDescription("Sign a file or recursive folder content")
                // Example 1: Basic (file-based key)
                .WithExample("sign", "-c", "./bin", "-k", "key.pem")
                // Example 2: With Password (file-based key)
                .WithExample("sign", "-c", "./bin", "-k", "key.pem", "-p", "MySecret")
                // Example 3: Full (file-based key)
                .WithExample("sign", "-c", "./bin", "-k", "key.pem", "-o", "release.sig", "-p", "MySecret")
                // Example 4: Private key string (cloud/API mode)
                .WithExample("sign", "-c", "./bin", "--privatekeystring", "\"-----BEGIN RSA PRIVATE KEY-----...-----END RSA PRIVATE KEY-----\"")
                // Example 5: Private key string with password
                .WithExample("sign", "-c", "./bin", "--privatekeystring", "\"-----BEGIN ENCRYPTED PRIVATE KEY-----...-----END ENCRYPTED PRIVATE KEY-----\"", "-p", "MySecret");
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
            .AddRow("[cyan]sign --help[/]", "[dim]Show signing options[/]")
            .AddRow("[cyan]sign -c <path> -k <key>[/]", "[dim]Basic signing (key file)[/]")
            .AddRow("[cyan]sign -c <path> -k <key> -p <pass>[/]", "[dim]Sign with encrypted key file[/]")
            .AddRow("[cyan]sign -c ./bin -k key.pem -o bin.sig[/]", "[dim]Full example (key file)[/]")
            .AddRow("[cyan]sign -c <path> --privatekeystring \"PEM...\"[/]", "[dim]Sign with key string (cloud/API)[/]")
            .AddRow("[cyan]sign -c <path> --privatekeystring \"PEM...\" -p <pass>[/]", "[dim]Sign with encrypted key string[/]");

        AnsiConsole.MarkupLine("[bold yellow]Available Commands:[/]");
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[bold green]Type command or 'exit'[/]");
    }
}
