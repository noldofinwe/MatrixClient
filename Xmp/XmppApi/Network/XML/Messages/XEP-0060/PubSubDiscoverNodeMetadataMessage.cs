﻿using System.Xml.Linq;
using XmppApi.Network.XML.Messages.XEP_0030;

namespace XmppApi.Network.XML.Messages.XEP_0060
{
    public class PubSubDiscoverNodeMetadataMessage: DiscoRequestMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly string NODE;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 17/07/2018 Created [Fabian Sauter]
        /// </history>
        public PubSubDiscoverNodeMetadataMessage(string from, string to, string node) : base(from, to, DiscoType.INFO)
        {
            NODE = node;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        protected override XElement getQuery()
        {
            XElement query = base.getQuery();
            query.Add(new XAttribute("node", NODE));
            return query;
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
