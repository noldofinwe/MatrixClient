using System.Collections.Generic;
using System.ComponentModel;
using XmppApi.Network.XML.Messages.XEP_0384;
using XmppApi.Network.XML.Messages.XEP_0384.Session;

namespace XmppApi.Events
{
    public class OmemoSessionBuildErrorEventArgs: CancelEventArgs
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly string CHAT_JID;
        public readonly OmemoSessionBuildError ERROR;
        public readonly IList<OmemoEncryptedMessage> MESSAGES;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public OmemoSessionBuildErrorEventArgs(string chatJid, OmemoSessionBuildError error, IList<OmemoEncryptedMessage> messages)
        {
            CHAT_JID = chatJid;
            ERROR = error;
            MESSAGES = messages;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
