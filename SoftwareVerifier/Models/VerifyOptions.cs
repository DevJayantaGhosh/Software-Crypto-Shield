using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SoftwareVerifier.Models;

public sealed class VerifyOptions : CommandSettings
{
    [CommandOption("-c|--content")]
    [Description("Path to file or folder to verify")]
    public required string ContentPath { get; init; }

    [CommandOption("-k|--key")]
    [Description("Path to PUBLIC key PEM file (required unless --publickeystring is used)")]
    public string? PublicKeyPath { get; init; }

    [CommandOption("--publickeystring")]
    [Description("Public key PEM string passed directly")]
    public string? PublicKeyString { get; init; }

    [CommandOption("-s|--signature")]
    [Description("Path to .sig signature file (required unless --signaturestring is used)")]
    public string? SignaturePath { get; init; }

    [CommandOption("--signaturestring")]
    [Description("Base64-encoded signature string passed directly")]
    public string? SignatureString { get; init; }

    [CommandOption("-j|--json")]
    [Description("Output result as JSON")]
    public bool JsonOnly { get; init; }

    [CommandOption("--silent")]
    [Description("Suppress banner and spinner")]
    public bool Silent { get; init; }

    [CommandOption("-v|--verbose")]
    [Description("Show verbose output")]
    public bool Verbose { get; init; }

    public override ValidationResult Validate()
    {
        // Check both Spectre-parsed value and pre-extracted value (from Program.ExtractedPublicKeyString)
        bool hasKeyPath = !string.IsNullOrWhiteSpace(PublicKeyPath);
        bool hasKeyString = !string.IsNullOrWhiteSpace(PublicKeyString)
                         || !string.IsNullOrWhiteSpace(Program.ExtractedPublicKeyString);

        // Public key: must provide exactly one source
        if (!hasKeyPath && !hasKeyString)
            return ValidationResult.Error("You must provide either --key (-k) or --publickeystring.");

        if (hasKeyPath && hasKeyString)
            return ValidationResult.Error("Provide only one of --key (-k) or --publickeystring, not both.");

        // Signature: must provide exactly one source
        if (string.IsNullOrWhiteSpace(SignaturePath) && string.IsNullOrWhiteSpace(SignatureString))
            return ValidationResult.Error("You must provide either --signature (-s) or --signaturestring.");

        if (!string.IsNullOrWhiteSpace(SignaturePath) && !string.IsNullOrWhiteSpace(SignatureString))
            return ValidationResult.Error("Provide only one of --signature (-s) or --signaturestring, not both.");

        return ValidationResult.Success();
    }
}