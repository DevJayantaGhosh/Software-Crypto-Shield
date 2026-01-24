using Spectre.Console.Cli;
using System.ComponentModel;

namespace SoftwareSigner.Models;

public sealed class SignOptions : CommandSettings
{
    [CommandOption("-c|--content")]
    [Description("Path to file or folder to sign")]
    public required string ContentPath { get; init; }

    [CommandOption("-k|--key")]
    [Description("Path to private key PEM file")]
    public required string PrivateKeyPath { get; init; }

    [CommandOption("-o|--out")]
    [Description("Output path for signature file")]
    [DefaultValue("signature.sig")]
    public string OutputPath { get; init; } = "signature.sig"; 

    [CommandOption("-j|--json")]
    [Description("Output result as JSON")]
    public bool JsonOnly { get; init; }

    [CommandOption("--silent")]
    [Description("Suppress banner and spinner")]
    public bool Silent { get; init; }

    [CommandOption("-v|--verbose")]
    [Description("Show verbose output")]
    public bool Verbose { get; init; }
}
