using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace SoftwareSigner.Models;

public sealed class SignOptions : CommandSettings
{
    [CommandOption("-c|--content")]
    [Description("Path to file or folder to sign")]
    public required string ContentPath { get; init; }

    [CommandOption("-k|--key")]
    [Description("Path to private key PEM file (required unless --privatekeystring is used)")]
    public string? PrivateKeyPath { get; init; }

    [CommandOption("--privatekeystring")]
    [Description("Private key PEM string passed directly ")]
    public string? PrivateKeyString { get; init; }

    [CommandOption("-o|--out")]
    [Description("Output path for signature file")]
    [DefaultValue("signature.sig")]
    public string OutputPath { get; init; } = "signature.sig";

    // Password protection
    [CommandOption("-p|--password")]
    [Description("Password for the private key (if encrypted)")]
    public string? Password { get; init; }

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
        // Check both Spectre-parsed value and pre-extracted value (from Program.ExtractedPrivateKeyString)
        bool hasKeyPath = !string.IsNullOrWhiteSpace(PrivateKeyPath);
        bool hasKeyString = !string.IsNullOrWhiteSpace(PrivateKeyString)
                         || !string.IsNullOrWhiteSpace(Program.ExtractedPrivateKeyString);

        if (!hasKeyPath && !hasKeyString)
            return ValidationResult.Error("You must provide either --key (-k) or --privatekeystring.");

        if (hasKeyPath && hasKeyString)
            return ValidationResult.Error("Provide only one of --key (-k) or --privatekeystring, not both.");

        return ValidationResult.Success();
    }
}
