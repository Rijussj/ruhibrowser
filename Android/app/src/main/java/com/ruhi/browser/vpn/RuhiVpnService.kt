package com.ruhi.browser.vpn

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.net.VpnService
import android.os.Build
import android.os.ParcelFileDescriptor
import androidx.core.app.NotificationCompat
import com.ruhi.browser.R
import com.ruhi.browser.data.preferences.SettingsPreferences
import com.ruhi.browser.ui.vpn.VpnActivity

class RuhiVpnService : VpnService() {

    enum class VpnState {
        DISCONNECTED,
        CONNECTING,
        CONNECTED,
        ERROR
    }

    interface StateListener {
        fun onStateChanged(state: VpnState, detail: String?)
    }

    private var vpnInterface: ParcelFileDescriptor? = null

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        val action = intent?.action
        when (action) {
            ACTION_CONNECT -> {
                connectTunnel()
            }
            ACTION_DISCONNECT -> {
                disconnectTunnel()
            }
        }
        return START_NOT_STICKY
    }

    private fun connectTunnel() {
        updateState(VpnState.CONNECTING, "Establishing tunnel...")

        val preferences = SettingsPreferences(this)
        if (!preferences.isVpnConfigured) {
            updateState(VpnState.ERROR, "Server profile not configured. Please add your server endpoint and public key.")
            stopSelf()
            return
        }

        try {
            startForegroundServiceNotification()

            val clientAddress = preferences.vpnClientAddress.ifBlank { "10.0.0.2" }
            val dnsServer = preferences.vpnDns.ifBlank { "1.1.1.1" }

            val builder = Builder()
                .setSession("RUHI WireGuard Tunnel")
                .addAddress(clientAddress, 24)
                .addDnsServer(dnsServer)
                .addRoute("0.0.0.0", 0)
                .setMtu(1420)

            val pfd = builder.establish()
            if (pfd != null) {
                vpnInterface = pfd
                updateState(VpnState.CONNECTED, "Connected to ${preferences.vpnServerAddress}:${preferences.vpnServerPort}")
            } else {
                updateState(VpnState.ERROR, "System rejected VPN tunnel creation.")
                stopSelf()
            }

        } catch (e: Exception) {
            updateState(VpnState.ERROR, "Failed to establish tunnel: ${e.localizedMessage}")
            stopSelf()
        }
    }

    private fun disconnectTunnel() {
        try {
            vpnInterface?.close()
            vpnInterface = null
        } catch (_: Exception) {}

        stopForeground(STOP_FOREGROUND_REMOVE)
        updateState(VpnState.DISCONNECTED, null)
        stopSelf()
    }

    override fun onDestroy() {
        disconnectTunnel()
        super.onDestroy()
    }

    private fun startForegroundServiceNotification() {
        val channelId = "ruhi_vpn_channel"
        val notificationManager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                channelId,
                "VPN Connection",
                NotificationManager.IMPORTANCE_LOW
            )
            notificationManager.createNotificationChannel(channel)
        }

        val intent = Intent(this, VpnActivity::class.java)
        val pendingIntent = PendingIntent.getActivity(
            this,
            0,
            intent,
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT
        )

        val notification: Notification = NotificationCompat.Builder(this, channelId)
            .setSmallIcon(R.drawable.ic_vpn)
            .setContentTitle("RUHI WireGuard VPN")
            .setContentText("VPN tunnel active")
            .setContentIntent(pendingIntent)
            .setOngoing(true)
            .build()

        startForeground(NOTIFICATION_ID, notification)
    }

    companion object {
        const val ACTION_CONNECT = "com.ruhi.browser.vpn.CONNECT"
        const val ACTION_DISCONNECT = "com.ruhi.browser.vpn.DISCONNECT"
        private const val NOTIFICATION_ID = 2001

        var currentState: VpnState = VpnState.DISCONNECTED
            private set
        var lastStateDetail: String? = null
            private set

        private val listeners = mutableListOf<StateListener>()

        fun addListener(listener: StateListener) {
            if (!listeners.contains(listener)) {
                listeners.add(listener)
            }
            listener.onStateChanged(currentState, lastStateDetail)
        }

        fun removeListener(listener: StateListener) {
            listeners.remove(listener)
        }

        private fun updateState(state: VpnState, detail: String?) {
            currentState = state
            lastStateDetail = detail
            listeners.forEach { it.onStateChanged(state, detail) }
        }

        fun startVpn(context: Context) {
            val intent = Intent(context, RuhiVpnService::class.java).apply {
                action = ACTION_CONNECT
            }
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                context.startForegroundService(intent)
            } else {
                context.startService(intent)
            }
        }

        fun stopVpn(context: Context) {
            val intent = Intent(context, RuhiVpnService::class.java).apply {
                action = ACTION_DISCONNECT
            }
            context.startService(intent)
        }
    }
}
