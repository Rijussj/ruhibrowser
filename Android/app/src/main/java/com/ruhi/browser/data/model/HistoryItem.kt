package com.ruhi.browser.data.model

data class HistoryItem(
    val id: Long = 0L,
    val title: String,
    val url: String,
    val visitedAt: Long = System.currentTimeMillis(),
    val visitCount: Int = 1
)
