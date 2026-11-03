using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace RuhiBrowser.Services
{
    public static class UrlResolver
    {
        private const string DefaultSearchTemplate = "https://www.google.com/search?q={0}";

        private static readonly Regex DomainRegex = new(
            @"^([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(:[0-9]{1,5})?(/.*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex LocalhostRegex = new(
            @"^localhost(:[0-9]{1,5})?(/.*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex Ipv4Regex = new(
            @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[0-9]{1,5})?(/.*)?$",
            RegexOptions.Compiled
        );

        private static readonly Regex WindowsDriveRegex = new(
            @"^[a-zA-Z]:[\\/]",
            RegexOptions.Compiled
        );

        public static string Resolve(string input, string? searchEngine = "Google")
        {
            var trimmed = input?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return string.Empty;
            }

            // Strip surrounding quotes if copied with Windows "Copy as path"
            var clean = trimmed.Trim('"', '\'').Trim();

            // 1. Check for file URI scheme (e.g. file:///C:/...)
            if (clean.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                if (Uri.TryCreate(clean, UriKind.Absolute, out var fileUri))
                {
                    return fileUri.AbsoluteUri;
                }
                return clean;
            }

            // 2. Check for local Windows file/folder paths (e.g. C:\..., D:/..., \\server\share...)
            if (WindowsDriveRegex.IsMatch(clean) || clean.StartsWith(@"\\"))
            {
                try
                {
                    return new Uri(Path.GetFullPath(clean)).AbsoluteUri;
                }
                catch
                {
                    return "file:///" + clean.Replace('\\', '/');
                }
            }

            // 3. Check if an exact local file or directory exists
            if (File.Exists(clean) || Directory.Exists(clean))
            {
                try
                {
                    return new Uri(Path.GetFullPath(clean)).AbsoluteUri;
                }
                catch { }
            }

            var lower = clean.ToLowerInvariant();
            if (lower == "ruhi://store" || lower == "ruhi:store" || lower == "chrome://store" || lower == "store" || lower == "ruhi://addons" || lower == "ruhi:addons" || lower == "ruhi extension store" || lower == "extension store")
            {
                return GetStoreUrl();
            }

            if (lower.StartsWith("ruhi://extension") || lower.StartsWith("ruhi:extension") ||
                lower.StartsWith("chrome://extension") || lower.StartsWith("edge://extension"))
            {
                return "ruhi://extensions";
            }

            if (lower.StartsWith("javascript:") || lower.StartsWith("data:") || lower.StartsWith("about:") ||
                lower.StartsWith("chrome-extension://") || lower.StartsWith("edge-extension://"))
            {
                return clean;
            }

            if (lower.StartsWith("http://") || lower.StartsWith("https://"))
            {
                return clean;
            }

            if (clean.Contains(" "))
            {
                return BuildSearchUrl(clean, searchEngine);
            }

            if (DomainRegex.IsMatch(clean) || LocalhostRegex.IsMatch(clean) || Ipv4Regex.IsMatch(clean))
            {
                return "https://" + clean;
            }

            return BuildSearchUrl(clean, searchEngine);
        }

        public static string BuildSearchUrl(string query, string? searchEngine = "Google")
        {
            var encoded = WebUtility.UrlEncode(query);
            var engineKey = (searchEngine ?? "Google").Trim().ToLowerInvariant();
            var template = engineKey switch
            {
                "duckduckgo" => "https://duckduckgo.com/?q={0}",
                "bing" => "https://www.bing.com/search?q={0}",
                "ecosia" => "https://www.ecosia.org/search?q={0}",
                "yahoo" => "https://search.yahoo.com/search?p={0}",
                "brave" => "https://search.brave.com/search?q={0}",
                _ => DefaultSearchTemplate
            };
            return string.Format(template, encoded);
        }

        public static bool IsHttps(string url)
        {
            return url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetStoreHtmlPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string localPath = Path.Combine(baseDir, "store", "index.html");
            if (File.Exists(localPath)) return localPath;

            string sourcePath = @"D:\RUHI Apps\RUHI Browser\Windows\store\index.html";
            if (File.Exists(sourcePath)) return sourcePath;

            return localPath;
        }

        public static string GetStoreUrl()
        {
            return "https://addonruhi.netlify.app/";
        }
    }
}
