using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmppDotNet;
using XmppDotNet.Extensions.Client.PubSub;
using XmppDotNet.Xml;

namespace MatrixClient.Services.Omemo;


public class OmemoPublisher
{
    private readonly XmppClient client;
    private readonly int deviceId;
    private readonly OmemoKeyBundle bundle;

    public OmemoPublisher(XmppClient client, int deviceId, OmemoKeyBundle bundle)
    {
        this.client = client;
        this.deviceId = deviceId;
        this.bundle = bundle;
    }
    public async Task PublishBundle(Jid to)
    {
        string node = $"urn:xmpp:omemo:2:bundles:{deviceId}";

        XElement payload = new XElement("bundle",
            new XElement("signedPreKeyPublic", Convert.ToBase64String(bundle.GetPublicKeyBytes(bundle.SignedPreKey)), new XAttribute("signedPreKeyId", 1)),
            new XElement("signedPreKeySignature", Convert.ToBase64String(bundle.SignedPreKeySignature)),
            new XElement("identityKey", Convert.ToBase64String(bundle.GetPublicKeyBytes(bundle.IdentityKey))),
            new XElement("prekeys",
                bundle.OneTimePreKeys.Select((key, index) =>
                    new XElement("preKeyPublic", Convert.ToBase64String(bundle.GetPublicKeyBytes(key)), new XAttribute("preKeyId", index + 1))
                )
            )
        );
        var xmppPayload = new XmppXElement(payload);
        var item = new  XmppDotNet.Xmpp.PubSub.Item(xmppPayload)
        {
            Id = deviceId.ToString(), // Optional: use deviceId as item ID
        };

        await client.PublishItemAsync(to, node, item);
    }

}
