using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class BookmarksWindow : Window
    {
        private List<Bookmark> _bookmarks = new();
        public string? SelectedUrl { get; private set; }

        public BookmarksWindow()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);
            LoadBookmarks();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void LoadBookmarks()
        {
            _bookmarks = DataManager.LoadBookmarks();
            LbBookmarks.ItemsSource = null;
            LbBookmarks.ItemsSource = _bookmarks;
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string id)
            {
                var bm = _bookmarks.Find(b => b.Id == id);
                _bookmarks.RemoveAll(b => b.Id == id);
                DataManager.SaveBookmarks(_bookmarks);
                LoadBookmarks();
                if (bm != null && SyncService.IsLoggedIn)
                {
                    _ = SyncService.DeleteBookmarkAsync(bm.Url);
                }
            }
        }

        private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (LbBookmarks.SelectedItem is Bookmark b)
            {
                SelectedUrl = b.Url;
                DialogResult = true;
                Close();
            }
        }
    }
}
