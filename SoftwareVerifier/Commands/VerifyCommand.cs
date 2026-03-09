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
        try
        {
            IHashService hashService = new HashService();
            bool isValid = false;
            bool usingKeyString = !string.IsNullOrWhiteSpace(settings.PublicKeyString);
            bool usingSignatureString = !string.IsNullOrWhiteSpace(settings.SignatureString);

            CliSpinner.Run(
                !settings.JsonOnly && !settings.Silent,
                "Verifying signature integrity... ",
                () =>
                {
                    // A. Load Public Key (from file or string)
                    var publicKey = usingKeyString
                        ? LoadPublicKeyFromString(settings.PublicKeyString!)
                        : LoadPublicKey(settings.PublicKeyPath!);

                    // B. Get correct Verifier (RSA or ECDSA) from Factory
                    ISignatureVerifier verifier = VerifierFactory.GetVerifier(publicKey);

                    // C. Compute Hash (Must match Signer exactly)
                    var contentHash = hashService.ComputeHashAsync(settings.ContentPath)
                                                 .GetAwaiter().GetResult();

                    // D. Read Signature (from file or string)
                    byte[] signatureBytes;
                    if (usingSignatureString)
                    {
                        try
                        {
                            signatureBytes = Convert.FromBase64String(settings.SignatureString!);
                        }
                        catch (FormatException)
                        {
                            throw new ArgumentException("The --signaturestring value is not valid Base64. Please provide the signature as a Base64-encoded string.");
                        }
                    }
                    else
                    {
                        if (!File.Exists(settings.SignaturePath))
                            throw new FileNotFoundException($"Signature file not found: {settings.SignaturePath}");

                        signatureBytes = File.ReadAllBytes(settings.SignaturePath!);
                    }

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
                    keySource = usingKeyString ? "publickeystring" : "file",
                    signatureSource = usingSignatureString ? "signaturestring" : "file",
                    signature_file = usingSignatureString ? "(provided via --signaturestring)" : settings.SignaturePath,
                    timestamp = DateTime.UtcNow
                };
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                if (isValid)
                {
                    AnsiConsole.MarkupLine("\n[bold green] SIGNATURE VERIFICATION: SUCCESS !  [/]");
                    AnsiConsole.MarkupLine("[green]The content is authentic and has not been modified.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[bold red] SIGNATURE VERIFICATION: FAILED ! [/]");
                    AnsiConsole.MarkupLine("[red]Error: Content may have been tampered or the key is incorrect.[/]");
                }

                if (settings.Verbose)
                {
                    AnsiConsole.MarkupLine($"[grey]Content :[/] [cyan]{settings.ContentPath}[/]");
                    if (usingKeyString)
                        AnsiConsole.MarkupLine($"[grey]Key     :[/] [cyan](provided via --publickeystring)[/]");
                    else
                        AnsiConsole.MarkupLine($"[grey]Key     :[/] [cyan]{settings.PublicKeyPath}[/]");
                    if (usingSignatureString)
                        AnsiConsole.MarkupLine($"[grey]Sig     :[/] [cyan](provided via --signaturestring)[/]");
                    else
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

    // Load public key from PEM string
    private AsymmetricKeyParameter LoadPublicKeyFromString(string pemContent)
    {
        if (string.IsNullOrWhiteSpace(pemContent))
            throw new ArgumentException("Public key string is empty or null.");

        using var reader = new StringReader(pemContent);
        var pemReader = new PemReader(reader);
        object? keyObject;

        try
        {
            keyObject = pemReader.ReadObject();
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to parse the public key string: {ex.Message}");
        }

        if (keyObject == null)
            throw new ArgumentException("Could not read a valid key object from the provided key string. Is it a valid PEM?");

        if (keyObject is AsymmetricKeyParameter key)
        {
            if (key.IsPrivate)
                throw new ArgumentException("The provided key string contains a PRIVATE key! Verification requires a PUBLIC key.");
            return key;
        }

        throw new ArgumentException($"Unknown key format in key string: {keyObject.GetType().Name}. Expected a public key.");
    }

    // Load public key from PEM file
    private AsymmetricKeyParameter LoadPublicKey(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Public key not found: {path}");

        using var reader = File.OpenText(path);
        var pemReader = new PemReader(reader);
        var keyObject = pemReader.ReadObject();

        if (keyObject == null) throw new ArgumentException("Could not parse PEM file. Is it valid?");

        if (keyObject is AsymmetricKeyParameter key)
        {
            // CRITICAL CHECK: Ensure user didn't pass a Private Key by mistake
            if (key.IsPrivate)
            {
                throw new ArgumentException("The provided key is a PRIVATE key! verification requires a PUBLIC key. Please check your file path.");
            }
            return key;
        }

        throw new Exception($"Invalid PEM public key format in {path}. Found type: {keyObject.GetType().Name}");
    }
}