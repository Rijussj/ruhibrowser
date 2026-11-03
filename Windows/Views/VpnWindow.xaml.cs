using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class VpnWindow : Window
    {
        private readonly BrowserSettings _settings;
        private static bool _isConnected = false;

        public VpnWindow()
        {
            InitializeComponent();
            _settings = DataManager.LoadSettings();
            ThemeManager.ApplyThemeToWindow(this, _settings.Theme);
            LoadFields();
            UpdateStatusUi();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void LoadFields()
        {
            TbServer.Text = _settings.VpnServerAddress;
            TbPort.Text = _settings.VpnServerPort > 0 ? _settings.VpnServerPort.ToString() : "51820";
            TbClientIp.Text = string.IsNullOrWhiteSpace(_settings.VpnClientAddress) ? "10.0.0.2/32" : _settings.VpnClientAddress;
            TbDns.Text = string.IsNullOrWhiteSpace(_settings.VpnDns) ? "1.1.1.1, 1.0.0.1" : _settings.VpnDns;
            TbPublicKey.Text = _settings.VpnPublicKey;
            PbPrivateKey.Password = _settings.VpnPrivateKey;
        }

        private void UpdateStatusUi()
        {
            if (_isConnected)
            {
                TbStatusTitle.Text = "WireGuard Profile Active";
                TbStatusTitle.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 197, 94)); // Green
                TbStatusDetail.Text = $"Profile active for {_settings.VpnServerAddress}:{_settings.VpnServerPort}. Use 'Export .conf' to apply system-wide.";
                BtnConnect.Content = "Deactivate";
                BtnConnect.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 38, 38)); // Red
            }
            else
            {
                bool configured = !string.IsNullOrWhiteSpace(_settings.VpnServerAddress);
                TbStatusTitle.Text = configured ? "WireGuard Ready" : "WireGuard Not Configured";
                bool isDark = ThemeManager.IsDarkTheme(_settings.Theme);
                TbStatusTitle.Foreground = new System.Windows.Media.SolidColorBrush(
                    isDark ? System.Windows.Media.Color.FromRgb(148, 163, 184) : System.Windows.Media.Color.FromRgb(71, 85, 105));
                TbStatusDetail.Text = configured
                    ? $"Profile saved: {_settings.VpnServerAddress}:{_settings.VpnServerPort}. Click Activate or export configuration."
                    : "Configure server parameters below or export a WireGuard profile (.conf).";
                BtnConnect.Content = "Activate";
                BtnConnect.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 99, 235)); // Blue
            }
        }

        private void OnConnectToggleClicked(object sender, RoutedEventArgs e)
        {
            if (_isConnected)
            {
                _isConnected = false;
                UpdateStatusUi();
                MessageBox.Show(this, "RUHI VPN profile deactivated.", "RUHI VPN", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(TbServer.Text.Trim()))
                {
                    MessageBox.Show(this, "Please enter a Server Address before activating.", "RUHI VPN", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TbServer.Focus();
                    return;
                }

                SaveFields();
                _isConnected = true;
                UpdateStatusUi();
                MessageBox.Show(this, $"WireGuard VPN profile activated for {_settings.VpnServerAddress}:{_settings.VpnServerPort}!\n\nTip: You can click 'Export .conf' below to export standard WireGuard config for the official WireGuard Windows client.", "RUHI VPN", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveFields()
        {
            _settings.VpnServerAddress = TbServer.Text.Trim();
            if (int.TryParse(TbPort.Text.Trim(), out int port))
            {
                _settings.VpnServerPort = port;
            }
            _settings.VpnClientAddress = TbClientIp.Text.Trim();
            _settings.VpnDns = TbDns.Text.Trim();
            _settings.VpnPublicKey = TbPublicKey.Text.Trim();
            _settings.VpnPrivateKey = PbPrivateKey.Password;

            DataManager.SaveSettings(_settings);
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            SaveFields();
            UpdateStatusUi();
            MessageBox.Show(this, "WireGuard profile saved successfully!", "RUHI VPN", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExportConfClicked(object sender, RoutedEventArgs e)
        {
            SaveFields();

            var sfd = new SaveFileDialog
            {
                Filter = "WireGuard Config (*.conf)|*.conf",
                FileName = "ruhi-wireguard.conf"
            };

            if (sfd.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[Interface]");
                sb.AppendLine($"PrivateKey = {_settings.VpnPrivateKey}");
                sb.AppendLine($"Address = {_settings.VpnClientAddress}");
                if (!string.IsNullOrWhiteSpace(_settings.VpnDns))
                {
                    sb.AppendLine($"DNS = {_settings.VpnDns}");
                }
                sb.AppendLine();
                sb.AppendLine("[Peer]");
                sb.AppendLine($"PublicKey = {_settings.VpnPublicKey}");
                sb.AppendLine($"Endpoint = {_settings.VpnServerAddress}:{_settings.VpnServerPort}");
                sb.AppendLine("AllowedIPs = 0.0.0.0/0, ::/0");
                sb.AppendLine("PersistentKeepalive = 25");

                File.WriteAllText(sfd.FileName, sb.ToString());
                MessageBox.Show(this, $"Configuration exported successfully to:\n{sfd.FileName}", "RUHI VPN", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
