using System;
using System.Text;
using System.Threading.Tasks;
using XmppDotNet.Idn;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp.Sasl;
using System.Threading;

namespace XmppDotNet.Sasl.Scram
{
    /// <summary>
    /// XMPP implementation of SCRAM SHA-1 SASL
    /// </summary>
    public class ScramSha1Processor : ISaslProcessor
    {
        /// <inheritdoc/>
        public async Task<XmppXElement> AuthenticateClientAsync(IXmppClient xmppClient, CancellationToken cancellationToken)
        {
            var scramHelper = new ScramHelper();

            var username = xmppClient.Jid.Local;
#if STRINGPREP
           var  password = StringPrep.SaslPrep(xmppClient.Password);
#else
            var password = xmppClient.Password;
#endif

            string msg = ToB64String(scramHelper.GenerateFirstClientMessage(username));
            var authMessage = new Auth(SaslMechanism.ScramSha1, msg);

            var ret1 = await xmppClient.SendAsync<Failure, Challenge>(authMessage, cancellationToken).ConfigureAwait(false);

            if (ret1 is Challenge)
            {
                var resp = GenerateFinalMessage(ret1 as Challenge, scramHelper, password);
                var ret2 = await xmppClient.SendAsync<Failure, Success>(resp, cancellationToken).ConfigureAwait(false);

                return ret2;
            }

            return ret1;
        }

        private Response GenerateFinalMessage(Challenge ch, ScramHelper scramHelper, string password)
        {
            byte[] b = ch.Bytes;
            string firstServerMessage = Encoding.UTF8.GetString(b, 0, b.Length);
            string clientFinalMessage = scramHelper.GenerateFinalClientMessage(firstServerMessage, password);
            return new Response(ToB64String(clientFinalMessage));
        }

        private static string ToB64String(string sin)
        {
            byte[] msg = Encoding.UTF8.GetBytes(sin);
            return Convert.ToBase64String(msg, 0, msg.Length);
        }

        private string FromB64String(string sin)
        {
            var b = Convert.FromBase64String(sin);
            return Encoding.UTF8.GetString(b, 0, b.Length);
        }
    }
}
