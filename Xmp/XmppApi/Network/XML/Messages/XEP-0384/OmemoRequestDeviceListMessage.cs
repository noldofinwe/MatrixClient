using XmppApi.Network.XML.Messages.XEP_0060;

namespace XmppApi.Network.XML.Messages.XEP_0384
{
    public class OmemoRequestDeviceListMessage: PubSubRequestNodeMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public OmemoRequestDeviceListMessage(string from, string to) : base(from, to, Consts.XML_XEP_0384_DEVICE_LIST_NODE, 1) { }

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
