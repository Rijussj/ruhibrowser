package com.ruhi.browser

import com.ruhi.browser.browser.UrlResolver
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class UrlResolverTest {

    @Test
    fun testEmptyInputReturnsEmpty() {
        assertEquals("", UrlResolver.resolve(""))
        assertEquals("", UrlResolver.resolve("   "))
    }

    @Test
    fun testExplicitHttpAndHttpsUrlsPreserved() {
        assertEquals("https://www.google.com", UrlResolver.resolve("https://www.google.com"))
        assertEquals("http://example.com/test?a=1", UrlResolver.resolve("http://example.com/test?a=1"))
    }

    @Test
    fun testDomainInputsPrependHttps() {
        assertEquals("https://example.com", UrlResolver.resolve("example.com"))
        assertEquals("https://sub.domain.co.uk/path", UrlResolver.resolve("sub.domain.co.uk/path"))
        assertEquals("https://github.com/android", UrlResolver.resolve("github.com/android"))
        assertEquals("https://localhost:8080", UrlResolver.resolve("localhost:8080"))
        assertEquals("https://192.168.1.1:8000/api", UrlResolver.resolve("192.168.1.1:8000/api"))
    }

    @Test
    fun testSearchQueriesProperlyEncoded() {
        assertEquals(
            "https://www.google.com/search?q=kotlin+android+development",
            UrlResolver.resolve("kotlin android development")
        )
        assertEquals(
            "https://www.google.com/search?q=what+is+1%2B1%3F",
            UrlResolver.resolve("what is 1+1?")
        )
    }

    @Test
    fun testSingleWordNonDomainBecomesSearch() {
        assertEquals(
            "https://www.google.com/search?q=antigravity",
            UrlResolver.resolve("antigravity")
        )
    }

    @Test
    fun testDangerousSchemesRoutedToSearch() {
        val resolved = UrlResolver.resolve("javascript:alert('pwned')")
        assertTrue(resolved.startsWith("https://www.google.com/search?q="))
        assertFalse(resolved.startsWith("javascript:"))
    }

    @Test
    fun testSearchEngineTemplates() {
        val query = "hello world"
        val google = com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_TEMPLATES[com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_ENGINE_GOOGLE]!!
        val ddg = com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_TEMPLATES[com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_ENGINE_DUCKDUCKGO]!!
        val bing = com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_TEMPLATES[com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_ENGINE_BING]!!
        val ecosia = com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_TEMPLATES[com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_ENGINE_ECOSIA]!!
        val yahoo = com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_TEMPLATES[com.ruhi.browser.data.preferences.SettingsPreferences.SEARCH_ENGINE_YAHOO]!!

        assertEquals("https://www.google.com/search?q=hello+world", UrlResolver.resolve(query, google))
        assertEquals("https://duckduckgo.com/?q=hello+world", UrlResolver.resolve(query, ddg))
        assertEquals("https://www.bing.com/search?q=hello+world", UrlResolver.resolve(query, bing))
        assertEquals("https://www.ecosia.org/search?q=hello+world", UrlResolver.resolve(query, ecosia))
        assertEquals("https://search.yahoo.com/search?p=hello+world", UrlResolver.resolve(query, yahoo))
    }

    @Test
    fun testIsHttps() {
        assertTrue(UrlResolver.isHttps("https://secure.site.com"))
        assertFalse(UrlResolver.isHttps("http://insecure.site.com"))
    }

    @Test
    fun testIsExternalScheme() {
        assertTrue(UrlResolver.isExternalScheme("tel:+1234567890"))
        assertTrue(UrlResolver.isExternalScheme("mailto:support@ruhi.com"))
        assertFalse(UrlResolver.isExternalScheme("https://ruhi.com"))
    }
}
