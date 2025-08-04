using System.Collections.Generic;
using XmppDotNet.Attributes;
using XmppDotNet.Xml;

namespace XmppDotNet.Xmpp.Jingle.Apps.Rtp
{
    [XmppTag(Name = "description", Namespace = Namespaces.JingleAppsRtp)]
    public class Description : XmppXElement
    {
        public Description()
            : base(Namespaces.JingleAppsRtp, "description")
        {
        }

        /// <summary>
        /// specifies the media type, such as "audio" or "video".
        /// </summary>
        public Media Media
        {
            get { return GetAttributeEnum<Media>("media"); }
            set { SetAttribute("media", value.ToString().ToLower()); }
        }

        /// <summary>
        /// The 32-bit synchronization source for this media stream, as defined in RFC 3550.
        /// </summary>
        public string Ssrc
        {
            get { return GetAttribute("ssrc"); }
            set { SetAttribute("ssrc", value); }
        }


        #region << PayloadType properties >>
        /// <summary>
        /// Adds the type of the payload.
        /// </summary>
        /// <returns></returns>
        public PayloadType AddPayloadType()
        {
            var payload = new PayloadType();
            Add(payload);

            return payload;
        }
        
        /// <summary>
        /// Adds the payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        public void AddPayloadType(PayloadType payload)
        {
            Add(payload);
        }
        
        /// <summary>
        /// Addpayloads the types.
        /// </summary>
        /// <param name="payloadTypes">The payload types.</param>
        public void AddPayloadTypes(PayloadType[] payloadTypes)
        {
            foreach (var payload in payloadTypes)
                Add(payload);
        }

        /// <summary>
        /// Gets the payload types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PayloadType> GetPayloadTypes()
        {
            return Elements<PayloadType>();
        }

        /// <summary>
        /// Sets the payloadType.
        /// </summary>
        /// <param name="payloadTypes">The payload types.</param>
        public void SetPayloadTypes(IEnumerable<PayloadType> payloadTypes)
        {
            RemoveAllPayloadTypes();
            foreach (PayloadType cand in payloadTypes)
                AddPayloadType(cand);
        }

        /// <summary>
        /// Removes all payload types.
        /// </summary>
        public void RemoveAllPayloadTypes()
        {
            RemoveAll<PayloadType>();
        }
        #endregion
    }
}
