package com.ruhi.browser

import com.ruhi.browser.model.Shortcut
import org.junit.Assert.assertEquals
import org.junit.Test

class ShortcutModelTest {

    @Test
    fun testShortcutCreation() {
        val shortcut = Shortcut("Google", "https://www.google.com", R.drawable.ic_globe)
        assertEquals("Google", shortcut.title)
        assertEquals("https://www.google.com", shortcut.url)
        assertEquals(R.drawable.ic_globe, shortcut.iconRes)
    }
}
