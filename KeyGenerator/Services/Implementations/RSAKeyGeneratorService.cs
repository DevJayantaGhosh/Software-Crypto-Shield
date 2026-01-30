using KeyGenerator.Models;
using KeyGenerator.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace KeyGenerator.Services.Implementations;

public sealed class RSAKeyGeneratorService : IKeyGeneratorService
{
    public async Task<KeyGenerationResult> GenerateAsync(
        GenerateOptions options,
        CancellationToken cancellationToken = default)
    {
        if (options.Size is not (2048 or 4096))
            throw new ArgumentException("RSA key size must be 2048 or 4096 bits");

        using var rsa = RSA.Create(options.Size);

        // 1. Export Public Key
        var pub = rsa.ExportRSAPublicKeyPem();

        // 2. Export Private Key
        string priv;
        if (!string.IsNullOrWhiteSpace(options.Password))
        {
            var pbeParams = new PbeParameters(
                PbeEncryptionAlgorithm.Aes256Cbc,
                HashAlgorithmName.SHA256,
                iterationCount: 100_000);

            priv = rsa.ExportEncryptedPkcs8PrivateKeyPem(
                options.Password.ToCharArray(),
                pbeParams);
        }
        else
        {
            priv = rsa.ExportRSAPrivateKeyPem();
        }

        // 3. Save Files
        var dir = Path.GetFullPath(options.OutputDir ?? "keys");
        Directory.CreateDirectory(dir);

        var pubPath = Path.Combine(dir, $"rsa-{options.Size}-public.pem");
        var privPath = Path.Combine(dir, $"rsa-{options.Size}-private.pem");

        await File.WriteAllTextAsync(pubPath, pub, Encoding.ASCII, cancellationToken);
        await File.WriteAllTextAsync(privPath, priv, Encoding.ASCII, cancellationToken);

        return new KeyGenerationResult(
            pubPath, privPath,
            AlgorithmType.RSA,
            options.Size,
            null,
            DateTimeOffset.UtcNow,
            new FileInfo(pubPath).Length,
            new FileInfo(privPath).Length);
    }
}
