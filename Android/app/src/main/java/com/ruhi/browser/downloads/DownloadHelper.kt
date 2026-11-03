package com.ruhi.browser.downloads

import android.app.DownloadManager
import android.content.Context
import android.net.Uri
import android.os.Environment
import android.webkit.CookieManager
import android.webkit.URLUtil
import android.widget.Toast
import com.ruhi.browser.browser.UrlResolver

object DownloadHelper {

    fun startDownload(
        context: Context,
        url: String,
        userAgent: String,
        contentDisposition: String,
        mimeType: String
    ): Long {
        if (!UrlResolver.isWebScheme(url)) {
            Toast.makeText(context, "Cannot download from invalid URL", Toast.LENGTH_SHORT).show()
            return -1L
        }

        try {
            val fileName = URLUtil.guessFileName(url, contentDisposition, mimeType)
            val request = DownloadManager.Request(Uri.parse(url)).apply {
                val cookies = CookieManager.getInstance().getCookie(url)
                if (!cookies.isNullOrEmpty()) {
                    addRequestHeader("Cookie", cookies)
                }
                addRequestHeader("User-Agent", userAgent)
                setDescription("RUHI Browser Download")
                setTitle(fileName)
                setMimeType(mimeType)
                setNotificationVisibility(DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED)
                setDestinationInExternalPublicDir(Environment.DIRECTORY_DOWNLOADS, fileName)
            }

            val downloadManager = context.getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
            val downloadId = downloadManager.enqueue(request)
            Toast.makeText(context, "Download started: $fileName", Toast.LENGTH_SHORT).show()
            return downloadId
        } catch (e: Exception) {
            Toast.makeText(context, "Download failed to start: ${e.message}", Toast.LENGTH_LONG).show()
            return -1L
        }
    }
}
