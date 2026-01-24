using KeyGenerator.Models;
using KeyGenerator.Services.Implementations;
using KeyGenerator.Services.Interfaces;

namespace KeyGenerator.Services.Factory;

public static class KeyGeneratorFactory
{
    public static IKeyGeneratorService Create(GenerateOptions options) =>
        options.Algorithm switch
        {
            AlgorithmType.RSA => new RSAKeyGeneratorService(),
            AlgorithmType.ECDSA => new ECDSAKeyGeneratorService(),
            _ => throw new InvalidOperationException("Unsupported algorithm")
        };
}
