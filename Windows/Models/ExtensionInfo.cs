using System;
using System.ComponentModel;
using Microsoft.Web.WebView2.Core;

namespace RuhiBrowser.Models
{
    public class ExtensionInfo : INotifyPropertyChanged
    {
        private bool _isEnabled;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string? IconPath { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string StatusText => IsEnabled ? "Active" : "Disabled";

        public CoreWebView2BrowserExtension? CoreExtension { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
