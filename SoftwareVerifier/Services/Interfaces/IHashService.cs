namespace SoftwareVerifier.Services.Interfaces;

public interface IHashService
{
    Task<byte[]> ComputeHashAsync(string path);
}
