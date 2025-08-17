using System.Collections.Generic;
using System.Xml;
using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_IoT
{
    public abstract class AbstractValueNodeEventMessage: AbstractPubSubEventMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly List<IoTValue> VALUES;
        public readonly string NODE_NAME;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public AbstractValueNodeEventMessage(XmlNode node, string nodeName) : base(node)
        {
            NODE_NAME = nodeName;
            VALUES = new List<IoTValue>();
            XmlNode eventNode = XMLUtils.getChildNode(node, "event", Consts.XML_XMLNS, Consts.XML_XEP_0060_NAMESPACE_EVENT);
            if (!(eventNode is null))
            {
                XmlNode itemsNode = XMLUtils.getChildNode(eventNode, "items", "node", NODE_NAME);
                if (!(itemsNode is null))
                {
                    foreach (XmlNode itemNode in itemsNode.ChildNodes)
                    {
                        if (string.Equals(itemNode.Name, "item"))
                        {
                            XmlNode valNode = XMLUtils.getChildNode(itemNode, "val", Consts.XML_XMLNS, Consts.XML_XEP_IOT_NAMESPACE);
                            if (!(valNode is null))
                            {
                                VALUES.Add(new IoTValue(itemNode.Attributes["id"]?.Value, valNode));
                            }
                        }
                    }
                }
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
