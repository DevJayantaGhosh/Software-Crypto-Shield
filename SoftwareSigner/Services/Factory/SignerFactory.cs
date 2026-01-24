using Org.BouncyCastle.Crypto;
using SoftwareSigner.Services.Interfaces;
using SoftwareSigner.Services.Strategies;

namespace SoftwareSigner.Services.Factory;

public static class SignerFactory
{
    private static readonly List<ISignerStrategy> _strategies = new()
    {
        new RsaSignerStrategy(),
        new EcdsaSignerStrategy()
    };

    public static ISignerStrategy GetStrategy(AsymmetricKeyParameter key)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(key));
        return strategy ?? throw new NotSupportedException("Unsupported key type. Only RSA and ECDSA keys are supported.");
    }
}
