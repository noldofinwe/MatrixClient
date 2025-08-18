﻿using System.Xml;
using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_0048
{
    public class BookmarksResultMessage: AbstractPubSubResultMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public StorageItem STORAGE { get; private set; }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 22/07/2018 Created [Fabian Sauter]
        /// </history>
        public BookmarksResultMessage(XmlNode n) : base(n)
        {
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
        protected override void loadContent(XmlNodeList content)
        {
            foreach (XmlNode node in content)
            {
                if (string.Equals(node.Name, "items") && string.Equals(node.Attributes["node"]?.Value, Consts.XML_XEP_0048_NAMESPACE))
                {
                    XmlNode itemNode = XMLUtils.getChildNode(node, "item", "id", "current");
                    if (itemNode != null)
                    {
                        STORAGE = new StorageItem(itemNode);
                        return;
                    }
                }
            }
            STORAGE = new StorageItem();
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
