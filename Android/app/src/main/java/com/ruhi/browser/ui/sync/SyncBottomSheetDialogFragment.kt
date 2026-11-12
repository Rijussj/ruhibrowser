package com.ruhi.browser.ui.sync

import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import com.google.android.material.bottomsheet.BottomSheetDialogFragment
import com.ruhi.browser.MainActivity
import com.ruhi.browser.R
import com.ruhi.browser.data.local.BookmarkDao
import com.ruhi.browser.data.local.RuhiDatabaseHelper
import com.ruhi.browser.data.repository.BookmarkRepository
import com.ruhi.browser.databinding.DialogSyncAccountBinding
import com.ruhi.browser.sync.SyncManager

class SyncBottomSheetDialogFragment : BottomSheetDialogFragment() {

    private var _binding: DialogSyncAccountBinding? = null
    private val binding get() = _binding!!
    private lateinit var syncManager: SyncManager

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View {
        _binding = DialogSyncAccountBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        syncManager = SyncManager.getInstance(requireContext())

        refreshUi()
        if (syncManager.isLoggedIn) {
            syncManager.refreshUserProfile {
                activity?.runOnUiThread { refreshUi() }
            }
        }

        binding.btnSyncLogin.setOnClickListener {
            val email = binding.etSyncEmail.text.toString().trim()
            val pass = binding.etSyncPassword.text.toString().trim()
            if (email.isEmpty() || pass.isEmpty()) {
                showError("Please enter email and password.")
                return@setOnClickListener
            }
            binding.btnSyncLogin.isEnabled = false
            syncManager.login(email, pass) { success, msg ->
                binding.btnSyncLogin.isEnabled = true
                if (success) {
                    Toast.makeText(requireContext(), "Logged in to RUHI Sync!", Toast.LENGTH_SHORT).show()
                    performSync()
                    refreshUi()
                } else {
                    showError(msg)
                }
            }
        }

        binding.btnSyncSignUp.setOnClickListener {
            val name = binding.etSyncName.text.toString().trim()
            val email = binding.etSyncEmail.text.toString().trim()
            val pass = binding.etSyncPassword.text.toString().trim()
            if (email.isEmpty() || pass.isEmpty()) {
                showError("Please enter email and password.")
                return@setOnClickListener
            }
            binding.btnSyncSignUp.isEnabled = false
            syncManager.signUp(email, pass, name) { success, msg ->
                binding.btnSyncSignUp.isEnabled = true
                if (success) {
                    Toast.makeText(requireContext(), "Account created & synced!", Toast.LENGTH_SHORT).show()
                    performSync()
                    refreshUi()
                } else {
                    showError(msg)
                }
            }
        }

        binding.btnSaveName.setOnClickListener {
            val newName = binding.etDashboardName.text.toString().trim()
            syncManager.updateDisplayName(newName) { ok ->
                refreshUi()
                val msg = if (ok) "Account name saved to Supabase Cloud!" else "Account name saved locally."
                Toast.makeText(requireContext(), msg, Toast.LENGTH_SHORT).show()
            }
        }

        binding.btnSyncNow.setOnClickListener {
            performSync()
            Toast.makeText(requireContext(), "Syncing with RUHI Cloud...", Toast.LENGTH_SHORT).show()
        }

        binding.btnSendTabToPc.setOnClickListener {
            val mainAct = activity as? MainActivity
            val currentTab = mainAct?.tabManager?.getActiveTab()
            val url = currentTab?.url ?: ""
            val title = currentTab?.title ?: ""

            if (url.isEmpty() || url == "ruhi://home" || url == "about:blank") {
                Toast.makeText(requireContext(), "No active web page to send.", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            syncManager.sendTabToDevice(null, url, title) { ok ->
                if (ok) {
                    Toast.makeText(requireContext(), "Tab sent to your Windows PC!", Toast.LENGTH_SHORT).show()
                    dismiss()
                } else {
                    Toast.makeText(requireContext(), "Failed to send tab.", Toast.LENGTH_SHORT).show()
                }
            }
        }

        binding.btnViewPasswords.setOnClickListener {
            Toast.makeText(requireContext(), "Fetching synced passwords...", Toast.LENGTH_SHORT).show()
            syncManager.pullPasswords { list ->
                activity?.runOnUiThread {
                    if (list.isEmpty()) {
                        com.google.android.material.dialog.MaterialAlertDialogBuilder(requireContext())
                            .setTitle("🔐 Synced Passwords")
                            .setMessage("No passwords found in cloud vault yet.\nSave passwords in RUHI Windows Browser to sync them here.")
                            .setPositiveButton("OK", null)
                            .show()
                    } else {
                        val items = list.map { "${it.websiteTitle.ifEmpty { it.websiteUrl }}\n👤 ${it.username}" }.toTypedArray()
                        com.google.android.material.dialog.MaterialAlertDialogBuilder(requireContext())
                            .setTitle("🔐 Synced Passwords (${list.size})")
                            .setItems(items) { _, which ->
                                val cred = list[which]
                                com.google.android.material.dialog.MaterialAlertDialogBuilder(requireContext())
                                    .setTitle(cred.websiteTitle.ifEmpty { cred.websiteUrl })
                                    .setMessage("Website: ${cred.websiteUrl}\nUsername: ${cred.username}")
                                    .setPositiveButton("Copy Username") { _, _ ->
                                        val clipboard = requireContext().getSystemService(android.content.Context.CLIPBOARD_SERVICE) as android.content.ClipboardManager
                                        clipboard.setPrimaryClip(android.content.ClipData.newPlainText("Username", cred.username))
                                        Toast.makeText(requireContext(), "Username copied!", Toast.LENGTH_SHORT).show()
                                    }
                                    .setNegativeButton("Close", null)
                                    .show()
                            }
                            .setPositiveButton("Close", null)
                            .show()
                    }
                }
            }
        }

        binding.btnSyncLogout.setOnClickListener {
            com.google.android.material.dialog.MaterialAlertDialogBuilder(requireContext())
                .setTitle("Sign Out of RUHI Account?")
                .setMessage("Your local bookmarks and browsing history will be kept on this device, but cloud synchronization with your PC will be paused.")
                .setPositiveButton("Sign Out") { _, _ ->
                    syncManager.logout()
                    refreshUi()
                    Toast.makeText(requireContext(), "Signed out of RUHI Account.", Toast.LENGTH_SHORT).show()
                }
                .setNegativeButton("Cancel", null)
                .show()
        }
    }

    private fun refreshUi() {
        if (syncManager.isLoggedIn) {
            binding.panelSyncAuth.visibility = View.GONE
            binding.panelSyncDashboard.visibility = View.VISIBLE
            val dName = syncManager.currentDisplayName
            binding.tvSyncUserName.text = if (dName.isNotBlank()) dName else "Ruhi User"
            binding.tvSyncUserEmail.text = syncManager.currentEmail
            binding.etDashboardName.setText(dName)
            val initial = (dName.ifBlank { syncManager.currentEmail.ifBlank { "R" } }).take(1).uppercase()
            binding.tvAccountAvatarInitial.text = initial
            binding.tvSyncStatus.text = "🟢 Connected to RUHI Cloud"

            syncManager.getLinkedDevices { devices ->
                activity?.runOnUiThread {
                    if (_binding != null && isAdded) {
                        renderLinkedDevices(devices)
                    }
                }
            }
        } else {
            binding.panelSyncAuth.visibility = View.VISIBLE
            binding.panelSyncDashboard.visibility = View.GONE
            if (syncManager.currentEmail.isNotBlank()) {
                binding.etSyncEmail.setText(syncManager.currentEmail)
                showError("Session expired or signed out. Please enter password to reconnect.")
            } else {
                binding.tvSyncAuthError.visibility = View.GONE
            }
        }
    }

    private fun renderLinkedDevices(devices: List<com.ruhi.browser.sync.LinkedDevice>) {
        val container = binding.layoutLinkedDevicesContainer
        container.removeAllViews()

        binding.tvDevicesCount.text = "${devices.size} Device${if (devices.size != 1) "s" else ""}"

        if (devices.isEmpty()) {
            val emptyTv = android.widget.TextView(requireContext()).apply {
                text = "No linked devices found yet.\nOpen RUHI Browser on your PC and sign in to link devices."
                textSize = 12f
                setTextColor(androidx.core.content.ContextCompat.getColor(requireContext(), R.color.text_secondary_dark))
                setPadding(0, 8, 0, 8)
            }
            container.addView(emptyTv)
            return
        }

        for (dev in devices) {
            val isCurrent = dev.deviceId == syncManager.deviceId
            val isWindows = dev.deviceType.contains("Windows", ignoreCase = true)

            val row = android.widget.LinearLayout(requireContext()).apply {
                orientation = android.widget.LinearLayout.HORIZONTAL
                gravity = android.view.Gravity.CENTER_VERTICAL
                setPadding(0, 8, 0, 8)
            }

            // Device Icon (💻 or 📱)
            val iconTv = android.widget.TextView(requireContext()).apply {
                text = if (isWindows) "💻" else "📱"
                textSize = 20f
                setPadding(0, 0, 12, 0)
            }
            row.addView(iconTv)

            // Info column
            val infoCol = android.widget.LinearLayout(requireContext()).apply {
                orientation = android.widget.LinearLayout.VERTICAL
                layoutParams = android.widget.LinearLayout.LayoutParams(0, android.widget.LinearLayout.LayoutParams.WRAP_CONTENT, 1f)
            }

            val nameTv = android.widget.TextView(requireContext()).apply {
                text = dev.deviceName.ifBlank { if (isWindows) "Windows PC" else "Android Phone" }
                textSize = 14f
                setTypeface(null, android.graphics.Typeface.BOLD)
            }
            infoCol.addView(nameTv)

            val subTv = android.widget.TextView(requireContext()).apply {
                val tabsText = if (dev.activeTabsCount > 0) " • ${dev.activeTabsCount} active tabs" else ""
                text = if (isCurrent) "This Android Device (Current)$tabsText" else "Active • ${dev.deviceType}$tabsText"
                textSize = 11f
                setTextColor(androidx.core.content.ContextCompat.getColor(requireContext(), R.color.brand_primary))
            }
            infoCol.addView(subTv)
            row.addView(infoCol)

            if (!isCurrent) {
                val sendBtn = com.google.android.material.button.MaterialButton(
                    requireContext(),
                    null,
                    com.google.android.material.R.attr.borderlessButtonStyle
                ).apply {
                    text = "Send Tab"
                    textSize = 11f
                    setPadding(12, 4, 12, 4)
                    setOnClickListener {
                        val mainAct = activity as? MainActivity
                        val currentTab = mainAct?.tabManager?.getActiveTab()
                        val url = currentTab?.url ?: ""
                        val title = currentTab?.title ?: ""

                        if (url.isBlank() || url == "ruhi://home" || url == "about:blank") {
                            Toast.makeText(requireContext(), "Open any webpage first to send.", Toast.LENGTH_SHORT).show()
                            return@setOnClickListener
                        }

                        syncManager.sendTabToDevice(dev.deviceId, url, title) { ok ->
                            activity?.runOnUiThread {
                                if (ok) {
                                    Toast.makeText(requireContext(), "Tab sent to ${dev.deviceName}!", Toast.LENGTH_SHORT).show()
                                    dismiss()
                                } else {
                                    Toast.makeText(requireContext(), "Failed to send tab.", Toast.LENGTH_SHORT).show()
                                }
                            }
                        }
                    }
                }
                row.addView(sendBtn)
            }

            container.addView(row)
        }
    }

    private fun showError(msg: String) {
        binding.tvSyncAuthError.text = msg
        binding.tvSyncAuthError.visibility = View.VISIBLE
    }

    private fun performSync() {
        binding.btnSyncNow.isEnabled = false
        binding.btnSyncNow.text = "Syncing..."
        val dbHelper = RuhiDatabaseHelper(requireContext())
        val repo = BookmarkRepository(BookmarkDao(dbHelper))
        syncManager.syncBookmarks(repo) { ok ->
            activity?.runOnUiThread {
                if (_binding != null) {
                    binding.btnSyncNow.isEnabled = true
                    binding.btnSyncNow.text = "⚡ Sync Now"
                }
                if (ok) {
                    Toast.makeText(requireContext(), "Bookmarks synced with PC!", Toast.LENGTH_SHORT).show()
                }
            }
        }
        val histRepo = com.ruhi.browser.data.repository.HistoryRepository(com.ruhi.browser.data.local.HistoryDao(dbHelper))
        syncManager.syncHistory(histRepo)
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }
}
