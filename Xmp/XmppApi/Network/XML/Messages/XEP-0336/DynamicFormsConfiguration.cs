﻿using System.Xml;
using System.Xml.Linq;

namespace XmppApi.Network.XML.Messages.XEP_0336
{
    public class DynamicFormsConfiguration
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public DynamicFormsFlags flags;
        /// <summary>
        /// Only has a value if the DynamicFormsFlags.ERROR flag in flags is set.
        /// </summary>
        public readonly string errorMessage;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public DynamicFormsConfiguration() { }

        public DynamicFormsConfiguration(XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                switch (n.Name)
                {
                    case "readOnly":
                        flags |= DynamicFormsFlags.READ_ONLY;
                        break;

                    case "postBack":
                        flags |= DynamicFormsFlags.POST_BACK;
                        break;

                    case "notSame":
                        flags |= DynamicFormsFlags.NOT_SAME;
                        break;

                    case "error":
                        flags |= DynamicFormsFlags.ERROR;
                        errorMessage = n.InnerText;
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void addToNode(XElement node)
        {
            XNamespace ns = Consts.XML_XEP_0336_NAMESPACE;
            if (flags.HasFlag(DynamicFormsFlags.READ_ONLY))
            {
                node.Add(new XElement(ns + "readOnly"));
            }
            if (flags.HasFlag(DynamicFormsFlags.POST_BACK))
            {
                node.Add(new XElement(ns + "postBack"));
            }
            if (flags.HasFlag(DynamicFormsFlags.NOT_SAME))
            {
                node.Add(new XElement(ns + "notSame"));
            }
            if (flags.HasFlag(DynamicFormsFlags.ERROR))
            {
                XElement errorNode = new XElement(ns + "error");
                errorNode.SetValue(errorMessage);
                node.Add(errorNode);
            }
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
