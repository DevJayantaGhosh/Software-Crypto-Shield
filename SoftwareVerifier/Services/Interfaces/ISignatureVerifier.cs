using Org.BouncyCastle.Crypto;

namespace SoftwareVerifier.Services.Interfaces;

public interface ISignatureVerifier
{
    bool VerifySignature(byte[] contentHash, byte[] signatureBytes, AsymmetricKeyParameter publicKey);
}
