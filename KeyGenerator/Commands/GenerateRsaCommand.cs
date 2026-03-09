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
                Password = settings.Password,
                KeyString = settings.KeyString
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
        // KeyString mode: output key strings
        if (settings.KeyString)
        {
            if (settings.JsonOnly)
            {
                var jsonObj = new
                {
                    algorithm = result.Algorithm.ToString(),
                    keySize = result.KeySize,
                    createdAtUtc = result.CreatedAtUtc,
                    publicKeyBytes = result.PublicKeyBytes,
                    privateKeyBytes = result.PrivateKeyBytes,
                    publicKey = result.PublicKeyString,
                    privateKey = result.PrivateKeyString,
                    passwordProtected = !string.IsNullOrEmpty(result.PrivateKeyString) &&
                                        result.PrivateKeyString.Contains("ENCRYPTED")
                };
                Console.WriteLine(JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                AnsiConsole.MarkupLine("[bold green] RSA keys generated (keystring mode)![/]");
                AnsiConsole.MarkupLine($"[grey]Size:[/] [cyan]{result.KeySize} bits[/]");
                AnsiConsole.MarkupLine("");
                AnsiConsole.MarkupLine("[bold yellow]===== PUBLIC KEY =====[/]");
                Console.WriteLine(result.PublicKeyString);
                AnsiConsole.MarkupLine("");
                AnsiConsole.MarkupLine("[bold yellow]===== PRIVATE KEY =====[/]");
                Console.WriteLine(result.PrivateKeyString);
            }
            return;
        }

        // Standard file mode output
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