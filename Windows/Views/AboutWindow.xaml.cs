using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnEmailClicked(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("mailto:myjawla@gmail.com");
        }

        private void OnLicenseChromiumClicked(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://chromium.googlesource.com/chromium/src/");
        }

        private void OnLicenseDotNetClicked(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://github.com/dotnet");
        }

        private void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch { }
                return;
            }

            MainWindow? mainWin = Owner as MainWindow;
            if (mainWin == null && Owner is Window ownerWin)
            {
                mainWin = ownerWin.Owner as MainWindow;
            }
            if (mainWin == null)
            {
                foreach (Window w in Application.Current.Windows)
                {
                    if (w is MainWindow mw)
                    {
                        mainWin = mw;
                        break;
                    }
                }
            }

            if (mainWin != null)
            {
                mainWin.Dispatcher.Invoke(() =>
                {
                    mainWin.OpenUrlInNewTab(url);
                    mainWin.Activate();
                });
                Close();
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
    }
}
