package com.ruhi.browser

import com.ruhi.browser.data.local.IBookmarkDao
import com.ruhi.browser.data.model.Bookmark
import com.ruhi.browser.data.repository.BookmarkRepository
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class BookmarkRepositoryTest {

    private class FakeBookmarkDao : IBookmarkDao {
        private val list = mutableListOf<Bookmark>()
        private var idCounter = 1L

        override fun insert(bookmark: Bookmark): Long {
            val id = idCounter++
            list.add(bookmark.copy(id = id))
            return id
        }

        override fun update(bookmark: Bookmark): Int {
            val idx = list.indexOfFirst { it.id == bookmark.id }
            if (idx != -1) {
                list[idx] = bookmark
                return 1
            }
            return 0
        }

        override fun delete(id: Long): Int {
            return if (list.removeIf { it.id == id }) 1 else 0
        }

        override fun deleteByUrl(url: String): Int {
            return if (list.removeIf { it.url == url }) 1 else 0
        }

        override fun isBookmarked(url: String): Boolean {
            return list.any { it.url == url }
        }

        override fun getAll(): List<Bookmark> {
            return list.toList()
        }
    }

    private lateinit var repository: BookmarkRepository
    private lateinit var fakeDao: FakeBookmarkDao

    @Before
    fun setUp() {
        fakeDao = FakeBookmarkDao()
        repository = BookmarkRepository(fakeDao)
    }

    @Test
    fun testAddAndGetBookmarks() {
        val id1 = repository.addBookmark("Google", "https://google.com")
        val id2 = repository.addBookmark("GitHub", "https://github.com")

        assertTrue(id1 > 0)
        assertTrue(id2 > 0)

        val bookmarks = repository.getBookmarks()
        assertEquals(2, bookmarks.size)
        assertTrue(repository.isBookmarked("https://google.com"))
        assertTrue(repository.isBookmarked("https://github.com"))
        assertFalse(repository.isBookmarked("https://unknown.com"))
    }

    @Test
    fun testUpdateBookmark() {
        val id = repository.addBookmark("Old Title", "https://google.com")
        val updated = repository.updateBookmark(Bookmark(id = id, title = "New Title", url = "https://google.com"))

        assertTrue(updated)
        val list = repository.getBookmarks()
        assertEquals("New Title", list.first().title)
    }

    @Test
    fun testDeleteBookmark() {
        val id = repository.addBookmark("Google", "https://google.com")
        assertTrue(repository.isBookmarked("https://google.com"))

        val deleted = repository.deleteBookmark(id)
        assertTrue(deleted)
        assertFalse(repository.isBookmarked("https://google.com"))
        assertEquals(0, repository.getBookmarks().size)
    }

    @Test
    fun testDeleteByUrl() {
        repository.addBookmark("Google", "https://google.com")
        assertTrue(repository.isBookmarked("https://google.com"))

        val deleted = repository.deleteBookmarkByUrl("https://google.com")
        assertTrue(deleted)
        assertFalse(repository.isBookmarked("https://google.com"))
    }

    @Test
    fun testEmptyUrlRejected() {
        val result = repository.addBookmark("Invalid", "")
        assertEquals(-1L, result)
        assertFalse(repository.isBookmarked(""))
    }
}
