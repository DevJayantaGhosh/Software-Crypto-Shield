using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using SoftwareVerifier.Services.Interfaces;
using SoftwareVerifier.Services.Implementations;

namespace SoftwareVerifier.Services.Factories;

public static class VerifierFactory
{
    public static ISignatureVerifier GetVerifier(AsymmetricKeyParameter publicKey)
    {
        return publicKey switch
        {
            RsaKeyParameters => new RsaVerifier(),
            ECPublicKeyParameters => new EcdsaVerifier(),
            _ => throw new NotSupportedException($"Unsupported key type: {publicKey.GetType().Name}")
        };
    }
}
