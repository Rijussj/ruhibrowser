package com.ruhi.browser.model

data class NewsArticle(
    val title: String,
    val description: String,
    val source: String,
    val timeAgo: String,
    val category: String,
    val url: String,
    val emoji: String = "📰"
)
