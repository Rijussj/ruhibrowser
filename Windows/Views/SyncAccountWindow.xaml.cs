using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class SyncAccountWindow : Window
    {
        private bool _isSignUpMode = false;
        private readonly string _currentTabUrl;
        private readonly string _currentTabTitle;
        private readonly Func<Task>? _triggerFullSyncAction;

        public SyncAccountWindow(string currentTabUrl = "", string currentTabTitle = "", Func<Task>? triggerFullSync = null)
        {
            InitializeComponent();
            _currentTabUrl = currentTabUrl;
            _currentTabTitle = currentTabTitle;
            _triggerFullSyncAction = triggerFullSync;

            var settings = DataManager.LoadSettings();
            ThemeManager.ApplyThemeToWindow(this, settings.Theme);

            if (!string.IsNullOrWhiteSpace(_currentTabUrl))
            {
                TbCurrentTabTitle.Text = string.IsNullOrWhiteSpace(_currentTabTitle) ? _currentTabUrl : _currentTabTitle;
                TbCurrentTabUrl.Text = _currentTabUrl;
            }
            else
            {
                TbCurrentTabTitle.Text = "No active page open";
                TbCurrentTabUrl.Text = "Open a website to send it to your phone";
            }

            RefreshView();
        }

        private void RefreshView()
        {
            if (SyncService.IsLoggedIn)
            {
                PanelAuth.Visibility = Visibility.Collapsed;
                PanelDashboard.Visibility = Visibility.Visible;

                string displayName = SyncService.CurrentDisplayName;
                TbLoggedInName.Text = string.IsNullOrWhiteSpace(displayName) ? "Ruhi User" : displayName;
                TbLoggedInEmail.Text = SyncService.CurrentEmail;
                TbEditDisplayName.Text = displayName;
                TbLoggedInInitial.Text = string.IsNullOrWhiteSpace(displayName)
                    ? (!string.IsNullOrWhiteSpace(SyncService.CurrentEmail) ? SyncService.CurrentEmail.Substring(0, 1).ToUpperInvariant() : "👤")
                    : displayName.Substring(0, 1).ToUpperInvariant();

                TbLastSyncTime.Text = "Last synced: " + DateTime.Now.ToString("h:mm tt");
                _ = LoadDevicesAsync();
            }
            else
            {
                PanelAuth.Visibility = Visibility.Visible;
                PanelDashboard.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnSaveDisplayNameClicked(object sender, RoutedEventArgs e)
        {
            string newName = TbEditDisplayName.Text.Trim();
            bool ok = await SyncService.UpdateDisplayNameAsync(newName);
            RefreshView();
            if (ok)
            {
                MessageBox.Show(this, "Account name updated & saved to Supabase Cloud!", "Ruhi Account", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, "Account name saved locally (cloud sync offline).", "Ruhi Account", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnTabLoginClicked(object sender, RoutedEventArgs e)
        {
            _isSignUpMode = false;
            PanelNameField.Visibility = Visibility.Collapsed;
            BtnTabLogin.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            BtnTabLogin.Foreground = Brushes.White;
            BtnTabSignUp.Background = Brushes.Transparent;
            BtnTabSignUp.Foreground = (Brush)FindResource("TextSecondaryBrush");
            BtnAuthSubmit.Content = "Sign In to Sync";
            TbAuthStatus.Visibility = Visibility.Collapsed;
        }

        private void OnTabSignUpClicked(object sender, RoutedEventArgs e)
        {
            _isSignUpMode = true;
            PanelNameField.Visibility = Visibility.Visible;
            BtnTabSignUp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
            BtnTabSignUp.Foreground = Brushes.White;
            BtnTabLogin.Background = Brushes.Transparent;
            BtnTabLogin.Foreground = (Brush)FindResource("TextSecondaryBrush");
            BtnAuthSubmit.Content = "Create Free Sync Account";
            TbAuthStatus.Visibility = Visibility.Collapsed;
        }

        private async void OnAuthSubmitClicked(object sender, RoutedEventArgs e)
        {
            string name = TbName.Text.Trim();
            string email = TbEmail.Text.Trim();
            string pass = PbPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            {
                ShowStatus("Please enter both email and password.", isError: true);
                return;
            }

            if (pass.Length < 6)
            {
                ShowStatus("Password must be at least 6 characters.", isError: true);
                return;
            }

            BtnAuthSubmit.IsEnabled = false;
            ShowStatus("Connecting to RUHI Cloud...", isError: false);

            if (_isSignUpMode)
            {
                var (success, msg) = await SyncService.SignUpAsync(email, pass, name);
                if (success)
                {
                    ShowStatus("Account created successfully!", isError: false);
                    await TriggerSync();
                    RefreshView();
                }
                else
                {
                    ShowStatus(msg, isError: true);
                }
            }
            else
            {
                var (success, msg) = await SyncService.LoginAsync(email, pass);
                if (success)
                {
                    ShowStatus("Login successful!", isError: false);
                    await TriggerSync();
                    RefreshView();
                }
                else
                {
                    ShowStatus(msg, isError: true);
                }
            }

            BtnAuthSubmit.IsEnabled = true;
        }

        private async Task TriggerSync()
        {
            if (_triggerFullSyncAction != null)
            {
                await _triggerFullSyncAction();
            }
        }

        private void ShowStatus(string msg, bool isError)
        {
            TbAuthStatus.Text = msg;
            TbAuthStatus.Foreground = isError ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
                                              : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
            TbAuthStatus.Visibility = Visibility.Visible;
        }

        private async void OnSyncNowClicked(object sender, RoutedEventArgs e)
        {
            TbLastSyncTime.Text = "Syncing now...";
            await TriggerSync();
            TbLastSyncTime.Text = "Last synced: " + DateTime.Now.ToString("h:mm tt");
            await LoadDevicesAsync();
            MessageBox.Show(this, "Bookmarks, history, and tabs synced with RUHI Cloud!", "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task LoadDevicesAsync()
        {
            PanelDevicesList.Children.Clear();
            var devices = await SyncService.GetDevicesAsync();

            if (devices == null || devices.Count == 0)
            {
                var tb = new TextBlock
                {
                    Text = "No other devices linked yet.\nOpen RUHI Browser on your Android phone and log in to link.",
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap
                };
                PanelDevicesList.Children.Add(tb);
                return;
            }

            foreach (var d in devices)
            {
                bool isThisDevice = d.DeviceId == SyncService.CurrentDeviceId;
                var card = new Border
                {
                    Background = (Brush)FindResource("InputBgBrush"),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 8, 10, 8),
                    Margin = new Thickness(0, 0, 0, 6)
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var sp = new StackPanel();
                string icon = d.DeviceType.Equals("Android", StringComparison.OrdinalIgnoreCase) ? "📱" : "💻";
                string name = isThisDevice ? $"{d.DeviceName} (This PC)" : d.DeviceName;
                
                var title = new TextBlock
                {
                    Text = $"{icon} {name}",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 13,
                    Foreground = (Brush)FindResource("TextPrimaryBrush")
                };
                var subtitle = new TextBlock
                {
                    Text = $"Active: {d.LastActive:g} • {d.ActiveTabs.Count} open tabs",
                    FontSize = 11,
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    Margin = new Thickness(0, 2, 0, 0)
                };

                sp.Children.Add(title);
                sp.Children.Add(subtitle);
                Grid.SetColumn(sp, 0);
                grid.Children.Add(sp);

                if (!isThisDevice)
                {
                    var btnSend = new Button
                    {
                        Content = "Send Tab ➔",
                        Style = (Style)FindResource("ModernBtn"),
                        Padding = new Thickness(10, 4, 10, 4),
                        FontSize = 11,
                        VerticalAlignment = VerticalAlignment.Center,
                        Tag = d.DeviceId
                    };
                    btnSend.Click += async (s, ev) =>
                    {
                        if (string.IsNullOrWhiteSpace(_currentTabUrl))
                        {
                            MessageBox.Show(this, "No active tab to send.", "Send Tab", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        bool ok = await SyncService.SendTabToDeviceAsync(d.DeviceId, _currentTabUrl, _currentTabTitle);
                        if (ok)
                        {
                            MessageBox.Show(this, $"Sent tab to {d.DeviceName} successfully!", "Tab Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    };
                    Grid.SetColumn(btnSend, 1);
                    grid.Children.Add(btnSend);
                }

                card.Child = grid;
                PanelDevicesList.Children.Add(card);
            }
        }

        private async void OnSendCurrentTabClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentTabUrl))
            {
                MessageBox.Show(this, "No active web page open to send.", "Send Tab", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool ok = await SyncService.SendTabToDeviceAsync(null, _currentTabUrl, _currentTabTitle);
            if (ok)
            {
                MessageBox.Show(this, "Tab sent to your linked phone and devices!\nIt will open automatically when you launch RUHI Browser on your phone.", "Tab Sent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, "Could not send tab. Make sure you are logged in to RUHI Sync.", "Send Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLogoutClicked(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(this, "Are you sure you want to sign out of RUHI Cloud Sync?", "Sign Out", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                SyncService.Logout();
                RefreshView();
            }
        }
    }
}
