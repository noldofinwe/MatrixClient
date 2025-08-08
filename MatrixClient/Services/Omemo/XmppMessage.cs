using System.Xml.Linq;
using XmppDotNet.Xmpp.Base;

namespace MatrixClient.Services.Omemo;

public class XmppMessage
{
    public string From { get; set; }
    public string Body { get; set; }
    public string EncryptedPayload { get; set; }
    public bool OmemoEncrypted { get; set; }
    public int DeviceId { get; set; }

    public static XmppMessage FromMessage(Message msg)
    {
        return new XmppMessage
        {
            From = msg.From.ToString(),
            Body = msg.Body,
            OmemoEncrypted = IsOmemoEncrypted(msg),
            EncryptedPayload = GetOmemoPayload(msg),
            DeviceId = GetOmemoDeviceId(msg)
        };
    }
    
    public static bool IsOmemoEncrypted(Message msg)
    {
        XNamespace omemoNs = "eu.siacs.conversations.axolotl";
        var encrypted = msg.Element(omemoNs + "encrypted"); // ← parentheses!
        return encrypted != null;
    }

    public static string GetOmemoPayload(Message msg)
    {
        XNamespace omemoNs = "eu.siacs.conversations.axolotl";
        var encrypted = msg.Element(omemoNs + "encrypted");
        var payload = encrypted?.Element(omemoNs + "payload");
        return payload?.Value;
    }

    
    public static int GetOmemoDeviceId(Message msg)
    {
        XNamespace omemoNs = "eu.siacs.conversations.axolotl";
        var header = msg
            .Element(omemoNs + "encrypted")?
            .Element(omemoNs + "header");

        var sidAttr = header?.Attribute("sid");
        return sidAttr != null && int.TryParse(sidAttr.Value, out var sid) ? sid : -1;
    }


}
