using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using RuhiBrowser.Models;

namespace RuhiBrowser.Services
{
    public static class ExtensionManagerService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        static ExtensionManagerService()
        {
            try
            {
                HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 RuhiBrowser/1.0");
            }
            catch { }
        }

        public static string GetExtensionsDirectory()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RuhiBrowser",
                "Extensions"
            );
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static bool IsExtensionZip(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;
                using var fs = File.OpenRead(filePath);
                using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
                return archive.Entries.Any(e =>
                    e.Name.Equals("manifest.json", StringComparison.OrdinalIgnoreCase) ||
                    e.FullName.Equals("manifest.json", StringComparison.OrdinalIgnoreCase) ||
                    e.FullName.EndsWith("/manifest.json", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        public static void ExtractCrxOrZip(string filePath, string destinationDir)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            int zipOffset = -1;

            // Search for zip magic signature PK (0x50, 0x4B, 0x03, 0x04)
            for (int i = 0; i <= fileBytes.Length - 4; i++)
            {
                if (fileBytes[i] == 0x50 && fileBytes[i + 1] == 0x4B && fileBytes[i + 2] == 0x03 && fileBytes[i + 3] == 0x04)
                {
                    zipOffset = i;
                    break;
                }
            }

            if (zipOffset < 0)
            {
                throw new InvalidDataException("The downloaded package is not a valid CRX or ZIP extension archive (Google may have removed or restricted this extension).");
            }

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            using var ms = new MemoryStream(fileBytes, zipOffset, fileBytes.Length - zipOffset);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            archive.ExtractToDirectory(destinationDir, overwriteFiles: true);
        }

        public static string FindManifestFolder(string rootDir)
        {
            if (File.Exists(Path.Combine(rootDir, "manifest.json")))
            {
                return rootDir;
            }
            if (Directory.Exists(rootDir))
            {
                try
                {
                    var files = Directory.GetFiles(rootDir, "manifest.json", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return Path.GetDirectoryName(files[0]) ?? rootDir;
                    }
                }
                catch { }
            }
            return rootDir;
        }

        public static (string Name, string Version, string Description, string? IconPath, string? NewTabUrl, string? HomepageUrl) ReadManifest(string folderPath)
        {
            string manifestFile = Path.Combine(folderPath, "manifest.json");
            if (!File.Exists(manifestFile))
            {
                return (Path.GetFileName(folderPath), "1.0", "Chrome extension", null, null, null);
            }

            try
            {
                var json = File.ReadAllText(manifestFile);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string name = root.TryGetProperty("name", out var np) ? np.GetString() ?? "" : Path.GetFileName(folderPath);
                string version = root.TryGetProperty("version", out var vp) ? vp.GetString() ?? "1.0" : "1.0";
                string description = root.TryGetProperty("description", out var dp) ? dp.GetString() ?? "" : "";

                string? iconPath = null;
                if (root.TryGetProperty("icons", out var iconsProp) && iconsProp.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in iconsProp.EnumerateObject().OrderByDescending(p => p.Name))
                    {
                        var rel = prop.Value.GetString();
                        if (!string.IsNullOrEmpty(rel))
                        {
                            var full = Path.Combine(folderPath, rel.Replace('/', Path.DirectorySeparatorChar));
                            if (File.Exists(full))
                            {
                                iconPath = full;
                                break;
                            }
                        }
                    }
                }

                string? newTabUrl = null;
                if (root.TryGetProperty("chrome_url_overrides", out var urlOverrides) && urlOverrides.ValueKind == JsonValueKind.Object)
                {
                    if (urlOverrides.TryGetProperty("newtab", out var newtabProp) && newtabProp.ValueKind == JsonValueKind.String)
                    {
                        newTabUrl = newtabProp.GetString();
                    }
                }

                string? homepageUrl = null;
                if (root.TryGetProperty("chrome_settings_overrides", out var settingsOverrides) && settingsOverrides.ValueKind == JsonValueKind.Object)
                {
                    if (settingsOverrides.TryGetProperty("homepage", out var homeProp) && homeProp.ValueKind == JsonValueKind.String)
                    {
                        homepageUrl = homeProp.GetString();
                    }
                    else if (settingsOverrides.TryGetProperty("startup_pages", out var spProp) && spProp.ValueKind == JsonValueKind.Array && spProp.GetArrayLength() > 0)
                    {
                        homepageUrl = spProp[0].GetString();
                    }
                }
                if (string.IsNullOrEmpty(homepageUrl) && root.TryGetProperty("homepage_url", out var homeUrlProp) && homeUrlProp.ValueKind == JsonValueKind.String)
                {
                    homepageUrl = homeUrlProp.GetString();
                }

                return (name, version, description, iconPath, newTabUrl, homepageUrl);
            }
            catch
            {
                return (Path.GetFileName(folderPath), "1.0", "Chrome extension", null, null, null);
            }
        }

        public static async Task<CoreWebView2BrowserExtension?> InstallExtensionFromFolderAsync(string folderPath, CoreWebView2Profile profile)
        {
            var manifestFolder = FindManifestFolder(folderPath);
            if (!File.Exists(Path.Combine(manifestFolder, "manifest.json")))
            {
                throw new FileNotFoundException("manifest.json not found in the selected folder.");
            }

            var ext = await profile.AddBrowserExtensionAsync(manifestFolder);
            if (ext != null)
            {
                ApplyExtensionOverridesIfAny(manifestFolder, ext);
            }
            return ext;
        }

        public static async Task<CoreWebView2BrowserExtension?> InstallExtensionFromArchiveAsync(string archiveFilePath, CoreWebView2Profile profile)
        {
            string extName = Path.GetFileNameWithoutExtension(archiveFilePath);
            string safeName = string.Join("_", (extName ?? "Extension").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrEmpty(safeName)) safeName = "Extension_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            string targetDir = Path.Combine(GetExtensionsDirectory(), safeName);

            if (Directory.Exists(targetDir))
            {
                targetDir = Path.Combine(GetExtensionsDirectory(), $"{safeName}_{Guid.NewGuid().ToString("N").Substring(0, 6)}");
            }

            ExtractCrxOrZip(archiveFilePath, targetDir);
            var manifestFolder = FindManifestFolder(targetDir);

            var ext = await profile.AddBrowserExtensionAsync(manifestFolder);
            if (ext != null)
            {
                ApplyExtensionOverridesIfAny(manifestFolder, ext);
            }
            return ext;
        }

        public static async Task<CoreWebView2BrowserExtension?> DownloadAndInstallZipExtensionAsync(string zipUrl, string extensionName, CoreWebView2Profile profile)
        {
            string safeName = string.Join("_", (extensionName ?? "Extension").Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrEmpty(safeName)) safeName = "Extension_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            string targetDir = Path.Combine(GetExtensionsDirectory(), safeName);
            if (Directory.Exists(targetDir))
            {
                try { Directory.Delete(targetDir, true); } 
                catch 
                {
                    targetDir = Path.Combine(GetExtensionsDirectory(), $"{safeName}_{Guid.NewGuid().ToString("N").Substring(0, 6)}");
                }
            }
            Directory.CreateDirectory(targetDir);

            string tempZip = Path.GetTempFileName();
            try
            {
                var bytes = await HttpClient.GetByteArrayAsync(zipUrl);
                await File.WriteAllBytesAsync(tempZip, bytes);

                ExtractCrxOrZip(tempZip, targetDir);
                var manifestFolder = FindManifestFolder(targetDir);

                var ext = await profile.AddBrowserExtensionAsync(manifestFolder);
                if (ext != null)
                {
                    ApplyExtensionOverridesIfAny(manifestFolder, ext);
                }
                return ext;
            }
            finally
            {
                if (File.Exists(tempZip))
                {
                    try { File.Delete(tempZip); } catch { }
                }
            }
        }

        private static void ApplyExtensionOverridesIfAny(string manifestFolder, CoreWebView2BrowserExtension ext)
        {
            try
            {
                var meta = ReadManifest(manifestFolder);
                string? targetOverride = !string.IsNullOrWhiteSpace(meta.NewTabUrl) ? meta.NewTabUrl : meta.HomepageUrl;
                if (!string.IsNullOrWhiteSpace(targetOverride))
                {
                    var settings = DataManager.LoadSettings();
                    string finalUrl;
                    if (targetOverride.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        targetOverride.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        finalUrl = targetOverride;
                    }
                    else
                    {
                        string rel = targetOverride.TrimStart('/', '\\').Replace('\\', '/');
                        finalUrl = $"chrome-extension://{ext.Id}/{rel}";
                    }

                    settings.ActiveExtensionNewTabUrl = finalUrl;
                    settings.ActiveExtensionNewTabName = ext.Name;
                    settings.HomepageMode = "Extension";
                    DataManager.SaveSettings(settings);
                }
            }
            catch { }
        }

        public static async Task EnsureAllExtensionsLoadedAsync(CoreWebView2Profile profile)
        {
            if (profile == null) return;
            string extensionsDir = GetExtensionsDirectory();
            if (!Directory.Exists(extensionsDir)) return;

            try
            {
                var existingCore = await profile.GetBrowserExtensionsAsync();
                var subdirs = Directory.GetDirectories(extensionsDir);
                foreach (var dir in subdirs)
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith(".") || dirName.Contains("_trash_")) continue;

                    var manifestFolder = FindManifestFolder(dir);
                    if (File.Exists(Path.Combine(manifestFolder, "manifest.json")))
                    {
                        var meta = ReadManifest(manifestFolder);
                        bool alreadyInProfile = existingCore.Any(c =>
                            string.Equals(c.Name, meta.Name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(c.Name, Path.GetFileName(dir), StringComparison.OrdinalIgnoreCase));

                        if (!alreadyInProfile)
                        {
                            try
                            {
                                await profile.AddBrowserExtensionAsync(manifestFolder);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        public static async Task<(string? Url, string? Name)> GetActiveNewTabOverrideAsync(CoreWebView2Profile? profile)
        {
            if (profile == null) return (null, null);

            try
            {
                var exts = await profile.GetBrowserExtensionsAsync();
                string extensionsDir = GetExtensionsDirectory();

                foreach (var ext in exts)
                {
                    if (!ext.IsEnabled) continue;

                    string? manifestFolder = null;
                    if (Directory.Exists(extensionsDir))
                    {
                        foreach (var dir in Directory.GetDirectories(extensionsDir))
                        {
                            var mf = FindManifestFolder(dir);
                            if (File.Exists(Path.Combine(mf, "manifest.json")))
                            {
                                var meta = ReadManifest(mf);
                                if (string.Equals(meta.Name, ext.Name, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(Path.GetFileName(dir), ext.Name, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(Path.GetFileName(dir), ext.Id, StringComparison.OrdinalIgnoreCase))
                                {
                                    manifestFolder = mf;
                                    break;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(manifestFolder))
                    {
                        var meta = ReadManifest(manifestFolder);
                        string? targetOverride = !string.IsNullOrWhiteSpace(meta.NewTabUrl) ? meta.NewTabUrl : meta.HomepageUrl;
                        if (!string.IsNullOrWhiteSpace(targetOverride))
                        {
                            if (targetOverride.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                targetOverride.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                            {
                                return (targetOverride, ext.Name);
                            }
                            string rel = targetOverride.TrimStart('/', '\\').Replace('\\', '/');
                            return ($"chrome-extension://{ext.Id}/{rel}", ext.Name);
                        }
                    }
                }
            }
            catch { }

            return (null, null);
        }

        public static string? ExtractExtensionIdFromUrl(string urlOrId)
        {
            if (string.IsNullOrWhiteSpace(urlOrId)) return null;
            urlOrId = urlOrId.Trim();

            // Match 32 lowercase alpha characters in Chrome extension IDs
            var match = Regex.Match(urlOrId, @"([a-z]{32})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.ToLowerInvariant();
            }
            return null;
        }

        public static async Task<CoreWebView2BrowserExtension?> InstallFromChromeWebStoreAsync(string urlOrId, CoreWebView2Profile profile)
        {
            string? extId = ExtractExtensionIdFromUrl(urlOrId);
            if (string.IsNullOrEmpty(extId))
            {
                throw new ArgumentException("Could not find a valid 32-character Chrome extension ID in the provided link.");
            }

            // 1. Primary: Official working Google Chrome Update 2 endpoint (returns HTTP 200 OK)
            string googleUrl = $"https://clients2.google.com/service/update2/crx?response=redirect&os=win&arch=x86-64&prod=chromecrx&prodchannel=unknown&prodversion=9999.0.9999.0&acceptformat=crx2,crx3&x=id%3D{extId}%26uc";

            try
            {
                return await DownloadAndInstallZipExtensionAsync(googleUrl, extId, profile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Google CRX download failed for {extId}: {ex.Message}. Trying Edge Add-ons fallback...");
                // 2. Fallback: Microsoft Edge Add-ons Store endpoint
                string edgeUrl = $"https://edge.microsoft.com/extensionwebstorebase/v1/crx?response=redirect&x=id%3D{extId}%26installsource%3Dondemand%26uc";
                return await DownloadAndInstallZipExtensionAsync(edgeUrl, extId, profile);
            }
        }

        public static async Task<CoreWebView2BrowserExtension?> InstallOfficialRuhiAddonAsync(string id, string name, CoreWebView2Profile profile)
        {
            string safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
            if (string.IsNullOrEmpty(safeName)) safeName = "RUHI_Official_Addon";

            string targetDir = Path.Combine(GetExtensionsDirectory(), safeName);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            string desc = id.Contains("shield")
                ? "Official RUHI Browser optimizer. Turbocharges ad filtering, stops telemetry, and secures DNS requests."
                : "Deeply integrated AI sidebar companion for page summarization, smart translations, and coding explanations.";

            var manifestObj = new
            {
                manifest_version = 3,
                name = name,
                version = "1.0.0",
                description = desc
            };

            string manifestPath = Path.Combine(targetDir, "manifest.json");
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifestObj, new JsonSerializerOptions { WriteIndented = true }));

            return await profile.AddBrowserExtensionAsync(targetDir);
        }
    }
}
