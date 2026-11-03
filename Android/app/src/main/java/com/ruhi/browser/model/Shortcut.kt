package com.ruhi.browser.model

import androidx.annotation.DrawableRes
import com.ruhi.browser.R

data class Shortcut(
    val title: String,
    val url: String,
    @param:DrawableRes val iconRes: Int = R.drawable.ic_globe
)

