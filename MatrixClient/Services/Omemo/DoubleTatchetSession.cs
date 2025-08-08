using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;

namespace MatrixClient.Services.Omemo;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;

/*
 *
 * var session = new DoubleRatchetSession(sharedSecret, recipientDhPublicKey);
   
   // Use session.SendChainKey to encrypt messages
   // Use session.ReceiveChainKey to decrypt incoming messages
   
 */

public class DoubleRatchetSession
{
    public byte[] RootKey { get; private set; }
    public byte[] SendChainKey { get; private set; }
    public byte[] ReceiveChainKey { get; private set; }

    public int RemoteDeviceId { get; set; }
    public AsymmetricCipherKeyPair DHKeyPair { get; private set; }
    public AsymmetricKeyParameter RemoteDHKey { get; private set; }

    public DoubleRatchetSession(byte[] sharedSecret, int remoteDeviceId, AsymmetricKeyParameter remoteDhKey = null)
    {
        RootKey = sharedSecret;
        DHKeyPair = GenerateDHKeyPair();
        RemoteDHKey = remoteDhKey;
        RemoteDeviceId = remoteDeviceId;
        if (RemoteDHKey != null)
        {
            AdvanceDHRatchet();
        }
        else
        {
            // Initial session, no remote DH key yet
            SendChainKey = HKDF(RootKey, "RatchetSend");
            ReceiveChainKey = HKDF(RootKey, "RatchetReceive");
        }
    }

    private AsymmetricCipherKeyPair GenerateDHKeyPair()
    {
        var generator = GeneratorUtilities.GetKeyPairGenerator("ECDH");
        generator.Init(new KeyGenerationParameters(new SecureRandom(), 256));
        return generator.GenerateKeyPair();
    }

    private void AdvanceDHRatchet()
    {
        // Perform DH between our private key and remote public key
        var agreement = new ECDHBasicAgreement();
        agreement.Init(DHKeyPair.Private);
        var dhResult = agreement.CalculateAgreement(RemoteDHKey).ToByteArrayUnsigned();

        // Mix into root key and derive new chain keys
        byte[] newRootKey = HKDF(RootKey, dhResult, "RatchetRoot");
        RootKey = newRootKey;
        SendChainKey = HKDF(RootKey, "RatchetSend");
        ReceiveChainKey = HKDF(RootKey, "RatchetReceive");
    }

    private byte[] HKDF(byte[] inputKeyMaterial, string info)
    {
        return HKDF(inputKeyMaterial, null, info);
    }

    private byte[] HKDF(byte[] inputKeyMaterial, byte[] salt, string info)
    {
        var hkdf = new Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
        hkdf.Init(new Org.BouncyCastle.Crypto.Parameters.HkdfParameters(inputKeyMaterial, salt, System.Text.Encoding.UTF8.GetBytes(info)));
        byte[] output = new byte[32]; // 256-bit key
        hkdf.GenerateBytes(output, 0, output.Length);
        return output;
    }
    
    public string Encrypt(string plaintext)
    {
        // Advance send chain key
        SendChainKey = HKDF(SendChainKey, "MessageKey");

        byte[] key = SendChainKey;
        byte[] iv = new byte[12]; // 96-bit IV for AES-GCM
        new SecureRandom().NextBytes(iv);

        var cipher = new GcmBlockCipher(new AesEngine());
        cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, iv));

        byte[] input = System.Text.Encoding.UTF8.GetBytes(plaintext);
        byte[] output = new byte[cipher.GetOutputSize(input.Length)];
        int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
        cipher.DoFinal(output, len);

        // Combine IV and ciphertext
        byte[] encrypted = new byte[iv.Length + output.Length];
        Buffer.BlockCopy(iv, 0, encrypted, 0, iv.Length);
        Buffer.BlockCopy(output, 0, encrypted, iv.Length, output.Length);

        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string ciphertextBase64)
    {
        byte[] encrypted = Convert.FromBase64String(ciphertextBase64);
        byte[] iv = new byte[12];
        byte[] ciphertext = new byte[encrypted.Length - 12];

        Buffer.BlockCopy(encrypted, 0, iv, 0, 12);
        Buffer.BlockCopy(encrypted, 12, ciphertext, 0, ciphertext.Length);

        // Advance receive chain key
        ReceiveChainKey = HKDF(ReceiveChainKey, "MessageKey");

        byte[] key = ReceiveChainKey;

        var cipher = new GcmBlockCipher(new AesEngine());
        cipher.Init(false, new AeadParameters(new KeyParameter(key), 128, iv));

        byte[] output = new byte[cipher.GetOutputSize(ciphertext.Length)];
        int len = cipher.ProcessBytes(ciphertext, 0, ciphertext.Length, output, 0);
        cipher.DoFinal(output, len);

        return System.Text.Encoding.UTF8.GetString(output);
    }

}
