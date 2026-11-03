package com.ruhi.browser.data.repository

import com.ruhi.browser.data.local.IHistoryDao
import com.ruhi.browser.data.model.HistoryItem

class HistoryRepository(private val historyDao: IHistoryDao) {

    /**
     * Records a browsing visit.
     * CRITICAL PRIVACY REQUIREMENT:
     * Incognito visits must NEVER be recorded in persistent history.
     */
    fun recordVisit(url: String, title: String, isIncognito: Boolean): Long {
        if (isIncognito || url.isBlank()) {
            return -1L
        }
        return historyDao.recordVisit(url = url, title = title, visitedAt = System.currentTimeMillis())
    }

    fun getHistory(): List<HistoryItem> {
        return historyDao.getAll()
    }

    fun deleteItem(id: Long): Boolean {
        return historyDao.delete(id) > 0
    }

    fun deleteByUrl(url: String): Boolean {
        return historyDao.deleteByUrl(url) > 0
    }

    fun clearAllHistory(): Boolean {
        return historyDao.clearAll() >= 0
    }
}
