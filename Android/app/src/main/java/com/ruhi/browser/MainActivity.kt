package com.ruhi.browser

import android.Manifest
import android.app.Activity
import android.content.ActivityNotFoundException
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.net.http.SslError
import android.os.Bundle
import android.speech.RecognizerIntent
import android.text.Editable
import android.text.TextWatcher
import android.view.View
import android.view.ViewGroup
import android.view.inputmethod.EditorInfo
import android.view.inputmethod.InputMethodManager
import android.webkit.WebView
import android.widget.ImageView
import android.widget.Toast
import android.graphics.Bitmap
import android.net.Uri
import androidx.activity.OnBackPressedCallback
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.app.AppCompatDelegate
import androidx.core.content.ContextCompat
import androidx.core.content.pm.ShortcutInfoCompat
import androidx.core.content.pm.ShortcutManagerCompat
import androidx.core.graphics.drawable.IconCompat
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.core.view.isVisible
import androidx.core.view.updateLayoutParams
import androidx.core.view.updatePadding
import androidx.recyclerview.widget.GridLayoutManager
import androidx.recyclerview.widget.LinearLayoutManager
import com.ruhi.browser.data.model.HistoryItem
import com.ruhi.browser.ui.home.RecentVisitsAdapter
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.ruhi.browser.util.DefaultBrowserHelper
import com.ruhi.browser.util.FaviconManager
import com.ruhi.browser.browser.RuhiWebChromeClient
import com.ruhi.browser.browser.RuhiWebViewClient
import com.ruhi.browser.browser.UrlResolver
import com.ruhi.browser.data.local.BookmarkDao
import com.ruhi.browser.data.local.HistoryDao
import com.ruhi.browser.data.local.RuhiDatabaseHelper
import com.ruhi.browser.data.preferences.SettingsPreferences
import com.ruhi.browser.data.repository.BookmarkRepository
import com.ruhi.browser.data.repository.HistoryRepository
import com.ruhi.browser.databinding.ActivityMainBinding
import com.ruhi.browser.databinding.ViewHomeBinding
import com.ruhi.browser.model.NewsArticle
import com.ruhi.browser.model.Shortcut
import com.ruhi.browser.tabs.Tab
import com.ruhi.browser.tabs.TabAdapter
import com.ruhi.browser.tabs.TabManager
import com.ruhi.browser.ui.bookmarks.BookmarksActivity
import com.ruhi.browser.ui.history.HistoryActivity
import com.ruhi.browser.ui.home.NewsAdapter
import com.ruhi.browser.ui.home.ShortcutAdapter
import com.ruhi.browser.ui.menu.MenuBottomSheetDialogFragment
import org.xmlpull.v1.XmlPullParser
import org.xmlpull.v1.XmlPullParserFactory
import java.io.InputStream
import java.net.HttpURLConnection
import java.net.URL
import java.text.SimpleDateFormat
import java.util.Locale
import java.util.concurrent.Executors

class MainActivity : AppCompatActivity(),
    RuhiWebViewClient.BrowserEventListener,
    RuhiWebChromeClient.ChromeEventListener {

    private lateinit var binding: ActivityMainBinding
    private lateinit var homeBinding: ViewHomeBinding

    // Data Repositories & Preferences
    lateinit var preferences: SettingsPreferences
        private set
    lateinit var bookmarkRepository: BookmarkRepository
        private set
    lateinit var historyRepository: HistoryRepository
        private set

    private val dbExecutor = Executors.newSingleThreadExecutor()

    // Activity Result Launchers
    val bookmarkLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) {
            val url = result.data?.getStringExtra(BookmarksActivity.EXTRA_OPEN_URL)
            if (!url.isNullOrBlank()) {
                createNewTab(url = url)
            }
        }
    }

    val historyLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) {
            val url = result.data?.getStringExtra(HistoryActivity.EXTRA_OPEN_URL)
            if (!url.isNullOrBlank()) {
                createNewTab(url = url)
            }
        }
        loadRecentVisits()
    }

    val defaultBrowserLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) {
        if (DefaultBrowserHelper.isDefaultBrowser(this)) {
            Toast.makeText(this, "RUHI Browser is now your default browser!", Toast.LENGTH_SHORT).show()
        }
    }

    val voiceSearchLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) {
            val spokenText = result.data
                ?.getStringArrayListExtra(RecognizerIntent.EXTRA_RESULTS)
                ?.firstOrNull()
            if (!spokenText.isNullOrBlank()) {
                binding.etSearchAddress.setText(spokenText)
                navigateToInput(spokenText)
            }
        }
    }

    private val recordAudioPermissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            startVoiceSearchIntent()
        } else {
            Toast.makeText(this, R.string.voice_search_permission_required, Toast.LENGTH_SHORT).show()
        }
    }

    // HTML5 File Chooser and Custom View (Fullscreen Video)
    private var fileChooserCallback: android.webkit.ValueCallback<Array<Uri>>? = null
    val fileChooserLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        val results: Array<Uri>? = if (result.resultCode == Activity.RESULT_OK) {
            val data = result.data
            if (data?.data != null) {
                arrayOf(data.data!!)
            } else if (data?.clipData != null) {
                val clipData = data.clipData!!
                Array(clipData.itemCount) { i -> clipData.getItemAt(i).uri }
            } else {
                null
            }
        } else {
            null
        }
        fileChooserCallback?.onReceiveValue(results)
        fileChooserCallback = null
    }

    private var customView: View? = null
    private var customViewCallback: android.webkit.WebChromeClient.CustomViewCallback? = null

    // Home Shortcuts State
    private lateinit var shortcutAdapter: ShortcutAdapter
    private val shortcutList = mutableListOf<Shortcut>()

    // Recent Visits State
    private lateinit var recentVisitsAdapter: RecentVisitsAdapter
    private val recentVisitsList = mutableListOf<HistoryItem>()

    // News Feed State
    private lateinit var newsAdapter: NewsAdapter
    private val allNewsArticles = mutableListOf<NewsArticle>()
    private val displayedNewsArticles = mutableListOf<NewsArticle>()
    private var activeNewsCategory: String = "top"

    // Tab Management
    val tabManager = TabManager()
    private val tabWebViews = mutableMapOf<String, WebView>()
    private lateinit var tabAdapter: TabAdapter

    private var isLoading = false
    private var lastFailedUrl: String? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        // Initialize persistent settings, repositories and favicon manager
        preferences = SettingsPreferences(this)
        FaviconManager.init(this)
        AppCompatDelegate.setDefaultNightMode(preferences.theme)

        val dbHelper = RuhiDatabaseHelper(this)
        bookmarkRepository = BookmarkRepository(BookmarkDao(dbHelper))
        historyRepository = HistoryRepository(HistoryDao(dbHelper))

        // Synchronize AdBlock manager with saved user preference
        com.ruhi.browser.blocking.AdBlockManager.isEnabled = preferences.adBlockEnabled

        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setupWindowInsets()
        setupHomeScreen()
        setupTabSwitcher()
        setupSearchBar()
        setupFindInPage()
        setupToolbarControls()
        setupBottomToolbar()
        setupBackNavigation()
        setupErrorViewActions()
        requestNotificationPermissionIfRequired()

        // Handle incoming URL if opened via external intent / default browser
        val incomingUrl = intent?.dataString
        if (!incomingUrl.isNullOrBlank()) {
            val initialTab = tabManager.createTab(url = incomingUrl, title = "")
            switchToTab(initialTab)
            loadUrl(incomingUrl)
        } else {
            // Create initial default tab
            val initialTab = tabManager.createTab(url = "", title = getString(R.string.menu_new_tab))
            switchToTab(initialTab)
        }
    }

    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
        val incomingUrl = intent.dataString
        if (!incomingUrl.isNullOrBlank()) {
            createNewTab(incomingUrl)
        }
    }

    private fun requestNotificationPermissionIfRequired() {
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.TIRAMISU) {
            if (ContextCompat.checkSelfPermission(this, Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED) {
                requestPermissions(arrayOf(Manifest.permission.POST_NOTIFICATIONS), 102)
            }
        }
    }

    private fun setupWindowInsets() {
        ViewCompat.setOnApplyWindowInsetsListener(binding.root) { _, windowInsets ->
            val statusInsets = windowInsets.getInsets(
                WindowInsetsCompat.Type.statusBars() or WindowInsetsCompat.Type.displayCutout()
            )
            val navInsets = windowInsets.getInsets(
                WindowInsetsCompat.Type.navigationBars() or WindowInsetsCompat.Type.systemBars()
            )
            val density = resources.displayMetrics.density
            val verticalPaddingPx = (6 * density).toInt()
            val horizontalPaddingPx = (8 * density).toInt()

            binding.topToolbar.updatePadding(
                top = statusInsets.top + verticalPaddingPx,
                bottom = verticalPaddingPx,
                left = statusInsets.left + horizontalPaddingPx,
                right = statusInsets.right + horizontalPaddingPx
            )
            binding.bottomToolbar.updatePadding(
                bottom = navInsets.bottom,
                left = navInsets.left,
                right = navInsets.right
            )
            binding.viewTabsSwitcher.tabsToolbar.updatePadding(
                top = statusInsets.top,
                left = statusInsets.left,
                right = statusInsets.right
            )
            val fabMarginBottomPx = (28 * density).toInt()
            val totalFabBottom = navInsets.bottom + fabMarginBottomPx
            binding.viewTabsSwitcher.fabNewTab.updateLayoutParams<ViewGroup.MarginLayoutParams> {
                bottomMargin = totalFabBottom
            }
            binding.viewTabsSwitcher.rvTabsGrid.updatePadding(
                bottom = totalFabBottom + (68 * density).toInt(),
                left = statusInsets.left + (8 * density).toInt(),
                right = statusInsets.right + (8 * density).toInt()
            )
            windowInsets
        }
    }

    private fun setupHomeScreen() {
        homeBinding = ViewHomeBinding.inflate(layoutInflater, binding.contentContainer, false)
        binding.contentContainer.addView(homeBinding.root, 0)

        loadShortcuts()

        homeBinding.rvShortcuts.layoutManager = GridLayoutManager(this, 4)
        shortcutAdapter = ShortcutAdapter(
            shortcuts = shortcutList,
            onShortcutClick = { shortcut ->
                loadUrl(shortcut.url)
            },
            onShortcutLongClick = { shortcut, position ->
                showShortcutOptionsDialog(shortcut, position)
            }
        )
        homeBinding.rvShortcuts.adapter = shortcutAdapter

        homeBinding.btnAddShortcut.setOnClickListener {
            showAddEditShortcutDialog(null, -1)
        }

        // Setup Recent Visits
        recentVisitsAdapter = RecentVisitsAdapter(
            visits = recentVisitsList,
            onItemClick = { item ->
                loadUrl(item.url)
            }
        )
        homeBinding.rvRecentVisits.layoutManager = LinearLayoutManager(this)
        val dividerItemDecoration = com.google.android.material.divider.MaterialDividerItemDecoration(
            this,
            LinearLayoutManager.VERTICAL
        ).apply {
            isLastItemDecorated = false
        }
        homeBinding.rvRecentVisits.addItemDecoration(dividerItemDecoration)
        homeBinding.rvRecentVisits.adapter = recentVisitsAdapter

        homeBinding.tvSeeAllHistory.setOnClickListener {
            val intent = Intent(this, HistoryActivity::class.java)
            historyLauncher.launch(intent)
        }

        loadRecentVisits()

        // Setup News Feed (MSN / Edge style powered by live TaazaWire RSS)
        newsAdapter = NewsAdapter(displayedNewsArticles) { article ->
            loadUrl(article.url)
        }
        homeBinding.rvNewsArticles.layoutManager = LinearLayoutManager(this)
        homeBinding.rvNewsArticles.adapter = newsAdapter

        homeBinding.cardNewsHero.setOnClickListener {
            val hero = displayedNewsArticles.firstOrNull()
            if (hero != null) {
                loadUrl(hero.url)
            }
        }

        homeBinding.btnRefreshNews.setOnClickListener {
            fetchTaazaWireNews(force = true)
        }

        setupNewsCategoryPills()
        fetchTaazaWireNews()
    }

    private fun setupNewsCategoryPills() {
        val pills = listOf(
            homeBinding.chipTop to "top",
            homeBinding.chipTech to "tech",
            homeBinding.chipFinance to "business",
            homeBinding.chipSports to "sports",
            homeBinding.chipEntertainment to "entertainment",
            homeBinding.chipBreaking to "world"
        )

        for ((chip, category) in pills) {
            chip.setOnClickListener {
                activeNewsCategory = category
                updateCategoryPillsUi(category)
                filterAndDisplayNews()
            }
        }
    }

    private fun updateCategoryPillsUi(selectedCat: String) {
        val pills = listOf(
            homeBinding.chipTop to "top",
            homeBinding.chipTech to "tech",
            homeBinding.chipFinance to "business",
            homeBinding.chipSports to "sports",
            homeBinding.chipEntertainment to "entertainment",
            homeBinding.chipBreaking to "world"
        )
        val selectedBg = ContextCompat.getDrawable(this, R.drawable.bg_chip_selected)
        val unselectedBg = ContextCompat.getDrawable(this, R.drawable.bg_chip_unselected)
        val whiteColor = ContextCompat.getColor(this, android.R.color.white)

        val typedValue = android.util.TypedValue()
        theme.resolveAttribute(com.google.android.material.R.attr.colorOnSurfaceVariant, typedValue, true)
        val unselectedTextColor = typedValue.data

        for ((chip, category) in pills) {
            val isSelected = category == selectedCat
            chip.background = if (isSelected) selectedBg else unselectedBg
            chip.setTextColor(if (isSelected) whiteColor else unselectedTextColor)
        }
    }

    private fun fetchTaazaWireNews(force: Boolean = false) {
        dbExecutor.execute {
            try {
                val url = URL("https://taazawire.in/feed/")
                val conn = url.openConnection() as HttpURLConnection
                conn.connectTimeout = 8000
                conn.readTimeout = 8000
                conn.setRequestProperty("User-Agent", "RUHI Browser Android/1.0")
                conn.connect()

                if (conn.responseCode == 200) {
                    val articles = parseRssFeed(conn.inputStream)
                    if (articles.isNotEmpty()) {
                        runOnUiThread {
                            allNewsArticles.clear()
                            allNewsArticles.addAll(articles)
                            filterAndDisplayNews()
                        }
                        return@execute
                    }
                }
            } catch (e: Exception) {
                // If offline or feed fails, fall back to default curated items
            }

            if (allNewsArticles.isEmpty()) {
                val fallbackArticles = getDefaultNewsArticles()
                runOnUiThread {
                    allNewsArticles.clear()
                    allNewsArticles.addAll(fallbackArticles)
                    filterAndDisplayNews()
                }
            }
        }
    }

    private fun parseRssFeed(inputStream: InputStream): List<NewsArticle> {
        val result = mutableListOf<NewsArticle>()
        try {
            val factory = XmlPullParserFactory.newInstance()
            factory.isNamespaceAware = false
            val parser = factory.newPullParser()
            parser.setInput(inputStream, "UTF-8")

            var eventType = parser.eventType
            var inItem = false
            var currentTitle = ""
            var currentLink = ""
            var currentDesc = ""
            var currentCategory = ""
            var currentPubDate = ""

            while (eventType != XmlPullParser.END_DOCUMENT) {
                val tagName = parser.name
                when (eventType) {
                    XmlPullParser.START_TAG -> {
                        if (tagName.equals("item", ignoreCase = true)) {
                            inItem = true
                            currentTitle = ""
                            currentLink = ""
                            currentDesc = ""
                            currentCategory = ""
                            currentPubDate = ""
                        } else if (inItem) {
                            when (tagName.lowercase(Locale.ROOT)) {
                                "title" -> currentTitle = parser.nextText().trim()
                                "link" -> currentLink = parser.nextText().trim()
                                "description" -> currentDesc = cleanHtml(parser.nextText().trim())
                                "category" -> {
                                    val cat = parser.nextText().trim()
                                    if (currentCategory.isEmpty()) currentCategory = cat
                                }
                                "pubdate" -> currentPubDate = parser.nextText().trim()
                            }
                        }
                    }
                    XmlPullParser.END_TAG -> {
                        if (tagName.equals("item", ignoreCase = true) && inItem) {
                            inItem = false
                            if (currentTitle.isNotBlank() && currentLink.isNotBlank()) {
                                val category = categorizeArticle(currentTitle, currentCategory)
                                val timeAgo = formatTimeAgo(currentPubDate)
                                val emoji = getCategoryEmoji(category)
                                result.add(
                                    NewsArticle(
                                        title = cleanHtml(currentTitle),
                                        description = currentDesc.take(150),
                                        source = "TaazaWire",
                                        timeAgo = timeAgo,
                                        category = category,
                                        url = currentLink,
                                        emoji = emoji
                                    )
                                )
                            }
                        }
                    }
                }
                eventType = parser.next()
            }
        } catch (e: Exception) {
            e.printStackTrace()
        } finally {
            try { inputStream.close() } catch (_: Exception) {}
        }
        return result
    }

    private fun cleanHtml(html: String): String {
        return html.replace(Regex("<[^>]*>"), "")
            .replace("&nbsp;", " ")
            .replace("&amp;", "&")
            .replace("&quot;", "\"")
            .replace("&#8217;", "'")
            .replace("&#8220;", "\"")
            .replace("&#8221;", "\"")
            .replace("&#8211;", "-")
            .trim()
    }

    private fun categorizeArticle(title: String, rawCategory: String): String {
        val text = "$title $rawCategory".lowercase(Locale.ROOT)
        return when {
            text.contains("tech") || text.contains("ai ") || text.contains("apple") || text.contains("google") || text.contains("software") || text.contains("phone") || text.contains("chip") -> "tech"
            text.contains("market") || text.contains("stock") || text.contains("economy") || text.contains("finance") || text.contains("bank") || text.contains("rupee") || text.contains("dollar") -> "business"
            text.contains("cricket") || text.contains("match") || text.contains("football") || text.contains("sport") || text.contains("cup") || text.contains("game") -> "sports"
            text.contains("movie") || text.contains("actor") || text.contains("cinema") || text.contains("bollywood") || text.contains("hollywood") || text.contains("song") -> "entertainment"
            text.contains("war") || text.contains("world") || text.contains("summit") || text.contains("israel") || text.contains("ukraine") || text.contains("un ") -> "world"
            else -> "top"
        }
    }

    private fun getCategoryEmoji(category: String): String {
        return when (category) {
            "tech" -> "💻"
            "business" -> "💼"
            "sports" -> "🏏"
            "entertainment" -> "🎬"
            "world" -> "⚡"
            else -> "🔥"
        }
    }

    private fun formatTimeAgo(pubDateStr: String): String {
        if (pubDateStr.isBlank()) return "Just now"
        try {
            val format = SimpleDateFormat("EEE, dd MMM yyyy HH:mm:ss Z", Locale.ENGLISH)
            val date = format.parse(pubDateStr)
            if (date != null) {
                val diffMs = System.currentTimeMillis() - date.time
                val diffHours = diffMs / (1000 * 60 * 60)
                return when {
                    diffHours < 1 -> "Just now"
                    diffHours < 24 -> "${diffHours}h ago"
                    else -> "${diffHours / 24}d ago"
                }
            }
        } catch (_: Exception) {}
        return "Recent"
    }

    private fun getDefaultNewsArticles(): List<NewsArticle> {
        return listOf(
            NewsArticle(
                title = "India Accelerates AI Mission with Cutting-Edge Supercomputing Infrastructure",
                description = "New investments in deep tech and domestic compute clusters empower startups and research labs nationwide.",
                source = "TaazaWire Tech",
                timeAgo = "1h ago",
                category = "tech",
                url = "https://taazawire.in",
                emoji = "💻"
            ),
            NewsArticle(
                title = "Markets Hit Fresh All-Time Highs Led by Banking and IT Rallies",
                description = "Benchmark indices surged past milestones today following strong quarterly corporate earnings and positive global cues.",
                source = "TaazaWire Markets",
                timeAgo = "2h ago",
                category = "business",
                url = "https://taazawire.in",
                emoji = "💼"
            ),
            NewsArticle(
                title = "Cricket Championship Thriller: Stellar Century Propels Team to Decisive Victory",
                description = "An extraordinary run chase in the final overs secured an unforgettable victory under floodlights.",
                source = "TaazaWire Sports",
                timeAgo = "3h ago",
                category = "sports",
                url = "https://taazawire.in",
                emoji = "🏏"
            ),
            NewsArticle(
                title = "Global Tech Leaders Unveil Next-Gen Generative AI Devices at Annual Summit",
                description = "On-device intelligence and ultra-fast silicon redefine mobile experience for millions worldwide.",
                source = "TaazaWire",
                timeAgo = "4h ago",
                category = "tech",
                url = "https://taazawire.in",
                emoji = "⚡"
            )
        )
    }

    private fun filterAndDisplayNews() {
        val filtered = if (activeNewsCategory == "top") {
            allNewsArticles
        } else {
            val matching = allNewsArticles.filter { it.category == activeNewsCategory }
            if (matching.isEmpty()) allNewsArticles else matching
        }

        displayedNewsArticles.clear()
        displayedNewsArticles.addAll(filtered)

        val hero = displayedNewsArticles.firstOrNull()
        if (hero != null) {
            homeBinding.cardNewsHero.isVisible = true
            homeBinding.tvHeroTitle.text = hero.title
            homeBinding.tvHeroDescription.text = hero.description
            homeBinding.tvHeroDescription.isVisible = hero.description.isNotBlank()
            homeBinding.tvHeroSourceTime.text = "${hero.source} • ${hero.timeAgo}"

            val listArticles = displayedNewsArticles.drop(1)
            newsAdapter.updateData(listArticles)
        } else {
            homeBinding.cardNewsHero.isVisible = false
            newsAdapter.updateData(emptyList())
        }
    }

    private fun loadRecentVisits() {
        val isIncognito = tabManager.getActiveTab()?.isIncognito == true
        if (isIncognito) {
            homeBinding.layoutRecentSection.isVisible = false
            return
        }
        homeBinding.layoutRecentSection.isVisible = true

        dbExecutor.execute {
            val allHistory = historyRepository.getHistory()
            val recentItems = allHistory.take(6)
            runOnUiThread {
                recentVisitsList.clear()
                recentVisitsList.addAll(recentItems)
                recentVisitsAdapter.updateData(recentVisitsList)

                val hasRecent = recentVisitsList.isNotEmpty()
                homeBinding.cardRecentContainer.isVisible = hasRecent
                homeBinding.cardRecentEmpty.isVisible = !hasRecent
                homeBinding.tvSeeAllHistory.isVisible = hasRecent
            }
        }
    }

    private fun loadShortcuts() {
        shortcutList.clear()
        shortcutList.addAll(preferences.getCustomShortcuts())
    }

    private fun showShortcutOptionsDialog(shortcut: Shortcut, position: Int) {
        val options = arrayOf(
            getString(R.string.edit_shortcut),
            getString(R.string.move_left),
            getString(R.string.move_right),
            getString(R.string.remove_shortcut)
        )

        MaterialAlertDialogBuilder(this)
            .setTitle(shortcut.title)
            .setItems(options) { dialog, which ->
                when (which) {
                    0 -> showAddEditShortcutDialog(shortcut, position)
                    1 -> moveShortcut(position, position - 1)
                    2 -> moveShortcut(position, position + 1)
                    3 -> removeShortcut(position)
                }
                dialog.dismiss()
            }
            .show()
    }

    private fun moveShortcut(fromPos: Int, toPos: Int) {
        if (toPos in 0 until shortcutList.size) {
            val item = shortcutList.removeAt(fromPos)
            shortcutList.add(toPos, item)
            preferences.saveCustomShortcuts(shortcutList)
            shortcutAdapter.updateData(shortcutList)
        }
    }

    private fun removeShortcut(position: Int) {
        if (position in 0 until shortcutList.size) {
            shortcutList.removeAt(position)
            preferences.saveCustomShortcuts(shortcutList)
            shortcutAdapter.updateData(shortcutList)
        }
    }

    private fun showAddEditShortcutDialog(existing: Shortcut?, position: Int) {
        val dialogView = layoutInflater.inflate(R.layout.dialog_edit_shortcut, null)
        val etTitle = dialogView.findViewById<com.google.android.material.textfield.TextInputEditText>(R.id.etShortcutTitle)
        val etUrl = dialogView.findViewById<com.google.android.material.textfield.TextInputEditText>(R.id.etShortcutUrl)

        if (existing != null) {
            etTitle.setText(existing.title)
            etUrl.setText(existing.url)
        }

        val titleRes = if (existing == null) R.string.add_shortcut else R.string.edit_shortcut

        MaterialAlertDialogBuilder(this)
            .setTitle(titleRes)
            .setView(dialogView)
            .setPositiveButton(R.string.action_save) { dialog, _ ->
                val title = etTitle.text?.toString()?.trim().orEmpty()
                val url = etUrl.text?.toString()?.trim().orEmpty()
                if (url.isNotEmpty()) {
                    val finalTitle = if (title.isEmpty()) url else title
                    val finalUrl = if (!url.startsWith("http://", true) && !url.startsWith("https://", true)) "https://$url" else url
                    val updated = Shortcut(finalTitle, finalUrl)
                    if (existing != null && position in 0 until shortcutList.size) {
                        shortcutList[position] = updated
                    } else {
                        shortcutList.add(updated)
                    }
                    preferences.saveCustomShortcuts(shortcutList)
                    shortcutAdapter.updateData(shortcutList)
                } else {
                    Toast.makeText(this, R.string.shortcut_empty_error, Toast.LENGTH_SHORT).show()
                }
                dialog.dismiss()
            }
            .setNegativeButton(R.string.action_cancel, null)
            .show()
    }

    private fun setupTabSwitcher() {
        tabAdapter = TabAdapter(
            tabs = tabManager.getTabs(),
            activeTabId = tabManager.activeTabId,
            onTabClick = { tab ->
                switchToTab(tab)
                hideTabSwitcher()
            },
            onTabClose = { tab ->
                closeTab(tab)
            }
        )

        binding.viewTabsSwitcher.rvTabsGrid.layoutManager = GridLayoutManager(this, 2)
        binding.viewTabsSwitcher.rvTabsGrid.adapter = tabAdapter

        binding.viewTabsSwitcher.btnDoneTabs.setOnClickListener {
            hideTabSwitcher()
        }

        binding.viewTabsSwitcher.btnNewTabTop.setOnClickListener {
            createNewTab()
        }

        binding.viewTabsSwitcher.fabNewTab.setOnClickListener {
            createNewTab()
        }
    }

    private fun setupSearchBar() {
        binding.etSearchAddress.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_GO || actionId == EditorInfo.IME_ACTION_SEARCH) {
                val input = binding.etSearchAddress.text.toString()
                navigateToInput(input)
                hideKeyboard()
                true
            } else {
                false
            }
        }

        binding.ivVoiceSearch.setOnClickListener {
            handleVoiceSearchClick()
        }

        binding.etSearchAddress.addTextChangedListener(object : TextWatcher {
            override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) {}
            override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) {
                val hasText = !s.isNullOrEmpty()
                val hasFocus = binding.etSearchAddress.hasFocus()
                binding.ivClearInput.isVisible = hasText && hasFocus
                binding.ivVoiceSearch.isVisible = !hasText
            }
            override fun afterTextChanged(s: Editable?) {}
        })

        binding.etSearchAddress.setOnFocusChangeListener { _, hasFocus ->
            val activeUrl = getActiveWebView()?.url.orEmpty()
            if (hasFocus) {
                if (activeUrl.isNotBlank()) {
                    binding.etSearchAddress.setText(activeUrl)
                    binding.etSearchAddress.selectAll()
                }
                binding.ivStarBookmark.isVisible = false
                val hasText = !binding.etSearchAddress.text.isNullOrEmpty()
                binding.ivClearInput.isVisible = hasText
                binding.ivVoiceSearch.isVisible = !hasText
            } else {
                if (activeUrl.isNotBlank()) {
                    binding.etSearchAddress.setText(getDisplayUrl(activeUrl))
                }
                binding.ivClearInput.isVisible = false
                binding.ivVoiceSearch.isVisible = true
                binding.ivStarBookmark.isVisible = binding.webViewContainer.isVisible
            }
        }

        binding.ivClearInput.setOnClickListener {
            binding.etSearchAddress.text?.clear()
        }
    }

    private fun handleVoiceSearchClick() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.RECORD_AUDIO) == PackageManager.PERMISSION_GRANTED) {
            startVoiceSearchIntent()
        } else {
            recordAudioPermissionLauncher.launch(Manifest.permission.RECORD_AUDIO)
        }
    }

    private fun startVoiceSearchIntent() {
        val intent = Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH).apply {
            putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_WEB_SEARCH)
            putExtra(RecognizerIntent.EXTRA_PROMPT, getString(R.string.voice_search_prompt))
        }
        try {
            voiceSearchLauncher.launch(intent)
        } catch (_: ActivityNotFoundException) {
            Toast.makeText(this, R.string.voice_search_unsupported, Toast.LENGTH_SHORT).show()
        }
    }

    private fun setupToolbarControls() {
        binding.btnRefreshStop.setOnClickListener {
            getActiveWebView()?.let { wv ->
                if (isLoading) {
                    wv.stopLoading()
                    setIsLoading(false)
                } else {
                    wv.reload()
                }
            }
        }

        binding.btnRuhiAccountTop.setOnClickListener {
            com.ruhi.browser.ui.sync.SyncBottomSheetDialogFragment().show(
                supportFragmentManager,
                "SyncBottomSheetDialogFragment"
            )
        }

        binding.ivStarBookmark.setOnClickListener {
            toggleBookmarkForCurrentPage()
        }
    }

    private fun setupBottomToolbar() {
        binding.btnBottomHome.setOnClickListener {
            hideTabSwitcher()
            showHomeScreen()
        }

        binding.btnBottomBookmarks.setOnClickListener {
            val intent = Intent(this, BookmarksActivity::class.java)
            bookmarkLauncher.launch(intent)
        }

        binding.btnBottomBookmarks.setOnLongClickListener {
            toggleBookmarkForCurrentPage()
            true
        }

        binding.btnBottomTabs.setOnClickListener {
            showTabSwitcher()
        }

        binding.btnBottomMenu.setOnClickListener {
            val menuSheet = MenuBottomSheetDialogFragment.newInstance()
            menuSheet.show(supportFragmentManager, MenuBottomSheetDialogFragment.TAG)
        }
    }

    private fun setupBackNavigation() {
        onBackPressedDispatcher.addCallback(this, object : OnBackPressedCallback(true) {
            override fun handleOnBackPressed() {
                if (customView != null || binding.customViewContainer.isVisible) {
                    onHideCustomView()
                    return
                }

                if (binding.viewFindInPage.layoutFindInPage.isVisible) {
                    hideFindInPage()
                    return
                }

                if (binding.viewTabsSwitcher.root.isVisible) {
                    hideTabSwitcher()
                    return
                }

                if (binding.webViewContainer.isVisible) {
                    val activeWv = getActiveWebView()
                    if (activeWv != null && activeWv.canGoBack()) {
                        activeWv.goBack()
                    } else {
                        showHomeScreen()
                    }
                } else {
                    isEnabled = false
                    onBackPressedDispatcher.onBackPressed()
                    isEnabled = true
                }
            }
        })
    }

    private fun setupErrorViewActions() {
        binding.viewError.btnRetry.setOnClickListener {
            lastFailedUrl?.let { url ->
                loadUrl(url)
            } ?: getActiveWebView()?.reload()
        }

        binding.viewError.btnErrorHome.setOnClickListener {
            showHomeScreen()
        }
    }

    // --- Bookmark Toggle ---

    private fun toggleBookmarkForCurrentPage() {
        val activeTab = tabManager.getActiveTab() ?: return
        val url = activeTab.url
        if (url.isBlank() || !UrlResolver.isWebScheme(url)) {
            Toast.makeText(this, "Cannot bookmark this page", Toast.LENGTH_SHORT).show()
            return
        }

        val title = if (activeTab.title.isNotBlank()) activeTab.title else url
        val isBookmarked = bookmarkRepository.isBookmarked(url)

        if (isBookmarked) {
            bookmarkRepository.deleteBookmarkByUrl(url)
            updateBookmarkIcon(url)
            Toast.makeText(this, "Bookmark removed", Toast.LENGTH_SHORT).show()
        } else {
            bookmarkRepository.addBookmark(title, url)
            updateBookmarkIcon(url)
            Toast.makeText(this, "Bookmark added", Toast.LENGTH_SHORT).show()
        }
    }

    private fun updateBookmarkIcon(url: String) {
        val isBookmarked = url.isNotBlank() && bookmarkRepository.isBookmarked(url)
        if (isBookmarked) {
            binding.ivBottomBookmark.setImageResource(R.drawable.ic_bookmark_added)
            binding.ivStarBookmark.setImageResource(R.drawable.ic_star_filled)
            binding.ivStarBookmark.setColorFilter(getColor(R.color.brand_primary))
        } else {
            binding.ivBottomBookmark.setImageResource(R.drawable.ic_bookmarks)
            binding.ivStarBookmark.setImageResource(R.drawable.ic_star_outline)
            binding.ivStarBookmark.setColorFilter(null)
        }
        binding.ivStarBookmark.isVisible = binding.webViewContainer.isVisible && !binding.etSearchAddress.hasFocus()
    }

    // --- Tab Management Operations ---

    fun createNewTab(url: String = "", isIncognito: Boolean = false) {
        val tab = tabManager.createTab(url = url, isIncognito = isIncognito, select = true)
        updateTabCountDisplay()
        hideTabSwitcher()
        switchToTab(tab)
        if (url.isNotBlank()) {
            loadUrl(url)
        }
    }

    fun switchToTab(tab: Tab) {
        tabManager.selectTab(tab.id)
        updateTabCountDisplay()

        // Detach current webviews from container
        binding.webViewContainer.removeAllViews()

        hideFindInPage()
        if (tab.url.isBlank()) {
            showHomeScreen()
        } else {
            showBrowserScreen()
            val wv = getOrCreateWebView(tab)
            applyDesktopSiteSettings(wv, tab.isDesktopSite)
            binding.webViewContainer.addView(
                wv,
                ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT,
                    ViewGroup.LayoutParams.MATCH_PARENT
                )
            )

            binding.etSearchAddress.setText(getDisplayUrl(tab.url))
            updateSecurityIndicator(tab.url)
            updateBookmarkIcon(tab.url)
            updateNavigationButtons(wv.canGoBack(), wv.canGoForward())
        }
    }

    fun closeTab(tab: Tab) {
        // Destroy WebView and remove from map to prevent memory leak
        tabWebViews.remove(tab.id)?.let { wv ->
            wv.stopLoading()
            (wv.parent as? ViewGroup)?.removeView(wv)
            wv.destroy()
        }

        val nextTab = tabManager.closeTab(tab.id)
        updateTabCountDisplay()
        tabAdapter.updateTabs(tabManager.getTabs(), tabManager.activeTabId)

        if (tab.isIncognito && tabManager.incognitoTabCount == 0) {
            // When all incognito tabs are closed, purge private session cookies and storage
            android.webkit.CookieManager.getInstance().removeSessionCookies(null)
            android.webkit.WebStorage.getInstance().deleteAllData()
            Toast.makeText(this, "Incognito session ended. Session cookies cleared.", Toast.LENGTH_SHORT).show()
        }

        if (!binding.viewTabsSwitcher.root.isVisible) {
            switchToTab(nextTab)
        }
    }

    private fun showTabSwitcher() {
        hideKeyboard()
        binding.topToolbar.isVisible = false
        binding.bottomToolbar.isVisible = false
        binding.viewTabsSwitcher.tvTabsSwitcherTitle.text =
            "${getString(R.string.nav_tabs)} (${tabManager.count})"
        tabAdapter.updateTabs(tabManager.getTabs(), tabManager.activeTabId)
        binding.viewTabsSwitcher.root.isVisible = true
    }

    private fun hideTabSwitcher() {
        binding.viewTabsSwitcher.root.isVisible = false
        binding.topToolbar.isVisible = true
        binding.bottomToolbar.isVisible = true
    }

    private fun updateTabCountDisplay() {
        binding.tvTabCountBadge.text = tabManager.count.toString()
    }

    private fun getActiveWebView(): WebView? {
        val activeId = tabManager.activeTabId ?: return null
        return tabWebViews[activeId]
    }

    private fun getOrCreateWebView(tab: Tab): WebView {
        tabWebViews[tab.id]?.let { return it }

        val newWv = WebView(this).apply {
            settings.apply {
                javaScriptEnabled = true
                domStorageEnabled = true
                allowFileAccess = false
                allowContentAccess = false
                builtInZoomControls = true
                displayZoomControls = false

                if (tab.isIncognito) {
                    cacheMode = android.webkit.WebSettings.LOAD_NO_CACHE
                }
            }
            applyDesktopSiteSettings(this, tab.isDesktopSite)
            webViewClient = RuhiWebViewClient(this@MainActivity, this@MainActivity)
            webChromeClient = RuhiWebChromeClient(this@MainActivity)

            setDownloadListener { url, userAgent, contentDisposition, mimetype, _ ->
                com.ruhi.browser.downloads.DownloadHelper.startDownload(
                    this@MainActivity,
                    url,
                    userAgent,
                    contentDisposition,
                    mimetype
                )
            }
        }

        tabWebViews[tab.id] = newWv
        return newWv
    }

    // --- Navigation Operations ---

    private fun navigateToInput(input: String) {
        val searchTemplate = preferences.getSearchTemplate()
        val resolved = UrlResolver.resolve(input, searchTemplate)
        if (resolved.isNotEmpty()) {
            loadUrl(resolved)
        }
    }

    fun loadUrl(url: String) {
        lastFailedUrl = null
        showBrowserScreen()

        val activeTab = tabManager.getActiveTab() ?: tabManager.createTab()
        activeTab.url = url
        val wv = getOrCreateWebView(activeTab)
        applyDesktopSiteSettings(wv, activeTab.isDesktopSite)

        if (binding.webViewContainer.indexOfChild(wv) == -1) {
            binding.webViewContainer.removeAllViews()
            binding.webViewContainer.addView(
                wv,
                ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT,
                    ViewGroup.LayoutParams.MATCH_PARENT
                )
            )
        }

        wv.loadUrl(url)
    }

    private fun showHomeScreen() {
        hideFindInPage()
        homeBinding.root.isVisible = true
        binding.webViewContainer.isVisible = false
        binding.viewError.root.isVisible = false
        binding.ivStarBookmark.isVisible = false

        binding.etSearchAddress.text?.clear()
        binding.etSearchAddress.clearFocus()

        val isIncognito = tabManager.getActiveTab()?.isIncognito == true
        homeBinding.cardIncognitoNotice.isVisible = isIncognito
        homeBinding.layoutRecentSection.isVisible = !isIncognito

        if (isIncognito) {
            binding.ivSecurityIndicator.setImageResource(R.drawable.ic_incognito)
            binding.ivSecurityIndicator.contentDescription = "Incognito Tab"
            binding.ivSecurityIndicator.setColorFilter(getColor(R.color.brand_primary))
        } else {
            binding.ivSecurityIndicator.setImageResource(R.drawable.ic_search)
            binding.ivSecurityIndicator.contentDescription = getString(R.string.search_hint)
            binding.ivSecurityIndicator.setColorFilter(null)
        }

        tabManager.updateActiveTab(url = "", title = getString(R.string.app_name))
        updateNavigationButtons(canGoBack = false, canGoForward = false)
        setIsLoading(false)
        loadRecentVisits()
    }

    private fun showBrowserScreen() {
        homeBinding.root.isVisible = false
        binding.webViewContainer.isVisible = true
        binding.viewError.root.isVisible = false
        binding.ivStarBookmark.isVisible = !binding.etSearchAddress.hasFocus()
    }

    private fun showErrorScreen(title: String, message: String) {
        homeBinding.root.isVisible = false
        binding.webViewContainer.isVisible = false
        binding.viewError.root.isVisible = true

        binding.viewError.tvErrorTitle.text = title
        binding.viewError.tvErrorMessage.text = message
        setIsLoading(false)
    }

    private fun handleBackAction() {
        if (binding.webViewContainer.isVisible) {
            val wv = getActiveWebView()
            if (wv != null && wv.canGoBack()) {
                wv.goBack()
            } else {
                showHomeScreen()
            }
        }
    }

    private fun updateNavigationButtons(canGoBack: Boolean, canGoForward: Boolean) {
        // Redundant top navigation buttons removed in Chrome-style layout in favor of system back gesture/button
    }

    private fun updateSecurityIndicator(url: String) {
        if (UrlResolver.isHttps(url)) {
            binding.ivSecurityIndicator.setImageResource(R.drawable.ic_lock)
            binding.ivSecurityIndicator.contentDescription = getString(R.string.secure_connection)
            binding.ivSecurityIndicator.setColorFilter(getColor(R.color.brand_primary))
        } else if (UrlResolver.isWebScheme(url)) {
            binding.ivSecurityIndicator.setImageResource(R.drawable.ic_info)
            binding.ivSecurityIndicator.contentDescription = getString(R.string.insecure_connection)
            binding.ivSecurityIndicator.setColorFilter(getColor(R.color.state_disabled))
        } else {
            binding.ivSecurityIndicator.setImageResource(R.drawable.ic_search)
            binding.ivSecurityIndicator.setColorFilter(null)
        }
    }

    private fun setIsLoading(loading: Boolean) {
        isLoading = loading
        binding.pageProgressBar.isVisible = loading
        if (loading) {
            binding.btnRefreshStop.setImageResource(R.drawable.ic_stop)
            binding.btnRefreshStop.contentDescription = getString(R.string.action_stop)
        } else {
            binding.pageProgressBar.progress = 0
            binding.btnRefreshStop.setImageResource(R.drawable.ic_refresh)
            binding.btnRefreshStop.contentDescription = getString(R.string.action_refresh)
        }
    }

    private fun hideKeyboard() {
        val imm = getSystemService(Context.INPUT_METHOD_SERVICE) as? InputMethodManager
        currentFocus?.let { focus ->
            imm?.hideSoftInputFromWindow(focus.windowToken, 0)
        }
        binding.etSearchAddress.clearFocus()
    }

    private fun showKeyboard(view: View) {
        val imm = getSystemService(Context.INPUT_METHOD_SERVICE) as? InputMethodManager
        imm?.showSoftInput(view, InputMethodManager.SHOW_IMPLICIT)
    }

    // --- Find in Page ---

    private fun setupFindInPage() {
        binding.viewFindInPage.etFindQuery.addTextChangedListener(object : TextWatcher {
            override fun beforeTextChanged(s: CharSequence?, start: Int, count: Int, after: Int) {}
            override fun onTextChanged(s: CharSequence?, start: Int, before: Int, count: Int) {
                val query = s?.toString().orEmpty()
                val wv = getActiveWebView()
                if (query.isNotEmpty()) {
                    wv?.findAllAsync(query)
                } else {
                    wv?.clearMatches()
                    binding.viewFindInPage.tvFindMatchCount.text = "0/0"
                }
            }
            override fun afterTextChanged(s: Editable?) {}
        })

        binding.viewFindInPage.etFindQuery.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_SEARCH) {
                getActiveWebView()?.findNext(true)
                true
            } else {
                false
            }
        }

        binding.viewFindInPage.btnFindNext.setOnClickListener {
            getActiveWebView()?.findNext(true)
        }

        binding.viewFindInPage.btnFindPrev.setOnClickListener {
            getActiveWebView()?.findNext(false)
        }

        binding.viewFindInPage.btnFindClose.setOnClickListener {
            hideFindInPage()
        }
    }

    fun showFindInPage() {
        binding.viewFindInPage.layoutFindInPage.isVisible = true
        binding.viewFindInPage.etFindQuery.requestFocus()
        showKeyboard(binding.viewFindInPage.etFindQuery)
        val wv = getActiveWebView() ?: return
        wv.setFindListener { activeMatchOrdinal, numberOfMatches, _ ->
            val current = if (numberOfMatches > 0) activeMatchOrdinal + 1 else 0
            binding.viewFindInPage.tvFindMatchCount.text = "$current/$numberOfMatches"
        }
        val currentQuery = binding.viewFindInPage.etFindQuery.text?.toString().orEmpty()
        if (currentQuery.isNotEmpty()) {
            wv.findAllAsync(currentQuery)
        }
    }

    fun hideFindInPage() {
        binding.viewFindInPage.layoutFindInPage.isVisible = false
        binding.viewFindInPage.etFindQuery.text?.clear()
        binding.viewFindInPage.tvFindMatchCount.text = "0/0"
        getActiveWebView()?.clearMatches()
        hideKeyboard()
    }

    // --- Desktop Site ---

    fun isCurrentTabDesktopSite(): Boolean {
        return tabManager.getActiveTab()?.isDesktopSite == true
    }

    fun toggleDesktopSite() {
        val activeTab = tabManager.getActiveTab() ?: return
        val newDesktopState = !activeTab.isDesktopSite
        activeTab.isDesktopSite = newDesktopState
        val wv = getActiveWebView() ?: return
        applyDesktopSiteSettings(wv, newDesktopState)
        wv.reload()
    }

    private fun applyDesktopSiteSettings(wv: WebView, isDesktop: Boolean) {
        val settings = wv.settings
        if (isDesktop) {
            settings.userAgentString = DESKTOP_USER_AGENT
            settings.useWideViewPort = true
            settings.loadWithOverviewMode = true
        } else {
            settings.userAgentString = getRuhiMobileUserAgent(settings)
            settings.useWideViewPort = false
            settings.loadWithOverviewMode = false
        }
    }

    private fun getRuhiMobileUserAgent(settings: android.webkit.WebSettings): String {
        val base = settings.userAgentString ?: android.webkit.WebSettings.getDefaultUserAgent(this)
        return base
            .replace("; wv", "")
            .replace("Version/4.0 ", "")
            .trim() + " RuhiBrowser/1.0"
    }

    // --- AI Assistant ---

    fun showAiAssistant() {
        val activeTab = tabManager.getActiveTab()
        val sheet = com.ruhi.browser.ui.ai.AiBottomSheetDialogFragment.newInstance(
            pageTitle = activeTab?.title,
            pageUrl = activeTab?.url,
            isIncognito = activeTab?.isIncognito == true
        )
        sheet.show(supportFragmentManager, com.ruhi.browser.ui.ai.AiBottomSheetDialogFragment.TAG)
    }

    // --- Share, Reader Mode and Clear Data ---

    fun shareCurrentPage() {
        val wv = getActiveWebView()
        val url = wv?.url.orEmpty()
        if (url.isBlank() || !UrlResolver.isWebScheme(url)) {
            Toast.makeText(this, "No web page to share", Toast.LENGTH_SHORT).show()
            return
        }
        val title = wv?.title.orEmpty().ifBlank { FaviconManager.getDomain(url) }
        val shareIntent = Intent(Intent.ACTION_SEND).apply {
            type = "text/plain"
            putExtra(Intent.EXTRA_SUBJECT, title)
            putExtra(Intent.EXTRA_TEXT, "$title\n$url")
        }
        startActivity(Intent.createChooser(shareIntent, "Share via"))
    }

    fun toggleReaderMode() {
        val wv = getActiveWebView() ?: return
        val url = wv.url.orEmpty()
        if (url.isBlank() || !UrlResolver.isWebScheme(url)) {
            Toast.makeText(this, "Reader mode not available for this page", Toast.LENGTH_SHORT).show()
            return
        }

        val readerScript = """
            (function() {
                if (window.__ruhiReaderActive) {
                    window.__ruhiReaderActive = false;
                    location.reload();
                    return 'disabled';
                }
                window.__ruhiReaderActive = true;
                var article = document.querySelector('article') || 
                              document.querySelector('main') || 
                              document.querySelector('[role="main"]') || 
                              document.querySelector('.post-content') || 
                              document.querySelector('.article-body') || 
                              document.querySelector('.content') || 
                              document.body;
                              
                var title = (document.querySelector('h1') ? document.querySelector('h1').innerText : document.title) || '';
                var clone = article.cloneNode(true);
                var unwanted = clone.querySelectorAll('script, style, nav, header, footer, aside, .ad, .advertisement, [role="banner"], [role="navigation"], iframe');
                for (var i = 0; i < unwanted.length; i++) {
                    unwanted[i].remove();
                }
                
                var isDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
                var bgColor = isDark ? '#121212' : '#FBFBFB';
                var textColor = isDark ? '#E0E0E0' : '#212121';
                var linkColor = isDark ? '#8AB4F8' : '#1A73E8';
                
                var readerHtml = '<!DOCTYPE html><html><head><meta name="viewport" content="width=device-width, initial-scale=1.0"><style>' +
                    'body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Georgia, serif; line-height: 1.8; font-size: 19px; max-width: 680px; margin: 0 auto; padding: 24px 20px 60px; background: ' + bgColor + '; color: ' + textColor + '; }' +
                    'h1 { font-size: 28px; line-height: 1.3; margin-bottom: 24px; font-weight: 700; }' +
                    'p { margin-bottom: 20px; }' +
                    'img { max-width: 100%; height: auto; border-radius: 8px; margin: 16px 0; display: block; }' +
                    'a { color: ' + linkColor + '; text-decoration: underline; }' +
                    '.ruhi-badge { display: inline-block; background: #6750A4; color: white; padding: 4px 10px; border-radius: 12px; font-size: 12px; font-weight: bold; margin-bottom: 16px; font-family: sans-serif; }' +
                    '</style></head><body>' +
                    '<div class="ruhi-badge">RUHI Reader Mode</div>' +
                    '<h1>' + title + '</h1>' +
                    clone.innerHTML +
                    '</body></html>';
                    
                document.open();
                document.write(readerHtml);
                document.close();
                return 'enabled';
            })();
        """.trimIndent()

        wv.evaluateJavascript(readerScript) { result ->
            if (result?.contains("enabled") == true) {
                Toast.makeText(this, "Reader mode enabled", Toast.LENGTH_SHORT).show()
            } else {
                Toast.makeText(this, "Reader mode disabled", Toast.LENGTH_SHORT).show()
            }
        }
    }

    fun showClearDataDialog() {
        val options = arrayOf(
            "Browsing history",
            "Cookies and site data",
            "Cached images and files",
            "Web storage and form data"
        )
        val checkedItems = booleanArrayOf(true, true, true, true)

        MaterialAlertDialogBuilder(this)
            .setTitle("Delete browsing data")
            .setMultiChoiceItems(options, checkedItems) { _, which, isChecked ->
                checkedItems[which] = isChecked
            }
            .setPositiveButton("Clear data") { dialog, _ ->
                val clearHistory = checkedItems[0]
                val clearCookies = checkedItems[1]
                val clearCache = checkedItems[2]
                val clearStorage = checkedItems[3]

                if (clearHistory) {
                    dbExecutor.execute {
                        historyRepository.clearAllHistory()
                        com.ruhi.browser.sync.SyncManager.getInstance(this@MainActivity).clearAllHistory()
                        runOnUiThread {
                            loadRecentVisits()
                        }
                    }
                }

                if (clearCookies) {
                    android.webkit.CookieManager.getInstance().removeAllCookies(null)
                    android.webkit.CookieManager.getInstance().flush()
                }

                if (clearStorage) {
                    android.webkit.WebStorage.getInstance().deleteAllData()
                }

                if (clearCache) {
                    tabWebViews.values.forEach { wv ->
                        wv.clearCache(true)
                    }
                }

                Toast.makeText(this, "Browsing data cleared", Toast.LENGTH_SHORT).show()
                dialog.dismiss()
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    // --- Web Page Translation ---

    fun showTranslateDialog() {
        val wv = getActiveWebView()
        val currentUrl = wv?.url.orEmpty()
        if (currentUrl.isBlank() || !UrlResolver.isWebScheme(currentUrl)) {
            Toast.makeText(this, "No webpage to translate", Toast.LENGTH_SHORT).show()
            return
        }

        // Check if currently viewing a Google Translated page
        val isAlreadyTranslated = currentUrl.contains("translate.google.com/translate")
        if (isAlreadyTranslated) {
            val originalUrl = try {
                val uri = Uri.parse(currentUrl)
                uri.getQueryParameter("u")
            } catch (_: Exception) { null }

            val actions = if (!originalUrl.isNullOrBlank()) {
                arrayOf("Translate to another language", "View original webpage")
            } else {
                arrayOf("Translate to another language")
            }

            MaterialAlertDialogBuilder(this)
                .setTitle("Page is translated")
                .setItems(actions) { dialog, which ->
                    dialog.dismiss()
                    if (which == 0) {
                        val targetUrl = originalUrl ?: currentUrl
                        openLanguageSelectionDialog(targetUrl)
                    } else if (which == 1 && !originalUrl.isNullOrBlank()) {
                        loadUrl(originalUrl)
                    }
                }
                .setNegativeButton(android.R.string.cancel, null)
                .show()
            return
        }

        openLanguageSelectionDialog(currentUrl)
    }

    private fun openLanguageSelectionDialog(targetUrl: String) {
        val languages = arrayOf(
            "Hindi (हिन्दी)",
            "English",
            "Bengali (বাংলা)",
            "Marathi (मराठी)",
            "Telugu (తెలుగు)",
            "Tamil (தமிழ்)",
            "Gujarati (ગુજરાતી)",
            "Urdu (اردو)",
            "Punjabi (ਪੰਜਾਬੀ)",
            "Spanish (Español)",
            "French (Français)",
            "Arabic (العربية)"
        )
        val langCodes = arrayOf(
            "hi", "en", "bn", "mr", "te", "ta", "gu", "ur", "pa", "es", "fr", "ar"
        )

        MaterialAlertDialogBuilder(this)
            .setTitle("Translate webpage to")
            .setItems(languages) { dialog, which ->
                val selectedCode = langCodes[which]
                val selectedName = languages[which].substringBefore(" (")
                try {
                    val encoded = java.net.URLEncoder.encode(targetUrl, "UTF-8")
                    val translateUrl = "https://translate.google.com/translate?sl=auto&tl=$selectedCode&u=$encoded"
                    loadUrl(translateUrl)
                    Toast.makeText(this, "Translating to $selectedName...", Toast.LENGTH_SHORT).show()
                } catch (_: Exception) {
                    Toast.makeText(this, "Unable to translate this page", Toast.LENGTH_SHORT).show()
                }
                dialog.dismiss()
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    // --- BrowserEventListener Callbacks ---

    override fun onPageStarted(url: String) {
        setIsLoading(true)
        if (!binding.etSearchAddress.hasFocus()) {
            binding.etSearchAddress.setText(getDisplayUrl(url))
        }
        tabManager.updateActiveTab(url = url, title = "")
        updateSecurityIndicator(url)
        updateBookmarkIcon(url)
        getActiveWebView()?.let { wv ->
            updateNavigationButtons(wv.canGoBack(), wv.canGoForward())
        }
    }

    override fun onPageFinished(url: String, canGoBack: Boolean, canGoForward: Boolean) {
        setIsLoading(false)
        if (!binding.etSearchAddress.hasFocus()) {
            binding.etSearchAddress.setText(getDisplayUrl(url))
        }
        val title = getActiveWebView()?.title.orEmpty()
        tabManager.updateActiveTab(url = url, title = title)
        updateSecurityIndicator(url)
        updateBookmarkIcon(url)
        updateNavigationButtons(canGoBack, canGoForward)

        // Record history: regular tabs only. Incognito tabs are excluded.
        val activeTab = tabManager.getActiveTab()
        if (activeTab != null && !activeTab.isIncognito && UrlResolver.isWebScheme(url)) {
            val pageTitle = if (title.isNotBlank()) title else url
            dbExecutor.execute {
                historyRepository.recordVisit(url, pageTitle, isIncognito = false)
            }
        }
    }

    override fun onPageError(url: String, errorCode: Int, description: String) {
        lastFailedUrl = url
        showErrorScreen(
            title = getString(R.string.error_page_title),
            message = "$description\n($url)"
        )
    }

    override fun onSslError(url: String, sslError: SslError?) {
        lastFailedUrl = url
        showErrorScreen(
            title = "Security Warning",
            message = getString(R.string.error_ssl_message)
        )
    }

    // --- ChromeEventListener Callbacks ---

    override fun onProgressChanged(progress: Int) {
        if (isLoading) {
            binding.pageProgressBar.progress = progress
            if (progress >= 100) {
                setIsLoading(false)
            }
        }
    }

    override fun onReceivedTitle(title: String) {
        if (binding.webViewContainer.isVisible && title.isNotBlank()) {
            tabManager.updateActiveTab(url = getActiveWebView()?.url.orEmpty(), title = title)
        }
    }

    override fun onShowFileChooser(
        filePathCallback: android.webkit.ValueCallback<Array<Uri>>?,
        fileChooserParams: android.webkit.WebChromeClient.FileChooserParams?
    ): Boolean {
        fileChooserCallback?.onReceiveValue(null)
        fileChooserCallback = filePathCallback

        val intent = fileChooserParams?.createIntent() ?: Intent(Intent.ACTION_GET_CONTENT).apply {
            type = "*/*"
            addCategory(Intent.CATEGORY_OPENABLE)
        }

        return try {
            fileChooserLauncher.launch(intent)
            true
        } catch (_: Exception) {
            fileChooserCallback?.onReceiveValue(null)
            fileChooserCallback = null
            false
        }
    }

    override fun onShowCustomView(view: View, callback: android.webkit.WebChromeClient.CustomViewCallback) {
        if (customView != null) {
            callback.onCustomViewHidden()
            return
        }
        customView = view
        customViewCallback = callback

        binding.topToolbar.isVisible = false
        binding.bottomToolbar.isVisible = false
        binding.contentContainer.isVisible = false

        binding.customViewContainer.removeAllViews()
        binding.customViewContainer.addView(
            view,
            ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            )
        )
        binding.customViewContainer.isVisible = true

        // True immersive fullscreen mode
        androidx.core.view.WindowCompat.getInsetsController(window, window.decorView).apply {
            hide(androidx.core.view.WindowInsetsCompat.Type.systemBars())
            systemBarsBehavior = androidx.core.view.WindowInsetsControllerCompat.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE
        }
    }

    override fun onHideCustomView() {
        val cv = customView ?: return
        binding.customViewContainer.removeView(cv)
        binding.customViewContainer.isVisible = false
        customView = null

        customViewCallback?.onCustomViewHidden()
        customViewCallback = null

        binding.topToolbar.isVisible = true
        binding.bottomToolbar.isVisible = true
        binding.contentContainer.isVisible = true

        // Restore normal system bars
        androidx.core.view.WindowCompat.getInsetsController(window, window.decorView).apply {
            show(androidx.core.view.WindowInsetsCompat.Type.systemBars())
        }
    }

    override fun onGeolocationPermissionsShowPrompt(
        origin: String?,
        callback: android.webkit.GeolocationPermissions.Callback?
    ) {
        callback?.invoke(origin, true, false)
    }

    // --- Lifecycle Management ---

    private var syncPeriodicTick = 0
    private val tabSyncHandler = android.os.Handler(android.os.Looper.getMainLooper())
    private val tabSyncRunnable = object : Runnable {
        override fun run() {
            try {
                val sync = com.ruhi.browser.sync.SyncManager.getInstance(this@MainActivity)
                if (sync.isLoggedIn) {
                    // Check for tabs instantly
                    sync.checkIncomingTabs { title, url ->
                        createNewTab(isIncognito = false)
                        loadUrl(url)
                        android.widget.Toast.makeText(this@MainActivity, "💻 Tab received from PC:\n$title", android.widget.Toast.LENGTH_LONG).show()
                    }

                    // Auto background sync for Bookmarks & History every 30s (15 * 2s)
                    syncPeriodicTick++
                    if (syncPeriodicTick % 15 == 0) {
                        sync.refreshUserProfile()
                        val dbHelper = com.ruhi.browser.data.local.RuhiDatabaseHelper(this@MainActivity)
                        val repo = com.ruhi.browser.data.repository.BookmarkRepository(com.ruhi.browser.data.local.BookmarkDao(dbHelper))
                        sync.syncBookmarks(repo)
                        val histRepo = com.ruhi.browser.data.repository.HistoryRepository(com.ruhi.browser.data.local.HistoryDao(dbHelper))
                        sync.syncHistory(histRepo) { modified ->
                            if (modified) {
                                runOnUiThread { loadRecentVisits() }
                            }
                        }
                        sync.updateHeartbeat(tabManager.getTabs().map { it.url })
                    }
                }
            } catch (_: Exception) { }
            tabSyncHandler.postDelayed(this, 2000)
        }
    }

    override fun onPause() {
        super.onPause()
        getActiveWebView()?.onPause()
        tabSyncHandler.removeCallbacks(tabSyncRunnable)
    }

    override fun onResume() {
        super.onResume()
        getActiveWebView()?.onResume()

        try {
            val sync = com.ruhi.browser.sync.SyncManager.getInstance(this)
            if (sync.isLoggedIn) {
                tabSyncHandler.removeCallbacks(tabSyncRunnable)
                tabSyncHandler.post(tabSyncRunnable)

                val dbHelper = com.ruhi.browser.data.local.RuhiDatabaseHelper(this)
                val repo = com.ruhi.browser.data.repository.BookmarkRepository(com.ruhi.browser.data.local.BookmarkDao(dbHelper))
                sync.syncBookmarks(repo)
                val histRepo = com.ruhi.browser.data.repository.HistoryRepository(com.ruhi.browser.data.local.HistoryDao(dbHelper))
                sync.syncHistory(histRepo) { modified ->
                    if (modified) {
                        runOnUiThread { loadRecentVisits() }
                    }
                }
                sync.updateHeartbeat(tabManager.getTabs().map { it.url })
            }
        } catch (_: Exception) { }
    }

    override fun onDestroy() {
        // Destroy all WebViews to prevent memory leaks
        tabWebViews.values.forEach { wv ->
            wv.stopLoading()
            (wv.parent as? ViewGroup)?.removeView(wv)
            wv.destroy()
        }
        tabWebViews.clear()
        dbExecutor.shutdown()
        super.onDestroy()
    }

    // --- URL Display and Instant App / Home Shortcuts ---

    fun getDisplayUrl(url: String): String {
        if (url.isBlank()) return ""
        return try {
            val uri = Uri.parse(url)
            val host = uri.host
            if (!host.isNullOrBlank()) {
                host.removePrefix("www.")
            } else {
                url
            }
        } catch (_: Exception) {
            url
        }
    }

    fun addCurrentPageToHomeScreen() {
        val wv = getActiveWebView() ?: return
        val url = wv.url ?: return
        if (url.isBlank() || !UrlResolver.isWebScheme(url)) {
            Toast.makeText(this, "Cannot add this page to Home screen", Toast.LENGTH_SHORT).show()
            return
        }
        val title = wv.title.orEmpty().ifBlank { FaviconManager.getDomain(url) }

        val dialogView = layoutInflater.inflate(R.layout.dialog_add_to_home, null)
        val etTitle = dialogView.findViewById<com.google.android.material.textfield.TextInputEditText>(R.id.etShortcutTitle)
        val ivIcon = dialogView.findViewById<ImageView>(R.id.ivShortcutPreview)
        etTitle.setText(title)

        val favicon = FaviconManager.getFavicon(url)
        if (favicon != null) {
            ivIcon.setImageBitmap(favicon)
            ivIcon.imageTintList = null
        } else {
            ivIcon.setImageResource(R.drawable.ic_globe)
        }

        MaterialAlertDialogBuilder(this)
            .setTitle("Add to Home screen")
            .setView(dialogView)
            .setPositiveButton("Add") { _, _ ->
                val finalTitle = etTitle.text?.toString()?.trim().orEmpty().ifBlank { title }
                createPinShortcut(finalTitle, url, favicon)
            }
            .setNegativeButton(android.R.string.cancel, null)
            .show()
    }

    private fun createPinShortcut(title: String, url: String, favicon: Bitmap?) {
        val launchIntent = Intent(applicationContext, MainActivity::class.java).apply {
            action = Intent.ACTION_VIEW
            data = Uri.parse(url)
            flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TOP
        }

        if (ShortcutManagerCompat.isRequestPinShortcutSupported(this)) {
            val iconCompat = if (favicon != null) {
                IconCompat.createWithBitmap(favicon)
            } else {
                IconCompat.createWithResource(this, R.drawable.ic_globe)
            }
            val pinShortcutInfo = ShortcutInfoCompat.Builder(this, "shortcut_${System.currentTimeMillis()}")
                .setShortLabel(title)
                .setLongLabel(title)
                .setIcon(iconCompat)
                .setIntent(launchIntent)
                .build()

            ShortcutManagerCompat.requestPinShortcut(this, pinShortcutInfo, null)
            Toast.makeText(this, "Adding \"$title\" to Home screen...", Toast.LENGTH_SHORT).show()
        } else {
            Toast.makeText(this, "Home screen shortcuts not supported by your launcher", Toast.LENGTH_SHORT).show()
        }
    }

    companion object {
        private const val DESKTOP_USER_AGENT =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 RuhiBrowser/1.0"
    }
}
