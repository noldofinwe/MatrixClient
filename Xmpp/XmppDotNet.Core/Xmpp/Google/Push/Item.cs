using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.Google.Push
{
    #region Xml sample
    /*
        <subscribe xmlns="google:push">
		    <item channel="cloudprint.google.com" from="cloudprint.google.com"/>
	    </subscribe>
     */
    #endregion

    [XmppTag(Name = Tag.Item, Namespace = Namespaces.GooglePush)]
    public class Item : XmppXElement
    {
        public Item()
            : base(Namespaces.GooglePush, "item")
        {
        }

        public string Channel
        {
            get { return GetAttribute("channel"); }
            set { SetAttribute("channel", value); }
        }

        public Jid From
        {
            get { return GetAttributeJid("from"); }
            set { SetAttribute("channel", value); }
        }
    }
}
