<h1 align="center">🛡️ Software Crypto Shield 🛡️</h1>

<p align="center">
  <strong>Enterprise-grade CLI toolkit for cryptographic key generation, software signing, and signature verification.</strong>
</p>

---




## 🔍 Overview

**Software Crypto Shield** is a three-tool CLI suite that provides a complete **sign → verify** workflow for software integrity protection:

| Tool | Purpose |
|---|---|
| **KeyGenerator** | Generate RSA or ECDSA key pairs (file or stdout) |
| **SoftwareSigner** | Hash & sign files/folders with a private key |
| **SoftwareVerifier** | Verify signatures using the corresponding public key |

Each tool supports **two modes**:

- **📁 File Mode** — Keys, signatures read/written as `.pem` / `.sig` files on device
- **☁️ String Mode** — Keys and signatures passed as PEM/Base64 strings via CLI (ideal for cloud, API, CI/CD pipelines)

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        Software Crypto Shield                           │
├───────────────────┬───────────────────────┬─────────────────────────────┤
│   KeyGenerator    │   SoftwareSigner      │   SoftwareVerifier          │
│                   │                       │                             │
│  ┌─────────────┐  │  ┌─────────────────┐  │  ┌───────────────────────┐  │
│  │  Commands   │  │  │   Commands      │  │  │     Commands          │  │
│  │ ┌─────────┐ │  │  │ ┌─────────────┐ │  │  │ ┌─────────────────┐   │  │
│  │ │ RSA Cmd │ │  │  │ │  Sign Cmd   │ │  │  │ │   Verify Cmd    │   │  │
│  │ │ ECDSA   │ │  │  │ └─────────────┘ │  │  │ └─────────────────┘   │  │
│  │ └─────────┘ │  │  └────────┬────────┘  │  └──────────┬────────────┘  │
│  └──────┬──────┘  │           │           │             │               │
│         │         │  ┌────────▼────────┐  │  ┌──────────▼───────────┐   │
│  ┌──────▼──────┐  │  │   Services      │  │  │     Services         │   │
│  │  Services   │  │  │ ┌─────────────┐ │  │  │ ┌─────────────────┐  │   │
│  │ ┌─────────┐ │  │  │ │ HashService │ │  │  │ │   HashService   │  │   │
│  │ │ RSA Gen │ │  │  │ │ SignService │ │  │  │ │   RSA Verifier  │  │   │
│  │ │ECDSA Gen│ │  │  │ │ RSA Signer  │ │  │  │ │  ECDSA Verifier │  │   │
│  │ └─────────┘ │  │  │ │ECDSA Signer │ │  │  │ └─────────────────┘  │   │
│  └──────┬──────┘  │  │ └─────────────┘ │  │  └──────────┬───────────┘   │
│         │         │  └────────┬────────┘  │             │               │
│  ┌──────▼──────┐  │  ┌────────▼────────┐  │  ┌──────────▼───────────┐   │
│  │   Models    │  │  │    Models       │  │  │      Models          │   │
│  │ ┌─────────┐ │  │  │ ┌─────────────┐ │  │  │ ┌─────────────────┐  │   │
│  │ │ Options │ │  │  │ │ SignOptions │ │  │  │ │  VerifyOptions  │  │   │
│  │ │ Result  │ │  │  │ └─────────────┘ │  │  │ └─────────────────┘  │   │
│  │ └─────────┘ │  │  └─────────────────┘  │  └──────────────────────┘   │
│  └─────────────┘  │                       │                             │
├───────────────────┴───────────────────────┴─────────────────────────────┤
│                     Factory Pattern + Strategy Pattern                  │
│              (Auto-detect RSA vs ECDSA from key format)                 │
└─────────────────────────────────────────────────────────────────────────┘
```





### 🔑 Key Generation

```
┌───────────────────────────────────────────────────────────────┐
│                     KEY GENERATION PROCESS                    │
│                                                               │
│  Algorithm ──► RSA (2048 / 4096)                              │
│            ──► ECDSA (P-256 / P-384 / P-521)                  │
│                         │                                     │
│                         ▼                                     │
│                   Generate Key Pair                           │
│               ┌─────────┬──────────┐                          │
│               │         │          │                          │
│               ▼         ▼          ▼                          │
│          Public Key   Private Key   (Optional)                │
│           (.pem)       (.pem)      Password ──► AES-256-CBC   │
│               │           │        Encrypt     PKCS#8 + PBE   │
│               │           │          │                        │
│               ▼           ▼          ▼                        │
│ ┌───────────────────────────────────────────────────────────┐ │
│ │                       Output Mode:                        │ │
│ │   📁 File    → pem files on disk                          │ │
│ │   ☁️ String  → --keystring stdout                         │ │
│ │   📋 JSON    → --json to stdout                           │ │
│ └───────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────┘


┌───────────────────────────────────────────────────────────────┐
│                     KEY GENERATION PROCESS                    │
│                                                               │
│  Algorithm ──► RSA (2048 / 4096)                              │
│            ──► ECDSA (P-256 / P-384 / P-521)                  │
│                         │                                     │
│                         ▼                                     │
│                   Generate Key Pair                           │
│               ┌─────────┬──────────┐                          │
│               │         │          │                          │
│               ▼         ▼          ▼                          │
│          Public Key   Private Key   (Optional)                │
│           (.pem)       (.pem)      Password ──► AES-256-CBC   │
│               │           │        Encrypt     PKCS#8 + PBE   │
│               │           │          │                        │
│               ▼           ▼          ▼                        │
│       ────────────────────────────────────────────            │
│  Output Mode:                                                 │
│    📁 File    → pem files on disk                             |
│    ☁️ String  → --keystring stdout                            │
│    📋 JSON    → --json to stdout                              │
│                                                                │
└────────────────────────────────────────────────────────────────┘
``


```

### ✍️ Signing

```
┌───────────────────────────────────────────────────────────────┐
│                       SIGNING PROCESS                         │
│                                                               │
│  Content Path ──► Recursive File Scan ──► SHA-512 Hash        │
│                                               │               │
│  Private Key ──► Decrypt (if password) ──►  SIGN              │
│  (file -k  or  --privatekeystring)            │               │
│                                        Signature Bytes        │
│                                               │               │
│         ┌──────────────────────────────────┐  │               │
│         │  Output Mode:                     │◄─┘              │
│         │  📁 File    → .sig file on disk  │                  │
│         │  ☁️  String  → Base64 to stdout  │                  │
│         │  📋 JSON    → --json to stdout   │                  │
│         └──────────────────────────────────┘                  │
└───────────────────────────────────────────────────────────────┘
```

### ✅ Verification

```
┌───────────────────────────────────────────────────────────────┐
│                     VERIFICATION PROCESS                      │
│                                                               │
│  Content Path ──► Recursive File Scan ──► SHA-512 Hash        │
│                                               │               │
│  Public Key  ────────────────────────►     VERIFY             │
│  (file -k  or  --publickeystring)             │               │
│                                               │               │
│  Signature   ────────────────────────►     (bool)             │
│  (file -s  or  --signaturestring)             │               │
│                                               │               │
│         ┌──────────────────────────────────┐  │               │
│         │  Result:                         │◄─┘               │
│         │  ✅ VALID   → integrity intact   │                  │
│         │  ❌ INVALID → content tampered   │                  │
│         └──────────────────────────────────┘                  │
└───────────────────────────────────────────────────────────────┘
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Build

```bash
dotnet build Software-Crypto-Shield.sln
```

### Publish (Standalone Executables)

```bash

# =========================
# Windows x64
# =========================
dotnet publish KeyGenerator      -c Release -r win-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64/
dotnet publish SoftwareSigner    -c Release -r win-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64/
dotnet publish SoftwareVerifier  -c Release -r win-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64/



# =========================
# Linux x64
# =========================
dotnet publish KeyGenerator      -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64/
dotnet publish SoftwareSigner    -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64/
dotnet publish SoftwareVerifier  -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64/



# =========================
# macOS (Apple Silicon: M1/M2/M3)
# =========================
dotnet publish KeyGenerator      -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64/
dotnet publish SoftwareSigner    -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64/
dotnet publish SoftwareVerifier  -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64/


# =========================
# macOS (Intel)
# =========================
dotnet publish KeyGenerator      -c Release -r osx-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64/
dotnet publish SoftwareSigner    -c Release -r osx-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64/
dotnet publish SoftwareVerifier  -c Release -r osx-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64/

```

---

## 🔧 Tools Reference

### 1. KeyGenerator

Generate RSA or ECDSA asymmetric key pairs.

#### Options

| Flag | Short | Description | Default |
|---|---|---|---|
| `--size` | `-s` | RSA key size (2048 or 4096) | `2048` |
| `--curve` | `-c` | ECDSA curve (P-256, P-384, P-521) | `P-256` |
| `--out` | `-o` | Output directory for key files | `keys/` |
| `--password` | `-p` | Encrypt private key with password | — |
| `--keystring` | — | Output keys as strings to stdout (no files) | `false` |
| `--json` | `-j` | JSON output format | `false` |
| `--verbose` | `-v` | Verbose output | `false` |
| `--silent` | — | Suppress UI output | `false` |
| `--version` | — | Show version | — |

#### Usage Examples

```bash
# RSA — File mode (default)
KeyGenerator.exe generate rsa
KeyGenerator.exe generate rsa -s 4096 -o ./mykeys -p MyPassword -v

# RSA — String mode (cloud/API)
KeyGenerator.exe generate rsa --keystring
KeyGenerator.exe generate rsa -s 4096 --keystring --json
KeyGenerator.exe generate rsa -s 4096 -p MyPassword --keystring --json

# ECDSA — File mode
KeyGenerator.exe generate ecdsa
KeyGenerator.exe generate ecdsa --curve P-384 -o ./mykeys -p MyPassword

# ECDSA — String mode (cloud/API)
KeyGenerator.exe generate ecdsa --keystring
KeyGenerator.exe generate ecdsa --curve P-384 --keystring --json
KeyGenerator.exe generate ecdsa --curve P-521 -p MyPassword --keystring --json
```

#### JSON Output (--keystring --json)

```json
{
  "algorithm": "RSA",
  "keySize": 4096,
  "createdAtUtc": "2026-03-09T14:22:28.123+00:00",
  "publicKeyBytes": 800,
  "privateKeyBytes": 3272,
  "publicKey": "-----BEGIN RSA PUBLIC KEY-----\n...\n-----END RSA PUBLIC KEY-----",
  "privateKey": "-----BEGIN ENCRYPTED PRIVATE KEY-----\n...\n-----END ENCRYPTED PRIVATE KEY-----",
  "passwordProtected": true
}
```

---

### 2. SoftwareSigner

Sign files or directories using a private key. Computes a SHA-512 hash of all content and signs it.

#### Options

| Flag | Short | Description | Default |
|---|---|---|---|
| `--content` | `-c` | Path to file or directory to sign | **(required)** |
| `--key` | `-k` | Path to private key `.pem` file | — |
| `--privatekeystring` | — | Private key PEM string (cloud/API mode) | — |
| `--output` | `-o` | Output signature file path | `signature.sig` |
| `--password` | `-p` | Password for encrypted private key | — |
| `--json` | `-j` | JSON output format | `false` |
| `--verbose` | `-v` | Verbose output | `false` |
| `--version` | — | Show version | — |

> **Note:** Use either `-k` (key file) **or** `--privatekeystring` (key string), not both.

#### Usage Examples

```bash
# File-based signing
SoftwareSigner.exe sign -c ./build -k private.pem
SoftwareSigner.exe sign -c ./build -k private.pem -p MyPassword
SoftwareSigner.exe sign -c ./build -k private.pem -o release.sig -j

# String-based signing (cloud/API — returns signature as Base64 to stdout)
SoftwareSigner.exe sign -c ./build --privatekeystring "-----BEGIN RSA PRIVATE KEY-----...-----END RSA PRIVATE KEY-----"
SoftwareSigner.exe sign -c ./build --privatekeystring "-----BEGIN ENCRYPTED PRIVATE KEY-----...-----END ENCRYPTED PRIVATE KEY-----" -p MyPassword
```

#### Output Behavior

| Mode | Signature Output |
|---|---|
| `-k <file>` | Writes `signature.sig` file to disk |
| `--privatekeystring "PEM"` | Prints Base64 signature string to stdout (no file) |
| `--privatekeystring "PEM" -j` | Prints JSON with `signatureString` field |

---

### 3. SoftwareVerifier

Verify digital signatures against file or directory content using a public key.

#### Options

| Flag | Short | Description | Default |
|---|---|---|---|
| `--content` | `-c` | Path to file or directory to verify | **(required)** |
| `--key` | `-k` | Path to public key `.pem` file | — |
| `--publickeystring` | — | Public key PEM string (cloud/API mode) | — |
| `--signature` | `-s` | Path to signature `.sig` file | — |
| `--signaturestring` | — | Base64 signature string (cloud/API mode) | — |
| `--json` | `-j` | JSON output format | `false` |
| `--verbose` | `-v` | Verbose output | `false` |
| `--version` | — | Show version | — |

> **Note:** Provide the public key via `-k` **or** `--publickeystring`. Provide the signature via `-s` **or** `--signaturestring`. You can mix file and string sources.

#### Usage Examples

```bash
# Fully file-based
SoftwareVerifier.exe verify -c ./build -k public.pem -s signature.sig

# Public key string + signature file
SoftwareVerifier.exe verify -c ./build --publickeystring "-----BEGIN PUBLIC KEY-----...-----END PUBLIC KEY-----" -s signature.sig

# Public key file + signature string
SoftwareVerifier.exe verify -c ./build -k public.pem --signaturestring "BASE64_SIGNATURE"

# Fully string-based (cloud/API — zero files)
SoftwareVerifier.exe verify -c ./build --publickeystring "-----BEGIN PUBLIC KEY-----...-----END PUBLIC KEY-----" --signaturestring "BASE64_SIGNATURE"
```

#### Input Flexibility Matrix

| Public Key Source | Signature Source | Valid? |
|---|---|---|
| `-k <file>` | `-s <file>` | ✅ Classic file mode |
| `--publickeystring "PEM"` | `--signaturestring "B64"` | ✅ Full cloud API Integration |

---

## ☁️ Cloud API CI/CD Integration Mode

The **string mode** across all three tools enables a **zero-file workflow** ideal for:


- 🏗️ **CI/CD pipelines** — keys stored as secrets, passed as environment variables


### Complete Zero-File Pipeline

```bash
# Step 1: Generate keys as strings
KeyGenerator.exe generate rsa -s 4096 --keystring --json > keys.json

# Step 2: Sign using private key string (outputs signature string)
SoftwareSigner.exe sign -c ./release --privatekeystring "$PRIVATE_KEY" --json

# Step 3: Verify using public key string + signature string
SoftwareVerifier.exe verify -c ./release --publickeystring "$PUBLIC_KEY" --signaturestring "$SIGNATURE"
```

---

## 🔄 End-to-End Examples

### Example 1: RSA File-Based Workflow

```bash
# Generate RSA 2048-bit key pair
KeyGenerator.exe generate rsa -s 2048 -o ./keys

# Sign a release folder
SoftwareSigner.exe sign -c ./my-app/bin -k ./keys/rsa-2048-private.pem -o release.sig

# Verify the signature
SoftwareVerifier.exe verify -c ./my-app/bin -k ./keys/rsa-2048-public.pem -s release.sig
# ✅ Signature is VALID

# Verify against tampered content
SoftwareVerifier.exe verify -c ./tampered-app/bin -k ./keys/rsa-2048-public.pem -s release.sig
# ❌ Signature is INVALID
```

### Example 2: ECDSA with Password Protection

```bash
# Generate ECDSA P-384 key pair with password
KeyGenerator.exe generate ecdsa --curve P-384 -p MySecretPass -o ./secure-keys

# Sign (must provide password for encrypted key)
SoftwareSigner.exe sign -c ./app -k ./secure-keys/ecdsa-nistP384-private.pem -p MySecretPass

# Verify (public key is never encrypted)
SoftwareVerifier.exe verify -c ./app -k ./secure-keys/ecdsa-nistP384-public.pem -s signature.sig
```

### Example 3: Full Cloud/API Mode (Zero Files)

```bash
# Generate keys as JSON strings
KeyGenerator.exe generate rsa -s 4096 -p SecurePass --keystring --json
# Output:
# {
#   "publicKey": "-----BEGIN RSA PUBLIC KEY-----\n...",
#   "privateKey": "-----BEGIN ENCRYPTED PRIVATE KEY-----\n...",
#   "passwordProtected": true
# }

# Sign using the private key string → get signature string back
SoftwareSigner.exe sign -c ./app --privatekeystring "-----BEGIN ENCRYPTED PRIVATE KEY-----..." -p SecurePass
# Output: BASE64_SIGNATURE_STRING

# Verify using public key string + signature string
SoftwareVerifier.exe verify -c ./app --publickeystring "-----BEGIN RSA PUBLIC KEY-----..." --signaturestring "BASE64_SIGNATURE_STRING"
# ✅ Signature is VALID
```

---

## 🏗️ Build & Publish

### Build All Projects

```bash
dotnet build Software-Crypto-Shield.sln
```

### Run Tests

```bash
dotnet test Software-Crypto-Shield.sln
```

### Publish Standalone Executables

```bash
# =========================
# Windows (x64)
# =========================
dotnet publish KeyGenerator      -c Release -r win-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64/
dotnet publish SoftwareSigner    -c Release -r win-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64/
dotnet publish SoftwareVerifier  -c Release -r win-x64   --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64/


# =========================
# Linux (x64)
# =========================
dotnet publish KeyGenerator      -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64/
dotnet publish SoftwareSigner    -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64/
dotnet publish SoftwareVerifier  -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64/


# =========================
# macOS (Apple Silicon: M1 / M2 / M3)
# =========================
dotnet publish KeyGenerator      -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64/
dotnet publish SoftwareSigner    -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64/
dotnet publish SoftwareVerifier  -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-arm64/


# =========================
# macOS (Intel x64)
# =========================
dotnet publish KeyGenerator      -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64/
dotnet publish SoftwareSigner    -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64/
dotnet publish SoftwareVerifier  -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/osx-x64/


```

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  <strong>🛡️ Software Crypto Shield</strong> — Protect your software with cryptographic integrity.
</p>
