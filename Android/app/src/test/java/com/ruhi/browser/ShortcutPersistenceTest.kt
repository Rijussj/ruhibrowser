package com.ruhi.browser

import com.ruhi.browser.model.Shortcut
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test

class ShortcutPersistenceTest {

    @Test
    fun testShortcutCreationAndModification() {
        val list = mutableListOf(
            Shortcut("Google", "https://www.google.com"),
            Shortcut("Wikipedia", "https://www.wikipedia.org"),
            Shortcut("GitHub", "https://www.github.com")
        )

        assertEquals(3, list.size)

        // Add
        list.add(Shortcut("Reddit", "https://www.reddit.com"))
        assertEquals(4, list.size)
        assertEquals("Reddit", list.last().title)

        // Reorder (move last to first)
        val item = list.removeAt(3)
        list.add(0, item)
        assertEquals("Reddit", list.first().title)
        assertEquals("Google", list[1].title)

        // Edit
        list[0] = Shortcut("Reddit News", "https://www.reddit.com/r/news")
        assertEquals("Reddit News", list.first().title)
        assertEquals("https://www.reddit.com/r/news", list.first().url)

        // Remove
        list.removeAt(0)
        assertEquals(3, list.size)
        assertEquals("Google", list.first().title)
    }

    @Test
    fun testShortcutDefaultIconRes() {
        val shortcut = Shortcut("Example", "https://example.com")
        assertTrue(shortcut.iconRes != 0)
        assertEquals(R.drawable.ic_globe, shortcut.iconRes)
    }
}
