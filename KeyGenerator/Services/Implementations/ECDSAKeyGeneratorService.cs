using KeyGenerator.Models;
using KeyGenerator.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace KeyGenerator.Services.Implementations;

public sealed class ECDSAKeyGeneratorService : IKeyGeneratorService
{
    public async Task<KeyGenerationResult> GenerateAsync(
        GenerateOptions options,
        CancellationToken cancellationToken = default)
    {
        var curve = options.Curve?.ToUpperInvariant() switch
        {
            "P-256" => ECCurve.NamedCurves.nistP256,
            "P-384" => ECCurve.NamedCurves.nistP384,
            "P-521" => ECCurve.NamedCurves.nistP521,
            _ => ECCurve.NamedCurves.nistP256
        };

        using var ecdsa = ECDsa.Create(curve);

        var pub = ecdsa.ExportSubjectPublicKeyInfoPem();
        var priv = ecdsa.ExportPkcs8PrivateKeyPem();

        var dir = Path.GetFullPath(options.OutputDir);
        Directory.CreateDirectory(dir);

        var curveName = curve.Oid?.FriendlyName ?? "P-256";
        var pubPath = Path.Combine(dir, $"ecdsa-{curveName}-public.pem");
        var privPath = Path.Combine(dir, $"ecdsa-{curveName}-private.pem");

        await File.WriteAllTextAsync(pubPath, pub, Encoding.ASCII, cancellationToken);
        await File.WriteAllTextAsync(privPath, priv, Encoding.ASCII, cancellationToken);

        return new KeyGenerationResult(
            pubPath, privPath,
            AlgorithmType.ECDSA,
            0,
            curveName,
            DateTimeOffset.UtcNow,
            new FileInfo(pubPath).Length,
            new FileInfo(privPath).Length);
    }
}
