using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.Rpc
{
    [XmppTag(Name = Tag.Query, Namespace = Namespaces.IqRpc)]
    public class Rpc : XmppXElement
    {
        public Rpc()
            : base(Namespaces.IqRpc, Tag.Query)
        {
        }

        public MethodCall MethodCall
        {
            get { return Element<MethodCall>(); }
            set { Replace(value); }
        }

        public MethodResponse MethodResponse
        {
            get { return Element<MethodResponse>(); }
            set { Replace(value); }
        }
    }
}
