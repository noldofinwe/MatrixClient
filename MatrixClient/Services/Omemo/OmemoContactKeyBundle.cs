using System.Collections.Generic;

namespace MatrixClient.Services.Omemo;

public class OmemoContactKeyBundle
{
    public int DeviceId { get; set; }
    public string IdentityKey { get; set; }
    public string SignedPreKey { get; set; }
    public List<string> PreKeys { get; set; } = new();
}
