using System;

namespace RuhiBrowser.Models
{
    public class HistoryItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime VisitedAt { get; set; } = DateTime.Now;
        public int VisitCount { get; set; } = 1;

        public HistoryItem() { }

        public HistoryItem(string title, string url)
        {
            Title = title;
            Url = url;
            VisitedAt = DateTime.Now;
            VisitCount = 1;
        }
    }
}
