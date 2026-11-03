using System;

namespace RuhiBrowser.Models
{
    public class TabGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "Group";
        public string ColorHex { get; set; } = "#3B82F6";
        public bool IsCollapsed { get; set; }

        public static readonly (string Name, string Hex)[] ChromeColors = new[]
        {
            ("Blue", "#3B82F6"),
            ("Red", "#EF4444"),
            ("Yellow", "#EAB308"),
            ("Green", "#10B981"),
            ("Pink", "#EC4899"),
            ("Purple", "#8B5CF6"),
            ("Cyan", "#06B6D4"),
            ("Orange", "#F97316"),
            ("Gray", "#64748B")
        };
    }
}
