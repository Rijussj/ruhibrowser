using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RuhiBrowser.Models;
using RuhiBrowser.Services;

namespace RuhiBrowser.Views
{
    public partial class AiAssistantWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(35) };
        private readonly string _pageTitle;
        private readonly string _pageUrl;
        private readonly BrowserSettings _settings;

        public AiAssistantWindow(string pageTitle = "", string pageUrl = "")
        {
            InitializeComponent();
            _pageTitle = pageTitle;
            _pageUrl = pageUrl;
            _settings = DataManager.LoadSettings();
            ThemeManager.ApplyThemeToWindow(this, _settings.Theme);

            if (!string.IsNullOrWhiteSpace(_pageTitle) || !string.IsNullOrWhiteSpace(_pageUrl))
            {
                CbIncludePage.Content = $"Include page context: {Truncate(_pageTitle, 35)}";
                CbIncludePage.IsChecked = true;
            }
            else
            {
                CbIncludePage.Visibility = Visibility.Collapsed;
            }

            TbPrompt.Focus();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnPromptKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                OnSendClicked(sender, e);
            }
        }

        private async void OnSendClicked(object sender, RoutedEventArgs e)
        {
            string prompt = TbPrompt.Text.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
                return;

            BtnSend.IsEnabled = false;
            TbPrompt.IsEnabled = false;
            TbResponse.Text = "Thinking... connecting to AI...";
            BtnCopy.Visibility = Visibility.Collapsed;

            try
            {
                string fullPrompt = BuildPrompt(prompt);
                string response = await QueryAiAsync(fullPrompt);
                TbResponse.Text = response;
                BtnCopy.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                TbResponse.Text = $"Error: {ex.Message}";
            }
            finally
            {
                BtnSend.IsEnabled = true;
                TbPrompt.IsEnabled = true;
                TbPrompt.Focus();
            }
        }

        private string BuildPrompt(string userPrompt)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are RUHI AI Assistant, an intelligent assistant built into RUHI Browser created by RUHI Application.");
            
            if (CbIncludePage.IsChecked == true && (!string.IsNullOrEmpty(_pageTitle) || !string.IsNullOrEmpty(_pageUrl)))
            {
                sb.AppendLine("\n[Current Page Context]");
                if (!string.IsNullOrEmpty(_pageTitle)) sb.AppendLine($"Title: {_pageTitle}");
                if (!string.IsNullOrEmpty(_pageUrl)) sb.AppendLine($"URL: {_pageUrl}");
                sb.AppendLine();
            }

            sb.AppendLine($"User Request: {userPrompt}");
            return sb.ToString();
        }

        private async Task<string> QueryAiAsync(string fullPrompt)
        {
            if (string.IsNullOrWhiteSpace(_settings.AiApiKey))
            {
                return "Hugging Face API token is not configured yet.\n\n" +
                       "To enable live AI inference:\n" +
                       "1. Click 'Configure API' at the top-right.\n" +
                       "2. Enter your free Hugging Face API key (from huggingface.co/settings/tokens).\n" +
                       "3. Click Save and try again!\n\n" +
                       "Tip: You can also use online AI services directly in RUHI Browser tabs (e.g. Gemini, ChatGPT, Claude).";
            }

            string endpoint = string.IsNullOrWhiteSpace(_settings.AiModelEndpoint) || _settings.AiModelEndpoint.Contains("api-inference.huggingface.co")
                ? "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.2"
                : _settings.AiModelEndpoint;

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {_settings.AiApiKey.Trim()}");

            var payload = new
            {
                inputs = fullPrompt,
                parameters = new
                {
                    max_new_tokens = 512,
                    temperature = 0.7,
                    return_full_text = false
                }
            };

            string json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"AI API returned HTTP {(int)response.StatusCode}: {responseBody}";
            }

            try
            {
                var doc = JsonNode.Parse(responseBody);
                if (doc is JsonArray arr && arr.Count > 0)
                {
                    var first = arr[0];
                    if (first?["generated_text"] != null)
                    {
                        return first["generated_text"]!.ToString().Trim();
                    }
                }
                return responseBody;
            }
            catch
            {
                return responseBody;
            }
        }

        private void OnConfigureApiClicked(object sender, RoutedEventArgs e)
        {
            bool isDark = ThemeManager.IsDarkTheme(_settings.Theme);
            var configWindow = new Window
            {
                Title = "Configure AI API",
                Width = 460,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#0F172A" : "#FFFFFF")
                ),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#F8FAFC" : "#0F172A")
                ),
                ResizeMode = ResizeMode.NoResize
            };

            var stack = new System.Windows.Controls.StackPanel { Margin = new Thickness(16) };
            
            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Hugging Face User Access Token:",
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#475569")
                ),
                Margin = new Thickness(0, 0, 0, 4)
            });

            var tbKey = new System.Windows.Controls.TextBox
            {
                Text = _settings.AiApiKey,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#383838" : "#F1F5F9")
                ),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A")
                ),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#3C4043" : "#CBD5E1")
                ),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 0, 0, 12)
            };
            stack.Children.Add(tbKey);

            stack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Model Endpoint URL:",
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#475569")
                ),
                Margin = new Thickness(0, 0, 0, 4)
            });

            var tbEndpoint = new System.Windows.Controls.TextBox
            {
                Text = string.IsNullOrWhiteSpace(_settings.AiModelEndpoint) || _settings.AiModelEndpoint.Contains("api-inference.huggingface.co")
                    ? "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.2"
                    : _settings.AiModelEndpoint,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#383838" : "#F1F5F9")
                ),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A")
                ),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(isDark ? "#3C4043" : "#CBD5E1")
                ),
                Padding = new Thickness(6),
                Margin = new Thickness(0, 0, 0, 16)
            };
            stack.Children.Add(tbEndpoint);

            var btnSave = new System.Windows.Controls.Button
            {
                Content = "Save Settings",
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 99, 235)),
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new Thickness(14, 6, 14, 6),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            btnSave.Click += (s, args) =>
            {
                _settings.AiApiKey = tbKey.Text.Trim();
                _settings.AiModelEndpoint = tbEndpoint.Text.Trim();
                DataManager.SaveSettings(_settings);
                configWindow.Close();
                MessageBox.Show(this, "AI Settings saved successfully!", "RUHI AI", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            stack.Children.Add(btnSave);

            configWindow.Content = stack;
            configWindow.ShowDialog();
        }

        private void OnCopyClicked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TbResponse.Text))
            {
                Clipboard.SetText(TbResponse.Text);
                MessageBox.Show(this, "Response copied to clipboard!", "RUHI AI", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static string Truncate(string val, int maxLen)
        {
            if (string.IsNullOrEmpty(val)) return string.Empty;
            return val.Length <= maxLen ? val : val.Substring(0, maxLen) + "...";
        }
    }
}
