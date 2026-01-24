using Errors;
using KeyGenerator.Models;
using KeyGenerator.Services.Factory;
using KeyGenerator.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;

namespace KeyGenerator.Commands;

public sealed class GenerateEcdsaCommand : Command<GenerateOptions>
{
    public override int Execute(CommandContext context, GenerateOptions settings)
    {


        try
        {
            var ecdsaOptions = new GenerateOptions
            {
                Algorithm = AlgorithmType.ECDSA,
                Size = settings.Size,
                OutputDir = settings.OutputDir,
                JsonOnly = settings.JsonOnly,
                Silent = settings.Silent,
                Verbose = settings.Verbose,
                Curve = settings.Curve
            };

            var service = KeyGeneratorFactory.Create(ecdsaOptions);
            KeyGenerationResult result = null!;

            CliSpinner.Run(
                !settings.JsonOnly && !settings.Silent,
                "Generating [bold cyan]ECDSA[/] keys... ⠋",
                () => result = service.GenerateAsync(ecdsaOptions).GetAwaiter().GetResult()
            );

            OutputResult(result, settings);
            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            ErrorOutput(settings, ex.Message);
            return ExitCodes.Unexpected;
        }
    }

    static void OutputResult(KeyGenerationResult result, GenerateOptions settings)
    {
        if (settings.JsonOnly)
        {
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return;
        }

        AnsiConsole.MarkupLine("[bold green]✔ ECDSA keys generated![/]");
        AnsiConsole.MarkupLine($"[grey]Curve:[/] [cyan]{result.Curve}[/]");
        AnsiConsole.MarkupLine($"[grey]Public:[/] [cyan]{result.PublicKeyPath}[/]");
        AnsiConsole.MarkupLine($"[grey]Private:[/] [cyan]{result.PrivateKeyPath}[/]");
    }

    static void ErrorOutput(GenerateOptions settings, string msg)
    {
        if (settings.JsonOnly)
            Console.WriteLine(JsonSerializer.Serialize(new { error = msg }));
        else
            AnsiConsole.MarkupLine($"[red]Error: {msg}[/]");
    }
}
