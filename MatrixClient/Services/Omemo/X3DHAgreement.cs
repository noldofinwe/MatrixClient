namespace MatrixClient.Services.Omemo;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;

/*
 *
 * var senderEphemeralKey = GenerateKeyPair(); // fresh key for this session
   var x3dh = new X3DHAgreement();
   
   byte[] sharedSecret = x3dh.PerformX3DH(
       senderEphemeralKey,
       recipientIdentityKey,
       recipientSignedPreKey,
       recipientOneTimePreKey // optional
   );
   
 */

public class X3DHAgreement
{
    public byte[] PerformX3DH(
        AsymmetricCipherKeyPair senderEphemeralKey,
        AsymmetricKeyParameter recipientIdentityKey,
        AsymmetricKeyParameter recipientSignedPreKey,
        AsymmetricKeyParameter recipientOneTimePreKey = null)
    {
        var dhResults = new List<byte[]>();

        // DH1: DH(EK, IK)
        dhResults.Add(PerformDH(senderEphemeralKey.Private, recipientIdentityKey));

        // DH2: DH(EK, SPK)
        dhResults.Add(PerformDH(senderEphemeralKey.Private, recipientSignedPreKey));

        // DH3: DH(SK, IK) — optional, if sender has a static key
        // Not used in OMEMO, so we skip it

        // DH4: DH(EK, OPK) — optional
        if (recipientOneTimePreKey != null)
        {
            dhResults.Add(PerformDH(senderEphemeralKey.Private, recipientOneTimePreKey));
        }

        // Concatenate all DH results
        byte[] combined = CombineDhResults(dhResults);

        // Derive shared secret using HKDF
        return DeriveSharedSecret(combined);
    }

    private byte[] PerformDH(AsymmetricKeyParameter privateKey, AsymmetricKeyParameter publicKey)
    {
        var agreement = new ECDHBasicAgreement();
        agreement.Init(privateKey);
        var sharedSecret = agreement.CalculateAgreement(publicKey);
        return sharedSecret.ToByteArrayUnsigned();
    }

    private byte[] CombineDhResults(List<byte[]> dhResults)
    {
        int totalLength = 0;
        foreach (var result in dhResults)
            totalLength += result.Length;

        byte[] combined = new byte[totalLength];
        int offset = 0;
        foreach (var result in dhResults)
        {
            Buffer.BlockCopy(result, 0, combined, offset, result.Length);
            offset += result.Length;
        }

        return combined;
    }

    private byte[] DeriveSharedSecret(byte[] inputKeyMaterial)
    {
        // Simple HKDF using SHA256
        var hkdf = new Org.BouncyCastle.Crypto.Generators.HkdfBytesGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
        hkdf.Init(new Org.BouncyCastle.Crypto.Parameters.HkdfParameters(inputKeyMaterial, null, null));
        byte[] output = new byte[32]; // 256-bit key
        hkdf.GenerateBytes(output, 0, output.Length);
        return output;
    }
}
