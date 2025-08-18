﻿using System.Xml;
using System.Xml.Linq;
using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_0402
{
    public class ConferenceItem: AbstractPubSubItem
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public string name;
        public bool autoJoin;

        public string nick;
        public string password;

        public readonly bool IS_VALID;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 15/07/2018 Created [Fabian Sauter]
        /// </history>
        public ConferenceItem()
        {
            IS_VALID = false;
        }

        public ConferenceItem(XmlNode node)
        {
            IS_VALID = false;

            if (node != null)
            {
                id = node.Attributes["id"]?.Value;

                XmlNode confNode = XMLUtils.getChildNode(node, "conference");
                if (confNode is null)
                {
                    return;
                }

                name = confNode.Attributes["name"]?.Value;
                autoJoin = XMLUtils.tryParseToBool(confNode.Attributes["autojoin"]?.Value);

                XmlNode nNode = XMLUtils.getChildNode(confNode, "nick");
                nick = nNode != null ? nNode.InnerText : "";

                XmlNode pNode = XMLUtils.getChildNode(confNode, "password");
                password = pNode != null ? pNode.InnerText : "";
                IS_VALID = true;
            }
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        protected override XElement getContent(XNamespace ns)
        {
            XElement confNode = new XElement(ns + "conference");
            confNode.Add(new XAttribute("autojoin", autoJoin));
            if (name != null)
            {
                confNode.Add(new XAttribute("name", name));
            }
            if (password != null)
            {
                confNode.Add(new XElement(ns + "password", password));
            }
            if (nick != null)
            {
                confNode.Add(new XElement(ns + "nick", nick));
            }
            return confNode;
        }

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
