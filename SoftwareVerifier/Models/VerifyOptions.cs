using Spectre.Console.Cli;
using System.ComponentModel;

namespace SoftwareVerifier.Models;

public sealed class VerifyOptions : CommandSettings
{
    [CommandOption("-c|--content")]
    [Description("Path to file or folder to verify")]
    public required string ContentPath { get; init; }

    [CommandOption("-k|--key")]
    [Description("Path to PUBLIC key PEM file")]
    public required string PublicKeyPath { get; init; }

    [CommandOption("-s|--signature")]
    [Description("Path to .sig signature file")]
    public required string SignaturePath { get; init; }

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
