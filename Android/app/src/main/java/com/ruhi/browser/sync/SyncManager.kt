package com.ruhi.browser.sync

import android.content.Context
import android.os.Handler
import android.os.Looper
import com.ruhi.browser.data.model.Bookmark
import com.ruhi.browser.data.preferences.SettingsPreferences
import com.ruhi.browser.data.repository.BookmarkRepository
import com.ruhi.browser.data.repository.HistoryRepository
import org.json.JSONArray
import org.json.JSONObject
import java.io.BufferedReader
import java.io.InputStreamReader
import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL
import java.nio.charset.StandardCharsets
import java.util.UUID
import java.util.concurrent.Executors

data class LinkedDevice(
    val deviceId: String,
    val deviceName: String,
    val deviceType: String,
    val lastActive: String,
    val activeTabsCount: Int
)

data class SyncedPassword(
    val id: String,
    val websiteUrl: String,
    val websiteTitle: String,
    val username: String,
    val encryptedPassword: String
)

private data class ApiResponse(val code: Int, val body: String)

class SyncManager private constructor(context: Context) {

    private val preferences = SettingsPreferences(context.applicationContext)
    private val executor = Executors.newSingleThreadExecutor()
    private val mainHandler = Handler(Looper.getMainLooper())
    private var lastActiveUrls: List<String> = emptyList()

    companion object {
        const val SUPABASE_URL = "https://voyzembamrvyrkrdnbsk.supabase.co"
        const val ANON_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZveXplbWJhbXJ2eXJrcmRuYnNrIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTM2MDgwNDIsImV4cCI6MjEwOTE4NDA0Mn0.4rmbKyD3HiV70ndDve-eUJg3WYkV7IDDdqOJRrPamXU"

        @Volatile
        private var instance: SyncManager? = null

        fun getInstance(context: Context): SyncManager {
            return instance ?: synchronized(this) {
                instance ?: SyncManager(context).also { instance = it }
            }
        }
    }

    fun isTokenExpired(token: String): Boolean {
        if (token.isBlank()) return true
        return try {
            val parts = token.split(".")
            if (parts.size >= 2) {
                val payload = String(
                    android.util.Base64.decode(parts[1], android.util.Base64.URL_SAFE or android.util.Base64.NO_WRAP or android.util.Base64.NO_PADDING),
                    StandardCharsets.UTF_8
                )
                val json = JSONObject(payload)
                val exp = json.optLong("exp", 0)
                val now = System.currentTimeMillis() / 1000
                now >= (exp - 60)
            } else true
        } catch (_: Exception) {
            true
        }
    }

    val isLoggedIn: Boolean
        get() {
            if (!preferences.syncEnabled) return false
            if (preferences.syncRefreshToken.isNotEmpty()) return true
            if (preferences.syncAccessToken.isNotEmpty() && !isTokenExpired(preferences.syncAccessToken)) return true
            return false
        }

    val currentEmail: String
        get() = preferences.syncEmail

    val currentDisplayName: String
        get() = preferences.syncDisplayName

    fun setDisplayName(name: String) {
        preferences.syncDisplayName = name
    }

    val deviceId: String
        get() = preferences.syncDeviceId

    val deviceName: String
        get() = preferences.syncDeviceName

    @Synchronized
    fun refreshSession(): Boolean {
        val rf = preferences.syncRefreshToken
        if (rf.isEmpty()) return false
        try {
            val url = URL("$SUPABASE_URL/auth/v1/token?grant_type=refresh_token")
            val conn = url.openConnection() as HttpURLConnection
            conn.requestMethod = "POST"
            conn.connectTimeout = 12000
            conn.readTimeout = 12000
            conn.setRequestProperty("apikey", ANON_KEY)
            conn.setRequestProperty("Content-Type", "application/json")
            conn.doOutput = true

            val reqBody = JSONObject().apply {
                put("refresh_token", rf)
            }
            OutputStreamWriter(conn.outputStream, StandardCharsets.UTF_8).use { it.write(reqBody.toString()) }

            val code = conn.responseCode
            if (code in 200..299) {
                val resp = readStream(conn.inputStream)
                val json = JSONObject(resp)
                val newAccess = json.optString("access_token", "")
                val newRefresh = json.optString("refresh_token", "")
                if (newAccess.isNotEmpty()) {
                    preferences.syncAccessToken = newAccess
                }
                if (newRefresh.isNotEmpty()) {
                    preferences.syncRefreshToken = newRefresh
                }
                val meta = json.optJSONObject("user")?.optJSONObject("user_metadata")
                val dName = meta?.optString("display_name", "") ?: ""
                if (dName.isNotEmpty()) {
                    preferences.syncDisplayName = dName
                }
                return true
            }
        } catch (_: Exception) { }
        return false
    }

    private fun sendRequest(
        endpoint: String,
        method: String,
        body: String? = null,
        headers: Map<String, String>? = null,
        hasAuth: Boolean = true
    ): ApiResponse {
        fun doSend(): ApiResponse {
            return try {
                val url = URL(SUPABASE_URL + endpoint)
                val conn = url.openConnection() as HttpURLConnection
                conn.requestMethod = method
                conn.connectTimeout = 12000
                conn.readTimeout = 12000
                conn.setRequestProperty("apikey", ANON_KEY)
                conn.setRequestProperty("Content-Type", "application/json")
                if (hasAuth && preferences.syncAccessToken.isNotEmpty()) {
                    conn.setRequestProperty("Authorization", "Bearer ${preferences.syncAccessToken}")
                }
                headers?.forEach { (k, v) -> conn.setRequestProperty(k, v) }
                if (body != null) {
                    conn.doOutput = true
                    OutputStreamWriter(conn.outputStream, StandardCharsets.UTF_8).use { it.write(body) }
                }
                val code = conn.responseCode
                val resp = if (code in 200..299) {
                    readStream(conn.inputStream)
                } else {
                    readStream(conn.errorStream)
                }
                ApiResponse(code, resp)
            } catch (e: Exception) {
                ApiResponse(-1, e.localizedMessage ?: "Network error")
            }
        }

        var res = doSend()
        if (hasAuth && res.code == 401) {
            if (preferences.syncRefreshToken.isNotEmpty()) {
                if (refreshSession()) {
                    res = doSend()
                } else {
                    preferences.syncAccessToken = ""
                    preferences.syncRefreshToken = ""
                }
            } else {
                preferences.syncAccessToken = ""
            }
        }
        return res
    }

    fun updateDisplayName(name: String, callback: ((Boolean) -> Unit)? = null) {
        preferences.syncDisplayName = name
        if (!isLoggedIn) {
            callback?.invoke(true)
            return
        }

        executor.execute {
            try {
                val body = JSONObject().apply {
                    put("data", JSONObject().apply {
                        put("display_name", name)
                    })
                }
                val res = sendRequest("/auth/v1/user", "PUT", body.toString())
                val ok = res.code in 200..299
                mainHandler.post { callback?.invoke(ok) }
            } catch (e: Exception) {
                mainHandler.post { callback?.invoke(false) }
            }
        }
    }

    fun refreshUserProfile(callback: ((Boolean) -> Unit)? = null) {
        if (!isLoggedIn) {
            callback?.invoke(false)
            return
        }
        executor.execute {
            try {
                val res = sendRequest("/auth/v1/user", "GET")
                if (res.code in 200..299) {
                    val json = JSONObject(res.body)
                    val meta = json.optJSONObject("user_metadata")
                    val dName = meta?.optString("display_name", "") ?: ""
                    if (dName.isNotEmpty()) {
                        preferences.syncDisplayName = dName
                    }
                    mainHandler.post { callback?.invoke(true) }
                } else {
                    mainHandler.post { callback?.invoke(false) }
                }
            } catch (_: Exception) {
                mainHandler.post { callback?.invoke(false) }
            }
        }
    }

    fun login(email: String, pass: String, callback: (Boolean, String) -> Unit) {
        executor.execute {
            try {
                val body = JSONObject().apply {
                    put("email", email)
                    put("password", pass)
                }
                val res = sendRequest("/auth/v1/token?grant_type=password", "POST", body.toString(), hasAuth = false)
                if (res.code in 200..299) {
                    val json = JSONObject(res.body)
                    preferences.syncEnabled = true
                    preferences.syncEmail = email
                    preferences.syncUserId = json.optJSONObject("user")?.optString("id", "") ?: ""
                    preferences.syncAccessToken = json.optString("access_token", "")
                    preferences.syncRefreshToken = json.optString("refresh_token", "")

                    val meta = json.optJSONObject("user")?.optJSONObject("user_metadata")
                    val dName = meta?.optString("display_name", "") ?: ""
                    if (dName.isNotEmpty()) {
                        preferences.syncDisplayName = dName
                    }

                    mainHandler.post { callback(true, "Login successful!") }
                } else {
                    val json = try { JSONObject(res.body) } catch (_: Exception) { JSONObject() }
                    val msg = json.optString("error_description", json.optString("msg", "Login failed."))
                    mainHandler.post { callback(false, msg) }
                }
            } catch (e: Exception) {
                mainHandler.post { callback(false, e.localizedMessage ?: "Connection error") }
            }
        }
    }

    fun signUp(email: String, pass: String, displayName: String = "", callback: (Boolean, String) -> Unit) {
        executor.execute {
            try {
                val body = JSONObject().apply {
                    put("email", email)
                    put("password", pass)
                    if (displayName.isNotBlank()) {
                        put("data", JSONObject().apply {
                            put("display_name", displayName)
                        })
                    }
                }
                val res = sendRequest("/auth/v1/signup", "POST", body.toString(), hasAuth = false)
                if (res.code in 200..299) {
                    val json = JSONObject(res.body)
                    val token = json.optString("access_token", "")
                    if (token.isNotEmpty()) {
                        preferences.syncEnabled = true
                        preferences.syncEmail = email
                        if (displayName.isNotBlank()) {
                            preferences.syncDisplayName = displayName
                        }
                        preferences.syncUserId = json.optJSONObject("user")?.optString("id", "") ?: ""
                        preferences.syncAccessToken = token
                        preferences.syncRefreshToken = json.optString("refresh_token", "")
                        mainHandler.post { callback(true, "Account created & logged in!") }
                    } else {
                        login(email, pass, callback)
                    }
                } else {
                    val json = try { JSONObject(res.body) } catch (_: Exception) { JSONObject() }
                    val msg = json.optString("msg", json.optString("error_description", "Sign up failed."))
                    mainHandler.post { callback(false, msg) }
                }
            } catch (e: Exception) {
                mainHandler.post { callback(false, e.localizedMessage ?: "Connection error") }
            }
        }
    }

    fun logout() {
        preferences.syncEnabled = false
        preferences.syncEmail = ""
        preferences.syncDisplayName = ""
        preferences.syncUserId = ""
        preferences.syncAccessToken = ""
        preferences.syncRefreshToken = ""
    }

    fun deleteBookmark(url: String) {
        if (!isLoggedIn || url.isBlank()) return
        executor.execute {
            try {
                val encUrl = java.net.URLEncoder.encode(url, "UTF-8")
                val body = JSONObject().apply {
                    put("is_deleted", true)
                }
                sendRequest("/rest/v1/ruhi_sync_bookmarks?url=eq.$encUrl", "PATCH", body.toString())
            } catch (_: Exception) { }
        }
    }

    fun clearAllBookmarks() {
        if (!isLoggedIn) return
        executor.execute {
            try {
                val body = JSONObject().apply {
                    put("is_deleted", true)
                }
                sendRequest("/rest/v1/ruhi_sync_bookmarks?user_id=eq.${preferences.syncUserId}", "PATCH", body.toString())
            } catch (_: Exception) { }
        }
    }

    fun deleteHistoryItem(url: String, id: Long? = null) {
        if (url.isNotBlank()) {
            preferences.addDeletedHistoryUrl(url)
        }
        updateHeartbeat(lastActiveUrls)
        if (!isLoggedIn) return
        executor.execute {
            try {
                if (id != null) {
                    sendRequest("/rest/v1/ruhi_sync_history?user_id=eq.${preferences.syncUserId}&id=eq.android_hist_$id", "DELETE")
                }
                if (url.isNotBlank()) {
                    val encUrl = java.net.URLEncoder.encode(url, "UTF-8")
                    sendRequest("/rest/v1/ruhi_sync_history?user_id=eq.${preferences.syncUserId}&url=eq.$encUrl", "DELETE")
                }
            } catch (_: Exception) { }
        }
    }

    fun clearAllHistory() {
        preferences.lastHistoryClearedAt = System.currentTimeMillis()
        preferences.clearDeletedHistoryUrls()
        updateHeartbeat(lastActiveUrls)
        if (!isLoggedIn) return
        executor.execute {
            try {
                sendRequest("/rest/v1/ruhi_sync_history?user_id=eq.${preferences.syncUserId}", "DELETE")
            } catch (_: Exception) { }
        }
    }

    fun syncBookmarks(bookmarkRepo: BookmarkRepository, callback: ((Boolean) -> Unit)? = null) {
        if (!isLoggedIn) {
            callback?.invoke(false)
            return
        }

        executor.execute {
            try {
                // 1. Pull remote bookmarks (including tombstones)
                val pullRes = sendRequest("/rest/v1/ruhi_sync_bookmarks?select=*", "GET")
                val remoteDeleted = mutableSetOf<String>()
                if (pullRes.code in 200..299) {
                    val arr = JSONArray(pullRes.body)

                    // First pass: identify all deleted bookmarks
                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val url = obj.optString("url", "").trim()
                        val isDeleted = obj.optBoolean("is_deleted", false)
                        if (url.isNotEmpty() && isDeleted) {
                            remoteDeleted.add(url.lowercase())
                            bookmarkRepo.deleteBookmarkByUrl(url)
                        }
                    }

                    // Second pass: add active bookmarks that aren't marked deleted
                    val localList = bookmarkRepo.getBookmarks()
                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val url = obj.optString("url", "").trim()
                        val title = obj.optString("title", "")
                        val isDeleted = obj.optBoolean("is_deleted", false)

                        if (!isDeleted && url.isNotEmpty() && !remoteDeleted.contains(url.lowercase())) {
                            if (localList.none { it.url.equals(url, ignoreCase = true) }) {
                                bookmarkRepo.addBookmark(title, url)
                            }
                        }
                    }
                }

                // 2. Push local bookmarks (excluding remotely deleted ones)
                val localBookmarks = bookmarkRepo.getBookmarks().filter { !remoteDeleted.contains(it.url.lowercase()) }
                if (localBookmarks.isNotEmpty()) {
                    val pushArr = JSONArray()
                    for (b in localBookmarks) {
                        pushArr.put(JSONObject().apply {
                            put("id", "android_" + b.id)
                            put("user_id", preferences.syncUserId)
                            put("url", b.url)
                            put("title", b.title)
                            put("folder", "")
                            put("is_deleted", false)
                        })
                    }

                    sendRequest(
                        "/rest/v1/ruhi_sync_bookmarks?on_conflict=id",
                        "POST",
                        pushArr.toString(),
                        mapOf("Prefer" to "resolution=merge-duplicates")
                    )
                }

                mainHandler.post { callback?.invoke(true) }
            } catch (e: Exception) {
                mainHandler.post { callback?.invoke(false) }
            }
        }
    }

    fun checkRemoteHistoryDeletions(historyRepo: HistoryRepository, onModified: (() -> Unit)? = null) {
        if (!isLoggedIn) return
        executor.execute {
            val modified = performCheckRemoteHistoryDeletions(historyRepo)
            if (modified && onModified != null) {
                mainHandler.post { onModified() }
            }
        }
    }

    private fun performCheckRemoteHistoryDeletions(historyRepo: HistoryRepository): Boolean {
        if (!isLoggedIn) return false
        var modified = false
        try {
            val res = sendRequest("/rest/v1/ruhi_sync_devices?select=*", "GET")
            if (res.code in 200..299) {
                val arr = JSONArray(res.body)
                val sdfIso = java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", java.util.Locale.US).apply {
                    timeZone = java.util.TimeZone.getTimeZone("UTC")
                }

                for (i in 0 until arr.length()) {
                    val dev = arr.getJSONObject(i)
                    val devId = dev.optString("device_id", "")
                    if (devId == deviceId) continue

                    val activeTabsObj = dev.optJSONObject("active_tabs") ?: continue

                    val clearedAtStr = activeTabsObj.optString("cleared_at", "")
                    if (clearedAtStr.isNotBlank() && clearedAtStr != "null") {
                        try {
                            val cleanIso = clearedAtStr.substringBefore('.').substringBefore('Z')
                            val date = sdfIso.parse(cleanIso)
                            val remoteClearedMs = date?.time ?: 0L
                            if (remoteClearedMs > preferences.lastHistoryClearedAt) {
                                preferences.lastHistoryClearedAt = remoteClearedMs
                                val currentHistory = historyRepo.getHistory()
                                for (item in currentHistory) {
                                    if (item.visitedAt <= remoteClearedMs) {
                                        historyRepo.deleteItem(item.id)
                                        modified = true
                                    }
                                }
                            }
                        } catch (_: Exception) {}
                    }

                    val delUrlsArr = activeTabsObj.optJSONArray("deleted_urls")
                    if (delUrlsArr != null) {
                        for (j in 0 until delUrlsArr.length()) {
                            val u = delUrlsArr.optString(j, "").trim().lowercase()
                            if (u.isNotBlank()) {
                                preferences.addDeletedHistoryUrl(u)
                                val deleted = historyRepo.deleteByUrl(u)
                                if (deleted) modified = true
                            }
                        }
                    }
                }
            }
        } catch (_: Exception) {}
        return modified
    }

    fun syncHistory(historyRepo: HistoryRepository, callback: ((Boolean) -> Unit)? = null) {
        if (!isLoggedIn) {
            callback?.invoke(false)
            return
        }

        executor.execute {
            try {
                // 1. Process remote history deletions first
                val deletionsApplied = performCheckRemoteHistoryDeletions(historyRepo)

                val clearedAt = preferences.lastHistoryClearedAt
                val deletedUrls = preferences.deletedHistoryUrls
                val sdf = java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", java.util.Locale.US).apply {
                    timeZone = java.util.TimeZone.getTimeZone("UTC")
                }

                // 2. Pull remote history
                var addedNew = false
                val pullRes = sendRequest("/rest/v1/ruhi_sync_history?select=*&order=visited_at.desc&limit=100", "GET")
                if (pullRes.code in 200..299) {
                    val arr = JSONArray(pullRes.body)
                    val localList = historyRepo.getHistory()

                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val url = obj.optString("url", "").trim()
                        val title = obj.optString("title", "")
                        val visitedStr = obj.optString("visited_at", "")

                        if (url.isEmpty()) continue

                        // If user has explicitly deleted this URL, skip and do not restore
                        if (deletedUrls.contains(url.lowercase())) continue

                        // If this item was visited BEFORE the user cleared all history, skip
                        if (clearedAt > 0 && visitedStr.isNotEmpty()) {
                            try {
                                val cleanStr = visitedStr.replace("Z", "").split(".")[0]
                                val itemTime = sdf.parse(cleanStr)?.time ?: 0L
                                if (itemTime <= clearedAt) continue
                            } catch (_: Exception) { }
                        }

                        if (localList.none { it.url.equals(url, ignoreCase = true) }) {
                            historyRepo.recordVisit(url = url, title = title.ifEmpty { url }, isIncognito = false)
                            addedNew = true
                        }
                    }
                }

                // 3. Push local history (exclude any deleted URLs or items before clearedAt)
                val allLocal = historyRepo.getHistory()
                val localHistory = allLocal
                    .filter { !deletedUrls.contains(it.url.trim().lowercase()) }
                    .filter { clearedAt == 0L || it.visitedAt > clearedAt }
                    .take(50)

                if (localHistory.isNotEmpty()) {
                    val pushArr = JSONArray()
                    val sdfIso = java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", java.util.Locale.US).apply {
                        timeZone = java.util.TimeZone.getTimeZone("UTC")
                    }
                    for (h in localHistory) {
                        pushArr.put(JSONObject().apply {
                            put("id", "android_hist_" + h.id)
                            put("user_id", preferences.syncUserId)
                            put("url", h.url)
                            put("title", h.title)
                            put("visited_at", sdfIso.format(java.util.Date(h.visitedAt)))
                        })
                    }

                    sendRequest(
                        "/rest/v1/ruhi_sync_history?on_conflict=id",
                        "POST",
                        pushArr.toString(),
                        mapOf("Prefer" to "resolution=merge-duplicates")
                    )
                }

                mainHandler.post { callback?.invoke(deletionsApplied || addedNew) }
            } catch (e: Exception) {
                mainHandler.post { callback?.invoke(false) }
            }
        }
    }

    fun pullPasswords(callback: (List<SyncedPassword>) -> Unit) {
        if (!isLoggedIn) {
            callback(emptyList())
            return
        }
        executor.execute {
            val list = mutableListOf<SyncedPassword>()
            try {
                val res = sendRequest("/rest/v1/ruhi_sync_passwords?select=*&order=created_at.desc", "GET")
                if (res.code in 200..299) {
                    val arr = JSONArray(res.body)
                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val id = obj.optString("id", "")
                        val url = obj.optString("website_url", "")
                        val title = obj.optString("website_title", "")
                        val user = obj.optString("username", "")
                        val pass = obj.optString("encrypted_password", "")
                        list.add(SyncedPassword(id, url, title, user, pass))
                    }
                }
            } catch (_: Exception) { }
            mainHandler.post { callback(list) }
        }
    }

    fun updateHeartbeat(activeUrls: List<String> = lastActiveUrls) {
        lastActiveUrls = activeUrls
        if (!isLoggedIn) return
        executor.execute {
            try {
                val tabsArr = JSONArray()
                activeUrls.forEach { tabsArr.put(it) }

                val delUrlsArr = JSONArray()
                preferences.deletedHistoryUrls.forEach { delUrlsArr.put(it) }

                val clearedAtIso = if (preferences.lastHistoryClearedAt > 0L) {
                    val sdfIso = java.text.SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'", java.util.Locale.US).apply {
                        timeZone = java.util.TimeZone.getTimeZone("UTC")
                    }
                    sdfIso.format(java.util.Date(preferences.lastHistoryClearedAt))
                } else null

                val activeTabsObj = JSONObject().apply {
                    put("tabs", tabsArr)
                    if (clearedAtIso != null) put("cleared_at", clearedAtIso) else put("cleared_at", JSONObject.NULL)
                    put("deleted_urls", delUrlsArr)
                }

                val body = JSONObject().apply {
                    put("device_id", deviceId)
                    put("user_id", preferences.syncUserId)
                    put("device_name", deviceName)
                    put("device_type", "Android")
                    put("active_tabs", activeTabsObj)
                }

                sendRequest(
                    "/rest/v1/ruhi_sync_devices?on_conflict=device_id",
                    "POST",
                    body.toString(),
                    mapOf("Prefer" to "resolution=merge-duplicates")
                )
            } catch (_: Exception) { }
        }
    }

    fun getLinkedDevices(callback: (List<LinkedDevice>) -> Unit) {
        if (!isLoggedIn) {
            callback(emptyList())
            return
        }
        executor.execute {
            val list = mutableListOf<LinkedDevice>()
            try {
                val res = sendRequest("/rest/v1/ruhi_sync_devices?select=*&order=last_active.desc", "GET")
                if (res.code in 200..299) {
                    val arr = JSONArray(res.body)
                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val devId = obj.optString("device_id", "")
                        val name = obj.optString("device_name", "")
                        val type = obj.optString("device_type", "Windows")
                        val last = obj.optString("last_active", "")
                        val tabs = when (val at = obj.opt("active_tabs")) {
                            is JSONArray -> at.length()
                            is JSONObject -> at.optJSONArray("tabs")?.length() ?: 0
                            else -> 0
                        }
                        list.add(LinkedDevice(devId, name, type, last, tabs))
                    }
                }
            } catch (_: Exception) { }
            mainHandler.post { callback(list) }
        }
    }

    fun sendTabToDevice(targetDeviceId: String?, url: String, title: String, callback: (Boolean) -> Unit) {
        if (!isLoggedIn) {
            callback(false)
            return
        }
        executor.execute {
            try {
                val body = JSONObject().apply {
                    put("user_id", preferences.syncUserId)
                    put("sender_device_id", deviceId)
                    if (!targetDeviceId.isNullOrBlank() && !targetDeviceId.equals("null", ignoreCase = true)) {
                        put("target_device_id", targetDeviceId)
                    }
                    put("url", url)
                    put("title", title)
                    put("is_opened", false)
                }
                val res = sendRequest("/rest/v1/ruhi_send_tabs", "POST", body.toString())
                val ok = res.code in 200..299
                mainHandler.post { callback(ok) }
            } catch (e: Exception) {
                mainHandler.post { callback(false) }
            }
        }
    }

    fun checkIncomingTabs(onTabReceived: (title: String, url: String) -> Unit) {
        if (!isLoggedIn) return
        executor.execute {
            try {
                val query = "/rest/v1/ruhi_send_tabs?select=*&is_opened=eq.false&sender_device_id=neq.$deviceId"
                val res = sendRequest(query, "GET")
                if (res.code in 200..299) {
                    val arr = JSONArray(res.body)
                    for (i in 0 until arr.length()) {
                        val obj = arr.getJSONObject(i)
                        val target = if (obj.isNull("target_device_id")) "" else obj.optString("target_device_id", "").trim()
                        if (target.isEmpty() || target.equals("null", ignoreCase = true) || target == deviceId) {
                            val id = obj.optString("id", "")
                            val url = obj.optString("url", "")
                            val title = obj.optString("title", "")

                            // Mark opened
                            markTabOpened(id)

                            mainHandler.post {
                                onTabReceived(title, url)
                            }
                        }
                    }
                }
            } catch (_: Exception) { }
        }
    }

    private fun markTabOpened(tabId: String) {
        try {
            val body = JSONObject().apply { put("is_opened", true) }
            sendRequest("/rest/v1/ruhi_send_tabs?id=eq.$tabId", "PATCH", body.toString())
        } catch (_: Exception) { }
    }

    private fun readStream(stream: java.io.InputStream?): String {
        if (stream == null) return ""
        val reader = BufferedReader(InputStreamReader(stream, StandardCharsets.UTF_8))
        val sb = StringBuilder()
        var line: String?
        while (reader.readLine().also { line = it } != null) {
            sb.append(line)
        }
        reader.close()
        return sb.toString()
    }
}
