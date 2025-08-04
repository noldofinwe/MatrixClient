﻿namespace XmppDotNet.Xmpp.Avatar
{
    using XmppDotNet.Attributes;
    using XmppDotNet.Xml;
    
    /// <summary>
    /// Represents the avatar metadata element
    /// </summary>
    [XmppTag(Name = "metadata", Namespace = Namespaces.AvatarMetadata)]
    public class Metadata : XmppXElement
    {
        public Metadata()
            : base(Namespaces.AvatarMetadata, "metadata")
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="Info"/>
        /// </summary>
        public Info Info
        {
            get { return Element<Info>(); }
            set { Replace(value); }
        }
    }
}
