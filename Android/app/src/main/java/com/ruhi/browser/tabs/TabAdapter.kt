package com.ruhi.browser.tabs

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.content.ContextCompat
import androidx.core.view.isVisible
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.R
import com.ruhi.browser.databinding.ItemTabCardBinding

class TabAdapter(
    private var tabs: List<Tab>,
    private var activeTabId: String?,
    private val onTabClick: (Tab) -> Unit,
    private val onTabClose: (Tab) -> Unit
) : RecyclerView.Adapter<TabAdapter.TabViewHolder>() {

    class TabViewHolder(val binding: ItemTabCardBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): TabViewHolder {
        val binding = ItemTabCardBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return TabViewHolder(binding)
    }

    override fun onBindViewHolder(holder: TabViewHolder, position: Int) {
        val tab = tabs[position]
        val context = holder.itemView.context
        val isActive = (tab.id == activeTabId)

        holder.binding.tvTabTitle.text = if (tab.title.isNotBlank()) tab.title else context.getString(R.string.menu_new_tab)
        holder.binding.tvTabUrl.text = if (tab.url.isNotBlank()) tab.url else "ruhi://home"

        if (tab.isIncognito) {
            holder.binding.ivTabFavicon.setImageResource(R.drawable.ic_incognito)
            holder.binding.ivTabFavicon.imageTintList = null
            holder.binding.tabCardHeader.setBackgroundColor(ContextCompat.getColor(context, R.color.surface_variant_dark))
        } else {
            val favicon = com.ruhi.browser.util.FaviconManager.getFavicon(tab.url)
            if (favicon != null) {
                holder.binding.ivTabFavicon.setImageBitmap(favicon)
                holder.binding.ivTabFavicon.imageTintList = null
            } else {
                holder.binding.ivTabFavicon.setImageResource(R.drawable.ic_globe)
                holder.binding.ivTabFavicon.setColorFilter(ContextCompat.getColor(context, R.color.brand_primary))
            }
            holder.binding.tabCardHeader.setBackgroundColor(ContextCompat.getColor(context, R.color.surface_variant_light))
        }

        // Highlight active tab
        if (isActive) {
            holder.binding.cardTab.strokeColor = ContextCompat.getColor(context, R.color.brand_primary)
            holder.binding.cardTab.strokeWidth = 4
            holder.binding.tvActiveBadge.isVisible = true
        } else {
            holder.binding.cardTab.strokeColor = ContextCompat.getColor(context, R.color.divider_light)
            holder.binding.cardTab.strokeWidth = 2
            holder.binding.tvActiveBadge.isVisible = false
        }

        holder.binding.cardTab.setOnClickListener {
            onTabClick(tab)
        }

        holder.binding.btnTabClose.setOnClickListener {
            onTabClose(tab)
        }
    }

    override fun getItemCount(): Int = tabs.size

    fun updateTabs(newTabs: List<Tab>, newActiveTabId: String?) {
        tabs = newTabs
        activeTabId = newActiveTabId
        notifyDataSetChanged()
    }
}
