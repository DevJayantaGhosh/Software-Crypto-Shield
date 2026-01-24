using System.Security.Cryptography;
using System.Text;
using SoftwareVerifier.Services.Interfaces;

namespace SoftwareVerifier.Services.Implementations;

public class HashService : IHashService
{
    public async Task<byte[]> ComputeHashAsync(string path)
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
                // Critical: Normalize separators to forward slash '/'
                // This ensures the hash is identical regardless of OS (Windows/Linux)
                string relativePath = Path.GetRelativePath(path, file).Replace("\\", "/");
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath);

                // 1. Hash the relative path first
                sha.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // 2. Hash the file content
                byte[] content = await File.ReadAllBytesAsync(file);
                sha.TransformBlock(content, 0, content.Length, content, 0);
            }

            // Finalize the hash computation
            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return sha.Hash!;
        }

        throw new FileNotFoundException($"Content path not found: {path}");
    }
}
