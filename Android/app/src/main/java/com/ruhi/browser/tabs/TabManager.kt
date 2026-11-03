package com.ruhi.browser.tabs

class TabManager {

    private val tabs = mutableListOf<Tab>()
    var activeTabId: String? = null
        private set

    val count: Int
        get() = tabs.size

    val regularTabCount: Int
        get() = tabs.count { !it.isIncognito }

    val incognitoTabCount: Int
        get() = tabs.count { it.isIncognito }

    fun getTabs(): List<Tab> = tabs.toList()

    fun getTabs(isIncognito: Boolean): List<Tab> = tabs.filter { it.isIncognito == isIncognito }

    fun getActiveTab(): Tab? = tabs.find { it.id == activeTabId }

    fun getTabById(tabId: String): Tab? = tabs.find { it.id == tabId }

    /**
     * Creates a new tab. If select is true or activeTabId is null, sets it as active.
     */
    fun createTab(
        url: String = "",
        title: String = "New tab",
        isIncognito: Boolean = false,
        select: Boolean = true
    ): Tab {
        val tab = Tab(url = url, title = title, isIncognito = isIncognito)
        tabs.add(tab)
        if (select || activeTabId == null) {
            activeTabId = tab.id
        }
        return tab
    }

    /**
     * Selects a tab by id. Returns the selected tab, or null if not found.
     */
    fun selectTab(tabId: String): Tab? {
        val tab = tabs.find { it.id == tabId }
        if (tab != null) {
            activeTabId = tab.id
        }
        return tab
    }

    /**
     * Closes a tab by id.
     * If the closed tab was active, selects an adjacent tab.
     * If closing the final tab, gracefully creates a new empty regular tab.
     */
    fun closeTab(tabId: String): Tab {
        val index = tabs.indexOfFirst { it.id == tabId }
        if (index == -1) {
            return getActiveTab() ?: createTab()
        }

        val wasActive = (tabId == activeTabId)
        tabs.removeAt(index)

        if (tabs.isEmpty()) {
            return createTab(url = "", title = "New tab", isIncognito = false, select = true)
        }

        if (wasActive) {
            val nextIndex = if (index >= tabs.size) tabs.size - 1 else index
            activeTabId = tabs[nextIndex].id
        }

        return getActiveTab() ?: tabs.first()
    }

    /**
     * Updates active tab URL and title.
     */
    fun updateActiveTab(url: String, title: String) {
        getActiveTab()?.let { tab ->
            tab.url = url
            if (title.isNotBlank()) {
                tab.title = title
            }
        }
    }

    /**
     * Closes all open tabs and resets to a single empty regular tab.
     */
    fun closeAllTabs(): Tab {
        tabs.clear()
        return createTab(url = "", title = "New tab", isIncognito = false, select = true)
    }
}
