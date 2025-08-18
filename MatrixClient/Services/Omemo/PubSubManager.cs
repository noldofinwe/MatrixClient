using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmppDotNet;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.PubSub;
using XmppDotNet.Xmpp.XData;

namespace MatrixClient.Services.Omemo;

using System.Xml.Linq;
using XmppDotNet.Extensions.Client.PubSub;

public class PubSubManager
{
  private readonly XmppClient _client;
  private static readonly XNamespace PubSubNs = "http://jabber.org/protocol/pubsub";

  public PubSubManager(XmppClient client)
  {
    _client = client;
  }

  public async Task<List<int>> GetDeviceListAsync(Jid contactJid)
  {
    XNamespace pubsubNs = "http://jabber.org/protocol/pubsub";
    XNamespace axolotlNs = "eu.siacs.conversations.axolotl";

    var iq = CreatePubSubIq(contactJid, "eu.siacs.conversations.axolotl.devicelist");
    var response = await _client.SendIqAsync(iq);

    // Start from <iq> root
    var pubsubElement = response?.Element(pubsubNs + "pubsub");
    if (pubsubElement == null) return null;

    var itemsElement = pubsubElement.Element(pubsubNs + "items");
    if (itemsElement == null) return null;

    var itemElement = itemsElement.Element(pubsubNs + "item");
    if (itemElement == null) return null;

    var listElement = itemElement.Element(axolotlNs + "list");
    if (listElement == null) return null;

    var deviceIds = listElement.Elements(axolotlNs + "device")
        .Select(e => e.Attribute("id")?.Value)
        .Where(id => !string.IsNullOrEmpty(id))
        .ToList();

    return deviceIds.Where(x => !string.IsNullOrEmpty(x)).Select(x => int.Parse(x)).ToList();
  }

  public async Task EnsureDeviceIdPublishedAsync(Jid to, int deviceId)
  {
    string node = "eu.siacs.conversations.axolotl.devicelist";
    XNamespace axolotlNs = "eu.siacs.conversations.axolotl";
    // Try to get the current device list
    List<int> existingDeviceIds;
    try
    {
      existingDeviceIds = await GetDeviceListAsync(to.Bare);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Failed to retrieve device list: {ex.Message}");
      existingDeviceIds = new List<int>();
    }

    // Check if the device ID is already present
    if (existingDeviceIds == null || !existingDeviceIds.Contains(deviceId))
    {
      if(existingDeviceIds == null)
      {
        existingDeviceIds = new List<int>();
      }
      var updatedDeviceIds = existingDeviceIds.Append(deviceId).Distinct();

      // Create the XML payload
      XElement payload = new XElement(axolotlNs + "list",
          updatedDeviceIds.Select(id => new XElement(axolotlNs + "device", new XAttribute("id", id)))
      );

      var xmppPayload = new XmppXElement(payload);
      var item = new XmppDotNet.Xmpp.PubSub.Item(xmppPayload)
      {
        Id = "current" // You can use a static ID like "current" for the device list
      };

      // Publish the updated device list
      var result = await _client.PublishItemAsync(to.Bare, node, item);
      Console.WriteLine($"Published device ID {deviceId} to device list.");
    }
    else
    {
      Console.WriteLine($"Device ID {deviceId} is already published.");
    }
  }

  public async Task<List<OmemoContactKeyBundle>> GetBundleAsync(Jid contactJid, int deviceId)
  {
    var node = $"eu.siacs.conversations.axolotl.bundles:{deviceId}";
    var iq = CreatePubSubIq(contactJid, node);
    var response = await _client.SendIqAsync(iq);

    XNamespace pubsubNs = "http://jabber.org/protocol/pubsub";
    XNamespace axolotlNs = "eu.siacs.conversations.axolotl";


    // Start from <iq> root
    var pubsubElement = response?.Element(pubsubNs + "pubsub");
    if (pubsubElement == null) return null;

    var itemsElement = pubsubElement.Element(pubsubNs + "items");
    if (itemsElement == null) return null;

    var itemElement = itemsElement.Element(pubsubNs + "item");
    if (itemElement == null) return null;

    var listElement = itemElement.Element(axolotlNs + "list");
    if (listElement == null) return null;

    var deviceIds = listElement.Elements(axolotlNs + "device")
        .Select(e => e.Attribute("id")?.Value)
        .Where(id => !string.IsNullOrEmpty(id))
        .ToList();

    var bundles = await FetchOmemoBundlesAsync(contactJid, deviceIds);
    return bundles;
  }

  public async Task<List<OmemoContactKeyBundle>> FetchOmemoBundlesAsync(Jid contactJid, List<string> deviceIds)
  {
    var bundles = new List<OmemoContactKeyBundle>();
    var pubsubNs = XNamespace.Get("http://jabber.org/protocol/pubsub");
    var axolotlNs = XNamespace.Get("eu.siacs.conversations.axolotl");

    foreach (var deviceId in deviceIds)
    {
      var pub = new PubSub();
      pub.Items = new Items
      {
        Node = $"eu.siacs.conversations.axolotl.bundles:{deviceId}"
      };

      var iq = new PubSubIq
      {
        Type = IqType.Get,
        To = contactJid.Bare,
        Id = Guid.NewGuid().ToString(),
        PubSub = pub
      };

      var response = await _client.SendIqAsync(iq);

      if (response == null) continue;
      if (response.Type == IqType.Error)
      {
        var errorText = response.Element(XNamespace.Get("urn:ietf:params:xml:ns:xmpp-stanzas") + "text")?.Value;
        Console.WriteLine($"Device {deviceId} not found: {errorText}");
        continue;
      }

      var bundle = response?
          .Element(pubsubNs + "pubsub")?
          .Element(pubsubNs + "items")?
          .Element(pubsubNs + "item")?
          .Element(axolotlNs + "bundle");

      if (bundle == null) continue;

      var parsed = new OmemoContactKeyBundle
      {
        DeviceId = int.Parse(deviceId),
        IdentityKey = bundle.Element(axolotlNs + "identityKey")?.Value,
        SignedPreKey = bundle.Element(axolotlNs + "signedPreKeyPublic")?.Value,
        //SignedPreKeySignature = bundle.Element(axolotlNs + "signedPreKeySignature")?.Value,
        PreKeys = bundle
              .Element(axolotlNs + "prekeys")?
              .Elements(axolotlNs + "preKeyPublic")
              .Select(e => e.Value)
              .ToList()
      };

      bundles.Add(parsed);

    }

    return bundles;
  }

  private Iq CreatePubSubIq(Jid to, string node)
  {
    var pub = new PubSub();
    pub.Items = new Items
    {
      Node = "eu.siacs.conversations.axolotl.devicelist"
    };

    var iq = new PubSubIq
    {
      Type = IqType.Get,
      To = to.Bare,
      Id = Guid.NewGuid().ToString(),
      PubSub = pub
    };

    return iq;
  }
}