using XmppDotNet.Attributes;

namespace XmppDotNet.Xmpp.XData
{
    /// <summary>
    /// Used in XData seach.
    /// includes the headers of the search results
    /// </summary>
    [XmppTag(Name = Tag.Item, Namespace = Namespaces.XData)]
    public class Item : FieldContainer
    {
        #region << Constructors >>
        public Item() : base("item")
        {            
        }
        #endregion
    }
}
