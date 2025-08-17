using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Omemo.Keys;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using XmppApi.Network;

namespace XmppApi.Crypto
{
    public static class CryptoUtils
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Performs <paramref name="a"/> XOR <paramref name="b"/> and returns the result.
        /// </summary>
        public static byte[] xor(byte[] a, byte[] b)
        {
            byte[] output = new byte[b.Length];
            for (int i = 0; i < b.Length; i++)
            {
                output[i] = (byte)(b[i] ^ a[i % a.Length]);
            }
            return output;
        }

        public static string byteArrayToHexString(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Generates the display fingerprint for the given OMEMO identity public key.
        /// </summary>
        /// <param name="keyRaw">The OMEMO identity public key as a byte[].</param>
        /// <returns>A hex string representing the given OMEMO identity public key. Split up in blocks of 8 characters separated by a whitespace.</returns>
        public static string generateOmemoFingerprint(byte[] keyRaw)
        {
            string fingerprint = byteArrayToHexString(keyRaw);
            return Regex.Replace(fingerprint, ".{8}", "$0 ").TrimEnd();
        }

        /// <summary>
        /// Generates the OMEMO fingerprint for displaying as a QR code.
        /// </summary>
        /// <param name="identityKey">The identity public key.</param>
        /// <param name="account">The XMPP account the fingerprint belongs to.</param>
        /// <returns>A string representation of the fingerprint concatenated with JID and device id.</returns>
        public static string generateOmemoQrCodeFingerprint(ECPubKeyModel identityKey, XMPPAccount account)
        {
            return generateOmemoQrCodeFingerprint(identityKey, account.getBareJid(), account.omemoDeviceId);
        }

        /// <summary>
        /// Generates the OMEMO fingerprint for displaying as a QR code.
        /// </summary>
        /// <param name="identityKey">The identity public key.</param>
        /// <param name="bareJid">The bare JID the fingerprint belongs to.</param>
        /// <param name="deviceId">The OMEMO device id the fingerprint belongs to.</param>
        /// <returns>A string representation of the fingerprint concatenated with JID and device id.</returns>
        public static string generateOmemoQrCodeFingerprint(ECPubKeyModel identityKey, string bareJid, uint deviceId)
        {
            StringBuilder sb = new StringBuilder("xmpp:");
            sb.Append(bareJid);
            sb.Append("?omemo-sid-");
            sb.Append(deviceId);
            sb.Append('=');
            sb.Append(byteArrayToHexString(identityKey.key));
            return sb.ToString();
        }

        public static byte[] hmacSha1(byte[] data, string key)
        {
            return hmacSha1(data, Encoding.UTF8.GetBytes(key));
        }

        public static byte[] hmacSha1(string data, string key)
        {
            return hmacSha1(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] hmacSha1(string data, byte[] key)
        {
            return hmacSha1(Encoding.UTF8.GetBytes(data), key);
        }

        public static byte[] hmacSha1(byte[] data, byte[] key)
        {
            return hmacSha(data, new HMACSHA1(key));
        }

        public static byte[] hmacSha256(byte[] data, string key)
        {
            return hmacSha256(data, Encoding.UTF8.GetBytes(key));
        }

        public static byte[] hmacSha256(string data, string key)
        {
            return hmacSha256(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] hmacSha256(string data, byte[] key)
        {
            return hmacSha256(Encoding.UTF8.GetBytes(data), key);
        }

        public static byte[] hmacSha256(byte[] data, byte[] key)
        {
            return hmacSha(data, new HMACSHA256(key));
        }

        public static byte[] hmacSha512(byte[] data, string key)
        {
            return hmacSha512(data, Encoding.UTF8.GetBytes(key));
        }

        public static byte[] hmacSha512(string data, string key)
        {
            return hmacSha512(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key));
        }

        public static byte[] hmacSha512(string data, byte[] key)
        {
            return hmacSha512(Encoding.UTF8.GetBytes(data), key);
        }

        public static byte[] hmacSha512(byte[] data, byte[] key)
        {
            return hmacSha(data, new HMACSHA512(key));
        }

        #endregion

        #region --Misc Methods (Private)--
        /// <summary>
        /// Hashes the given <paramref name="data"/> with the given <paramref name="algName"/> and returns it.
        /// <para/>
        /// Source: https://docs.microsoft.com/en-us/uwp/api/windows.security.cryptography.core.hashalgorithmprovider
        /// </summary>
        /// <param name="data">The data that should get hashed.</param>
        /// <param name="algName">The <see cref="HashAlgorithmNames"/> name that should get used for hashing.</param>
        /// <returns></returns>
        public static byte[] Hash(byte[] data, string algName)
        {
            IDigest digest = algName switch
            {
                "SHA256" => new Sha256Digest(),
                "SHA1" => new Sha1Digest(),
                "MD5" => new MD5Digest(),
                _ => throw new ArgumentException($"Unsupported algorithm: {algName}")
            };

            byte[] result = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(result, 0);

            // Optional: verify hash length matches expected
            if (result.Length != digest.GetDigestSize())
            {
                throw new InvalidOperationException("There was an error creating the hash");
            }

            return result;
        }

        private static byte[] hmacSha(byte[] data, HMAC hmac)
        {
            hmac.Initialize();
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// RFC 2898 with SHA.
        /// </summary>
        /// <returns></returns>
        public static byte[] pbkdf2Sha(string normalizedPassword, byte[] salt, int iterations, HashAlgorithmName hash, int dkLen)
        {
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(normalizedPassword, salt, iterations, hash);
            return deriveBytes.GetBytes(dkLen);
        }

        public static string GenerateNonceBase64(int length)
        {
            byte[] nonce = new byte[length];
            RandomNumberGenerator.Fill(nonce);
            return Convert.ToBase64String(nonce);
        }
        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
