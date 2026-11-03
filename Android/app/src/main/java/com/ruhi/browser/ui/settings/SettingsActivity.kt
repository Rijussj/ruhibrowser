package com.ruhi.browser.ui.settings

import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.webkit.CookieManager
import android.webkit.WebStorage
import android.webkit.WebView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.app.AppCompatDelegate
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.ruhi.browser.R
import com.ruhi.browser.blocking.AdBlockManager
import com.ruhi.browser.data.local.HistoryDao
import com.ruhi.browser.data.local.RuhiDatabaseHelper
import com.ruhi.browser.data.preferences.SettingsPreferences
import com.ruhi.browser.data.repository.HistoryRepository
import com.ruhi.browser.databinding.ActivitySettingsBinding
import com.ruhi.browser.ui.downloads.DownloadsActivity

class SettingsActivity : AppCompatActivity() {

    private lateinit var binding: ActivitySettingsBinding
    private lateinit var preferences: SettingsPreferences
    private lateinit var historyRepository: HistoryRepository

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivitySettingsBinding.inflate(layoutInflater)
        setContentView(binding.root)

        preferences = SettingsPreferences(this)
        val dbHelper = RuhiDatabaseHelper(this)
        historyRepository = HistoryRepository(HistoryDao(dbHelper))

        setupToolbar()
        setupGeneralSection()
        setupAppearanceSection()
        setupSearchSection()
        setupPrivacySection()
        setupDownloadsSection()
        setupInformationalSections()
    }

    private val defaultBrowserLauncher = registerForActivityResult(androidx.activity.result.contract.ActivityResultContracts.StartActivityForResult()) {
        updateDefaultBrowserSummary()
    }

    private fun setupGeneralSection() {
        updateDefaultBrowserSummary()
        binding.settingDefaultBrowser.setOnClickListener {
            com.ruhi.browser.util.DefaultBrowserHelper.requestDefaultBrowser(this, defaultBrowserLauncher)
        }
    }

    private fun updateDefaultBrowserSummary() {
        val isDefault = com.ruhi.browser.util.DefaultBrowserHelper.isDefaultBrowser(this)
        binding.tvDefaultBrowserStatus.text = if (isDefault) {
            "RUHI Browser is currently your default browser"
        } else {
            "Tap to set RUHI Browser as your default browser"
        }
    }

    private fun setupToolbar() {
        binding.settingsToolbar.setNavigationOnClickListener {
            finish()
        }
    }

    private fun setupAppearanceSection() {
        updateThemeSummary()

        binding.settingTheme.setOnClickListener {
            val themes = arrayOf(
                getString(R.string.theme_system),
                getString(R.string.theme_light),
                getString(R.string.theme_dark)
            )

            val currentMode = preferences.theme
            val selectedIndex = when (currentMode) {
                SettingsPreferences.THEME_LIGHT -> 1
                SettingsPreferences.THEME_DARK -> 2
                else -> 0
            }

            MaterialAlertDialogBuilder(this)
                .setTitle(getString(R.string.setting_theme_title))
                .setSingleChoiceItems(themes, selectedIndex) { dialog, which ->
                    val newMode = when (which) {
                        1 -> SettingsPreferences.THEME_LIGHT
                        2 -> SettingsPreferences.THEME_DARK
                        else -> SettingsPreferences.THEME_SYSTEM
                    }
                    preferences.theme = newMode
                    AppCompatDelegate.setDefaultNightMode(newMode)
                    updateThemeSummary()
                    dialog.dismiss()
                }
                .setNegativeButton(android.R.string.cancel, null)
                .show()
        }
    }

    private fun updateThemeSummary() {
        val summary = when (preferences.theme) {
            SettingsPreferences.THEME_LIGHT -> getString(R.string.theme_light)
            SettingsPreferences.THEME_DARK -> getString(R.string.theme_dark)
            else -> getString(R.string.theme_system)
        }
        binding.tvCurrentTheme.text = summary
    }

    private fun setupSearchSection() {
        updateSearchEngineSummary()

        binding.settingSearchEngine.setOnClickListener {
            val engines = arrayOf(
                SettingsPreferences.SEARCH_ENGINE_GOOGLE,
                SettingsPreferences.SEARCH_ENGINE_DUCKDUCKGO,
                SettingsPreferences.SEARCH_ENGINE_BING,
                SettingsPreferences.SEARCH_ENGINE_ECOSIA,
                SettingsPreferences.SEARCH_ENGINE_YAHOO
            )

            val selectedIndex = engines.indexOf(preferences.searchEngine).coerceAtLeast(0)

            MaterialAlertDialogBuilder(this)
                .setTitle(getString(R.string.setting_search_engine_title))
                .setSingleChoiceItems(engines, selectedIndex) { dialog, which ->
                    preferences.searchEngine = engines[which]
                    updateSearchEngineSummary()
                    dialog.dismiss()
                }
                .setNegativeButton(android.R.string.cancel, null)
                .show()
        }
    }

    private fun updateSearchEngineSummary() {
        binding.tvCurrentSearchEngine.text = "${preferences.searchEngine} (Default)"
    }

    private fun setupPrivacySection() {
        updateAdblockSummary()

        binding.settingAdblock.setOnClickListener {
            val isCurrentlyEnabled = preferences.adBlockEnabled
            val newStatus = !isCurrentlyEnabled
            preferences.adBlockEnabled = newStatus
            AdBlockManager.isEnabled = newStatus
            updateAdblockSummary()
            val statusMsg = if (newStatus) "Ad & tracker filtering enabled" else "Ad & tracker filtering disabled"
            Toast.makeText(this, statusMsg, Toast.LENGTH_SHORT).show()
        }

        binding.settingClearData.setOnClickListener {
            showClearBrowsingDataDialog()
        }
    }

    private fun updateAdblockSummary() {
        val statusText = if (preferences.adBlockEnabled) "Enabled (Best-effort request filtering)" else "Disabled"
        val descView = binding.settingAdblock.getChildAt(1) as? android.widget.TextView
        descView?.text = statusText
    }

    private fun showClearBrowsingDataDialog() {
        val options = arrayOf(
            "Browsing history",
            "Cookies and site data",
            "Cached images and files"
        )
        val checkedItems = booleanArrayOf(true, true, true)

        MaterialAlertDialogBuilder(this)
            .setTitle(getString(R.string.setting_clear_data_title))
            .setMultiChoiceItems(options, checkedItems) { _, which, isChecked ->
                checkedItems[which] = isChecked
            }
            .setPositiveButton("Clear Data") { _, _ ->
                if (checkedItems[0]) {
                    historyRepository.clearAllHistory()
                }
                if (checkedItems[1]) {
                    CookieManager.getInstance().removeAllCookies(null)
                    WebStorage.getInstance().deleteAllData()
                }
                if (checkedItems[2]) {
                    WebView(this).clearCache(true)
                }
                Toast.makeText(this, "Selected browsing data cleared", Toast.LENGTH_SHORT).show()
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    private fun setupDownloadsSection() {
        binding.settingDownloads.setOnClickListener {
            startActivity(Intent(this, DownloadsActivity::class.java))
        }
    }

    override fun onResume() {
        super.onResume()
        updateDefaultBrowserSummary()
        updateAiSummary()
        updateVpnSummary()
    }

    private fun updateAiSummary() {
        val descView = binding.settingAi.getChildAt(1) as? android.widget.TextView
        val status = if (preferences.aiApiKey.isNotBlank()) "Configured (Hugging Face)" else "Token required"
        descView?.text = status
    }

    private fun updateVpnSummary() {
        val descView = binding.settingVpn.getChildAt(1) as? android.widget.TextView
        val status = when (com.ruhi.browser.vpn.RuhiVpnService.currentState) {
            com.ruhi.browser.vpn.RuhiVpnService.VpnState.CONNECTED -> "Connected (WireGuard tunnel active)"
            com.ruhi.browser.vpn.RuhiVpnService.VpnState.CONNECTING -> "Connecting..."
            com.ruhi.browser.vpn.RuhiVpnService.VpnState.ERROR -> "Error (Check server settings)"
            com.ruhi.browser.vpn.RuhiVpnService.VpnState.DISCONNECTED -> {
                if (preferences.isVpnConfigured) "Configured (Disconnected)" else "Configuration required"
            }
        }
        descView?.text = status
    }

    private fun setupInformationalSections() {
        updateAiSummary()
        updateVpnSummary()

        binding.settingAi.setOnClickListener {
            showEditAiDialog()
        }

        binding.settingVpn.setOnClickListener {
            val intent = Intent(this, com.ruhi.browser.ui.vpn.VpnActivity::class.java)
            startActivity(intent)
        }

        binding.settingAboutApp.setOnClickListener {
            showAboutDialog()
        }

        binding.settingVersion.setOnClickListener {
            showAboutDialog()
        }

        binding.settingPrivacyPolicy.setOnClickListener {
            showPrivacyPolicyDialog()
        }

        binding.settingLicenses.setOnClickListener {
            showLicensesDialog()
        }
    }

    private fun showAboutDialog() {
        val dialogView = layoutInflater.inflate(R.layout.dialog_about, null)
        MaterialAlertDialogBuilder(this)
            .setView(dialogView)
            .setPositiveButton("OK", null)
            .show()
    }

    private fun showPrivacyPolicyDialog() {
        val dialogView = layoutInflater.inflate(R.layout.dialog_privacy_policy, null)
        MaterialAlertDialogBuilder(this)
            .setView(dialogView)
            .setPositiveButton("Close", null)
            .show()
    }

    private fun showLicensesDialog() {
        val dialogView = layoutInflater.inflate(R.layout.dialog_licenses, null)

        dialogView.findViewById<android.view.View>(R.id.licenseKotlin)?.setOnClickListener {
            openUrl("https://github.com/JetBrains/kotlin")
        }
        dialogView.findViewById<android.view.View>(R.id.licenseAndroidX)?.setOnClickListener {
            openUrl("https://github.com/androidx/androidx")
        }
        dialogView.findViewById<android.view.View>(R.id.licenseMaterial)?.setOnClickListener {
            openUrl("https://github.com/material-components/material-components-android")
        }
        dialogView.findViewById<android.view.View>(R.id.licenseChromium)?.setOnClickListener {
            openUrl("https://chromium.googlesource.com/chromium/src/")
        }
        dialogView.findViewById<android.view.View>(R.id.licenseWireGuard)?.setOnClickListener {
            openUrl("https://git.zx2c4.com/wireguard-android/")
        }
        dialogView.findViewById<android.view.View>(R.id.licenseOkHttp)?.setOnClickListener {
            openUrl("https://square.github.io/okhttp/")
        }

        MaterialAlertDialogBuilder(this)
            .setView(dialogView)
            .setPositiveButton("Close", null)
            .show()
    }

    private fun openUrl(url: String) {
        try {
            val intent = Intent(Intent.ACTION_VIEW, Uri.parse(url))
            startActivity(intent)
        } catch (e: Exception) {
            Toast.makeText(this, "Unable to open link", Toast.LENGTH_SHORT).show()
        }
    }

    private fun showEditAiDialog() {
        val dialogView = layoutInflater.inflate(R.layout.dialog_edit_ai_settings, null)
        val etToken = dialogView.findViewById<com.google.android.material.textfield.TextInputEditText>(R.id.etAiToken)
        val etEndpoint = dialogView.findViewById<com.google.android.material.textfield.TextInputEditText>(R.id.etAiEndpoint)
        val cbEnabled = dialogView.findViewById<com.google.android.material.checkbox.MaterialCheckBox>(R.id.cbAiEnabled)

        etToken.setText(preferences.aiApiKey)
        etEndpoint.setText(preferences.aiModelEndpoint)
        cbEnabled.isChecked = preferences.aiEnabled

        MaterialAlertDialogBuilder(this)
            .setTitle(R.string.setting_ai_title)
            .setView(dialogView)
            .setPositiveButton(R.string.action_save) { dialog, _ ->
                val token = etToken.text?.toString()?.trim().orEmpty()
                val endpoint = etEndpoint.text?.toString()?.trim().orEmpty()
                preferences.aiApiKey = token
                preferences.aiModelEndpoint = endpoint.ifEmpty { SettingsPreferences.DEFAULT_AI_ENDPOINT }
                preferences.aiEnabled = cbEnabled.isChecked
                updateAiSummary()
                Toast.makeText(this, "AI settings saved", Toast.LENGTH_SHORT).show()
                dialog.dismiss()
            }
            .setNegativeButton(R.string.action_cancel, null)
            .show()
    }
}
