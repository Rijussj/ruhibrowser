using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Web.WebView2.Wpf;

namespace RuhiBrowser.Models
{
    public class BrowserTab : INotifyPropertyChanged
    {
        private string _title = "New Tab";
        private string _url = string.Empty;
        private bool _isLoading;
        private bool _isSelected;

        public string Id { get; } = Guid.NewGuid().ToString();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Url
        {
            get => _url;
            set { _url = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public bool IsIncognito { get; set; }
        public bool IsPinned { get; set; }
        public bool IsMuted { get; set; }
        public bool IsSleeping { get; set; }
        public DateTime LastActiveTime { get; set; } = DateTime.Now;
        public int BlockedAdsCount { get; set; }
        public string? GroupId { get; set; }
        public string? TempUserDataFolder { get; set; }

        public WebView2? WebView { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
