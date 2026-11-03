namespace RuhiBrowser.Models
{
    public class Shortcut
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string FaviconUrl { get; set; } = string.Empty;

        public Shortcut() { }

        public Shortcut(string title, string url, string faviconUrl = "")
        {
            Title = title;
            Url = url;
            FaviconUrl = faviconUrl;
        }
    }
}
