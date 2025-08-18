using System.Xml.Linq;
using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_IoT
{
    public class IoTPubSubItem: AbstractPubSubItem
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly IoTValue VALUE;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public IoTPubSubItem(IoTValue value, string id)
        {
            VALUE = value;
            this.id = id;
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
            return VALUE.toXElement(ns);
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
