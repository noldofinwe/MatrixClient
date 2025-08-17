﻿using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_0384
{
    public class OmemoSubscribeToDeviceListMessage: PubSubSubscribeMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public OmemoSubscribeToDeviceListMessage(string fromFullJid, string fromBareJid, string to) : base(fromFullJid, fromBareJid, to, Consts.XML_XEP_0384_DEVICE_LIST_NODE) { }

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
