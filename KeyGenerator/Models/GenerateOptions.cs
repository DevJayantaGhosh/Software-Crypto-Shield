using Spectre.Console.Cli;
using System.ComponentModel;

namespace KeyGenerator.Models;

public sealed class GenerateOptions : CommandSettings
{
    [CommandOption("-a|--algorithm")]
    [DefaultValue(AlgorithmType.RSA)]
    public AlgorithmType Algorithm { get; init; }

    [CommandOption("-s|--size")]
    [DefaultValue(2048)]
    public int Size { get; init; }

    [CommandOption("-c|--curve")]
    public string? Curve { get; init; }

    [CommandOption("-o|--out")]
    [DefaultValue("keys")]
    public string? OutputDir { get; init; }

    [CommandOption("-j|--json")]
    public bool JsonOnly { get; init; }

    [CommandOption("--silent")]
    public bool Silent { get; init; }

    [CommandOption("-v|--verbose")]
    public bool Verbose { get; init; }
}
