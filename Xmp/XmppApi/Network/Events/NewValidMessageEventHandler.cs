using XmppApi.Network.XML.Messages;

namespace XmppApi.Network.Events
{
    public delegate void NewValidMessageEventHandler(IMessageSender sender, NewValidMessageEventArgs args);
}
