using System;
using XmppDotNet.Attributes;
using XmppDotNet.Xmpp.Base;

namespace XmppDotNet.Xmpp.MessageArchiving
{
    [XmppTag(Name = "list", Namespace = Namespaces.Archiving)]
    public class List : XmppXElementWithResultSet
    {
        #region Schema
        /*
          <xs:element name='list'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='chat' minOccurs='0' maxOccurs='unbounded'/>
                <xs:any processContents='lax' namespace='##other' minOccurs='0' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='end' type='xs:dateTime' use='optional'/>
              <xs:attribute name='exactmatch' type='xs:boolean' use='optional'/>
              <xs:attribute name='start' type='xs:dateTime' use='optional'/>
              <xs:attribute name='with' type='xs:string' use='optional'/>
            </xs:complexType>
          </xs:element>
        */
        #endregion

        public List() : base(Namespaces.Archiving, "list")
        {
        }

        public Jid With
        {
            get { return GetAttributeJid("with"); }
            set { SetAttribute("with", value); }
        }

        public DateTime Start
        {
            get { return GetAttributeIso8601Date("start"); }
            set { SetAttributeIso8601Date("start", value); }
        }

        public DateTime End
        {
            get { return GetAttributeIso8601Date("end"); }
            set { SetAttributeIso8601Date("end", value); }
        }

        public bool ExactMatch
        {
            get { return GetAttributeBool("exactmatch"); }
            set { SetAttribute("exactmatch", value); }
        }
    }
}
