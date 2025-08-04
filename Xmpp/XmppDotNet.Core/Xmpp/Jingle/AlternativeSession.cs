using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.Jingle
{
    /*
        <alternative-session>
            <sid>b84tkkwlmb48kgfb</sid>
        </alternative-session>
    */
    [XmppTag(Name = "alternative-session", Namespace = Namespaces.Jingle)]
    public class AlternativeSession : XmppXElement
    {
        public AlternativeSession() : base(Namespaces.Jingle, "alternative-session")
        {}

        public string Sid
        {
            get { return GetTag("sid"); }
            set { SetTag("sid", value); }
        }
    }
}
