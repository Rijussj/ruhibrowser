using System;
using System.Collections.Generic;

namespace RuhiBrowser.Models
{
    public class BrowserSettings
    {
        public string SearchEngine { get; set; } = "Google";
        public bool AdBlockEnabled { get; set; } = true;
        public string Theme { get; set; } = "System";
        public string DownloadFolder { get; set; } = string.Empty;

        public string HomepageMode { get; set; } = "Home"; // "Home", "HomeSimple", "HomeNews", "HomeStandard", "HomeProductive", "HomeWallpaper", "Extension", "Google", "Blank", "Custom"
        public string HomeScreenStyle { get; set; } = "News"; // "Simple", "News", "Standard", "Productive", "Wallpaper"
        public string CustomHomepageUrl { get; set; } = string.Empty;
        public string ActiveExtensionNewTabUrl { get; set; } = string.Empty;
        public string ActiveExtensionNewTabName { get; set; } = string.Empty;
        public string QuickNoteText { get; set; } = string.Empty;
        public string DailyGoalText { get; set; } = string.Empty;
        public bool DailyGoalCompleted { get; set; } = false;

        // AI Assistant Settings
        public string AiApiKey { get; set; } = string.Empty;
        public string AiModelEndpoint { get; set; } = "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.2";

        // WireGuard VPN Settings
        public string VpnServerAddress { get; set; } = string.Empty;
        public int VpnServerPort { get; set; } = 51820;
        public string VpnClientAddress { get; set; } = "10.0.0.2/32";
        public string VpnDns { get; set; } = "1.1.1.1, 1.0.0.1";
        public string VpnPublicKey { get; set; } = string.Empty;
        public string VpnPrivateKey { get; set; } = string.Empty;

        // Memory Saver / Tab Snoozing Settings
        public bool MemorySaverEnabled { get; set; } = true;
        public int InactiveTabSleepMinutes { get; set; } = 20;

        // Cloud Sync Settings (Cross-Device Sync)
        public bool SyncEnabled { get; set; } = false;
        public string SyncDisplayName { get; set; } = string.Empty;
        public string SyncEmail { get; set; } = string.Empty;
        public string SyncUserId { get; set; } = string.Empty;
        public string SyncAccessToken { get; set; } = string.Empty;
        public string SyncRefreshToken { get; set; } = string.Empty;
        public string SyncDeviceId { get; set; } = string.Empty;
        public string SyncDeviceName { get; set; } = string.Empty;
        public DateTime? LastSyncTime { get; set; }
        public DateTime? LastHistoryClearedAt { get; set; }
        public List<string> DeletedHistoryUrls { get; set; } = new();
    }
}
