using KeyGenerator.Models;

namespace KeyGenerator.Services.Interfaces;

public interface IKeyGeneratorService
{
    Task<KeyGenerationResult> GenerateAsync(
        GenerateOptions options,
        CancellationToken cancellationToken = default);
}
