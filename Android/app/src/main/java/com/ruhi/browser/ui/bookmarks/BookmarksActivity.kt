package com.ruhi.browser.ui.bookmarks

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import android.widget.EditText
import android.widget.LinearLayout
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.isVisible
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.ruhi.browser.data.local.BookmarkDao
import com.ruhi.browser.data.local.RuhiDatabaseHelper
import com.ruhi.browser.data.model.Bookmark
import com.ruhi.browser.data.repository.BookmarkRepository
import com.ruhi.browser.databinding.ActivityBookmarksBinding

class BookmarksActivity : AppCompatActivity() {

    private lateinit var binding: ActivityBookmarksBinding
    private lateinit var repository: BookmarkRepository
    private lateinit var adapter: BookmarkAdapter

    companion object {
        const val EXTRA_OPEN_URL = "extra_open_url"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityBookmarksBinding.inflate(layoutInflater)
        setContentView(binding.root)

        val dbHelper = RuhiDatabaseHelper(this)
        repository = BookmarkRepository(BookmarkDao(dbHelper))

        setupToolbar()
        setupRecyclerView()
        loadBookmarks()
    }

    override fun onResume() {
        super.onResume()
        val sync = com.ruhi.browser.sync.SyncManager.getInstance(this)
        if (sync.isLoggedIn) {
            sync.syncBookmarks(repository) {
                runOnUiThread { loadBookmarks() }
            }
        }
        loadBookmarks()
    }

    private fun setupToolbar() {
        binding.bookmarksToolbar.setNavigationOnClickListener {
            finish()
        }

        binding.btnAddBookmark.setOnClickListener {
            showAddBookmarkDialog()
        }
    }

    private fun setupRecyclerView() {
        adapter = BookmarkAdapter(
            bookmarks = emptyList(),
            onBookmarkClick = { bookmark ->
                val resultIntent = Intent().apply {
                    putExtra(EXTRA_OPEN_URL, bookmark.url)
                }
                setResult(Activity.RESULT_OK, resultIntent)
                finish()
            },
            onBookmarkEdit = { bookmark ->
                showEditBookmarkDialog(bookmark)
            },
            onBookmarkDelete = { bookmark ->
                repository.deleteBookmark(bookmark.id)
                loadBookmarks()
                val sync = com.ruhi.browser.sync.SyncManager.getInstance(this)
                sync.deleteBookmark(bookmark.url)
                sync.syncBookmarks(repository)
            }
        )

        binding.rvBookmarks.layoutManager = LinearLayoutManager(this)
        binding.rvBookmarks.adapter = adapter
        binding.rvBookmarks.addItemDecoration(
            DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        )
    }

    private fun loadBookmarks() {
        val list = repository.getBookmarks()
        adapter.updateList(list)
        binding.layoutBookmarksEmpty.isVisible = list.isEmpty()
        binding.rvBookmarks.isVisible = list.isNotEmpty()
    }

    private fun showAddBookmarkDialog() {
        val titleInput = EditText(this).apply {
            hint = "Page Title"
        }
        val urlInput = EditText(this).apply {
            hint = "https://example.com"
        }

        val container = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(50, 20, 50, 20)
            addView(titleInput)
            addView(urlInput)
        }

        MaterialAlertDialogBuilder(this)
            .setTitle("Add Bookmark")
            .setView(container)
            .setPositiveButton("Save") { _, _ ->
                val title = titleInput.text.toString().trim()
                val url = urlInput.text.toString().trim()
                if (url.isNotBlank()) {
                    repository.addBookmark(title, url)
                    loadBookmarks()
                    com.ruhi.browser.sync.SyncManager.getInstance(this).syncBookmarks(repository)
                }
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun showEditBookmarkDialog(bookmark: Bookmark) {
        val titleInput = EditText(this).apply {
            setText(bookmark.title)
        }
        val urlInput = EditText(this).apply {
            setText(bookmark.url)
        }

        val container = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(50, 20, 50, 20)
            addView(titleInput)
            addView(urlInput)
        }

        MaterialAlertDialogBuilder(this)
            .setTitle("Edit Bookmark")
            .setView(container)
            .setPositiveButton("Update") { _, _ ->
                val title = titleInput.text.toString().trim()
                val url = urlInput.text.toString().trim()
                if (url.isNotBlank()) {
                    repository.updateBookmark(bookmark.copy(title = title, url = url))
                    loadBookmarks()
                }
            }
            .setNegativeButton("Cancel", null)
            .show()
    }
}
