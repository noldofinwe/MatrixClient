﻿using System.Xml;
using System.Xml.Linq;

namespace XmppApi.Network.XML.Messages.XEP_0060
{
    public abstract class AbstractPubSubMessage: IQMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 07/04/2018 Created [Fabian Sauter]
        /// </history>
        protected AbstractPubSubMessage(string from, string to) : base(from, to, SET, getRandomId())
        {
        }

        protected AbstractPubSubMessage(XmlNode n) : base(n)
        {

        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        protected override XElement getQuery()
        {
            XNamespace ns = getPubSubNamespace();
            XElement pubSubNode = new XElement(ns + "pubsub");

            addContent(pubSubNode, ns);
            return pubSubNode;
        }

        protected abstract void addContent(XElement node, XNamespace ns);

        protected virtual XNamespace getPubSubNamespace()
        {
            return Consts.XML_XEP_0060_NAMESPACE;
        }

        protected XmlNode getPubSubNode(XmlNode node)
        {
            return XMLUtils.getChildNode(node, "pubsub", Consts.XML_XMLNS, Consts.XML_XEP_0060_NAMESPACE);
        }

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
