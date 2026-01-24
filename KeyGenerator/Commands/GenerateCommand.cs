using Errors;
using KeyGenerator.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace KeyGenerator.Commands;

public sealed class GenerateCommand : Command<GenerateOptions>
{
    public override int Execute(CommandContext context, GenerateOptions settings)
    {
        // Only show subcommands info if running interactively and not silent/json
        if (!settings.JsonOnly && !settings.Silent)
        {
            AnsiConsole.MarkupLine("[bold yellow]Subcommands:[/]");
            AnsiConsole.MarkupLine("  [cyan]rsa[/] [dim]- RSA 2048/4096[/]");
            AnsiConsole.MarkupLine("  [cyan]ecdsa[/] [dim]- ECDSA P-256/P-384/P-521[/]");
            AnsiConsole.MarkupLine("\n[dim]Examples:[/]");
            AnsiConsole.MarkupLine("  [green]generate rsa -s 4096 -o keys[/]");
            AnsiConsole.MarkupLine("  [green]generate ecdsa --curve P-384[/]");
        }
        return ExitCodes.Success;
    }
}
