using Errors;
using KeyGenerator.Models;
using KeyGenerator.Services.Factory;
using KeyGenerator.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;

namespace KeyGenerator.Commands;

public sealed class GenerateRsaCommand : Command<GenerateOptions>
{
    public override int Execute(CommandContext context, GenerateOptions settings)
    {

        try
        {
            var rsaOptions = new GenerateOptions
            {
                Algorithm = AlgorithmType.RSA,
                Size = settings.Size,
                OutputDir = settings.OutputDir,
                JsonOnly = settings.JsonOnly,
                Silent = settings.Silent,
                Verbose = settings.Verbose,
                Curve = null,
                Password = settings.Password
            };

            var service = KeyGeneratorFactory.Create(rsaOptions);
            KeyGenerationResult result = null!;

            CliSpinner.Run(
                !settings.JsonOnly && !settings.Silent,
                "Generating [bold cyan]RSA[/] keys... ",
                () => result = service.GenerateAsync(rsaOptions).GetAwaiter().GetResult()
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

        AnsiConsole.MarkupLine("[bold green] RSA keys generated![/]");
        AnsiConsole.MarkupLine($"[grey]Size:[/] [cyan]{result.KeySize} bits[/]");
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
