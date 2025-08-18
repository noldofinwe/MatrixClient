﻿using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace XmppApi.Network.XML.Messages.XEP_0045
{
    // https://xmpp.org/extensions/xep-0045.html#ban
    public class BanListMessage: IQMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly List<BanedUser> BANED_USERS;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 12/03/2018 Created [Fabian Sauter]
        /// </history>
        public BanListMessage(string from, string roomJid) : base(from, roomJid, GET, getRandomId())
        {
            BANED_USERS = new List<BanedUser>();
        }

        public BanListMessage(XmlNode node) : base(node)
        {
            BANED_USERS = new List<BanedUser>();

            XmlNode qNode = XMLUtils.getChildNode(node, "query", Consts.XML_XMLNS, Consts.XML_XEP_0045_NAMESPACE_ADMIN);
            if (qNode != null)
            {
                foreach (XmlNode n in qNode.ChildNodes)
                {
                    if (Equals(n.Name, "item"))
                    {
                        BANED_USERS.Add(new BanedUser(n));
                    }
                }
            }
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        protected override XElement getQuery()
        {
            XNamespace ns = Consts.XML_XEP_0045_NAMESPACE_ADMIN;
            XElement queryNode = new XElement(ns + "query");

            XElement itemNode = new XElement(ns + "item");
            itemNode.Add(new XAttribute("affiliation", Utils.mucAffiliationToString(MUCAffiliation.OUTCAST)));
            queryNode.Add(itemNode);

            return queryNode;
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
