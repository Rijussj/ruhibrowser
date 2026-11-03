using System.Windows;
using System.Windows.Input;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class ClearDataDialog : Window
    {
        public bool ClearHistorySelected { get; private set; }
        public bool ClearCookiesSelected { get; private set; }
        public bool ClearCacheSelected { get; private set; }
        public bool ClearBookmarksSelected { get; private set; }
        public bool ClearPasswordsSelected { get; private set; }

        public ClearDataDialog()
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

        private void OnClearClicked(object sender, RoutedEventArgs e)
        {
            ClearHistorySelected = CbHistory.IsChecked == true;
            ClearCookiesSelected = CbCookies.IsChecked == true;
            ClearCacheSelected = CbCache.IsChecked == true;
            ClearBookmarksSelected = CbBookmarks.IsChecked == true;
            ClearPasswordsSelected = CbPasswords.IsChecked == true;

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
