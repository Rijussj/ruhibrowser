using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public class LanguageOption
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public LanguageOption(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }

    public partial class TranslateWindow : Window
    {
        public string? SelectedLanguageCode { get; private set; }

        public TranslateWindow()
        {
            InitializeComponent();
            ThemeManager.ApplyThemeToWindow(this);
            LoadLanguages();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void LoadLanguages()
        {
            var languages = new List<LanguageOption>
            {
                new("Hindi (हिन्दी)", "hi"),
                new("Bengali (বাংলা)", "bn"),
                new("Marathi (मराठी)", "mr"),
                new("Telugu (తెలుగు)", "te"),
                new("Tamil (தமிழ்)", "ta"),
                new("Gujarati (ગુજરાતી)", "gu"),
                new("Urdu (اردو)", "ur"),
                new("Punjabi (ਪੰਜਾਬੀ)", "pa"),
                new("English", "en"),
                new("Arabic (العربية)", "ar"),
                new("Spanish (Español)", "es"),
                new("French (Français)", "fr"),
                new("German (Deutsch)", "de"),
                new("Russian (Русский)", "ru"),
                new("Japanese (日本語)", "ja"),
                new("Chinese (中文)", "zh-CN")
            };

            LbLanguages.ItemsSource = languages;
            LbLanguages.SelectedIndex = 0;
        }

        private void OnTranslateClicked(object sender, RoutedEventArgs e)
        {
            if (LbLanguages.SelectedItem is LanguageOption opt)
            {
                SelectedLanguageCode = opt.Code;
                DialogResult = true;
                Close();
            }
        }

        private void OnLanguageDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            OnTranslateClicked(sender, e);
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
