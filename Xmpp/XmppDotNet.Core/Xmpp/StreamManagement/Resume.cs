using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.StreamManagement
{
    [XmppTag(Name = "resume", Namespace = Namespaces.FeatureStreamManagement)]
    public class Resume : XmppXElement
    {
        internal Resume(string tagname) : base(Namespaces.FeatureStreamManagement, tagname)
        {
        }

        public Resume() : this("resume")
        {
        }

        /// <summary>
        /// The sequenze number of the last handled stanza of the previous connection.
        /// </summary>
        public long LastHandledStanza
        {
            get { return GetAttributeLong("h"); }
            set { SetAttribute("h", value); }
        }

        /// <summary>
        /// The SM-ID of the former stream.
        /// </summary>
        public string PreviousId
        {
            get { return GetAttribute("previd"); }
            set { SetAttribute("previd", value); }
        }
    }
}
