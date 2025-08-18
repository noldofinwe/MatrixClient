using System;
using System.Linq;
using System.Xml.Linq;
using XmppDotNet;
using XmppDotNet.Xmpp.Base;

namespace MatrixClient.Services.Omemo;

public class XmppMessage
{
  public Jid From { get; set; }
  public string Body { get; set; }
  public string EncryptedPayload { get; set; }
  public bool OmemoEncrypted { get; set; }
  public int SenderDeviceId { get; set; }
  public string Key { get; set; }

  public static XmppMessage FromMessage(Message msg, int deviceId)
  {
    var isOmemoEncrypted = IsOmemoEncrypted(msg);
    return new XmppMessage
    {
      From = msg.From,
      Body = msg.Body,
      OmemoEncrypted = isOmemoEncrypted,
      EncryptedPayload = GetOmemoPayload(msg),
      SenderDeviceId = GetOmemoDeviceId(msg),

      Key = isOmemoEncrypted ? ExtractOmemoKey(msg, deviceId) : null
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


  public static string ExtractOmemoKey(Message msg, int yourDeviceId)
  {

    // OMEMO namespace
    XNamespace omenoNs = "eu.siacs.conversations.axolotl";

    // Find the <encrypted> element
    var encrypted = msg.Descendants(omenoNs + "encrypted").FirstOrDefault();
    if (encrypted == null)
      throw new Exception("No <encrypted> element found.");

    // Find the <header> element
    var header = encrypted.Element(omenoNs + "header");
    if (header == null)
      throw new Exception("No <header> element found.");

    // Find the <key> element with matching rid
    var keyElement = header.Elements(omenoNs + "key")
        .FirstOrDefault(k => (string)k.Attribute("rid") == yourDeviceId.ToString());

    if (keyElement == null)
      return null;
      //throw new Exception($"No <key> element found for device ID {yourDeviceId}.");

    return keyElement.Value; // Base64-encoded encrypted key
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
