﻿using System.Xml.Linq;
using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_0384
{
    public class OmemoRequestBundleInformationMessage: PubSubRequestNodeMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly uint DEVICE_ID;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public OmemoRequestBundleInformationMessage(string from, string to, uint deviceId) : base(from, to, Consts.XML_XEP_0384_BUNDLES_NODE, 1)
        {
            DEVICE_ID = deviceId;
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
        protected override XElement getContent(XNamespace ns)
        {
            XElement itemsNode = base.getContent(ns);
            XElement itemNode = new XElement(ns + "item");
            itemNode.Add(new XAttribute("id", DEVICE_ID));
            itemsNode.Add(itemNode);
            return itemsNode;
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
