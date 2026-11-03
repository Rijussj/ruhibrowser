package com.ruhi.browser.util

import android.content.Context
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.graphics.Color
import android.net.Uri
import android.os.Handler
import android.os.Looper
import android.util.LruCache
import java.io.File
import java.io.FileOutputStream
import java.net.HttpURLConnection
import java.net.URL
import java.util.concurrent.Executors

object FaviconManager {

    private val cache = LruCache<String, Bitmap>(50)
    private val executor = Executors.newFixedThreadPool(2)
    private val mainHandler = Handler(Looper.getMainLooper())
    private var cacheDir: File? = null

    fun init(context: Context) {
        if (cacheDir == null) {
            cacheDir = File(context.cacheDir, "favicons").apply { mkdirs() }
        }
    }

    fun getDomain(url: String?): String {
        if (url.isNullOrBlank()) return ""
        return try {
            val uri = Uri.parse(url)
            val host = uri.host ?: return ""
            host.removePrefix("www.")
        } catch (_: Exception) {
            ""
        }
    }

    fun getFavicon(url: String?): Bitmap? {
        val domain = getDomain(url)
        if (domain.isBlank()) return null

        cache.get(domain)?.let { return it }

        val diskFile = File(cacheDir ?: return null, "${domain.hashCode()}.png")
        if (diskFile.exists()) {
            try {
                val bitmap = BitmapFactory.decodeFile(diskFile.absolutePath)
                if (bitmap != null) {
                    cache.put(domain, bitmap)
                    return bitmap
                }
            } catch (_: Exception) {}
        }
        return null
    }

    fun saveFavicon(url: String?, bitmap: Bitmap?) {
        if (bitmap == null) return
        val domain = getDomain(url)
        if (domain.isBlank()) return

        cache.put(domain, bitmap)
        executor.execute {
            val dir = cacheDir ?: return@execute
            val diskFile = File(dir, "${domain.hashCode()}.png")
            try {
                FileOutputStream(diskFile).use { out ->
                    bitmap.compress(Bitmap.CompressFormat.PNG, 100, out)
                }
            } catch (_: Exception) {}
        }
    }

    fun fetchFavicon(url: String?, onLoaded: (Bitmap?) -> Unit) {
        val domain = getDomain(url)
        if (domain.isBlank()) {
            onLoaded(null)
            return
        }

        val cached = getFavicon(url)
        if (cached != null) {
            onLoaded(cached)
            return
        }

        executor.execute {
            var result: Bitmap? = null
            try {
                val faviconUrl = "https://www.google.com/s2/favicons?domain=$domain&sz=128"
                val connection = URL(faviconUrl).openConnection() as HttpURLConnection
                connection.connectTimeout = 4000
                connection.readTimeout = 4000
                connection.doInput = true
                connection.connect()
                if (connection.responseCode == 200) {
                    val input = connection.inputStream
                    result = BitmapFactory.decodeStream(input)
                    if (result != null) {
                        saveFavicon(url, result)
                    }
                }
                connection.disconnect()
            } catch (_: Exception) {}

            mainHandler.post {
                onLoaded(result)
            }
        }
    }

    fun getDomainInitial(url: String?): String {
        val domain = getDomain(url)
        return if (domain.isNotEmpty()) {
            domain.first().uppercase()
        } else {
            "W"
        }
    }

    fun getDomainColor(url: String?): Int {
        val domain = getDomain(url)
        val colors = intArrayOf(
            Color.parseColor("#4285F4"), // Google Blue
            Color.parseColor("#EA4335"), // Google Red
            Color.parseColor("#FBBC05"), // Google Yellow
            Color.parseColor("#34A853"), // Google Green
            Color.parseColor("#9C27B0"), // Purple
            Color.parseColor("#009688"), // Teal
            Color.parseColor("#FF5722"), // Deep Orange
            Color.parseColor("#3F51B5"), // Indigo
            Color.parseColor("#00BCD4"), // Cyan
            Color.parseColor("#E91E63")  // Pink
        )
        val hash = kotlin.math.abs(domain.hashCode())
        return colors[hash % colors.size]
    }
}
