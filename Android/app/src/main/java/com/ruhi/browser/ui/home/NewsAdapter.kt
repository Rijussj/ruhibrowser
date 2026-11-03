package com.ruhi.browser.ui.home

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.view.isVisible
import androidx.recyclerview.widget.RecyclerView
import com.ruhi.browser.databinding.ItemNewsArticleBinding
import com.ruhi.browser.model.NewsArticle

class NewsAdapter(
    private var articles: List<NewsArticle>,
    private val onArticleClick: (NewsArticle) -> Unit
) : RecyclerView.Adapter<NewsAdapter.NewsViewHolder>() {

    class NewsViewHolder(val binding: ItemNewsArticleBinding) : RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): NewsViewHolder {
        val binding = ItemNewsArticleBinding.inflate(
            LayoutInflater.from(parent.context),
            parent,
            false
        )
        return NewsViewHolder(binding)
    }

    override fun onBindViewHolder(holder: NewsViewHolder, position: Int) {
        val item = articles[position]
        holder.binding.tvNewsCategoryBadge.text = item.category.uppercase()
        holder.binding.tvNewsSource.text = item.source
        holder.binding.tvNewsTime.text = item.timeAgo
        holder.binding.tvNewsTitle.text = item.title
        holder.binding.tvNewsDescription.text = item.description
        holder.binding.tvNewsDescription.isVisible = item.description.isNotBlank()

        holder.binding.root.setOnClickListener {
            onArticleClick(item)
        }
    }

    override fun getItemCount(): Int = articles.size

    fun updateData(newArticles: List<NewsArticle>) {
        this.articles = newArticles
        notifyDataSetChanged()
    }
}
