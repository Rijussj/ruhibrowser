package com.ruhi.browser

import com.ruhi.browser.tabs.TabManager
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class TabManagerTest {

    private lateinit var tabManager: TabManager

    @Before
    fun setUp() {
        tabManager = TabManager()
    }

    @Test
    fun testInitialTabCreation() {
        val tab = tabManager.createTab(url = "https://google.com", title = "Google")
        assertEquals(1, tabManager.count)
        assertEquals(tab.id, tabManager.activeTabId)
        assertEquals("https://google.com", tabManager.getActiveTab()?.url)
        assertEquals("Google", tabManager.getActiveTab()?.title)
    }

    @Test
    fun testMultipleTabsAndSwitching() {
        val tab1 = tabManager.createTab(url = "https://site1.com", title = "Site 1")
        val tab2 = tabManager.createTab(url = "https://site2.com", title = "Site 2")
        val tab3 = tabManager.createTab(url = "https://site3.com", title = "Site 3", select = false)

        assertEquals(3, tabManager.count)
        assertEquals(tab2.id, tabManager.activeTabId)

        tabManager.selectTab(tab1.id)
        assertEquals(tab1.id, tabManager.activeTabId)

        tabManager.selectTab(tab3.id)
        assertEquals(tab3.id, tabManager.activeTabId)
    }

    @Test
    fun testCloseInactiveTab() {
        val tab1 = tabManager.createTab(url = "https://site1.com")
        val tab2 = tabManager.createTab(url = "https://site2.com")

        assertEquals(tab2.id, tabManager.activeTabId)
        tabManager.closeTab(tab1.id)

        assertEquals(1, tabManager.count)
        assertEquals(tab2.id, tabManager.activeTabId)
    }

    @Test
    fun testCloseActiveTabSwitchesToAdjacent() {
        val tab1 = tabManager.createTab(url = "https://site1.com")
        val tab2 = tabManager.createTab(url = "https://site2.com")
        val tab3 = tabManager.createTab(url = "https://site3.com")

        assertEquals(tab3.id, tabManager.activeTabId)
        tabManager.closeTab(tab3.id)

        assertEquals(2, tabManager.count)
        assertEquals(tab2.id, tabManager.activeTabId)

        tabManager.selectTab(tab1.id)
        tabManager.closeTab(tab1.id)

        assertEquals(1, tabManager.count)
        assertEquals(tab2.id, tabManager.activeTabId)
    }

    @Test
    fun testClosingLastTabGracefullyLeavesNewTab() {
        val tab = tabManager.createTab(url = "https://lastsite.com")
        assertEquals(1, tabManager.count)

        val newTab = tabManager.closeTab(tab.id)
        assertNotNull(newTab)
        assertEquals(1, tabManager.count)
        assertEquals(newTab.id, tabManager.activeTabId)
        assertEquals("", newTab.url)
    }

    @Test
    fun testIncognitoTabSeparation() {
        tabManager.createTab(url = "https://regular.com", isIncognito = false)
        tabManager.createTab(url = "https://secret.com", isIncognito = true)

        assertEquals(2, tabManager.count)
        assertEquals(1, tabManager.regularTabCount)
        assertEquals(1, tabManager.incognitoTabCount)

        val incognitoTabs = tabManager.getTabs(isIncognito = true)
        assertEquals(1, incognitoTabs.size)
        assertTrue(incognitoTabs.first().isIncognito)

        val regularTabs = tabManager.getTabs(isIncognito = false)
        assertEquals(1, regularTabs.size)
        assertFalse(regularTabs.first().isIncognito)
    }

    @Test
    fun testUpdateActiveTab() {
        tabManager.createTab(url = "", title = "New tab")
        tabManager.updateActiveTab("https://ruhi.com", "RUHI")

        assertEquals("https://ruhi.com", tabManager.getActiveTab()?.url)
        assertEquals("RUHI", tabManager.getActiveTab()?.title)
    }
}
