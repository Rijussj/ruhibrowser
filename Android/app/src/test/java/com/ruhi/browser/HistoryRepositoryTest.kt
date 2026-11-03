package com.ruhi.browser

import com.ruhi.browser.data.local.IHistoryDao
import com.ruhi.browser.data.model.HistoryItem
import com.ruhi.browser.data.repository.HistoryRepository
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Before
import org.junit.Test

class HistoryRepositoryTest {

    private class FakeHistoryDao : IHistoryDao {
        private val list = mutableListOf<HistoryItem>()
        private var idCounter = 1L

        override fun recordVisit(url: String, title: String, visitedAt: Long): Long {
            val existingIdx = list.indexOfFirst { it.url == url }
            return if (existingIdx != -1) {
                val existing = list[existingIdx]
                list[existingIdx] = existing.copy(
                    title = if (title.isNotBlank()) title else existing.title,
                    visitedAt = visitedAt,
                    visitCount = existing.visitCount + 1
                )
                existing.id
            } else {
                val id = idCounter++
                list.add(HistoryItem(id, title, url, visitedAt, 1))
                id
            }
        }

        override fun getAll(): List<HistoryItem> {
            return list.toList()
        }

        override fun delete(id: Long): Int {
            return if (list.removeIf { it.id == id }) 1 else 0
        }

        override fun clearAll(): Int {
            val size = list.size
            list.clear()
            return size
        }
    }

    private lateinit var repository: HistoryRepository
    private lateinit var fakeDao: FakeHistoryDao

    @Before
    fun setUp() {
        fakeDao = FakeHistoryDao()
        repository = HistoryRepository(fakeDao)
    }

    @Test
    fun testRecordNormalVisit() {
        val id = repository.recordVisit("https://google.com", "Google", isIncognito = false)
        assertTrue(id > 0)

        val history = repository.getHistory()
        assertEquals(1, history.size)
        assertEquals("https://google.com", history.first().url)
        assertEquals("Google", history.first().title)
    }

    @Test
    fun testIncognitoVisitsNeverRecorded() {
        // CRITICAL PRIVACY REQUIREMENT TEST
        val id = repository.recordVisit("https://secret.com", "Secret Site", isIncognito = true)
        assertEquals(-1L, id)

        val history = repository.getHistory()
        assertEquals(0, history.size)
    }

    @Test
    fun testDeleteHistoryItem() {
        val id1 = repository.recordVisit("https://site1.com", "Site 1", isIncognito = false)
        val id2 = repository.recordVisit("https://site2.com", "Site 2", isIncognito = false)
        assertEquals(2, repository.getHistory().size)

        val deleted = repository.deleteItem(id1)
        assertTrue(deleted)

        val remaining = repository.getHistory()
        assertEquals(1, remaining.size)
        assertEquals(id2, remaining.first().id)
    }

    @Test
    fun testClearAllHistory() {
        repository.recordVisit("https://site1.com", "Site 1", isIncognito = false)
        repository.recordVisit("https://site2.com", "Site 2", isIncognito = false)
        assertEquals(2, repository.getHistory().size)

        val cleared = repository.clearAllHistory()
        assertTrue(cleared)
        assertEquals(0, repository.getHistory().size)
    }

    @Test
    fun testEmptyUrlNotRecorded() {
        val result = repository.recordVisit("", "Empty", isIncognito = false)
        assertEquals(-1L, result)
        assertEquals(0, repository.getHistory().size)
    }
}
