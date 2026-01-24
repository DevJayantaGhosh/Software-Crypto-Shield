using KeyGenerator.Models;
using KeyGenerator.Services.Factory;
using KeyGenerator.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace KeyGenerator.Commands;

public sealed class GenerateCommand : Command<GenerateOptions>
{
    public override int Execute(CommandContext context, GenerateOptions settings)
    {
        // -------------------------
        // Banner (disabled for JSON or Silent)
        // -------------------------
        if (!settings.JsonOnly && !settings.Silent)
        {
            AppBannerPrinter.Print();
        }

        // -------------------------
        // Prepare options
        // -------------------------
        var options = settings; // already a CommandSettings model

        // -------------------------
        // Key generator service
        // -------------------------
        var service = KeyGeneratorFactory.Create(options);

        KeyGenerationResult result = null!;

        // -------------------------
        // Spinner for long operations
        // -------------------------
        CliSpinner.Run(
            enabled: !settings.JsonOnly && !settings.Silent,
            message: "Generating cryptographic keys...",
            action: () =>
            {
                result = service.GenerateAsync(options).GetAwaiter().GetResult();
            }
        );

        // -------------------------
        // Output results
        // -------------------------
        if (settings.JsonOnly)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                result,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            ));
        }
        else
        {
            AnsiConsole.MarkupLine("[bold green]✔ Keys generated successfully[/]");
            AnsiConsole.MarkupLine($"[grey]Public :[/] {result.PublicKeyPath}");
            AnsiConsole.MarkupLine($"[grey]Private:[/] {result.PrivateKeyPath}");
        }

        return 0;
    }
}
