package com.ruhi.browser.browser

import android.content.Context
import android.content.Intent
import android.net.Uri
import android.net.http.SslError
import android.webkit.SslErrorHandler
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebView
import android.webkit.WebViewClient
import android.widget.Toast
import com.ruhi.browser.R

class RuhiWebViewClient(
    private val context: Context,
    private val listener: BrowserEventListener
) : WebViewClient() {

    interface BrowserEventListener {
        fun onPageStarted(url: String)
        fun onPageFinished(url: String, canGoBack: Boolean, canGoForward: Boolean)
        fun onPageError(url: String, errorCode: Int, description: String)
        fun onSslError(url: String, sslError: SslError?)
    }

    override fun shouldOverrideUrlLoading(view: WebView?, request: WebResourceRequest?): Boolean {
        val uri = request?.url ?: return false
        val url = uri.toString()

        // 1. Google Play Store URLs (e.g. market:// or https://play.google.com/store/apps/details?id=...)
        if (isPlayStoreUrl(url)) {
            if (launchPlayStore(url)) {
                return true
            }
        }

        // 2. Standard web navigation: handled internally by WebView
        if (UrlResolver.isWebScheme(url)) {
            return false
        }

        // 3. Android intent scheme (intent://...#Intent;...)
        if (url.startsWith("intent:", ignoreCase = true)) {
            return handleIntentUri(view, url)
        }

        // 4. Custom schemes (upi://, market://, whatsapp://, phonepe://, paytmmp://, tel:, mailto:, sms:, etc.)
        if (UrlResolver.isExternalScheme(url)) {
            return launchCustomScheme(uri, url)
        }

        return false
    }

    override fun shouldInterceptRequest(
        view: WebView?,
        request: WebResourceRequest?
    ): android.webkit.WebResourceResponse? {
        if (request != null && com.ruhi.browser.blocking.AdBlockManager.shouldBlock(request.url, request.isForMainFrame)) {
            return com.ruhi.browser.blocking.AdBlockManager.createBlockedResponse()
        }
        return super.shouldInterceptRequest(view, request)
    }

    private fun isPlayStoreUrl(url: String): Boolean {
        val lower = url.lowercase()
        return (lower.startsWith("market://") ||
                (lower.contains("play.google.com") && lower.contains("/store/apps/details")))
    }

    private fun launchPlayStore(url: String): Boolean {
        try {
            val uri = Uri.parse(url)
            val id = uri.getQueryParameter("id")
            val targetUri = if (!id.isNullOrBlank()) Uri.parse("market://details?id=$id") else uri
            val playIntent = Intent(Intent.ACTION_VIEW, targetUri).apply {
                setPackage("com.android.vending")
                flags = Intent.FLAG_ACTIVITY_NEW_TASK
            }
            context.startActivity(playIntent)
            return true
        } catch (e: Exception) {
            return try {
                val intent = Intent(Intent.ACTION_VIEW, Uri.parse(url)).apply {
                    flags = Intent.FLAG_ACTIVITY_NEW_TASK
                }
                context.startActivity(intent)
                true
            } catch (e2: Exception) {
                false
            }
        }
    }

    private fun handleIntentUri(view: WebView?, url: String): Boolean {
        try {
            val intent = Intent.parseUri(url, Intent.URI_INTENT_SCHEME)
            intent.addCategory(Intent.CATEGORY_BROWSABLE)
            intent.component = null
            intent.selector = null

            val resolveInfo = context.packageManager.resolveActivity(intent, 0)
            if (resolveInfo != null) {
                intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK
                context.startActivity(intent)
                return true
            }

            // Fallback URL if target app is not installed
            val fallbackUrl = intent.getStringExtra("browser_fallback_url")
            if (!fallbackUrl.isNullOrBlank()) {
                view?.loadUrl(fallbackUrl)
                return true
            }

            // Fallback to Play Store if package name is specified
            val packageName = intent.`package`
            if (!packageName.isNullOrBlank()) {
                return launchPlayStore("market://details?id=$packageName")
            }
        } catch (e: Exception) {
            Toast.makeText(
                context,
                context.getString(R.string.external_intent_error),
                Toast.LENGTH_SHORT
            ).show()
        }
        return true
    }

    private fun launchCustomScheme(uri: Uri, url: String): Boolean {
        try {
            val intent = Intent(Intent.ACTION_VIEW, uri).apply {
                flags = Intent.FLAG_ACTIVITY_NEW_TASK
            }
            context.startActivity(intent)
            return true
        } catch (e: Exception) {
            val isPayment = url.startsWith("upi:", ignoreCase = true) ||
                    url.startsWith("phonepe:", ignoreCase = true) ||
                    url.startsWith("paytmmp:", ignoreCase = true) ||
                    url.startsWith("tez:", ignoreCase = true)
            val msg = if (isPayment) {
                "No supported UPI / payment app found"
            } else {
                context.getString(R.string.external_intent_error)
            }
            Toast.makeText(context, msg, Toast.LENGTH_SHORT).show()
            return true
        }
    }

    override fun onPageStarted(view: WebView?, url: String?, favicon: android.graphics.Bitmap?) {
        super.onPageStarted(view, url, favicon)
        url?.let { listener.onPageStarted(it) }
    }

    override fun onPageFinished(view: WebView?, url: String?) {
        super.onPageFinished(view, url)
        val currentUrl = url ?: view?.url.orEmpty()
        listener.onPageFinished(
            currentUrl,
            canGoBack = view?.canGoBack() == true,
            canGoForward = view?.canGoForward() == true
        )
    }

    override fun onReceivedError(
        view: WebView?,
        request: WebResourceRequest?,
        error: WebResourceError?
    ) {
        super.onReceivedError(view, request, error)
        if (request?.isForMainFrame == true) {
            val failingUrl = request.url.toString()
            val errorCode = error?.errorCode ?: -1
            val description = error?.description?.toString() ?: "Unknown network error"
            listener.onPageError(failingUrl, errorCode, description)
        }
    }

    /**
     * CRITICAL SECURITY REQUIREMENT:
     * Never bypass SSL/TLS certificate errors.
     * Always cancel the request.
     */
    override fun onReceivedSslError(
        view: WebView?,
        handler: SslErrorHandler?,
        error: SslError?
    ) {
        handler?.cancel()
        val failingUrl = error?.url.orEmpty()
        listener.onSslError(failingUrl, error)
    }
}
