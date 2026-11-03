package com.ruhi.browser.ui.downloads

import android.app.DownloadManager
import android.content.ActivityNotFoundException
import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.ViewGroup
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.isVisible
import androidx.recyclerview.widget.DividerItemDecoration
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.databinding.ActivityDownloadsBinding
import com.ruhi.browser.databinding.ItemDownloadBinding

data class DownloadItem(
    val id: Long,
    val title: String,
    val totalBytes: Long,
    val status: Int,
    val localUri: String?
)

class DownloadAdapter(
    private var items: List<DownloadItem>,
    private val onItemClick: (DownloadItem) -> Unit
) : RecyclerView.Adapter<DownloadAdapter.DownloadViewHolder>() {

    class DownloadViewHolder(val binding: ItemDownloadBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): DownloadViewHolder {
        val binding = ItemDownloadBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return DownloadViewHolder(binding)
    }

    override fun onBindViewHolder(holder: DownloadViewHolder, position: Int) {
        val item = items[position]
        holder.binding.tvDownloadTitle.text = item.title

        val statusText = when (item.status) {
            DownloadManager.STATUS_SUCCESSFUL -> "Completed"
            DownloadManager.STATUS_RUNNING -> "Downloading..."
            DownloadManager.STATUS_PAUSED -> "Paused"
            DownloadManager.STATUS_FAILED -> "Failed"
            else -> "Pending"
        }

        val sizeText = if (item.totalBytes > 0) {
            String.format(" • %.1f MB", item.totalBytes / (1024.0 * 1024.0))
        } else {
            ""
        }

        holder.binding.tvDownloadStatus.text = "$statusText$sizeText"

        holder.binding.root.setOnClickListener {
            onItemClick(item)
        }
    }

    override fun getItemCount(): Int = items.size

    fun updateItems(newItems: List<DownloadItem>) {
        items = newItems
        notifyDataSetChanged()
    }
}

class DownloadsActivity : AppCompatActivity() {

    private lateinit var binding: ActivityDownloadsBinding
    private lateinit var adapter: DownloadAdapter

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityDownloadsBinding.inflate(layoutInflater)
        setContentView(binding.root)

        binding.downloadsToolbar.setNavigationOnClickListener {
            finish()
        }

        adapter = DownloadAdapter(emptyList()) { item ->
            openDownloadedFile(item)
        }

        binding.rvDownloads.layoutManager = LinearLayoutManager(this)
        binding.rvDownloads.adapter = adapter
        binding.rvDownloads.addItemDecoration(
            DividerItemDecoration(this, DividerItemDecoration.VERTICAL)
        )

        loadDownloads()
    }

    private fun loadDownloads() {
        val downloadManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
        val query = DownloadManager.Query()
        val cursor = downloadManager.query(query)
        val list = mutableListOf<DownloadItem>()

        cursor?.use { c ->
            val idIdx = c.getColumnIndex(DownloadManager.COLUMN_ID)
            val titleIdx = c.getColumnIndex(DownloadManager.COLUMN_TITLE)
            val totalBytesIdx = c.getColumnIndex(DownloadManager.COLUMN_TOTAL_SIZE_BYTES)
            val statusIdx = c.getColumnIndex(DownloadManager.COLUMN_STATUS)
            val uriIdx = c.getColumnIndex(DownloadManager.COLUMN_LOCAL_URI)

            while (c.moveToNext()) {
                val id = if (idIdx != -1) c.getLong(idIdx) else 0L
                val title = if (titleIdx != -1) c.getString(titleIdx) ?: "Download" else "Download"
                val totalBytes = if (totalBytesIdx != -1) c.getLong(totalBytesIdx) else 0L
                val status = if (statusIdx != -1) c.getInt(statusIdx) else 0
                val localUri = if (uriIdx != -1) c.getString(uriIdx) else null

                list.add(DownloadItem(id, title, totalBytes, status, localUri))
            }
        }

        adapter.updateItems(list)
        binding.layoutDownloadsEmpty.isVisible = list.isEmpty()
        binding.rvDownloads.isVisible = list.isNotEmpty()
    }

    private fun openDownloadedFile(item: DownloadItem) {
        if (item.status != DownloadManager.STATUS_SUCCESSFUL) {
            Toast.makeText(this, "File is not finished downloading yet", Toast.LENGTH_SHORT).show()
            return
        }

        val downloadManager = getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager
        try {
            val fileUri = downloadManager.getUriForDownloadedFile(item.id)
            if (fileUri != null) {
                val intent = Intent(Intent.ACTION_VIEW).apply {
                    setDataAndType(fileUri, downloadManager.getMimeTypeForDownloadedFile(item.id))
                    addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
                    addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
                }
                startActivity(intent)
            } else {
                Toast.makeText(this, "Cannot access downloaded file", Toast.LENGTH_SHORT).show()
            }
        } catch (e: ActivityNotFoundException) {
            Toast.makeText(this, "No app available to open this file type", Toast.LENGTH_SHORT).show()
        } catch (e: Exception) {
            Toast.makeText(this, "Unable to open file: ${e.message}", Toast.LENGTH_SHORT).show()
        }
    }
}
