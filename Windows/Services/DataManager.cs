using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RuhiBrowser.Models;

namespace RuhiBrowser.Services
{
    public class DataManager
    {
        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RuhiBrowser"
        );

        private static readonly string BookmarksPath = Path.Combine(AppDataDir, "bookmarks.json");
        private static readonly string HistoryPath = Path.Combine(AppDataDir, "history.json");
        private static readonly string ShortcutsPath = Path.Combine(AppDataDir, "shortcuts.json");
        private static readonly string SettingsPath = Path.Combine(AppDataDir, "settings.json");
        private static readonly string TabGroupsPath = Path.Combine(AppDataDir, "tab_groups.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        static DataManager()
        {
            if (!Directory.Exists(AppDataDir))
            {
                Directory.CreateDirectory(AppDataDir);
            }
        }

        // --- Settings ---
        public static BrowserSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<BrowserSettings>(json, JsonOptions) ?? new BrowserSettings();
                }
            }
            catch { }
            return new BrowserSettings();
        }

        public static void SaveSettings(BrowserSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, JsonOptions);
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }

        // --- Shortcuts ---
        public static List<Shortcut> LoadShortcuts()
        {
            try
            {
                if (File.Exists(ShortcutsPath))
                {
                    var json = File.ReadAllText(ShortcutsPath);
                    var list = JsonSerializer.Deserialize<List<Shortcut>>(json, JsonOptions);
                    if (list != null && list.Count > 0)
                    {
                        return list;
                    }
                }
            }
            catch { }

            return GetDefaultShortcuts();
        }

        public static void SaveShortcuts(List<Shortcut> shortcuts)
        {
            try
            {
                var json = JsonSerializer.Serialize(shortcuts, JsonOptions);
                File.WriteAllText(ShortcutsPath, json);
            }
            catch { }
        }

        public static List<Shortcut> GetDefaultShortcuts()
        {
            return new List<Shortcut>
            {
                new("Google", "https://www.google.com"),
                new("TaazaWire", "https://taazawire.in"),
                new("YouTube", "https://www.youtube.com"),
                new("Wikipedia", "https://www.wikipedia.org"),
                new("GitHub", "https://www.github.com"),
                new("Reddit", "https://www.reddit.com")
            };
        }

        // --- Tab Groups ---
        public static List<TabGroup> LoadTabGroups()
        {
            try
            {
                if (File.Exists(TabGroupsPath))
                {
                    var json = File.ReadAllText(TabGroupsPath);
                    return JsonSerializer.Deserialize<List<TabGroup>>(json, JsonOptions) ?? new List<TabGroup>();
                }
            }
            catch { }
            return new List<TabGroup>();
        }

        public static void SaveTabGroups(List<TabGroup> groups)
        {
            try
            {
                var json = JsonSerializer.Serialize(groups, JsonOptions);
                File.WriteAllText(TabGroupsPath, json);
            }
            catch { }
        }

        // --- Bookmarks ---
        public static List<Bookmark> LoadBookmarks()
        {
            try
            {
                if (File.Exists(BookmarksPath))
                {
                    var json = File.ReadAllText(BookmarksPath);
                    return JsonSerializer.Deserialize<List<Bookmark>>(json, JsonOptions) ?? new List<Bookmark>();
                }
            }
            catch { }
            return new List<Bookmark>();
        }

        public static void SaveBookmarks(List<Bookmark> bookmarks)
        {
            try
            {
                var json = JsonSerializer.Serialize(bookmarks, JsonOptions);
                File.WriteAllText(BookmarksPath, json);
            }
            catch { }
        }

        // --- History ---
        public static List<HistoryItem> LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    var json = File.ReadAllText(HistoryPath);
                    return JsonSerializer.Deserialize<List<HistoryItem>>(json, JsonOptions) ?? new List<HistoryItem>();
                }
            }
            catch { }
            return new List<HistoryItem>();
        }

        public static void SaveHistory(List<HistoryItem> history)
        {
            try
            {
                var json = JsonSerializer.Serialize(history, JsonOptions);
                File.WriteAllText(HistoryPath, json);
            }
            catch { }
        }

        public static void RecordVisit(string title, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("edge://") || url.StartsWith("about:"))
            {
                return;
            }

            var list = LoadHistory();
            var existing = list.Find(h => string.Equals(h.Url, url, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (!string.IsNullOrWhiteSpace(title))
                {
                    existing.Title = title;
                }
                existing.VisitedAt = DateTime.Now;
                existing.VisitCount++;
            }
            else
            {
                list.Insert(0, new HistoryItem(string.IsNullOrWhiteSpace(title) ? url : title, url));
            }

            // Keep top 1000 history entries
            if (list.Count > 1000)
            {
                list = list.GetRange(0, 1000);
            }

            SaveHistory(list);
        }

        public static void ClearHistory()
        {
            try
            {
                if (File.Exists(HistoryPath)) File.Delete(HistoryPath);
            }
            catch { }
        }

        public static void ClearBookmarks()
        {
            try
            {
                if (File.Exists(BookmarksPath)) File.Delete(BookmarksPath);
            }
            catch { }
        }

        public static void ClearAllData()
        {
            ClearHistory();
            ClearBookmarks();
        }
    }
}
