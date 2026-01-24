using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using SoftwareVerifier.Models;
using SoftwareVerifier.Services.Factories;
using SoftwareVerifier.Services.Implementations;
using SoftwareVerifier.Services.Interfaces;
using SoftwareVerifier.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;

namespace SoftwareVerifier.Commands;

public sealed class VerifyCommand : Command<VerifyOptions>
{
    public override int Execute(CommandContext context, VerifyOptions settings)
    {
        // NO Banner here (Handled by Program.cs)

        try
        {
            // 1. Direct instantiation (No DI)
            IHashService hashService = new HashService();
            bool isValid = false;

            CliSpinner.Run(
                !settings.JsonOnly && !settings.Silent,
                "Verifying signature integrity... ",
                () =>
                {
                    // A. Load Public Key
                    var publicKey = LoadPublicKey(settings.PublicKeyPath);

                    // B. Get correct Verifier (RSA or ECDSA) from Factory
                    ISignatureVerifier verifier = VerifierFactory.GetVerifier(publicKey);

                    // C. Compute Hash (Must match Signer exactly)
                    var contentHash = hashService.ComputeHashAsync(settings.ContentPath)
                                                 .GetAwaiter().GetResult();

                    // D. Read Signature File
                    if (!File.Exists(settings.SignaturePath))
                        throw new FileNotFoundException($"Signature file not found: {settings.SignaturePath}");

                    var signatureBytes = File.ReadAllBytes(settings.SignaturePath);

                    // E. Verify
                    isValid = verifier.VerifySignature(contentHash, signatureBytes, publicKey);
                }
            );

            // Handle Output (JSON vs Interactive)
            if (settings.JsonOnly)
            {
                var result = new
                {
                    status = isValid ? "valid" : "invalid",
                    content = settings.ContentPath,
                    signature_file = settings.SignaturePath,
                    timestamp = DateTime.UtcNow
                };
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                if (isValid)
                {
                    AnsiConsole.MarkupLine("\n[bold green] VALID SIGNATURE !  [/]");
                    AnsiConsole.MarkupLine("[green]The content is authentic and has not been modified.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[bold red] INVALID SIGNATURE ! [/]");
                    AnsiConsole.MarkupLine("[red]Warning: Content may have been tampered with or the key is incorrect.[/]");
                }

                if (settings.Verbose)
                {
                    AnsiConsole.MarkupLine($"[grey]Content :[/] [cyan]{settings.ContentPath}[/]");
                    AnsiConsole.MarkupLine($"[grey]Key     :[/] [cyan]{settings.PublicKeyPath}[/]");
                    AnsiConsole.MarkupLine($"[grey]Sig     :[/] [cyan]{settings.SignaturePath}[/]");
                }
            }

            // Return 0 for Valid, 1 for Invalid (Common for CI/CD)
            return isValid ? 0 : 1;
        }
        catch (Exception ex)
        {
            if (settings.JsonOnly)
            {
                Console.WriteLine(JsonSerializer.Serialize(new { status = "error", error = ex.Message }));
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                if (settings.Verbose)
                    AnsiConsole.WriteException(ex);
            }

            return 2; // Error execution code
        }
    }

    // Helper to load PEM key
    private AsymmetricKeyParameter LoadPublicKey(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Public key not found: {path}");

        using var reader = File.OpenText(path);
        var pemReader = new PemReader(reader);
        var keyObject = pemReader.ReadObject();

        if (keyObject is AsymmetricKeyParameter key) return key;

        throw new Exception($"Invalid PEM public key format in {path}");
    }
}
