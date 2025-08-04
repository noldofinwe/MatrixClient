using XmppDotNet.Attributes;

namespace XmppDotNet.Xmpp.Rpc
{
    [XmppTag(Name = "methodCall", Namespace = Namespaces.IqRpc)]
    public class MethodCall : XmlRpcBase
    {
        public MethodCall()
            : base("methodCall")
        {
        }

        /// <summary>
        /// The RPC Method name
        /// </summary>
        public string MethodName
        {
            get { return GetTag("methodName"); }
            set { SetTag("methodName", value); }
        }
    }
}
