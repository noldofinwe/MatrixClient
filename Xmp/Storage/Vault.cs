using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using XmppApi.Network;

namespace Storage
{
    public static class Vault
    {
        private const string VAULT_NAME_PREFIX = "XMPP_LOGIN_DATA_VAULT_3_";
        private const string DB_PATH = "vault.db";

        static Vault()
        {
            using var connection = new SqliteConnection($"Data Source={DB_PATH}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS PasswordVault (
                    Key TEXT PRIMARY KEY,
                    UserName TEXT,
                    Password TEXT
                );";
            command.ExecuteNonQuery();
        }

        public static async Task<PasswordCredential?> GetPasswordCredentialForAccount(XMPPAccount account)
        {
            string key = VAULT_NAME_PREFIX + account.getBareJid();
            using var connection = new SqliteConnection($"Data Source={DB_PATH}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT UserName, Password FROM PasswordVault WHERE Key = $key";
            command.Parameters.AddWithValue("$key", key);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string userName = reader.GetString(0);
                string password = reader.GetString(1);
                account.user.password = password;
                return new PasswordCredential(key, userName, password);
            }
            return null;
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
                using var connection = new SqliteConnection($"Data Source={DB_PATH}");
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"INSERT INTO PasswordVault (Key, UserName, Password)
                      VALUES ($key, $userName, $password);";
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$userName", account.user.localPart);
                command.Parameters.AddWithValue("$password", account.user.password);
                await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task DeletePassword(XMPPAccount account)
        {
            string key = VAULT_NAME_PREFIX + account.getBareJid();
            using var connection = new SqliteConnection($"Data Source={DB_PATH}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM PasswordVault WHERE Key = $key";
            command.Parameters.AddWithValue("$key", key);
            await command.ExecuteNonQueryAsync();
        }

        public static async Task DeleteAllVaults(List<XMPPAccount> accounts)
        {
            using var connection = new SqliteConnection($"Data Source={DB_PATH}");
            await connection.OpenAsync();
            foreach (var account in accounts)
            {
                string key = VAULT_NAME_PREFIX + account.getBareJid();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM PasswordVault WHERE Key = $key";
                command.Parameters.AddWithValue("$key", key);
                await command.ExecuteNonQueryAsync();
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
