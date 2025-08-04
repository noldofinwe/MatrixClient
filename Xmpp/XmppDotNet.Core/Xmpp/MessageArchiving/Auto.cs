using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.MessageArchiving
{
    [XmppTag(Name = "auto", Namespace = Namespaces.Archiving)]
    public class Auto : XmppXElement
    {
        #region Schema
        /*
          <xs:element name='auto'>
            <xs:complexType>
              <xs:sequence>
                <xs:any processContents='lax' namespace='##other' minOccurs='0' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='save' type='xs:boolean' use='required'/>
            </xs:complexType>
          </xs:element>
        */
        #endregion
        public Auto() : base(Namespaces.Archiving, "auto")
        {
        }

        public new bool Save
        {
            get { return GetAttributeBool("save"); }
            set { SetAttribute("save", value); }
        }
    }
}
