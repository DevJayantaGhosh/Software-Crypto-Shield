namespace SoftwareSigner.Services.Interfaces;

public interface ISignatureService
{
    Task<string> SignAsync(string contentPath, string keyPath, string outPath, string? password = null);
    Task<string> SignWithKeyStringAsync(string contentPath, string privateKeyPem, string? password = null);
}
