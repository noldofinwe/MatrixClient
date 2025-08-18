using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;
using XmppApi.Network;

namespace Storage
{
  
    public static class Vault
    {
        private const string VAULT_NAME_PREFIX = "XMPP_LOGIN_DATA_VAULT_3_";

        public static async Task<PasswordCredential?> GetPasswordCredentialForAccount(XMPPAccount account)
        {
            string key = VAULT_NAME_PREFIX + account.getBareJid();
            try
            {
                string password = await SecureStorage.Default.GetAsync(key);
                
                account.user.password = password;
                return string.IsNullOrEmpty(password) 
                    ? null 
                    : new PasswordCredential(key, account.user.localPart, password);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<PasswordCredential>> GetAll(List<XMPPAccount> accounts)
        {
            var list = new List<PasswordCredential>();
            foreach (var account in accounts)
            {
                var cred = await GetPasswordCredentialForAccount(account);
                if (cred != null)
                    list.Add(cred);
            }
            return list;
        }

        public static async Task StorePassword(XMPPAccount account)
        {
            await DeletePassword(account);
            if (!string.IsNullOrEmpty(account.user.password))
            {
                string key = VAULT_NAME_PREFIX + account.getBareJid();
                await SecureStorage.Default.SetAsync(key, account.user.password);
            }
        }

        public static async Task DeletePassword(XMPPAccount account)
        {
            string key = VAULT_NAME_PREFIX + account.getBareJid();
            SecureStorage.Default.Remove(key);
        }

        public static void DeleteAllVaults(List<XMPPAccount> accounts)
        {
            foreach (var account in accounts)
            {
                string key = VAULT_NAME_PREFIX + account.getBareJid();
                SecureStorage.Default.Remove(key);
            }
        }
    }
    
    public class PasswordCredential
    {
        public string Resource { get; }
        public string UserName { get; }
        public string Password { get; private set; }

        public PasswordCredential(string resource, string username, string password)
        {
            Resource = resource;
            UserName = username;
            Password = password;
        }

        public void RetrievePassword()
        {
            // Nothing to do here in our custom version
        }
    }

}
