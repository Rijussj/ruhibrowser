package com.ruhi.browser.ui.ai

import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Toast
import androidx.core.view.isVisible
import com.google.android.material.bottomsheet.BottomSheetDialogFragment
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.ruhi.browser.R
import com.ruhi.browser.ai.AiAssistantClient
import com.ruhi.browser.data.preferences.SettingsPreferences
import com.ruhi.browser.databinding.LayoutAiAssistantBinding
import com.ruhi.browser.ui.settings.SettingsActivity

class AiBottomSheetDialogFragment : BottomSheetDialogFragment() {

    private var _binding: LayoutAiAssistantBinding? = null
    private val binding get() = _binding!!

    private lateinit var preferences: SettingsPreferences
    private lateinit var aiClient: AiAssistantClient

    private var pageTitle: String? = null
    private var pageUrl: String? = null
    private var isIncognito: Boolean = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        pageTitle = arguments?.getString(ARG_PAGE_TITLE)
        pageUrl = arguments?.getString(ARG_PAGE_URL)
        isIncognito = arguments?.getBoolean(ARG_IS_INCOGNITO, false) ?: false
    }

    override fun onCreateView(
        inflater: LayoutInflater,
        container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View {
        _binding = LayoutAiAssistantBinding.inflate(inflater, container, false)
        return binding.root
    }

    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)

        preferences = SettingsPreferences(requireContext())
        aiClient = AiAssistantClient(requireContext())

        setupUIState()
        setupListeners()
    }

    private fun setupUIState() {
        val hasKey = preferences.aiApiKey.isNotBlank()
        binding.layoutAiUnconfigured.isVisible = !hasKey

        if (isIncognito) {
            binding.cbIncludePageContext.isEnabled = false
            binding.cbIncludePageContext.isChecked = false
            binding.cbIncludePageContext.text = "${getString(R.string.ai_include_page)} (Disabled in Incognito)"
        } else if (pageUrl.isNullOrBlank()) {
            binding.cbIncludePageContext.isEnabled = false
            binding.cbIncludePageContext.isChecked = false
        } else {
            binding.cbIncludePageContext.isChecked = preferences.aiConsentGiven
        }
    }

    private fun setupListeners() {
        binding.btnAiSettings.setOnClickListener {
            startActivity(Intent(requireContext(), SettingsActivity::class.java))
            dismiss()
        }

        binding.btnConfigureAi.setOnClickListener {
            startActivity(Intent(requireContext(), SettingsActivity::class.java))
            dismiss()
        }

        binding.cbIncludePageContext.setOnCheckedChangeListener { _, isChecked ->
            if (isChecked && !preferences.aiConsentGiven) {
                showConsentDialog()
            }
        }

        binding.btnSendAiPrompt.setOnClickListener {
            handleSendPrompt()
        }

        binding.btnCopyAiResponse.setOnClickListener {
            val text = binding.tvAiResponseContent.text.toString()
            if (text.isNotBlank()) {
                val clipboard = requireContext().getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
                val clip = ClipData.newPlainText("AI Response", text)
                clipboard.setPrimaryClip(clip)
                Toast.makeText(requireContext(), "Response copied to clipboard", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun showConsentDialog() {
        MaterialAlertDialogBuilder(requireContext())
            .setTitle(R.string.ai_consent_dialog_title)
            .setMessage(R.string.ai_consent_dialog_message)
            .setPositiveButton(R.string.ai_consent_agree) { _, _ ->
                preferences.aiConsentGiven = true
                binding.cbIncludePageContext.isChecked = true
            }
            .setNegativeButton(R.string.ai_consent_decline) { dialog, _ ->
                binding.cbIncludePageContext.isChecked = false
                dialog.dismiss()
            }
            .setCancelable(false)
            .show()
    }

    private fun handleSendPrompt() {
        val prompt = binding.etAiPrompt.text.toString().trim()
        if (prompt.isEmpty()) {
            Toast.makeText(requireContext(), "Please enter a question or prompt", Toast.LENGTH_SHORT).show()
            return
        }

        if (preferences.aiApiKey.isBlank()) {
            Toast.makeText(requireContext(), R.string.ai_setup_required, Toast.LENGTH_LONG).show()
            return
        }

        binding.aiProgressBar.isVisible = true
        binding.btnSendAiPrompt.isEnabled = false

        val includeContext = binding.cbIncludePageContext.isChecked && !isIncognito
        val contextSummary = if (includeContext) {
            "Title: ${pageTitle.orEmpty()}\nURL: ${pageUrl.orEmpty()}"
        } else {
            null
        }

        aiClient.query(
            userPrompt = prompt,
            pageContext = contextSummary,
            includePageContext = includeContext,
            listener = object : AiAssistantClient.AiResponseListener {
                override fun onResponse(response: String) {
                    if (_binding == null) return
                    binding.aiProgressBar.isVisible = false
                    binding.btnSendAiPrompt.isEnabled = true
                    binding.cardAiResponse.isVisible = true
                    binding.tvAiResponseContent.text = response
                }

                override fun onError(errorMessage: String) {
                    if (_binding == null) return
                    binding.aiProgressBar.isVisible = false
                    binding.btnSendAiPrompt.isEnabled = true
                    binding.cardAiResponse.isVisible = true
                    binding.tvAiResponseContent.text = "Error: $errorMessage"
                }
            }
        )
    }

    override fun onDestroyView() {
        super.onDestroyView()
        _binding = null
    }

    companion object {
        const val TAG = "AiBottomSheetDialogFragment"
        private const val ARG_PAGE_TITLE = "arg_page_title"
        private const val ARG_PAGE_URL = "arg_page_url"
        private const val ARG_IS_INCOGNITO = "arg_is_incognito"

        fun newInstance(pageTitle: String?, pageUrl: String?, isIncognito: Boolean): AiBottomSheetDialogFragment {
            return AiBottomSheetDialogFragment().apply {
                arguments = Bundle().apply {
                    putString(ARG_PAGE_TITLE, pageTitle)
                    putString(ARG_PAGE_URL, pageUrl)
                    putBoolean(ARG_IS_INCOGNITO, isIncognito)
                }
            }
        }
    }
}
