using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SoftwareSigner.Services.Interfaces;

namespace SoftwareSigner.Services.Strategies;

public class RsaSignerStrategy : ISignerStrategy
{
    public bool CanHandle(AsymmetricKeyParameter key) => key is RsaPrivateCrtKeyParameters;

    public byte[] Sign(byte[] hash, AsymmetricKeyParameter privateKey)
    {
        var signer = SignerUtilities.GetSigner("SHA512withRSA");
        signer.Init(true, privateKey);
        signer.BlockUpdate(hash, 0, hash.Length);
        return signer.GenerateSignature();
    }
}
