using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace RuhiBrowser.Services
{
    public static class DefaultBrowserHelper
    {
        private const string AppProgId = "RuhiBrowserHTML";
        private const string AppName = "RUHI Browser";

        public static bool IsDefaultBrowser()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice");
                var progId = key?.GetValue("ProgId")?.ToString();
                if (!string.IsNullOrEmpty(progId) && progId.StartsWith("RuhiBrowser", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public static void RegisterAsBrowser()
        {
            try
            {
                var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    exePath = Path.Combine(baseDir, "RuhiBrowser.exe");
                }

                if (!File.Exists(exePath))
                {
                    return;
                }

                // 1. Register in HKCU\Software\Clients\StartMenuInternet\RuhiBrowser
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Clients\StartMenuInternet\RuhiBrowser"))
                {
                    key.SetValue("", AppName);
                    using (var iconKey = key.CreateSubKey("DefaultIcon"))
                    {
                        iconKey.SetValue("", $"\"{exePath}\",0");
                    }
                    using (var shellKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        shellKey.SetValue("", $"\"{exePath}\"");
                    }
                    using (var capKey = key.CreateSubKey("Capabilities"))
                    {
                        capKey.SetValue("ApplicationDescription", "Fast, Secure & Private Web Browser by RUHI Application");
                        capKey.SetValue("ApplicationIcon", $"\"{exePath}\",0");
                        capKey.SetValue("ApplicationName", AppName);

                        using var faKey = capKey.CreateSubKey("FileAssociations");
                        faKey.SetValue(".htm", AppProgId);
                        faKey.SetValue(".html", AppProgId);
                        faKey.SetValue(".shtml", AppProgId);
                        faKey.SetValue(".xht", AppProgId);
                        faKey.SetValue(".xhtml", AppProgId);
                        faKey.SetValue(".pdf", AppProgId);

                        using var uaKey = capKey.CreateSubKey("URLAssociations");
                        uaKey.SetValue("http", AppProgId);
                        uaKey.SetValue("https", AppProgId);
                        uaKey.SetValue("ftp", AppProgId);

                        using var smKey = capKey.CreateSubKey("StartMenu");
                        smKey.SetValue("StartMenuInternet", "RuhiBrowser");
                    }
                }

                // 2. Register in HKCU\Software\RegisteredApplications
                using (var regAppsKey = Registry.CurrentUser.CreateSubKey(@"Software\RegisteredApplications"))
                {
                    regAppsKey.SetValue("RuhiBrowser", @"Software\Clients\StartMenuInternet\RuhiBrowser\Capabilities");
                }

                // 3. Register in HKCU\Software\Classes\RuhiBrowserHTML
                using (var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{AppProgId}"))
                {
                    progIdKey.SetValue("", "RUHI Browser HTML Document");
                    using (var iconKey = progIdKey.CreateSubKey("DefaultIcon"))
                    {
                        iconKey.SetValue("", $"\"{exePath}\",0");
                    }
                    using (var cmdKey = progIdKey.CreateSubKey(@"shell\open\command"))
                    {
                        cmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                }
            }
            catch { }
        }

        public static void OpenDefaultBrowserSettings()
        {
            RegisterAsBrowser();
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:defaultapps",
                    UseShellExecute = true
                });
            }
            catch
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "control.exe",
                        Arguments = "/name Microsoft.DefaultPrograms /page pageDefaultProgram",
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
    }
}
