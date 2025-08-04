using XmppDotNet.Attributes;

namespace XmppDotNet.Xmpp.Privacy
{
    [XmppTag(Name = "default", Namespace = Namespaces.IqPrivacy)]
    public class Default : Base.XmppXElementWithNameAttribute
    {
        public Default() : this("default")
        {
        }

        internal Default(string name)
            : base(Namespaces.IqPrivacy, name)
        {
        }
    }
}
