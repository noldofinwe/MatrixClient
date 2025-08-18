using System.Xml;

namespace XmppApi.Network.XML.Messages.XEP_0030
{
    public class DiscoItem: IDiscoItem
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly string JID;
        public readonly string NAME;
        public readonly string NODE;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 01/01/2018 Created [Fabian Sauter]
        /// </history>
        public DiscoItem(XmlNode n)
        {
            if (n != null)
            {
                JID = n.Attributes["jid"]?.Value;
                NAME = n.Attributes["name"]?.Value;
                NODE = n.Attributes["node"]?.Value;
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
