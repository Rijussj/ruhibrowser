package com.ruhi.browser.ui.home

import android.content.res.ColorStateList
import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.view.isVisible
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.data.model.HistoryItem
import com.ruhi.browser.databinding.ItemRecentVisitBinding
import com.ruhi.browser.util.FaviconManager

class RecentVisitsAdapter(
    private var visits: List<HistoryItem>,
    private val onItemClick: (HistoryItem) -> Unit
) : RecyclerView.Adapter<RecentVisitsAdapter.RecentViewHolder>() {

    class RecentViewHolder(val binding: ItemRecentVisitBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecentViewHolder {
        val binding = ItemRecentVisitBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return RecentViewHolder(binding)
    }

    override fun onBindViewHolder(holder: RecentViewHolder, position: Int) {
        val item = visits[position]
        val cleanDomain = FaviconManager.getDomain(item.url)
        holder.binding.tvRecentTitle.text = if (item.title.isNotBlank()) item.title else cleanDomain
        holder.binding.tvRecentUrl.text = cleanDomain

        val cached = FaviconManager.getFavicon(item.url)
        if (cached != null) {
            holder.binding.ivRecentFavicon.isVisible = true
            holder.binding.tvRecentInitial.isVisible = false
            holder.binding.ivRecentFavicon.setImageBitmap(cached)
            holder.binding.ivRecentFavicon.imageTintList = null
            holder.binding.layoutRecentFaviconContainer.backgroundTintList = null
        } else {
            val initial = FaviconManager.getDomainInitial(item.url)
            val bgColor = FaviconManager.getDomainColor(item.url)
            holder.binding.ivRecentFavicon.isVisible = false
            holder.binding.tvRecentInitial.isVisible = true
            holder.binding.tvRecentInitial.text = initial
            holder.binding.layoutRecentFaviconContainer.backgroundTintList = ColorStateList.valueOf(bgColor)

            FaviconManager.fetchFavicon(item.url) { bitmap ->
                if (holder.bindingAdapterPosition == position && bitmap != null) {
                    holder.binding.ivRecentFavicon.isVisible = true
                    holder.binding.tvRecentInitial.isVisible = false
                    holder.binding.ivRecentFavicon.setImageBitmap(bitmap)
                    holder.binding.ivRecentFavicon.imageTintList = null
                    holder.binding.layoutRecentFaviconContainer.backgroundTintList = null
                }
            }
        }

        holder.binding.root.setOnClickListener {
            onItemClick(item)
        }
    }

    override fun getItemCount(): Int = visits.size

    fun updateData(newVisits: List<HistoryItem>) {
        visits = newVisits
        notifyDataSetChanged()
    }
}
