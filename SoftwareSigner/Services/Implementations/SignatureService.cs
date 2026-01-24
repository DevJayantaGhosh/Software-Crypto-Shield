using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using SoftwareSigner.Services.Factory;
using SoftwareSigner.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SoftwareSigner.Services.Implementations;

public class SignatureService : ISignatureService
{
    public async Task<string> SignAsync(string contentPath, string keyPath, string outPath)
    {
        // 1. Compute Hash
        byte[] hash = await ComputeHashAsync(contentPath);

        // 2. Load Key
        var privateKey = LoadPrivateKey(keyPath);

        // 3. Get Strategy & Sign
        var strategy = SignerFactory.GetStrategy(privateKey);
        byte[] signature = strategy.Sign(hash, privateKey);

        // 4. Save Signature
        await File.WriteAllBytesAsync(outPath, signature);

        return Convert.ToBase64String(signature);
    }

    private AsymmetricKeyParameter LoadPrivateKey(string path)
    {
        using var reader = File.OpenText(path);
        var pemReader = new PemReader(reader);
        var keyObject = pemReader.ReadObject();

        if (keyObject is AsymmetricCipherKeyPair pair) return pair.Private;
        if (keyObject is AsymmetricKeyParameter key) return key;

        throw new ArgumentException("Invalid PEM file: Could not read private key.");
    }

    private async Task<byte[]> ComputeHashAsync(string path)
    {
        if (File.Exists(path))
        {
            using var sha = SHA512.Create();
            using var stream = File.OpenRead(path);
            return await sha.ComputeHashAsync(stream);
        }
        else if (Directory.Exists(path))
        {
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                 .OrderBy(p => p.ToLowerInvariant())
                                 .ToArray();

            using var sha = SHA512.Create();
            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(path, file).Replace("\\", "/");
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath);
                sha.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                byte[] content = await File.ReadAllBytesAsync(file);
                sha.TransformBlock(content, 0, content.Length, content, 0);
            }
            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return sha.Hash!;
        }
        throw new FileNotFoundException($"Content path not found: {path}");
    }
}
