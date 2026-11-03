using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Web.WebView2.Core;
using RuhiBrowser.Services;

namespace RuhiBrowser.Models
{
    public class DownloadItem : INotifyPropertyChanged
    {
        private long _bytesReceived;
        private long _totalBytes = -1;
        private double _progressPercentage;
        private string _speedText = string.Empty;
        private string _statusText = "Downloading...";
        private string _timeRemainingText = string.Empty;
        private bool _canPause = true;
        private bool _canResume;
        private bool _isCompleted;
        private bool _isFailed;
        private bool _isPaused;

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        public CoreWebView2DownloadOperation? Operation { get; set; }

        public long BytesReceived
        {
            get => _bytesReceived;
            set
            {
                if (_bytesReceived != value)
                {
                    _bytesReceived = value;
                    OnPropertyChanged(nameof(BytesReceived));
                    OnPropertyChanged(nameof(TransferredText));
                }
            }
        }

        public long TotalBytes
        {
            get => _totalBytes;
            set
            {
                if (_totalBytes != value)
                {
                    _totalBytes = value;
                    OnPropertyChanged(nameof(TotalBytes));
                    OnPropertyChanged(nameof(TransferredText));
                }
            }
        }

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set
            {
                if (Math.Abs(_progressPercentage - value) > 0.01)
                {
                    _progressPercentage = value;
                    OnPropertyChanged(nameof(ProgressPercentage));
                }
            }
        }

        public string SpeedText
        {
            get => _speedText;
            set
            {
                if (_speedText != value)
                {
                    _speedText = value;
                    OnPropertyChanged(nameof(SpeedText));
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string TimeRemainingText
        {
            get => _timeRemainingText;
            set
            {
                if (_timeRemainingText != value)
                {
                    _timeRemainingText = value;
                    OnPropertyChanged(nameof(TimeRemainingText));
                }
            }
        }

        public bool CanPause
        {
            get => _canPause;
            set
            {
                if (_canPause != value)
                {
                    _canPause = value;
                    OnPropertyChanged(nameof(CanPause));
                }
            }
        }

        public bool CanResume
        {
            get => _canResume;
            set
            {
                if (_canResume != value)
                {
                    _canResume = value;
                    OnPropertyChanged(nameof(CanResume));
                }
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                    OnPropertyChanged(nameof(IsPaused));
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public bool IsFailed
        {
            get => _isFailed;
            set
            {
                if (_isFailed != value)
                {
                    _isFailed = value;
                    OnPropertyChanged(nameof(IsFailed));
                }
            }
        }

        public string TransferredText
        {
            get
            {
                string rec = FormatBytes(BytesReceived);
                if (TotalBytes > 0)
                {
                    return $"{rec} / {FormatBytes(TotalBytes)}";
                }
                return rec;
            }
        }

        public string FileIcon
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(FilePath)) return "💾";
                    string ext = Path.GetExtension(FilePath).ToLowerInvariant();
                    return ext switch
                    {
                        ".crx" => "🧩",
                        ".pdf" => "📄",
                        ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "📦",
                        ".exe" or ".msi" or ".bat" or ".cmd" => "⚙️",
                        ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".svg" or ".bmp" => "🖼️",
                        ".mp4" or ".mkv" or ".avi" or ".mov" or ".webm" => "🎬",
                        ".mp3" or ".wav" or ".flac" or ".ogg" or ".m4a" => "🎵",
                        ".doc" or ".docx" or ".txt" or ".rtf" => "📝",
                        ".xls" or ".xlsx" or ".csv" => "📊",
                        ".ppt" or ".pptx" => "📽️",
                        ".html" or ".htm" or ".json" or ".xml" => "🌐",
                        _ => "💾"
                    };
                }
                catch
                {
                    return "💾";
                }
            }
        }

        public bool IsCrxExtension => !string.IsNullOrEmpty(FilePath) && FilePath.EndsWith(".crx", StringComparison.OrdinalIgnoreCase);

        public bool IsExtensionArchive
        {
            get
            {
                if (string.IsNullOrEmpty(FilePath)) return false;
                if (FilePath.EndsWith(".crx", StringComparison.OrdinalIgnoreCase)) return true;
                if (FilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(Url) && Url.Contains("addonruhi", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    return ExtensionManagerService.IsExtensionZip(FilePath);
                }
                return false;
            }
        }

        public void Pause()
        {
            if (Operation != null && !IsCompleted && !IsFailed)
            {
                try
                {
                    Operation.Pause();
                    IsPaused = true;
                    CanPause = false;
                    CanResume = true;
                    StatusText = "Paused";
                    SpeedText = string.Empty;
                }
                catch { }
            }
        }

        public void Resume()
        {
            if (Operation != null && !IsCompleted && !IsFailed)
            {
                try
                {
                    Operation.Resume();
                    IsPaused = false;
                    CanPause = true;
                    CanResume = false;
                    StatusText = "Downloading...";
                }
                catch { }
            }
        }

        public void Cancel()
        {
            if (Operation != null && !IsCompleted)
            {
                try
                {
                    Operation.Cancel();
                    IsFailed = true;
                    CanPause = false;
                    CanResume = false;
                    StatusText = "Cancelled";
                    SpeedText = string.Empty;
                }
                catch { }
            }
        }

        public void ShowInFolder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FilePath)) return;
                if (File.Exists(FilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{FilePath}\"");
                }
                else
                {
                    string dir = Path.GetDirectoryName(FilePath) ?? string.Empty;
                    if (Directory.Exists(dir))
                    {
                        Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
                    }
                }
            }
            catch { }
        }

        public void OpenFile()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath))
                {
                    Process.Start(new ProcessStartInfo { FileName = FilePath, UseShellExecute = true });
                }
            }
            catch { }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "0 B";
            if (bytes < 1024) return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return $"{kb:F1} KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return $"{mb:F1} MB";
            double gb = mb / 1024.0;
            return $"{gb:F2} GB";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
