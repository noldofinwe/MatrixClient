﻿using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.HttpUpload
{
    [XmppTag(Name = "get", Namespace = Namespaces.HttpUpload)]
    public class Get : XmppXElement
    {
        public Get(): base(Namespaces.HttpUpload, "get")
        {
        }

        /// <summary>
        ///   Gets or sets the url.
        /// </summary>
        /// <value>
        ///   The url.
        /// </value>
        public string Url
        {
            get { return GetAttribute("url"); }
            set { SetAttribute("url", value); }
        }
    }
}
