using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using RuhiBrowser.Models;

namespace RuhiBrowser.Services
{
    public class SyncDevice
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "Windows";
        public DateTime LastActive { get; set; } = DateTime.Now;
        public List<string> ActiveTabs { get; set; } = new();
    }

    public class SentTab
    {
        public string Id { get; set; } = string.Empty;
        public string SenderDeviceId { get; set; } = string.Empty;
        public string TargetDeviceId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
    }

    public static class SyncService
    {
        public const string SupabaseUrl = "https://voyzembamrvyrkrdnbsk.supabase.co";
        public const string AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZveXplbWJhbXJ2eXJrcmRuYnNrIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTM2MDgwNDIsImV4cCI6MjEwOTE4NDA0Mn0.4rmbKyD3HiV70ndDve-eUJg3WYkV7IDDdqOJRrPamXU";

        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        public static event Action<SentTab>? TabReceived;
        public static event Action? SyncCompleted;

        private static BrowserSettings _settings = new();
        private static List<string>? _lastActiveUrls = null;

        public static bool IsLoggedIn => _settings.SyncEnabled && (!string.IsNullOrWhiteSpace(_settings.SyncAccessToken) || !string.IsNullOrWhiteSpace(_settings.SyncRefreshToken));
        public static string CurrentEmail => _settings.SyncEmail;
        public static string CurrentDisplayName => _settings.SyncDisplayName;
        public static string CurrentDeviceId => _settings.SyncDeviceId;

        public static void SetDisplayName(string name)
        {
            _settings.SyncDisplayName = name ?? string.Empty;
            DataManager.SaveSettings(_settings);
        }

        public static async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactory)
        {
            var req = requestFactory();
            var resp = await _http.SendAsync(req);
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized && !string.IsNullOrWhiteSpace(_settings.SyncRefreshToken))
            {
                bool refreshed = await RefreshSessionAsync();
                if (refreshed)
                {
                    var retryReq = requestFactory();
                    resp = await _http.SendAsync(retryReq);
                }
            }
            return resp;
        }

        public static async Task<bool> RefreshSessionAsync()
        {
            if (string.IsNullOrWhiteSpace(_settings.SyncRefreshToken)) return false;
            try
            {
                var body = JsonSerializer.Serialize(new { refresh_token = _settings.SyncRefreshToken });
                var req = new HttpRequestMessage(HttpMethod.Post, SupabaseUrl + "/auth/v1/token?grant_type=refresh_token")
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                req.Headers.Add("apikey", AnonKey);

                var resp = await _http.SendAsync(req);
                if (!resp.IsSuccessStatusCode) return false;

                var json = await resp.Content.ReadAsStringAsync();
                var node = JsonNode.Parse(json);
                string newAccessToken = node?["access_token"]?.ToString() ?? "";
                string newRefreshToken = node?["refresh_token"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    _settings.SyncAccessToken = newAccessToken;
                    if (!string.IsNullOrEmpty(newRefreshToken))
                    {
                        _settings.SyncRefreshToken = newRefreshToken;
                    }

                    var meta = node?["user"]?["user_metadata"];
                    if (meta != null && meta["display_name"] != null)
                    {
                        string dName = meta["display_name"]?.ToString() ?? "";
                        if (!string.IsNullOrWhiteSpace(dName)) _settings.SyncDisplayName = dName;
                    }

                    _settings.LastSyncTime = DateTime.Now;
                    DataManager.SaveSettings(_settings);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> UpdateDisplayNameAsync(string newName)
        {
            _settings.SyncDisplayName = newName ?? string.Empty;
            DataManager.SaveSettings(_settings);

            if (!IsLoggedIn) return true;

            try
            {
                var body = JsonSerializer.Serialize(new
                {
                    data = new { display_name = newName }
                });
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Put, "/auth/v1/user", body));
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task RefreshUserProfileAsync()
        {
            if (!IsLoggedIn) return;
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, "/auth/v1/user"));
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var node = JsonNode.Parse(json);
                    var meta = node?["user_metadata"];
                    if (meta != null && meta["display_name"] != null)
                    {
                        string dName = meta["display_name"]?.ToString() ?? "";
                        if (!string.IsNullOrWhiteSpace(dName))
                        {
                            _settings.SyncDisplayName = dName;
                            DataManager.SaveSettings(_settings);
                        }
                    }
                }
            }
            catch { }
        }

        public static void Initialize(BrowserSettings settings)
        {
            _settings = settings;
            if (string.IsNullOrWhiteSpace(_settings.SyncDeviceId))
            {
                _settings.SyncDeviceId = Guid.NewGuid().ToString();
                _settings.SyncDeviceName = Environment.MachineName + " (Windows)";
                DataManager.SaveSettings(_settings);
            }
            if (IsLoggedIn && !string.IsNullOrWhiteSpace(_settings.SyncRefreshToken))
            {
                _ = RefreshSessionAsync();
            }
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string path, string? jsonBody = null)
        {
            var req = new HttpRequestMessage(method, SupabaseUrl + path);
            req.Headers.Add("apikey", AnonKey);
            if (!string.IsNullOrWhiteSpace(_settings.SyncAccessToken))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SyncAccessToken);
            }
            if (jsonBody != null)
            {
                req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }
            return req;
        }

        public static async Task<(bool success, string message)> SignUpAsync(string email, string password, string displayName = "")
        {
            try
            {
                var payload = new
                {
                    email = email,
                    password = password,
                    data = new { display_name = displayName }
                };
                var body = JsonSerializer.Serialize(payload);
                var req = CreateRequest(HttpMethod.Post, "/auth/v1/signup", body);
                var resp = await _http.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    var node = JsonNode.Parse(json);
                    string err = node?["msg"]?.ToString() ?? node?["error_description"]?.ToString() ?? "Registration failed.";
                    return (false, err);
                }

                var resNode = JsonNode.Parse(json);
                string token = resNode?["access_token"]?.ToString() ?? string.Empty;
                string uid = resNode?["user"]?["id"]?.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(uid))
                {
                    _settings.SyncEnabled = true;
                    _settings.SyncEmail = email;
                    if (!string.IsNullOrWhiteSpace(displayName)) _settings.SyncDisplayName = displayName;
                    _settings.SyncUserId = uid;
                    _settings.SyncAccessToken = token;
                    _settings.SyncRefreshToken = resNode?["refresh_token"]?.ToString() ?? string.Empty;
                    _settings.LastSyncTime = DateTime.Now;
                    DataManager.SaveSettings(_settings);
                    return (true, "Account created & logged in successfully!");
                }

                return await LoginAsync(email, password);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool success, string message)> LoginAsync(string email, string password)
        {
            try
            {
                var body = JsonSerializer.Serialize(new { email, password });
                var req = CreateRequest(HttpMethod.Post, "/auth/v1/token?grant_type=password", body);
                var resp = await _http.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    var node = JsonNode.Parse(json);
                    string err = node?["error_description"]?.ToString() ?? node?["msg"]?.ToString() ?? "Login failed. Check email/password.";
                    return (false, err);
                }

                var resNode = JsonNode.Parse(json);
                _settings.SyncEnabled = true;
                _settings.SyncEmail = email;
                _settings.SyncUserId = resNode?["user"]?["id"]?.ToString() ?? string.Empty;
                _settings.SyncAccessToken = resNode?["access_token"]?.ToString() ?? string.Empty;
                _settings.SyncRefreshToken = resNode?["refresh_token"]?.ToString() ?? string.Empty;

                var meta = resNode?["user"]?["user_metadata"];
                if (meta != null && meta["display_name"] != null)
                {
                    string dName = meta["display_name"]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(dName)) _settings.SyncDisplayName = dName;
                }

                _settings.LastSyncTime = DateTime.Now;
                DataManager.SaveSettings(_settings);

                return (true, "Login successful!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static void Logout()
        {
            _settings.SyncEnabled = false;
            _settings.SyncEmail = string.Empty;
            _settings.SyncDisplayName = string.Empty;
            _settings.SyncUserId = string.Empty;
            _settings.SyncAccessToken = string.Empty;
            _settings.SyncRefreshToken = string.Empty;
            DataManager.SaveSettings(_settings);
        }

        #region Bookmarks Sync

        public static async Task<bool> PushBookmarksAsync(List<Bookmark> localBookmarks)
        {
            if (!IsLoggedIn || localBookmarks == null || localBookmarks.Count == 0) return false;
            try
            {
                var list = localBookmarks.Select(b => new
                {
                    id = b.Id,
                    user_id = _settings.SyncUserId,
                    url = b.Url,
                    title = b.Title,
                    folder = "",
                    updated_at = DateTime.UtcNow.ToString("o"),
                    is_deleted = false
                }).ToList();

                var body = JsonSerializer.Serialize(list);
                var resp = await SendRequestAsync(() =>
                {
                    var req = CreateRequest(HttpMethod.Post, "/rest/v1/ruhi_sync_bookmarks?on_conflict=id", body);
                    req.Headers.Add("Prefer", "resolution=merge-duplicates");
                    return req;
                });
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<(List<Bookmark> active, List<string> deletedUrls)> PullBookmarksAsync()
        {
            if (!IsLoggedIn) return (new List<Bookmark>(), new List<string>());
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, "/rest/v1/ruhi_sync_bookmarks?select=*&order=created_at.asc"));
                if (!resp.IsSuccessStatusCode) return (new List<Bookmark>(), new List<string>());

                var json = await resp.Content.ReadAsStringAsync();
                var items = JsonNode.Parse(json)?.AsArray();
                if (items == null) return (new List<Bookmark>(), new List<string>());

                var active = new List<Bookmark>();
                var deleted = new List<string>();
                foreach (var item in items)
                {
                    if (item == null) continue;
                    bool isDel = item["is_deleted"]?.GetValue<bool>() ?? false;
                    string u = item["url"]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(u)) continue;

                    if (isDel)
                    {
                        deleted.Add(u);
                    }
                    else
                    {
                        active.Add(new Bookmark
                        {
                            Id = item["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                            Title = item["title"]?.ToString() ?? string.Empty,
                            Url = u,
                            CreatedAt = DateTime.TryParse(item["created_at"]?.ToString(), out var dt) ? dt : DateTime.Now
                        });
                    }
                }
                return (active, deleted);
            }
            catch
            {
                return (new List<Bookmark>(), new List<string>());
            }
        }

        public static async Task<bool> DeleteBookmarkAsync(string url)
        {
            if (!IsLoggedIn || string.IsNullOrWhiteSpace(url)) return false;
            try
            {
                var body = JsonSerializer.Serialize(new { is_deleted = true, updated_at = DateTime.UtcNow.ToString("o") });
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Patch, $"/rest/v1/ruhi_sync_bookmarks?url=eq.{Uri.EscapeDataString(url)}", body));
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public static async Task<bool> ClearAllBookmarksAsync()
        {
            if (!IsLoggedIn) return false;
            try
            {
                var body = JsonSerializer.Serialize(new { is_deleted = true, updated_at = DateTime.UtcNow.ToString("o") });
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Patch, $"/rest/v1/ruhi_sync_bookmarks?user_id=eq.{_settings.SyncUserId}", body));
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        #endregion

        #region History Sync

        public static async Task<bool> PushHistoryAsync(List<HistoryItem> localHistory)
        {
            if (!IsLoggedIn || localHistory == null || localHistory.Count == 0) return false;
            try
            {
                var clearedAt = _settings.LastHistoryClearedAt;
                var deletedUrls = new HashSet<string>(_settings.DeletedHistoryUrls?.Select(u => u.ToLowerInvariant()) ?? Enumerable.Empty<string>());

                var filtered = localHistory
                    .Where(h => !string.IsNullOrWhiteSpace(h.Url))
                    .Where(h => !deletedUrls.Contains(h.Url.ToLowerInvariant()))
                    .Where(h => !clearedAt.HasValue || h.VisitedAt.ToUniversalTime() > clearedAt.Value.ToUniversalTime())
                    .OrderByDescending(h => h.VisitedAt)
                    .Take(50)
                    .Select(h => new
                    {
                        id = h.Id,
                        user_id = _settings.SyncUserId,
                        url = h.Url,
                        title = h.Title,
                        visited_at = h.VisitedAt.ToUniversalTime().ToString("o")
                    }).ToList();

                if (filtered.Count == 0) return true;

                var body = JsonSerializer.Serialize(filtered);
                var resp = await SendRequestAsync(() =>
                {
                    var req = CreateRequest(HttpMethod.Post, "/rest/v1/ruhi_sync_history?on_conflict=id", body);
                    req.Headers.Add("Prefer", "resolution=merge-duplicates");
                    return req;
                });
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> DeleteHistoryItemAsync(string url, string? id = null)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                _settings.DeletedHistoryUrls ??= new List<string>();
                var lower = url.ToLowerInvariant();
                if (!_settings.DeletedHistoryUrls.Contains(lower))
                {
                    _settings.DeletedHistoryUrls.Add(lower);
                    if (_settings.DeletedHistoryUrls.Count > 200)
                    {
                        _settings.DeletedHistoryUrls.RemoveAt(0);
                    }
                    DataManager.SaveSettings(_settings);
                }
            }

            _ = UpdateDeviceHeartbeatAsync(null);

            if (!IsLoggedIn) return false;
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    _ = await SendRequestAsync(() => CreateRequest(HttpMethod.Delete, $"/rest/v1/ruhi_sync_history?user_id=eq.{_settings.SyncUserId}&id=eq.{Uri.EscapeDataString(id)}"));
                }
                if (!string.IsNullOrWhiteSpace(url))
                {
                    var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Delete, $"/rest/v1/ruhi_sync_history?user_id=eq.{_settings.SyncUserId}&url=eq.{Uri.EscapeDataString(url)}"));
                    return resp.IsSuccessStatusCode;
                }
                return true;
            }
            catch { return false; }
        }

        public static async Task<bool> ClearAllHistoryAsync()
        {
            _settings.LastHistoryClearedAt = DateTime.UtcNow;
            _settings.DeletedHistoryUrls?.Clear();
            DataManager.SaveSettings(_settings);
            _ = UpdateDeviceHeartbeatAsync(null);

            if (!IsLoggedIn) return false;
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Delete, $"/rest/v1/ruhi_sync_history?user_id=eq.{_settings.SyncUserId}"));
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public static async Task<List<HistoryItem>> PullHistoryAsync()
        {
            if (!IsLoggedIn) return new List<HistoryItem>();
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, "/rest/v1/ruhi_sync_history?select=*&order=visited_at.desc&limit=100"));
                if (!resp.IsSuccessStatusCode) return new List<HistoryItem>();

                var json = await resp.Content.ReadAsStringAsync();
                var items = JsonNode.Parse(json)?.AsArray();
                if (items == null) return new List<HistoryItem>();

                var result = new List<HistoryItem>();
                var clearedAt = _settings.LastHistoryClearedAt;
                var deletedUrls = new HashSet<string>(_settings.DeletedHistoryUrls?.Select(u => u.ToLowerInvariant()) ?? Enumerable.Empty<string>());

                foreach (var item in items)
                {
                    if (item == null) continue;
                    var url = item["url"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(url)) continue;

                    // If user deleted this URL, DO NOT pull it back
                    if (deletedUrls.Contains(url.ToLowerInvariant())) continue;

                    var visitedStr = item["visited_at"]?.ToString();
                    if (clearedAt.HasValue && !string.IsNullOrWhiteSpace(visitedStr))
                    {
                        if (DateTime.TryParse(visitedStr, out var vDt))
                        {
                            if (vDt.ToUniversalTime() <= clearedAt.Value.ToUniversalTime())
                            {
                                // Item was visited before user cleared all history -> SKIP
                                continue;
                            }
                        }
                    }

                    result.Add(new HistoryItem
                    {
                        Id = item["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Title = item["title"]?.ToString() ?? string.Empty,
                        Url = url,
                        VisitedAt = DateTime.TryParse(visitedStr, out var dt) ? dt : DateTime.Now
                    });
                }
                return result;
            }
            catch
            {
                return new List<HistoryItem>();
            }
        }

        #endregion

        #region Passwords Sync

        public static async Task<bool> PushPasswordsAsync(List<SavedCredential> credentials)
        {
            if (!IsLoggedIn || credentials == null || credentials.Count == 0) return false;
            try
            {
                var list = credentials.Select(c => new
                {
                    id = c.Id,
                    user_id = _settings.SyncUserId,
                    website_url = c.WebsiteUrl,
                    website_title = c.WebsiteTitle,
                    username = c.Username,
                    encrypted_password = c.EncryptedPassword,
                    updated_at = DateTime.UtcNow.ToString("o")
                }).ToList();

                var body = JsonSerializer.Serialize(list);
                var resp = await SendRequestAsync(() =>
                {
                    var req = CreateRequest(HttpMethod.Post, "/rest/v1/ruhi_sync_passwords?on_conflict=id", body);
                    req.Headers.Add("Prefer", "resolution=merge-duplicates");
                    return req;
                });
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<SavedCredential>> PullPasswordsAsync()
        {
            if (!IsLoggedIn) return new List<SavedCredential>();
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, "/rest/v1/ruhi_sync_passwords?select=*&order=created_at.desc"));
                if (!resp.IsSuccessStatusCode) return new List<SavedCredential>();

                var json = await resp.Content.ReadAsStringAsync();
                var items = JsonNode.Parse(json)?.AsArray();
                if (items == null) return new List<SavedCredential>();

                var result = new List<SavedCredential>();
                foreach (var item in items)
                {
                    if (item == null) continue;
                    result.Add(new SavedCredential
                    {
                        Id = item["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                        WebsiteUrl = item["website_url"]?.ToString() ?? string.Empty,
                        WebsiteTitle = item["website_title"]?.ToString() ?? string.Empty,
                        Username = item["username"]?.ToString() ?? string.Empty,
                        EncryptedPassword = item["encrypted_password"]?.ToString() ?? string.Empty
                    });
                }
                return result;
            }
            catch
            {
                return new List<SavedCredential>();
            }
        }

        #endregion

        #region Devices & Live Tab Sync

        public static async Task UpdateDeviceHeartbeatAsync(List<string>? activeTabUrls)
        {
            if (!IsLoggedIn) return;
            try
            {
                if (activeTabUrls != null)
                {
                    _lastActiveUrls = activeTabUrls;
                }
                var tabsToSend = activeTabUrls ?? _lastActiveUrls ?? new List<string>();

                var payload = new
                {
                    device_id = _settings.SyncDeviceId,
                    user_id = _settings.SyncUserId,
                    device_name = _settings.SyncDeviceName,
                    device_type = "Windows",
                    last_active = DateTime.UtcNow.ToString("o"),
                    active_tabs = new
                    {
                        tabs = tabsToSend,
                        cleared_at = _settings.LastHistoryClearedAt?.ToUniversalTime().ToString("o"),
                        deleted_urls = _settings.DeletedHistoryUrls ?? new List<string>()
                    }
                };

                var body = JsonSerializer.Serialize(payload);
                await SendRequestAsync(() =>
                {
                    var req = CreateRequest(HttpMethod.Post, "/rest/v1/ruhi_sync_devices?on_conflict=device_id", body);
                    req.Headers.Add("Prefer", "resolution=merge-duplicates");
                    return req;
                });
            }
            catch { }
        }

        public static async Task<bool> CheckRemoteHistoryDeletionsAsync()
        {
            if (!IsLoggedIn) return false;
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, "/rest/v1/ruhi_sync_devices?select=*"));
                if (!resp.IsSuccessStatusCode) return false;

                var json = await resp.Content.ReadAsStringAsync();
                var arr = JsonNode.Parse(json)?.AsArray();
                if (arr == null) return false;

                bool modified = false;
                var localHist = DataManager.LoadHistory();
                _settings.DeletedHistoryUrls ??= new List<string>();

                foreach (var device in arr)
                {
                    if (device == null) continue;
                    var devId = device["device_id"]?.ToString();
                    if (devId == _settings.SyncDeviceId) continue; // Skip self

                    var activeTabsNode = device["active_tabs"];
                    if (activeTabsNode is JsonObject obj)
                    {
                        var clearedAtStr = obj["cleared_at"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(clearedAtStr) && DateTime.TryParse(clearedAtStr, out var cDt))
                        {
                            var utcCDt = cDt.ToUniversalTime();
                            if (!_settings.LastHistoryClearedAt.HasValue || utcCDt > _settings.LastHistoryClearedAt.Value.ToUniversalTime())
                            {
                                _settings.LastHistoryClearedAt = utcCDt;
                                int removed = localHist.RemoveAll(h => h.VisitedAt.ToUniversalTime() <= utcCDt);
                                if (removed > 0) modified = true;
                            }
                        }

                        if (obj["deleted_urls"] is JsonArray delUrlsArr)
                        {
                            foreach (var uNode in delUrlsArr)
                            {
                                var u = uNode?.ToString()?.Trim().ToLowerInvariant();
                                if (!string.IsNullOrWhiteSpace(u))
                                {
                                    if (!_settings.DeletedHistoryUrls.Contains(u))
                                    {
                                        _settings.DeletedHistoryUrls.Add(u);
                                    }
                                    int removed = localHist.RemoveAll(h => h.Url.Trim().Equals(u, StringComparison.OrdinalIgnoreCase));
                                    if (removed > 0) modified = true;
                                }
                            }
                        }
                    }
                }

                if (modified)
                {
                    DataManager.SaveHistory(localHist);
                    DataManager.SaveSettings(_settings);
                }
                return modified;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<SyncDevice>> GetDevicesAsync()
        {
            if (!IsLoggedIn) return new List<SyncDevice>();
            try
            {
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, "/rest/v1/ruhi_sync_devices?select=*&order=last_active.desc"));
                if (!resp.IsSuccessStatusCode) return new List<SyncDevice>();

                var json = await resp.Content.ReadAsStringAsync();
                var arr = JsonNode.Parse(json)?.AsArray();
                if (arr == null) return new List<SyncDevice>();

                var list = new List<SyncDevice>();
                foreach (var node in arr)
                {
                    if (node == null) continue;
                    var d = new SyncDevice
                    {
                        DeviceId = node["device_id"]?.ToString() ?? "",
                        DeviceName = node["device_name"]?.ToString() ?? "",
                        DeviceType = node["device_type"]?.ToString() ?? "Windows",
                        LastActive = DateTime.TryParse(node["last_active"]?.ToString(), out var dt) ? dt : DateTime.Now
                    };
                    if (node["active_tabs"] is JsonArray tabsArr)
                    {
                        foreach (var t in tabsArr)
                        {
                            if (t != null) d.ActiveTabs.Add(t.ToString());
                        }
                    }
                    else if (node["active_tabs"] is JsonObject tabsObj && tabsObj["tabs"] is JsonArray innerTabs)
                    {
                        foreach (var t in innerTabs)
                        {
                            if (t != null) d.ActiveTabs.Add(t.ToString());
                        }
                    }
                    list.Add(d);
                }
                return list;
            }
            catch
            {
                return new List<SyncDevice>();
            }
        }

        public static async Task<bool> SendTabToDeviceAsync(string? targetDeviceId, string url, string title)
        {
            if (!IsLoggedIn) return false;
            try
            {
                var payload = new Dictionary<string, object>
                {
                    ["user_id"] = _settings.SyncUserId,
                    ["sender_device_id"] = _settings.SyncDeviceId,
                    ["url"] = url,
                    ["title"] = title,
                    ["sent_at"] = DateTime.UtcNow.ToString("o"),
                    ["is_opened"] = false
                };
                if (!string.IsNullOrWhiteSpace(targetDeviceId) && !targetDeviceId.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    payload["target_device_id"] = targetDeviceId;
                }

                var body = JsonSerializer.Serialize(payload);
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Post, "/rest/v1/ruhi_send_tabs", body));
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task CheckIncomingTabsAsync()
        {
            if (!IsLoggedIn) return;
            try
            {
                // Find tabs targeted to this device or all devices where is_opened=false and not sent by this device
                string query = $"/rest/v1/ruhi_send_tabs?select=*&is_opened=eq.false&sender_device_id=neq.{_settings.SyncDeviceId}";
                var resp = await SendRequestAsync(() => CreateRequest(HttpMethod.Get, query));
                if (!resp.IsSuccessStatusCode) return;

                var json = await resp.Content.ReadAsStringAsync();
                var arr = JsonNode.Parse(json)?.AsArray();
                if (arr == null || arr.Count == 0) return;

                foreach (var node in arr)
                {
                    if (node == null) continue;
                    string target = node["target_device_id"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(target) && !target.Equals("null", StringComparison.OrdinalIgnoreCase) && target != _settings.SyncDeviceId)
                    {
                        continue; // Targeted for a different device
                    }

                    string tabId = node["id"]?.ToString() ?? "";
                    var sentTab = new SentTab
                    {
                        Id = tabId,
                        SenderDeviceId = node["sender_device_id"]?.ToString() ?? "",
                        TargetDeviceId = target,
                        Url = node["url"]?.ToString() ?? "",
                        Title = node["title"]?.ToString() ?? "",
                        SentAt = DateTime.TryParse(node["sent_at"]?.ToString(), out var dt) ? dt : DateTime.Now
                    };

                    // Mark as opened
                    _ = MarkTabOpenedAsync(tabId);

                    // Raise event to open in browser
                    TabReceived?.Invoke(sentTab);
                }
            }
            catch { }
        }

        private static async Task MarkTabOpenedAsync(string tabId)
        {
            try
            {
                var body = JsonSerializer.Serialize(new { is_opened = true });
                await SendRequestAsync(() => CreateRequest(HttpMethod.Patch, $"/rest/v1/ruhi_send_tabs?id=eq.{tabId}", body));
            }
            catch { }
        }

        #endregion

        public static void NotifySyncDone()
        {
            _settings.LastSyncTime = DateTime.Now;
            DataManager.SaveSettings(_settings);
            SyncCompleted?.Invoke();
        }
    }
}
