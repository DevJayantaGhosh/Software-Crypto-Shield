using Org.BouncyCastle.Crypto;

namespace SoftwareSigner.Services.Interfaces;

public interface ISignerStrategy
{
    bool CanHandle(AsymmetricKeyParameter key);
    byte[] Sign(byte[] hash, AsymmetricKeyParameter privateKey);
}
