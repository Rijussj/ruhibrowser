using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly BrowserSettings _settings;
        private bool _isTokenVisible = false;
        private static readonly HttpClient _testClient = new() { Timeout = TimeSpan.FromSeconds(10) };

        public Action<bool, bool, bool, bool>? ClearDataHandler { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();

            _settings = DataManager.LoadSettings();

            // Apply Theme immediately on launch
            ApplyTheme(_settings.Theme);

            // 1. Default Browser
            UpdateDefaultBrowserStatus();

            // 2. Homepage / Startup Mode
            InitHomepageMode();

            // 3. Search Engine
            InitSearchEngine();

            // 4. Theme
            InitTheme();

            // 5. AdBlock
            CbAdBlock.IsChecked = _settings.AdBlockEnabled;
            UpdateAdBlockLabel(_settings.AdBlockEnabled);

            // 5b. Memory Saver
            CbMemorySaver.IsChecked = _settings.MemorySaverEnabled;

            // 6. Downloads
            var defaultDownloads = string.IsNullOrWhiteSpace(_settings.DownloadFolder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
                : _settings.DownloadFolder;
            TbDownloadsPath.Text = defaultDownloads;

            // 7. AI Settings
            PbAiToken.Password = _settings.AiApiKey;
            TbAiTokenVisible.Text = _settings.AiApiKey;
            TbAiEndpoint.Text = string.IsNullOrWhiteSpace(_settings.AiModelEndpoint)
                ? "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2"
                : _settings.AiModelEndpoint;

            // 8. VPN Summary
            UpdateVpnSummary();
        }

        private void UpdateDefaultBrowserStatus()
        {
            bool isDefault = DefaultBrowserHelper.IsDefaultBrowser();
            if (isDefault)
            {
                TbDefaultBrowserStatus.Text = "RUHI Browser is currently your default browser";
                TbDefaultBrowserStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
            }
            else
            {
                TbDefaultBrowserStatus.Text = "RUHI Browser is not currently set as default browser";
                TbDefaultBrowserStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }
        }

        private void InitHomepageMode()
        {
            if (CbiExtensionMode != null)
            {
                if (!string.IsNullOrWhiteSpace(_settings.ActiveExtensionNewTabName))
                {
                    CbiExtensionMode.Content = $"Extension New Tab ({_settings.ActiveExtensionNewTabName})";
                    CbiExtensionMode.IsEnabled = true;
                }
                else
                {
                    CbiExtensionMode.Content = "Extension New Tab (None installed)";
                    CbiExtensionMode.IsEnabled = false;
                }
            }

            var mode = _settings.HomepageMode ?? "Home";
            switch (mode.ToLowerInvariant())
            {
                case "extension":
                    CbHomepageMode.SelectedIndex = 1;
                    break;
                case "google":
                    CbHomepageMode.SelectedIndex = 2;
                    break;
                case "blank":
                    CbHomepageMode.SelectedIndex = 3;
                    break;
                case "custom":
                    CbHomepageMode.SelectedIndex = 4;
                    TbCustomHomepageUrl.Visibility = Visibility.Visible;
                    break;
                default:
                    CbHomepageMode.SelectedIndex = 0;
                    break;
            }
            TbCustomHomepageUrl.Text = string.IsNullOrWhiteSpace(_settings.CustomHomepageUrl) ? "https://" : _settings.CustomHomepageUrl;

            if (GridHomeScreenStyle != null)
            {
                GridHomeScreenStyle.Visibility = (CbHomepageMode.SelectedIndex == 0) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (CbHomeScreenStyle != null)
            {
                var style = _settings.HomeScreenStyle ?? "Simple";
                switch (style.ToLowerInvariant())
                {
                    case "simple":
                        CbHomeScreenStyle.SelectedIndex = 0;
                        break;
                    case "news":
                        CbHomeScreenStyle.SelectedIndex = 1;
                        break;
                    case "standard":
                        CbHomeScreenStyle.SelectedIndex = 2;
                        break;
                    case "productive":
                        CbHomeScreenStyle.SelectedIndex = 3;
                        break;
                    case "wallpaper":
                        CbHomeScreenStyle.SelectedIndex = 4;
                        break;
                    default:
                        CbHomeScreenStyle.SelectedIndex = 0;
                        break;
                }
            }
        }

        private void OnHomepageModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TbCustomHomepageUrl == null) return;

            TbCustomHomepageUrl.Visibility = (CbHomepageMode.SelectedIndex == 4) ? Visibility.Visible : Visibility.Collapsed;
            if (GridHomeScreenStyle != null)
            {
                GridHomeScreenStyle.Visibility = (CbHomepageMode.SelectedIndex == 0) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OnHomeScreenStyleChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TbHomeScreenStyleDesc == null || CbHomeScreenStyle == null) return;
            switch (CbHomeScreenStyle.SelectedIndex)
            {
                case 0:
                    TbHomeScreenStyleDesc.Text = "⚡ Simple: Fast & clean with search bar and shortcuts only.";
                    break;
                case 1:
                    TbHomeScreenStyleDesc.Text = "📰 With News: Search, shortcuts, and live trending news headlines.";
                    break;
                case 2:
                    TbHomeScreenStyleDesc.Text = "🧭 Standard: Shortcuts and recent browsing history.";
                    break;
                case 3:
                    TbHomeScreenStyleDesc.Text = "⏱ Productive: Digital clock, focus timer, daily goals, scratchpad & tools.";
                    break;
                case 4:
                    TbHomeScreenStyleDesc.Text = "🌄 Scenic Wallpaper: Atmospheric aesthetic gradient backdrop.";
                    break;
            }
        }

        private void InitSearchEngine()
        {
            foreach (ComboBoxItem item in CbSearchEngine.Items)
            {
                if (string.Equals(item.Content.ToString(), _settings.SearchEngine, StringComparison.OrdinalIgnoreCase))
                {
                    CbSearchEngine.SelectedItem = item;
                    break;
                }
            }
            if (CbSearchEngine.SelectedItem == null) CbSearchEngine.SelectedIndex = 0;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private void InitTheme()
        {
            foreach (ComboBoxItem item in CbTheme.Items)
            {
                if (string.Equals(item.Content.ToString(), _settings.Theme, StringComparison.OrdinalIgnoreCase))
                {
                    CbTheme.SelectedItem = item;
                    break;
                }
            }
            if (CbTheme.SelectedItem == null) CbTheme.SelectedIndex = 0;
        }

        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbTheme?.SelectedItem is ComboBoxItem item)
            {
                ApplyTheme(item.Content?.ToString() ?? "Dark");
            }
        }

        public void ApplyTheme(string? theme)
        {
            bool isDark = true;
            if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                isDark = false;
            }
            else if (string.Equals(theme, "System", StringComparison.OrdinalIgnoreCase))
            {
                isDark = !IsWindowsInLightMode();
            }

            Resources["WindowBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F1F5F9"));
            Resources["HeaderFooterBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#28292C" : "#FFFFFF"));
            Resources["BorderBrushColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1"));
            Resources["CardBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF"));
            Resources["TextPrimaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            Resources["TextSecondaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));
            Resources["SectionHeadingBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#38BDF8" : "#0284C7"));
            Resources["InputBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F8FAFC"));
            Resources["InputBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#3C4043" : "#CBD5E1"));
            Resources["InputTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            Resources["SecondaryBtnBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));
            Resources["SecondaryBtnFgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            Resources["CloseBtnBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));
            Resources["CloseBtnFgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#475569"));

            Background = (SolidColorBrush)Resources["WindowBgBrush"];
            Foreground = (SolidColorBrush)Resources["TextPrimaryBrush"];

            ApplyNativeWindowTheme(isDark);
        }

        private void ApplyNativeWindowTheme(bool isDark)
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    int darkMode = isDark ? 1 : 0;
                    DwmSetWindowAttribute(hwnd, 20, ref darkMode, sizeof(int));
                    DwmSetWindowAttribute(hwnd, 19, ref darkMode, sizeof(int));
                }
            }
            catch { }
        }

        private static bool IsWindowsInLightMode()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var val = key?.GetValue("AppsUseLightTheme");
                if (val is int intVal && intVal == 1)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void UpdateAdBlockLabel(bool isEnabled)
        {
            CbAdBlock.Content = isEnabled ? "Enabled (Interception active)" : "Disabled";
            CbAdBlock.Foreground = new SolidColorBrush(isEnabled
                ? (Color)ColorConverter.ConvertFromString("#60A5FA")
                : (Color)ColorConverter.ConvertFromString("#94A3B8"));
        }

        private void OnAdBlockChecked(object sender, RoutedEventArgs e)
        {
            UpdateAdBlockLabel(true);
        }

        private void OnAdBlockUnchecked(object sender, RoutedEventArgs e)
        {
            UpdateAdBlockLabel(false);
        }

        private void UpdateVpnSummary()
        {
            if (!string.IsNullOrWhiteSpace(_settings.VpnServerAddress))
            {
                TbVpnStatusSummary.Text = $"Configured ({_settings.VpnServerAddress}:{_settings.VpnServerPort})";
            }
            else
            {
                TbVpnStatusSummary.Text = "No profile configured. Click Configure VPN to setup.";
            }
        }

        private void OnSetDefaultBrowserClicked(object sender, RoutedEventArgs e)
        {
            DefaultBrowserHelper.OpenDefaultBrowserSettings();
            UpdateDefaultBrowserStatus();
        }

        private void OnBrowseDownloadsClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select Default Downloads Folder",
                    UseDescriptionForTitle = true,
                    SelectedPath = TbDownloadsPath.Text
                };

                var hwnd = new WindowInteropHelper(this).Handle;
                var owner = new Win32WindowWrapper(hwnd);

                if (dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
                {
                    TbDownloadsPath.Text = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open folder picker: " + ex.Message, "Downloads", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnClearDataClicked(object sender, RoutedEventArgs e)
        {
            var dlg = new ClearDataDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                if (dlg.ClearHistorySelected)
                {
                    DataManager.ClearHistory();
                }

                if (dlg.ClearBookmarksSelected)
                {
                    DataManager.ClearBookmarks();
                }

                if (dlg.ClearPasswordsSelected)
                {
                    PasswordVaultService.SaveCredentials(new List<SavedCredential>());
                }

                ClearDataHandler?.Invoke(
                    dlg.ClearHistorySelected,
                    dlg.ClearCookiesSelected,
                    dlg.ClearCacheSelected,
                    dlg.ClearBookmarksSelected
                );

                MessageBox.Show(this, "Selected browsing data cleared successfully.", "Clear Browsing Data", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnManagePasswordsClicked(object sender, RoutedEventArgs e)
        {
            var win = new PasswordsWindow { Owner = this };
            win.ShowDialog();
        }

        private void OnManageExtensionsClicked(object sender, RoutedEventArgs e)
        {
            var win = new ExtensionsWindow(null) { Owner = this };
            win.ShowDialog();
        }

        private void OnToggleTokenVisibility(object sender, RoutedEventArgs e)
        {
            _isTokenVisible = !_isTokenVisible;
            if (_isTokenVisible)
            {
                TbAiTokenVisible.Text = PbAiToken.Password;
                PbAiToken.Visibility = Visibility.Collapsed;
                TbAiTokenVisible.Visibility = Visibility.Visible;
                BtnToggleToken.Content = "Hide";
            }
            else
            {
                PbAiToken.Password = TbAiTokenVisible.Text;
                TbAiTokenVisible.Visibility = Visibility.Collapsed;
                PbAiToken.Visibility = Visibility.Visible;
                BtnToggleToken.Content = "Show";
            }
        }

        private void OnResetAiEndpoint(object sender, RoutedEventArgs e)
        {
            TbAiEndpoint.Text = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2";
        }

        private async void OnTestAiClicked(object sender, RoutedEventArgs e)
        {
            string token = _isTokenVisible ? TbAiTokenVisible.Text.Trim() : PbAiToken.Password.Trim();
            string endpoint = TbAiEndpoint.Text.Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                TbAiTestResult.Text = "⚠️ Please enter a Hugging Face API token first";
                TbAiTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                return;
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                TbAiTestResult.Text = "⚠️ Please enter a model inference endpoint";
                TbAiTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                return;
            }

            BtnTestAi.IsEnabled = false;
            TbAiTestResult.Text = "Testing connection...";
            TbAiTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA"));

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
                req.Headers.Add("Authorization", $"Bearer {token}");
                var payload = new { inputs = "Hello", parameters = new { max_new_tokens = 5 } };
                req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var resp = await _testClient.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    TbAiTestResult.Text = "✅ Connected successfully! Model is ready.";
                    TbAiTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                }
                else
                {
                    TbAiTestResult.Text = $"❌ Error HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}";
                    TbAiTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                }
            }
            catch (Exception ex)
            {
                TbAiTestResult.Text = $"❌ Connection failed: {ex.Message}";
                TbAiTestResult.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
            finally
            {
                BtnTestAi.IsEnabled = true;
            }
        }

        private void OnOpenVpnSettingsClicked(object sender, RoutedEventArgs e)
        {
            var vpnWin = new VpnWindow { Owner = this };
            vpnWin.ShowDialog();
            var reloaded = DataManager.LoadSettings();
            _settings.VpnServerAddress = reloaded.VpnServerAddress;
            _settings.VpnServerPort = reloaded.VpnServerPort;
            _settings.VpnClientAddress = reloaded.VpnClientAddress;
            _settings.VpnDns = reloaded.VpnDns;
            _settings.VpnPublicKey = reloaded.VpnPublicKey;
            _settings.VpnPrivateKey = reloaded.VpnPrivateKey;
            UpdateVpnSummary();
        }

        private void OnResetSettingsClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(this,
                "Are you sure you want to restore all settings to their original defaults?\n\n" +
                "• Startup and Homepage will reset to RUHI Home Screen (Simple)\n" +
                "• Search engine will reset to Google\n" +
                "• Theme will reset to Dark\n" +
                "• AdBlock will be enabled\n" +
                "• Downloads folder will reset to default\n" +
                "• AI Assistant & VPN settings will be reset\n\n" +
                "Note: Your bookmarks, history, and saved passwords will NOT be deleted.",
                "Reset Settings - RUHI Browser",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var defaultSettings = new BrowserSettings();

            _settings.SearchEngine = defaultSettings.SearchEngine;
            _settings.AdBlockEnabled = defaultSettings.AdBlockEnabled;
            _settings.Theme = defaultSettings.Theme;
            _settings.DownloadFolder = defaultSettings.DownloadFolder;
            _settings.HomepageMode = defaultSettings.HomepageMode;
            _settings.HomeScreenStyle = defaultSettings.HomeScreenStyle;
            _settings.CustomHomepageUrl = defaultSettings.CustomHomepageUrl;
            _settings.ActiveExtensionNewTabUrl = defaultSettings.ActiveExtensionNewTabUrl;
            _settings.ActiveExtensionNewTabName = defaultSettings.ActiveExtensionNewTabName;
            _settings.QuickNoteText = defaultSettings.QuickNoteText;
            _settings.DailyGoalText = defaultSettings.DailyGoalText;
            _settings.DailyGoalCompleted = defaultSettings.DailyGoalCompleted;
            _settings.AiApiKey = defaultSettings.AiApiKey;
            _settings.AiModelEndpoint = defaultSettings.AiModelEndpoint;
            _settings.VpnServerAddress = defaultSettings.VpnServerAddress;
            _settings.VpnServerPort = defaultSettings.VpnServerPort;
            _settings.VpnClientAddress = defaultSettings.VpnClientAddress;
            _settings.VpnDns = defaultSettings.VpnDns;
            _settings.VpnPublicKey = defaultSettings.VpnPublicKey;
            _settings.VpnPrivateKey = defaultSettings.VpnPrivateKey;

            AdBlockManager.IsEnabled = _settings.AdBlockEnabled;
            DataManager.SaveSettings(_settings);

            // Refresh UI controls
            InitHomepageMode();
            InitSearchEngine();
            InitTheme();
            CbAdBlock.IsChecked = _settings.AdBlockEnabled;
            UpdateAdBlockLabel(_settings.AdBlockEnabled);
            CbMemorySaver.IsChecked = _settings.MemorySaverEnabled;

            var defaultDownloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            TbDownloadsPath.Text = defaultDownloads;

            PbAiToken.Password = string.Empty;
            TbAiTokenVisible.Text = string.Empty;
            TbAiEndpoint.Text = _settings.AiModelEndpoint;
            TbAiTestResult.Text = string.Empty;
            UpdateVpnSummary();

            // Re-apply Theme
            ApplyTheme(_settings.Theme);

            MessageBox.Show(this,
                "All settings have been successfully restored to their original defaults.",
                "Settings Reset",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnOpenAboutClicked(object sender, RoutedEventArgs e)
        {
            new AboutWindow { Owner = this }.ShowDialog();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            // 1. Search Engine
            if (CbSearchEngine.SelectedItem is ComboBoxItem seItem)
            {
                _settings.SearchEngine = seItem.Content.ToString() ?? "Google";
            }

            // 2. Theme
            if (CbTheme.SelectedItem is ComboBoxItem themeItem)
            {
                _settings.Theme = themeItem.Content.ToString() ?? "Dark";
            }

            // 3. Homepage
            switch (CbHomepageMode.SelectedIndex)
            {
                case 1:
                    _settings.HomepageMode = "Extension";
                    break;
                case 2:
                    _settings.HomepageMode = "Google";
                    break;
                case 3:
                    _settings.HomepageMode = "Blank";
                    break;
                case 4:
                    _settings.HomepageMode = "Custom";
                    _settings.CustomHomepageUrl = TbCustomHomepageUrl.Text.Trim();
                    break;
                default:
                    _settings.HomepageMode = "Home";
                    break;
            }

            // Home Screen Style
            if (CbHomeScreenStyle?.SelectedItem is ComboBoxItem styleItem && styleItem.Tag != null)
            {
                _settings.HomeScreenStyle = styleItem.Tag.ToString() ?? "Simple";
            }

            // 4. AdBlock
            _settings.AdBlockEnabled = CbAdBlock.IsChecked == true;
            AdBlockManager.IsEnabled = _settings.AdBlockEnabled;

            // 4b. Memory Saver
            _settings.MemorySaverEnabled = CbMemorySaver.IsChecked == true;

            // 5. Downloads
            _settings.DownloadFolder = TbDownloadsPath.Text.Trim();
            if (!string.IsNullOrWhiteSpace(_settings.DownloadFolder) && !Directory.Exists(_settings.DownloadFolder))
            {
                try
                {
                    Directory.CreateDirectory(_settings.DownloadFolder);
                }
                catch { }
            }

            // 6. AI Token & Endpoint
            _settings.AiApiKey = _isTokenVisible ? TbAiTokenVisible.Text.Trim() : PbAiToken.Password.Trim();
            _settings.AiModelEndpoint = TbAiEndpoint.Text.Trim();

            // Save
            DataManager.SaveSettings(_settings);

            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private class Win32WindowWrapper : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; }
            public Win32WindowWrapper(IntPtr handle) => Handle = handle;
        }
    }
}
