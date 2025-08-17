using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Logging;
using Omemo.Keys;
using Omemo;
using Shared;
using XmppApi.Network.XML.Messages.XEP_0384;

namespace XmppApi.XmppUri
{
    public static class UriUtils
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\

        #region --Attributes--

        public const string URI_SCHEME = "xmpp";

        #endregion

        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\

        #region --Constructors--

        #endregion

        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\

        #region --Set-, Get- Methods--

        /// <summary>
        /// Combines the user info and host to a bare JID.
        /// </summary>
        /// <param name="uri">The <see cref="System.Uri"/> that should be used.</param>
        /// <returns>A bare JID or null if the host or user info is empty, white space only or null.</returns>
        public static string getBareJidFromUri(Uri uri)
        {
            if (string.IsNullOrWhiteSpace(uri.Host) || string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(uri.UserInfo);
            sb.Append('@');
            sb.Append(uri.Host);
            return sb.ToString();
        }

        #endregion

        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\

        #region --Misc Methods (Public)--

        /// <summary>
        /// Builds a <see cref="System.Uri"/> object from the given <paramref name="bareJid"/> and <paramref name="queryPairs"/>.
        /// Uses <see cref="URI_SCHEME"/> as scheme.
        /// </summary>
        /// <param name="bareJid">The bare JID that should be used as host.</param>
        /// <param name="queryPairs">All queries.</param>
        /// <returns>The <see cref="System.Uri"/> object representing the given attributes.</returns>
        public static Uri buildUri(string bareJid, Dictionary<string, string> queryPairs)
        {
            string query = string.Join("&",
                queryPairs.Keys.Select(key =>
                    !string.IsNullOrWhiteSpace(queryPairs[key])
                        ? string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(queryPairs[key]))
                        : WebUtility.UrlEncode(key)));

            UriBuilder builder = new UriBuilder
            {
                Scheme = "xmpp",
                Host = bareJid,
                Query = query
            };

            return builder.Uri;
        }

        public static string toXmppUriString(Uri uri)
        {
            StringBuilder sb = new StringBuilder(uri.Scheme);
            sb.Append(':');
            sb.Append(uri.Host);
            sb.Append(uri.Query);
            return sb.ToString();
        }

        /// <summary>
        /// Builds a <see cref="System.Uri"/> object from the given <paramref name="queryPairs"/>.
        /// Uses <see cref="URI_SCHEME"/> as scheme.
        /// </summary>
        /// <param name="queryPairs">All queries.</param>
        /// <returns>The <see cref="System.Uri"/> object representing the given attributes.</returns>
        public static Uri buildUri(Dictionary<string, string> queryPairs)
        {
            return buildUri("", queryPairs);
        }

        public static NameValueCollection ParseUriQueryName(Uri uri)
        {
            try
            {
                return HttpUtility.ParseQueryString(uri.Query);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to decode XMPP Uri: " + uri.ToString(), e);
                return null;
            }
        }
        
        /// <summary>
        /// Parses the given <see cref="System.Uri"/> query to a <see cref="WwwFormUrlDecoder"/> object.
        /// </summary>
        /// <param name="uri">The <see cref="System.Uri"/> thats query part should be parsed.</param>
        /// <returns>A list of attributes and values font in the given <see cref="System.Uri"/>.</returns>
        public static Dictionary<string, string> ParseUriQuery(Uri uri)
        {
            try
            {
                var query = uri.Query.TrimStart('?');
                var result = new Dictionary<string, string>();

                foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = pair.Split('=', 2);
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
                    result[key] = value;
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error("Failed to decode XMPP Uri: " + uri.ToString(), e);
                return new Dictionary<string, string>();
            }
        }


        /// <summary>
        /// [WIP]<para/>
        /// Parses XMPP IRIs and URIs based on RFC 5122 and returns the result.
        /// </summary>
        /// <param name="uri">The URI or IRI that should get parsed.</param>
        /// <returns>The URI or IRI result or null if an error occurred.</returns>
        public static IUriAction Parse(Uri uri)
        {
            if (string.IsNullOrEmpty(uri?.OriginalString))
            {
                return null;
            }

            if (!string.Equals(uri.Scheme, "xmpp", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warn("Failed to parse XMPP URI. No 'xmpp' scheme.");
                return null;
            }

            string tmp = uri.OriginalString.Substring(5); // remove 'xmpp:'

            // Authority parsing (optional, not used later)
            string authority = null;
            if (tmp.StartsWith("//"))
            {
                tmp = tmp.Substring(2);
                int authEnd = tmp.IndexOfAny(new[] { '/', '?', '#' });
                if (authEnd >= 0)
                {
                    authority = tmp.Substring(0, authEnd);
                    tmp = tmp.Substring(authEnd + 1);
                }
            }

            var queryParams = ParseUriQuery(uri);
            if (queryParams == null)
            {
                return null;
            }

            if (string.Equals(uri.AbsolutePath, "iot-register", StringComparison.OrdinalIgnoreCase))
            {
                if (!queryParams.TryGetValue("mac", out var mac) || string.IsNullOrEmpty(mac))
                {
                    Logger.Error("None or invalid IoT MAC address: " + uri.OriginalString);
                    return null;
                }

                if (!queryParams.TryGetValue("algo", out var algo) || string.IsNullOrEmpty(algo))
                {
                    Logger.Error("None or invalid IoT key algorithm: " + uri.OriginalString);
                    return null;
                }

                if (!queryParams.TryGetValue("key", out var key) || string.IsNullOrEmpty(key))
                {
                    Logger.Error("None or invalid IoT key: " + uri.OriginalString);
                    return null;
                }

                return new RegisterIoTUriAction(mac, algo, key);
            }
            else
            {
                var omemoEntry = queryParams.FirstOrDefault(kvp => kvp.Key.StartsWith("omemo-sid-"));
                if (!string.IsNullOrEmpty(omemoEntry.Value))
                {
                    try
                    {
                        byte[] fingerprintBytes = SharedUtils.HexStringToByteArray(omemoEntry.Value);
                        var pubKey = new ECPubKeyModel(fingerprintBytes);

                        if (uint.TryParse(omemoEntry.Key.Replace("omemo-sid-", "").Trim(), out uint deviceId))
                        {
                            var address = new OmemoProtocolAddress(uri.LocalPath, deviceId);
                            return new OmemoFingerprintUriAction(new OmemoFingerprint(pubKey, address));
                        }
                        else
                        {
                            Logger.Warn("Failed to parse XMPP URI. Invalid device ID: " + omemoEntry.Key);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to parse XMPP URI. Parsing fingerprint failed: " + omemoEntry.Value, e);
                    }
                }
            }

            return null;
        }

        #endregion

        #region --Misc Methods (Private)--

        #endregion

        #region --Misc Methods (Protected)--

        #endregion

        //--------------------------------------------------------Events:---------------------------------------------------------------------\\

        #region --Events--

        #endregion
    }
}