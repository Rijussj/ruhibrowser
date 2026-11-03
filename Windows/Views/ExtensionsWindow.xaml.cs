using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class ExtensionsWindow : Window
    {
        private readonly CoreWebView2Profile? _profile;
        private readonly Action<string>? _navigateAction;
        private readonly List<ExtensionInfo> _extensions = new();

        public ExtensionsWindow(CoreWebView2Profile? profile, Action<string>? navigateAction = null)
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);

            _profile = profile;
            _navigateAction = navigateAction;

            Loaded += async (s, e) => await LoadExtensionsAsync();
        }

        public async Task LoadExtensionsAsync()
        {
            _extensions.Clear();

            string extensionsDir = ExtensionManagerService.GetExtensionsDirectory();

            // 1. Auto-sync extensions on disk with WebView2 Profile if profile is ready
            if (_profile != null && Directory.Exists(extensionsDir))
            {
                try
                {
                    var existingCore = await _profile.GetBrowserExtensionsAsync();
                    var subdirs = Directory.GetDirectories(extensionsDir);
                    foreach (var dir in subdirs)
                    {
                        var dirName = Path.GetFileName(dir);
                        if (dirName.StartsWith(".") || dirName.Contains("_trash_")) continue;

                        var manifestFolder = ExtensionManagerService.FindManifestFolder(dir);
                        if (File.Exists(Path.Combine(manifestFolder, "manifest.json")))
                        {
                            var meta = ExtensionManagerService.ReadManifest(manifestFolder);
                            bool alreadyInProfile = existingCore.Any(c =>
                                string.Equals(c.Name, meta.Name, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(c.Name, Path.GetFileName(dir), StringComparison.OrdinalIgnoreCase));

                            if (!alreadyInProfile)
                            {
                                try
                                {
                                    await _profile.AddBrowserExtensionAsync(manifestFolder);
                                }
                                catch (Exception regEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Failed auto-syncing extension {dir}: {regEx.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error during extension disk sync: " + ex.Message);
                }
            }

            var processedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 2. Load all registered extensions from profile
            if (_profile != null)
            {
                try
                {
                    var coreExts = await _profile.GetBrowserExtensionsAsync();
                    foreach (var ext in coreExts)
                    {
                        // Filter out WebView2 internal / system extensions (never show internal components)
                        if (ext.Id == "dgiklkfkllikcanfonkcabmbdfmgleag" ||
                            ext.Id == "mhjfbmdgcfjbbpaeojofohoefgiehjai" ||
                            ext.Name.StartsWith("Microsoft ", StringComparison.OrdinalIgnoreCase) ||
                            ext.Name.Contains("Edge PDF Viewer", StringComparison.OrdinalIgnoreCase) ||
                            ext.Name.Contains("Clipboard Extension", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var info = new ExtensionInfo
                        {
                            Id = ext.Id,
                            Name = ext.Name,
                            IsEnabled = ext.IsEnabled,
                            CoreExtension = ext
                        };

                        // Check disk for manifest metadata & root folder
                        if (Directory.Exists(extensionsDir))
                        {
                            var subdirs = Directory.GetDirectories(extensionsDir);
                            foreach (var dir in subdirs)
                            {
                                var manifestFolder = ExtensionManagerService.FindManifestFolder(dir);
                                var meta = ExtensionManagerService.ReadManifest(manifestFolder);
                                if (string.Equals(meta.Name, ext.Name, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(Path.GetFileName(dir), ext.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    info.Version = $"v{meta.Version}";
                                    info.Description = meta.Description;
                                    info.IconPath = meta.IconPath;
                                    info.FolderPath = dir; // Root folder for safe deletion
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(info.Version)) info.Version = "v1.0";
                        if (string.IsNullOrEmpty(info.Description)) info.Description = "Installed Chrome Extension";

                        _extensions.Add(info);
                        processedNames.Add(ext.Name);
                    }
                }
                catch (Exception ex)
                {
                    TbStatusMessage.Text = "Status: " + ex.Message;
                }
            }

            // 3. Fallback: Show any extensions found on disk even if not registered in profile
            if (Directory.Exists(extensionsDir))
            {
                try
                {
                    var subdirs = Directory.GetDirectories(extensionsDir);
                    foreach (var dir in subdirs)
                    {
                        var dirName = Path.GetFileName(dir);
                        if (dirName.StartsWith(".") || dirName.Contains("_trash_")) continue;

                        var manifestFolder = ExtensionManagerService.FindManifestFolder(dir);
                        if (File.Exists(Path.Combine(manifestFolder, "manifest.json")))
                        {
                            var meta = ExtensionManagerService.ReadManifest(manifestFolder);
                            if (!processedNames.Contains(meta.Name) && !processedNames.Contains(Path.GetFileName(dir)))
                            {
                                _extensions.Add(new ExtensionInfo
                                {
                                    Id = Path.GetFileName(dir),
                                    Name = meta.Name,
                                    Version = $"v{meta.Version}",
                                    Description = string.IsNullOrEmpty(meta.Description) ? "Installed Chrome Extension" : meta.Description,
                                    IconPath = meta.IconPath,
                                    FolderPath = dir,
                                    IsEnabled = true
                                });
                                processedNames.Add(meta.Name);
                            }
                        }
                    }
                }
                catch { }
            }

            RenderInstalledExtensions();
        }

        private void RenderInstalledExtensions()
        {
            PanelInstalledExtensions.Children.Clear();
            TbInstalledCount.Text = _extensions.Count.ToString();

            if (_extensions.Count == 0)
            {
                BorderEmptyState.Visibility = Visibility.Visible;
                return;
            }

            BorderEmptyState.Visibility = Visibility.Collapsed;
            bool isDark = ThemeManager.IsDarkTheme();

            var cardBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF"));
            var cardBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1"));
            var textFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            var mutedFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));

            foreach (var ext in _extensions)
            {
                var card = new Border
                {
                    Background = cardBg,
                    BorderBrush = cardBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(16, 12, 16, 12)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Icon
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 0 }); // Info
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Controls

                // 1. Icon Box
                var iconBox = new Border
                {
                    Width = 42,
                    Height = 42,
                    CornerRadius = new CornerRadius(9),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0")),
                    Margin = new Thickness(0, 0, 14, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (!string.IsNullOrEmpty(ext.IconPath) && File.Exists(ext.IconPath))
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bmp.UriSource = new Uri(ext.IconPath, UriKind.Absolute);
                        bmp.DecodePixelWidth = 42;
                        bmp.EndInit();
                        bmp.Freeze();
                        iconBox.Child = new Image { Source = bmp, Stretch = Stretch.Uniform, Margin = new Thickness(5) };
                    }
                    catch
                    {
                        iconBox.Child = new TextBlock { Text = "🧩", FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    }
                }
                else
                {
                    iconBox.Child = new TextBlock { Text = "🧩", FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                }
                Grid.SetColumn(iconBox, 0);
                grid.Children.Add(iconBox);

                // 2. Info Stack
                var infoStack = new StackPanel 
                { 
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 14, 0)
                };

                var titleRow = new DockPanel { LastChildFill = true };

                var versionBadge = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0")),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 1, 6, 1),
                    Margin = new Thickness(8, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                versionBadge.Child = new TextBlock
                {
                    Text = ext.Version,
                    FontSize = 10.5,
                    Foreground = mutedFg,
                    FontWeight = FontWeights.Medium
                };
                DockPanel.SetDock(versionBadge, Dock.Right);
                titleRow.Children.Add(versionBadge);

                var tbName = new TextBlock
                {
                    Text = ext.Name,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = textFg,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                titleRow.Children.Add(tbName);
                infoStack.Children.Add(titleRow);

                var tbDesc = new TextBlock
                {
                    Text = ext.Description,
                    FontSize = 12,
                    Foreground = mutedFg,
                    Margin = new Thickness(0, 4, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxHeight = 36
                };
                infoStack.Children.Add(tbDesc);
                Grid.SetColumn(infoStack, 1);
                grid.Children.Add(infoStack);

                // 3. Controls Stack
                var controlsStack = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                // Modern Pill Toggle Switch
                var btnToggle = new Button
                {
                    Content = ext.IsEnabled ? "✓ ON" : "OFF",
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ext.IsEnabled ? "#10B981" : (isDark ? "#3C4043" : "#94A3B8"))),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 11.5,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(14, 6, 14, 6),
                    Margin = new Thickness(0, 0, 10, 0),
                    Cursor = Cursors.Hand
                };
                btnToggle.Resources.Add(typeof(Border), new Style(typeof(Border)) { Setters = { new Setter(Border.CornerRadiusProperty, new CornerRadius(14)) } });
                btnToggle.Click += async (s, e) =>
                {
                    if (ext.CoreExtension != null)
                    {
                        try
                        {
                            bool newState = !ext.IsEnabled;
                            await ext.CoreExtension.EnableAsync(newState);
                            ext.IsEnabled = newState;
                            btnToggle.Content = ext.IsEnabled ? "✓ ON" : "OFF";
                            btnToggle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ext.IsEnabled ? "#10B981" : (isDark ? "#3C4043" : "#94A3B8")));
                            TbStatusMessage.Text = $"Extension \"{ext.Name}\" is now {(ext.IsEnabled ? "Active" : "Disabled")}.";

                            var overrideInfo = await ExtensionManagerService.GetActiveNewTabOverrideAsync(_profile);
                            var settings = DataManager.LoadSettings();
                            if (!string.IsNullOrEmpty(overrideInfo.Url))
                            {
                                settings.ActiveExtensionNewTabUrl = overrideInfo.Url;
                                settings.ActiveExtensionNewTabName = overrideInfo.Name ?? string.Empty;
                                settings.HomepageMode = "Extension";
                            }
                            else if (settings.HomepageMode == "Extension")
                            {
                                settings.ActiveExtensionNewTabUrl = string.Empty;
                                settings.ActiveExtensionNewTabName = string.Empty;
                                settings.HomepageMode = "Home";
                            }
                            DataManager.SaveSettings(settings);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "Could not toggle extension: " + ex.Message, "Extensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                };
                controlsStack.Children.Add(btnToggle);

                // Remove Button
                var btnRemove = new Button
                {
                    Content = "🗑",
                    ToolTip = "Remove Extension",
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#7F1D1D" : "#FECACA")),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(9, 5, 9, 5),
                    Cursor = Cursors.Hand
                };
                btnRemove.Resources.Add(typeof(Border), new Style(typeof(Border)) { Setters = { new Setter(Border.CornerRadiusProperty, new CornerRadius(7)) } });
                btnRemove.Click += async (s, e) =>
                {
                    if (MessageBox.Show(this, $"Remove extension \"{ext.Name}\"?", "Remove Extension", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        btnRemove.IsEnabled = false;
                        try
                        {
                            // 1. Remove from profile directly
                            if (ext.CoreExtension != null)
                            {
                                try
                                {
                                    await ext.CoreExtension.RemoveAsync();
                                }
                                catch (Exception comEx)
                                {
                                    System.Diagnostics.Debug.WriteLine("RemoveAsync error: " + comEx.Message);
                                }
                            }
                            else if (_profile != null)
                            {
                                try
                                {
                                    var liveExts = await _profile.GetBrowserExtensionsAsync();
                                    var target = liveExts.FirstOrDefault(x => x.Id == ext.Id || string.Equals(x.Name, ext.Name, StringComparison.OrdinalIgnoreCase));
                                    if (target != null)
                                    {
                                        await target.RemoveAsync();
                                    }
                                }
                                catch { }
                            }

                            // 2. Force GC to release any file handles
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            // 3. Delete physical directory on disk
                            if (!string.IsNullOrEmpty(ext.FolderPath) && Directory.Exists(ext.FolderPath))
                            {
                                try 
                                {
                                    var di = new DirectoryInfo(ext.FolderPath);
                                    foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
                                    {
                                        file.Attributes = FileAttributes.Normal;
                                        try { file.Delete(); } catch { }
                                    }
                                    Directory.Delete(ext.FolderPath, true); 
                                } 
                                catch 
                                {
                                    // Move completely outside of Extensions folder into system Temp
                                    try
                                    {
                                        string trash = Path.Combine(Path.GetTempPath(), "RuhiTrash_" + Guid.NewGuid().ToString("N"));
                                        Directory.Move(ext.FolderPath, trash);
                                        try { Directory.Delete(trash, true); } catch { }
                                    }
                                    catch { }
                                }
                            }

                            // 4. Instantly remove from memory list and update UI
                            _extensions.RemoveAll(x => x.Id == ext.Id || string.Equals(x.Name, ext.Name, StringComparison.OrdinalIgnoreCase));
                            RenderInstalledExtensions();
                            TbStatusMessage.Text = $"Extension \"{ext.Name}\" removed successfully.";

                            var overrideInfo = await ExtensionManagerService.GetActiveNewTabOverrideAsync(_profile);
                            var settings = DataManager.LoadSettings();
                            if (!string.IsNullOrEmpty(overrideInfo.Url))
                            {
                                settings.ActiveExtensionNewTabUrl = overrideInfo.Url;
                                settings.ActiveExtensionNewTabName = overrideInfo.Name ?? string.Empty;
                            }
                            else if (settings.HomepageMode == "Extension")
                            {
                                settings.ActiveExtensionNewTabUrl = string.Empty;
                                settings.ActiveExtensionNewTabName = string.Empty;
                                settings.HomepageMode = "Home";
                            }
                            DataManager.SaveSettings(settings);

                            if (Owner is MainWindow mw)
                            {
                                _ = mw.Dispatcher.InvokeAsync(async () => await mw.NotifyStoreTabsExtensionChangedAsync());
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "Could not remove extension: " + ex.Message, "Extensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        finally
                        {
                            btnRemove.IsEnabled = true;
                        }
                    }
                };
                controlsStack.Children.Add(btnRemove);

                Grid.SetColumn(controlsStack, 2);
                grid.Children.Add(controlsStack);

                card.Child = grid;
                PanelInstalledExtensions.Children.Add(card);
            }
        }

        private async void OnLoadUnpackedClicked(object sender, RoutedEventArgs e)
        {
            if (_profile == null)
            {
                MessageBox.Show(this, "WebView2 profile is not ready.", "Extensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Unpacked Extension Folder (containing manifest.json)",
                UseDescriptionForTitle = true
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    TbStatusMessage.Text = "Installing unpacked extension...";
                    var ext = await ExtensionManagerService.InstallExtensionFromFolderAsync(dlg.SelectedPath, _profile);
                    await LoadExtensionsAsync();
                    var settings = DataManager.LoadSettings();
                    string extName = ext?.Name ?? "Selected";
                    string msg = $"Extension \"{extName}\" installed and activated successfully!";
                    if (settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(settings.ActiveExtensionNewTabUrl))
                    {
                        msg += $"\n\nNew Tab / Homepage has been set to {settings.ActiveExtensionNewTabName ?? extName}!";
                        _navigateAction?.Invoke(settings.ActiveExtensionNewTabUrl);
                    }
                    MessageBox.Show(this, msg, "Extension Installed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to load unpacked extension:\n" + ex.Message, "Extension Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void OnInstallArchiveClicked(object sender, RoutedEventArgs e)
        {
            if (_profile == null)
            {
                MessageBox.Show(this, "WebView2 profile is not ready.", "Extensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dlg = new OpenFileDialog
            {
                Title = "Select Extension Archive (.crx or .zip)",
                Filter = "Extension Files (*.crx;*.zip)|*.crx;*.zip|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    TbStatusMessage.Text = "Extracting and installing extension...";
                    var ext = await ExtensionManagerService.InstallExtensionFromArchiveAsync(dlg.FileName, _profile);
                    await LoadExtensionsAsync();
                    var settings = DataManager.LoadSettings();
                    string extName = ext?.Name ?? Path.GetFileNameWithoutExtension(dlg.FileName);
                    string msg = $"Extension \"{extName}\" installed successfully!";
                    if (settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(settings.ActiveExtensionNewTabUrl))
                    {
                        msg += $"\n\nNew Tab / Homepage has been set to {settings.ActiveExtensionNewTabName ?? extName}!";
                        _navigateAction?.Invoke(settings.ActiveExtensionNewTabUrl);
                    }
                    MessageBox.Show(this, msg, "Extension Installed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Failed to install extension archive:\n" + ex.Message, "Extension Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnWebStoreUrlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnInstallFromUrlClicked(sender, e);
            }
        }

        private async void OnInstallFromUrlClicked(object sender, RoutedEventArgs e)
        {
            if (_profile == null)
            {
                MessageBox.Show(this, "WebView2 profile is not ready.", "Extensions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var input = TbWebStoreUrl.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show(this, "Please paste an extension URL or 32-letter Extension ID.", "Extension Installer", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                BtnInstallFromUrl.IsEnabled = false;
                BtnInstallFromUrl.Content = "⏳ Installing...";
                TbStatusMessage.Text = "Fetching extension from add-on servers...";

                var ext = await ExtensionManagerService.InstallFromChromeWebStoreAsync(input, _profile);
                TbWebStoreUrl.Text = string.Empty;
                await LoadExtensionsAsync();

                var settings = DataManager.LoadSettings();
                string extName = ext?.Name ?? "Extension";
                string msg = $"Extension \"{extName}\" downloaded and installed successfully!";
                if (settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(settings.ActiveExtensionNewTabUrl))
                {
                    msg += $"\n\nNew Tab / Homepage has been set to {settings.ActiveExtensionNewTabName ?? extName}!";
                    _navigateAction?.Invoke(settings.ActiveExtensionNewTabUrl);
                }

                MessageBox.Show(this, msg, "Extension Installed 🧩", MessageBoxButton.OK, MessageBoxImage.Information);
                TbStatusMessage.Text = $"Extension \"{extName}\" installed successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to install extension:\n" + ex.Message + "\n\nTip: You can also download the .CRX file directly and click 'Install File'.", "Installation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbStatusMessage.Text = "Install failed: " + ex.Message;
            }
            finally
            {
                BtnInstallFromUrl.IsEnabled = true;
                BtnInstallFromUrl.Content = "➕ Add Extension";
            }
        }

        private void OnOpenCrx4ChromeClicked(object sender, RoutedEventArgs e)
        {
            _navigateAction?.Invoke("https://www.google.com/");
            Close();
        }

        private void OnOpenRuhiStoreClicked(object sender, RoutedEventArgs e)
        {
            _navigateAction?.Invoke("https://addonruhi.netlify.app/");
            Close();
        }

        private void OnOpenWebStoreClicked(object sender, RoutedEventArgs e)
        {
            _navigateAction?.Invoke("https://addonruhi.netlify.app/");
            Close();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (HeaderTitleStack == null || HeaderActionsPanel == null) return;

            // When header available width < 720px, dock Title to TOP so Actions Panel gets the full width of the window
            if (e.NewSize.Width < 720)
            {
                DockPanel.SetDock(HeaderTitleStack, Dock.Top);
                HeaderTitleStack.Margin = new Thickness(0, 0, 0, 10);
                HeaderActionsPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                // Wide mode: Title on Left, Actions on Right
                DockPanel.SetDock(HeaderTitleStack, Dock.Left);
                HeaderTitleStack.Margin = new Thickness(0, 0, 16, 0);
                HeaderActionsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
