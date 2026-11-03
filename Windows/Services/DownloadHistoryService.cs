using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RuhiBrowser.Models;

namespace RuhiBrowser.Services
{
    public class DownloadHistoryRecord
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public static class DownloadHistoryService
    {
        private static readonly string HistoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RuhiBrowser",
            "downloads_history.json"
        );

        public static List<DownloadHistoryRecord> LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    var json = File.ReadAllText(HistoryPath);
                    var list = JsonSerializer.Deserialize<List<DownloadHistoryRecord>>(json);
                    return list ?? new List<DownloadHistoryRecord>();
                }
            }
            catch { }
            return new List<DownloadHistoryRecord>();
        }

        public static void SaveHistory(List<DownloadHistoryRecord> records)
        {
            try
            {
                var dir = Path.GetDirectoryName(HistoryPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(records, options);
                File.WriteAllText(HistoryPath, json);
            }
            catch { }
        }

        public static void AddRecord(DownloadItem item)
        {
            try
            {
                var list = LoadHistory();
                list.RemoveAll(x => x.Id == item.Id || (x.FilePath == item.FilePath && File.Exists(item.FilePath)));
                list.Insert(0, new DownloadHistoryRecord
                {
                    Id = item.Id,
                    FileName = item.FileName,
                    FilePath = item.FilePath,
                    Url = item.Url,
                    TotalBytes = item.TotalBytes > 0 ? item.TotalBytes : item.BytesReceived,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime ?? DateTime.Now,
                    Status = item.StatusText
                });

                if (list.Count > 100)
                {
                    list = list.GetRange(0, 100);
                }

                SaveHistory(list);
            }
            catch { }
        }

        public static void ClearHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    File.Delete(HistoryPath);
                }
            }
            catch { }
        }
    }
}
