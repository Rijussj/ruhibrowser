package com.ruhi.browser.blocking

import android.net.Uri
import java.io.ByteArrayInputStream
import android.webkit.WebResourceResponse

object AdBlockManager {

    var isEnabled: Boolean = true

    // Legitimate local rule-set of well-known tracking and advertising domains
    private val KNOWN_AD_DOMAINS = setOf(
        "doubleclick.net",
        "googleadservices.com",
        "googlesyndication.com",
        "adservice.google.com",
        "pagead2.googlesyndication.com",
        "adnxs.com",
        "advertising.com",
        "scorecardresearch.com",
        "criteo.com",
        "criteo.net",
        "outbrain.com",
        "taboola.com",
        "amazon-adsystem.com",
        "popads.net",
        "propellerads.com",
        "adcolony.com",
        "admob.com",
        "unityads.unity3d.com"
    )

    private val allowlist = mutableSetOf<String>()

    fun setAllowlisted(host: String, allowed: Boolean) {
        val clean = host.lowercase().trim()
        if (allowed) {
            allowlist.add(clean)
        } else {
            allowlist.remove(clean)
        }
    }

    fun isAllowlisted(host: String): Boolean {
        val clean = host.lowercase().trim()
        return allowlist.any { clean == it || clean.endsWith(".$it") }
    }

    /**
     * Determines whether a web request corresponds to a known ad or tracker.
     *
     * SAFETY REQUIREMENT:
     * Never block main-frame navigation requests (isForMainFrame == true).
     */
    fun shouldBlock(uri: Uri?, isForMainFrame: Boolean): Boolean {
        if (!isEnabled || uri == null || isForMainFrame) {
            return false
        }

        val host = uri.host?.lowercase() ?: return false
        if (isAllowlisted(host)) {
            return false
        }

        return KNOWN_AD_DOMAINS.any { host == it || host.endsWith(".$it") }
    }

    /**
     * Helper to test against plain URL string (useful in JVM unit tests).
     */
    fun shouldBlockUrl(url: String, isForMainFrame: Boolean): Boolean {
        if (!isEnabled || isForMainFrame) return false
        val host = extractHost(url) ?: return false
        if (isAllowlisted(host)) return false
        return KNOWN_AD_DOMAINS.any { host == it || host.endsWith(".$it") }
    }

    fun createBlockedResponse(): WebResourceResponse {
        return WebResourceResponse(
            "text/plain",
            "UTF-8",
            ByteArrayInputStream(ByteArray(0))
        )
    }

    private fun extractHost(url: String): String? {
        val withoutScheme = url.substringAfter("://", "")
        if (withoutScheme.isEmpty()) return null
        val hostAndPort = withoutScheme.substringBefore('/').substringBefore('?')
        return hostAndPort.substringBefore(':').lowercase()
    }
}
