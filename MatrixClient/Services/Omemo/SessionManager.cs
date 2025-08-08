using System;
using System.Collections.Generic;
using System.Linq;

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
    

    public DoubleRatchetSession GetOrCreateSession(string contactJid, int deviceId, OmemoContactKeyBundle contactBundle)
    {
        string key = $"{contactJid}:{deviceId}";

        if (!sessions.TryGetValue(key, out var session))
        {
            // Generate ephemeral key pair
            var senderEphemeralKey = OmemoKeyBundle.GenerateEphemeralKey();

            // Perform X3DH
            var sharedSecret = new X3DHAgreement().PerformX3DH(
                senderEphemeralKey,
                OmemoKeyBundle.ParsePublicKey(contactBundle.IdentityKey).Public,
                OmemoKeyBundle.ParsePublicKey(contactBundle.SignedPreKey).Public, 
                null // optional
            );

            // Initialize Double Ratchet session
            session = new DoubleRatchetSession(sharedSecret, deviceId, null);
            sessions[key] = session;
        }

        return session;
    }

    
    public string Decrypt(string contactJid, int deviceId, string ciphertext)
    {
        string key = $"{contactJid}:{deviceId}";

        if (!sessions.TryGetValue(key, out var session))
            throw new InvalidOperationException("No session found for contact/device");

        return session.Decrypt(ciphertext);
    }
}
