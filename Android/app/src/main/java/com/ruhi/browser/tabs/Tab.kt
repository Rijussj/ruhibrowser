package com.ruhi.browser.tabs

import java.util.UUID

data class Tab(
    val id: String = UUID.randomUUID().toString(),
    var url: String = "",
    var title: String = "New tab",
    val isIncognito: Boolean = false,
    var isDesktopSite: Boolean = false,
    val createdAt: Long = System.currentTimeMillis()
)
