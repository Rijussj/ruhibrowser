package com.ruhi.browser.ui.history

import android.app.Activity
import android.content.Intent
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.isVisible
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import com.google.android.material.dialog.MaterialAlertDialogBuilder
import com.ruhi.browser.data.local.HistoryDao
import com.ruhi.browser.data.local.RuhiDatabaseHelper
import com.ruhi.browser.data.repository.HistoryRepository
import com.ruhi.browser.databinding.ActivityHistoryBinding

class HistoryActivity : AppCompatActivity() {

    private lateinit var binding: ActivityHistoryBinding
    private lateinit var repository: HistoryRepository
    private lateinit var adapter: HistoryAdapter

    companion object {
        const val EXTRA_OPEN_URL = "extra_open_url"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityHistoryBinding.inflate(layoutInflater)
        setContentView(binding.root)

        val dbHelper = RuhiDatabaseHelper(this)
        repository = HistoryRepository(HistoryDao(dbHelper))

        setupToolbar()
        setupRecyclerView()
        loadHistory()
    }

    override fun onResume() {
        super.onResume()
        val sync = com.ruhi.browser.sync.SyncManager.getInstance(this)
        if (sync.isLoggedIn) {
            sync.syncHistory(repository) {
                runOnUiThread { loadHistory() }
            }
        }
        loadHistory()
    }

    private fun setupToolbar() {
        binding.historyToolbar.setNavigationOnClickListener {
            finish()
        }

        binding.btnClearAllHistory.setOnClickListener {
            showClearAllDialog()
        }
    }

    private fun setupRecyclerView() {
        adapter = HistoryAdapter(
            historyList = emptyList(),
            onHistoryClick = { item ->
                val resultIntent = Intent().apply {
                    putExtra(EXTRA_OPEN_URL, item.url)
                }
                setResult(Activity.RESULT_OK, resultIntent)
                finish()
            },
            onHistoryDelete = { item ->
                repository.deleteItem(item.id)
                loadHistory()
                com.ruhi.browser.sync.SyncManager.getInstance(this).deleteHistoryItem(item.url)
            }
        )

        binding.rvHistory.layoutManager = LinearLayoutManager(this)
        binding.rvHistory.adapter = adapter
        binding.rvHistory.addItemDecoration(
            DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        )
    }

    private fun loadHistory() {
        val list = repository.getHistory()
        adapter.updateList(list)
        binding.layoutHistoryEmpty.isVisible = list.isEmpty()
        binding.rvHistory.isVisible = list.isNotEmpty()
        binding.btnClearAllHistory.isVisible = list.isNotEmpty()
    }

    private fun showClearAllDialog() {
        MaterialAlertDialogBuilder(this)
            .setTitle("Clear Browsing History")
            .setMessage("Are you sure you want to clear all browsing history?")
            .setPositiveButton("Clear") { _, _ ->
                repository.clearAllHistory()
                loadHistory()
                com.ruhi.browser.sync.SyncManager.getInstance(this).clearAllHistory()
            }
            .setNegativeButton("Cancel", null)
            .show()
    }
}
