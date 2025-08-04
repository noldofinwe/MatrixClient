using System;
using System.Collections.Generic;
using XmppDotNet.Xmpp.Base;

namespace XmppDotNet.Xmpp.MessageArchiving
{
    public abstract class ArchiveBase : XmppXElementWithXData
    {
        #region Schema
        /*
          <xs:element name='chat'>
            <xs:complexType>
              <xs:choice minOccurs='0' maxOccurs='unbounded'>
                <xs:element name='from' type='messageType'/>
                <xs:element name='next' type='linkType'/>
                <xs:element ref='note'/>
                <xs:element name='previous' type='linkType'/>
                <xs:element name='to' type='messageType'/>
                <xs:any processContents='lax' namespace='##other'/>
              </xs:choice>
              <xs:attribute name='start' type='xs:dateTime' use='required'/>
              <xs:attribute name='subject' type='xs:string' use='optional'/>
              <xs:attribute name='thread' use='optional' type='xs:string'/>
              <xs:attribute name='version' use='optional' type='xs:nonNegativeInteger'/>
              <xs:attribute name='with' type='xs:string' use='required'/>
            </xs:complexType>
          </xs:element>
        */
        #endregion

        protected ArchiveBase(string tagName)
            : base(Namespaces.Archiving, tagName)
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

        public string Thread
        {
            get { return GetAttribute("thread"); }
            set { SetAttribute("thread", value); }
        }

        public string Subject
        {
            get { return GetAttribute("subject"); }
            set { SetAttribute("subject", value); }
        }

        public int Version
        {
            get { return GetAttributeInt("version"); }
            set { SetAttribute("version", value); }
        }

        public IEnumerable<ArchiveItem> GetItems()
        {
            return Elements<ArchiveItem>();
        }
        
        /// <summary>
        /// Gets or sets the link to the "next" collection.
        /// </summary>
        /// <value>
        /// The next.
        /// </value>
        public Next Next
        {
            get { return Element<Next>(); }
            set { Replace(value); }
        }

        /// <summary>
        /// Gets or sets the link to the "previous" collection.
        /// </summary>
        /// <value>
        /// The previous.
        /// </value>
        public Previous Previous
        {
            get { return Element<Previous>(); }
            set { Replace(value); }
        }
    }
}
