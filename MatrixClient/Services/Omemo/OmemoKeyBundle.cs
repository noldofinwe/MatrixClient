using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;


namespace MatrixClient.Services.Omemo;


public class OmemoKeyBundle
{
    public AsymmetricCipherKeyPair IdentityKey { get; private set; }
    public AsymmetricCipherKeyPair SignedPreKey { get; private set; }
    public byte[] SignedPreKeySignature { get; private set; }
    public List<AsymmetricCipherKeyPair> OneTimePreKeys { get; private set; }

    public OmemoKeyBundle(int oneTimePreKeyCount = 100)
    {
        GenerateIdentityKey();
        GenerateSignedPreKey();
        GenerateOneTimePreKeys(oneTimePreKeyCount);
    }

    private void GenerateIdentityKey()
    {
        IdentityKey = GenerateKeyPair();
    }

    private void GenerateSignedPreKey()
    {
        SignedPreKey = GenerateKeyPair();
        SignedPreKeySignature = SignKey(SignedPreKey.Public, IdentityKey.Private);
    }

    private void GenerateOneTimePreKeys(int count)
    {
        OneTimePreKeys = new List<AsymmetricCipherKeyPair>();
        for (int i = 0; i < count; i++)
        {
            OneTimePreKeys.Add(GenerateKeyPair());
        }
    }

    private AsymmetricCipherKeyPair GenerateKeyPair()
    {
        var generator = GeneratorUtilities.GetKeyPairGenerator("ECDH");
        generator.Init(new KeyGenerationParameters(new SecureRandom(), 256));
        return generator.GenerateKeyPair();
    }

    private byte[] SignKey(AsymmetricKeyParameter keyToSign, AsymmetricKeyParameter signingKey)
    {
        var signer = SignerUtilities.GetSigner("SHA256withECDSA");
        signer.Init(true, signingKey);
        byte[] keyBytes = ((ECPublicKeyParameters)keyToSign).Q.GetEncoded();
        signer.BlockUpdate(keyBytes, 0, keyBytes.Length);
        return signer.GenerateSignature();
    }

    public string SerializeBundle()
    {
        // You can serialize to XML, JSON, or XMPP stanza format
        // Here's a placeholder for JSON-style output
        return $"{{ \"identityKey\": \"{Convert.ToBase64String(GetPublicKeyBytes(IdentityKey))}\", " +
               $"\"signedPreKey\": \"{Convert.ToBase64String(GetPublicKeyBytes(SignedPreKey))}\", " +
               $"\"signature\": \"{Convert.ToBase64String(SignedPreKeySignature)}\", " +
               $"\"oneTimePreKeys\": [{string.Join(",", OneTimePreKeys.ConvertAll(k => $"\"{Convert.ToBase64String(GetPublicKeyBytes(k))}\""))}] }}";
    }

    public byte[] GetPublicKeyBytes(AsymmetricCipherKeyPair keyPair)
    {
        return ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded();
    }
    public static OmemoKeyBundle FromXml(XElement xml)
    {
        var bundle = new OmemoKeyBundle();

        var identityKeyBase64 = xml.Element("identityKey")?.Value;
        var signedPreKeyElement = xml.Element("signedPreKeyPublic");
        var signatureBase64 = xml.Element("signedPreKeySignature")?.Value;

        bundle.IdentityKey = ParsePublicKey(identityKeyBase64);
        bundle.SignedPreKey = ParsePublicKey(signedPreKeyElement?.Value);
        bundle.SignedPreKeySignature = Convert.FromBase64String(signatureBase64);

        var prekeys = xml.Element("prekeys")?.Elements("preKeyPublic");
        bundle.OneTimePreKeys = new List<AsymmetricCipherKeyPair>();
        foreach (var pk in prekeys)
        {
            var key = ParsePublicKey(pk.Value);
            bundle.OneTimePreKeys.Add(key); // No private key
        }

        return bundle;
    }

    public static AsymmetricCipherKeyPair ParsePublicKey(string base64)
    {
        byte[] keyBytes = Convert.FromBase64String(base64);

        // X25519 public key (32 bytes)
        var publicKey = new X25519PublicKeyParameters(keyBytes, 0);

        // No private key available from contact's bundle
        return new AsymmetricCipherKeyPair(publicKey, null);
    }

    
    public static AsymmetricCipherKeyPair GenerateKeyPairFull()
    {
        var gen = new ECKeyPairGenerator();
        var curve = ECNamedCurveTable.GetByName("curve25519");
        var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

        var keyGenParams = new ECKeyGenerationParameters(domainParams, new SecureRandom());
        gen.Init(keyGenParams);

        return gen.GenerateKeyPair();
    }

    public static AsymmetricCipherKeyPair GenerateEphemeralKey()
    {
        var generator = new X25519KeyPairGenerator();
        generator.Init(new X25519KeyGenerationParameters(new SecureRandom()));
        return generator.GenerateKeyPair();
    }


}
