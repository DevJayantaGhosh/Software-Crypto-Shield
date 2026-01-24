using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SoftwareSigner.Services.Interfaces;

namespace SoftwareSigner.Services.Strategies;

public class EcdsaSignerStrategy : ISignerStrategy
{
    public bool CanHandle(AsymmetricKeyParameter key) => key is ECPrivateKeyParameters;

    public byte[] Sign(byte[] hash, AsymmetricKeyParameter privateKey)
    {
        var signer = SignerUtilities.GetSigner("SHA512withECDSA");
        signer.Init(true, privateKey);
        signer.BlockUpdate(hash, 0, hash.Length);
        return signer.GenerateSignature();
    }
}
