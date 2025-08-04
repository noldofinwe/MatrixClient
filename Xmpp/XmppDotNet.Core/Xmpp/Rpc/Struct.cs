using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.Rpc
{
    [XmppTag(Name = "struct", Namespace = Namespaces.IqRpc)]
    internal class Struct : XmppXElement
    {
        public Struct()
            : base(Namespaces.IqRpc, "struct")
        {
        }
    }
}
