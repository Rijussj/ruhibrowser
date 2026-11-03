package com.ruhi.browser.browser

import java.net.URLEncoder
import java.nio.charset.StandardCharsets

object UrlResolver {

    private const val DEFAULT_SEARCH_ENGINE_TEMPLATE = "https://www.google.com/search?q=%s"

    // Matches valid hostnames (e.g. example.com, sub.example.co.uk, localhost:8080, 192.168.1.1)
    private val DOMAIN_REGEX = Regex(
        "^([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\\.)+[a-zA-Z]{2,}(:[0-9]{1,5})?(/.*)?$"
    )
    private val LOCALHOST_REGEX = Regex("^localhost(:[0-9]{1,5})?(/.*)?$", RegexOption.IGNORE_CASE)
    private val IPV4_REGEX = Regex(
        "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])(:[0-9]{1,5})?(/.*)?$"
    )

    /**
     * Resolves a raw address bar input string to either:
     * 1. Direct HTTP/HTTPS navigation URL
     * 2. Prepending https:// for domain-like inputs
     * 3. A securely encoded search query URL using the default search engine
     *
     * Disallows unsafe local schemes like javascript:, file:, content:, data:.
     */
    fun resolve(input: String, searchTemplate: String = DEFAULT_SEARCH_ENGINE_TEMPLATE): String {
        val trimmed = input.trim()
        if (trimmed.isEmpty()) {
            return ""
        }

        // Block or sanitize dangerous local schemes by routing to search
        val lower = trimmed.lowercase()
        if (lower.startsWith("javascript:") ||
            lower.startsWith("file:") ||
            lower.startsWith("content:") ||
            lower.startsWith("data:")
        ) {
            return buildSearchUrl(trimmed, searchTemplate)
        }

        // Explicit web schemes
        if (lower.startsWith("http://") || lower.startsWith("https://")) {
            return trimmed
        }

        // If input contains spaces, it's a search phrase
        if (trimmed.contains(" ")) {
            return buildSearchUrl(trimmed, searchTemplate)
        }

        // Check if input matches domain, localhost, or IPv4 address
        if (DOMAIN_REGEX.matches(trimmed) ||
            LOCALHOST_REGEX.matches(trimmed) ||
            IPV4_REGEX.matches(trimmed)
        ) {
            return "https://$trimmed"
        }

        // Fallback: search query
        return buildSearchUrl(trimmed, searchTemplate)
    }

    private fun buildSearchUrl(query: String, searchTemplate: String): String {
        val encodedQuery = URLEncoder.encode(query, StandardCharsets.UTF_8.name())
        return String.format(searchTemplate, encodedQuery)
    }

    fun isHttps(url: String): Boolean {
        return url.startsWith("https://", ignoreCase = true)
    }

    fun isWebScheme(url: String): Boolean {
        return url.startsWith("http://", ignoreCase = true) || url.startsWith("https://", ignoreCase = true)
    }

    fun isExternalScheme(url: String): Boolean {
        val lower = url.trim().lowercase()
        if (isWebScheme(lower) ||
            lower.startsWith("about:") ||
            lower.startsWith("javascript:") ||
            lower.startsWith("data:") ||
            lower.startsWith("file:")
        ) {
            return false
        }
        val colonIndex = lower.indexOf(':')
        return colonIndex > 1
    }
}
