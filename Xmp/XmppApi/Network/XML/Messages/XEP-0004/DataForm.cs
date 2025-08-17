﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace XmppApi.Network.XML.Messages.XEP_0004
{
    public class DataForm
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public DataFormType type;
        public List<Field> fields;
        public string titel;
        public string instructions;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 02/06/2018 Created [Fabian Sauter]
        /// </history>
        public DataForm() : this(DataFormType.RESULT)
        {
        }

        public DataForm(DataFormType type) : this(type, new List<Field>())
        {
        }

        public DataForm(DataFormType type, List<Field> fields)
        {
            this.fields = fields;
            this.type = type;
            titel = null;
            instructions = null;
        }

        public DataForm(XmlNode node) : this(DataFormType.CANCEL)
        {
            loadRoomConfig(node);
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public Field getField(string var)
        {
            return fields.Find((f) => { return Equals(var, f.var); });
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public void addToXElement(XElement node)
        {
            XNamespace ns = Consts.XML_XEP_0004_NAMESPACE;
            XElement xNode = new XElement(ns + "x");
            xNode.Add(new XAttribute("type", type.ToString().ToLowerInvariant()));

            // Add the XEP-0336 (Data Forms - Dynamic Forms) namespace even if we do not need it:
            xNode.Add(new XAttribute(XNamespace.Xmlns + "xdd", Consts.XML_XEP_0336_NAMESPACE));

            foreach (Field f in fields)
            {
                xNode.Add(f.toXElement(ns));
            }

            if (titel != null)
            {
                xNode.Add(new XElement("title", titel));
            }

            if (instructions != null)
            {
                xNode.Add(new XElement("instructions", instructions));
            }

            node.Add(xNode);
        }

        public static DataFormType parseDataFormType(string s)
        {
            DataFormType type = DataFormType.FROM;
            Enum.TryParse(s?.ToUpper(), out type);
            return type;
        }

        public void loadRoomConfig(XmlNode node)
        {
            type = parseDataFormType(node.Attributes["type"]?.Value);

            foreach (XmlNode n in node.ChildNodes)
            {
                switch (n.Name)
                {
                    case "field":
                        fields.Add(new Field(n));
                        break;

                    case "title":
                        titel = n.InnerText;
                        break;

                    case "instructions":
                        instructions = n.InnerText;
                        break;

                    default:
                        Logging.Logger.Warn("Unknown data form config element '" + n.Name + "' received!");
                        break;
                }
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
