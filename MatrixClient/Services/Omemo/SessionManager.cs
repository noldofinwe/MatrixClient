using System;
using System.Collections.Generic;
using System.Linq;
using XmppDotNet;

namespace MatrixClient.Services.Omemo;

public class SessionManager
{
  public SessionManager(OmemoKeyBundle myBundle)
  {
    this.myBundle = myBundle;
  }
  private Dictionary<string, DoubleRatchetSession> sessions = new();
  private readonly OmemoKeyBundle myBundle;

  public void StoreSession(string jid, int deviceId, DoubleRatchetSession session)
  {
    sessions[$"{jid}:{deviceId}"] = session;
  }

  public DoubleRatchetSession GetSession(int deviceId)
  {
    return sessions.Values.FirstOrDefault(s => s.RemoteDeviceId == deviceId);
  }


  public DoubleRatchetSession GetOrCreateSession(string contactJid, OmemoContactKeyBundle contactBundle)
  {
    string key = $"{contactJid}:{contactBundle.DeviceId}";

    if (!sessions.TryGetValue(key, out var session))
    {
      // Generate ephemeral key pair
      var senderEphemeralKey = OmemoKeyBundle.GenerateEphemeralKey();

      // Perform X3DH
      var sharedSecret = new X3DHAgreement().PerformX3DH(
          senderEphemeralKey,
          OmemoKeyBundle.ParsePublicKey(contactBundle.IdentityKey),
          OmemoKeyBundle.ParsePublicKey(contactBundle.SignedPreKey),
          OmemoKeyBundle.ParsePublicKey(contactBundle.PreKeys.FirstOrDefault()) // Use first prekey if available, otherwise pass
      );

      // Initialize Double Ratchet session
      session = new DoubleRatchetSession(sharedSecret, contactBundle.DeviceId, null);
      sessions[key] = session;
    }

    return session;
  }


  public string Decrypt(Jid contactJid, int deviceId, string ciphertext)
  {
    string key = $"{contactJid.Bare}:{deviceId}";

    if (!sessions.TryGetValue(key, out var session))
      return null;
    //throw new InvalidOperationException("No session found for contact/device");

    return session.Decrypt(ciphertext);
  }

  internal bool HasSession(Jid contactJid, int deviceId)
  {
    string key = $"{contactJid.Bare}:{deviceId}";
    return sessions.ContainsKey(key);
  }
}
