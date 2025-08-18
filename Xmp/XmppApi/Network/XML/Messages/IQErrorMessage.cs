using System.Xml;
using System.Xml.Linq;

namespace XmppApi.Network.XML.Messages
{
    public class IQErrorMessage: IQMessage
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly Error ERROR_OBJ;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 21/02/2018 Created [Fabian Sauter]
        /// </history>
        public IQErrorMessage(XmlNode n) : base(n)
        {
            XmlNode eNode = XMLUtils.getChildNode(n, ERROR);
            if (eNode is null)
            {
                ERROR_OBJ = new Error();
            }
            else
            {
                ERROR_OBJ = new Error(eNode);
            }
        }

        public IQErrorMessage(string from, string to, string id) : base(from, to, ERROR, id) { }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public override XElement toXElement()
        {
            XElement iq = base.toXElement();
            iq.Add(ERROR_OBJ.toXElement(""));
            return iq;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public override string ToString()
        {
            return "IQErrorMessage: " + ERROR_OBJ.ToString();
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
