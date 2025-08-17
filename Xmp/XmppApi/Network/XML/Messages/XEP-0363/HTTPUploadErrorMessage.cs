﻿using System;
using System.Xml;
using XmppApi.Network.XML.Messages.XEP_0082;

namespace XmppApi.Network.XML.Messages.XEP_0363
{
    public class HTTPUploadErrorMessage: IQErrorMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly string TEXT;
        public readonly string TYPE_SHORT;
        public readonly string TYPE_LONG;
        public readonly HTTPUploadRequestSlotMessage REQUEST_MESSAGE;

        public readonly bool RETRY;
        public readonly DateTime RETRY_STAMP;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 02/06/2018 Created [Fabian Sauter]
        /// </history>
        public HTTPUploadErrorMessage(XmlNode node) : base(node)
        {
            RETRY = false;
            RETRY_STAMP = DateTime.MinValue;

            XmlNode errorNode = XMLUtils.getChildNode(node, "error");
            if (errorNode != null)
            {
                TYPE_SHORT = errorNode.Attributes["type"]?.Value;

                foreach (XmlNode n in errorNode.ChildNodes)
                {
                    switch (n.Name)
                    {
                        case "text" when string.Equals(n.NamespaceURI, Consts.XML_ERROR_NAMESPACE):
                            TEXT = n.InnerText;
                            break;

                        case "retry" when string.Equals(n.NamespaceURI, Consts.XML_XEP_0363_NAMESPACE):
                            RETRY = true;
                            XmlAttribute stamp = XMLUtils.getAttribute(n, "stamp");
                            if (stamp != null)
                            {
                                RETRY_STAMP = DateTimeHelper.Parse(stamp.Value);
                            }
                            break;

                        default:
                            if (n.NamespaceURI.Equals(Consts.XML_ERROR_NAMESPACE))
                            {
                                TYPE_LONG = n.Name;
                            }
                            break;
                    }
                }
            }

            XmlNode requestNode = XMLUtils.getChildNode(node, "request", Consts.XML_XMLNS, Consts.XML_XEP_0363_NAMESPACE);
            if (requestNode != null)
            {
                REQUEST_MESSAGE = new HTTPUploadRequestSlotMessage(node);
            }
            else
            {
                REQUEST_MESSAGE = null;
            }
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
