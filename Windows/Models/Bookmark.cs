using System;

namespace RuhiBrowser.Models
{
    public class Bookmark
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Bookmark() { }

        public Bookmark(string title, string url)
        {
            Title = title;
            Url = url;
            CreatedAt = DateTime.Now;
        }
    }
}
