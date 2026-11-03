package com.ruhi.browser.ui.home

import android.content.res.ColorStateList
import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.view.isVisible
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.databinding.ItemShortcutBinding
import com.ruhi.browser.model.Shortcut
import com.ruhi.browser.util.FaviconManager

class ShortcutAdapter(
    shortcuts: List<Shortcut>,
    private val onShortcutClick: (Shortcut) -> Unit,
    private val onShortcutLongClick: ((Shortcut, Int) -> Unit)? = null
) : RecyclerView.Adapter<ShortcutAdapter.ShortcutViewHolder>() {

    private val shortcuts: MutableList<Shortcut> = shortcuts.toMutableList()

    class ShortcutViewHolder(val binding: ItemShortcutBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ShortcutViewHolder {
        val binding = ItemShortcutBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return ShortcutViewHolder(binding)
    }

    override fun onBindViewHolder(holder: ShortcutViewHolder, position: Int) {
        val item = shortcuts[position]
        holder.binding.tvShortcutTitle.text = item.title

        val cached = FaviconManager.getFavicon(item.url)
        if (cached != null) {
            holder.binding.ivShortcutIcon.isVisible = true
            holder.binding.tvShortcutInitial.isVisible = false
            holder.binding.ivShortcutIcon.setImageBitmap(cached)
            holder.binding.layoutShortcutCircle.backgroundTintList = null
        } else {
            val initial = FaviconManager.getDomainInitial(item.url)
            val bgColor = FaviconManager.getDomainColor(item.url)
            holder.binding.ivShortcutIcon.isVisible = false
            holder.binding.tvShortcutInitial.isVisible = true
            holder.binding.tvShortcutInitial.text = initial
            holder.binding.layoutShortcutCircle.backgroundTintList = ColorStateList.valueOf(bgColor)

            FaviconManager.fetchFavicon(item.url) { bitmap ->
                if (holder.bindingAdapterPosition == position && bitmap != null) {
                    holder.binding.ivShortcutIcon.isVisible = true
                    holder.binding.tvShortcutInitial.isVisible = false
                    holder.binding.ivShortcutIcon.setImageBitmap(bitmap)
                    holder.binding.layoutShortcutCircle.backgroundTintList = null
                }
            }
        }

        holder.binding.root.setOnClickListener {
            onShortcutClick(item)
        }
        holder.binding.root.setOnLongClickListener {
            val pos = holder.bindingAdapterPosition
            if (pos != RecyclerView.NO_POSITION) {
                onShortcutLongClick?.invoke(item, pos)
            }
            true
        }
    }

    override fun getItemCount(): Int = shortcuts.size

    fun updateData(newShortcuts: List<Shortcut>) {
        val safeCopy = newShortcuts.toList()
        shortcuts.clear()
        shortcuts.addAll(safeCopy)
        notifyDataSetChanged()
    }
}
