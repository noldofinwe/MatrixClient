using System;
using System.Threading.Tasks;
using XmppDotNet;
using XmppDotNet.Xmpp.Base;

namespace MatrixClient.Services.Omemo;

public class OmemoClient
{
    private readonly XmppClient xmppClient;
    private readonly int deviceId;
    private readonly OmemoKeyBundle keyBundle;
    private readonly OmemoPublisher publisher;
    private readonly SessionManager sessionManager;

    public int DeviceId => deviceId;
    public OmemoClient(XmppClient xmppClient, int deviceId)
    {
        this.xmppClient = xmppClient;
        this.deviceId = deviceId;

        // Generate keys
        keyBundle = new OmemoKeyBundle();

        // Publish bundle to PEP
        publisher = new OmemoPublisher(xmppClient, deviceId, keyBundle);

        // Initialize session manager
        sessionManager = new SessionManager(keyBundle);

    }

    public async Task PublishBundle()
    {
        await publisher.PublishBundle(xmppClient.Jid.Bare);
    }
  

    public void CreateSession(Jid jid, OmemoContactKeyBundle contactBundle )
    {
        sessionManager.GetOrCreateSession(jid, deviceId, contactBundle);
    }
    public void HandleMessage(Message el)
    {
       var xmppMessage = XmppMessage.FromMessage(el);
       if (xmppMessage.OmemoEncrypted)
       {
           var test =  sessionManager.Decrypt(el.From, 12345, xmppMessage.Body);
       }
    }
}
