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

            CliSpinner.Run(
                !settings.JsonOnly && !settings.Silent,
                "Computing SHA512 hash and signing... ",
                () =>
                {
                    signatureBase64 = service.SignAsync(
                        settings.ContentPath,
                        settings.PrivateKeyPath,
                        settings.OutputPath,
                        settings.Password
                    ).GetAwaiter().GetResult();
                }
            );

            if (settings.JsonOnly)
            {
                var result = new
                {
                    status = "success",
                    content = settings.ContentPath,
                    output = settings.OutputPath,
                    signature_preview = signatureBase64[..Math.Min(30, signatureBase64.Length)] + "..."
                };
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                AnsiConsole.MarkupLine("[bold green] Content Signed Successfully![/]");
                AnsiConsole.MarkupLine($"[grey]Content Path:[/] [cyan]{settings.ContentPath}[/]");
                AnsiConsole.MarkupLine($"[grey]Private Key :[/] [cyan]{settings.PrivateKeyPath}[/]");
                AnsiConsole.MarkupLine($"[grey]Signature   :[/] [cyan]{settings.OutputPath}[/]");

                if (settings.Verbose)
                {
                    AnsiConsole.MarkupLine("\n[dim]Signature Preview (Base64):[/]");
                    AnsiConsole.MarkupLine($"[dim]{signatureBase64[..50]}...[/]");
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
