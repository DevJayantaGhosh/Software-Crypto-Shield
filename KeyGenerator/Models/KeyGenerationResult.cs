namespace KeyGenerator.Models;

public sealed record KeyGenerationResult(
    string PublicKeyPath,
    string PrivateKeyPath,
    AlgorithmType Algorithm,
    int KeySize,
    string? Curve,
    DateTimeOffset CreatedAtUtc,
    long PublicKeyBytes,
    long PrivateKeyBytes,
    string? PublicKeyString = null,
    string? PrivateKeyString = null
);