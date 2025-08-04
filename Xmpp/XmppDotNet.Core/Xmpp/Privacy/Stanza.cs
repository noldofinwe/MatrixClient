using System;

namespace XmppDotNet.Xmpp.Privacy
{
    /// <summary>
    /// enum for block or allow communications.
    /// This flags could be combined under the following conditions.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///     <item>All must stand alone, its not allowed to combine thsi flag</item>
    ///     <item>Message, Iq, IncomingPresence and Outgoing Presence could be combined, 
    ///         <b>but</b> its not allowed to combine more than 3 of this flag.
    ///         If you need all of them you have to use the All flag</item>
    /// </list>
    /// </remarks>    
    [Flags]
    public enum Stanza
    {
        /// <summary>
        /// Block all stanzas
        /// !!! Don't combine this flag with others!!!
        /// </summary>
        All = 0,

        /// <summary>
        /// Block messages
        /// </summary>
        Message = 1,

        /// <summary>
        /// Block IQs
        /// </summary>
        Iq = 2,

        /// <summary>
        /// Block Incoming Presences
        /// </summary>
        IncomingPresence = 4,

        /// <summary>
        /// Block Outgoing Presences
        /// </summary>
        OutgoingPresence = 8,
    }
}
