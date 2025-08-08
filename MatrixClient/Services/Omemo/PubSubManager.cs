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
        var iq = CreatePubSubIq(contactJid, "eu.siacs.conversations.axolotl.devicelist");
        var response = await _client.SendIqAsync(iq);
        var items = response?
            .Element(PubSubNs + "items")?
            .Elements(PubSubNs + "item");

        return items?
            .Select(item => int.TryParse(item.Attribute("id")?.Value, out var id) ? id : -1)
            .Where(id => id != -1)
            .ToList() ?? new List<int>();
    }

    public async Task<OmemoContactKeyBundle> GetBundleAsync(Jid contactJid, int deviceId)
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
        return bundles?.FirstOrDefault();
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
                Node = $"eu.siacs.conversations.axolotl.devicelist:{deviceId}"
            };

            var iq = new PubSubIq
            {
                Type = IqType.Get,
                To = contactJid,
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
                PreKeys = bundle.Elements(axolotlNs + "preKeyPublic")
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
            To = to,
            Id = Guid.NewGuid().ToString(),
            PubSub = pub
        };

        return iq;
    }
}