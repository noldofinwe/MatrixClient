﻿using System;
using System.Security.Cryptography;
using XmppApi.Crypto;
using XmppApi.Network.XML.Messages.Features.SASL.SCRAM.SHA1;
using XmppApi.Network.XML.Messages.Processor;

namespace XmppApi.Network.XML.Messages.Features.SASL.SCRAM.SHA256
{
    // https://tools.ietf.org/html/rfc7677
    public class ScramSHA256SASLMechanism: ScramSHA1SASLMechanism
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public ScramSHA256SASLMechanism(string id, string password, SASLConnection saslConnection) : base(id, password, saslConnection)
        {
        }

        public ScramSHA256SASLMechanism(string id, string password, string clientNonceBase64, SASLConnection saslConnection) : base(id, password, clientNonceBase64, saslConnection)
        {
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public override SelectSASLMechanismMessage getSelectSASLMechanismMessage()
        {
            clientFirstMsg = "n=" + ID + ",r=" + CLIENT_NONCE_BASE_64;
            string encClientFirstMsg = encodeStringBase64("n,," + clientFirstMsg);

            return new SelectSASLMechanismMessage("SCRAM-SHA-256", encClientFirstMsg);
        }

        #endregion

        #region --Misc Methods (Private)--

        #endregion

        #region --Misc Methods (Protected)--
        protected override string computeAnswer(int iterations)
        {
            string clientFinalMessageBare = "c=biws,r=" + serverNonce;
            byte[] saltBytes = Convert.FromBase64String(saltBase64);
            byte[] saltedPassword = CryptoUtils.pbkdf2Sha(PASSWORD_NORMALIZED, saltBytes, iterations, HashAlgorithmName.SHA256, 32);

            byte[] clientKey = CryptoUtils.hmacSha256("Client Key", saltedPassword);
            byte[] storedKey = CryptoUtils.Hash(clientKey, "SHA256");
            string authMessage = clientFirstMsg + ',' + serverFirstMsg + ',' + clientFinalMessageBare;

            byte[] clientSignature = CryptoUtils.hmacSha256(authMessage, storedKey);
            byte[] clientProof = CryptoUtils.xor(clientKey, clientSignature);
            string clientFinalMessage = clientFinalMessageBare + ",p=" + Convert.ToBase64String(clientProof);

            return encodeStringBase64(clientFinalMessage);
        }

        protected override bool isValidIterationsCount(int iters)
        {
            return iters >= 4096;
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
