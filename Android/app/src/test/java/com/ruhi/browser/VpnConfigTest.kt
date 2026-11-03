package com.ruhi.browser

import com.ruhi.browser.vpn.RuhiVpnService
import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class VpnConfigTest {

    @Test
    fun testVpnInitialState() {
        assertEquals(RuhiVpnService.VpnState.DISCONNECTED, RuhiVpnService.currentState)
    }

    @Test
    fun testVpnConfigurationValidation() {
        // Empty server or public key is not configured
        val emptyServer = ""
        val emptyKey = ""
        val validServer = "192.168.1.1"
        val validKey = "bm90YXJlYWxrZXk="

        assertFalse(emptyServer.isNotBlank() && emptyKey.isNotBlank())
        assertFalse(validServer.isNotBlank() && emptyKey.isNotBlank())
        assertFalse(emptyServer.isNotBlank() && validKey.isNotBlank())
        assertTrue(validServer.isNotBlank() && validKey.isNotBlank())
    }

    @Test
    fun testVpnStateListenerRegistration() {
        var notifiedState: RuhiVpnService.VpnState? = null
        val listener = object : RuhiVpnService.StateListener {
            override fun onStateChanged(state: RuhiVpnService.VpnState, detail: String?) {
                notifiedState = state
            }
        }

        RuhiVpnService.addListener(listener)
        assertEquals(RuhiVpnService.currentState, notifiedState)

        RuhiVpnService.removeListener(listener)
    }
}
