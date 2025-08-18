using System.Threading.Tasks;
using XmppApi.Network.XML.Messages;
using XmppApi.Network.XML.Messages.XEP_0198;

namespace XmppApi.Network
{
    internal class StreamManagementHelper
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 21/05/2018 Created [Fabian Sauter]
        /// </history>
        public StreamManagementHelper()
        {
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task onMessageReceivedAsync(AbstractMessage msg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (msg is SMRequestMessage reqMsg)
            {

            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task onMessageSend(AbstractMessage msg)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
