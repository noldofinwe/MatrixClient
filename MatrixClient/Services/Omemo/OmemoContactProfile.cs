using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XmppDotNet;

namespace MatrixClient.Services.Omemo;

public class OmemoContactProfile
{
    public Jid ContactJid { get; }
    public List<OmemoContactKeyBundle> Bundles { get; } = new();

    public OmemoContactProfile(Jid contactJid)
    {
        ContactJid = contactJid;
    }

    public async Task LoadAsync(PubSubManager pubSub)
    {
        Bundles.Clear();
        var deviceIds = await pubSub.GetDeviceListAsync(ContactJid);

        foreach (var deviceId in deviceIds)
        {
            var bundle = await pubSub.GetBundleAsync(ContactJid, deviceId);
            if (bundle != null)
                Bundles.Add(bundle);
        }
    }

    public OmemoContactKeyBundle GetBundleForDevice(int deviceId)
    {
        return Bundles.FirstOrDefault(b => b.DeviceId == deviceId);
    }

    public IEnumerable<int> GetDeviceIds()
    {
        return Bundles.Select(b => b.DeviceId);
    }

    public bool HasValidBundles => Bundles.Any();
}
