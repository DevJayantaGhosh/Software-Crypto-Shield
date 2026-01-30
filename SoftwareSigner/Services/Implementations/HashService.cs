using System.Security.Cryptography;
using System.Text;
using SoftwareSigner.Services.Interfaces;

namespace SoftwareSigner.Services.Implementations;

public class HashService : IHashService
{
    public async Task<byte[]> ComputeHashAsync(string path)
    {
        // Case 1: Single File
        if (File.Exists(path))
        {
            using var sha = SHA512.Create();
            using var stream = File.OpenRead(path);
            return await sha.ComputeHashAsync(stream);
        }
        // Case 2: Directory (Recursive)
        else if (Directory.Exists(path))
        {
            // Get all files, sorted deterministically to ensure consistent hash
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                 .OrderBy(p => p.ToLowerInvariant())
                                 .ToArray();

            using var sha = SHA512.Create();
            foreach (var file in files)
            {
                // Critical: Normalize separators to forward slash '/'
                string relativePath = Path.GetRelativePath(path, file).Replace("\\", "/");
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath);

                // 1. Hash the relative path (so folder structure matters)
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
