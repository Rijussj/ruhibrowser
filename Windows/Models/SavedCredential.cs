using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RuhiBrowser.Models
{
    public class SavedCredential : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _websiteUrl = string.Empty;
        private string _websiteTitle = string.Empty;
        private string _username = string.Empty;
        private string _encryptedPassword = string.Empty;
        private DateTime _createdAt = DateTime.Now;
        private DateTime _lastUsedAt = DateTime.Now;

        private bool _isPasswordRevealed = false;
        private string _decryptedPassword = string.Empty;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string WebsiteUrl
        {
            get => _websiteUrl;
            set { _websiteUrl = value; OnPropertyChanged(); }
        }

        public string WebsiteTitle
        {
            get => _websiteTitle;
            set { _websiteTitle = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string EncryptedPassword
        {
            get => _encryptedPassword;
            set { _encryptedPassword = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        public DateTime LastUsedAt
        {
            get => _lastUsedAt;
            set { _lastUsedAt = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsPasswordRevealed
        {
            get => _isPasswordRevealed;
            set
            {
                _isPasswordRevealed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayPassword));
                OnPropertyChanged(nameof(RevealIcon));
            }
        }

        [JsonIgnore]
        public string DecryptedPassword
        {
            get => _decryptedPassword;
            set
            {
                _decryptedPassword = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayPassword));
            }
        }

        [JsonIgnore]
        public string DisplayPassword => IsPasswordRevealed ? _decryptedPassword : "••••••••••••";

        [JsonIgnore]
        public string RevealIcon => IsPasswordRevealed ? "🙈" : "👁";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
