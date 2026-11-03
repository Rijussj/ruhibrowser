package com.ruhi.browser.data.repository

import com.ruhi.browser.data.local.IBookmarkDao
import com.ruhi.browser.data.model.Bookmark

class BookmarkRepository(private val bookmarkDao: IBookmarkDao) {

    fun addBookmark(title: String, url: String): Long {
        if (url.isBlank()) return -1L
        val cleanTitle = if (title.isNotBlank()) title else url
        return bookmarkDao.insert(Bookmark(title = cleanTitle, url = url))
    }

    fun updateBookmark(bookmark: Bookmark): Boolean {
        return bookmarkDao.update(bookmark) > 0
    }

    fun deleteBookmark(id: Long): Boolean {
        return bookmarkDao.delete(id) > 0
    }

    fun deleteBookmarkByUrl(url: String): Boolean {
        return bookmarkDao.deleteByUrl(url) > 0
    }

    fun isBookmarked(url: String): Boolean {
        if (url.isBlank()) return false
        return bookmarkDao.isBookmarked(url)
    }

    fun getBookmarks(): List<Bookmark> {
        return bookmarkDao.getAll()
    }
}
