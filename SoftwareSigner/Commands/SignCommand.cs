using SoftwareSigner.Models;
using SoftwareSigner.Services.Implementations;
using SoftwareSigner.Services.Interfaces;
using SoftwareSigner.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;

namespace SoftwareSigner.Commands;

public sealed class SignCommand : Command<SignOptions>
{
    public override int Execute(CommandContext context, SignOptions settings)
    {
        try
        {
            ISignatureService service = new SignatureService();
            string signatureBase64 = "";
            bool usingKeyString = !string.IsNullOrWhiteSpace(settings.PrivateKeyString);

            CliSpinner.Run(
                !settings.JsonOnly && !settings.Silent,
                "Computing SHA512 hash and signing... ",
                () =>
                {
                    if (usingKeyString)
                    {
                        // Key-string mode: returns Base64 signature string, no file written
                        signatureBase64 = service.SignWithKeyStringAsync(
                            settings.ContentPath,
                            settings.PrivateKeyString!,
                            settings.Password
                        ).GetAwaiter().GetResult();
                    }
                    else
                    {
                        // File mode: writes signature to output file
                        signatureBase64 = service.SignAsync(
                            settings.ContentPath,
                            settings.PrivateKeyPath!,
                            settings.OutputPath,
                            settings.Password
                        ).GetAwaiter().GetResult();
                    }
                }
            );

            if (settings.JsonOnly)
            {
                if (usingKeyString)
                {
                    // Cloud/API mode: full signature string in JSON output
                    var result = new
                    {
                        status = "success",
                        content = settings.ContentPath,
                        keySource = "privatekeystring",
                        signatureString = signatureBase64,
                        timestamp = DateTime.UtcNow
                    };
                    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    // File mode: reference the output file
                    var result = new
                    {
                        status = "success",
                        content = settings.ContentPath,
                        keySource = "file",
                        output = settings.OutputPath,
                        signature_preview = signatureBase64[..Math.Min(30, signatureBase64.Length)] + "..."
                    };
                    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[bold green] Content Signed Successfully![/]");
                AnsiConsole.MarkupLine($"[grey]Content Path:[/] [cyan]{settings.ContentPath}[/]");

                if (usingKeyString)
                {
                    // Cloud/API mode: print full signature string to stdout
                    AnsiConsole.MarkupLine($"[grey]Private Key :[/] [cyan](provided via --privatekeystring)[/]");
                    AnsiConsole.MarkupLine($"[grey]Output      :[/] [cyan](signature string returned to stdout)[/]");
                    AnsiConsole.MarkupLine("\n[bold yellow]Signature String (Base64):[/]");
                    // Use Console.WriteLine for clean, parseable output
                    Console.WriteLine(signatureBase64);
                }
                else
                {
                    // File mode
                    AnsiConsole.MarkupLine($"[grey]Private Key :[/] [cyan]{settings.PrivateKeyPath}[/]");
                    AnsiConsole.MarkupLine($"[grey]Signature   :[/] [cyan]{settings.OutputPath}[/]");

                    if (settings.Verbose)
                    {
                        AnsiConsole.MarkupLine("\n[dim]Signature Preview (Base64):[/]");
                        AnsiConsole.MarkupLine($"[dim]{signatureBase64[..Math.Min(50, signatureBase64.Length)]}...[/]");
                    }
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            if (settings.JsonOnly)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { error = ex.Message }));
            }
            else
            {
                string safeMessage = Markup.Escape(ex.Message);
                AnsiConsole.MarkupLine($"[red]Error: {safeMessage}[/]");

                if (settings.Verbose)
                    AnsiConsole.WriteException(ex);
            }

            return 1;
        }
    }
}