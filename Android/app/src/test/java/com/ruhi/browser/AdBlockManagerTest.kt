package com.ruhi.browser

import com.ruhi.browser.blocking.AdBlockManager
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class AdBlockManagerTest {

    @Before
    fun setUp() {
        AdBlockManager.isEnabled = true
        AdBlockManager.setAllowlisted("doubleclick.net", false)
    }

    @Test
    fun testBlockKnownTrackerSubResource() {
        assertTrue(AdBlockManager.shouldBlockUrl("https://doubleclick.net/ad.js", isForMainFrame = false))
        assertTrue(AdBlockManager.shouldBlockUrl("https://adservice.google.com/ads", isForMainFrame = false))
        assertTrue(AdBlockManager.shouldBlockUrl("https://subdomain.criteo.com/track", isForMainFrame = false))
    }

    @Test
    fun testNeverBlockMainFrameNavigation() {
        // Critical safety: never block top-level user navigation even if domain is in filter
        assertFalse(AdBlockManager.shouldBlockUrl("https://doubleclick.net", isForMainFrame = true))
        assertFalse(AdBlockManager.shouldBlockUrl("https://adservice.google.com", isForMainFrame = true))
    }

    @Test
    fun testAllowLegitimateDomains() {
        assertFalse(AdBlockManager.shouldBlockUrl("https://google.com/search?q=test", isForMainFrame = false))
        assertFalse(AdBlockManager.shouldBlockUrl("https://wikipedia.org/wiki/Android", isForMainFrame = false))
        assertFalse(AdBlockManager.shouldBlockUrl("https://github.com", isForMainFrame = false))
    }

    @Test
    fun testAllowlistBypassesBlocking() {
        assertTrue(AdBlockManager.shouldBlockUrl("https://outbrain.com/widget", isForMainFrame = false))

        AdBlockManager.setAllowlisted("outbrain.com", true)
        assertFalse(AdBlockManager.shouldBlockUrl("https://outbrain.com/widget", isForMainFrame = false))

        AdBlockManager.setAllowlisted("outbrain.com", false)
        assertTrue(AdBlockManager.shouldBlockUrl("https://outbrain.com/widget", isForMainFrame = false))
    }

    @Test
    fun testToggleDisableStopsBlocking() {
        AdBlockManager.isEnabled = false
        assertFalse(AdBlockManager.shouldBlockUrl("https://doubleclick.net/ad.js", isForMainFrame = false))
    }
}
