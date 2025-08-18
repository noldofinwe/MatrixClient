using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Logging;

namespace XmppApi
{
    /// <summary>
    /// Represents on curated server provider.
    /// </summary>
    public class Provider
    {
        public string jid;
        public bool company;
        public bool passwordReset;
        public int ratingComplianceTester;
        public string ratingImObservatoryCtS;
        public string ratingImObservatoryStS;
        public int maxFileUploadSize;
        public int fileUploadStorageTime;
        public int mamStorageTime;
        public bool professionalHosting;
        public bool free;
        public string legalNotice;
        public DateTime onlineSince;
        public string registrationWebPage;
    }

    /// <summary>
    /// A curated list of XMPP server providers based on:
    /// https://invent.kde.org/melvo/xmpp-providers/-/tree/master
    /// </summary>
    public class XMPPProviders
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\

        #region --Attributes--

        public static readonly XMPPProviders INSTANCE = new XMPPProviders();

        private static readonly string[] ADDITIONAL_PROVIDERS_SIMPLE = new string[]
        {
            "xmpp.uwpx.org",
            "dukgo.com",
            "mailbox.org",
            "kein.ninja"
        };

        public string[] providersASimple;
        public string[] providersBSimple;
        public string[] providersCSimple;

        public Provider[] providersA;
        public Provider[] providersB;
        public Provider[] providersC;

        /// <summary>
        /// Path to the simple list of A-level providers.
        /// </summary>
        private const string A_LEVEL_SIMPLE_PATH = @"Resources/Providers/providers-A-simple.json";

        /// <summary>
        /// Path to the list of A-level providers.
        /// </summary>
        private const string A_LEVEL_PATH = @"Resources/Providers/providers-A.json";

        /// <summary>
        /// Path to the simple list of B-level providers.
        /// </summary>
        private const string B_LEVEL_SIMPLE_PATH = @"Resources/Providers/providers-B-simple.json";

        /// <summary>
        /// Path to the list of B-level providers.
        /// </summary>
        private const string B_LEVEL_PATH = @"Resources/Providers/providers-B.json";

        /// <summary>
        /// Path to the simple list of C-level providers.
        /// </summary>
        private const string C_LEVEL_SIMPLE_PATH = @"Resources/Providers/providers-C-simple.json";

        /// <summary>
        /// Path to the list of C-level providers.
        /// </summary>
        private const string C_LEVEL_PATH = @"Resources/Providers/providers-C.json";

        #endregion

        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\

        #region --Constructors--

        private XMPPProviders()
        {
        }

        #endregion

        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\

        #region --Set-, Get- Methods--

        #endregion

        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\

        #region --Misc Methods (Public)--

        /// <summary>
        /// Initialized the curated list of server providers and loads all them from the files.
        /// </summary>
        public async Task initAsync()
        {
            await loadProvidersSimpleAsync();
            await loadProvidersAsync();
        }

        #endregion

        #region --Misc Methods (Private)--

        private async Task loadProvidersSimpleAsync()
        {
            providersASimple = await loadProvidersSimpleFromFileAsync(A_LEVEL_SIMPLE_PATH);
            providersBSimple = await loadProvidersSimpleFromFileAsync(B_LEVEL_SIMPLE_PATH);
            providersCSimple = await loadProvidersSimpleFromFileAsync(C_LEVEL_SIMPLE_PATH);
        }

        private async Task loadProvidersAsync()
        {
            providersA = await LoadProvidersFromFileAsync(A_LEVEL_PATH);
            providersB = await LoadProvidersFromFileAsync(B_LEVEL_PATH);
            providersC = await LoadProvidersFromFileAsync(C_LEVEL_PATH);
        }

        private async Task<string[]> loadProvidersSimpleFromFileAsync(string path)
        {
            Logger.Info($"Loading simple providers from: {path}");
            List<string> providers;
            try
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = await r.ReadToEndAsync();
                    providers = JsonSerializer.Deserialize<List<string>>(json);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load simple providers from file '{path}' with:", e);
                return ADDITIONAL_PROVIDERS_SIMPLE;
            }

            Logger.Info($"Loaded {providers.Count} simple providers from {path} successfully.");
            providers.AddRange(ADDITIONAL_PROVIDERS_SIMPLE);
            return providers.ToArray();
        }

        private List<Provider> ParseProviders(string json)
        {
            var result = new List<Provider>();
            using var doc = JsonDocument.Parse(json);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                try
                {
                    var provider = new Provider
                    {
                        jid = element.GetProperty("jid").GetString(),
                        company = element.GetProperty("company").GetBoolean(),
                        fileUploadStorageTime = element.GetProperty("maximumHttpFileUploadStorageTime").GetInt32(),
                        free = element.GetProperty("freeOfCharge").GetBoolean(),
                        legalNotice =
                            element.TryGetProperty("legalNotice", out var ln) && ln.TryGetProperty("en", out var en)
                                ? en.GetString()
                                : null,
                        mamStorageTime = element.GetProperty("maximumMessageArchiveManagementStorageTime").GetInt32(),
                        maxFileUploadSize = element.GetProperty("maximumHttpFileUploadFileSize").GetInt32(),
                        onlineSince = DateTime.Parse(element.GetProperty("onlineSince").GetString()),
                        passwordReset = element.TryGetProperty("passwordReset", out _) ? true : false,
                        professionalHosting = element.GetProperty("professionalHosting").GetBoolean(),
                        ratingComplianceTester = element.GetProperty("ratingXmppComplianceTester").GetInt32(),
                        ratingImObservatoryCtS = element.GetProperty("ratingImObservatoryClientToServer").GetString(),
                        ratingImObservatoryStS = element.GetProperty("ratingImObservatoryServerToServer").GetString(),
                        registrationWebPage = element.TryGetProperty("registrationWebPage", out var reg) &&
                                              reg.TryGetProperty("en", out var regEn)
                            ? regEn.GetString()
                            : "https://" + element.GetProperty("jid").GetString()
                    };
                    result.Add(provider);
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to parse provider with: ", e);
                }
            }

            return result;
        }


        private async Task<Provider[]> LoadProvidersFromFileAsync(string path)
        {
            Logger.Info($"Loading providers from: {path}");

            try
            {
                using FileStream stream = File.OpenRead(path);
                var providers = await JsonSerializer.DeserializeAsync<List<Provider>>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (providers == null)
                {
                    Logger.Warn($"No providers found in file: {path}");
                    return Array.Empty<Provider>();
                }

                Logger.Info($"Loaded {providers.Count} providers from {path} successfully.");
                return providers.ToArray();
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load providers from file '{path}' with:", e);
                return Array.Empty<Provider>();
            }
        }


        #endregion

        #region --Misc Methods (Protected)--

        #endregion

        //--------------------------------------------------------Events:---------------------------------------------------------------------\\

        #region --Events--

        #endregion
    }
}