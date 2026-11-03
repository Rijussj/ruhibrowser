package com.ruhi.browser.ui.history

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.data.model.HistoryItem
import com.ruhi.browser.databinding.ItemHistoryBinding

class HistoryAdapter(
    private var historyList: List<HistoryItem>,
    private val onHistoryClick: (HistoryItem) -> Unit,
    private val onHistoryDelete: (HistoryItem) -> Unit
) : RecyclerView.Adapter<HistoryAdapter.HistoryViewHolder>() {

    class HistoryViewHolder(val binding: ItemHistoryBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): HistoryViewHolder {
        val binding = ItemHistoryBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return HistoryViewHolder(binding)
    }

    override fun onBindViewHolder(holder: HistoryViewHolder, position: Int) {
        val item = historyList[position]
        holder.binding.tvHistoryTitle.text = item.title
        holder.binding.tvHistoryUrl.text = item.url

        holder.binding.root.setOnClickListener {
            onHistoryClick(item)
        }

        holder.binding.btnHistoryDelete.setOnClickListener {
            onHistoryDelete(item)
        }
    }

    override fun getItemCount(): Int = historyList.size

    fun updateList(newList: List<HistoryItem>) {
        historyList = newList
        notifyDataSetChanged()
    }
}
