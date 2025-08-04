using XmppDotNet.Attributes;

namespace XmppDotNet.Xmpp.MessageArchiving
{
    /// <summary>
    /// The Note specifies a private note about the conversation. 
    /// </summary>
    [XmppTag(Name = "note", Namespace = Namespaces.Archiving)]
    public class Note : ArchiveItem
    {
        public Note() : base("note")
        {}
    }
}
