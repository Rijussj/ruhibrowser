package com.ruhi.browser.data.model

data class Bookmark(
    val id: Long = 0L,
    val title: String,
    val url: String,
    val createdAt: Long = System.currentTimeMillis(),
    val position: Int = 0
)
