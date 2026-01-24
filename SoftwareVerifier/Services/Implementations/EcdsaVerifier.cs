using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using SoftwareVerifier.Services.Interfaces;

namespace SoftwareVerifier.Services.Implementations;

public class EcdsaVerifier : ISignatureVerifier
{
    private const string Algorithm = "SHA512withECDSA";

    public bool VerifySignature(byte[] contentHash, byte[] signatureBytes, AsymmetricKeyParameter publicKey)
    {
        ISigner signer = SignerUtilities.GetSigner(Algorithm);
        signer.Init(false, publicKey);
        signer.BlockUpdate(contentHash, 0, contentHash.Length);

        return signer.VerifySignature(signatureBytes);
    }
}
