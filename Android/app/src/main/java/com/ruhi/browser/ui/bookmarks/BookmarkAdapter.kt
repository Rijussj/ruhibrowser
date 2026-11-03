package com.ruhi.browser.ui.bookmarks

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.data.model.Bookmark
import com.ruhi.browser.databinding.ItemBookmarkBinding

class BookmarkAdapter(
    private var bookmarks: List<Bookmark>,
    private val onBookmarkClick: (Bookmark) -> Unit,
    private val onBookmarkEdit: (Bookmark) -> Unit,
    private val onBookmarkDelete: (Bookmark) -> Unit
) : RecyclerView.Adapter<BookmarkAdapter.BookmarkViewHolder>() {

    class BookmarkViewHolder(val binding: ItemBookmarkBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): BookmarkViewHolder {
        val binding = ItemBookmarkBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return BookmarkViewHolder(binding)
    }

    override fun onBindViewHolder(holder: BookmarkViewHolder, position: Int) {
        val item = bookmarks[position]
        holder.binding.tvBookmarkTitle.text = item.title
        holder.binding.tvBookmarkUrl.text = item.url

        holder.binding.root.setOnClickListener {
            onBookmarkClick(item)
        }

        holder.binding.btnBookmarkEdit.setOnClickListener {
            onBookmarkEdit(item)
        }

        holder.binding.btnBookmarkDelete.setOnClickListener {
            onBookmarkDelete(item)
        }
    }

    override fun getItemCount(): Int = bookmarks.size

    fun updateList(newList: List<Bookmark>) {
        bookmarks = newList
        notifyDataSetChanged()
    }
}
