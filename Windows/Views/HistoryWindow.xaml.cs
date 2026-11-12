using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class HistoryWindow : Window
    {
        private List<HistoryItem> _history = new();
        public string? SelectedUrl { get; private set; }

        public HistoryWindow()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);
            LoadHistory();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void LoadHistory()
        {
            _history = DataManager.LoadHistory();
            LbHistory.ItemsSource = null;
            LbHistory.ItemsSource = _history;
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                var item = _history.Find(h => h.Id == id);
                _history.RemoveAll(h => h.Id == id);
                DataManager.SaveHistory(_history);
                LoadHistory();
                if (item != null)
                {
                    _ = SyncService.DeleteHistoryItemAsync(item.Url, item.Id);
                }
            }
        }

        private void OnClearAllClicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all browsing history?",
                "Clear History",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                _history.Clear();
                DataManager.SaveHistory(_history);
                LoadHistory();
                _ = SyncService.ClearAllHistoryAsync();
            }
        }

        private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (LbHistory.SelectedItem is HistoryItem item)
            {
                SelectedUrl = item.Url;
                DialogResult = true;
                Close();
            }
        }
    }
}
