using XmppDotNet.Attributes;

namespace XmppDotNet.Xmpp.Privacy
{
    /// <summary>
    /// 
    /// </summary>
    public enum Type
    {
        None = -1,

        /// <summary>
        /// 
        /// </summary>
        [Name("jid")]
        Jid,

        /// <summary>
        /// 
        /// </summary>
        [Name("group")]
        Group,

        /// <summary>
        /// 
        /// </summary>
        [Name("subscription")]
        Subscription
    }
}
