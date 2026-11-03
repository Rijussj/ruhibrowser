package com.ruhi.browser

import com.ruhi.browser.data.preferences.SettingsPreferences
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class AiAssistantTest {

    @Test
    fun testDefaultEndpointIsValidHttps() {
        val defaultEndpoint = SettingsPreferences.DEFAULT_AI_ENDPOINT
        assertTrue(defaultEndpoint.startsWith("https://"))
        assertTrue(defaultEndpoint.contains("huggingface.co"))
    }

    @Test
    fun testPromptContextTruncation() {
        val longContext = "A".repeat(3000)
        val truncated = if (longContext.length > 1500) {
            longContext.substring(0, 1500) + "…"
        } else {
            longContext
        }

        assertEquals(1501, truncated.length)
        assertTrue(truncated.endsWith("…"))
    }

    @Test
    fun testPromptConstructionWithoutContext() {
        val userPrompt = "What is the capital of France?"
        val includeContext = false
        val pageContext: String? = "Some page context"

        val fullPrompt = if (includeContext && !pageContext.isNullOrBlank()) {
            "Context from current webpage:\n\"\"\"\n$pageContext\n\"\"\"\n\nUser request: $userPrompt"
        } else {
            userPrompt
        }

        assertEquals(userPrompt, fullPrompt)
        assertFalse(fullPrompt.contains("Context from current webpage"))
    }

    @Test
    fun testPromptConstructionWithContext() {
        val userPrompt = "Summarize this page"
        val includeContext = true
        val pageContext = "Kotlin is a modern programming language."

        val fullPrompt = if (includeContext && !pageContext.isNullOrBlank()) {
            "Context from current webpage:\n\"\"\"\n$pageContext\n\"\"\"\n\nUser request: $userPrompt"
        } else {
            userPrompt
        }

        assertTrue(fullPrompt.contains("Context from current webpage"))
        assertTrue(fullPrompt.contains(pageContext))
        assertTrue(fullPrompt.contains(userPrompt))
    }
}
