package com.ruhi.browser.ui.menu

import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import com.google.android.material.bottomsheet.BottomSheetDialogFragment
import com.ruhi.browser.MainActivity
import com.ruhi.browser.R
import com.ruhi.browser.databinding.BottomSheetMenuBinding
import com.ruhi.browser.ui.bookmarks.BookmarksActivity
import com.ruhi.browser.ui.history.HistoryActivity
import com.ruhi.browser.ui.settings.SettingsActivity

class MenuBottomSheetDialogFragment : BottomSheetDialogFragment() {

    private var _binding: BottomSheetMenuBinding? = null
    private val binding get() = _binding!!

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View {
        _binding = BottomSheetMenuBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        // Phase 3 Active: New Tab
        binding.itemNewTab.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.createNewTab(isIncognito = false)
        }

        // Phase 3 Active: New Incognito Tab
        binding.itemNewIncognitoTab.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.createNewTab(isIncognito = true)
        }

        // Phase 4 Active: Bookmarks
        binding.itemBookmarks.setOnClickListener {
            dismiss()
            val intent = Intent(requireContext(), BookmarksActivity::class.java)
            (activity as? MainActivity)?.bookmarkLauncher?.launch(intent)
        }

        // Phase 4 Active: History
        binding.itemHistory.setOnClickListener {
            dismiss()
            val intent = Intent(requireContext(), HistoryActivity::class.java)
            (activity as? MainActivity)?.historyLauncher?.launch(intent)
        }

        // Firefox-Style Ruhi Account Profile Card
        val syncManager = com.ruhi.browser.sync.SyncManager.getInstance(requireContext())
        fun updateAccountUi() {
            if (!isAdded) return
            if (syncManager.isLoggedIn && syncManager.currentEmail.isNotBlank()) {
                val dName = syncManager.currentDisplayName
                val email = syncManager.currentEmail
                binding.tvAccountTitle.text = if (dName.isNotBlank()) dName else email
                binding.tvAccountSubtitle.text = if (dName.isNotBlank() && email.isNotBlank()) email else "Ruhi Account • Manage sync & devices"
                val initial = if (dName.isNotBlank()) dName.firstOrNull()?.uppercaseChar()?.toString() else email.firstOrNull()?.uppercaseChar()?.toString()
                binding.tvAccountAvatarInitial.text = initial ?: "👤"
                binding.viewAccountOnlineDot.visibility = View.VISIBLE
            } else {
                val hasEmail = syncManager.currentEmail.isNotBlank()
                binding.tvAccountTitle.text = if (hasEmail) "Reconnect Account" else "Sign in to Ruhi Account"
                binding.tvAccountSubtitle.text = if (hasEmail) "Session expired • Tap to sign in" else "Sync tabs, bookmarks & history"
                binding.tvAccountAvatarInitial.text = "👤"
                binding.viewAccountOnlineDot.visibility = View.GONE
            }
        }
        updateAccountUi()
        if (syncManager.isLoggedIn) {
            syncManager.refreshUserProfile {
                activity?.runOnUiThread {
                    updateAccountUi()
                }
            }
        }

        // Ruhi Account Card click
        binding.itemSync.setOnClickListener {
            dismiss()
            com.ruhi.browser.ui.sync.SyncBottomSheetDialogFragment().show(
                parentFragmentManager,
                "SyncBottomSheetDialogFragment"
            )
        }

        // Active: Settings
        binding.itemSettings.setOnClickListener {
            dismiss()
            val intent = Intent(requireContext(), SettingsActivity::class.java)
            startActivity(intent)
        }

        // Downloads
        binding.itemDownloads.setOnClickListener {
            dismiss()
            val intent = Intent(requireContext(), com.ruhi.browser.ui.downloads.DownloadsActivity::class.java)
            startActivity(intent)
        }

        // Phase 6 Active: Desktop Site
        val isDesktop = (activity as? MainActivity)?.isCurrentTabDesktopSite() == true
        binding.ivDesktopSiteCheck.visibility = if (isDesktop) View.VISIBLE else View.GONE
        binding.itemDesktopSite.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.toggleDesktopSite()
        }

        // Phase 6 Active: Find in Page
        binding.itemFindInPage.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.showFindInPage()
        }

        // Share
        binding.itemShare.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.shareCurrentPage()
        }

        // Reading Mode
        binding.itemReaderMode.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.toggleReaderMode()
        }

        // Translate Page
        binding.itemTranslate.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.showTranslateDialog()
        }

        // Add to Home screen (Instant App / PWA)
        binding.itemAddToHomeScreen.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.addCurrentPageToHomeScreen()
        }

        // Phase 7 Active: AI Assistant
        binding.itemAiAssistant.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.showAiAssistant()
        }

        // WireGuard VPN
        if (com.ruhi.browser.vpn.RuhiVpnService.currentState == com.ruhi.browser.vpn.RuhiVpnService.VpnState.CONNECTED) {
            binding.tvVpnBadge.text = "Active"
            binding.tvVpnBadge.setTextColor(requireContext().getColor(R.color.brand_primary))
        }
        binding.itemVpn.setOnClickListener {
            dismiss()
            val intent = Intent(requireContext(), com.ruhi.browser.ui.vpn.VpnActivity::class.java)
            startActivity(intent)
        }

        // Set as Default Browser
        binding.itemDefaultBrowser.setOnClickListener {
            dismiss()
            val mainActivity = activity as? MainActivity
            com.ruhi.browser.util.DefaultBrowserHelper.requestDefaultBrowser(
                requireActivity(),
                mainActivity?.defaultBrowserLauncher
            )
        }

        // Delete Browsing Data
        binding.itemClearData.setOnClickListener {
            dismiss()
            (activity as? MainActivity)?.showClearDataDialog()
        }
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }

    companion object {
        const val TAG = "MenuBottomSheetDialogFragment"
        fun newInstance() = MenuBottomSheetDialogFragment()
    }
}
