using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace RuhiBrowser.Services
{
    public static class ThemeManager
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static bool IsDarkTheme(string? theme = null)
        {
            if (string.IsNullOrWhiteSpace(theme))
            {
                var settings = DataManager.LoadSettings();
                theme = settings.Theme;
            }

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

        public static bool IsWindowsInLightMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var val = key?.GetValue("AppsUseLightTheme");
                if (val is int intVal && intVal == 1)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public static void ApplyNativeWindowTheme(Window window, bool isDark)
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    int darkMode = isDark ? 1 : 0;
                    DwmSetWindowAttribute(hwnd, 20, ref darkMode, sizeof(int));
                    DwmSetWindowAttribute(hwnd, 19, ref darkMode, sizeof(int));
                }
            }
            catch { }
        }

        public static void ApplyThemeToWindow(Window window, string? theme = null)
        {
            bool isDark = IsDarkTheme(theme);

            window.Resources["WindowBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F1F5F9"));
            window.Resources["HeaderFooterBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#28292C" : "#FFFFFF"));
            window.Resources["BorderBrushColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#CBD5E1"));
            window.Resources["CardBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#242424" : "#FFFFFF"));
            window.Resources["TextPrimaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            window.Resources["TextSecondaryBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#64748B"));
            window.Resources["SectionHeadingBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#38BDF8" : "#0284C7"));
            window.Resources["InputBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#181818" : "#F8FAFC"));
            window.Resources["InputBorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#3C4043" : "#CBD5E1"));
            window.Resources["InputTextBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            window.Resources["SecondaryBtnBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));
            window.Resources["SecondaryBtnFgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#FFFFFF" : "#0F172A"));
            window.Resources["CloseBtnBgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#383838" : "#E2E8F0"));
            window.Resources["CloseBtnFgBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#94A3B8" : "#475569"));

            window.Background = (SolidColorBrush)window.Resources["WindowBgBrush"];
            window.Foreground = (SolidColorBrush)window.Resources["TextPrimaryBrush"];

            ApplyNativeWindowTheme(window, isDark);
        }
    }
}
