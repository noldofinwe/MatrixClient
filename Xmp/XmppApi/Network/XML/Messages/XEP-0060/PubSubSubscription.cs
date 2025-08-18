using System;
using System.Xml;

namespace XmppApi.Network.XML.Messages.XEP_0060
{
    public class PubSubSubscription
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly string NODE;
        public readonly string JID;
        public readonly PubSubSubscriptionState SUBSCRIPTION;
        public readonly string SUB_ID;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 10/11/2018 Created [Fabian Sauter]
        /// </history>
        public PubSubSubscription(XmlNode node)
        {
            NODE = node.Attributes["node"]?.Value;
            JID = node.Attributes["jid"]?.Value;
            if (!Enum.TryParse(node.Attributes[""]?.Value, out SUBSCRIPTION))
            {
                SUBSCRIPTION = PubSubSubscriptionState.NONE;
            }
            SUB_ID = node.Attributes["subid"]?.Value;
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
