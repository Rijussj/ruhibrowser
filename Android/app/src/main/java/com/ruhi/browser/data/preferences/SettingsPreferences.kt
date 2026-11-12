package com.ruhi.browser.data.preferences

import android.content.Context
import android.content.SharedPreferences
import androidx.appcompat.app.AppCompatDelegate
import com.ruhi.browser.model.Shortcut
import org.json.JSONArray
import org.json.JSONObject

class SettingsPreferences(context: Context) {

    private val prefs: SharedPreferences =
        context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)

    companion object {
        private const val PREFS_NAME = "ruhi_browser_settings"

        private const val KEY_THEME = "pref_theme"
        private const val KEY_SEARCH_ENGINE = "pref_search_engine"
        private const val KEY_HOMEPAGE = "pref_homepage"
        private const val KEY_ADBLOCK = "pref_adblock"
        private const val KEY_CUSTOM_SHORTCUTS = "pref_custom_shortcuts"

        // Phase 7: AI Assistant Keys
        private const val KEY_AI_API_KEY = "pref_ai_api_key"
        private const val KEY_AI_ENDPOINT = "pref_ai_endpoint"
        private const val KEY_AI_ENABLED = "pref_ai_enabled"
        private const val KEY_AI_CONSENT = "pref_ai_consent"
        const val DEFAULT_AI_ENDPOINT = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.3"

        // Phase 7: VPN Keys
        private const val KEY_VPN_SERVER_ADDRESS = "pref_vpn_server_address"
        private const val KEY_VPN_SERVER_PORT = "pref_vpn_server_port"
        private const val KEY_VPN_CLIENT_ADDRESS = "pref_vpn_client_address"
        private const val KEY_VPN_DNS = "pref_vpn_dns"
        private const val KEY_VPN_PUBLIC_KEY = "pref_vpn_public_key"
        private const val KEY_VPN_PRIVATE_KEY = "pref_vpn_private_key"

        // Phase 8: Cloud Sync Keys
        private const val KEY_SYNC_ENABLED = "pref_sync_enabled"
        private const val KEY_SYNC_DISPLAY_NAME = "pref_sync_display_name"
        private const val KEY_SYNC_EMAIL = "pref_sync_email"
        private const val KEY_SYNC_USER_ID = "pref_sync_user_id"
        private const val KEY_SYNC_ACCESS_TOKEN = "pref_sync_access_token"
        private const val KEY_SYNC_REFRESH_TOKEN = "pref_sync_refresh_token"
        private const val KEY_SYNC_DEVICE_ID = "pref_sync_device_id"
        private const val KEY_SYNC_DEVICE_NAME = "pref_sync_device_name"
        private const val KEY_LAST_HISTORY_CLEARED_AT = "pref_last_history_cleared_at"
        private const val KEY_DELETED_HISTORY_URLS = "pref_deleted_history_urls"

        const val THEME_SYSTEM = AppCompatDelegate.MODE_NIGHT_FOLLOW_SYSTEM
        const val THEME_LIGHT = AppCompatDelegate.MODE_NIGHT_NO
        const val THEME_DARK = AppCompatDelegate.MODE_NIGHT_YES

        const val SEARCH_ENGINE_GOOGLE = "Google"
        const val SEARCH_ENGINE_DUCKDUCKGO = "DuckDuckGo"
        const val SEARCH_ENGINE_BING = "Bing"
        const val SEARCH_ENGINE_ECOSIA = "Ecosia"
        const val SEARCH_ENGINE_YAHOO = "Yahoo"

        val SEARCH_TEMPLATES = mapOf(
            SEARCH_ENGINE_GOOGLE to "https://www.google.com/search?q=%s",
            SEARCH_ENGINE_DUCKDUCKGO to "https://duckduckgo.com/?q=%s",
            SEARCH_ENGINE_BING to "https://www.bing.com/search?q=%s",
            SEARCH_ENGINE_ECOSIA to "https://www.ecosia.org/search?q=%s",
            SEARCH_ENGINE_YAHOO to "https://search.yahoo.com/search?p=%s"
        )

        val DEFAULT_SHORTCUTS = listOf(
            Shortcut("Google", "https://www.google.com"),
            Shortcut("Wikipedia", "https://www.wikipedia.org"),
            Shortcut("GitHub", "https://www.github.com"),
            Shortcut("YouTube", "https://www.youtube.com"),
            Shortcut("Reddit", "https://www.reddit.com"),
            Shortcut("News", "https://news.google.com")
        )
    }

    var theme: Int
        get() = prefs.getInt(KEY_THEME, THEME_SYSTEM)
        set(value) = prefs.edit().putInt(KEY_THEME, value).apply()

    var searchEngine: String
        get() = prefs.getString(KEY_SEARCH_ENGINE, SEARCH_ENGINE_GOOGLE) ?: SEARCH_ENGINE_GOOGLE
        set(value) = prefs.edit().putString(KEY_SEARCH_ENGINE, value).apply()

    var homepage: String
        get() = prefs.getString(KEY_HOMEPAGE, "ruhi://home") ?: "ruhi://home"
        set(value) = prefs.edit().putString(KEY_HOMEPAGE, value).apply()

    var adBlockEnabled: Boolean
        get() = prefs.getBoolean(KEY_ADBLOCK, true)
        set(value) = prefs.edit().putBoolean(KEY_ADBLOCK, value).apply()

    fun getSearchTemplate(): String {
        return SEARCH_TEMPLATES[searchEngine] ?: SEARCH_TEMPLATES.getValue(SEARCH_ENGINE_GOOGLE)
    }

    fun getCustomShortcuts(): List<Shortcut> {
        val raw = prefs.getString(KEY_CUSTOM_SHORTCUTS, null)
        if (raw.isNullOrBlank()) {
            return DEFAULT_SHORTCUTS
        }
        val list = mutableListOf<Shortcut>()
        try {
            val jsonArray = JSONArray(raw)
            for (i in 0 until jsonArray.length()) {
                val obj = jsonArray.getJSONObject(i)
                val title = obj.optString("title", "").trim()
                val url = obj.optString("url", "").trim()
                if (url.isNotEmpty()) {
                    list.add(Shortcut(title = if (title.isEmpty()) url else title, url = url))
                }
            }
        } catch (_: Exception) {
            return DEFAULT_SHORTCUTS
        }
        return if (list.isNotEmpty()) list else DEFAULT_SHORTCUTS
    }

    fun saveCustomShortcuts(shortcuts: List<Shortcut>) {
        val jsonArray = JSONArray()
        for (shortcut in shortcuts) {
            val obj = JSONObject()
            obj.put("title", shortcut.title)
            obj.put("url", shortcut.url)
            jsonArray.put(obj)
        }
        prefs.edit().putString(KEY_CUSTOM_SHORTCUTS, jsonArray.toString()).apply()
    }

    // AI Assistant Properties
    var aiApiKey: String
        get() = prefs.getString(KEY_AI_API_KEY, "") ?: ""
        set(value) = prefs.edit().putString(KEY_AI_API_KEY, value).apply()

    var aiModelEndpoint: String
        get() = prefs.getString(KEY_AI_ENDPOINT, DEFAULT_AI_ENDPOINT) ?: DEFAULT_AI_ENDPOINT
        set(value) = prefs.edit().putString(KEY_AI_ENDPOINT, value).apply()

    var aiEnabled: Boolean
        get() = prefs.getBoolean(KEY_AI_ENABLED, true)
        set(value) = prefs.edit().putBoolean(KEY_AI_ENABLED, value).apply()

    var aiConsentGiven: Boolean
        get() = prefs.getBoolean(KEY_AI_CONSENT, false)
        set(value) = prefs.edit().putBoolean(KEY_AI_CONSENT, value).apply()

    // VPN Properties
    var vpnServerAddress: String
        get() = prefs.getString(KEY_VPN_SERVER_ADDRESS, "") ?: ""
        set(value) = prefs.edit().putString(KEY_VPN_SERVER_ADDRESS, value).apply()

    var vpnServerPort: Int
        get() = prefs.getInt(KEY_VPN_SERVER_PORT, 51820)
        set(value) = prefs.edit().putInt(KEY_VPN_SERVER_PORT, value).apply()

    var vpnClientAddress: String
        get() = prefs.getString(KEY_VPN_CLIENT_ADDRESS, "10.0.0.2") ?: "10.0.0.2"
        set(value) = prefs.edit().putString(KEY_VPN_CLIENT_ADDRESS, value).apply()

    var vpnDns: String
        get() = prefs.getString(KEY_VPN_DNS, "1.1.1.1") ?: "1.1.1.1"
        set(value) = prefs.edit().putString(KEY_VPN_DNS, value).apply()

    var vpnPublicKey: String
        get() = prefs.getString(KEY_VPN_PUBLIC_KEY, "") ?: ""
        set(value) = prefs.edit().putString(KEY_VPN_PUBLIC_KEY, value).apply()

    var vpnPrivateKey: String
        get() = prefs.getString(KEY_VPN_PRIVATE_KEY, "") ?: ""
        set(value) = prefs.edit().putString(KEY_VPN_PRIVATE_KEY, value).apply()

    val isVpnConfigured: Boolean
        get() = vpnServerAddress.isNotBlank() && vpnPublicKey.isNotBlank()

    var syncEnabled: Boolean
        get() = prefs.getBoolean(KEY_SYNC_ENABLED, false)
        set(value) = prefs.edit().putBoolean(KEY_SYNC_ENABLED, value).apply()

    var syncDisplayName: String
        get() = prefs.getString(KEY_SYNC_DISPLAY_NAME, "") ?: ""
        set(value) = prefs.edit().putString(KEY_SYNC_DISPLAY_NAME, value).apply()

    var syncEmail: String
        get() = prefs.getString(KEY_SYNC_EMAIL, "") ?: ""
        set(value) = prefs.edit().putString(KEY_SYNC_EMAIL, value).apply()

    var syncUserId: String
        get() = prefs.getString(KEY_SYNC_USER_ID, "") ?: ""
        set(value) = prefs.edit().putString(KEY_SYNC_USER_ID, value).apply()

    var syncAccessToken: String
        get() = prefs.getString(KEY_SYNC_ACCESS_TOKEN, "") ?: ""
        set(value) = prefs.edit().putString(KEY_SYNC_ACCESS_TOKEN, value).apply()

    var syncRefreshToken: String
        get() = prefs.getString(KEY_SYNC_REFRESH_TOKEN, "") ?: ""
        set(value) = prefs.edit().putString(KEY_SYNC_REFRESH_TOKEN, value).apply()

    var syncDeviceId: String
        get() {
            var id = prefs.getString(KEY_SYNC_DEVICE_ID, "") ?: ""
            if (id.isEmpty()) {
                id = java.util.UUID.randomUUID().toString()
                prefs.edit().putString(KEY_SYNC_DEVICE_ID, id).apply()
            }
            return id
        }
        set(value) = prefs.edit().putString(KEY_SYNC_DEVICE_ID, value).apply()

    var syncDeviceName: String
        get() = prefs.getString(KEY_SYNC_DEVICE_NAME, android.os.Build.MODEL + " (Android)") ?: (android.os.Build.MODEL + " (Android)")
        set(value) = prefs.edit().putString(KEY_SYNC_DEVICE_NAME, value).apply()

    var lastHistoryClearedAt: Long
        get() = prefs.getLong(KEY_LAST_HISTORY_CLEARED_AT, 0L)
        set(value) = prefs.edit().putLong(KEY_LAST_HISTORY_CLEARED_AT, value).apply()

    var deletedHistoryUrls: Set<String>
        get() = prefs.getStringSet(KEY_DELETED_HISTORY_URLS, emptySet()) ?: emptySet()
        set(value) = prefs.edit().putStringSet(KEY_DELETED_HISTORY_URLS, value).apply()

    fun addDeletedHistoryUrl(url: String) {
        if (url.isBlank()) return
        val current = deletedHistoryUrls.toMutableSet()
        current.add(url.trim().lowercase())
        if (current.size > 200) {
            val trimmed = current.toList().takeLast(200).toSet()
            deletedHistoryUrls = trimmed
        } else {
            deletedHistoryUrls = current
        }
    }

    fun clearDeletedHistoryUrls() {
        prefs.edit().remove(KEY_DELETED_HISTORY_URLS).apply()
    }
}
