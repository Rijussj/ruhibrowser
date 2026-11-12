using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using RuhiBrowser.Models;
using RuhiBrowser.Services;
using RuhiBrowser.Views;

namespace RuhiBrowser
{
    public partial class MainWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint WM_NCPAINT = 0x0085;

        private const int WM_NCHITTEST = 0x0084;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        private static readonly string DefaultUserDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RuhiBrowser",
            "WebView2Data"
        );
        private static CoreWebView2Environment? _sharedEnvironment;

        private readonly List<BrowserTab> _tabs = new();
        private readonly Stack<string> _recentlyClosedTabs = new();
        private readonly List<DownloadItem> _downloads = new();
        private readonly Dictionary<string, (long LastBytes, DateTime LastTime)> _speedTrackers = new();
        private readonly List<TabGroup> _tabGroups = new();
        private BrowserTab? _activeTab;
        private BrowserSettings _settings;
        private List<Shortcut> _shortcuts = new();
        private List<Bookmark> _bookmarks = new();

        public MainWindow()
        {
            InitializeComponent();

            _settings = DataManager.LoadSettings();
            _bookmarks = DataManager.LoadBookmarks();
            _shortcuts = DataManager.LoadShortcuts();
            try { _tabGroups.AddRange(DataManager.LoadTabGroups()); } catch { }

            AdBlockManager.IsEnabled = _settings.AdBlockEnabled;

            ApplyTheme();
            DefaultBrowserHelper.RegisterAsBrowser();
            InitMemorySaverTimer();
            InitSyncService();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ApplyNativeWindowTheme(IsCurrentThemeDark());

            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    var source = HwndSource.FromHwnd(hwnd);
                    source?.AddHook(WndProc);
                }
            }
            catch { }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();

            try
            {
                var history = DownloadHistoryService.LoadHistory();
                foreach (var record in history)
                {
                    _downloads.Add(new DownloadItem
                    {
                        Id = record.Id,
                        FileName = record.FileName,
                        FilePath = record.FilePath,
                        Url = record.Url,
                        TotalBytes = record.TotalBytes,
                        BytesReceived = record.TotalBytes,
                        ProgressPercentage = 100,
                        IsCompleted = string.Equals(record.Status, "Completed", StringComparison.OrdinalIgnoreCase),
                        IsFailed = !string.Equals(record.Status, "Completed", StringComparison.OrdinalIgnoreCase),
                        StatusText = record.Status,
                        StartTime = record.StartTime,
                        EndTime = record.EndTime
                    });
                }
            }
            catch { }

            if (_tabs.Count == 0)
            {
                string? initialTarget = null;
                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
                {
                    var resolved = UrlResolver.Resolve(args[1]);
                    if (!string.IsNullOrWhiteSpace(resolved))
                    {
                        initialTarget = resolved;
                    }
                }

                CreateNewTab(initialTarget);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Execute after window is physically presented on screen
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                TriggerNativeFrameRecalculation();
            }));
        }

        public void HandleExternalUrl(string? rawUrl)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();

                if (!string.IsNullOrWhiteSpace(rawUrl))
                {
                    var resolved = UrlResolver.Resolve(rawUrl);
                    CreateNewTab(resolved);
                }
                else
                {
                    CreateNewTab();
                }
            }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            try
            {
                _clockTimer?.Stop();
                _pomodoroTimer?.Stop();
                _wallpaperTimer?.Stop();
                DataManager.SaveTabGroups(_tabGroups);
            }
            catch { }

            foreach (var tab in _tabs.ToList())
            {
                try
                {
                    tab.WebView?.Dispose();
                    tab.WebView = null;
                }
                catch { }
            }

            Task.Run(() =>
            {
                try
                {
                    var tempDir = Path.GetTempPath();
                    var dirs = Directory.GetDirectories(tempDir, "RuhiBrowser_InPrivate_*");
                    foreach (var d in dirs)
                    {
                        try { Directory.Delete(d, true); } catch { }
                    }
                }
                catch { }
            });
        }

        public bool IsCurrentThemeDark()
        {
            var theme = _settings.Theme ?? "Dark";
            if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (string.Equals(theme, "System", StringComparison.OrdinalIgnoreCase))
            {
                return !IsWindowsInLightMode();
            }
            return true;
        }

        private static bool IsWindowsInLightMode()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var val = key?.GetValue("AppsUseLightTheme");
                if (val is int intVal && intVal == 1)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public void ApplyTheme()
        {
            bool isDark = IsCurrentThemeDark();

            var bgWindow = (Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F1F5F9");
            var fgWindow = (Color)ColorConverter.ConvertFromString(isDark ? "#F8FAFC" : "#0F172A");

            var bgTabStrip = (Color)ColorConverter.ConvertFromString(isDark ? "#1F1F1F" : "#E2E8F0");
            var borderTabStrip = (Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1");

            var bgToolbar = (Color)ColorConverter.ConvertFromString(isDark ? "#28292C" : "#FFFFFF");
            var borderToolbar = (Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1");

            var bgOmnibox = (Color)ColorConverter.ConvertFromString(isDark ? "#202124" : "#F8FAFC");
            var borderOmnibox = (Color)ColorConverter.ConvertFromString(isDark ? "#3C4043" : "#CBD5E1");
            var fgOmnibox = (Color)ColorConverter.ConvertFromString(isDark ? "#F8FAFC" : "#0F172A");

            var bgCard = (Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF");
            var borderCard = (Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1");
            var textMuted = (Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B");

            Background = new SolidColorBrush(bgWindow);
            Foreground = new SolidColorBrush(fgWindow);

            if (BorderTabStrip != null)
            {
                BorderTabStrip.Background = new SolidColorBrush(bgTabStrip);
                BorderTabStrip.BorderBrush = new SolidColorBrush(borderTabStrip);
            }

            if (BorderNavToolbar != null)
            {
                BorderNavToolbar.Background = new SolidColorBrush(bgToolbar);
                BorderNavToolbar.BorderBrush = new SolidColorBrush(borderToolbar);
            }

            if (BorderOmnibox != null)
            {
                BorderOmnibox.Background = new SolidColorBrush(bgOmnibox);
                BorderOmnibox.BorderBrush = new SolidColorBrush(borderOmnibox);
            }

            if (BorderUrlSuggestions != null)
            {
                BorderUrlSuggestions.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF"));
                BorderUrlSuggestions.BorderBrush = new SolidColorBrush(borderOmnibox);
            }

            if (TbUrl != null)
            {
                TbUrl.Foreground = new SolidColorBrush(fgOmnibox);
            }

            if (HomeScreen != null)
            {
                HomeScreen.Background = new SolidColorBrush(bgWindow);
            }

            if (TbHomeTitle != null)
            {
                TbHomeTitle.Foreground = new SolidColorBrush(fgWindow);
            }

            if (TbHomeSubtitle != null)
            {
                TbHomeSubtitle.Foreground = new SolidColorBrush(textMuted);
            }

            if (BorderHomeSearch != null)
            {
                BorderHomeSearch.Background = new SolidColorBrush(isDark ? (Color)ColorConverter.ConvertFromString("#202124") : Colors.White);
                BorderHomeSearch.BorderBrush = new SolidColorBrush(borderOmnibox);
            }

            if (TbHomeSearch != null)
            {
                TbHomeSearch.Foreground = new SolidColorBrush(fgWindow);
            }

            if (BorderRecentVisits != null)
            {
                BorderRecentVisits.Background = new SolidColorBrush(bgCard);
                BorderRecentVisits.BorderBrush = new SolidColorBrush(borderCard);
            }

            if (TbRecentVisitsTitle != null)
            {
                TbRecentVisitsTitle.Foreground = new SolidColorBrush(textMuted);
            }

            if (TbShortcutsTitle != null)
            {
                TbShortcutsTitle.Foreground = new SolidColorBrush(textMuted);
            }

            var btnBrush = new SolidColorBrush(textMuted);
            if (BtnBack != null) BtnBack.Foreground = btnBrush;
            if (BtnForward != null) BtnForward.Foreground = btnBrush;
            if (BtnReload != null) BtnReload.Foreground = btnBrush;
            if (BtnHome != null) BtnHome.Foreground = btnBrush;
            if (BtnAi != null) BtnAi.Foreground = btnBrush;
            if (BtnTranslate != null) BtnTranslate.Foreground = btnBrush;
            if (BtnReader != null) BtnReader.Foreground = btnBrush;
            if (BtnDownloads != null) BtnDownloads.Foreground = btnBrush;
            if (TbDownloadIcon != null) TbDownloadIcon.Foreground = btnBrush;
            if (BtnExtensions != null) BtnExtensions.Foreground = btnBrush;
            if (BtnMenu != null) BtnMenu.Foreground = btnBrush;
            if (BtnNewTab != null) BtnNewTab.Foreground = btnBrush;
            if (BtnWindowMinimize != null) BtnWindowMinimize.Foreground = btnBrush;
            if (BtnWindowMaximize != null) BtnWindowMaximize.Foreground = btnBrush;
            if (BtnWindowClose != null) BtnWindowClose.Foreground = btnBrush;

            if (BorderDownloadsPopup != null)
            {
                var bgPopup = (Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF");
                var borderPopup = (Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1");
                var bgFooter = (Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F8FAFC");
                BorderDownloadsPopup.Background = new SolidColorBrush(bgPopup);
                BorderDownloadsPopup.BorderBrush = new SolidColorBrush(borderPopup);
                if (BorderDownloadsHeader != null) BorderDownloadsHeader.BorderBrush = new SolidColorBrush(borderPopup);
                if (BorderDownloadsFooter != null)
                {
                    BorderDownloadsFooter.Background = new SolidColorBrush(bgFooter);
                    BorderDownloadsFooter.BorderBrush = new SolidColorBrush(borderPopup);
                }
                if (TbDownloadsHeaderTitle != null) TbDownloadsHeaderTitle.Foreground = new SolidColorBrush(fgWindow);
                if (TbDownloadsHeaderCount != null) TbDownloadsHeaderCount.Foreground = new SolidColorBrush(textMuted);
                if (TbDownloadsEmptyTitle != null) TbDownloadsEmptyTitle.Foreground = new SolidColorBrush(textMuted);
                if (TbDownloadsFooterStatus != null) TbDownloadsFooterStatus.Foreground = new SolidColorBrush(textMuted);
            }

            Resources["MenuBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF"));
            Resources["MenuBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#3C4043" : "#CBD5E1"));
            Resources["MenuTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            Resources["MenuHoverBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#F1F5F9"));
            Resources["MenuSepBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));

            ApplyNativeWindowTheme(isDark);
            RenderTabHeaders();
            RenderHomeScreenShortcuts();
            RenderHomeScreenRecentVisits();
            ApplyHomeScreenStyle();

            var scheme = isDark ? CoreWebView2PreferredColorScheme.Dark : CoreWebView2PreferredColorScheme.Light;
            foreach (var tab in _tabs)
            {
                if (tab.WebView?.CoreWebView2?.Profile != null)
                {
                    tab.WebView.CoreWebView2.Profile.PreferredColorScheme = scheme;
                }
            }
        }

        private void ApplyNativeWindowTheme(bool isDark = true)
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    int darkMode = isDark ? 1 : 0;
                    DwmSetWindowAttribute(hwnd, 20, ref darkMode, sizeof(int));
                    DwmSetWindowAttribute(hwnd, 19, ref darkMode, sizeof(int));
                    SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER);
                }
            }
            catch { }
        }

        private void TriggerNativeFrameRecalculation()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero && GetWindowRect(hwnd, out RECT rect))
                {
                    int w = rect.Right - rect.Left;
                    int h = rect.Bottom - rect.Top;

                    ApplyNativeWindowTheme(IsCurrentThemeDark());

                    // Physical Win32 window size nudge (1 pixel) forces Windows DWM to send WM_NCCALCSIZE and paint caption buttons
                    SetWindowPos(hwnd, IntPtr.Zero, rect.Left, rect.Top, w + 1, h, SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
                    SetWindowPos(hwnd, IntPtr.Zero, rect.Left, rect.Top, w, h, SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);

                    SendMessage(hwnd, WM_NCPAINT, (IntPtr)1, IntPtr.Zero);
                }
            }
            catch { }
        }

        private void OnMinimizeClicked(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClicked(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                BtnWindowMaximize.Content = "🗖";
            }
            else
            {
                WindowState = WindowState.Maximized;
                BtnWindowMaximize.Content = "🗗";
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (BtnWindowMaximize != null)
            {
                BtnWindowMaximize.Content = WindowState == WindowState.Maximized ? "🗗" : "🗖";
            }

            if (WebContainer != null)
            {
                WebContainer.Margin = WindowState == WindowState.Maximized
                    ? new Thickness(0)
                    : new Thickness(4, 0, 4, 4);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCHITTEST && WindowState == WindowState.Normal)
            {
                try
                {
                    int screenX = unchecked((short)(long)lParam);
                    int screenY = unchecked((short)((long)lParam >> 16));

                    var windowPoint = PointFromScreen(new Point(screenX, screenY));

                    double width = ActualWidth;
                    double height = ActualHeight;

                    if (width > 0 && height > 0)
                    {
                        const double border = 6.0;

                        bool isLeft = windowPoint.X <= border && windowPoint.X >= -border;
                        bool isRight = windowPoint.X >= width - border && windowPoint.X <= width + border;
                        bool isTop = windowPoint.Y <= border && windowPoint.Y >= -border;
                        bool isBottom = windowPoint.Y >= height - border && windowPoint.Y <= height + border;

                        if (isTop && isLeft) { handled = true; return (IntPtr)HTTOPLEFT; }
                        if (isTop && isRight) { handled = true; return (IntPtr)HTTOPRIGHT; }
                        if (isBottom && isLeft) { handled = true; return (IntPtr)HTBOTTOMLEFT; }
                        if (isBottom && isRight) { handled = true; return (IntPtr)HTBOTTOMRIGHT; }
                        if (isLeft) { handled = true; return (IntPtr)HTLEFT; }
                        if (isRight) { handled = true; return (IntPtr)HTRIGHT; }
                        if (isTop) { handled = true; return (IntPtr)HTTOP; }
                        if (isBottom) { handled = true; return (IntPtr)HTBOTTOM; }
                    }
                }
                catch { }
            }

            return IntPtr.Zero;
        }

        private void OnTabStripPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Do not drag if user clicked directly on any Button
            if (e.OriginalSource is DependencyObject dep)
            {
                var parent = dep;
                while (parent != null && parent != sender)
                {
                    if (parent is Button) return;
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    try
                    {
                        DragMove();
                    }
                    catch { }
                }
            }
        }

        private void OnTabScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (TabScrollViewer != null)
            {
                TabScrollViewer.ScrollToHorizontalOffset(TabScrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.WidthChanged && _tabs.Count > 5)
            {
                RenderTabHeaders();
            }
        }

        #region Keyboard Shortcuts

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.T:
                        e.Handled = true;
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            ReopenClosedTab();
                        else
                            CreateNewTab();
                        break;
                    case Key.W:
                        e.Handled = true;
                        if (_activeTab != null) CloseTab(_activeTab);
                        break;
                    case Key.F:
                        e.Handled = true;
                        ShowFindInPage();
                        break;
                    case Key.R:
                        e.Handled = true;
                        OnReloadClicked(this, new RoutedEventArgs());
                        break;
                    case Key.H:
                        e.Handled = true;
                        OpenHistoryWindow();
                        break;
                    case Key.B:
                        e.Handled = true;
                        OpenBookmarksWindow();
                        break;
                    case Key.J:
                        e.Handled = true;
                        OnDownloadsClicked(this, new RoutedEventArgs());
                        break;
                    case Key.L:
                        e.Handled = true;
                        TbUrl.Focus();
                        TbUrl.SelectAll();
                        break;
                    case Key.C:
                        if (!TbUrl.IsFocused && _activeTab?.WebView?.CoreWebView2 != null)
                        {
                            CopySelectedTextAsync(_activeTab.WebView);
                        }
                        break;
                    case Key.O:
                        e.Handled = true;
                        OpenFilePrompt();
                        break;
                    case Key.S:
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        {
                            e.Handled = true;
                            OnSplitScreenClicked(this, new RoutedEventArgs());
                        }
                        break;
                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.F12:
                        e.Handled = true;
                        OpenDevTools();
                        break;
                    case Key.F5:
                        e.Handled = true;
                        OnReloadClicked(this, new RoutedEventArgs());
                        break;
                    case Key.F3:
                        e.Handled = true;
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                            FindPrevious();
                        else
                            FindNext();
                        break;
                    case Key.Escape:
                        if (FindInPageBar.Visibility == Visibility.Visible)
                        {
                            e.Handled = true;
                            CloseFindInPage();
                        }
                        break;
                }
            }
        }

        private void OpenFilePrompt()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Open File - RUHI Browser",
                    Filter = "All Supported Files (*.pdf;*.html;*.htm;*.png;*.jpg;*.jpeg;*.webp;*.gif;*.svg;*.mp4;*.webm;*.mp3;*.txt)|*.pdf;*.html;*.htm;*.png;*.jpg;*.jpeg;*.webp;*.gif;*.svg;*.mp4;*.webm;*.mp3;*.txt|PDF Documents (*.pdf)|*.pdf|Image Files (*.png;*.jpg;*.jpeg;*.webp;*.gif;*.svg)|*.png;*.jpg;*.jpeg;*.webp;*.gif;*.svg|Media Files (*.mp4;*.webm;*.mp3;*.wav)|*.mp4;*.webm;*.mp3;*.wav|Web & Text Files (*.html;*.htm;*.txt)|*.html;*.htm;*.txt|All Files (*.*)|*.*",
                    Multiselect = true
                };

                if (dlg.ShowDialog(this) == true && dlg.FileNames != null && dlg.FileNames.Length > 0)
                {
                    for (int i = 0; i < dlg.FileNames.Length; i++)
                    {
                        var target = UrlResolver.Resolve(dlg.FileNames[i]);
                        if (i == 0 && _activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
                        {
                            NavigateTab(_activeTab, target);
                        }
                        else
                        {
                            CreateNewTab(target);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open file: " + ex.Message, "Open File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Drag and Drop (Files, PDFs, Images)

        private void OnWindowDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnWindowDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                {
                    e.Handled = true;
                    for (int i = 0; i < files.Length; i++)
                    {
                        var target = UrlResolver.Resolve(files[i]);
                        if (i == 0 && _activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
                        {
                            NavigateTab(_activeTab, target);
                        }
                        else
                        {
                            CreateNewTab(target);
                        }
                    }
                }
            }
        }

        private void OnUrlPreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void OnUrlDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0 && _activeTab != null)
                {
                    e.Handled = true;
                    var target = UrlResolver.Resolve(files[0]);
                    NavigateTab(_activeTab, target);
                }
            }
        }

        #endregion

        #region Tab Management

        public void OpenUrlInNewTab(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            CreateNewTab(url);
        }

        private BrowserTab CreateNewTab(string? url = null, bool isIncognito = false, string? groupId = null)
        {
            _settings = DataManager.LoadSettings();

            if (url == null && !isIncognito)
            {
                var mode = _settings.HomepageMode ?? "Home";
                if (string.Equals(mode, "Extension", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_settings.ActiveExtensionNewTabUrl))
                {
                    url = _settings.ActiveExtensionNewTabUrl;
                }
                else if (string.Equals(mode, "Google", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://www.google.com";
                }
                else if (string.Equals(mode, "Blank", StringComparison.OrdinalIgnoreCase))
                {
                    url = "about:blank";
                }
                else if (string.Equals(mode, "Custom", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_settings.CustomHomepageUrl))
                {
                    url = _settings.CustomHomepageUrl;
                }
            }

            var tab = new BrowserTab
            {
                Title = string.IsNullOrWhiteSpace(url) ? "New Tab" : url,
                Url = url ?? string.Empty,
                IsIncognito = isIncognito,
                GroupId = groupId
            };

            // Setup WebView2 control for this tab
            var webView = new WebView2();
            tab.WebView = webView;

            InitializeWebView(tab, webView);

            if (!string.IsNullOrEmpty(groupId))
            {
                int lastGroupIndex = _tabs.FindLastIndex(t => t.GroupId == groupId);
                if (lastGroupIndex >= 0)
                {
                    _tabs.Insert(lastGroupIndex + 1, tab);
                }
                else
                {
                    _tabs.Add(tab);
                }
            }
            else
            {
                _tabs.Add(tab);
            }

            RenderTabHeaders();
            SwitchToTab(tab);

            if (!string.IsNullOrWhiteSpace(url))
            {
                NavigateTab(tab, url);
            }

            return tab;
        }

        private async void InitializeWebView(BrowserTab tab, WebView2 webView)
        {
            try
            {
                CoreWebView2Environment env;
                var envOptions = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = "--enable-features=msPdfAnnotations,msPdfDraw,msPdfAddText,msPdfTwoPageSpread,msPdfOcr,msWebView2BrowserExtension --enable-extensions",
                    AreBrowserExtensionsEnabled = true
                };

                if (tab.IsIncognito)
                {
                    string incognitoDir = Path.Combine(Path.GetTempPath(), "RuhiBrowser_InPrivate_" + Guid.NewGuid().ToString("N"));
                    tab.TempUserDataFolder = incognitoDir;
                    env = await CoreWebView2Environment.CreateAsync(userDataFolder: incognitoDir, options: envOptions);
                }
                else
                {
                    if (_sharedEnvironment == null)
                    {
                        if (!Directory.Exists(DefaultUserDataFolder))
                        {
                            try { Directory.CreateDirectory(DefaultUserDataFolder); } catch { }
                        }
                        _sharedEnvironment = await CoreWebView2Environment.CreateAsync(userDataFolder: DefaultUserDataFolder, options: envOptions);
                    }
                    env = _sharedEnvironment;
                }

                await webView.EnsureCoreWebView2Async(env);

                if (!tab.IsIncognito)
                {
                    try
                    {
                        await ExtensionManagerService.EnsureAllExtensionsLoadedAsync(webView.CoreWebView2.Profile);
                        var overrideInfo = await ExtensionManagerService.GetActiveNewTabOverrideAsync(webView.CoreWebView2.Profile);
                        if (!string.IsNullOrEmpty(overrideInfo.Url))
                        {
                            if (string.IsNullOrEmpty(_settings.ActiveExtensionNewTabUrl) || _settings.HomepageMode == "Extension")
                            {
                                _settings.ActiveExtensionNewTabUrl = overrideInfo.Url;
                                _settings.ActiveExtensionNewTabName = overrideInfo.Name ?? string.Empty;
                                DataManager.SaveSettings(_settings);
                            }
                        }
                    }
                    catch { }
                }

                // Enable all PDF toolbar items, DevTools and default context menus
                webView.CoreWebView2.Settings.HiddenPdfToolbarItems = CoreWebView2PdfToolbarItems.None;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

                // Configure clean RUHI Browser User-Agent (strips 'Edg/...' so websites detect RUHI Browser instead of Microsoft Edge)
                try
                {
                    string currentUa = webView.CoreWebView2.Settings.UserAgent;
                    if (!string.IsNullOrWhiteSpace(currentUa) && currentUa.Contains("Edg/"))
                    {
                        webView.CoreWebView2.Settings.UserAgent = System.Text.RegularExpressions.Regex.Replace(currentUa, @"\s*Edg\/\S+", "") + " RuhiBrowser/1.0";
                    }
                    else if (string.IsNullOrWhiteSpace(currentUa))
                    {
                        webView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 RuhiBrowser/1.0";
                    }
                }
                catch { }

                // Auto-allow Clipboard permissions
                webView.CoreWebView2.PermissionRequested += (s, args) =>
                {
                    if (args.PermissionKind == CoreWebView2PermissionKind.ClipboardRead)
                    {
                        args.State = CoreWebView2PermissionState.Allow;
                        args.Handled = true;
                    }
                };

                // If tab was created with a specific URL (e.g. extension page or website), navigate now that CoreWebView2 is ready
                if (!string.IsNullOrWhiteSpace(tab.Url) && tab.Url != "about:blank")
                {
                    try
                    {
                        webView.CoreWebView2.Navigate(tab.Url);
                    }
                    catch { }
                }

                // Unlock text selection and copy across all websites
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(UnlockCopyScript);

                // Enable Edge-style Picture-in-Picture button on all video elements
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(EdgePipScript);

                // Enable Edge-style Chrome Web Store bridge
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(ChromeWebStoreBridgeScript);

                // Intercept in-page custom protocol clicks (e.g. ruhi://extension)
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(RuhiProtocolInterceptorScript);

                // Handle external URI scheme launch (e.g. ruhi://...)
                webView.CoreWebView2.LaunchingExternalUriScheme += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Uri) && IsExtensionsProtocolUrl(e.Uri))
                    {
                        e.Cancel = true;
                        Dispatcher.BeginInvoke(new Action(() => OpenExtensionsWindow()));
                    }
                };

                // Native CoreWebView2 NavigationStarting
                webView.CoreWebView2.NavigationStarting += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Uri) && IsExtensionsProtocolUrl(e.Uri))
                    {
                        e.Cancel = true;
                        Dispatcher.BeginInvoke(new Action(() => OpenExtensionsWindow()));
                    }
                };

                webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                // Sync Zoom factor changes to UI badge
                webView.ZoomFactorChanged += (s, e) =>
                {
                    if (_activeTab == tab)
                    {
                        UpdateZoomBadge();
                    }
                };

                // Set PreferredColorScheme according to active Theme
                bool isDark = IsCurrentThemeDark();
                webView.CoreWebView2.Profile.PreferredColorScheme = isDark
                    ? CoreWebView2PreferredColorScheme.Dark
                    : CoreWebView2PreferredColorScheme.Light;

                // Configure AdBlocker request interception
                webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                webView.CoreWebView2.WebResourceRequested += (s, e) =>
                {
                    if (AdBlockManager.IsEnabled && Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var uri))
                    {
                        string? pageHost = null;
                        try
                        {
                            if (!string.IsNullOrEmpty(tab.Url) && Uri.TryCreate(tab.Url, UriKind.Absolute, out var pageUri))
                            {
                                pageHost = pageUri.Host;
                            }
                        }
                        catch { }

                        if (AdBlockManager.ShouldBlock(uri, pageHost))
                        {
                            e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(
                                null, 403, "Blocked by RUHI AdBlocker", "Content-Type: text/plain"
                            );

                            tab.BlockedAdsCount++;
                            AdBlockManager.TotalBlockedCount++;

                            if (tab == _activeTab)
                            {
                                Dispatcher.BeginInvoke(new Action(UpdateShieldBadge));
                            }
                        }
                    }
                };

                // Navigation Events
                webView.NavigationStarting += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Uri) && IsExtensionsProtocolUrl(e.Uri))
                    {
                        e.Cancel = true;
                        Dispatcher.BeginInvoke(new Action(() => OpenExtensionsWindow()));
                        return;
                    }

                    tab.IsLoading = true;
                    tab.BlockedAdsCount = 0;
                    tab.LastActiveTime = DateTime.Now;
                    if (_activeTab == tab)
                    {
                        PbLoading.Visibility = Visibility.Visible;
                        Dispatcher.BeginInvoke(new Action(UpdateShieldBadge));
                    }
                };

                webView.NavigationCompleted += (s, e) =>
                {
                    tab.IsLoading = false;
                    tab.LastActiveTime = DateTime.Now;
                    if (_activeTab == tab)
                    {
                        PbLoading.Visibility = Visibility.Collapsed;
                        UpdateNavigationUI();
                        UpdateShieldBadge();
                    }

                    if (e.IsSuccess && !tab.IsIncognito && !string.IsNullOrWhiteSpace(tab.Url))
                    {
                        DataManager.RecordVisit(tab.Title, tab.Url);
                        RenderHomeScreenRecentVisits();
                    }
                };

                webView.SourceChanged += (s, e) =>
                {
                    tab.Url = webView.Source?.ToString() ?? string.Empty;
                    tab.LastActiveTime = DateTime.Now;
                    if (_activeTab == tab)
                    {
                        UpdateUrlBarDisplay(tab.Url);
                        UpdateSecurityBadge(tab.Url);
                        UpdateBookmarkButton(tab.Url);
                        UpdateNavigationUI();
                        UpdateShieldBadge();
                    }
                };

                webView.CoreWebView2.DocumentTitleChanged += (s, e) =>
                {
                    tab.Title = string.IsNullOrWhiteSpace(webView.CoreWebView2.DocumentTitle) ? "Untitled" : webView.CoreWebView2.DocumentTitle;
                    if (_isSplitScreen && tab == _splitTab && TbSplitRightTitle != null)
                    {
                        TbSplitRightTitle.Text = tab.Title;
                    }
                    RenderTabHeaders();
                };

                // New Window requested: open in a new RUHI tab instead of external browser
                webView.CoreWebView2.NewWindowRequested += (s, e) =>
                {
                    e.Handled = true;
                    if (!string.IsNullOrEmpty(e.Uri) && IsExtensionsProtocolUrl(e.Uri))
                    {
                        Dispatcher.BeginInvoke(new Action(() => OpenExtensionsWindow()));
                        return;
                    }
                    CreateNewTab(e.Uri, tab.IsIncognito);
                };

                // Download Starting
                webView.CoreWebView2.DownloadStarting += OnWebViewDownloadStarting;
            }
            catch (Exception ex)
            {
                var res = MessageBox.Show(
                    "RUHI Browser requires the Chromium Web Runtime to display web pages on your PC.\n\n" +
                    "Would you like to open the official download page to install the Web Runtime now?\n\n(Details: " + ex.Message + ")",
                    "RUHI Browser - Web Engine Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
                            UseShellExecute = true
                        });
                    }
                    catch { }
                }
            }
        }

        private void SwitchToTab(BrowserTab tab)
        {
            _activeTab = tab;
            tab.LastActiveTime = DateTime.Now;
            if (tab.IsSleeping)
            {
                WakeTab(tab);
            }
            RenderTabHeaders();

            if (_isSplitScreen && _splitTab != null)
            {
                RenderSplitView();
            }
            else
            {
                GridWebLeft.Children.Clear();
                if (tab.WebView != null)
                {
                    GridWebLeft.Children.Add(tab.WebView);
                }
            }

            if (string.IsNullOrWhiteSpace(tab.Url) || tab.Url == "about:blank")
            {
                HomeScreen.Visibility = Visibility.Visible;
                WebContainer.Visibility = Visibility.Collapsed;
                TbUrl.Text = string.Empty;
                TbSecurityIcon.Text = "🛡";
                TbSecurityIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                ApplyHomeScreenStyle();
            }
            else
            {
                HomeScreen.Visibility = Visibility.Collapsed;
                WebContainer.Visibility = Visibility.Visible;
                UpdateUrlBarDisplay(tab.Url);
                UpdateSecurityBadge(tab.Url);
            }

            UpdateBookmarkButton(tab.Url);
            UpdateNavigationUI();
            UpdateZoomBadge();
            UpdateShieldBadge();
            CloseFindInPage();
        }

        private void CloseTab(BrowserTab tab)
        {
            int index = _tabs.IndexOf(tab);
            if (index < 0) return;

            if (_isSplitScreen && tab == _splitTab)
            {
                CloseSplitScreen();
            }

            if (!string.IsNullOrWhiteSpace(tab.Url) && tab.Url != "about:blank" && !tab.IsIncognito)
            {
                _recentlyClosedTabs.Push(tab.Url);
            }

            if (tab == _activeTab)
            {
                GridWebLeft.Children.Clear();
            }

            var tempFolder = tab.TempUserDataFolder;
            tab.WebView?.Dispose();
            tab.WebView = null;
            _tabs.RemoveAt(index);

            if (!string.IsNullOrEmpty(tempFolder))
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1500);
                    try
                    {
                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }
                    }
                    catch { }
                });
            }

            if (!string.IsNullOrEmpty(tab.GroupId))
            {
                var gid = tab.GroupId;
                if (!_tabs.Any(t => t.GroupId == gid))
                {
                    _tabGroups.RemoveAll(g => g.Id == gid);
                    try { DataManager.SaveTabGroups(_tabGroups); } catch { }
                }
            }

            bool wasActive = (tab == _activeTab);
            if (_tabs.Count == 0)
            {
                CreateNewTab();
            }
            else if (wasActive)
            {
                int newIndex = Math.Min(index, _tabs.Count - 1);
                SwitchToTab(_tabs[newIndex]);
            }

            RenderTabHeaders();
        }

        private void ReopenClosedTab()
        {
            if (_recentlyClosedTabs.Count > 0)
            {
                var url = _recentlyClosedTabs.Pop();
                CreateNewTab(url);
            }
        }

        private void RenderTabHeaders()
        {
            SpTabs.Children.Clear();
            bool isDark = IsCurrentThemeDark();
            var renderedGroups = new HashSet<string>();

            // 1. Render Pinned tabs first (pinned tabs are never in groups)
            foreach (var tab in _tabs.Where(t => t.IsPinned))
            {
                RenderSingleTabHeader(tab, null, isDark);
            }

            // 2. Render Unpinned tabs (grouped or ungrouped)
            foreach (var tab in _tabs.Where(t => !t.IsPinned))
            {
                TabGroup? group = null;
                if (!string.IsNullOrEmpty(tab.GroupId))
                {
                    group = _tabGroups.FirstOrDefault(g => g.Id == tab.GroupId);
                }

                if (group != null && !renderedGroups.Contains(group.Id))
                {
                    renderedGroups.Add(group.Id);
                    var groupPill = CreateGroupPillHeader(group);
                    SpTabs.Children.Add(groupPill);
                }

                if (group != null && group.IsCollapsed)
                {
                    // Tab is collapsed inside group pill
                    continue;
                }

                RenderSingleTabHeader(tab, group, isDark);
            }
        }

        private void RenderSingleTabHeader(BrowserTab tab, TabGroup? group, bool isDark)
        {
            bool isActive = (tab == _activeTab);

            string bgHex = isActive ? (isDark ? "#28292C" : "#FFFFFF") : (isDark ? "#181818" : "#E2E8F0");
            string borderHex = isActive ? "#2563EB" : (isDark ? "#383838" : "#CBD5E1");
            string fgHex = isActive ? (isDark ? "#FFFFFF" : "#0F172A") : (isDark ? "#94A3B8" : "#475569");
            string closeHex = isDark ? "#94A3B8" : "#64748B";

            int unpinnedCount = _tabs.Count(t => !t.IsPinned);
            double tabMaxWidth = 200;
            if (unpinnedCount > 5 && TabScrollViewer != null && TabScrollViewer.ActualWidth > 200)
            {
                double available = TabScrollViewer.ActualWidth - 60;
                double perTab = available / unpinnedCount;
                tabMaxWidth = Math.Clamp(perTab, 90, 200);
            }

            var tabBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex)),
                BorderBrush = group != null
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(group.ColorHex))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderHex)),
                BorderThickness = group != null
                    ? new Thickness(1, 3, 1, isActive ? 0 : 1)
                    : new Thickness(1, 1, 1, isActive ? 0 : 1),
                CornerRadius = new CornerRadius(6, 6, 0, 0),
                Margin = new Thickness(2, 4, 2, 0),
                Padding = tab.IsPinned ? new Thickness(10, 4, 10, 4) : new Thickness(10, 4, 8, 4),
                MaxWidth = tab.IsPinned ? 46 : tabMaxWidth,
                Height = 32,
                Cursor = Cursors.Hand,
                ToolTip = group != null ? $"[{group.Title}] {tab.Title}" : tab.Title
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string prefix = "";
            if (tab.IsPinned) prefix += "📌";
            if (tab.IsIncognito) prefix += (prefix.Length > 0 ? " " : "") + "🕶";
            if (tab.IsMuted) prefix += (prefix.Length > 0 ? " " : "") + "🔇";
            if (tab.IsSleeping) prefix += (prefix.Length > 0 ? " " : "") + "💤";

            if (tab.IsSleeping)
            {
                tabBorder.Opacity = 0.72;
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                var prefixText = new TextBlock
                {
                    Text = prefix,
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    Margin = tab.IsPinned ? new Thickness(0) : new Thickness(0, 0, 4, 0)
                };
                Grid.SetColumn(prefixText, 0);
                grid.Children.Add(prefixText);
            }

            if (!tab.IsPinned)
            {
                var titleText = new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(tab.Title) ? "New Tab" : tab.Title,
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgHex)),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                Grid.SetColumn(titleText, 1);
                grid.Children.Add(titleText);

                var btnClose = new Button
                {
                    Content = "✕",
                    Background = Brushes.Transparent,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(closeHex)),
                    BorderThickness = new Thickness(0),
                    FontSize = 11,
                    Width = 20,
                    Height = 20,
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = Cursors.Hand,
                    ToolTip = "Close tab (Ctrl+W)"
                };
                btnClose.MouseEnter += (s, e) => btnClose.Foreground = isDark ? Brushes.White : Brushes.Black;
                btnClose.MouseLeave += (s, e) => btnClose.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(closeHex));
                btnClose.Click += (s, e) =>
                {
                    e.Handled = true;
                    CloseTab(tab);
                };
                Grid.SetColumn(btnClose, 2);
                grid.Children.Add(btnClose);
            }

            tabBorder.Child = grid;
            tabBorder.ContextMenu = CreateTabContextMenu(tab);

            tabBorder.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    e.Handled = true;
                    SwitchToTab(tab);
                }
                else if (e.ChangedButton == MouseButton.Middle && !tab.IsPinned)
                {
                    e.Handled = true;
                    CloseTab(tab);
                }
            };

            SpTabs.Children.Add(tabBorder);

            if (isActive)
            {
                tabBorder.Loaded += (s, e) => { try { tabBorder.BringIntoView(); } catch { } };
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    try { tabBorder.BringIntoView(); } catch { }
                }));
            }
        }

        private Border CreateGroupPillHeader(TabGroup group)
        {
            var count = _tabs.Count(t => t.GroupId == group.Id);
            var color = (Color)ColorConverter.ConvertFromString(group.ColorHex);

            var pill = new Border
            {
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(12),
                Height = 24,
                Margin = new Thickness(4, 6, 2, 0),
                Padding = new Thickness(10, 2, 10, 2),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = $"Group: {group.Title} ({count} tabs)\n• Click to {(group.IsCollapsed ? "expand" : "collapse")}\n• Right-click to edit name & color"
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            var tbTitle = new TextBlock
            {
                Text = group.Title,
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(tbTitle);

            if (group.IsCollapsed)
            {
                var tbCount = new TextBlock
                {
                    Text = $" ({count})",
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0")),
                    VerticalAlignment = VerticalAlignment.Center
                };
                stack.Children.Add(tbCount);
            }

            pill.Child = stack;

            pill.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    e.Handled = true;
                    ToggleGroupCollapse(group);
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    e.Handled = true;
                    var menu = CreateGroupContextMenu(group);
                    menu.PlacementTarget = pill;
                    menu.IsOpen = true;
                }
            };

            pill.ContextMenu = CreateGroupContextMenu(group);

            return pill;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;
                var found = FindVisualChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }

        private ContextMenu CreateModernContextMenu()
        {
            var menu = new ContextMenu();
            if (TryFindResource("ModernContextMenu") is Style contextMenuStyle)
            {
                menu.Style = contextMenuStyle;
            }
            double maxH = SystemParameters.WorkArea.Height - 80;
            if (maxH > 250)
            {
                menu.MaxHeight = maxH;
            }
            menu.PreviewMouseWheel += (s, e) =>
            {
                if (s is ContextMenu cm)
                {
                    var sv = FindVisualChild<ScrollViewer>(cm);
                    if (sv != null)
                    {
                        if (e.Delta < 0)
                            sv.ScrollToVerticalOffset(sv.VerticalOffset + 50);
                        else if (e.Delta > 0)
                            sv.ScrollToVerticalOffset(Math.Max(0, sv.VerticalOffset - 50));
                        e.Handled = true;
                    }
                }
            };
            return menu;
        }

        private MenuItem CreateModernMenuItem(string header, RoutedEventHandler? handler = null)
        {
            var item = new MenuItem { Header = header };
            if (TryFindResource("ModernMenuItem") is Style itemStyle)
            {
                item.Style = itemStyle;
            }
            if (handler != null)
            {
                item.Click += handler;
            }
            return item;
        }

        private ContextMenu CreateGroupContextMenu(TabGroup group)
        {
            var menu = CreateModernContextMenu();

            MenuItem CreateMenuItem(string header, RoutedEventHandler handler) => CreateModernMenuItem(header, handler);

            // 1. Edit Name & Color dialog
            menu.Items.Add(CreateMenuItem($"✏️ Edit Group: \"{group.Title}\"...", (s, ev) =>
            {
                OpenEditGroupDialog(group);
            }));

            // 2. Change Color quick submenu
            var colorMenu = CreateModernMenuItem("🎨 Quick Colors");

            foreach (var (name, hex) in TabGroup.ChromeColors)
            {
                var colorItem = CreateModernMenuItem($"● {name}");
                colorItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                colorItem.FontWeight = FontWeights.Bold;
                string capturedHex = hex;
                colorItem.Click += (s, ev) =>
                {
                    group.ColorHex = capturedHex;
                    RenderTabHeaders();
                };
                colorMenu.Items.Add(colorItem);
            }
            menu.Items.Add(colorMenu);

            menu.Items.Add(new Separator());

            // 3. New Tab in Group
            menu.Items.Add(CreateMenuItem("➕ New Tab in Group", (s, ev) =>
            {
                CreateNewTabInGroup(group);
            }));

            // 4. Toggle Collapse
            menu.Items.Add(CreateMenuItem(group.IsCollapsed ? "▶ Expand Group" : "▼ Collapse Group", (s, ev) =>
            {
                ToggleGroupCollapse(group);
            }));

            menu.Items.Add(new Separator());

            // 5. Ungroup
            menu.Items.Add(CreateMenuItem("🔓 Ungroup (ग्रुप हटाएं)", (s, ev) =>
            {
                Ungroup(group);
            }));

            // 6. Close Group
            menu.Items.Add(CreateMenuItem("✕ Close Group (पूरा ग्रुप बंद करें)", (s, ev) =>
            {
                CloseGroup(group);
            }));

            return menu;
        }

        private void OpenEditGroupDialog(TabGroup group)
        {
            var dlg = new RenameGroupDialog(group.Title, group.ColorHex) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                group.Title = dlg.GroupTitle;
                group.ColorHex = dlg.SelectedColorHex;
                RenderTabHeaders();
            }
        }

        private void CreateNewTabInGroup(TabGroup group)
        {
            group.IsCollapsed = false;
            CreateNewTab(groupId: group.Id);
        }

        private void ToggleGroupCollapse(TabGroup group)
        {
            group.IsCollapsed = !group.IsCollapsed;

            if (group.IsCollapsed && _activeTab?.GroupId == group.Id)
            {
                var visibleTab = _tabs.FirstOrDefault(t => t.GroupId != group.Id);
                if (visibleTab != null)
                {
                    SwitchToTab(visibleTab);
                }
            }

            RenderTabHeaders();
        }

        private void Ungroup(TabGroup group)
        {
            foreach (var tab in _tabs.Where(t => t.GroupId == group.Id))
            {
                tab.GroupId = null;
            }
            _tabGroups.Remove(group);
            try { DataManager.SaveTabGroups(_tabGroups); } catch { }
            RenderTabHeaders();
        }

        private void CloseGroup(TabGroup group)
        {
            var tabsToClose = _tabs.Where(t => t.GroupId == group.Id).ToList();
            foreach (var tab in tabsToClose)
            {
                CloseTab(tab);
            }
            _tabGroups.Remove(group);
            try { DataManager.SaveTabGroups(_tabGroups); } catch { }
            RenderTabHeaders();
        }

        private void AddTabToNewGroup(BrowserTab tab)
        {
            var nextColor = TabGroup.ChromeColors[_tabGroups.Count % TabGroup.ChromeColors.Length].Hex;
            var defaultName = $"Group {_tabGroups.Count + 1}";

            var dlg = new RenameGroupDialog(defaultName, nextColor) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                var group = new TabGroup
                {
                    Title = dlg.GroupTitle,
                    ColorHex = dlg.SelectedColorHex
                };
                _tabGroups.Add(group);
                tab.GroupId = group.Id;
                try { DataManager.SaveTabGroups(_tabGroups); } catch { }
                RenderTabHeaders();
            }
        }

        private void AddTabToExistingGroup(BrowserTab tab, TabGroup group)
        {
            tab.GroupId = group.Id;
            group.IsCollapsed = false;

            _tabs.Remove(tab);
            int lastIndex = _tabs.FindLastIndex(t => t.GroupId == group.Id);
            if (lastIndex >= 0)
            {
                _tabs.Insert(lastIndex + 1, tab);
            }
            else
            {
                _tabs.Add(tab);
            }

            RenderTabHeaders();
        }

        private ContextMenu CreateTabContextMenu(BrowserTab tab)
        {
            var menu = CreateModernContextMenu();

            MenuItem CreateItem(string header, RoutedEventHandler handler) => CreateModernMenuItem(header, handler);

            // Tab Grouping Menu
            if (!tab.IsPinned)
            {
                var groupSubMenu = CreateModernMenuItem("🏷️ Add Tab to Group (ग्रुप में जोड़ें)");

                var newGroupItem = CreateModernMenuItem("➕ New Group... (नया ग्रुप)", (s, ev) => AddTabToNewGroup(tab));
                newGroupItem.FontWeight = FontWeights.SemiBold;
                groupSubMenu.Items.Add(newGroupItem);

                if (_tabGroups.Count > 0)
                {
                    groupSubMenu.Items.Add(new Separator());
                    foreach (var g in _tabGroups)
                    {
                        var gItem = CreateModernMenuItem($"● {g.Title}");
                        gItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(g.ColorHex));
                        gItem.FontWeight = FontWeights.SemiBold;
                        var capturedGroup = g;
                        gItem.Click += (s, ev) => AddTabToExistingGroup(tab, capturedGroup);
                        groupSubMenu.Items.Add(gItem);
                    }
                }
                menu.Items.Add(groupSubMenu);

                if (!string.IsNullOrEmpty(tab.GroupId))
                {
                    var curGroup = _tabGroups.FirstOrDefault(g => g.Id == tab.GroupId);
                    menu.Items.Add(CreateItem("Remove from Group (ग्रुप से हटाएं)", (s, ev) =>
                    {
                        tab.GroupId = null;
                        if (curGroup != null && !_tabs.Any(t => t.GroupId == curGroup.Id))
                        {
                            _tabGroups.Remove(curGroup);
                            try { DataManager.SaveTabGroups(_tabGroups); } catch { }
                        }
                        RenderTabHeaders();
                    }));

                    if (curGroup != null)
                    {
                        menu.Items.Add(CreateItem($"Close Group \"{curGroup.Title}\" (ग्रुप बंद करें)", (s, ev) =>
                        {
                            CloseGroup(curGroup);
                        }));
                    }
                }

                menu.Items.Add(new Separator());
            }

            menu.Items.Add(CreateItem(tab.IsPinned ? "Unpin Tab (पिन हटाएं)" : "Pin Tab (📌 पिन करें)", (s, ev) =>
            {
                tab.IsPinned = !tab.IsPinned;
                if (tab.IsPinned)
                {
                    tab.GroupId = null;
                    _tabs.Remove(tab);
                    int lastPinned = _tabs.FindLastIndex(t => t.IsPinned);
                    _tabs.Insert(lastPinned >= 0 ? lastPinned + 1 : 0, tab);
                }
                RenderTabHeaders();
            }));

            menu.Items.Add(CreateItem(tab.IsMuted ? "Unmute Tab (🔊 आवाज़ चालू करें)" : "Mute Tab (🔇 आवाज़ बंद करें)", (s, ev) =>
            {
                tab.IsMuted = !tab.IsMuted;
                if (tab.WebView?.CoreWebView2 != null)
                {
                    tab.WebView.CoreWebView2.IsMuted = tab.IsMuted;
                }
                RenderTabHeaders();
            }));

            menu.Items.Add(CreateItem("Duplicate Tab (📄 डुप्लिकेट)", (s, ev) =>
            {
                CreateNewTab(tab.Url, groupId: tab.GroupId);
            }));

            menu.Items.Add(CreateItem("Reload Tab (↻ रीलोड)", (s, ev) =>
            {
                tab.WebView?.Reload();
            }));

            // Memory Saver sleep/wake option
            if (tab.IsSleeping)
            {
                menu.Items.Add(CreateItem("⚡ Wake Tab (टैब एक्टिव करें)", (s, ev) => WakeTab(tab)));
            }
            else if (tab != _activeTab && !tab.IsPinned)
            {
                menu.Items.Add(CreateItem("💤 Put Tab to Sleep (RAM 50% बचाएं)", (s, ev) => SleepTab(tab)));
            }

            if (SyncService.IsLoggedIn && !string.IsNullOrWhiteSpace(tab.Url) && tab.Url != "about:blank")
            {
                menu.Items.Add(CreateItem("📱 Send Tab to Phone / Devices", async (s, ev) =>
                {
                    bool ok = await SyncService.SendTabToDeviceAsync(null, tab.Url, tab.Title);
                    if (ok)
                    {
                        MessageBox.Show(this, $"Sent \"{tab.Title}\" to your linked phone!", "Tab Sent", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Could not send tab. Make sure RUHI Cloud Sync is active.", "Send Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }));
            }

            menu.Items.Add(new Separator());

            if (!tab.IsPinned)
            {
                menu.Items.Add(CreateItem("Close Tab (✕ क्लोज़)", (s, ev) =>
                {
                    CloseTab(tab);
                }));
            }

            menu.Items.Add(CreateItem("Close Other Tabs (बाकी सब बंद करें)", (s, ev) =>
            {
                var toClose = _tabs.Where(t => t != tab && !t.IsPinned).ToList();
                foreach (var t in toClose)
                {
                    CloseTab(t);
                }
            }));

            menu.Items.Add(CreateItem("Close Tabs to the Right (दाएं वाले बंद करें)", (s, ev) =>
            {
                int index = _tabs.IndexOf(tab);
                if (index >= 0)
                {
                    var toClose = _tabs.Skip(index + 1).Where(t => !t.IsPinned).ToList();
                    foreach (var t in toClose)
                    {
                        CloseTab(t);
                    }
                }
            }));

            if (_recentlyClosedTabs.Count > 0)
            {
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateItem("Reopen Closed Tab (Ctrl+Shift+T)", (s, ev) =>
                {
                    ReopenClosedTab();
                }));
            }

            return menu;
        }

        #endregion

        #region Navigation & Address Bar

        private static bool IsExtensionsProtocolUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            string clean = url.Trim().ToLowerInvariant();
            return clean.StartsWith("ruhi://extension") ||
                   clean.StartsWith("ruhi:extension") ||
                   clean.StartsWith("ruhi://extensions") ||
                   clean.StartsWith("ruhi:extensions") ||
                   clean.StartsWith("ruhi://manage-extension") ||
                   clean.StartsWith("ruhi://manage-extensions") ||
                   clean.StartsWith("ruhi://extensions-manager") ||
                   clean.StartsWith("ruhi://extension-manager") ||
                   clean.StartsWith("chrome://extension") ||
                   clean.StartsWith("chrome://extensions") ||
                   clean.StartsWith("edge://extension") ||
                   clean.StartsWith("edge://extensions");
        }

        private static bool IsExtensionStoreUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            string lower = url.Trim().ToLowerInvariant();
            return lower.Contains("addonruhi.netlify.app") ||
                   lower == "ruhi://store" || lower == "ruhi:store" ||
                   lower == "ruhi://addons" || lower == "ruhi:addons";
        }

        private void UpdateUrlBarDisplay(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || url == "about:blank")
            {
                TbUrl.Text = string.Empty;
                return;
            }

            if (IsExtensionStoreUrl(url))
            {
                TbUrl.Text = "RUHI Add-On-Store";
            }
            else if (url.StartsWith("chrome-extension://", StringComparison.OrdinalIgnoreCase) &&
                     !string.IsNullOrWhiteSpace(_settings.ActiveExtensionNewTabUrl) &&
                     url.StartsWith(_settings.ActiveExtensionNewTabUrl, StringComparison.OrdinalIgnoreCase))
            {
                TbUrl.Text = string.Empty;
            }
            else
            {
                TbUrl.Text = url;
            }
        }

        private void NavigateTab(BrowserTab tab, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            if (IsExtensionsProtocolUrl(input))
            {
                OpenExtensionsWindow();
                return;
            }

            string targetUrl = UrlResolver.Resolve(input, _settings.SearchEngine);
            if (IsExtensionsProtocolUrl(targetUrl))
            {
                OpenExtensionsWindow();
                return;
            }

            tab.Url = targetUrl;
            tab.BlockedAdsCount = 0;
            tab.LastActiveTime = DateTime.Now;
            if (tab.IsSleeping)
            {
                WakeTab(tab);
            }
            HomeScreen.Visibility = Visibility.Collapsed;
            WebContainer.Visibility = Visibility.Visible;

            if (tab.WebView != null)
            {
                if (tab.WebView.CoreWebView2 != null)
                {
                    tab.WebView.CoreWebView2.Navigate(targetUrl);
                }
                else
                {
                    if (targetUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                        targetUrl.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                    {
                        try { tab.WebView.Source = new Uri(targetUrl); } catch { }
                    }
                }
            }

            UpdateUrlBarDisplay(targetUrl);
            UpdateSecurityBadge(targetUrl);
            UpdateShieldBadge();
        }

        private class UrlSuggestionItem
        {
            public string Icon { get; set; } = "🌐";
            public string Title { get; set; } = "";
            public string Subtitle { get; set; } = "";
            public string TargetUrl { get; set; } = "";
        }

        private static readonly HttpClient _suggestHttpClient = new() { Timeout = TimeSpan.FromSeconds(2.5) };
        private System.Threading.CancellationTokenSource? _suggestCts;

        private static readonly (string Key, string Domain, string Title)[] PopularDomains = new[]
        {
            ("you", "https://www.youtube.com", "YouTube"),
            ("yt", "https://www.youtube.com", "YouTube"),
            ("taa", "https://taazawire.in", "TaazaWire"),
            ("taaza", "https://taazawire.in", "TaazaWire"),
            ("goo", "https://www.google.com", "Google"),
            ("gma", "https://mail.google.com", "Gmail"),
            ("fac", "https://www.facebook.com", "Facebook"),
            ("fb", "https://www.facebook.com", "Facebook"),
            ("ins", "https://www.instagram.com", "Instagram"),
            ("ig", "https://www.instagram.com", "Instagram"),
            ("twi", "https://twitter.com", "Twitter / X"),
            ("x", "https://x.com", "X"),
            ("git", "https://github.com", "GitHub"),
            ("wik", "https://www.wikipedia.org", "Wikipedia"),
            ("ama", "https://www.amazon.com", "Amazon"),
            ("net", "https://www.netflix.com", "Netflix"),
            ("cha", "https://chatgpt.com", "ChatGPT"),
            ("wha", "https://web.whatsapp.com", "WhatsApp Web"),
            ("red", "https://www.reddit.com", "Reddit"),
            ("lin", "https://www.linkedin.com", "LinkedIn"),
            ("pin", "https://www.pinterest.com", "Pinterest"),
            ("spo", "https://www.spotify.com", "Spotify")
        };

        private readonly List<UrlSuggestionItem> _builtinSuggestions = new()
        {
            new UrlSuggestionItem { Icon = "🧩", Title = "ruhi://extensions", Subtitle = "Extensions Manager — Manage RUHI Browser add-ons", TargetUrl = "ruhi://extensions" },
            new UrlSuggestionItem { Icon = "🛍️", Title = "ruhi://store", Subtitle = "RUHI Add-On-Store — Discover extensions & themes", TargetUrl = "ruhi://store" },
            new UrlSuggestionItem { Icon = "⚙️", Title = "ruhi://settings", Subtitle = "Settings — General, Appearance, Adblocker, Privacy", TargetUrl = "ruhi://settings" },
            new UrlSuggestionItem { Icon = "🕒", Title = "ruhi://history", Subtitle = "History — Browsing history", TargetUrl = "ruhi://history" },
            new UrlSuggestionItem { Icon = "📥", Title = "ruhi://downloads", Subtitle = "Downloads — Manage downloaded files", TargetUrl = "ruhi://downloads" },
            new UrlSuggestionItem { Icon = "🔒", Title = "ruhi://passwords", Subtitle = "Passwords — Saved credentials & logins", TargetUrl = "ruhi://passwords" },
            new UrlSuggestionItem { Icon = "🛠️", Title = "ruhi://pdftools", Subtitle = "PDF Tools — Annotate, Rotate, Edit", TargetUrl = "ruhi://pdftools" },
        };

        private int _selectedSuggestionIndex = -1;
        private readonly List<UrlSuggestionItem> _currentSuggestions = new();
        private bool _isUpdatingUrlTextProgrammatically = false;

        private void OnUrlTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingUrlTextProgrammatically) return;

            string text = TbUrl.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                PopupUrlSuggestions.IsOpen = false;
                PanelUrlSuggestions.Children.Clear();
                return;
            }

            if (_activeTab != null && (text == _activeTab.Url || text == "RUHI Add-On-Store" || text == "RUHI Extension Store"))
            {
                PopupUrlSuggestions.IsOpen = false;
                return;
            }

            ShowUrlSuggestions(text);
        }

        private void ShowUrlSuggestions(string query)
        {
            _suggestCts?.Cancel();
            _suggestCts = new System.Threading.CancellationTokenSource();
            var token = _suggestCts.Token;

            PanelUrlSuggestions.Children.Clear();
            _currentSuggestions.Clear();
            _selectedSuggestionIndex = -1;

            string q = query.Trim().ToLowerInvariant();
            string cleanQ = q.Replace("ruhi://", "").Replace("ruhi:", "").Replace("ruhi", "").Trim('/');

            // 1. Built-in pages (ONLY when specifically typing ruhi internal URLs or settings/extensions keywords)
            bool isSearchingInternal = q.StartsWith("ruhi") || q.StartsWith("chrome://") || q.StartsWith("edge://");
            if (isSearchingInternal)
            {
                foreach (var item in _builtinSuggestions)
                {
                    string target = item.TargetUrl.ToLowerInvariant();
                    if (string.IsNullOrEmpty(cleanQ) || target.Contains(cleanQ))
                    {
                        _currentSuggestions.Add(item);
                    }
                }
            }
            else
            {
                if (q.StartsWith("ext") || q.StartsWith("addon"))
                {
                    _currentSuggestions.Add(_builtinSuggestions[0]); // ruhi://extensions
                    _currentSuggestions.Add(_builtinSuggestions[1]); // ruhi://store
                }
                else if (q.StartsWith("sett"))
                {
                    _currentSuggestions.Add(_builtinSuggestions[2]); // ruhi://settings
                }
                else if (q.StartsWith("hist"))
                {
                    _currentSuggestions.Add(_builtinSuggestions[3]); // ruhi://history
                }
                else if (q.StartsWith("down"))
                {
                    _currentSuggestions.Add(_builtinSuggestions[4]); // ruhi://downloads
                }
                else if (q.StartsWith("pass"))
                {
                    _currentSuggestions.Add(_builtinSuggestions[5]); // ruhi://passwords
                }
            }

            // 2. Popular Domain Fast Matches (e.g. you -> YouTube, goo -> Google, git -> GitHub)
            foreach (var (k, domain, title) in PopularDomains)
            {
                if (q.StartsWith(k) || title.ToLowerInvariant().StartsWith(q) || domain.Contains(q))
                {
                    if (!_currentSuggestions.Any(x => x.TargetUrl == domain))
                    {
                        _currentSuggestions.Add(new UrlSuggestionItem
                        {
                            Icon = "🌐",
                            Title = title,
                            Subtitle = domain,
                            TargetUrl = domain
                        });
                    }
                    if (_currentSuggestions.Count >= 2) break;
                }
            }

            // 3. Match Bookmarks
            foreach (var b in _bookmarks)
            {
                if (b.Title.ToLowerInvariant().Contains(q) || b.Url.ToLowerInvariant().Contains(q))
                {
                    if (!_currentSuggestions.Any(x => x.TargetUrl == b.Url))
                    {
                        _currentSuggestions.Add(new UrlSuggestionItem
                        {
                            Icon = "⭐",
                            Title = b.Title,
                            Subtitle = b.Url,
                            TargetUrl = b.Url
                        });
                    }
                }
                if (_currentSuggestions.Count >= 4) break;
            }

            // 4. Default Search Query
            _currentSuggestions.Add(new UrlSuggestionItem
            {
                Icon = "🔍",
                Title = $"Search Google for \"{query}\"",
                Subtitle = "Web Search",
                TargetUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}"
            });

            if (BorderOmnibox != null && BorderOmnibox.ActualWidth > 100)
            {
                BorderUrlSuggestions.Width = BorderOmnibox.ActualWidth;
            }
            else
            {
                BorderUrlSuggestions.Width = 460;
            }

            RenderUrlSuggestionList();
            PopupUrlSuggestions.PlacementTarget = BorderOmnibox;
            PopupUrlSuggestions.IsOpen = true;

            // 5. Asynchronously fetch live Google Chrome-style word recommendations
            _ = FetchGoogleWordSuggestionsAsync(query, token);
        }

        private async Task FetchGoogleWordSuggestionsAsync(string query, System.Threading.CancellationToken token)
        {
            try
            {
                await Task.Delay(100, token);
                if (token.IsCancellationRequested) return;

                string url = $"https://suggestqueries.google.com/complete/search?client=chrome&q={Uri.EscapeDataString(query)}";
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                var resp = await _suggestHttpClient.SendAsync(req, token);
                if (!resp.IsSuccessStatusCode || token.IsCancellationRequested) return;

                var json = await resp.Content.ReadAsStringAsync(token);
                if (token.IsCancellationRequested || string.IsNullOrWhiteSpace(json)) return;

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array || doc.RootElement.GetArrayLength() < 2) return;

                var suggestionsArray = doc.RootElement[1];
                JsonElement titlesArray = default;
                if (doc.RootElement.GetArrayLength() >= 3 && doc.RootElement[2].ValueKind == JsonValueKind.Array)
                {
                    titlesArray = doc.RootElement[2];
                }

                var liveItems = new List<UrlSuggestionItem>();
                for (int i = 0; i < suggestionsArray.GetArrayLength() && liveItems.Count < 6; i++)
                {
                    string textVal = suggestionsArray[i].GetString() ?? "";
                    if (string.IsNullOrWhiteSpace(textVal)) continue;

                    string titleVal = "";
                    if (titlesArray.ValueKind == JsonValueKind.Array && i < titlesArray.GetArrayLength())
                    {
                        titleVal = titlesArray[i].GetString() ?? "";
                    }

                    bool isNavUrl = textVal.StartsWith("http://") || textVal.StartsWith("https://");
                    string target = isNavUrl ? textVal : $"https://www.google.com/search?q={Uri.EscapeDataString(textVal)}";

                    if (_currentSuggestions.Any(x => x.TargetUrl == target || string.Equals(x.Title, textVal, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    liveItems.Add(new UrlSuggestionItem
                    {
                        Icon = isNavUrl ? "🌐" : "🔍",
                        Title = !string.IsNullOrEmpty(titleVal) ? titleVal : textVal,
                        Subtitle = isNavUrl ? textVal : "Google Search",
                        TargetUrl = target
                    });
                }

                if (token.IsCancellationRequested || liveItems.Count == 0) return;

                await Dispatcher.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested || !PopupUrlSuggestions.IsOpen) return;

                    int insertIdx = Math.Min(_currentSuggestions.Count - 1, 2);
                    if (insertIdx < 0) insertIdx = 0;

                    foreach (var item in liveItems)
                    {
                        if (!_currentSuggestions.Any(x => x.TargetUrl == item.TargetUrl))
                        {
                            _currentSuggestions.Insert(insertIdx++, item);
                        }
                    }

                    RenderUrlSuggestionList();
                });
            }
            catch { }
        }

        private void RenderUrlSuggestionList()
        {
            PanelUrlSuggestions.Children.Clear();
            bool isDark = IsCurrentThemeDark();
            var textFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            var subFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));
            var hoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#F1F5F9"));

            for (int i = 0; i < _currentSuggestions.Count; i++)
            {
                int index = i;
                var item = _currentSuggestions[i];

                var row = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(0, 1, 0, 1),
                    Background = (index == _selectedSuggestionIndex) ? hoverBg : Brushes.Transparent,
                    Cursor = Cursors.Hand
                };

                var sp = new Grid();
                sp.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                sp.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var tbIcon = new TextBlock
                {
                    Text = item.Icon,
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(tbIcon, 0);
                sp.Children.Add(tbIcon);

                var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                var tbTitle = new TextBlock
                {
                    Text = item.Title,
                    FontSize = 12.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = textFg,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                textStack.Children.Add(tbTitle);

                if (!string.IsNullOrEmpty(item.Subtitle))
                {
                    var tbSub = new TextBlock
                    {
                        Text = item.Subtitle,
                        FontSize = 11,
                        Foreground = subFg,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    textStack.Children.Add(tbSub);
                }

                Grid.SetColumn(textStack, 1);
                sp.Children.Add(textStack);

                row.Child = sp;

                row.MouseEnter += (s, e) =>
                {
                    _selectedSuggestionIndex = index;
                    UpdateSuggestionRowHighlights();
                };

                row.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    e.Handled = true;
                    ExecuteSuggestion(item);
                };

                PanelUrlSuggestions.Children.Add(row);
            }
        }

        private void UpdateSuggestionRowHighlights()
        {
            bool isDark = IsCurrentThemeDark();
            var hoverBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#F1F5F9"));

            for (int i = 0; i < PanelUrlSuggestions.Children.Count; i++)
            {
                if (PanelUrlSuggestions.Children[i] is Border b)
                {
                    b.Background = (i == _selectedSuggestionIndex) ? hoverBg : Brushes.Transparent;
                }
            }
        }

        private void ExecuteSuggestion(UrlSuggestionItem item)
        {
            PopupUrlSuggestions.IsOpen = false;

            if (item.TargetUrl == "ruhi://extensions" || item.TargetUrl == "ruhi://extension")
            {
                OpenExtensionsWindow();
                return;
            }
            if (item.TargetUrl == "ruhi://store" || item.TargetUrl == "ruhi://addons")
            {
                OpenRuhiStore();
                return;
            }
            if (item.TargetUrl == "ruhi://settings")
            {
                OpenSettingsWindow();
                return;
            }
            if (item.TargetUrl == "ruhi://history")
            {
                OpenHistoryWindow();
                return;
            }
            if (item.TargetUrl == "ruhi://downloads")
            {
                OnDownloadsClicked(this, new RoutedEventArgs());
                return;
            }
            if (item.TargetUrl == "ruhi://passwords")
            {
                OpenPasswordsWindow();
                return;
            }
            if (item.TargetUrl == "ruhi://pdftools")
            {
                OpenPdfToolsWindow(_activeTab?.Url);
                return;
            }

            if (_activeTab != null)
            {
                NavigateTab(_activeTab, item.TargetUrl);
            }
            else
            {
                CreateNewTab(item.TargetUrl);
            }
        }

        private void OnUrlKeyDown(object sender, KeyEventArgs e)
        {
            if (PopupUrlSuggestions.IsOpen)
            {
                if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    _selectedSuggestionIndex++;
                    if (_selectedSuggestionIndex >= _currentSuggestions.Count) _selectedSuggestionIndex = 0;
                    UpdateSuggestionRowHighlights();
                    if (_selectedSuggestionIndex >= 0 && _selectedSuggestionIndex < _currentSuggestions.Count)
                    {
                        _isUpdatingUrlTextProgrammatically = true;
                        TbUrl.Text = _currentSuggestions[_selectedSuggestionIndex].TargetUrl;
                        TbUrl.CaretIndex = TbUrl.Text.Length;
                        _isUpdatingUrlTextProgrammatically = false;
                    }
                    return;
                }
                else if (e.Key == Key.Up)
                {
                    e.Handled = true;
                    _selectedSuggestionIndex--;
                    if (_selectedSuggestionIndex < 0) _selectedSuggestionIndex = _currentSuggestions.Count - 1;
                    UpdateSuggestionRowHighlights();
                    if (_selectedSuggestionIndex >= 0 && _selectedSuggestionIndex < _currentSuggestions.Count)
                    {
                        _isUpdatingUrlTextProgrammatically = true;
                        TbUrl.Text = _currentSuggestions[_selectedSuggestionIndex].TargetUrl;
                        TbUrl.CaretIndex = TbUrl.Text.Length;
                        _isUpdatingUrlTextProgrammatically = false;
                    }
                    return;
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    PopupUrlSuggestions.IsOpen = false;
                    return;
                }
                else if (e.Key == Key.Enter)
                {
                    if (_selectedSuggestionIndex >= 0 && _selectedSuggestionIndex < _currentSuggestions.Count)
                    {
                        e.Handled = true;
                        ExecuteSuggestion(_currentSuggestions[_selectedSuggestionIndex]);
                        return;
                    }
                    PopupUrlSuggestions.IsOpen = false;
                }
            }

            if (e.Key == Key.Enter && _activeTab != null)
            {
                string text = TbUrl.Text.Trim();
                if (string.Equals(text, "RUHI Add-On-Store", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(text, "RUHI Extension Store", StringComparison.OrdinalIgnoreCase))
                {
                    NavigateTab(_activeTab, UrlResolver.GetStoreUrl());
                }
                else
                {
                    NavigateTab(_activeTab, text);
                }
            }
        }

        private void OnUrlPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }

        private void OnUrlGotFocus(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null && !string.IsNullOrWhiteSpace(_activeTab.Url) && _activeTab.Url != "about:blank")
            {
                if (IsExtensionStoreUrl(_activeTab.Url))
                {
                    TbUrl.Text = "RUHI Add-On-Store";
                }
                else
                {
                    TbUrl.Text = _activeTab.Url;
                }
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
            {
                TbUrl.SelectAll();
            }));
        }

        private void OnUrlLostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
            {
                await Task.Delay(250);
                if (!TbUrl.IsFocused && !PopupUrlSuggestions.IsMouseOver)
                {
                    PopupUrlSuggestions.IsOpen = false;
                }
            }));
            UpdateUrlBarDisplay(_activeTab?.Url);
        }

        private void OnUrlMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TbUrl.SelectAll();
        }

        private void OnNewTabClicked(object sender, RoutedEventArgs e)
        {
            CreateNewTab();
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.WebView?.CoreWebView2 != null && _activeTab.WebView.CanGoBack)
            {
                _activeTab.WebView.GoBack();
            }
        }

        private void OnForwardClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.WebView?.CoreWebView2 != null && _activeTab.WebView.CanGoForward)
            {
                _activeTab.WebView.GoForward();
            }
        }

        private void OnReloadClicked(object sender, RoutedEventArgs e)
        {
            _activeTab?.WebView?.Reload();
        }

        private void OnHomeClicked(object sender, RoutedEventArgs e)
        {
            _settings = DataManager.LoadSettings();
            if (_activeTab != null)
            {
                var mode = _settings.HomepageMode ?? "Home";
                if (string.Equals(mode, "Extension", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_settings.ActiveExtensionNewTabUrl))
                {
                    NavigateTab(_activeTab, _settings.ActiveExtensionNewTabUrl);
                }
                else if (string.Equals(mode, "Google", StringComparison.OrdinalIgnoreCase))
                {
                    NavigateTab(_activeTab, "https://www.google.com");
                }
                else if (string.Equals(mode, "Blank", StringComparison.OrdinalIgnoreCase))
                {
                    NavigateTab(_activeTab, "about:blank");
                }
                else if (string.Equals(mode, "Custom", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_settings.CustomHomepageUrl))
                {
                    NavigateTab(_activeTab, _settings.CustomHomepageUrl);
                }
                else
                {
                    _activeTab.Url = string.Empty;
                    _activeTab.Title = "New Tab";
                    SwitchToTab(_activeTab);
                }
            }
        }

        private bool IsPdfUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            var clean = url.Split('?')[0].Split('#')[0];
            return clean.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateNavigationUI()
        {
            if (_activeTab?.WebView != null)
            {
                BtnBack.IsEnabled = _activeTab.WebView.CanGoBack;
                BtnForward.IsEnabled = _activeTab.WebView.CanGoForward;
            }

            bool isPdf = _activeTab != null && IsPdfUrl(_activeTab.Url);
            BtnPdfEdit.Visibility = isPdf ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateSecurityBadge(string url)
        {
            if (IsExtensionStoreUrl(url) || url.StartsWith("chrome-extension://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("edge-extension://", StringComparison.OrdinalIgnoreCase))
            {
                TbSecurityIcon.Text = "🧩";
                TbSecurityIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A855F7"));
            }
            else if (url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                TbSecurityIcon.Text = "📄";
                TbSecurityIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"));
            }
            else if (UrlResolver.IsHttps(url))
            {
                TbSecurityIcon.Text = "🔒";
                TbSecurityIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
            }
            else if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                TbSecurityIcon.Text = "⚠️";
                TbSecurityIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAB308"));
            }
            else
            {
                TbSecurityIcon.Text = "🛡";
                TbSecurityIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            }
        }

        private void UpdateBookmarkButton(string url)
        {
            var isBookmarked = !string.IsNullOrWhiteSpace(url) && _bookmarks.Exists(b => string.Equals(b.Url, url, StringComparison.OrdinalIgnoreCase));
            BtnBookmark.Content = isBookmarked ? "★" : "☆";
            BtnBookmark.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isBookmarked ? "#F59E0B" : "#94A3B8"));
        }

        private void OnBookmarkClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null || string.IsNullOrWhiteSpace(_activeTab.Url)) return;

            var url = _activeTab.Url;
            var title = string.IsNullOrWhiteSpace(_activeTab.Title) ? url : _activeTab.Title;

            var existing = _bookmarks.Find(b => string.Equals(b.Url, url, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _bookmarks.Remove(existing);
                DataManager.SaveBookmarks(_bookmarks);
                UpdateBookmarkButton(url);
                if (SyncService.IsLoggedIn) _ = SyncService.DeleteBookmarkAsync(url);
                MessageBox.Show("Bookmark removed", "RUHI Browser", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _bookmarks.Add(new Bookmark(title, url));
                DataManager.SaveBookmarks(_bookmarks);
                UpdateBookmarkButton(url);
                if (SyncService.IsLoggedIn) _ = PerformFullSyncAsync();
                MessageBox.Show("Bookmark added!", "RUHI Browser", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void OnReaderModeClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.WebView?.CoreWebView2 == null || string.IsNullOrWhiteSpace(_activeTab.Url)) return;

            const string readerScript = @"(function() {
                if (window.__ruhiReaderActive) {
                    location.reload();
                    return;
                }
                window.__ruhiReaderActive = true;
                const article = document.querySelector('article') || document.querySelector('main') || document.body;
                const title = document.title;
                const content = article.innerHTML;
                document.body.innerHTML = `
                    <div style='max-width:740px;margin:40px auto;padding:24px;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,sans-serif;line-height:1.8;color:#E2E8F0;background:#0F172A;border-radius:12px;'>
                        <h1 style='color:#38BDF8;font-size:30px;line-height:1.3;'>${title}</h1>
                        <hr style='border:0;border-top:1px solid #3C4043;margin:20px 0;'>
                        <div style='font-size:18px;'>${content}</div>
                    </div>`;
                document.body.style.background = '#181818';
            })();";

            try
            {
                await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(readerScript);
            }
            catch { }
        }

        private void OnWebViewDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
        {
            try
            {
                var folder = !string.IsNullOrWhiteSpace(_settings.DownloadFolder) && Directory.Exists(_settings.DownloadFolder)
                    ? _settings.DownloadFolder
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (!Directory.Exists(folder))
                {
                    try { Directory.CreateDirectory(folder); } catch { }
                }

                if (Directory.Exists(folder))
                {
                    var filename = Path.GetFileName(e.ResultFilePath);
                    e.ResultFilePath = Path.Combine(folder, filename);
                }

                // Suppress default Edge download UI so our RUHI Download Hub takes over
                e.Handled = true;

                var op = e.DownloadOperation;
                if (op == null) return;

                var downloadItem = new DownloadItem
                {
                    FileName = Path.GetFileName(e.ResultFilePath),
                    FilePath = e.ResultFilePath,
                    Url = op.Uri ?? string.Empty,
                    Operation = op,
                    StatusText = "Downloading...",
                    StartTime = DateTime.Now
                };

                lock (_downloads)
                {
                    _downloads.Insert(0, downloadItem);
                }

                _speedTrackers[downloadItem.Id] = (0, DateTime.Now);

                DateTime lastUiDispatch = DateTime.MinValue;

                // Wire events for download operation
                op.BytesReceivedChanged += (s, ev) =>
                {
                    var now = DateTime.Now;
                    if ((now - lastUiDispatch).TotalMilliseconds < 250)
                    {
                        return;
                    }
                    lastUiDispatch = now;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var now = DateTime.Now;
                        downloadItem.BytesReceived = (long)op.BytesReceived;
                        if (op.TotalBytesToReceive.HasValue && op.TotalBytesToReceive.Value > 0)
                        {
                            downloadItem.TotalBytes = (long)op.TotalBytesToReceive.Value;
                            downloadItem.ProgressPercentage = ((double)downloadItem.BytesReceived / downloadItem.TotalBytes) * 100.0;
                        }

                        if (_speedTrackers.TryGetValue(downloadItem.Id, out var tracker))
                        {
                            var elapsed = (now - tracker.LastTime).TotalSeconds;
                            if (elapsed >= 0.5)
                            {
                                var diff = downloadItem.BytesReceived - tracker.LastBytes;
                                if (diff >= 0)
                                {
                                    var bytesPerSec = diff / elapsed;
                                    downloadItem.SpeedText = $"{DownloadItem.FormatBytes((long)bytesPerSec)}/s";

                                    if (downloadItem.TotalBytes > 0 && bytesPerSec > 0)
                                    {
                                        var remainBytes = downloadItem.TotalBytes - downloadItem.BytesReceived;
                                        var remainSec = remainBytes / bytesPerSec;
                                        if (remainSec < 60) downloadItem.TimeRemainingText = $"{(int)remainSec}s left";
                                        else downloadItem.TimeRemainingText = $"{(int)(remainSec / 60)}m {(int)(remainSec % 60)}s left";
                                    }
                                }
                                _speedTrackers[downloadItem.Id] = (downloadItem.BytesReceived, now);
                            }
                        }
                        else
                        {
                            _speedTrackers[downloadItem.Id] = (downloadItem.BytesReceived, now);
                        }

                        UpdateTopBarDownloadProgress();
                        if (PopupDownloads.IsOpen)
                        {
                            UpdateActiveDownloadCard(downloadItem);
                        }
                    }));
                };

                op.StateChanged += (s, ev) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (op.State == CoreWebView2DownloadState.Completed)
                        {
                            downloadItem.IsCompleted = true;
                            downloadItem.CanPause = false;
                            downloadItem.CanResume = false;
                            downloadItem.StatusText = "Completed";
                            downloadItem.ProgressPercentage = 100;
                            downloadItem.SpeedText = string.Empty;
                            downloadItem.TimeRemainingText = string.Empty;
                            downloadItem.EndTime = DateTime.Now;

                            DownloadHistoryService.AddRecord(downloadItem);
                            TriggerDownloadCompletedAnimation();

                            if (downloadItem.IsExtensionArchive && File.Exists(downloadItem.FilePath))
                            {
                                AutoInstallDownloadedExtension(downloadItem);
                            }
                        }
                        else if (op.State == CoreWebView2DownloadState.Interrupted)
                        {
                            downloadItem.IsFailed = true;
                            downloadItem.CanPause = false;
                            downloadItem.CanResume = op.CanResume;
                            downloadItem.StatusText = op.InterruptReason == CoreWebView2DownloadInterruptReason.UserCanceled ? "Cancelled" : "Failed";
                            downloadItem.SpeedText = string.Empty;
                            downloadItem.TimeRemainingText = string.Empty;
                            downloadItem.EndTime = DateTime.Now;

                            DownloadHistoryService.AddRecord(downloadItem);
                        }

                        UpdateTopBarDownloadProgress();
                        if (PopupDownloads.IsOpen)
                        {
                            RenderDownloadsHub();
                        }
                    }));
                };

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateTopBarDownloadProgress();
                    PopupDownloads.IsOpen = true;
                    RenderDownloadsHub();
                }));
            }
            catch { }
        }

        private async void AutoInstallDownloadedExtension(DownloadItem item)
        {
            try
            {
                var profile = _activeTab?.WebView?.CoreWebView2?.Profile;
                if (profile == null)
                {
                    profile = _tabs.FirstOrDefault(t => t.WebView?.CoreWebView2?.Profile != null)?.WebView?.CoreWebView2?.Profile;
                }

                if (profile != null)
                {
                    var ext = await ExtensionManagerService.InstallExtensionFromArchiveAsync(item.FilePath, profile);
                    string extName = ext?.Name ?? Path.GetFileNameWithoutExtension(item.FileName);

                    _settings = DataManager.LoadSettings();
                    string installMsg = $"Extension \"{extName}\" was automatically installed and activated successfully in RUHI Browser! 🧩";
                    if (_settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(_settings.ActiveExtensionNewTabUrl))
                    {
                        installMsg += $"\n\nNew Tab / Homepage has been set to {_settings.ActiveExtensionNewTabName ?? extName}!";
                        if (_activeTab != null && (string.IsNullOrEmpty(_activeTab.Url) || _activeTab.Url == "about:blank"))
                        {
                            NavigateTab(_activeTab, _settings.ActiveExtensionNewTabUrl);
                        }
                        else
                        {
                            CreateNewTab(_settings.ActiveExtensionNewTabUrl);
                        }
                    }

                    MessageBox.Show(
                        this,
                        installMsg,
                        "Extension Installed 🧩",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Auto refresh ExtensionsWindow if open
                    foreach (Window win in Application.Current.Windows)
                    {
                        if (win is ExtensionsWindow extWin)
                        {
                            _ = extWin.LoadExtensionsAsync();
                        }
                    }
                }
                else
                {
                    PromptInstallDownloadedCrx(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not auto-install extension \"{item.FileName}\":\n{ex.Message}",
                    "Extension Install Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private async void PromptInstallDownloadedCrx(DownloadItem item)
        {
            try
            {
                var result = MessageBox.Show(
                    this,
                    $"Chrome Extension archive downloaded:\n\"{item.FileName}\"\n\nWould you like to install and activate this extension in RUHI Browser now?",
                    "Install Chrome Extension 🧩",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    var profile = _activeTab?.WebView?.CoreWebView2?.Profile;
                    if (profile != null)
                    {
                        var ext = await ExtensionManagerService.InstallExtensionFromArchiveAsync(item.FilePath, profile);
                        _settings = DataManager.LoadSettings();
                        string msg = $"Extension \"{ext?.Name ?? item.FileName}\" was installed and activated successfully in RUHI Browser!";
                        if (_settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(_settings.ActiveExtensionNewTabUrl))
                        {
                            msg += $"\n\nNew Tab page has been set to {ext?.Name ?? item.FileName}!";
                            if (_activeTab != null && (string.IsNullOrEmpty(_activeTab.Url) || _activeTab.Url == "about:blank"))
                            {
                                NavigateTab(_activeTab, _settings.ActiveExtensionNewTabUrl);
                            }
                        }
                        MessageBox.Show(
                            this,
                            msg,
                            "Extension Installed 🧩",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                    else
                    {
                        MessageBox.Show(this, "Browser profile is not ready. You can install it from Extensions Manager (🧩).", "Extension Install", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not install extension:\n" + ex.Message, "Extension Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDownloadsClicked(object sender, RoutedEventArgs e)
        {
            PopupDownloads.IsOpen = !PopupDownloads.IsOpen;
            if (PopupDownloads.IsOpen)
            {
                RenderDownloadsHub();
            }
        }

        private void OnOpenDownloadsFolderClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                string folder = !string.IsNullOrWhiteSpace(_settings.DownloadFolder) && Directory.Exists(_settings.DownloadFolder)
                    ? _settings.DownloadFolder
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open Downloads folder: " + ex.Message, "Downloads", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnClearDownloadsListClicked(object sender, RoutedEventArgs e)
        {
            lock (_downloads)
            {
                _downloads.RemoveAll(d => d.IsCompleted || d.IsFailed);
            }
            DownloadHistoryService.ClearHistory();
            RenderDownloadsHub();
            UpdateTopBarDownloadProgress();
        }

        private void UpdateTopBarDownloadProgress()
        {
            if (BadgeActiveDownloads == null || TbDownloadIcon == null || BtnDownloads == null) return;

            var allActiveOrPaused = _downloads.Where(d => !d.IsCompleted && !d.IsFailed).ToList();
            if (allActiveOrPaused.Count > 0)
            {
                BadgeActiveDownloads.Visibility = Visibility.Visible;
                TbActiveDownloadCount.Text = allActiveOrPaused.Count.ToString();

                long totalReceived = 0;
                long totalExpected = 0;
                foreach (var d in allActiveOrPaused)
                {
                    totalReceived += d.BytesReceived;
                    if (d.TotalBytes > 0)
                    {
                        totalExpected += d.TotalBytes;
                    }
                    else
                    {
                        totalExpected += Math.Max(d.BytesReceived, 1024 * 1024);
                    }
                }

                double prog = totalExpected > 0 ? Math.Clamp((double)totalReceived / totalExpected, 0.0, 0.99) : 0.5;
                UpdateDownloadRing(prog);

                TbDownloadIcon.Text = "⬇";
                var isDark = ThemeManager.IsDarkTheme(_settings.Theme);
                TbDownloadIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#38BDF8" : "#2563EB"));
                BtnDownloads.ToolTip = $"Downloads ({allActiveOrPaused.Count} active) • {(int)(prog * 100)}%";
            }
            else
            {
                BadgeActiveDownloads.Visibility = Visibility.Collapsed;
                if (PathDownloadRing != null && PathDownloadRing.Visibility == Visibility.Visible && TbDownloadIcon.Text == "⬇")
                {
                    PathDownloadRing.Visibility = Visibility.Collapsed;
                    PathDownloadRing.Data = null;
                }
                var isDark = ThemeManager.IsDarkTheme(_settings.Theme);
                TbDownloadIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));
                BtnDownloads.ToolTip = "Downloads (Ctrl+J)";
            }
        }

        private void UpdateDownloadRing(double progress)
        {
            if (PathDownloadRing == null) return;

            if (progress <= 0.001)
            {
                PathDownloadRing.Data = null;
                PathDownloadRing.Visibility = Visibility.Collapsed;
                return;
            }

            PathDownloadRing.Visibility = Visibility.Visible;

            if (progress >= 0.999)
            {
                PathDownloadRing.Data = new EllipseGeometry(new Point(14, 14), 12, 12);
                PathDownloadRing.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                return;
            }

            PathDownloadRing.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));

            double angle = progress * 360.0;
            double rad = (angle - 90) * Math.PI / 180.0;
            double cx = 14;
            double cy = 14;
            double r = 12;
            double startX = cx;
            double startY = cy - r;
            double endX = cx + r * Math.Cos(rad);
            double endY = cy + r * Math.Sin(rad);

            var figure = new PathFigure
            {
                StartPoint = new Point(startX, startY),
                IsClosed = false
            };
            var arc = new ArcSegment
            {
                Point = new Point(endX, endY),
                Size = new Size(r, r),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = angle > 180.0
            };
            figure.Segments.Add(arc);

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            PathDownloadRing.Data = geometry;
        }

        private async void TriggerDownloadCompletedAnimation()
        {
            try
            {
                UpdateDownloadRing(1.0);
                if (PathDownloadRing != null)
                {
                    PathDownloadRing.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                }
                if (TbDownloadIcon != null)
                {
                    TbDownloadIcon.Text = "✓";
                    TbDownloadIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                }

                await Task.Delay(4000);

                if (!_downloads.Any(d => !d.IsCompleted && !d.IsFailed))
                {
                    if (PathDownloadRing != null)
                    {
                        PathDownloadRing.Visibility = Visibility.Collapsed;
                        PathDownloadRing.Data = null;
                    }
                    if (TbDownloadIcon != null)
                    {
                        TbDownloadIcon.Text = "⬇";
                        var isDark = ThemeManager.IsDarkTheme(_settings.Theme);
                        TbDownloadIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));
                    }
                    if (BtnDownloads != null)
                    {
                        BtnDownloads.ToolTip = "Downloads (Ctrl+J)";
                    }
                }
            }
            catch { }
        }

        private void RenderDownloadsHub()
        {
            if (PanelDownloadsList == null) return;
            PanelDownloadsList.Children.Clear();

            var isDark = ThemeManager.IsDarkTheme(_settings.Theme);
            var cardBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#202124" : "#F8FAFC"));
            var cardBorder = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));
            var textFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            var mutedFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));
            var iconBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));

            List<DownloadItem> allItems;
            lock (_downloads)
            {
                allItems = _downloads.ToList();
            }

            if (allItems.Count == 0)
            {
                PanelDownloadsEmpty.Visibility = Visibility.Visible;
                TbDownloadsHeaderCount.Text = "";
                TbDownloadsFooterStatus.Text = "No active downloads";
                TbDownloadsSpeedBadge.Text = "";
                return;
            }

            PanelDownloadsEmpty.Visibility = Visibility.Collapsed;

            var activeCount = allItems.Count(d => !d.IsCompleted && !d.IsFailed && !d.IsPaused);
            var pausedCount = allItems.Count(d => d.IsPaused);
            TbDownloadsHeaderCount.Text = activeCount > 0 ? $"({activeCount} downloading)" : (pausedCount > 0 ? $"({pausedCount} paused)" : $"({allItems.Count} total)");

            if (activeCount > 0)
            {
                TbDownloadsFooterStatus.Text = $"{activeCount} file(s) downloading...";
            }
            else if (pausedCount > 0)
            {
                TbDownloadsFooterStatus.Text = $"{pausedCount} file(s) paused";
            }
            else
            {
                TbDownloadsFooterStatus.Text = "All downloads complete";
            }

            foreach (var item in allItems)
            {
                var card = new Border
                {
                    Tag = item.Id,
                    Background = cardBg,
                    BorderBrush = cardBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(10, 4, 10, 4),
                    Padding = new Thickness(10, 8, 10, 8)
                };

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Row 0: Icon + File name + Status badge
                var row0 = new Grid();
                row0.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row0.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row0.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var iconBox = new Border
                {
                    Width = 32,
                    Height = 32,
                    CornerRadius = new CornerRadius(6),
                    Background = iconBg,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                iconBox.Child = new TextBlock
                {
                    Text = item.FileIcon,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(iconBox, 0);
                row0.Children.Add(iconBox);

                var nameStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                var tbName = new TextBlock
                {
                    Text = item.FileName,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12.5,
                    Foreground = textFg,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    ToolTip = item.FilePath
                };
                nameStack.Children.Add(tbName);

                var tbDetails = new TextBlock
                {
                    Tag = $"details_{item.Id}",
                    Text = BuildDetailsText(item),
                    FontSize = 11,
                    Foreground = mutedFg,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                nameStack.Children.Add(tbDetails);
                Grid.SetColumn(nameStack, 1);
                row0.Children.Add(nameStack);

                var statusPill = CreateStatusBadge(item);
                Grid.SetColumn(statusPill, 2);
                row0.Children.Add(statusPill);

                Grid.SetRow(row0, 0);
                grid.Children.Add(row0);

                // Row 1: Progress bar (if not completed)
                if (!item.IsCompleted)
                {
                    var pb = new ProgressBar
                    {
                        Tag = $"pb_{item.Id}",
                        Height = 4,
                        Margin = new Thickness(0, 8, 0, 4),
                        Minimum = 0,
                        Maximum = 100,
                        Value = item.ProgressPercentage,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#0F172A" : "#E2E8F0")),
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(
                            item.IsPaused ? "#F59E0B" : (item.IsFailed ? "#EF4444" : "#3B82F6")
                        )),
                        BorderThickness = new Thickness(0)
                    };
                    Grid.SetRow(pb, 1);
                    grid.Children.Add(pb);
                }

                // Row 2: Action Buttons
                var actionsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 6, 0, 0)
                };

                if (!item.IsCompleted && !item.IsFailed)
                {
                    if (item.IsPaused)
                    {
                        var btnResume = CreateActionBtn("▶ Resume", "#3B82F6", (s, ev) =>
                        {
                            item.Resume();
                            RenderDownloadsHub();
                            UpdateTopBarDownloadProgress();
                        });
                        actionsPanel.Children.Add(btnResume);
                    }
                    else
                    {
                        var btnPause = CreateActionBtn("⏸ Pause", isDark ? "#94A3B8" : "#475569", (s, ev) =>
                        {
                            item.Pause();
                            RenderDownloadsHub();
                            UpdateTopBarDownloadProgress();
                        });
                        actionsPanel.Children.Add(btnPause);
                    }

                    var btnCancel = CreateActionBtn("✕ Cancel", "#EF4444", (s, ev) =>
                    {
                        item.Cancel();
                        RenderDownloadsHub();
                        UpdateTopBarDownloadProgress();
                    });
                    actionsPanel.Children.Add(btnCancel);
                }
                else if (item.IsCompleted)
                {
                    if (item.IsExtensionArchive && File.Exists(item.FilePath))
                    {
                        var btnInstall = CreateActionBtn("🧩 Install", "#8B5CF6", async (s, ev) =>
                        {
                            try
                            {
                                var profile = _activeTab?.WebView?.CoreWebView2?.Profile;
                                if (profile != null)
                                {
                                    var ext = await ExtensionManagerService.InstallExtensionFromArchiveAsync(item.FilePath, profile);
                                    _settings = DataManager.LoadSettings();
                                    string extName = ext?.Name ?? item.FileName;
                                    string msg = $"Extension \"{extName}\" was installed and activated successfully in RUHI Browser! 🧩";
                                    if (_settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(_settings.ActiveExtensionNewTabUrl))
                                    {
                                        msg += $"\n\nNew Tab / Homepage has been set to {_settings.ActiveExtensionNewTabName ?? extName}!";
                                        if (_activeTab != null && (string.IsNullOrEmpty(_activeTab.Url) || _activeTab.Url == "about:blank"))
                                        {
                                            NavigateTab(_activeTab, _settings.ActiveExtensionNewTabUrl);
                                        }
                                        else
                                        {
                                            CreateNewTab(_settings.ActiveExtensionNewTabUrl);
                                        }
                                    }
                                    MessageBox.Show(
                                        this,
                                        msg,
                                        "Extension Installed 🧩",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information
                                    );
                                }
                                else
                                {
                                    MessageBox.Show(this, "Browser profile is not ready.", "Install Extension", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, "Could not install extension:\n" + ex.Message, "Extension Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                        actionsPanel.Children.Add(btnInstall);
                    }

                    var btnOpen = CreateActionBtn("🚀 Open", "#10B981", (s, ev) =>
                    {
                        item.OpenFile();
                    });
                    actionsPanel.Children.Add(btnOpen);

                    var btnFolder = CreateActionBtn("📁 Show in Folder", isDark ? "#38BDF8" : "#0284C7", (s, ev) =>
                    {
                        item.ShowInFolder();
                    });
                    actionsPanel.Children.Add(btnFolder);
                }
                else if (item.IsFailed)
                {
                    if (File.Exists(item.FilePath))
                    {
                        var btnFolder = CreateActionBtn("📁 Show in Folder", isDark ? "#38BDF8" : "#0284C7", (s, ev) =>
                        {
                            item.ShowInFolder();
                        });
                        actionsPanel.Children.Add(btnFolder);
                    }

                    var btnRemove = CreateActionBtn("🗑 Remove", isDark ? "#94A3B8" : "#64748B", (s, ev) =>
                    {
                        lock (_downloads) { _downloads.Remove(item); }
                        RenderDownloadsHub();
                        UpdateTopBarDownloadProgress();
                    });
                    actionsPanel.Children.Add(btnRemove);
                }

                Grid.SetRow(actionsPanel, 2);
                grid.Children.Add(actionsPanel);

                card.Child = grid;
                PanelDownloadsList.Children.Add(card);
            }
        }

        private void UpdateActiveDownloadCard(DownloadItem item)
        {
            if (PanelDownloadsList == null) return;

            bool found = false;
            foreach (UIElement child in PanelDownloadsList.Children)
            {
                if (child is Border card && Equals(card.Tag, item.Id))
                {
                    found = true;
                    if (card.Child is Grid grid)
                    {
                        foreach (UIElement elem in grid.Children)
                        {
                            if (elem is Grid row0)
                            {
                                foreach (UIElement sub in row0.Children)
                                {
                                    if (sub is StackPanel nameStack)
                                    {
                                        foreach (UIElement nameElem in nameStack.Children)
                                        {
                                            if (nameElem is TextBlock tb && Equals(tb.Tag, $"details_{item.Id}"))
                                            {
                                                tb.Text = BuildDetailsText(item);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (elem is ProgressBar pb && Equals(pb.Tag, $"pb_{item.Id}"))
                            {
                                pb.Value = item.ProgressPercentage;
                            }
                        }
                    }
                    break;
                }
            }

            if (!found)
            {
                RenderDownloadsHub();
            }
        }

        private static string BuildDetailsText(DownloadItem item)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(item.TransferredText))
            {
                parts.Add(item.TransferredText);
            }
            if (!string.IsNullOrWhiteSpace(item.SpeedText) && !item.IsCompleted && !item.IsPaused)
            {
                parts.Add(item.SpeedText);
            }
            if (!string.IsNullOrWhiteSpace(item.TimeRemainingText) && !item.IsCompleted && !item.IsPaused)
            {
                parts.Add(item.TimeRemainingText);
            }
            return string.Join(" • ", parts);
        }

        private static Border CreateStatusBadge(DownloadItem item)
        {
            string text = item.StatusText;
            string bg = "#383838";
            string fg = "#94A3B8";

            if (item.IsCompleted)
            {
                bg = "#064E3B";
                fg = "#34D399";
                text = "Completed";
            }
            else if (item.IsPaused)
            {
                bg = "#78350F";
                fg = "#FBBF24";
                text = "Paused";
            }
            else if (item.IsFailed || item.StatusText == "Cancelled")
            {
                bg = "#7F1D1D";
                fg = "#F87171";
                text = item.StatusText;
            }
            else
            {
                bg = "#1E3A8A";
                fg = "#60A5FA";
                text = $"{item.ProgressPercentage:F0}%";
            }

            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bg)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0)
            };
            border.Child = new TextBlock
            {
                Text = text,
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fg))
            };
            return border;
        }

        private static Button CreateActionBtn(string text, string colorHex, RoutedEventHandler handler)
        {
            var btn = new Button
            {
                Content = text,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(6, 3, 6, 3),
                Margin = new Thickness(4, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.Click += handler;
            return btn;
        }

        private void OnTranslateClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null || string.IsNullOrWhiteSpace(_activeTab.Url) || _activeTab.Url.StartsWith("about:"))
            {
                MessageBox.Show(this, "Please navigate to a webpage first before translating.", "Translate Page", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new TranslateWindow { Owner = this };
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.SelectedLanguageCode))
            {
                string targetLang = win.SelectedLanguageCode;
                string translateUrl = $"https://translate.google.com/translate?sl=auto&tl={targetLang}&u={Uri.EscapeDataString(_activeTab.Url)}";
                NavigateTab(_activeTab, translateUrl);
            }
        }

        private void OnAiClicked(object sender, RoutedEventArgs e)
        {
            string title = _activeTab?.Title ?? string.Empty;
            string url = _activeTab?.Url ?? string.Empty;

            var aiWin = new AiAssistantWindow(title, url) { Owner = this };
            aiWin.Show();
        }

        private void OnShareClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null && !string.IsNullOrWhiteSpace(_activeTab.Url))
            {
                try
                {
                    Clipboard.SetDataObject(_activeTab.Url, true);
                    MessageBox.Show(this, $"Page link copied to clipboard!\n\n{_activeTab.Url}", "RUHI Browser - Share", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not copy link to clipboard: " + ex.Message, "RUHI Browser - Share", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(this, "No active page URL to copy.", "RUHI Browser - Share", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnPdfEditClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null) return;

            var menu = CreateModernContextMenu();

            MenuItem CreatePdfItem(string header, RoutedEventHandler handler) => CreateModernMenuItem(header, handler);

            menu.Items.Add(CreatePdfItem("✏️ Draw / Pen (पेन से ड्रा या लिखें)", async (s, ev) =>
            {
                if (_activeTab?.WebView?.CoreWebView2 != null)
                {
                    _activeTab.WebView.Focus();
                    try
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#ink')?.click();"
                        );
                    }
                    catch { }
                }
            }));

            menu.Items.Add(CreatePdfItem("🖍️ Highlight Text (टेक्स्ट हाइलाइट करें)", async (s, ev) =>
            {
                if (_activeTab?.WebView?.CoreWebView2 != null)
                {
                    _activeTab.WebView.Focus();
                    try
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#highlight')?.click();"
                        );
                    }
                    catch { }
                }
            }));

            menu.Items.Add(CreatePdfItem("🔤 Add Text Note (टेक्स्ट नोट लिखें)", async (s, ev) =>
            {
                if (_activeTab?.WebView?.CoreWebView2 != null)
                {
                    _activeTab.WebView.Focus();
                    try
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#text-annotation')?.click();"
                        );
                    }
                    catch { }
                }
            }));

            menu.Items.Add(new Separator());

            menu.Items.Add(CreatePdfItem("🔄 Rotate 90° (पेज घुमाएँ)", async (s, ev) =>
            {
                if (_activeTab?.WebView?.CoreWebView2 != null)
                {
                    try
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#rotate-cw')?.click();"
                        );
                    }
                    catch { }
                }
            }));

            menu.Items.Add(CreatePdfItem("💾 Save Edited PDF (Ctrl+S)", async (s, ev) =>
            {
                if (_activeTab?.WebView?.CoreWebView2 != null)
                {
                    _activeTab.WebView.Focus();
                    try
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#save')?.click();"
                        );
                    }
                    catch { }
                }
            }));

            menu.Items.Add(CreatePdfItem("🖨️ Print PDF (Ctrl+P)", async (s, ev) =>
            {
                if (_activeTab?.WebView?.CoreWebView2 != null)
                {
                    _activeTab.WebView.Focus();
                    try
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                            "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#print')?.click();"
                        );
                    }
                    catch { }
                }
            }));

            menu.Items.Add(new Separator());

            menu.Items.Add(CreatePdfItem("🛠️ PDF Tools & Options Window...", (s, ev) =>
            {
                OpenPdfToolsWindow(_activeTab?.Url);
            }));

            menu.PlacementTarget = BtnPdfEdit;
            menu.IsOpen = true;
        }

        private void OpenPdfToolsWindow(string? pdfPath = null)
        {
            var win = new PdfToolsWindow(pdfPath) { Owner = this };
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.PdfPath))
            {
                var targetUri = UrlResolver.Resolve(win.PdfPath);

                if (win.SelectedAction == "OpenTab")
                {
                    if (_activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
                    {
                        NavigateTab(_activeTab, targetUri);
                    }
                    else
                    {
                        CreateNewTab(targetUri);
                    }
                }
                else if (win.SelectedAction == "Rotate")
                {
                    if (_activeTab == null || !string.Equals(_activeTab.Url, targetUri, StringComparison.OrdinalIgnoreCase))
                    {
                        CreateNewTab(targetUri);
                    }
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
                    {
                        await Task.Delay(700);
                        if (_activeTab?.WebView?.CoreWebView2 != null)
                        {
                            await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                                "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#rotate-cw')?.click();"
                            );
                        }
                    }));
                }
                else if (win.SelectedAction == "Print")
                {
                    if (_activeTab == null || !string.Equals(_activeTab.Url, targetUri, StringComparison.OrdinalIgnoreCase))
                    {
                        CreateNewTab(targetUri);
                    }
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(async () =>
                    {
                        await Task.Delay(700);
                        if (_activeTab?.WebView?.CoreWebView2 != null)
                        {
                            await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(
                                "document.querySelector('pdf-viewer')?.shadowRoot?.querySelector('#print')?.click();"
                            );
                        }
                    }));
                }
            }
        }

        private void OpenBookmarksWindow()
        {
            var win = new BookmarksWindow { Owner = this };
            var result = win.ShowDialog();
            _bookmarks = DataManager.LoadBookmarks();
            UpdateBookmarkButton(_activeTab?.Url ?? "");
            if (result == true && !string.IsNullOrWhiteSpace(win.SelectedUrl) && _activeTab != null)
            {
                NavigateTab(_activeTab, win.SelectedUrl);
            }
        }

        private void OpenHistoryWindow()
        {
            var win = new HistoryWindow { Owner = this };
            var result = win.ShowDialog();
            RenderHomeScreenRecentVisits();
            if (result == true && !string.IsNullOrWhiteSpace(win.SelectedUrl) && _activeTab != null)
            {
                NavigateTab(_activeTab, win.SelectedUrl);
            }
        }

        private void OpenPasswordsWindow()
        {
            var win = new PasswordsWindow { Owner = this };
            win.ShowDialog();
        }

        private ExtensionsWindow? _extensionsWindow;

        private void OpenExtensionsWindow()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(async () =>
            {
                try
                {
                    if (_extensionsWindow != null && _extensionsWindow.IsLoaded)
                    {
                        _extensionsWindow.Activate();
                        if (_extensionsWindow.WindowState == WindowState.Minimized)
                        {
                            _extensionsWindow.WindowState = WindowState.Normal;
                        }
                        return;
                    }

                    if (_activeTab?.WebView != null && _activeTab.WebView.CoreWebView2 == null)
                    {
                        await _activeTab.WebView.EnsureCoreWebView2Async();
                    }

                    var profile = _activeTab?.WebView?.CoreWebView2?.Profile;
                    if (profile == null)
                    {
                        foreach (var tab in _tabs)
                        {
                            if (tab.WebView?.CoreWebView2?.Profile != null)
                            {
                                profile = tab.WebView.CoreWebView2.Profile;
                                break;
                            }
                        }
                    }

                    _extensionsWindow = new ExtensionsWindow(profile, url =>
                    {
                        if (_activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
                        {
                            NavigateTab(_activeTab, url);
                        }
                        else
                        {
                            CreateNewTab(url);
                        }
                    }) { Owner = this };

                    _extensionsWindow.Closed += (s, e) =>
                    {
                        _extensionsWindow = null;
                        _ = NotifyStoreTabsExtensionChangedAsync();
                    };

                    _extensionsWindow.Show();
                    _extensionsWindow.Activate();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error opening ExtensionsWindow: " + ex.Message);
                }
            }));
        }

        private void OpenRuhiStore()
        {
            var storeUrl = UrlResolver.GetStoreUrl();
            if (_activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
            {
                NavigateTab(_activeTab, storeUrl);
            }
            else
            {
                CreateNewTab(storeUrl);
            }
        }

        private void OnExtensionsClicked(object sender, RoutedEventArgs e)
        {
            OpenExtensionsWindow();
        }

        private void OpenSettingsWindow()
        {
            var win = new SettingsWindow
            {
                Owner = this,
                ClearDataHandler = async (hist, cookies, cache, bookmarks) =>
                {
                    if (hist)
                    {
                        _ = SyncService.ClearAllHistoryAsync();
                    }
                    if (bookmarks)
                    {
                        _bookmarks.Clear();
                        UpdateBookmarkButton(_activeTab?.Url ?? "");
                    }
                    RenderHomeScreenRecentVisits();
                    await ClearWebView2DataAsync(hist, cookies, cache);
                }
            };

            win.ShowDialog();
            _settings = DataManager.LoadSettings();
            AdBlockManager.IsEnabled = _settings.AdBlockEnabled;
            ApplyHomeScreenStyle();
            ApplyTheme();
        }

        private async Task ClearWebView2DataAsync(bool history, bool cookies, bool cache)
        {
            try
            {
                CoreWebView2BrowsingDataKinds kinds = 0;
                if (cookies) kinds |= CoreWebView2BrowsingDataKinds.Cookies;
                if (cache) kinds |= CoreWebView2BrowsingDataKinds.DiskCache | CoreWebView2BrowsingDataKinds.CacheStorage;
                if (history) kinds |= CoreWebView2BrowsingDataKinds.BrowsingHistory;

                if (kinds != 0)
                {
                    foreach (var tab in _tabs)
                    {
                        if (tab.WebView?.CoreWebView2?.Profile != null)
                        {
                            await tab.WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(kinds);
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        private void OpenDevTools()
        {
            try
            {
                _activeTab?.WebView?.CoreWebView2?.OpenDevToolsWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open Developer Tools: " + ex.Message, "Developer Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private const string UnlockCopyScript = @"
(function() {
    try {
        var host = window.location.hostname || '';
        if (host.includes('docs.google.com') || host.includes('sheets.google.com') ||
            host.includes('figma.com') || host.includes('canva.com') ||
            host.includes('codesandbox.io') || host.includes('github.dev') ||
            host.includes('trello.com') || host.includes('miro.com')) {
            return;
        }

        var isEditable = function(el) {
            if (!el) return false;
            var tag = el.tagName ? el.tagName.toLowerCase() : '';
            return tag === 'input' || tag === 'textarea' || el.isContentEditable;
        };

        var allow = function(e) {
            if (isEditable(e.target)) return;
            e.stopPropagation();
        };

        ['copy', 'cut'].forEach(function(evt) {
            document.addEventListener(evt, allow, false);
        });

        var style = document.createElement('style');
        style.textContent = 'body, p, span, div, h1, h2, h3, h4, h5, h6, li, td, th { -webkit-user-select: text !important; user-select: text !important; }';
        (document.head || document.documentElement).appendChild(style);
    } catch(e) {}
})();
";

        private async void CopySelectedTextAsync(WebView2 webView)
        {
            try
            {
                await webView.ExecuteScriptAsync("document.execCommand('copy');");
                var json = await webView.ExecuteScriptAsync("window.getSelection() ? window.getSelection().toString() : '';");
                if (!string.IsNullOrEmpty(json) && json != "\"\"" && json != "null")
                {
                    var text = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            try
                            {
                                Clipboard.SetDataObject(text, true);
                                break;
                            }
                            catch
                            {
                                await Task.Delay(50);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private const string EdgePipScript = @"
(function() {
    if (window.__ruhiEdgePipInjected) return;
    window.__ruhiEdgePipInjected = true;

    try {
        var style = document.createElement('style');
        style.id = 'ruhi-edge-pip-style';
        style.textContent = `
            .ruhi-edge-pip-btn {
                position: fixed !important;
                z-index: 2147483647 !important;
                display: inline-flex !important;
                align-items: center !important;
                gap: 7px !important;
                padding: 6px 14px !important;
                background: rgba(15, 23, 42, 0.92) !important;
                color: #FFFFFF !important;
                border: 1px solid rgba(255, 255, 255, 0.22) !important;
                border-radius: 20px !important;
                box-shadow: 0 4px 18px rgba(0, 0, 0, 0.5) !important;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif !important;
                font-size: 12px !important;
                font-weight: 600 !important;
                cursor: pointer !important;
                opacity: 0 !important;
                pointer-events: none !important;
                transition: opacity 0.2s ease, transform 0.2s ease, background 0.15s ease, border-color 0.15s ease !important;
                transform: translateX(-50%) translateY(-6px) !important;
                user-select: none !important;
                backdrop-filter: blur(10px) !important;
                -webkit-backdrop-filter: blur(10px) !important;
            }
            .ruhi-edge-pip-btn:hover {
                background: rgba(30, 41, 59, 0.98) !important;
                border-color: #38BDF8 !important;
                box-shadow: 0 6px 22px rgba(56, 189, 248, 0.35) !important;
                transform: translateX(-50%) translateY(-6px) scale(1.03) !important;
            }
            .ruhi-edge-pip-btn.ruhi-pip-visible {
                opacity: 1 !important;
                pointer-events: auto !important;
                transform: translateX(-50%) translateY(0) !important;
            }
            .ruhi-edge-pip-btn svg {
                width: 14px !important;
                height: 14px !important;
                fill: none !important;
                stroke: currentColor !important;
                stroke-width: 2 !important;
                stroke-linecap: round !important;
                stroke-linejoin: round !important;
            }
        `;
        (document.head || document.documentElement).appendChild(style);

        var activeBtn = null;
        var hideTimeout = null;
        var currentVideo = null;

        function getOrCreatePipButton() {
            if (activeBtn && document.body && document.body.contains(activeBtn)) return activeBtn;
            
            var btn = document.createElement('button');
            btn.className = 'ruhi-edge-pip-btn';
            btn.innerHTML = `<svg viewBox=""0 0 24 24""><rect x=""2"" y=""4"" width=""20"" height=""16"" rx=""2""></rect><path d=""M13 11h7v7h-7z"" fill=""currentColor""></path></svg><span>Picture in picture</span>`;

            btn.addEventListener('click', async function(e) {
                e.stopPropagation();
                e.preventDefault();
                if (!currentVideo) return;
                try {
                    if (document.pictureInPictureElement) {
                        await document.exitPictureInPicture();
                    } else {
                        await currentVideo.requestPictureInPicture();
                    }
                } catch (err) { }
            });

            btn.addEventListener('mouseenter', function() {
                if (hideTimeout) clearTimeout(hideTimeout);
                btn.classList.add('ruhi-pip-visible');
            });

            btn.addEventListener('mouseleave', function() {
                scheduleHide();
            });

            if (document.body) {
                document.body.appendChild(btn);
            } else {
                document.documentElement.appendChild(btn);
            }
            activeBtn = btn;
            return btn;
        }

        function showButtonOnVideo(video) {
            if (!video) return;
            var rect = video.getBoundingClientRect();
            if (rect.width < 140 || rect.height < 90 || rect.bottom < 0 || rect.top > window.innerHeight) {
                hideButton();
                return;
            }

            currentVideo = video;
            var btn = getOrCreatePipButton();

            btn.style.top = Math.max(12, rect.top + 14) + 'px';
            btn.style.left = (rect.left + rect.width / 2) + 'px';
            btn.classList.add('ruhi-pip-visible');

            if (hideTimeout) clearTimeout(hideTimeout);
            hideTimeout = setTimeout(hideButton, 3200);
        }

        function scheduleHide() {
            if (hideTimeout) clearTimeout(hideTimeout);
            hideTimeout = setTimeout(hideButton, 1200);
        }

        function hideButton() {
            if (activeBtn) {
                activeBtn.classList.remove('ruhi-pip-visible');
            }
        }

        function attachVideo(video) {
            if (video.__ruhiEdgePipBound) return;
            video.__ruhiEdgePipBound = true;

            video.addEventListener('mouseenter', function() { showButtonOnVideo(video); });
            video.addEventListener('mousemove', function() { showButtonOnVideo(video); });
            video.addEventListener('mouseleave', function() { scheduleHide(); });
            video.addEventListener('play', function() { showButtonOnVideo(video); });

            var container = video.closest('.html5-video-player, .player-container, .video-js, [class*=""player""]');
            if (container && !container.__ruhiEdgePipBound) {
                container.__ruhiEdgePipBound = true;
                container.addEventListener('mousemove', function() { showButtonOnVideo(video); });
                container.addEventListener('mouseleave', function() { scheduleHide(); });
            }
        }

        function scanAllVideos() {
            document.querySelectorAll('video').forEach(attachVideo);
        }

        scanAllVideos();
        var observer = new MutationObserver(function() { scanAllVideos(); });
        observer.observe(document.documentElement, { childList: true, subtree: true });
    } catch (e) { }
})();
";

        private const string RuhiProtocolInterceptorScript = @"
(function() {
    function isExt(u) {
        if (!u || typeof u !== 'string') return false;
        var s = u.trim().toLowerCase();
        return s.startsWith('ruhi://extension') || s.startsWith('ruhi:extension') ||
               s.startsWith('chrome://extension') || s.startsWith('edge://extension');
    }
    function isStore(u) {
        if (!u || typeof u !== 'string') return false;
        var s = u.trim().toLowerCase();
        return s === 'ruhi://store' || s === 'ruhi:store' || s === 'ruhi://addons' || s === 'ruhi:addons';
    }
    function sendExt() {
        if (window.chrome && window.chrome.webview) {
            try { window.chrome.webview.postMessage(JSON.stringify({ type: 'open_extensions_manager' })); } catch(e){}
        }
    }
    function sendStore() {
        if (window.chrome && window.chrome.webview) {
            try { window.chrome.webview.postMessage(JSON.stringify({ type: 'open_store' })); } catch(e){}
        }
    }

    function checkElement(el) {
        if (!el) return false;
        var href = (el.getAttribute && (el.getAttribute('href') || el.getAttribute('data-href'))) || el.href || '';
        if (isExt(href)) {
            sendExt();
            return true;
        }
        if (isStore(href)) {
            sendStore();
            return true;
        }
        return false;
    }

    function handleEvent(e) {
        try {
            var curr = e.target;
            while (curr && curr !== document && curr !== document.documentElement) {
                if (checkElement(curr)) {
                    e.preventDefault();
                    e.stopPropagation();
                    return;
                }
                curr = curr.parentElement;
            }
        } catch (ex) { }
    }

    document.addEventListener('click', handleEvent, true);
    document.addEventListener('auxclick', handleEvent, true);
})();
";

        private const string ChromeWebStoreBridgeScript = @"
(function() {
    var host = window.location.hostname || '';
    if (!host.includes('chromewebstore.google.com') && !host.includes('chrome.google.com')) return;

    // 1. Edge-style webstorePrivate polyfill
    window.chrome = window.chrome || {};
    window.chrome.app = window.chrome.app || {
        isInstalled: false,
        getDetails: function() { return null; },
        getIsInstalled: function() { return false; },
        installState: function(cb) { if (cb) cb('not_installed'); },
        runningState: function() { return 'cannot_run'; }
    };

    function notifyInstall(extId, btn) {
        if (!extId) return;
        if (btn) {
            btn.textContent = '⏳ Installing in RUHI...';
            btn.style.opacity = '0.7';
        }
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage(JSON.stringify({
                type: 'install_chrome_extension',
                id: extId,
                title: document.title
            }));
        }
    }

    window.chrome.webstorePrivate = {
        install: function(id, onSuccess, onFailure) {
            notifyInstall(id);
            if (onSuccess) onSuccess();
        },
        completeInstall: function(id, cb) { if (cb) cb(); },
        enable: function(id, cb) { if (cb) cb(); },
        getBrowserLogin: function(cb) { if (cb) cb({ login: '' }); },
        getStoreLogin: function(cb) { if (cb) cb(''); },
        getWebGLStatus: function(cb) { if (cb) cb('webgl_allowed'); },
        isInIncognitoMode: function(cb) { if (cb) cb(false); },
        isExtensionInstalled: function(id, cb) { if (cb) cb(false); }
    };

    function extractExtensionId() {
        var path = window.location.pathname || '';
        var match = path.match(/([a-z]{32})/i);
        if (match) return match[1].toLowerCase();
        var full = window.location.href || '';
        var anyMatch = full.match(/([a-z]{32})/i);
        return anyMatch ? anyMatch[1].toLowerCase() : null;
    }

    function ensureBanner() {
        var extId = extractExtensionId();
        if (!extId) {
            var existing = document.getElementById('ruhi-webstore-banner');
            if (existing) {
                existing.remove();
                if (document.body && document.body.dataset.ruhiPadded) {
                    document.body.style.paddingTop = document.body.dataset.ruhiOrigPadding || '';
                    delete document.body.dataset.ruhiPadded;
                }
            }
            return;
        }

        if (document.getElementById('ruhi-webstore-banner')) return;

        var banner = document.createElement('div');
        banner.id = 'ruhi-webstore-banner';
        banner.style.cssText = 'position:fixed;top:0;left:0;right:0;height:46px;background:#0F172A;border-bottom:2px solid #38BDF8;z-index:2147483647;display:flex;align-items:center;justify-content:space-between;padding:0 20px;font-family:system-ui,-apple-system,BlinkMacSystemFont,sans-serif;box-shadow:0 4px 16px rgba(0,0,0,0.5);color:#FFFFFF;box-sizing:border-box;';

        banner.innerHTML = '<div style=""display:flex;align-items:center;gap:10px;""><span style=""font-size:18px;"">🧩</span><span style=""font-size:13px;font-weight:600;color:#F8FAFC;"">You can install this extension directly in <strong>RUHI Browser</strong>!</span></div><button id=""ruhi-webstore-banner-btn"" style=""background:#2563EB;color:#FFFFFF;border:none;border-radius:6px;padding:6px 16px;font-size:12.5px;font-weight:600;cursor:pointer;box-shadow:0 2px 8px rgba(37,99,235,0.4);transition:all 0.15s;"">➕ Add to RUHI Browser</button>';

        if (document.body) {
            if (!document.body.dataset.ruhiPadded) {
                document.body.dataset.ruhiOrigPadding = document.body.style.paddingTop || '';
                document.body.dataset.ruhiPadded = 'true';
            }
            document.body.appendChild(banner);
            document.body.style.paddingTop = '46px';
        } else {
            document.documentElement.appendChild(banner);
        }

        var btn = document.getElementById('ruhi-webstore-banner-btn');
        if (btn) {
            btn.addEventListener('mouseenter', function() { btn.style.background = '#1D4ED8'; });
            btn.addEventListener('mouseleave', function() { btn.style.background = '#2563EB'; });
            btn.addEventListener('click', function() {
                notifyInstall(extId, btn);
            });
        }
    }

    function hookButtons() {
        var extId = extractExtensionId();
        if (!extId) return;

        var allButtons = document.querySelectorAll('button, div[role=""button""]');
        allButtons.forEach(function(b) {
            if (b.id === 'ruhi-webstore-banner-btn' || b.__ruhiHooked) return;
            var txt = (b.textContent || '').trim().toLowerCase();
            var aria = (b.getAttribute('aria-label') || '').toLowerCase();
            if (txt.includes('add to chrome') || txt.includes('add to') || aria.includes('add to chrome') || txt === 'install') {
                b.__ruhiHooked = true;
                b.addEventListener('click', function(e) {
                    e.stopImmediatePropagation();
                    e.preventDefault();
                    notifyInstall(extId, b);
                }, true);
            }
        });
    }

    function replaceUnavailableNotice() {
        var extId = extractExtensionId();
        if (!extId) return;

        var allElems = document.querySelectorAll('div, p, span, section');
        for (var i = 0; i < allElems.length; i++) {
            var el = allElems[i];
            if (el.children.length <= 2 && el.textContent && el.textContent.includes('Item currently unavailable')) {
                if (el.dataset.ruhiHandled) continue;
                el.dataset.ruhiHandled = 'true';

                var targetContainer = el.parentElement || el;
                el.style.display = 'none';

                var actionBtn = document.createElement('button');
                actionBtn.className = 'ruhi-inpage-action-btn';
                actionBtn.innerHTML = '<span style=""font-size:15px;margin-right:6px;"">➕</span><strong>Add to RUHI Browser</strong>';
                actionBtn.style.cssText = 'background:#1A73E8;color:#FFFFFF;border:none;border-radius:24px;padding:10px 24px;font-size:14px;font-weight:600;cursor:pointer;display:inline-flex;align-items:center;box-shadow:0 2px 8px rgba(26,115,232,0.4);margin:8px 0;transition:all 0.2s;font-family:system-ui,-apple-system,sans-serif;';

                actionBtn.addEventListener('mouseenter', function() { actionBtn.style.background = '#1557B0'; });
                actionBtn.addEventListener('mouseleave', function() { actionBtn.style.background = '#1A73E8'; });
                actionBtn.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    notifyInstall(extId, actionBtn);
                });

                targetContainer.appendChild(actionBtn);
                break;
            }
        }
    }

    setInterval(function() {
        ensureBanner();
        hookButtons();
        replaceUnavailableNotice();
    }, 600);
})();
";

        private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string? rawMsg = null;
                try { rawMsg = e.TryGetWebMessageAsString(); } catch { }

                var rawJson = e.WebMessageAsJson;
                string allContent = (rawMsg ?? "") + " " + (rawJson ?? "");

                if (allContent.Contains("open_extensions_manager") || IsExtensionsProtocolUrl(rawMsg) || IsExtensionsProtocolUrl(rawJson))
                {
                    OpenExtensionsWindow();
                    return;
                }

                if (allContent.Contains("open_store") || allContent.Contains("ruhi://store") || allContent.Contains("ruhi://addons"))
                {
                    if (_activeTab != null) NavigateTab(_activeTab, UrlResolver.GetStoreUrl());
                    return;
                }

                JsonElement root = default;
                JsonDocument? doc = null;
                string? type = null;

                try
                {
                    string? jsonToParse = (!string.IsNullOrEmpty(rawMsg) && rawMsg.TrimStart().StartsWith("{"))
                        ? rawMsg
                        : rawJson;

                    if (!string.IsNullOrEmpty(jsonToParse))
                    {
                        doc = JsonDocument.Parse(jsonToParse);
                        root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.String)
                        {
                            var innerStr = root.GetString();
                            if (!string.IsNullOrEmpty(innerStr) && innerStr.TrimStart().StartsWith("{"))
                            {
                                doc.Dispose();
                                doc = JsonDocument.Parse(innerStr);
                                root = doc.RootElement;
                            }
                        }
                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            type = root.TryGetProperty("type", out var tp) ? tp.GetString() : null;
                        }
                    }
                }
                catch { }

                if (type == "get_installed_extensions")
                {
                    var installed = await GetInstalledExtensionsListAsync();
                    var json = JsonSerializer.Serialize(installed);
                    if (_activeTab?.WebView?.CoreWebView2 != null)
                    {
                        await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync($"if (typeof window.syncInstalledExtensions === 'function') {{ window.syncInstalledExtensions({json}); }}");
                    }
                    return;
                }

                if (type == "uninstall_chrome_extension")
                {
                    var extId = root.TryGetProperty("id", out var ip) ? ip.GetString() : null;
                    var extName = root.TryGetProperty("name", out var np) ? np.GetString() : null;
                    await UninstallExtensionAsync(extId, extName);
                    await NotifyStoreTabsExtensionChangedAsync();
                    return;
                }

                if (type == "download_file")
                {
                    var fileUrl = root.TryGetProperty("url", out var up) ? up.GetString() : null;
                    var filename = root.TryGetProperty("filename", out var fp) ? fp.GetString() : "extension.crx";
                    if (!string.IsNullOrEmpty(fileUrl))
                    {
                        _ = StartDirectDownloadAsync(fileUrl, filename ?? "extension.crx");
                    }
                    return;
                }

                if (type == "install_chrome_extension")
                {
                    var extId = root.TryGetProperty("id", out var ip) ? ip.GetString() : null;
                    var zipUrl = root.TryGetProperty("zipUrl", out var zp) ? zp.GetString() : null;
                    var extTitle = root.TryGetProperty("name", out var np) ? np.GetString() : null;

                    if (!string.IsNullOrEmpty(extId) || !string.IsNullOrEmpty(zipUrl))
                    {
                        if (_activeTab?.WebView != null && _activeTab.WebView.CoreWebView2 == null)
                        {
                            try { await _activeTab.WebView.EnsureCoreWebView2Async(); } catch { }
                        }
                        var profile = _activeTab?.WebView?.CoreWebView2?.Profile;
                        if (profile == null)
                        {
                            foreach (var tab in _tabs)
                            {
                                if (tab.WebView?.CoreWebView2?.Profile != null)
                                {
                                    profile = tab.WebView.CoreWebView2.Profile;
                                    break;
                                }
                            }
                        }
                        if (profile != null)
                        {
                            try
                            {
                                CoreWebView2BrowserExtension? ext = null;
                                if (!string.IsNullOrEmpty(zipUrl))
                                {
                                    ext = await ExtensionManagerService.DownloadAndInstallZipExtensionAsync(zipUrl, extTitle ?? extId ?? "Extension", profile);
                                }
                                else if (!string.IsNullOrEmpty(extId) && extId.StartsWith("ruhi-"))
                                {
                                    ext = await ExtensionManagerService.InstallOfficialRuhiAddonAsync(extId, extTitle ?? "RUHI Official Add-on", profile);
                                }
                                else if (!string.IsNullOrEmpty(extId))
                                {
                                    ext = await ExtensionManagerService.InstallFromChromeWebStoreAsync(extId, profile);
                                }

                                // Update button in webpage to show success
                                string successScript = @$"
                                    var b = document.getElementById('ruhi-webstore-banner-btn');
                                    if (b) {{ b.innerText = '✓ Added to RUHI Browser'; b.style.background = '#16A34A'; b.disabled = true; }}
                                    document.querySelectorAll('.ruhi-inpage-action-btn').forEach(function(el) {{
                                        el.innerHTML = '<span style=""margin-right:6px;"">✓</span><strong>Added to RUHI</strong>';
                                        el.style.background = '#16A34A';
                                        el.disabled = true;
                                    }});
                                    document.querySelectorAll('button, div[role=""button""]').forEach(function(el) {{
                                        if (el.textContent && el.textContent.includes('Installing')) {{
                                            el.textContent = '✓ Added to RUHI';
                                            el.style.background = '#16A34A';
                                        }}
                                    }});
                                    if (typeof markAsInstalled === 'function') {{
                                        markAsInstalled('{extId ?? ""}');
                                    }}
                                ";
                                if (_activeTab?.WebView?.CoreWebView2 != null)
                                {
                                    await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(successScript);
                                }

                                await NotifyStoreTabsExtensionChangedAsync();

                                foreach (Window w in Application.Current.Windows)
                                {
                                    if (w is ExtensionsWindow ew)
                                    {
                                        _ = ew.Dispatcher.InvokeAsync(async () => await ew.LoadExtensionsAsync());
                                    }
                                }

                                _settings = DataManager.LoadSettings();
                                string extName = ext?.Name ?? extId ?? "Extension";
                                string installMsg = $"Chrome Extension \"{extName}\" has been installed and activated in RUHI Browser successfully! 🧩";
                                if (_settings.HomepageMode == "Extension" && !string.IsNullOrEmpty(_settings.ActiveExtensionNewTabUrl))
                                {
                                    installMsg += $"\n\nNew Tab / Homepage has been set to {_settings.ActiveExtensionNewTabName ?? extName}!";
                                    if (_activeTab != null && (string.IsNullOrEmpty(_activeTab.Url) || _activeTab.Url == "about:blank"))
                                    {
                                        NavigateTab(_activeTab, _settings.ActiveExtensionNewTabUrl);
                                    }
                                    else
                                    {
                                        CreateNewTab(_settings.ActiveExtensionNewTabUrl);
                                    }
                                }

                                MessageBox.Show(
                                    this,
                                    installMsg,
                                    "Extension Installed 🧩",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information
                                );
                            }
                            catch (Exception ex)
                            {
                                string failScript = @"
                                    var b = document.getElementById('ruhi-webstore-banner-btn');
                                    if (b) { b.innerText = '❌ Failed (Retry)'; b.style.background = '#DC2626'; }
                                    document.querySelectorAll('.ruhi-inpage-action-btn').forEach(function(el) {
                                        el.innerHTML = '<span style=""margin-right:6px;"">⚠️</span><strong>Failed (Retry)</strong>';
                                        el.style.background = '#DC2626';
                                    });
                                ";
                                if (_activeTab?.WebView?.CoreWebView2 != null)
                                {
                                    await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(failScript);
                                }

                                MessageBox.Show(
                                    this,
                                    "Could not install extension:\n" + ex.Message,
                                    "Extension Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning
                                );
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public async Task<List<object>> GetInstalledExtensionsListAsync()
        {
            var list = new List<object>();
            var extensionsDir = ExtensionManagerService.GetExtensionsDirectory();
            var processedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            CoreWebView2Profile? profile = _activeTab?.WebView?.CoreWebView2?.Profile;
            if (profile == null)
            {
                foreach (var tab in _tabs)
                {
                    if (tab.WebView?.CoreWebView2?.Profile != null)
                    {
                        profile = tab.WebView.CoreWebView2.Profile;
                        break;
                    }
                }
            }

            if (profile != null)
            {
                try
                {
                    var liveExts = await profile.GetBrowserExtensionsAsync();
                    foreach (var ext in liveExts)
                    {
                        string folder = "";
                        if (Directory.Exists(extensionsDir))
                        {
                            foreach (var dir in Directory.GetDirectories(extensionsDir))
                            {
                                var manifestFolder = ExtensionManagerService.FindManifestFolder(dir);
                                var meta = ExtensionManagerService.ReadManifest(manifestFolder);
                                if (string.Equals(meta.Name, ext.Name, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(Path.GetFileName(dir), ext.Name, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(Path.GetFileName(dir), ext.Id, StringComparison.OrdinalIgnoreCase))
                                {
                                    folder = Path.GetFileName(dir);
                                    break;
                                }
                            }
                        }

                        list.Add(new
                        {
                            id = ext.Id,
                            name = ext.Name,
                            folder = folder,
                            isEnabled = ext.IsEnabled
                        });
                        processedNames.Add(ext.Name);
                        if (!string.IsNullOrEmpty(folder)) processedNames.Add(folder);
                    }
                }
                catch { }
            }

            // Also include physical folders with manifest.json not yet in processedNames
            if (Directory.Exists(extensionsDir))
            {
                try
                {
                    foreach (var dir in Directory.GetDirectories(extensionsDir))
                    {
                        var dirName = Path.GetFileName(dir);
                        if (processedNames.Contains(dirName)) continue;

                        var manifestFolder = ExtensionManagerService.FindManifestFolder(dir);
                        if (File.Exists(Path.Combine(manifestFolder, "manifest.json")))
                        {
                            var meta = ExtensionManagerService.ReadManifest(manifestFolder);
                            if (!processedNames.Contains(meta.Name))
                            {
                                list.Add(new
                                {
                                    id = dirName,
                                    name = meta.Name,
                                    folder = dirName,
                                    isEnabled = true
                                });
                                processedNames.Add(meta.Name);
                            }
                        }
                    }
                }
                catch { }
            }

            return list;
        }

        public async Task UninstallExtensionAsync(string? extId, string? extName)
        {
            if (string.IsNullOrWhiteSpace(extId) && string.IsNullOrWhiteSpace(extName)) return;

            CoreWebView2Profile? profile = _activeTab?.WebView?.CoreWebView2?.Profile;
            if (profile == null)
            {
                foreach (var tab in _tabs)
                {
                    if (tab.WebView?.CoreWebView2?.Profile != null)
                    {
                        profile = tab.WebView.CoreWebView2.Profile;
                        break;
                    }
                }
            }

            if (profile != null)
            {
                try
                {
                    var liveExts = await profile.GetBrowserExtensionsAsync();
                    foreach (var ext in liveExts)
                    {
                        bool matches = (!string.IsNullOrEmpty(extId) && (string.Equals(ext.Id, extId, StringComparison.OrdinalIgnoreCase) || ext.Id.Contains(extId, StringComparison.OrdinalIgnoreCase))) ||
                                       (!string.IsNullOrEmpty(extName) && string.Equals(ext.Name, extName, StringComparison.OrdinalIgnoreCase));

                        if (!matches && !string.IsNullOrEmpty(extId))
                        {
                            var extDir = ExtensionManagerService.GetExtensionsDirectory();
                            if (Directory.Exists(extDir))
                            {
                                foreach (var dir in Directory.GetDirectories(extDir))
                                {
                                    if (Path.GetFileName(dir).Equals(extId, StringComparison.OrdinalIgnoreCase) ||
                                        dir.Contains(extId, StringComparison.OrdinalIgnoreCase))
                                    {
                                        var meta = ExtensionManagerService.ReadManifest(ExtensionManagerService.FindManifestFolder(dir));
                                        if (string.Equals(meta.Name, ext.Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (matches)
                        {
                            try { await ext.EnableAsync(false); } catch { }
                            await Task.Delay(50);
                            try { await ext.RemoveAsync(); } catch { }
                        }
                    }
                }
                catch { }
            }

            // Remove physical directory from disk
            try
            {
                var extDir = ExtensionManagerService.GetExtensionsDirectory();
                if (Directory.Exists(extDir))
                {
                    foreach (var dir in Directory.GetDirectories(extDir))
                    {
                        var dirName = Path.GetFileName(dir);
                        bool matchesDir = (!string.IsNullOrEmpty(extId) && (string.Equals(dirName, extId, StringComparison.OrdinalIgnoreCase) || dirName.Contains(extId, StringComparison.OrdinalIgnoreCase)));
                        if (!matchesDir && !string.IsNullOrEmpty(extName))
                        {
                            var meta = ExtensionManagerService.ReadManifest(ExtensionManagerService.FindManifestFolder(dir));
                            if (string.Equals(meta.Name, extName, StringComparison.OrdinalIgnoreCase) || dirName.Equals(extName, StringComparison.OrdinalIgnoreCase))
                            {
                                matchesDir = true;
                            }
                        }

                        if (matchesDir)
                        {
                            try { Directory.Delete(dir, true); }
                            catch
                            {
                                try
                                {
                                    string trash = Path.Combine(extDir, "." + dirName + "_trash_" + Guid.NewGuid().ToString("N")[..6]);
                                    Directory.Move(dir, trash);
                                    try { Directory.Delete(trash, true); } catch { }
                                }
                                catch { }
                            }
                        }
                    }
                }

                // If this extension was the new tab provider, revert back to Home
                if (!string.IsNullOrEmpty(_settings.ActiveExtensionNewTabName) &&
                    (string.Equals(_settings.ActiveExtensionNewTabName, extName, StringComparison.OrdinalIgnoreCase) ||
                     (!string.IsNullOrEmpty(extId) && _settings.ActiveExtensionNewTabUrl.Contains(extId))))
                {
                    _settings.ActiveExtensionNewTabUrl = string.Empty;
                    _settings.ActiveExtensionNewTabName = string.Empty;
                    if (_settings.HomepageMode == "Extension")
                    {
                        _settings.HomepageMode = "Home";
                    }
                    DataManager.SaveSettings(_settings);
                }
            }
            catch { }
        }

        public async Task NotifyStoreTabsExtensionChangedAsync()
        {
            try
            {
                var installed = await GetInstalledExtensionsListAsync();
                var json = JsonSerializer.Serialize(installed);

                foreach (var tab in _tabs.ToList())
                {
                    if (tab.WebView?.CoreWebView2 != null)
                    {
                        try
                        {
                            await tab.WebView.CoreWebView2.ExecuteScriptAsync($"if (typeof window.syncInstalledExtensions === 'function') {{ window.syncInstalledExtensions({json}); }}");
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private async Task StartDirectDownloadAsync(string url, string filename)
        {
            DownloadItem? downloadItem = null;
            try
            {
                var folder = !string.IsNullOrWhiteSpace(_settings.DownloadFolder) && Directory.Exists(_settings.DownloadFolder)
                    ? _settings.DownloadFolder
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                if (!Directory.Exists(folder))
                {
                    try { Directory.CreateDirectory(folder); } catch { }
                }

                var filePath = Path.Combine(folder, filename);
                if (File.Exists(filePath))
                {
                    string nameOnly = Path.GetFileNameWithoutExtension(filename);
                    string extOnly = Path.GetExtension(filename);
                    filePath = Path.Combine(folder, $"{nameOnly}_{DateTime.Now:yyyyMMdd_HHmmss}{extOnly}");
                }

                downloadItem = new DownloadItem
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    Url = url,
                    StatusText = "Downloading...",
                    StartTime = DateTime.Now
                };

                lock (_downloads)
                {
                    _downloads.Insert(0, downloadItem);
                }

                Dispatcher.Invoke(() =>
                {
                    UpdateTopBarDownloadProgress();
                    RenderDownloadsHub();
                });

                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    MaxAutomaticRedirections = 10
                };
                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 RuhiBrowser/1.0");

                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException($"Server returned no file ({response.StatusCode}). Extension may be unlisted or restricted by Google.");
                }
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                if (totalBytes > 0)
                {
                    downloadItem.TotalBytes = totalBytes;
                }

                await using var contentStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                int bytesRead;
                long totalRead = 0;
                var sw = System.Diagnostics.Stopwatch.StartNew();

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    downloadItem.BytesReceived = totalRead;
                    if (totalBytes > 0)
                    {
                        downloadItem.ProgressPercentage = ((double)totalRead / totalBytes) * 100.0;
                    }

                    if (sw.ElapsedMilliseconds > 300)
                    {
                        sw.Restart();
                        Dispatcher.Invoke(() =>
                        {
                            UpdateTopBarDownloadProgress();
                        });
                    }
                }

                downloadItem.IsCompleted = true;
                downloadItem.StatusText = "Completed";
                downloadItem.EndTime = DateTime.Now;

                DownloadHistoryService.AddRecord(downloadItem);

                Dispatcher.Invoke(() =>
                {
                    UpdateTopBarDownloadProgress();
                    RenderDownloadsHub();
                    TriggerDownloadCompletedAnimation();

                    if (downloadItem.IsExtensionArchive && File.Exists(downloadItem.FilePath))
                    {
                        AutoInstallDownloadedExtension(downloadItem);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Direct download error: " + ex.Message);
                if (downloadItem != null)
                {
                    downloadItem.IsFailed = true;
                    downloadItem.StatusText = "Failed: Server returned no file";
                    Dispatcher.Invoke(() =>
                    {
                        UpdateTopBarDownloadProgress();
                        RenderDownloadsHub();
                    });
                }
            }
        }

        private async void OnPipClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.WebView?.CoreWebView2 != null)
            {
                string script = @"
                    (function() {
                        const video = document.querySelector('video');
                        if (video) {
                            if (document.pictureInPictureElement) {
                                document.exitPictureInPicture();
                            } else {
                                video.requestPictureInPicture().catch(err => alert('Picture-in-Picture: ' + err.message));
                            }
                        } else {
                            alert('No video found on this page to pop out.');
                        }
                    })();
                ";
                try
                {
                    await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(script);
                }
                catch { }
            }
        }

        private void UpdateZoomBadge()
        {
            if (_activeTab?.WebView != null)
            {
                double zoom = _activeTab.WebView.ZoomFactor;
                int percent = (int)Math.Round(zoom * 100);
                if (Math.Abs(zoom - 1.0) > 0.04)
                {
                    BtnZoom.Content = $"{percent}%";
                    BtnZoom.ToolTip = $"Zoom: {percent}% (Click to reset to 100%)";
                    BtnZoom.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnZoom.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                BtnZoom.Visibility = Visibility.Collapsed;
            }
        }

        private void OnResetZoomClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab?.WebView != null)
            {
                _activeTab.WebView.ZoomFactor = 1.0;
                UpdateZoomBadge();
            }
        }

        private MenuItem CreateFirefoxStyleAccountItem()
        {
            var cardGrid = new Grid
            {
                Margin = new Thickness(0),
                Cursor = Cursors.Hand,
                MinWidth = 240
            };
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 1. Avatar Circle with optional green active badge
            var avatarContainer = new Grid
            {
                Width = 36,
                Height = 36,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var avatarCircle = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = SyncService.IsLoggedIn
                    ? new LinearGradientBrush(
                        Color.FromRgb(37, 99, 235), // Royal Blue
                        Color.FromRgb(16, 185, 129), // Emerald Green
                        45)
                    : new SolidColorBrush(Color.FromRgb(51, 65, 85)), // Slate 700
                BorderBrush = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                BorderThickness = new Thickness(1)
            };
            avatarContainer.Children.Add(avatarCircle);

            string dName = SyncService.CurrentDisplayName;
            string email = SyncService.CurrentEmail;

            string initial = "👤";
            if (SyncService.IsLoggedIn)
            {
                if (!string.IsNullOrWhiteSpace(dName))
                    initial = dName.Trim().Substring(0, 1).ToUpperInvariant();
                else if (!string.IsNullOrWhiteSpace(email))
                    initial = email.Trim().Substring(0, 1).ToUpperInvariant();
            }

            var avatarLabel = new TextBlock
            {
                Text = initial,
                FontSize = SyncService.IsLoggedIn ? 15 : 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarContainer.Children.Add(avatarLabel);

            if (SyncService.IsLoggedIn)
            {
                var dotActive = new System.Windows.Shapes.Ellipse
                {
                    Width = 9,
                    Height = 9,
                    Fill = new SolidColorBrush(Color.FromRgb(34, 197, 94)),
                    Stroke = new SolidColorBrush(Color.FromRgb(24, 24, 24)),
                    StrokeThickness = 1.5,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, -1, -1)
                };
                avatarContainer.Children.Add(dotActive);
            }

            Grid.SetColumn(avatarContainer, 0);
            cardGrid.Children.Add(avatarContainer);

            // 2. Account Details Stack
            var detailsStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };

            string title = "Sign in to Ruhi Account";
            string subtitle = "Sync tabs, bookmarks & history";
            if (SyncService.IsLoggedIn)
            {
                title = !string.IsNullOrWhiteSpace(dName) ? dName : email;
                subtitle = !string.IsNullOrWhiteSpace(dName) && !string.IsNullOrWhiteSpace(email) ? email : "Ruhi Account • Manage sync & devices";
            }

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 190
            };

            var subtitleText = new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                Margin = new Thickness(0, 2, 0, 0)
            };

            detailsStack.Children.Add(titleText);
            detailsStack.Children.Add(subtitleText);

            Grid.SetColumn(detailsStack, 1);
            cardGrid.Children.Add(detailsStack);

            // 3. Chevron / Arrow
            var chevron = new TextBlock
            {
                Text = "›",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 2, 0)
            };
            Grid.SetColumn(chevron, 2);
            cardGrid.Children.Add(chevron);

            var item = new MenuItem
            {
                Header = cardGrid,
                Padding = new Thickness(10, 8, 10, 8),
                Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            if (TryFindResource("ModernMenuItem") is Style itemStyle)
            {
                item.Style = itemStyle;
            }
            item.Click += OnSyncClicked;
            return item;
        }

        private void OnMenuClicked(object sender, RoutedEventArgs e)
        {
            var menu = CreateModernContextMenu();

            MenuItem CreateItem(string header, RoutedEventHandler handler) => CreateModernMenuItem(header, handler);

            menu.Items.Add(CreateFirefoxStyleAccountItem());
            menu.Items.Add(new Separator());

            menu.Items.Add(CreateItem("New Tab (Ctrl+T)", (s, ev) => CreateNewTab()));
            menu.Items.Add(CreateItem("New InPrivate Tab", (s, ev) => CreateNewTab(isIncognito: true)));
            if (_recentlyClosedTabs.Count > 0)
            {
                menu.Items.Add(CreateItem("Reopen Closed Tab (Ctrl+Shift+T)", (s, ev) => ReopenClosedTab()));
            }
            menu.Items.Add(CreateItem("Open File... (Ctrl+O)", (s, ev) => OpenFilePrompt()));
            menu.Items.Add(CreateItem("PDF Tools & Editor ✏️", (s, ev) => OpenPdfToolsWindow(_activeTab?.Url)));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateItem("Find in Page (Ctrl+F)", (s, ev) => ShowFindInPage()));
            menu.Items.Add(CreateItem("Translate Page...", OnTranslateClicked));
            menu.Items.Add(CreateItem("Floating Video (PiP)", OnPipClicked));
            menu.Items.Add(CreateItem("Split Screen (Dual Tabs) 🪟 (Ctrl+Shift+S)", OnSplitScreenClicked));
            menu.Items.Add(CreateItem("RUHI Privacy & Ad Shield 🛡️", OnShieldClicked));
            menu.Items.Add(CreateItem("RUHI AI Assistant ✨", OnAiClicked));
            menu.Items.Add(CreateItem("RUHI WireGuard VPN 🛡", (s, ev) => new VpnWindow { Owner = this }.ShowDialog()));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateItem("Bookmarks (Ctrl+B)", (s, ev) => OpenBookmarksWindow()));
            menu.Items.Add(CreateItem("History (Ctrl+H)", (s, ev) => OpenHistoryWindow()));
            menu.Items.Add(CreateItem("Downloads Hub (Ctrl+J)", OnDownloadsClicked));
            menu.Items.Add(CreateItem("Passwords Vault 🔐", (s, ev) => OpenPasswordsWindow()));
            menu.Items.Add(CreateItem("Manage Extensions 🧩", (s, ev) => OpenExtensionsWindow()));
            menu.Items.Add(CreateItem("RUHI Add-On-Store 🛍️", (s, ev) => OpenRuhiStore()));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateItem("Clear Browsing Data", (s, ev) =>
            {
                var dlg = new ClearDataDialog { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    if (dlg.ClearHistorySelected)
                    {
                        DataManager.ClearHistory();
                        DownloadHistoryService.ClearHistory();
                        lock (_downloads) { _downloads.RemoveAll(d => d.IsCompleted || d.IsFailed); }
                        UpdateTopBarDownloadProgress();
                        _ = SyncService.ClearAllHistoryAsync();
                    }
                    if (dlg.ClearBookmarksSelected)
                    {
                        DataManager.ClearBookmarks();
                        _bookmarks.Clear();
                        if (SyncService.IsLoggedIn) _ = SyncService.ClearAllBookmarksAsync();
                    }
                    if (dlg.ClearPasswordsSelected)
                    {
                        PasswordVaultService.SaveCredentials(new List<SavedCredential>());
                    }
                    RenderHomeScreenRecentVisits();
                    _ = ClearWebView2DataAsync(dlg.ClearHistorySelected, dlg.ClearCookiesSelected, dlg.ClearCacheSelected);
                    MessageBox.Show(this, "Selected browsing data cleared successfully.", "Clear Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }));
            menu.Items.Add(CreateItem("Developer Tools (F12)", (s, ev) => OpenDevTools()));
            menu.Items.Add(CreateItem("Settings", (s, ev) => OpenSettingsWindow()));
            menu.Items.Add(CreateItem("About RUHI Browser", (s, ev) => new AboutWindow { Owner = this }.ShowDialog()));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateItem("Exit", (s, ev) => Close()));

            menu.PlacementTarget = BtnMenu;
            menu.IsOpen = true;
        }

        #endregion

        #region Find In Page

        private void ShowFindInPage()
        {
            FindInPageBar.Visibility = Visibility.Visible;
            TbFindQuery.Focus();
            TbFindQuery.SelectAll();
            if (!string.IsNullOrEmpty(TbFindQuery.Text))
            {
                ExecuteFind(TbFindQuery.Text, backwards: false);
            }
        }

        private void CloseFindInPage()
        {
            FindInPageBar.Visibility = Visibility.Collapsed;
            TbFindCount.Text = "0/0";
        }

        private void OnCloseFindClicked(object sender, RoutedEventArgs e)
        {
            CloseFindInPage();
        }

        private void OnFindQueryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    FindPrevious();
                else
                    FindNext();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                CloseFindInPage();
            }
        }

        private void OnFindQueryChanged(object sender, TextChangedEventArgs e)
        {
            string query = TbFindQuery.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                TbFindCount.Text = "0/0";
                return;
            }

            ExecuteFind(query, backwards: false);
        }

        private void OnFindNextClicked(object sender, RoutedEventArgs e)
        {
            FindNext();
        }

        private void OnFindPreviousClicked(object sender, RoutedEventArgs e)
        {
            FindPrevious();
        }

        private void FindNext()
        {
            string query = TbFindQuery.Text;
            if (!string.IsNullOrWhiteSpace(query))
            {
                ExecuteFind(query, backwards: false);
            }
        }

        private void FindPrevious()
        {
            string query = TbFindQuery.Text;
            if (!string.IsNullOrWhiteSpace(query))
            {
                ExecuteFind(query, backwards: true);
            }
        }

        private async void ExecuteFind(string query, bool backwards)
        {
            if (_activeTab?.WebView?.CoreWebView2 == null) return;

            try
            {
                string jsonQuery = JsonSerializer.Serialize(query);
                string backwardBool = backwards ? "true" : "false";

                // Native browser text search
                string script = $"window.find({jsonQuery}, false, {backwardBool}, true, false, false, false);";
                await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(script);

                // Count occurrences for counter display
                string countScript = $@"
                    (function() {{
                        var text = document.body ? document.body.innerText : '';
                        var q = {jsonQuery};
                        if (!q || !text) return 0;
                        var count = 0;
                        var pos = 0;
                        var lowerText = text.toLowerCase();
                        var lowerQ = q.toLowerCase();
                        while ((pos = lowerText.indexOf(lowerQ, pos)) !== -1) {{
                            count++;
                            pos += lowerQ.length;
                        }}
                        return count;
                    }})();
                ";
                string countResult = await _activeTab.WebView.CoreWebView2.ExecuteScriptAsync(countScript);
                if (int.TryParse(countResult, out int matches))
                {
                    TbFindCount.Text = matches > 0 ? $"{matches} match{(matches > 1 ? "es" : "")}" : "0 matches";
                }
            }
            catch { }
        }

        #endregion

        #region Home Screen & Shortcuts

        private void OnHomeSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _activeTab != null)
            {
                NavigateTab(_activeTab, TbHomeSearch.Text);
            }
        }

        private void OnHomeSearchClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab != null && !string.IsNullOrWhiteSpace(TbHomeSearch.Text))
            {
                NavigateTab(_activeTab, TbHomeSearch.Text);
            }
        }

        private void RenderHomeScreenShortcuts()
        {
            WpShortcuts.Children.Clear();
            bool isDark = IsCurrentThemeDark();

            string tileBg = isDark ? "#383838" : "#FFFFFF";
            string tileHoverBg = isDark ? "#3C4043" : "#F1F5F9";
            string tileBorder = isDark ? "#3C4043" : "#CBD5E1";
            var titleFg = isDark ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
            string addBg = isDark ? "#152033" : "#F8FAFC";
            string iconBg = isDark ? "#0F172A" : "#F1F5F9";
            var addFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));

            // Render existing shortcuts
            foreach (var shortcut in _shortcuts)
            {
                var tile = new Border
                {
                    Style = (Style)FindResource("ShortcutTileStyle"),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tileBg)),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tileBorder)),
                    BorderThickness = new Thickness(1),
                    ContextMenu = CreateShortcutContextMenu(shortcut)
                };

                tile.MouseEnter += (s, e) => tile.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tileHoverBg));
                tile.MouseLeave += (s, e) => tile.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tileBg));

                var sp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

                // Circular bubble for Favicon / Icon
                var iconBubble = new Border
                {
                    Width = 40,
                    Height = 40,
                    CornerRadius = new CornerRadius(20),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconBg)),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var iconGrid = new Grid();

                var initialText = new TextBlock
                {
                    Text = GetDomainInitial(shortcut.Url),
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                iconGrid.Children.Add(initialText);

                // Site Favicon Image
                var domain = GetDomain(shortcut.Url);
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    try
                    {
                        var favUrl = !string.IsNullOrWhiteSpace(shortcut.FaviconUrl)
                            ? shortcut.FaviconUrl
                            : $"https://www.google.com/s2/favicons?domain={domain}&sz=64";

                        var img = new Image
                        {
                            Width = 24,
                            Height = 24,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Visibility = Visibility.Collapsed
                        };
                        RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(favUrl, UriKind.Absolute);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();

                        bmp.DownloadCompleted += (s, e) =>
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                img.Visibility = Visibility.Visible;
                                initialText.Visibility = Visibility.Collapsed;
                            }));
                        };
                        bmp.DownloadFailed += (s, e) =>
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                img.Visibility = Visibility.Collapsed;
                                initialText.Visibility = Visibility.Visible;
                            }));
                        };

                        if (!bmp.IsDownloading && bmp.PixelWidth > 0)
                        {
                            img.Visibility = Visibility.Visible;
                            initialText.Visibility = Visibility.Collapsed;
                        }

                        img.Source = bmp;
                        iconGrid.Children.Add(img);
                    }
                    catch { }
                }

                iconBubble.Child = iconGrid;

                var titleText = new TextBlock
                {
                    Text = shortcut.Title,
                    FontSize = 12,
                    Foreground = titleFg,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 6, 0, 0),
                    MaxWidth = 96
                };

                sp.Children.Add(iconBubble);
                sp.Children.Add(titleText);
                tile.Child = sp;

                tile.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left && _activeTab != null)
                    {
                        NavigateTab(_activeTab, shortcut.Url);
                    }
                };

                WpShortcuts.Children.Add(tile);
            }

            // Render "+ Add Shortcut" card
            var addTile = new Border
            {
                Style = (Style)FindResource("ShortcutTileStyle"),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(addBg)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tileBorder)),
                BorderThickness = new Thickness(1)
            };

            addTile.MouseEnter += (s, e) => addTile.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(tileHoverBg));
            addTile.MouseLeave += (s, e) => addTile.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(addBg));

            var addSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var addBubble = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(20),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconBg)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var addIcon = new TextBlock
            {
                Text = "➕",
                FontSize = 15,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            addBubble.Child = addIcon;

            var addTitle = new TextBlock
            {
                Text = "Add Shortcut",
                FontSize = 12,
                Foreground = addFg,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };
            addSp.Children.Add(addBubble);
            addSp.Children.Add(addTitle);
            addTile.Child = addSp;

            addTile.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    var win = new AddEditShortcutWindow { Owner = this };
                    if (win.ShowDialog() == true && win.ResultShortcut != null)
                    {
                        _shortcuts.Add(win.ResultShortcut);
                        DataManager.SaveShortcuts(_shortcuts);
                        RenderHomeScreenShortcuts();
                    }
                }
            };

            WpShortcuts.Children.Add(addTile);
        }

        private ContextMenu CreateShortcutContextMenu(Shortcut shortcut)
        {
            var menu = CreateModernContextMenu();

            var editItem = CreateModernMenuItem("Edit Shortcut", (s, e) =>
            {
                var win = new AddEditShortcutWindow(shortcut) { Owner = this };
                if (win.ShowDialog() == true && win.ResultShortcut != null)
                {
                    bool urlChanged = !string.Equals(shortcut.Url, win.ResultShortcut.Url, StringComparison.OrdinalIgnoreCase);
                    shortcut.Title = win.ResultShortcut.Title;
                    shortcut.Url = win.ResultShortcut.Url;
                    if (urlChanged)
                    {
                        shortcut.FaviconUrl = string.Empty;
                    }
                    DataManager.SaveShortcuts(_shortcuts);
                    RenderHomeScreenShortcuts();
                }
            });
            menu.Items.Add(editItem);

            var deleteItem = CreateModernMenuItem("Delete Shortcut", (s, e) =>
            {
                _shortcuts.Remove(shortcut);
                DataManager.SaveShortcuts(_shortcuts);
                RenderHomeScreenShortcuts();
            });
            deleteItem.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
            menu.Items.Add(deleteItem);

            return menu;
        }

        private void RenderHomeScreenRecentVisits()
        {
            SpRecentVisits.Children.Clear();
            var history = DataManager.LoadHistory();
            var count = 0;
            bool isDark = IsCurrentThemeDark();
            var titleFg = isDark ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
            var timeFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#64748B" : "#94A3B8"));

            foreach (var item in history)
            {
                if (count++ >= 5) break;

                var grid = new Grid { Margin = new Thickness(0, 4, 0, 4), Cursor = Cursors.Hand };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var sp = new StackPanel();
                sp.Children.Add(new TextBlock
                {
                    Text = item.Title,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = titleFg,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
                sp.Children.Add(new TextBlock
                {
                    Text = item.Url,
                    FontSize = 11,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                    TextTrimming = TextTrimming.CharacterEllipsis
                });
                Grid.SetColumn(sp, 0);
                grid.Children.Add(sp);

                var timeBlock = new TextBlock
                {
                    Text = item.VisitedAt.ToString("g"),
                    FontSize = 11,
                    Foreground = timeFg,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(timeBlock, 1);
                grid.Children.Add(timeBlock);

                grid.MouseDown += (s, e) =>
                {
                    if (_activeTab != null)
                    {
                        NavigateTab(_activeTab, item.Url);
                    }
                };

                SpRecentVisits.Children.Add(grid);
            }

            if (count == 0)
            {
                SpRecentVisits.Children.Add(new TextBlock
                {
                    Text = "No recent visits yet. Start browsing to see your activity here.",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")),
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 4)
                });
            }
        }

        private static string GetDomainInitial(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var host = uri.Host.Replace("www.", "");
                    return char.ToUpperInvariant(host[0]).ToString();
                }
            }
            catch { }
            return "🌐";
        }

        private static string GetDomain(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return uri.Host.Replace("www.", "");
                }
            }
            catch { }
            return string.Empty;
        }

        #region Home Screen Layouts & Presets

        public class NewsArticleItem
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string TimeAgo { get; set; } = string.Empty;
            public string Category { get; set; } = "Top";
            public string? ImageUrl { get; set; }
        }

        private DispatcherTimer? _clockTimer;
        private bool _isApplyingNote;
        private bool _isApplyingGoal;
        private DispatcherTimer? _pomodoroTimer;
        private int _pomodoroSecondsRemaining = 25 * 60;
        private bool _isPomodoroRunning = false;
        private int _pomodoroDurationMinutes = 25;

        private static readonly (string name, string relativePath)[] _scenicWallpapers = new[]
        {
            ("🏔️ Alpine Mountain Lake", "Assets/wallpaper1.jpg"),
            ("🌌 Aurora Borealis Sky", "Assets/wallpaper2.jpg"),
            ("🌅 Golden Coast Sunset", "Assets/wallpaper3.jpg")
        };
        private readonly List<BitmapImage?> _scenicWallpaperImages = new();
        private int _currentWallpaperIndex = 0;
        private DispatcherTimer? _wallpaperTimer;
        private bool _isWallpaperAutoRotate = true;
        private const int WallpaperIntervalSeconds = 20;

        private static readonly string[] _productiveQuotes = new[]
        {
            "“Focus on being productive instead of busy.”",
            "“Small daily improvements over time lead to stunning results.”",
            "“The secret of getting ahead is getting started.”",
            "“Do what you can, with what you have, where you are.”",
            "“Deep work produces rare value in a world full of noise.”",
            "“Your future is created by what you do today, not tomorrow.”",
            "“Simplicity is the prerequisite for reliability and focus.”"
        };

        private static readonly HttpClient _newsClient = CreateNewsHttpClient();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ImageSource> _newsImageCache = new(StringComparer.OrdinalIgnoreCase);
        private string _activeNewsCategory = "top";
        private readonly Dictionary<string, (DateTime fetchedAt, List<NewsArticleItem> items)> _newsCategoryCache = new(StringComparer.OrdinalIgnoreCase);

        private static HttpClient CreateNewsHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            return client;
        }

        private void ApplyHomeScreenStyle()
        {
            var style = (_settings.HomeScreenStyle ?? "News").Trim();
            bool isDark = IsCurrentThemeDark();

            switch (style.ToLowerInvariant())
            {
                case "standard":
                    if (SpHomeMainContainer != null) SpHomeMainContainer.MaxWidth = 800;
                    if (SpHomeBrandHeader != null) SpHomeBrandHeader.Visibility = Visibility.Visible;
                    if (SpHomeProductiveHeader != null) SpHomeProductiveHeader.Visibility = Visibility.Collapsed;
                    if (BorderRecentVisits != null) BorderRecentVisits.Visibility = Visibility.Visible;
                    if (BorderHomeNews != null) BorderHomeNews.Visibility = Visibility.Collapsed;
                    if (BorderHomeDailyGoal != null) BorderHomeDailyGoal.Visibility = Visibility.Collapsed;
                    if (BorderHomePomodoro != null) BorderHomePomodoro.Visibility = Visibility.Collapsed;
                    if (BorderHomeNotes != null) BorderHomeNotes.Visibility = Visibility.Collapsed;
                    if (BorderHomeProductiveTools != null) BorderHomeProductiveTools.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperBackdrop != null) BorderWallpaperBackdrop.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperOverlay != null) BorderWallpaperOverlay.Visibility = Visibility.Collapsed;
                    if (HomeScreen != null) HomeScreen.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F8FAFC"));
                    StopWallpaperTimer();
                    RenderHomeScreenRecentVisits();
                    break;

                case "news":
                    if (SpHomeMainContainer != null) SpHomeMainContainer.MaxWidth = 1080;
                    if (SpHomeBrandHeader != null) SpHomeBrandHeader.Visibility = Visibility.Visible;
                    if (SpHomeProductiveHeader != null) SpHomeProductiveHeader.Visibility = Visibility.Collapsed;
                    if (BorderRecentVisits != null) BorderRecentVisits.Visibility = Visibility.Collapsed;
                    if (BorderHomeNews != null) BorderHomeNews.Visibility = Visibility.Visible;
                    if (BorderHomeDailyGoal != null) BorderHomeDailyGoal.Visibility = Visibility.Collapsed;
                    if (BorderHomePomodoro != null) BorderHomePomodoro.Visibility = Visibility.Collapsed;
                    if (BorderHomeNotes != null) BorderHomeNotes.Visibility = Visibility.Collapsed;
                    if (BorderHomeProductiveTools != null) BorderHomeProductiveTools.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperBackdrop != null) BorderWallpaperBackdrop.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperOverlay != null) BorderWallpaperOverlay.Visibility = Visibility.Collapsed;
                    if (HomeScreen != null) HomeScreen.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F8FAFC"));
                    StopWallpaperTimer();
                    InitClockTimer();
                    _ = LoadTrendingNewsAsync();
                    break;

                case "productive":
                    if (SpHomeMainContainer != null) SpHomeMainContainer.MaxWidth = 800;
                    if (SpHomeBrandHeader != null) SpHomeBrandHeader.Visibility = Visibility.Collapsed;
                    if (SpHomeProductiveHeader != null) SpHomeProductiveHeader.Visibility = Visibility.Visible;
                    if (BorderRecentVisits != null) BorderRecentVisits.Visibility = Visibility.Collapsed;
                    if (BorderHomeNews != null) BorderHomeNews.Visibility = Visibility.Collapsed;
                    if (BorderHomeDailyGoal != null) BorderHomeDailyGoal.Visibility = Visibility.Visible;
                    if (BorderHomePomodoro != null) BorderHomePomodoro.Visibility = Visibility.Visible;
                    if (BorderHomeNotes != null) BorderHomeNotes.Visibility = Visibility.Visible;
                    if (BorderHomeProductiveTools != null) BorderHomeProductiveTools.Visibility = Visibility.Visible;
                    if (BorderWallpaperBackdrop != null) BorderWallpaperBackdrop.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperOverlay != null) BorderWallpaperOverlay.Visibility = Visibility.Collapsed;
                    if (HomeScreen != null) HomeScreen.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#080E1A" : "#F1F5F9"));
                    StopWallpaperTimer();

                    ApplyProductiveThemeColors(isDark);
                    InitClockTimer();

                    // Motivational quote
                    if (TbHomeQuote != null)
                    {
                        TbHomeQuote.Text = _productiveQuotes[DateTime.Now.DayOfYear % _productiveQuotes.Length];
                    }

                    // Load Daily Goal
                    _isApplyingGoal = true;
                    if (TbDailyFocusGoal != null) TbDailyFocusGoal.Text = _settings.DailyGoalText ?? string.Empty;
                    if (CbDailyGoalCheck != null) CbDailyGoalCheck.IsChecked = _settings.DailyGoalCompleted;
                    UpdateDailyGoalUi();
                    _isApplyingGoal = false;

                    // Load Quick Notes
                    _isApplyingNote = true;
                    if (TbQuickNote != null) TbQuickNote.Text = _settings.QuickNoteText ?? string.Empty;
                    _isApplyingNote = false;
                    UpdateNotesWordCount();

                    // Pomodoro Display
                    UpdatePomodoroDisplay();
                    break;

                case "wallpaper":
                    if (SpHomeMainContainer != null) SpHomeMainContainer.MaxWidth = 800;
                    if (SpHomeBrandHeader != null) SpHomeBrandHeader.Visibility = Visibility.Visible;
                    if (SpHomeProductiveHeader != null) SpHomeProductiveHeader.Visibility = Visibility.Collapsed;
                    if (BorderRecentVisits != null) BorderRecentVisits.Visibility = Visibility.Collapsed;
                    if (BorderHomeNews != null) BorderHomeNews.Visibility = Visibility.Collapsed;
                    if (BorderHomeDailyGoal != null) BorderHomeDailyGoal.Visibility = Visibility.Collapsed;
                    if (BorderHomePomodoro != null) BorderHomePomodoro.Visibility = Visibility.Collapsed;
                    if (BorderHomeNotes != null) BorderHomeNotes.Visibility = Visibility.Collapsed;
                    if (BorderHomeProductiveTools != null) BorderHomeProductiveTools.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperBackdrop != null) BorderWallpaperBackdrop.Visibility = Visibility.Visible;
                    if (BorderWallpaperOverlay != null) BorderWallpaperOverlay.Visibility = Visibility.Visible;
                    if (HomeScreen != null) HomeScreen.Background = Brushes.Transparent;
                    ApplyCurrentWallpaper(animate: false);
                    StartWallpaperTimer();
                    break;

                case "simple":
                default:
                    if (SpHomeMainContainer != null) SpHomeMainContainer.MaxWidth = 800;
                    if (SpHomeBrandHeader != null) SpHomeBrandHeader.Visibility = Visibility.Visible;
                    if (SpHomeProductiveHeader != null) SpHomeProductiveHeader.Visibility = Visibility.Collapsed;
                    if (BorderRecentVisits != null) BorderRecentVisits.Visibility = Visibility.Collapsed;
                    if (BorderHomeNews != null) BorderHomeNews.Visibility = Visibility.Collapsed;
                    if (BorderHomeDailyGoal != null) BorderHomeDailyGoal.Visibility = Visibility.Collapsed;
                    if (BorderHomePomodoro != null) BorderHomePomodoro.Visibility = Visibility.Collapsed;
                    if (BorderHomeNotes != null) BorderHomeNotes.Visibility = Visibility.Collapsed;
                    if (BorderHomeProductiveTools != null) BorderHomeProductiveTools.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperBackdrop != null) BorderWallpaperBackdrop.Visibility = Visibility.Collapsed;
                    if (BorderWallpaperOverlay != null) BorderWallpaperOverlay.Visibility = Visibility.Collapsed;
                    if (HomeScreen != null) HomeScreen.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F8FAFC"));
                    StopWallpaperTimer();
                    break;


            }
        }

        private void ApplyProductiveThemeColors(bool isDark)
        {
            var clockCol = isDark ? "#FFFFFF" : "#0F172A";
            var cardBg = isDark ? "#242424" : "#FFFFFF";
            var cardBorder = isDark ? "#383838" : "#CBD5E1";
            var textCol = isDark ? "#F8FAFC" : "#0F172A";
            var subTextCol = isDark ? "#94A3B8" : "#64748B";
            var noteBg = isDark ? "#181818" : "#F8FAFC";
            var greetingPillBg = isDark ? "#141E33" : "#F1F5F9";
            var greetingPillBorder = isDark ? "#25354F" : "#CBD5E1";

            if (TbHomeClock != null) TbHomeClock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(clockCol));
            if (TbHomeClockAmPm != null) TbHomeClockAmPm.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(subTextCol));
            if (BorderHomeGreeting != null)
            {
                BorderHomeGreeting.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(greetingPillBg));
                BorderHomeGreeting.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(greetingPillBorder));
            }
            if (TbHomeGreeting != null) TbHomeGreeting.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#E2E8F0" : "#383838"));

            Border?[] cards = { BorderHomeDailyGoal, BorderHomePomodoro, BorderHomeNotes, BorderHomeProductiveTools };
            foreach (var card in cards)
            {
                if (card != null)
                {
                    card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBg));
                    card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBorder));
                }
            }

            if (TbDailyFocusGoal != null) TbDailyFocusGoal.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textCol));
            if (TbQuickNote != null)
            {
                TbQuickNote.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(noteBg));
                TbQuickNote.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textCol));
                TbQuickNote.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBorder));
            }
        }

        private void InitClockTimer()
        {
            if (_clockTimer != null) return;
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (s, e) => UpdateProductiveClock();
            _clockTimer.Start();
            UpdateProductiveClock();
        }

        private void UpdateProductiveClock()
        {
            var now = DateTime.Now;

            if (TbHomeClock != null)
            {
                TbHomeClock.Text = now.ToString("hh:mm");
                if (TbHomeClockSeconds != null)
                {
                    TbHomeClockSeconds.Text = $":{now:ss}";
                }
                if (TbHomeClockAmPm != null)
                {
                    TbHomeClockAmPm.Text = now.ToString("tt");
                }
            }

            if (TbHomeGreeting != null)
            {
                string icon;
                string greeting;
                if (now.Hour >= 4 && now.Hour < 12) { icon = "🌅"; greeting = "Good morning"; }
                else if (now.Hour >= 12 && now.Hour < 17) { icon = "☀️"; greeting = "Good afternoon"; }
                else if (now.Hour >= 17 && now.Hour < 22) { icon = "🌆"; greeting = "Good evening"; }
                else { icon = "🌙"; greeting = "Late night focus"; }

                TbHomeGreeting.Text = $"{icon} {greeting}, Welcome to RUHI  •  📅 {now:dddd, MMMM dd}";
            }

            if (BorderHomeNews != null && BorderHomeNews.Visibility == Visibility.Visible && TbNewsDateGlance != null)
            {
                TbNewsDateGlance.Text = $"🕒 {now:h:mm tt}  •  📅 {now:dddd, d MMM}";
            }
        }

        #region Productive Mode Helpers & Handlers

        private void OnDailyFocusGoalChanged(object sender, TextChangedEventArgs e)
        {
            if (_isApplyingGoal || TbDailyFocusGoal == null) return;
            _settings.DailyGoalText = TbDailyFocusGoal.Text;
            DataManager.SaveSettings(_settings);
            UpdateDailyGoalUi();
        }

        private void OnDailyGoalCheckChanged(object sender, RoutedEventArgs e)
        {
            if (_isApplyingGoal) return;
            _settings.DailyGoalCompleted = CbDailyGoalCheck?.IsChecked == true;
            DataManager.SaveSettings(_settings);
            UpdateDailyGoalUi();
        }

        private void OnDailyGoalGotFocus(object sender, RoutedEventArgs e)
        {
            if (TbDailyGoalPlaceholder != null)
                TbDailyGoalPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void OnDailyGoalLostFocus(object sender, RoutedEventArgs e)
        {
            if (TbDailyGoalPlaceholder != null)
                TbDailyGoalPlaceholder.Visibility = string.IsNullOrEmpty(TbDailyFocusGoal?.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateDailyGoalUi()
        {
            bool hasText = !string.IsNullOrEmpty(TbDailyFocusGoal?.Text);
            bool isCompleted = CbDailyGoalCheck?.IsChecked == true;

            if (TbDailyGoalPlaceholder != null)
            {
                TbDailyGoalPlaceholder.Visibility = (hasText || TbDailyFocusGoal?.IsFocused == true) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (TbDailyGoalBadge != null)
            {
                TbDailyGoalBadge.Visibility = (isCompleted && hasText) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (TbDailyFocusGoal != null)
            {
                if (isCompleted && hasText)
                {
                    TbDailyFocusGoal.TextDecorations = TextDecorations.Strikethrough;
                    TbDailyFocusGoal.Opacity = 0.65;
                }
                else
                {
                    TbDailyFocusGoal.TextDecorations = null;
                    TbDailyFocusGoal.Opacity = 1.0;
                }
            }
        }

        private void InitPomodoroTimer()
        {
            if (_pomodoroTimer != null) return;
            _pomodoroTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _pomodoroTimer.Tick += (s, e) =>
            {
                if (_pomodoroSecondsRemaining > 0)
                {
                    _pomodoroSecondsRemaining--;
                    UpdatePomodoroDisplay();
                }
                else
                {
                    _pomodoroTimer.Stop();
                    _isPomodoroRunning = false;
                    if (BtnPomodoroToggle != null) BtnPomodoroToggle.Content = "▶ Start";
                    if (TbPomodoroStatus != null) TbPomodoroStatus.Text = "🎉 Session finished! Take a well-deserved break.";
                    try
                    {
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                    catch { }
                }
            };
        }

        private void UpdatePomodoroDisplay()
        {
            int mins = _pomodoroSecondsRemaining / 60;
            int secs = _pomodoroSecondsRemaining % 60;
            if (TbPomodoroTime != null)
            {
                TbPomodoroTime.Text = $"{mins:D2}:{secs:D2}";
            }
        }

        private void OnPomodoroToggleClicked(object sender, RoutedEventArgs e)
        {
            InitPomodoroTimer();

            if (_isPomodoroRunning)
            {
                _pomodoroTimer?.Stop();
                _isPomodoroRunning = false;
                if (BtnPomodoroToggle != null) BtnPomodoroToggle.Content = "▶ Resume";
                if (TbPomodoroStatus != null) TbPomodoroStatus.Text = "⏸ Paused";
            }
            else
            {
                if (_pomodoroSecondsRemaining <= 0)
                {
                    _pomodoroSecondsRemaining = _pomodoroDurationMinutes * 60;
                }
                _pomodoroTimer?.Start();
                _isPomodoroRunning = true;
                if (BtnPomodoroToggle != null) BtnPomodoroToggle.Content = "⏸ Pause";
                if (TbPomodoroStatus != null)
                {
                    TbPomodoroStatus.Text = _pomodoroDurationMinutes switch
                    {
                        5 => "☕ Short break in progress...",
                        15 => "🌴 Long break in progress...",
                        _ => "🎯 Deep focus in progress... Stay in the zone!"
                    };
                }
            }
        }

        private void OnPomodoroResetClicked(object sender, RoutedEventArgs e)
        {
            _pomodoroTimer?.Stop();
            _isPomodoroRunning = false;
            _pomodoroSecondsRemaining = _pomodoroDurationMinutes * 60;
            if (BtnPomodoroToggle != null) BtnPomodoroToggle.Content = "▶ Start";
            if (TbPomodoroStatus != null) TbPomodoroStatus.Text = $"Ready to focus • {_pomodoroDurationMinutes} min session";
            UpdatePomodoroDisplay();
        }

        private void OnPomodoroAdd5MinClicked(object sender, RoutedEventArgs e)
        {
            _pomodoroSecondsRemaining += 5 * 60;
            UpdatePomodoroDisplay();
        }

        private void OnPomodoroModeClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag && int.TryParse(tag, out int mins))
            {
                _pomodoroDurationMinutes = mins;
                _pomodoroSecondsRemaining = mins * 60;
                _pomodoroTimer?.Stop();
                _isPomodoroRunning = false;
                if (BtnPomodoroToggle != null) BtnPomodoroToggle.Content = "▶ Start";

                Button?[] pBtns = { BtnPomoFocus, BtnPomoShort, BtnPomoLong };
                foreach (var b in pBtns)
                {
                    if (b == null) continue;
                    bool isSel = (b == btn);
                    b.Background = isSel ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB")) : Brushes.Transparent;
                    b.Foreground = isSel ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
                }

                if (TbPomodoroStatus != null)
                {
                    TbPomodoroStatus.Text = mins switch
                    {
                        5 => "☕ Ready for a 5 min quick recharge",
                        15 => "🌴 Ready for a 15 min refreshing break",
                        _ => "🎯 Ready to focus • 25 min session"
                    };
                }
                UpdatePomodoroDisplay();
            }
        }

        private void UpdateNotesWordCount()
        {
            if (TbNotesWordCount == null || TbQuickNote == null) return;
            string text = TbQuickNote.Text ?? string.Empty;
            int chars = text.Length;
            int words = string.IsNullOrWhiteSpace(text)
                ? 0
                : text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            TbNotesWordCount.Text = $"{words} words • {chars} chars";
        }

        private void OnNotesAddBulletClicked(object sender, RoutedEventArgs e)
        {
            if (TbQuickNote == null) return;
            int caret = TbQuickNote.CaretIndex;
            string prefix = (caret > 0 && !TbQuickNote.Text.EndsWith("\n")) ? "\n• " : "• ";
            TbQuickNote.Text = TbQuickNote.Text.Insert(caret, prefix);
            TbQuickNote.CaretIndex = caret + prefix.Length;
            TbQuickNote.Focus();
        }

        private void OnNotesAddTimeClicked(object sender, RoutedEventArgs e)
        {
            if (TbQuickNote == null) return;
            int caret = TbQuickNote.CaretIndex;
            string stamp = $"[{DateTime.Now:HH:mm}] ";
            TbQuickNote.Text = TbQuickNote.Text.Insert(caret, stamp);
            TbQuickNote.CaretIndex = caret + stamp.Length;
            TbQuickNote.Focus();
        }

        private void OnProductiveQuickToolClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                if (tag.Equals("ai_assistant", StringComparison.OrdinalIgnoreCase))
                {
                    OnAiClicked(sender, e);
                }
                else if (tag.StartsWith("http://") || tag.StartsWith("https://"))
                {
                    if (_activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
                    {
                        NavigateTab(_activeTab, tag);
                    }
                    else
                    {
                        CreateNewTab(tag);
                    }
                }
            }
        }

        private void OnQuickNoteChanged(object sender, TextChangedEventArgs e)
        {
            UpdateNotesWordCount();
            if (_isApplyingNote || TbQuickNote == null) return;
            _settings.QuickNoteText = TbQuickNote.Text;
            DataManager.SaveSettings(_settings);
        }

        private void OnCopyNotesClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TbQuickNote != null && !string.IsNullOrEmpty(TbQuickNote.Text))
                {
                    Clipboard.SetText(TbQuickNote.Text);
                    MessageBox.Show(this, "Scratchpad notes copied to clipboard! 📋", "Quick Notes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch { }
        }

        private void OnClearNotesClicked(object sender, RoutedEventArgs e)
        {
            if (TbQuickNote != null) TbQuickNote.Text = string.Empty;
            _settings.QuickNoteText = string.Empty;
            DataManager.SaveSettings(_settings);
            UpdateNotesWordCount();
        }

        #endregion

        #region Scenic Wallpaper Handlers

        private void InitScenicWallpapers()
        {
            if (_scenicWallpaperImages.Count == 0)
            {
                for (int i = 0; i < _scenicWallpapers.Length; i++)
                {
                    _scenicWallpaperImages.Add(LoadWallpaperBitmap(_scenicWallpapers[i].relativePath));
                }
            }
        }

        private BitmapImage? LoadWallpaperBitmap(string relativePath)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                try
                {
                    string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();
                        return bmp;
                    }
                }
                catch { }
            }
            return null;
        }

        private void StartWallpaperTimer()
        {
            if (_wallpaperTimer == null)
            {
                _wallpaperTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(WallpaperIntervalSeconds) };
                _wallpaperTimer.Tick += (s, e) =>
                {
                    if (_isWallpaperAutoRotate)
                    {
                        _currentWallpaperIndex = (_currentWallpaperIndex + 1) % _scenicWallpapers.Length;
                        ApplyCurrentWallpaper(animate: true);
                    }
                };
            }
            if (_isWallpaperAutoRotate && !_wallpaperTimer.IsEnabled)
            {
                _wallpaperTimer.Start();
            }
        }

        private void StopWallpaperTimer()
        {
            _wallpaperTimer?.Stop();
        }

        private void ApplyCurrentWallpaper(bool animate)
        {
            InitScenicWallpapers();

            if (_currentWallpaperIndex < 0 || _currentWallpaperIndex >= _scenicWallpapers.Length)
            {
                _currentWallpaperIndex = 0;
            }

            BitmapImage? bmp = null;
            if (_currentWallpaperIndex < _scenicWallpaperImages.Count)
            {
                bmp = _scenicWallpaperImages[_currentWallpaperIndex];
            }

            if (ImgWallpaperBackdrop != null)
            {
                if (bmp != null)
                {
                    if (animate)
                    {
                        var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1.0, 0.3, TimeSpan.FromMilliseconds(200));
                        fadeOut.Completed += (s, e) =>
                        {
                            ImgWallpaperBackdrop.Source = bmp;
                            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0.3, 1.0, TimeSpan.FromMilliseconds(300));
                            ImgWallpaperBackdrop.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                        };
                        ImgWallpaperBackdrop.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                    }
                    else
                    {
                        ImgWallpaperBackdrop.Source = bmp;
                        ImgWallpaperBackdrop.Opacity = 1.0;
                    }
                }
            }
        }

        #endregion

        private async void LoadArticleImageAsync(Image targetImage, string? imageUrl, FrameworkElement? placeholder)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            if (_newsImageCache.TryGetValue(imageUrl, out var cachedSource))
            {
                targetImage.Source = cachedSource;
                targetImage.Visibility = Visibility.Visible;
                if (placeholder != null) placeholder.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                using var resp = await _newsClient.SendAsync(req);
                if (resp.IsSuccessStatusCode)
                {
                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    if (bytes.Length > 100)
                    {
                        var bmp = new BitmapImage();
                        using (var ms = new MemoryStream(bytes))
                        {
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.DecodePixelWidth = 550;
                            bmp.StreamSource = ms;
                            bmp.EndInit();
                        }
                        bmp.Freeze();

                        _newsImageCache[imageUrl] = bmp;
                        targetImage.Source = bmp;
                        targetImage.Visibility = Visibility.Visible;
                        if (placeholder != null) placeholder.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch
            {
                // Fallback remains visible
            }
        }

        private async void OnRefreshNewsClicked(object sender, RoutedEventArgs e)
        {
            await LoadTrendingNewsAsync(forceRefresh: true);
        }

        private async void OnNewsCategoryClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string cat)
            {
                _activeNewsCategory = cat.ToLowerInvariant();
                HighlightNewsCategoryButton(_activeNewsCategory);
                await LoadTrendingNewsAsync(forceRefresh: false);
            }
        }

        private void HighlightNewsCategoryButton(string activeCat)
        {
            Button?[] buttons = { BtnNewsCatTop, BtnNewsCatTech, BtnNewsCatBusiness, BtnNewsCatSports, BtnNewsCatEntertainment, BtnNewsCatWorld };
            bool isDark = IsCurrentThemeDark();

            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                bool isActive = string.Equals(btn.Tag?.ToString(), activeCat, StringComparison.OrdinalIgnoreCase);
                if (isActive)
                {
                    btn.Background = new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#1D4ED8"),
                        (Color)ColorConverter.ConvertFromString("#2563EB"),
                        new Point(0, 0), new Point(1, 0));
                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA"));
                    btn.Foreground = Brushes.White;
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#141E33" : "#F1F5F9"));
                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#25354F" : "#CBD5E1"));
                    btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#475569"));
                    btn.FontWeight = FontWeights.SemiBold;
                }
            }
        }

        private static (Brush bannerBrush, string emoji, string tagLabel) GetCategoryTheme(string category)
        {
            switch (category.ToLowerInvariant())
            {
                case "tech":
                    return (new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#0F172A"),
                        (Color)ColorConverter.ConvertFromString("#0284C7"),
                        new Point(0, 0), new Point(1, 1)), "💻", "Tech & Gadgets");

                case "business":
                    return (new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#0B132B"),
                        (Color)ColorConverter.ConvertFromString("#059669"),
                        new Point(0, 0), new Point(1, 1)), "💼", "Finance & Yojna");

                case "sports":
                    return (new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#1C1917"),
                        (Color)ColorConverter.ConvertFromString("#D97706"),
                        new Point(0, 0), new Point(1, 1)), "🏏", "Cricket & Sports");

                case "entertainment":
                    return (new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#1E1B4B"),
                        (Color)ColorConverter.ConvertFromString("#C026D3"),
                        new Point(0, 0), new Point(1, 1)), "🎬", "Entertainment");

                case "world":
                case "breaking":
                    return (new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#0F172A"),
                        (Color)ColorConverter.ConvertFromString("#DC2626"),
                        new Point(0, 0), new Point(1, 1)), "⚡", "Breaking News");

                default:
                    return (new LinearGradientBrush(
                        (Color)ColorConverter.ConvertFromString("#0B132B"),
                        (Color)ColorConverter.ConvertFromString("#2563EB"),
                        new Point(0, 0), new Point(1, 1)), "🔥", "Top Story");
            }
        }

        private async Task LoadTrendingNewsAsync(bool forceRefresh = false)
        {
            if (BorderHomeNews == null) return;

            var now = DateTime.Now;
            if (TbNewsDateGlance != null)
            {
                TbNewsDateGlance.Text = $"🕒 {now:h:mm tt}  •  📅 {now:dddd, d MMM}";
            }

            HighlightNewsCategoryButton(_activeNewsCategory);

            if (TbNewsLoadingStatus != null)
            {
                TbNewsLoadingStatus.Text = $"⏳ Loading {_activeNewsCategory} feed...";
                TbNewsLoadingStatus.Visibility = Visibility.Visible;
            }

            try
            {
                var articles = await FetchTrendingNewsAsync(forceRefresh);

                if (TbNewsLoadingStatus != null)
                {
                    TbNewsLoadingStatus.Visibility = Visibility.Collapsed;
                }

                if (BorderHeroArticle != null)
                {
                    BorderHeroArticle.Child = null;
                    BorderHeroArticle.Visibility = Visibility.Collapsed;
                }

                if (UgNewsArticles != null)
                {
                    UgNewsArticles.Children.Clear();
                }

                if (articles.Count == 0)
                {
                    if (TbNewsLoadingStatus != null)
                    {
                        TbNewsLoadingStatus.Text = "No stories available right now.";
                        TbNewsLoadingStatus.Visibility = Visibility.Visible;
                    }
                    return;
                }

                bool isDark = IsCurrentThemeDark();

                // 1. Featured Lead Hero Story Card with Real Photo
                if (articles.Count > 0 && BorderHeroArticle != null)
                {
                    BorderHeroArticle.Child = CreateEdgeHeroCard(articles[0], _activeNewsCategory, isDark);
                    BorderHeroArticle.Visibility = Visibility.Visible;
                }

                // 2. 3-Column Story Cards Grid (Loads up to 13 initial cards: 1 hero + 12 in grid)
                if (UgNewsArticles != null)
                {
                    int showCount = Math.Min(articles.Count, 13);
                    for (int i = 1; i < showCount; i++)
                    {
                        UgNewsArticles.Children.Add(CreateEdgeStoryCard(articles[i], _activeNewsCategory, isDark));
                    }

                    if (BtnNewsLoadMore != null)
                    {
                        BtnNewsLoadMore.Visibility = articles.Count > 13 ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                if (TbNewsLoadingStatus != null)
                {
                    TbNewsLoadingStatus.Text = "Unable to load feed: " + ex.Message;
                    TbNewsLoadingStatus.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnNewsLoadMoreClicked(object sender, RoutedEventArgs e)
        {
            if (UgNewsArticles == null) return;
            bool isDark = IsCurrentThemeDark();

            if (_newsCategoryCache.TryGetValue(_activeNewsCategory, out var cached) && cached.items != null)
            {
                int currentCount = UgNewsArticles.Children.Count + 1; // accounts for hero card
                for (int i = currentCount; i < cached.items.Count; i++)
                {
                    UgNewsArticles.Children.Add(CreateEdgeStoryCard(cached.items[i], _activeNewsCategory, isDark));
                }
            }

            if (BtnNewsLoadMore != null)
            {
                BtnNewsLoadMore.Visibility = Visibility.Collapsed;
            }
        }

        private Border CreateEdgeHeroCard(NewsArticleItem art, string category, bool isDark)
        {
            var cardBg = isDark ? "#162032" : "#FFFFFF";
            var cardHoverBg = isDark ? "#1D2D47" : "#F8FAFC";
            var borderCol = isDark ? "#24334C" : "#E2E8F0";
            var titleCol = isDark ? "#FFFFFF" : "#0F172A";

            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBg)),
                CornerRadius = new CornerRadius(14),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderCol)),
                BorderThickness = new Thickness(1),
                Height = 230,
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(380) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var theme = GetCategoryTheme(category);

            // Left: Photo Container
            var visualBorder = new Border
            {
                Background = theme.bannerBrush,
                CornerRadius = new CornerRadius(13, 0, 0, 13),
                ClipToBounds = true
            };
            var visualGrid = new Grid();

            var placeholderBorder = new Border { Background = theme.bannerBrush };
            var iconText = new TextBlock
            {
                Text = theme.emoji,
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            placeholderBorder.Child = iconText;
            visualGrid.Children.Add(placeholderBorder);

            var realImage = new Image
            {
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            visualGrid.Children.Add(realImage);

            var leadPill = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(7, 3, 7, 3),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10)
            };
            leadPill.Child = new TextBlock
            {
                Text = "★ LEAD STORY",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            visualGrid.Children.Add(leadPill);
            visualBorder.Child = visualGrid;
            Grid.SetColumn(visualBorder, 0);
            grid.Children.Add(visualBorder);

            LoadArticleImageAsync(realImage, art.ImageUrl, placeholderBorder);

            // Right: Content Details
            var spContent = new StackPanel
            {
                Margin = new Thickness(22, 16, 22, 16),
                VerticalAlignment = VerticalAlignment.Center
            };

            var spMetaTop = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            var badgeSource = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(7, 2, 7, 2),
                Margin = new Thickness(0, 0, 8, 0)
            };
            badgeSource.Child = new TextBlock
            {
                Text = art.Source.ToUpperInvariant(),
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            spMetaTop.Children.Add(badgeSource);

            var tbTimeTop = new TextBlock
            {
                Text = art.TimeAgo,
                FontSize = 11.5,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                VerticalAlignment = VerticalAlignment.Center
            };
            spMetaTop.Children.Add(tbTimeTop);
            spContent.Children.Add(spMetaTop);

            var tbHeadline = new TextBlock
            {
                Text = art.Title,
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                LineHeight = 23,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(titleCol)),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 64,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 2, 0, 6)
            };
            spContent.Children.Add(tbHeadline);

            if (!string.IsNullOrEmpty(art.Description))
            {
                var tbDesc = new TextBlock
                {
                    Text = art.Description,
                    FontSize = 12.5,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    TextWrapping = TextWrapping.Wrap,
                    MaxHeight = 38,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    LineHeight = 18,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                spContent.Children.Add(tbDesc);
            }

            var tbReadAction = new TextBlock
            {
                Text = "Read on TaazaWire ↗",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"))
            };
            spContent.Children.Add(tbReadAction);

            Grid.SetColumn(spContent, 1);
            grid.Children.Add(spContent);

            card.Child = grid;

            card.MouseEnter += (s, e) =>
            {
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardHoverBg));
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            };
            card.MouseLeave += (s, e) =>
            {
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBg));
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderCol));
            };

            string targetUrl = art.Url;
            card.MouseLeftButtonUp += (s, e) => OpenNewsArticle(targetUrl);

            return card;
        }

        private Border CreateEdgeStoryCard(NewsArticleItem art, string category, bool isDark)
        {
            var cardBg = isDark ? "#162032" : "#FFFFFF";
            var cardHoverBg = isDark ? "#1D2D47" : "#F8FAFC";
            var borderCol = isDark ? "#24334C" : "#E2E8F0";
            var titleCol = isDark ? "#FFFFFF" : "#0F172A";

            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBg)),
                CornerRadius = new CornerRadius(12),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderCol)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(6),
                Cursor = Cursors.Hand
            };

            var spCard = new StackPanel();
            var theme = GetCategoryTheme(category);

            // Visual Photo Header (Height 150)
            var bannerBorder = new Border
            {
                Background = theme.bannerBrush,
                CornerRadius = new CornerRadius(11, 11, 0, 0),
                ClipToBounds = true,
                Height = 150
            };
            var bannerGrid = new Grid();

            var placeholderBorder = new Border { Background = theme.bannerBrush };
            var iconText = new TextBlock
            {
                Text = theme.emoji,
                FontSize = 34,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            placeholderBorder.Child = iconText;
            bannerGrid.Children.Add(placeholderBorder);

            var realImage = new Image
            {
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            bannerGrid.Children.Add(realImage);

            var catPill = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(8)
            };
            catPill.Child = new TextBlock
            {
                Text = theme.tagLabel.ToUpperInvariant(),
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            bannerGrid.Children.Add(catPill);
            bannerBorder.Child = bannerGrid;
            spCard.Children.Add(bannerBorder);

            LoadArticleImageAsync(realImage, art.ImageUrl, placeholderBorder);

            // Card Body
            var spBody = new StackPanel { Margin = new Thickness(12, 10, 12, 12) };

            var spMeta = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            var badgeSource = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#F1F5F9")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(5, 1, 5, 1),
                Margin = new Thickness(0, 0, 6, 0)
            };
            badgeSource.Child = new TextBlock
            {
                Text = art.Source,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#60A5FA" : "#2563EB"))
            };
            spMeta.Children.Add(badgeSource);

            var tbTime = new TextBlock
            {
                Text = art.TimeAgo,
                FontSize = 10.5,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                VerticalAlignment = VerticalAlignment.Center
            };
            spMeta.Children.Add(tbTime);
            spBody.Children.Add(spMeta);

            var gridTitle = new Grid();
            gridTitle.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridTitle.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var tbTitle = new TextBlock
            {
                Text = art.Title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                LineHeight = 18,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(titleCol)),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 36,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(tbTitle, 0);
            gridTitle.Children.Add(tbTitle);

            var tbArrow = new TextBlock
            {
                Text = "↗",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(6, 0, 0, 0)
            };
            Grid.SetColumn(tbArrow, 1);
            gridTitle.Children.Add(tbArrow);

            spBody.Children.Add(gridTitle);
            spCard.Children.Add(spBody);

            card.Child = spCard;

            card.MouseEnter += (s, e) =>
            {
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardHoverBg));
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            };
            card.MouseLeave += (s, e) =>
            {
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cardBg));
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderCol));
            };

            string targetUrl = art.Url;
            card.MouseLeftButtonUp += (s, e) => OpenNewsArticle(targetUrl);

            return card;
        }

        private void OpenNewsArticle(string targetUrl)
        {
            if (string.IsNullOrEmpty(targetUrl)) return;

            if (_activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || HomeScreen.Visibility == Visibility.Visible))
            {
                NavigateTab(_activeTab, targetUrl);
            }
            else
            {
                CreateNewTab(targetUrl);
            }
        }

        private async Task<List<NewsArticleItem>> FetchTrendingNewsAsync(bool forceRefresh = false)
        {
            string category = _activeNewsCategory;
            if (!forceRefresh && _newsCategoryCache.TryGetValue(category, out var cached) && (DateTime.Now - cached.fetchedAt).TotalMinutes < 15)
            {
                return cached.items;
            }

            try
            {
                const string feedUrl = "https://taazawire.in/feed/";

                var xmlString = await _newsClient.GetStringAsync(feedUrl);
                var doc = XDocument.Parse(xmlString);
                var items = doc.Descendants("item");

                var allArticles = new List<NewsArticleItem>();
                foreach (var item in items)
                {
                    string rawTitle = item.Element("title")?.Value ?? "";
                    string link = item.Element("link")?.Value ?? "";
                    string source = item.Element("source")?.Value ?? "";
                    string pubDate = item.Element("pubDate")?.Value ?? "";
                    string rawDesc = item.Element("description")?.Value ?? "";

                    string cleanTitle = rawTitle.Trim();
                    const string sourceSuffix = " - TaazaWire";
                    if (cleanTitle.EndsWith(sourceSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        cleanTitle = cleanTitle.Substring(0, cleanTitle.Length - sourceSuffix.Length).Trim();
                    }

                    string cleanDesc = System.Text.RegularExpressions.Regex.Replace(rawDesc, "<.*?>", string.Empty).Trim();
                    if (cleanDesc.Length > 160) cleanDesc = cleanDesc.Substring(0, 157) + "...";

                    string timeAgo = "Just now";
                    if (DateTime.TryParse(pubDate, out var dt))
                    {
                        var diff = DateTime.UtcNow - dt.ToUniversalTime();
                        if (diff.TotalHours < 1) timeAgo = $"{(int)Math.Max(1, diff.TotalMinutes)}m ago";
                        else if (diff.TotalHours < 24) timeAgo = $"{(int)diff.TotalHours}h ago";
                        else timeAgo = $"{(int)diff.TotalDays}d ago";
                    }

                    // Extract high-res image
                    string? imageUrl = null;
                    var enc = item.Element("enclosure");
                    if (enc != null && enc.Attribute("url") != null)
                    {
                        imageUrl = enc.Attribute("url")?.Value;
                    }
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        foreach (var elem in item.Elements())
                        {
                            if ((elem.Name.LocalName.Equals("content", StringComparison.OrdinalIgnoreCase) ||
                                 elem.Name.LocalName.Equals("thumbnail", StringComparison.OrdinalIgnoreCase)) &&
                                elem.Attribute("url") != null)
                            {
                                imageUrl = elem.Attribute("url")?.Value;
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(rawDesc, @"<img[^>]+src=[""'](?<url>[^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            imageUrl = m.Groups["url"].Value;
                        }
                    }

                    var itemCats = item.Elements("category").Select(c => c.Value.Trim().ToLowerInvariant()).ToList();
                    string primaryCat = itemCats.FirstOrDefault() ?? "top";

                    string finalSource = string.IsNullOrEmpty(source) ? "TaazaWire" : source;

                    allArticles.Add(new NewsArticleItem
                    {
                        Title = cleanTitle,
                        Description = cleanDesc,
                        Source = finalSource,
                        Url = string.IsNullOrWhiteSpace(link) ? "https://taazawire.in/" : link,
                        TimeAgo = timeAgo,
                        Category = primaryCat,
                        ImageUrl = imageUrl
                    });
                }

                // Filter by category
                List<NewsArticleItem> filteredList;
                switch (category.ToLowerInvariant())
                {
                    case "tech":
                        filteredList = allArticles.Where(a => a.Category.Contains("tech") || a.Title.Contains("टेक") || a.Title.Contains("5G") || a.Title.Contains("स्मार्टफोन")).ToList();
                        break;
                    case "business":
                        filteredList = allArticles.Where(a => a.Category.Contains("finance") || a.Category.Contains("business") || a.Category.Contains("yojna") || a.Title.Contains("रुपये") || a.Title.Contains("बैंक")).ToList();
                        break;
                    case "sports":
                        filteredList = allArticles.Where(a => a.Category.Contains("cricket") || a.Category.Contains("sport") || a.Title.Contains("टेस्ट") || a.Title.Contains("विकेट") || a.Title.Contains("मैच")).ToList();
                        break;
                    case "entertainment":
                        filteredList = allArticles.Where(a => a.Category.Contains("entertainment") || a.Category.Contains("cinema") || a.Title.Contains("फिल्म") || a.Title.Contains("गाना") || a.Title.Contains("शाहरुख")).ToList();
                        break;
                    case "world":
                    case "breaking":
                        filteredList = allArticles.Where(a => a.Category.Contains("breaking") || a.Category.Contains("world") || a.Category.Contains("yojna")).ToList();
                        break;
                    default: // top
                        filteredList = allArticles;
                        break;
                }

                // Top up if a specific category has few items so layout is full
                if (filteredList.Count < 6 && category != "top")
                {
                    foreach (var a in allArticles)
                    {
                        if (!filteredList.Contains(a))
                        {
                            filteredList.Add(a);
                            if (filteredList.Count >= 13) break;
                        }
                    }
                }

                if (filteredList.Count > 0)
                {
                    _newsCategoryCache[category] = (DateTime.Now, filteredList);
                    return filteredList;
                }
            }
            catch { }

            return GetFallbackNews(category);
        }

        private static List<NewsArticleItem> GetFallbackNews(string category)
        {
            switch (category.ToLowerInvariant())
            {
                case "tech":
                    return new List<NewsArticleItem>
                    {
                        new NewsArticleItem { Title = "स्मार्टफोन और टेक जगत की ताज़ा खबरें, नए 5G लॉन्च और गैजेट्स रिव्यू", Description = "टेक जगत के नए 5G स्मार्टफोन्स, AI फीचर्स और तकनीकी अपडेट्स जानिए TaazaWire पर।", Source = "TaazaWire", Url = "https://taazawire.in/category/tech", TimeAgo = "15m ago", Category = "tech" },
                        new NewsArticleItem { Title = "Vivo और OnePlus के नए फ्लैगशिप स्मार्टफोन्स में मिलेगी 9000mAh बैटरी और AMOLED डिस्प्ले", Description = "फास्ट चार्जिंग और बेहतरीन कैमरा सेंसर्स के साथ नए स्मार्टफोन सीरीज का ऐलान।", Source = "TaazaWire", Url = "https://taazawire.in/category/tech", TimeAgo = "1h ago", Category = "tech" },
                        new NewsArticleItem { Title = "Redmi 5G स्मार्टफोन 120Hz OLED डिस्प्ले के साथ हुआ लॉन्च, जानें कीमत और फीचर्स", Description = "किफायती कीमत में शानदार डिस्प्ले और दमदार प्रोसेसर के साथ नया डिवाइस पेश।", Source = "TaazaWire", Url = "https://taazawire.in/category/tech", TimeAgo = "2h ago", Category = "tech" },
                        new NewsArticleItem { Title = "Samsung 50MP कैमरा और Exynos प्रोसेसर के साथ नया 5G स्मार्टफोन पेश करने को तैयार", Description = "प्रीमियम डिज़ाइन और आधुनिक AI फोटोग्राफी फीचर्स के साथ जल्द होगी एंट्री।", Source = "TaazaWire", Url = "https://taazawire.in/category/tech", TimeAgo = "3h ago", Category = "tech" }
                    };
                case "business":
                    return new List<NewsArticleItem>
                    {
                        new NewsArticleItem { Title = "फाइनेंस और सरकारी योजनाओं के नए नियम, जानें कैसे मिलेगा लाभ", Description = "केंद्रीय व राज्य सरकारी योजनाओं के आवेदन, सब्सिडी और वित्तीय अपडेट्स।", Source = "TaazaWire", Url = "https://taazawire.in/category/finance", TimeAgo = "20m ago", Category = "business" },
                        new NewsArticleItem { Title = "प्रधानमंत्री आवास व कल्याणकारी योजनाओं की नई सूची जारी, ऐसे चेक करें नाम", Description = "पात्र परिवारों के लिए सरकारी सहायता राशि और योजना के नए दिशा-निर्देश जारी।", Source = "TaazaWire", Url = "https://taazawire.in/category/sarkari-yojna", TimeAgo = "1h ago", Category = "business" },
                        new NewsArticleItem { Title = "बैंक फिक्स्ड डिपॉजिट और बचत खातों पर ब्याज दरों में संशोधन, ग्राहकों को मिलेगा लाभ", Description = "वरिष्ठ नागरिकों और आम ग्राहकों के लिए नई ब्याज दरों का ऐलान।", Source = "TaazaWire", Url = "https://taazawire.in/category/finance", TimeAgo = "2h ago", Category = "business" }
                    };
                case "sports":
                    return new List<NewsArticleItem>
                    {
                        new NewsArticleItem { Title = "अनौपचारिक टेस्ट: मैथ्यू बॉयल का शानदार शतक, मुकेश ने झटके पांच विकेट", Description = "भारत ए और न्यूजीलैंड ए के बीच रोमांचक मुकाबला, गेंदबाजों और बल्लेबाजों का दमदार प्रदर्शन।", Source = "TaazaWire", Url = "https://taazawire.in/category/cricket", TimeAgo = "30m ago", Category = "sports" },
                        new NewsArticleItem { Title = "क्रिकेट वर्ल्ड टूर्नामेंट: आगामी सीरीज के लिए भारतीय टीम का ऐलान", Description = "नए युवा खिलाड़ियों को मिला मौका, अनुभवी सीनियर्स की टीम में वापसी।", Source = "TaazaWire", Url = "https://taazawire.in/category/cricket", TimeAgo = "1h ago", Category = "sports" },
                        new NewsArticleItem { Title = "टी20 लीग के रोमांचक मुकाबले में आखिरी ओवर में शानदार जीत", Description = "शानदार छक्के के साथ मैच का रोमांचक अंत, दर्शकों में उत्साह का माहौल।", Source = "TaazaWire", Url = "https://taazawire.in/category/cricket", TimeAgo = "2h ago", Category = "sports" }
                    };
                case "entertainment":
                    return new List<NewsArticleItem>
                    {
                        new NewsArticleItem { Title = "शाहरुख खान के जन्मदिन पर फैंस को तोहफा: 'किंग' फिल्म का पहला गाना हुआ रिलीज", Description = "फिल्म के मेकर्स ने रिलीज किया धमाकेदार गाना, मन्नत के बाहर फैंस का उमड़ा हुजूम।", Source = "TaazaWire", Url = "https://taazawire.in/category/entertainment", TimeAgo = "25m ago", Category = "entertainment" },
                        new NewsArticleItem { Title = "बॉक्स ऑफिस पर नए रिकॉर्ड: आगामी ब्लॉकबस्टर फिल्मों की रिलीज डेट का ऐलान", Description = "सिनेमाघरों में आने वाली बड़ी फिल्मों का टीजर जारी, सोशल मीडिया पर चर्चा।", Source = "TaazaWire", Url = "https://taazawire.in/category/entertainment", TimeAgo = "1h ago", Category = "entertainment" },
                        new NewsArticleItem { Title = "ओटीटी प्लेटफॉर्म्स पर इस हफ्ते रिलीज होने वाली धमाकेदार फिल्में और वेब सीरीज", Description = "सस्पेंस, एक्शन और थ्रिलर से भरपूर नए शोज की पूरी लिस्ट देखिए TaazaWire पर।", Source = "TaazaWire", Url = "https://taazawire.in/category/entertainment", TimeAgo = "2h ago", Category = "entertainment" }
                    };
                case "world":
                case "breaking":
                    return new List<NewsArticleItem>
                    {
                        new NewsArticleItem { Title = "अपडेट्स: तमिलनाडु में टला बड़ा हादसा, सात रेल बोगियां पटरी से उतरीं", Description = "रेलवे अधिकारियों ने मौके पर पहुंचकर राहत और बहाली कार्य शुरू किया।", Source = "TaazaWire", Url = "https://taazawire.in/category/breaking", TimeAgo = "15m ago", Category = "world" },
                        new NewsArticleItem { Title = "चक्रवाती तूफान मुरासु को लेकर IMD का हाई अलर्ट, कई राज्यों में भारी बारिश की चेतावनी", Description = "मौसम विभाग ने तटीय इलाकों के लिए चेतावनी जारी की, सुरक्षा बल अलर्ट मोड पर।", Source = "TaazaWire", Url = "https://taazawire.in/category/breaking", TimeAgo = "1h ago", Category = "world" },
                        new NewsArticleItem { Title = "यूजीसी नेट परीक्षा: लाखों अभ्यर्थियों ने जमा की फीस, करेक्शन विंडो खुली", Description = "उम्मीदवार आधिकारिक पोर्टल पर जाकर अपने आवेदन में जरूरी सुधार कर सकते हैं।", Source = "TaazaWire", Url = "https://taazawire.in/category/breaking", TimeAgo = "2h ago", Category = "world" }
                    };
                default:
                    return new List<NewsArticleItem>
                    {
                        new NewsArticleItem { Title = "किंग: ‘इस बार दिल में आग लगी है’; शाहरुख के बर्थडे पर फैंस को तोहफा, रिलीज हुआ पहला गाना", Description = "शाहरुख खान के जन्मदिन के मौके पर 'किंग' फिल्म का पहला गाना रिलीज, जानिए क्या है खास।", Source = "TaazaWire", Url = "https://taazawire.in/", TimeAgo = "Just now", Category = "top" },
                        new NewsArticleItem { Title = "तमिलनाडु में टला बड़ा हादसा, बिना इंजन छह किमी दौड़ीं रेल बोगियां", Description = "रेलवे प्रशासन ने जांच के आदेश दिए, यात्रियों की सुरक्षा सुनिश्चित।", Source = "TaazaWire", Url = "https://taazawire.in/category/breaking", TimeAgo = "30m ago", Category = "top" },
                        new NewsArticleItem { Title = "चक्रवाती तूफान का अलर्ट: तटीय राज्यों में मूसलाधार बारिश की संभावना, मौसम विभाग की चेतावनी", Description = "मौसम विभाग ने आपातकालीन सेवाओं को तैयार रहने के निर्देश दिए।", Source = "TaazaWire", Url = "https://taazawire.in/category/breaking", TimeAgo = "45m ago", Category = "top" },
                        new NewsArticleItem { Title = "स्मार्टफोन जगत में 9000mAh बैटरी और AMOLED डिस्प्ले के नए डिवाइस जल्द", Description = "आधुनिक टेक फीचर्स और किफायती कीमतों के साथ नए मॉडल लॉन्च की तैयारी।", Source = "TaazaWire", Url = "https://taazawire.in/category/tech", TimeAgo = "1h ago", Category = "top" }
                    };
            }
        }

        #endregion

        #region Memory Saver / Tab Snoozing

        private DispatcherTimer? _memorySaverTimer;

        private void InitMemorySaverTimer()
        {
            if (_memorySaverTimer != null) return;
            _memorySaverTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            _memorySaverTimer.Tick += (s, e) =>
            {
                if (!_settings.MemorySaverEnabled) return;
                int sleepThresholdMins = Math.Max(5, _settings.InactiveTabSleepMinutes);

                foreach (var tab in _tabs)
                {
                    if (tab != _activeTab && tab != _splitTab && !tab.IsPinned && !tab.IsSleeping && !tab.IsMuted)
                    {
                        if ((DateTime.Now - tab.LastActiveTime).TotalMinutes >= sleepThresholdMins)
                        {
                            SleepTab(tab);
                        }
                    }
                }
            };
            _memorySaverTimer.Start();
        }

        private async void SleepTab(BrowserTab tab)
        {
            if (tab == _activeTab || tab == _splitTab || tab.IsPinned || tab.IsSleeping) return;

            tab.IsSleeping = true;
            try
            {
                if (tab.WebView?.CoreWebView2 != null)
                {
                    await tab.WebView.CoreWebView2.TrySuspendAsync();
                }
            }
            catch { }

            RenderTabHeaders();
        }

        private void WakeTab(BrowserTab tab)
        {
            if (!tab.IsSleeping) return;
            try
            {
                tab.WebView?.CoreWebView2?.Resume();
            }
            catch { }

            tab.IsSleeping = false;
            tab.LastActiveTime = DateTime.Now;
            RenderTabHeaders();
        }

        #endregion

        #region Privacy & Ad Shield Counter

        private void UpdateShieldBadge()
        {
            if (BtnShield == null || TbShieldCount == null) return;

            if (_activeTab == null || string.IsNullOrWhiteSpace(_activeTab.Url) || _activeTab.Url == "about:blank" || HomeScreen.Visibility == Visibility.Visible)
            {
                BtnShield.Visibility = Visibility.Collapsed;
                return;
            }

            BtnShield.Visibility = Visibility.Visible;
            int count = _activeTab.BlockedAdsCount;
            TbShieldCount.Text = count.ToString();

            string host = GetDomain(_activeTab.Url);
            bool isAllowed = AdBlockManager.IsDomainAllowed(host);

            if (isAllowed || !AdBlockManager.IsEnabled)
            {
                BtnShield.ToolTip = $"RUHI Shield — Paused for {host}";
                TbShieldCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }
            else
            {
                BtnShield.ToolTip = $"RUHI Shield — {count} ads & trackers blocked on {host}";
                TbShieldCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(count > 0 ? "#38BDF8" : "#94A3B8"));
            }
        }

        private void OnShieldClicked(object sender, RoutedEventArgs e)
        {
            if (PopupShield == null || _activeTab == null) return;

            string host = GetDomain(_activeTab.Url);
            if (string.IsNullOrEmpty(host)) host = "Current Site";

            if (TbShieldSiteDomain != null) TbShieldSiteDomain.Text = host;
            if (TbShieldPageBlocked != null) TbShieldPageBlocked.Text = $"{_activeTab.BlockedAdsCount} ads & trackers blocked";
            if (TbShieldTotalSession != null) TbShieldTotalSession.Text = $"Total session: {AdBlockManager.TotalBlockedCount}";

            bool isAllowed = AdBlockManager.IsDomainAllowed(host);
            if (CbShieldSiteToggle != null)
            {
                CbShieldSiteToggle.IsChecked = !isAllowed;
            }
            if (TbShieldStatusText != null)
            {
                TbShieldStatusText.Text = (!isAllowed && AdBlockManager.IsEnabled) ? "Protection is Active" : "Shield Paused on this site";
                TbShieldStatusText.Foreground = (!isAllowed && AdBlockManager.IsEnabled)
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
            }

            PopupShield.IsOpen = true;
        }

        private void OnShieldSiteToggleClicked(object sender, RoutedEventArgs e)
        {
            if (_activeTab == null) return;
            string host = GetDomain(_activeTab.Url);
            if (string.IsNullOrEmpty(host)) return;

            bool isProtectOn = CbShieldSiteToggle?.IsChecked ?? true;
            AdBlockManager.SetDomainAllowed(host, !isProtectOn);

            UpdateShieldBadge();
            if (TbShieldStatusText != null)
            {
                TbShieldStatusText.Text = isProtectOn ? "Protection is Active" : "Shield Paused on this site";
                TbShieldStatusText.Foreground = isProtectOn
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
            }
        }

        #endregion

        #region Split Screen (Side-by-Side Dual Tabs)

        private BrowserTab? _splitTab = null;
        private bool _isSplitScreen = false;

        private void OnSplitScreenClicked(object sender, RoutedEventArgs e)
        {
            if (_isSplitScreen)
            {
                CloseSplitScreen();
            }
            else
            {
                var otherTab = _tabs.FirstOrDefault(t => t != _activeTab && !t.IsPinned) ??
                               _tabs.FirstOrDefault(t => t != _activeTab);

                if (otherTab == null)
                {
                    otherTab = CreateNewTab("https://taazawire.in");
                }

                if (_activeTab != null && otherTab != null)
                {
                    ActivateSplitScreen(_activeTab, otherTab);
                }
            }
        }

        private void ActivateSplitScreen(BrowserTab leftTab, BrowserTab rightTab)
        {
            _isSplitScreen = true;
            _splitTab = rightTab;

            if (rightTab.IsSleeping)
            {
                WakeTab(rightTab);
            }

            RenderSplitView();
        }

        private void RenderSplitView()
        {
            if (!_isSplitScreen || _splitTab == null || _activeTab == null) return;

            ColWebLeft.Width = new GridLength(1, GridUnitType.Star);
            ColWebSplitter.Width = new GridLength(4);
            ColWebRight.Width = new GridLength(1, GridUnitType.Star);

            SplitterWeb.Visibility = Visibility.Visible;
            GridWebRight.Visibility = Visibility.Visible;

            // Attach Left WebView
            GridWebLeft.Children.Clear();
            if (_activeTab.WebView != null)
            {
                GridWebLeft.Children.Add(_activeTab.WebView);
            }

            // Attach Right WebView
            GridWebRightContent.Children.Clear();
            if (_splitTab.WebView != null)
            {
                GridWebRightContent.Children.Add(_splitTab.WebView);
            }

            if (TbSplitRightTitle != null)
            {
                TbSplitRightTitle.Text = string.IsNullOrWhiteSpace(_splitTab.Title) ? "Right Tab" : _splitTab.Title;
            }

            if (BtnSplitScreen != null)
            {
                BtnSplitScreen.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
                BtnSplitScreen.Foreground = Brushes.White;
                BtnSplitScreen.ToolTip = "Exit Split Screen View";
            }

            HomeScreen.Visibility = Visibility.Collapsed;
            WebContainer.Visibility = Visibility.Visible;
        }

        private void CloseSplitScreen()
        {
            _isSplitScreen = false;
            _splitTab = null;

            ColWebLeft.Width = new GridLength(1, GridUnitType.Star);
            ColWebSplitter.Width = new GridLength(0);
            ColWebRight.Width = new GridLength(0);

            SplitterWeb.Visibility = Visibility.Collapsed;
            GridWebRight.Visibility = Visibility.Collapsed;
            GridWebRightContent.Children.Clear();

            GridWebLeft.Children.Clear();
            if (_activeTab?.WebView != null)
            {
                GridWebLeft.Children.Add(_activeTab.WebView);
            }

            if (BtnSplitScreen != null)
            {
                BtnSplitScreen.Background = Brushes.Transparent;
                BtnSplitScreen.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(IsCurrentThemeDark() ? "#94A3B8" : "#475569"));
                BtnSplitScreen.ToolTip = "Split Screen (Side-by-Side Dual Tabs)";
            }

            if (_activeTab != null && (string.IsNullOrWhiteSpace(_activeTab.Url) || _activeTab.Url == "about:blank"))
            {
                HomeScreen.Visibility = Visibility.Visible;
                WebContainer.Visibility = Visibility.Collapsed;
            }
            else
            {
                HomeScreen.Visibility = Visibility.Collapsed;
                WebContainer.Visibility = Visibility.Visible;
            }
        }

        private void OnCloseSplitScreenClicked(object sender, RoutedEventArgs e)
        {
            CloseSplitScreen();
        }

        #endregion

        #region Cloud Sync & Devices

        private DispatcherTimer? _syncPollTimer;
        private DispatcherTimer? _syncPeriodicTimer;

        private void InitSyncService()
        {
            SyncService.Initialize(_settings);
            SyncService.TabReceived += OnTabReceivedFromOtherDevice;
            SyncService.SyncCompleted += () => Dispatcher.Invoke(UpdateSyncButtonUI);

            UpdateSyncButtonUI();

            if (_syncPollTimer == null)
            {
                // Check incoming tabs from phone every 2.5 seconds for instant response
                _syncPollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                _syncPollTimer.Tick += async (s, e) =>
                {
                    if (SyncService.IsLoggedIn)
                    {
                        await SyncService.CheckIncomingTabsAsync();
                    }
                };
                _syncPollTimer.Start();
            }

            if (_syncPeriodicTimer == null)
            {
                // Auto background sync every 30 seconds
                _syncPeriodicTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                _syncPeriodicTimer.Tick += async (s, e) =>
                {
                    if (SyncService.IsLoggedIn)
                    {
                        await PerformFullSyncAsync();
                    }
                };
                _syncPeriodicTimer.Start();
            }

            if (SyncService.IsLoggedIn)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    await Dispatcher.InvokeAsync(PerformFullSyncAsync);
                });
            }
        }

        private void OnTabReceivedFromOtherDevice(SentTab sentTab)
        {
            Dispatcher.Invoke(() =>
            {
                CreateNewTab(sentTab.Url);
                MessageBox.Show(this,
                    $"📱 Tab received from linked phone / device!\n\nTitle: {sentTab.Title}\nURL: {sentTab.Url}",
                    "Tab Received from Phone",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }

        private void UpdateSyncButtonUI()
        {
            if (DotSyncActive != null)
            {
                DotSyncActive.Visibility = SyncService.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
            }
            if (BtnSendToPhone != null)
            {
                BtnSendToPhone.ToolTip = SyncService.IsLoggedIn
                    ? $"Send Tab to Phone (📱 Linked: {SyncService.CurrentEmail})"
                    : "Send Tab to Phone (Click to Sign In & Link Devices)";
            }
        }

        private async void OnSendToPhoneClicked(object sender, RoutedEventArgs e)
        {
            if (!SyncService.IsLoggedIn)
            {
                var res = MessageBox.Show(this,
                    "Ruhi Account is required to send tabs between devices.\n\nWould you like to open Ruhi Account to sign in?",
                    "Ruhi Account",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes)
                {
                    OnSyncClicked(sender, e);
                }
                return;
            }

            string currentUrl = _activeTab?.Url ?? "";
            string currentTitle = _activeTab?.Title ?? "";

            if (string.IsNullOrWhiteSpace(currentUrl) || currentUrl.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "Please open a webpage first to send it to your phone.", "Send to Phone", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool sent = await SyncService.SendTabToDeviceAsync(null, currentUrl, currentTitle);
            if (sent)
            {
                MessageBox.Show(this, $"📱 \"{currentTitle}\" has been sent to your phone!", "Sent to Phone", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, "Failed to send tab to phone. Please check your internet connection.", "Send to Phone", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSyncClicked(object sender, RoutedEventArgs e)
        {
            var win = new SyncAccountWindow(_activeTab?.Url ?? "", _activeTab?.Title ?? "", PerformFullSyncAsync)
            {
                Owner = this
            };
            win.ShowDialog();
            UpdateSyncButtonUI();
        }

        private async Task PerformFullSyncAsync()
        {
            if (!SyncService.IsLoggedIn) return;
            try
            {
                await SyncService.RefreshUserProfileAsync();

                // 1. Pull remote bookmarks & apply deletions first
                var (remoteActive, remoteDeleted) = await SyncService.PullBookmarksAsync();
                if (remoteDeleted.Count > 0)
                {
                    int removed = _bookmarks.RemoveAll(b => remoteDeleted.Any(d => d.Equals(b.Url, StringComparison.OrdinalIgnoreCase)));
                    if (removed > 0)
                    {
                        DataManager.SaveBookmarks(_bookmarks);
                        Dispatcher.Invoke(() => UpdateBookmarkButton(_activeTab?.Url ?? ""));
                    }
                }

                // Add active bookmarks from remote that we don't have and are not deleted
                bool addedAny = false;
                foreach (var rb in remoteActive)
                {
                    if (!_bookmarks.Any(b => b.Url.Equals(rb.Url, StringComparison.OrdinalIgnoreCase)) &&
                        !remoteDeleted.Any(d => d.Equals(rb.Url, StringComparison.OrdinalIgnoreCase)))
                    {
                        _bookmarks.Add(rb);
                        addedAny = true;
                    }
                }
                if (addedAny)
                {
                    DataManager.SaveBookmarks(_bookmarks);
                    Dispatcher.Invoke(() => UpdateBookmarkButton(_activeTab?.Url ?? ""));
                }

                // 2. Push local active bookmarks (excluding any marked deleted)
                var toPush = _bookmarks.Where(b => !remoteDeleted.Any(d => d.Equals(b.Url, StringComparison.OrdinalIgnoreCase))).ToList();
                await SyncService.PushBookmarksAsync(toPush);

                // 3. Push and Pull History
                var localHist = DataManager.LoadHistory();
                await SyncService.PushHistoryAsync(localHist);

                var remoteHist = await SyncService.PullHistoryAsync();
                bool addedHist = false;
                foreach (var rh in remoteHist)
                {
                    if (!localHist.Any(h => h.Url.Equals(rh.Url, StringComparison.OrdinalIgnoreCase)))
                    {
                        localHist.Add(rh);
                        addedHist = true;
                    }
                }
                if (addedHist)
                {
                    DataManager.SaveHistory(localHist);
                }

                // 3b. Push and Pull Passwords
                var localCreds = PasswordVaultService.LoadCredentials();
                await SyncService.PushPasswordsAsync(localCreds);

                var remoteCreds = await SyncService.PullPasswordsAsync();
                bool addedCreds = false;
                foreach (var rc in remoteCreds)
                {
                    if (!localCreds.Any(c => c.WebsiteUrl.Equals(rc.WebsiteUrl, StringComparison.OrdinalIgnoreCase) && c.Username.Equals(rc.Username, StringComparison.OrdinalIgnoreCase)))
                    {
                        localCreds.Add(rc);
                        addedCreds = true;
                    }
                }
                if (addedCreds)
                {
                    PasswordVaultService.SaveCredentials(localCreds);
                }

                // 4. Update heartbeat & open tabs list
                var activeUrls = _tabs.Select(t => t.Url).Where(u => !string.IsNullOrWhiteSpace(u) && u != "about:blank").ToList();
                await SyncService.UpdateDeviceHeartbeatAsync(activeUrls);

                SyncService.NotifySyncDone();
            }
            catch { }
        }

        #endregion

        #endregion
    }
}
