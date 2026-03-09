using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SoftwareSigner.Services.Factory;
using SoftwareSigner.Services.Interfaces;

namespace SoftwareSigner.Services.Implementations;

public class SignatureService : ISignatureService
{
    private readonly IHashService _hashService;
    public SignatureService()
    {
        _hashService = new HashService();
    }

    public async Task<string> SignAsync(string contentPath, string keyPath, string outPath, string? password = null)
    {
        // 1. Compute Hash
        byte[] hash = await _hashService.ComputeHashAsync(contentPath);

        // 2. Load Key
        var privateKey = LoadPrivateKey(keyPath, password);

        // 3. Get Strategy & Sign
        var strategy = SignerFactory.GetStrategy(privateKey);
        byte[] signature = strategy.Sign(hash, privateKey);

        // 4. Save Signature
        await File.WriteAllBytesAsync(outPath, signature);

        return Convert.ToBase64String(signature);
    }

    public async Task<string> SignWithKeyStringAsync(string contentPath, string privateKeyPem, string? password = null)
    {
        // 1. Compute Hash
        byte[] hash = await _hashService.ComputeHashAsync(contentPath);

        // 2. Load Key from PEM string
        var privateKey = LoadPrivateKeyFromString(privateKeyPem, password);

        // 3. Get Strategy & Sign
        var strategy = SignerFactory.GetStrategy(privateKey);
        byte[] signature = strategy.Sign(hash, privateKey);

        // 4. Return Base64 signature string (no file write in key-string mode)
        return Convert.ToBase64String(signature);
    }

    /// <summary>
    /// Normalizes a PEM string that may have been copied from JSON output or other encoded sources.
    /// Handles: JSON unicode escapes (\uXXXX), literal \n/\r/\t, carriage returns, extra whitespace.
    /// </summary>
    private static string NormalizePem(string pem)
    {
        if (string.IsNullOrWhiteSpace(pem))
            return pem;

        // 1. Decode all JSON unicode escape sequences (\uXXXX → actual character)
        pem = System.Text.RegularExpressions.Regex.Replace(pem, @"\\u([0-9A-Fa-f]{4})", match =>
        {
            int codePoint = Convert.ToInt32(match.Groups[1].Value, 16);
            return ((char)codePoint).ToString();
        });

        // 2. Replace literal two-char escape sequences with actual characters
        //    (These appear when PEM is copied from JSON string values)
        if (!pem.Contains('\n') || pem.TrimEnd().EndsWith("-----"))
        {
            pem = pem.Replace("\\n", "\n");
        }
        pem = pem.Replace("\\r", "\r");
        pem = pem.Replace("\\t", "\t");

        // 3. Normalize line endings: \r\n → \n, standalone \r → \n
        pem = pem.Replace("\r\n", "\n").Replace("\r", "\n");

        // 4. Remove any surrounding quotes (single or double) that may have been included
        pem = pem.Trim();
        if ((pem.StartsWith("\"") && pem.EndsWith("\"")) || (pem.StartsWith("'") && pem.EndsWith("'")))
        {
            pem = pem[1..^1].Trim();
        }

        return pem;
    }

    private AsymmetricKeyParameter LoadPrivateKeyFromString(string pemContent, string? password)
    {
        if (string.IsNullOrWhiteSpace(pemContent))
            throw new ArgumentException("Private key string is empty or null.");

        // Normalize PEM from JSON output (literal \n → real newlines, \u002B → +)
        pemContent = NormalizePem(pemContent);

        using var reader = new StringReader(pemContent);

        IPasswordFinder? passwordFinder = string.IsNullOrEmpty(password)
            ? null
            : new StaticPasswordFinder(password);

        var pemReader = new PemReader(reader, passwordFinder);
        object? keyObject;

        try
        {
            keyObject = pemReader.ReadObject();
        }
        catch (Exception ex)
        {
            bool isWrongPassword =
                ex is InvalidCipherTextException ||
                ex.InnerException is InvalidCipherTextException ||
                ex.Message.Contains("pad block corrupted") ||
                ex.Message.Contains("bad decrypt");

            if (isWrongPassword)
                throw new InvalidOperationException("Failed to decrypt the private key. The password provided is incorrect.");

            if (ex.Message.Contains("problem creating ENCRYPTED private key") || ex is PasswordException)
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("The private key is ENCRYPTED, but no password was provided.\nHint: Use the '-p' or '--password' option.");

                throw new InvalidOperationException("Failed to decrypt the private key. The password provided might be incorrect.");
            }

            if (ex is PemException)
                throw new ArgumentException($"Invalid PEM format: {ex.Message}");

            throw new InvalidOperationException($"An unexpected error occurred while reading the key string: {ex.Message}");
        }

        if (keyObject == null)
            throw new ArgumentException("Could not read a valid key object from the provided key string. Is it a valid PEM?");

        if (keyObject is AsymmetricCipherKeyPair pair) return pair.Private;
        if (keyObject is AsymmetricKeyParameter key)
        {
            if (!key.IsPrivate)
                throw new ArgumentException("The provided key string contains a PUBLIC key, but a PRIVATE key is required for signing.");
            return key;
        }

        throw new ArgumentException($"Unknown key format in key string: {keyObject.GetType().Name}. Expected PrivateKey or KeyPair.");
    }

    private AsymmetricKeyParameter LoadPrivateKey(string path, string? password)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Private Key file not found at: {path}");

        using var reader = File.OpenText(path);

        IPasswordFinder? passwordFinder = string.IsNullOrEmpty(password)
            ? null
            : new StaticPasswordFinder(password);

        var pemReader = new PemReader(reader, passwordFinder);
        object? keyObject;

        try
        {
            keyObject = pemReader.ReadObject();
        }
        catch (Exception ex)
        {
            // 1. Check for Wrong Password ("pad block corrupted" or "bad decrypt")
            // Can be in Message or InnerException depending on BC version
            bool isWrongPassword =
                ex is InvalidCipherTextException ||
                ex.InnerException is InvalidCipherTextException ||
                ex.Message.Contains("pad block corrupted") ||
                ex.Message.Contains("bad decrypt");

            if (isWrongPassword)
            {
                throw new InvalidOperationException("Failed to decrypt the private key. The password provided is incorrect.");
            }

            // 2. Check for Missing Password
            // "problem creating ENCRYPTED private key" usually means we tried to read encrypted key with null finder
            if (ex.Message.Contains("problem creating ENCRYPTED private key") || ex is PasswordException)
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("The private key is ENCRYPTED, but no password was provided.\nHint: Use the '-p' or '--password' option.");
                }

                // If password WAS provided but we got here, it's likely a generic failure or unsupported algo
                throw new InvalidOperationException("Failed to decrypt the private key. The password provided might be incorrect.");
            }

            // 3. Check for PEM format errors
            if (ex is PemException)
            {
                throw new ArgumentException($"Invalid PEM file format: {ex.Message}");
            }

            // 4. Unknown error
            throw new InvalidOperationException($"An unexpected error occurred while reading the key: {ex.Message}");
        }

        if (keyObject == null)
            throw new ArgumentException("Could not read a valid key object from the file. Is it an empty or corrupted PEM file?");

        if (keyObject is AsymmetricCipherKeyPair pair) return pair.Private;
        if (keyObject is AsymmetricKeyParameter key)
        {
            if (!key.IsPrivate)
                throw new ArgumentException("The provided file contains a PUBLIC key, but a PRIVATE key is required for signing.");
            return key;
        }

        throw new ArgumentException($"Unknown key format found in file: {keyObject.GetType().Name}. Expected PrivateKey or KeyPair.");
    }


    private class StaticPasswordFinder : IPasswordFinder
    {
        private readonly char[] _password;
        public StaticPasswordFinder(string password) => _password = password.ToCharArray();
        public char[] GetPassword() => _password;
    }
}
