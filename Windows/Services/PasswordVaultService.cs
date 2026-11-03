using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RuhiBrowser.Models;

namespace RuhiBrowser.Services
{
    public static class PasswordVaultService
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("RuhiBrowser_VaultEntropy_2026_SecureStorage");

        private static readonly string VaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RuhiBrowser",
            "passwords_vault.json"
        );

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true
        };

        public static string EncryptPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            }
        }

        public static string DecryptPassword(string cipherTextBase64)
        {
            if (string.IsNullOrEmpty(cipherTextBase64)) return string.Empty;
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                try
                {
                    byte[] fallbackBytes = Convert.FromBase64String(cipherTextBase64);
                    return Encoding.UTF8.GetString(fallbackBytes);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static List<SavedCredential> LoadCredentials()
        {
            try
            {
                if (File.Exists(VaultPath))
                {
                    var json = File.ReadAllText(VaultPath);
                    var list = JsonSerializer.Deserialize<List<SavedCredential>>(json, JsonOpts);
                    return list ?? new List<SavedCredential>();
                }
            }
            catch { }
            return new List<SavedCredential>();
        }

        public static void SaveCredentials(List<SavedCredential> list)
        {
            try
            {
                var dir = Path.GetDirectoryName(VaultPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonSerializer.Serialize(list, JsonOpts);
                File.WriteAllText(VaultPath, json);
            }
            catch { }
        }

        public static void AddOrUpdate(string url, string title, string username, string password)
        {
            var list = LoadCredentials();
            var existing = list.Find(c => string.Equals(c.WebsiteUrl, url, StringComparison.OrdinalIgnoreCase)
                                       && string.Equals(c.Username, username, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.EncryptedPassword = EncryptPassword(password);
                existing.WebsiteTitle = string.IsNullOrWhiteSpace(title) ? existing.WebsiteTitle : title;
                existing.LastUsedAt = DateTime.Now;
            }
            else
            {
                list.Add(new SavedCredential
                {
                    WebsiteUrl = url,
                    WebsiteTitle = string.IsNullOrWhiteSpace(title) ? GetDomainFromUrl(url) : title,
                    Username = username,
                    EncryptedPassword = EncryptPassword(password),
                    CreatedAt = DateTime.Now,
                    LastUsedAt = DateTime.Now
                });
            }

            SaveCredentials(list);
        }

        public static bool Delete(string id)
        {
            var list = LoadCredentials();
            int count = list.RemoveAll(c => c.Id == id);
            if (count > 0)
            {
                SaveCredentials(list);
                return true;
            }
            return false;
        }

        public static string GetDomainFromUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return url;
            }
        }
    }
}
