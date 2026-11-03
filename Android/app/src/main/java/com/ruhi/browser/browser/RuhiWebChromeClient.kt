package com.ruhi.browser.browser

import android.net.Uri
import android.view.View
import android.webkit.GeolocationPermissions
import android.webkit.ValueCallback
import android.webkit.WebChromeClient
import android.webkit.WebView

class RuhiWebChromeClient(
    private val listener: ChromeEventListener
) : WebChromeClient() {

    interface ChromeEventListener {
        fun onProgressChanged(progress: Int)
        fun onReceivedTitle(title: String)
        fun onShowFileChooser(
            filePathCallback: ValueCallback<Array<Uri>>?,
            fileChooserParams: FileChooserParams?
        ): Boolean
        fun onShowCustomView(view: View, callback: CustomViewCallback)
        fun onHideCustomView()
        fun onGeolocationPermissionsShowPrompt(
            origin: String?,
            callback: GeolocationPermissions.Callback?
        )
    }

    override fun onProgressChanged(view: WebView?, newProgress: Int) {
        super.onProgressChanged(view, newProgress)
        listener.onProgressChanged(newProgress)
    }

    override fun onReceivedTitle(view: WebView?, title: String?) {
        super.onReceivedTitle(view, title)
        title?.let { listener.onReceivedTitle(it) }
    }

    override fun onReceivedIcon(view: WebView?, icon: android.graphics.Bitmap?) {
        super.onReceivedIcon(view, icon)
        if (icon != null) {
            com.ruhi.browser.util.FaviconManager.saveFavicon(view?.url, icon)
        }
    }

    override fun onShowFileChooser(
        webView: WebView?,
        filePathCallback: ValueCallback<Array<Uri>>?,
        fileChooserParams: FileChooserParams?
    ): Boolean {
        return listener.onShowFileChooser(filePathCallback, fileChooserParams)
    }

    override fun onShowCustomView(view: View?, callback: CustomViewCallback?) {
        super.onShowCustomView(view, callback)
        if (view != null && callback != null) {
            listener.onShowCustomView(view, callback)
        }
    }

    override fun onHideCustomView() {
        super.onHideCustomView()
        listener.onHideCustomView()
    }

    override fun onGeolocationPermissionsShowPrompt(
        origin: String?,
        callback: GeolocationPermissions.Callback?
    ) {
        listener.onGeolocationPermissionsShowPrompt(origin, callback)
    }
}
