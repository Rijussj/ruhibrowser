package com.ruhi.browser.ui.vpn

import android.app.Activity
import android.content.Intent
import android.net.VpnService
import android.os.Bundle
import android.widget.Toast
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.google.android.material.textfield.TextInputEditText
import com.ruhi.browser.R
import com.ruhi.browser.data.preferences.SettingsPreferences
import com.ruhi.browser.databinding.ActivityVpnBinding
import com.ruhi.browser.vpn.RuhiVpnService

class VpnActivity : AppCompatActivity(), RuhiVpnService.StateListener {

    private lateinit var binding: ActivityVpnBinding
    private lateinit var preferences: SettingsPreferences

    private val vpnConsentLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) {
            startVpnTunnel()
        } else {
            Toast.makeText(this, "VPN permission was denied", Toast.LENGTH_SHORT).show()
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityVpnBinding.inflate(layoutInflater)
        setContentView(binding.root)

        preferences = SettingsPreferences(this)

        setupToolbar()
        updateProfileSummary()
        setupListeners()
    }

    override fun onStart() {
        super.onStart()
        RuhiVpnService.addListener(this)
    }

    override fun onStop() {
        RuhiVpnService.removeListener(this)
        super.onStop()
    }

    private fun setupToolbar() {
        binding.vpnToolbar.setNavigationOnClickListener {
            finish()
        }
    }

    private fun setupListeners() {
        binding.btnToggleVpn.setOnClickListener {
            when (RuhiVpnService.currentState) {
                RuhiVpnService.VpnState.CONNECTED,
                RuhiVpnService.VpnState.CONNECTING -> {
                    RuhiVpnService.stopVpn(this)
                }
                RuhiVpnService.VpnState.DISCONNECTED,
                RuhiVpnService.VpnState.ERROR -> {
                    prepareAndConnectVpn()
                }
            }
        }

        binding.btnEditProfile.setOnClickListener {
            showEditProfileDialog()
        }
    }

    private fun prepareAndConnectVpn() {
        if (!preferences.isVpnConfigured) {
            Toast.makeText(this, R.string.vpn_not_configured_msg, Toast.LENGTH_LONG).show()
            showEditProfileDialog()
            return
        }

        val prepareIntent = VpnService.prepare(this)
        if (prepareIntent != null) {
            vpnConsentLauncher.launch(prepareIntent)
        } else {
            startVpnTunnel()
        }
    }

    private fun startVpnTunnel() {
        RuhiVpnService.startVpn(this)
    }

    override fun onStateChanged(state: RuhiVpnService.VpnState, detail: String?) {
        runOnUiThread {
            when (state) {
                RuhiVpnService.VpnState.DISCONNECTED -> {
                    binding.tvVpnStatusTitle.text = getString(R.string.vpn_status_disconnected)
                    binding.tvVpnStatusDetail.text = detail ?: "Tap connect to establish tunnel"
                    binding.ivVpnStatusIcon.setImageResource(R.drawable.ic_vpn)
                    binding.ivVpnStatusIcon.setColorFilter(getColor(R.color.state_disabled))
                    binding.btnToggleVpn.text = getString(R.string.vpn_connect)
                    binding.btnToggleVpn.isEnabled = true
                }
                RuhiVpnService.VpnState.CONNECTING -> {
                    binding.tvVpnStatusTitle.text = getString(R.string.vpn_status_connecting)
                    binding.tvVpnStatusDetail.text = detail ?: "Contacting server..."
                    binding.ivVpnStatusIcon.setImageResource(R.drawable.ic_vpn)
                    binding.ivVpnStatusIcon.setColorFilter(getColor(R.color.brand_primary))
                    binding.btnToggleVpn.text = getString(R.string.action_stop)
                    binding.btnToggleVpn.isEnabled = true
                }
                RuhiVpnService.VpnState.CONNECTED -> {
                    binding.tvVpnStatusTitle.text = getString(R.string.vpn_status_connected)
                    binding.tvVpnStatusDetail.text = detail ?: "Traffic is protected"
                    binding.ivVpnStatusIcon.setImageResource(R.drawable.ic_check)
                    binding.ivVpnStatusIcon.setColorFilter(getColor(R.color.brand_primary))
                    binding.btnToggleVpn.text = getString(R.string.vpn_disconnect)
                    binding.btnToggleVpn.isEnabled = true
                }
                RuhiVpnService.VpnState.ERROR -> {
                    binding.tvVpnStatusTitle.text = getString(R.string.vpn_status_error)
                    binding.tvVpnStatusDetail.text = detail ?: "Check server configuration"
                    binding.ivVpnStatusIcon.setImageResource(R.drawable.ic_vpn)
                    binding.ivVpnStatusIcon.setColorFilter(getColor(R.color.state_error))
                    binding.btnToggleVpn.text = getString(R.string.action_retry)
                    binding.btnToggleVpn.isEnabled = true
                }
            }
        }
    }

    private fun updateProfileSummary() {
        if (preferences.isVpnConfigured) {
            val keyStatus = if (preferences.vpnPrivateKey.isNotBlank()) "Configured" else "Public key only"
            binding.tvProfileSummary.text = "Endpoint: ${preferences.vpnServerAddress}:${preferences.vpnServerPort}\n" +
                    "Client IP: ${preferences.vpnClientAddress} | DNS: ${preferences.vpnDns}\n" +
                    "WireGuard Keys: $keyStatus"
        } else {
            binding.tvProfileSummary.text = "No server profile configured.\nConfigure server endpoint and keys to enable VPN."
        }
    }

    private fun showEditProfileDialog() {
        val dialogView = layoutInflater.inflate(R.layout.dialog_edit_vpn_profile, null)
        val etServer = dialogView.findViewById<TextInputEditText>(R.id.etVpnServer)
        val etPort = dialogView.findViewById<TextInputEditText>(R.id.etVpnPort)
        val etClientIp = dialogView.findViewById<TextInputEditText>(R.id.etVpnClientIp)
        val etDns = dialogView.findViewById<TextInputEditText>(R.id.etVpnDns)
        val etPublicKey = dialogView.findViewById<TextInputEditText>(R.id.etVpnPublicKey)
        val etPrivateKey = dialogView.findViewById<TextInputEditText>(R.id.etVpnPrivateKey)

        etServer.setText(preferences.vpnServerAddress)
        etPort.setText(preferences.vpnServerPort.toString())
        etClientIp.setText(preferences.vpnClientAddress)
        etDns.setText(preferences.vpnDns)
        etPublicKey.setText(preferences.vpnPublicKey)
        etPrivateKey.setText(preferences.vpnPrivateKey)

        MaterialAlertDialogBuilder(this)
            .setTitle(R.string.vpn_edit_profile)
            .setView(dialogView)
            .setPositiveButton(R.string.action_save) { dialog, _ ->
                val server = etServer.text?.toString()?.trim().orEmpty()
                val portStr = etPort.text?.toString()?.trim().orEmpty()
                val clientIp = etClientIp.text?.toString()?.trim().orEmpty()
                val dns = etDns.text?.toString()?.trim().orEmpty()
                val pubKey = etPublicKey.text?.toString()?.trim().orEmpty()
                val privKey = etPrivateKey.text?.toString()?.trim().orEmpty()

                preferences.vpnServerAddress = server
                preferences.vpnServerPort = portStr.toIntOrNull() ?: 51820
                if (clientIp.isNotBlank()) preferences.vpnClientAddress = clientIp
                if (dns.isNotBlank()) preferences.vpnDns = dns
                preferences.vpnPublicKey = pubKey
                preferences.vpnPrivateKey = privKey

                updateProfileSummary()
                Toast.makeText(this, "Profile saved", Toast.LENGTH_SHORT).show()
                dialog.dismiss()
            }
            .setNegativeButton(R.string.action_cancel, null)
            .show()
    }
}
