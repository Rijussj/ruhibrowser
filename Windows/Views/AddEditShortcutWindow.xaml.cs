using System;
using System.Windows;
using System.Windows.Input;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class AddEditShortcutWindow : Window
    {
        private readonly Shortcut? _existing;
        public Shortcut? ResultShortcut { get; private set; }

        public AddEditShortcutWindow(Shortcut? existing = null)
        {
            InitializeComponent();
            _existing = existing;
            ThemeManager.ApplyThemeToWindow(this);

            if (existing != null)
            {
                TbDialogTitle.Text = "Edit Shortcut";
                Title = "Edit Shortcut - RUHI Browser";
                TbTitle.Text = existing.Title;
                TbUrl.Text = existing.Url;
            }

            Loaded += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(TbTitle.Text))
                {
                    TbTitle.Focus();
                }
                else
                {
                    TbUrl.Focus();
                    TbUrl.SelectAll();
                }
            };
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            var title = TbTitle.Text?.Trim() ?? string.Empty;
            var url = TbUrl.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter a valid URL.", "Shortcut Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbUrl.Focus();
                return;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                try
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    {
                        title = uri.Host.Replace("www.", "");
                    }
                    else
                    {
                        title = url;
                    }
                }
                catch
                {
                    title = url;
                }
            }

            ResultShortcut = new Shortcut(title, url, _existing?.FaviconUrl ?? string.Empty);
            DialogResult = true;
            Close();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
