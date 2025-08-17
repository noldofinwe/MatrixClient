﻿using System.Xml;
using System.Xml.Linq;
using XmppApi.Network.XML.Messages.Features;

namespace XmppApi.Network.XML.Messages
{
    internal class OpenStreamAnswerMessage: AbstractMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private readonly XmlNode STREAM_NODE;
        private readonly string FROM;
        private readonly string TO;
        private readonly StreamFeaturesMessage STREAM_FEATURES;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 29/01/2017 Created [Fabian Sauter]
        /// </history>
        public OpenStreamAnswerMessage(XmlNode streamNode) : base(streamNode.Attributes["id"]?.Value)
        {
            STREAM_NODE = streamNode;
            FROM = STREAM_NODE.Attributes["from"]?.Value;
            TO = STREAM_NODE.Attributes.GetNamedItem("to")?.Value;
            STREAM_FEATURES = getStreamFeaturesMessage(streamNode);
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public StreamFeaturesMessage getStreamFeaturesMessage()
        {
            return STREAM_FEATURES;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public override XElement toXElement()
        {
            throw new System.NotImplementedException();
        }

        public StreamFeaturesMessage getStreamFeaturesMessage(XmlNode node)
        {
            XmlNode n = XMLUtils.getChildNode(node, "stream:features");
            return n is null ? null : new StreamFeaturesMessage(n);
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
