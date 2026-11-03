using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class PasswordsWindow : Window
    {
        private List<SavedCredential> _allCredentials = new();

        public PasswordsWindow()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);
            LoadVault();
        }

        private void LoadVault()
        {
            _allCredentials = PasswordVaultService.LoadCredentials();
            FilterList();
        }

        private void FilterList()
        {
            var query = TbSearch.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allCredentials
                : _allCredentials.Where(c => c.WebsiteTitle.ToLowerInvariant().Contains(query) ||
                                            c.WebsiteUrl.ToLowerInvariant().Contains(query) ||
                                            c.Username.ToLowerInvariant().Contains(query)).ToList();

            LbCredentials.ItemsSource = null;
            LbCredentials.ItemsSource = filtered;
            TbCount.Text = $"{filtered.Count} of {_allCredentials.Count} password{(filtered.Count == 1 ? "" : "s")}";
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterList();
        }

        private void OnAddNewClicked(object sender, RoutedEventArgs e)
        {
            BorderAddCard.Visibility = Visibility.Visible;
            TbNewWebsite.Focus();
        }

        private void OnCancelAddClicked(object sender, RoutedEventArgs e)
        {
            BorderAddCard.Visibility = Visibility.Collapsed;
            TbNewWebsite.Text = string.Empty;
            TbNewUsername.Text = string.Empty;
            PbNewPassword.Password = string.Empty;
        }

        private void OnSaveNewCredentialClicked(object sender, RoutedEventArgs e)
        {
            var website = TbNewWebsite.Text?.Trim() ?? string.Empty;
            var username = TbNewUsername.Text?.Trim() ?? string.Empty;
            var password = PbNewPassword.Password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(website))
            {
                MessageBox.Show(this, "Please enter a website URL or domain.", "Password Vault", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(this, "Please enter a username or email.", "Password Vault", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show(this, "Please enter a password.", "Password Vault", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var title = PasswordVaultService.GetDomainFromUrl(website);
            PasswordVaultService.AddOrUpdate(website, title, username, password);

            OnCancelAddClicked(sender, e);
            LoadVault();
        }

        private void OnToggleRevealClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SavedCredential cred)
            {
                if (!cred.IsPasswordRevealed)
                {
                    cred.DecryptedPassword = PasswordVaultService.DecryptPassword(cred.EncryptedPassword);
                    cred.IsPasswordRevealed = true;
                }
                else
                {
                    cred.IsPasswordRevealed = false;
                    cred.DecryptedPassword = string.Empty;
                }
            }
        }

        private void OnCopyUsernameClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string username && !string.IsNullOrEmpty(username))
            {
                try
                {
                    Clipboard.SetDataObject(username, true);
                    MessageBox.Show(this, "Username copied to clipboard!", "Password Vault", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not access clipboard: " + ex.Message, "Clipboard Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void OnCopyPasswordClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SavedCredential cred)
            {
                var decrypted = PasswordVaultService.DecryptPassword(cred.EncryptedPassword);
                if (!string.IsNullOrEmpty(decrypted))
                {
                    try
                    {
                        Clipboard.SetDataObject(decrypted, true);
                        MessageBox.Show(this, "Password copied to clipboard!", "Password Vault", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Could not access clipboard: " + ex.Message, "Clipboard Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                if (MessageBox.Show(this, "Are you sure you want to delete this saved password?", "Delete Credential",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    PasswordVaultService.Delete(id);
                    LoadVault();
                }
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
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
