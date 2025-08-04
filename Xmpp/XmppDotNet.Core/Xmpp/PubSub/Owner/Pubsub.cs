using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.PubSub.Owner
{
    /// <summary>
    /// 
    /// </summary>
    [XmppTag(Name = "pubsub", Namespace = Namespaces.PubsubOwner)]
    public class PubSub : XmppXElement
    {
        #region Schema
        /*
          <xs:element name='pubsub'>
            <xs:complexType>
              <xs:choice>
                <xs:element ref='affiliations'/>
                <xs:element ref='configure'/>
                <xs:element ref='delete'/>
                <xs:element ref='purge'/>
                <xs:element ref='subscriptions'/>
              </xs:choice>
            </xs:complexType>
          </xs:element>
        */
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PubSub"/> class.
        /// </summary>
        public PubSub() : base(Namespaces.PubsubOwner, "pubsub")
        {
        }

        #region << Properties >>
        public Delete Delete
        {
            get { return Element<Delete>(); }
            set { Replace(value); }
        }

        public Purge Purge
        {
            get { return Element<Purge>(); }
            set { Replace(value); }
        }

        /// <summary>
        /// Gets or sets the Affiliations.
        /// </summary>
        /// <value>
        /// The affiliations.
        /// </value>
        public Affiliations Affiliations
        {
            get { return Element<Affiliations>(); }
            set { Replace(value); }
        }

        public Configure Configure
        {
            get { return Element<Configure>(); }
            set { Replace(value); }
        }

        /// <summary>
        /// Gets or sets the Subscriptions.
        /// </summary>
        /// <value>
        /// The subscriptions.
        /// </value>
        public Subscriptions Subscriptions
        {
            get { return Element<Subscriptions>(); }
            set { Replace(value); }
        }

        public PubSubOwnerType Type
        {
            get
            {
                if (Affiliations != null)
                    return PubSubOwnerType.Affiliations;
                
                if (Configure != null)
                    return PubSubOwnerType.Configure;
                
                if (Delete != null)
                    return PubSubOwnerType.Delete;
                
                if (Purge != null)
                    return PubSubOwnerType.Purge;
                
                if (Subscriptions != null)
                    return PubSubOwnerType.Subscriptions;

                return PubSubOwnerType.None;
            }
            set
            {
                RemoveNodes();
                switch (value)
                {
                    case PubSubOwnerType.Affiliations:
                        Add(new Subscriptions());
                        break;
                    case PubSubOwnerType.Configure:
                        Add(new Configure());
                        break;
                    case PubSubOwnerType.Delete:
                        Add(new Delete());
                        break;
                    case PubSubOwnerType.Purge:
                        Add(new Purge());
                        break;
                    case PubSubOwnerType.Subscriptions:
                        Add(new Subscriptions());
                        break;
                }
            }
        }
        #endregion
    }
}
