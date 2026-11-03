import SwiftUI
import WebKit

struct WebViewContainer: NSViewRepresentable {
    @ObservedObject var tab: BrowserTab

    func makeNSView(context: Context) -> WKWebView {
        tab.webView.navigationDelegate = context.coordinator
        tab.webView.uiDelegate = context.coordinator

        // KVO observers for progress, title, url, canGoBack, canGoForward
        context.coordinator.setupObservers(for: tab.webView)

        AdBlockManager.shared.apply(to: tab.webView.configuration)
        return tab.webView
    }

    func updateNSView(_ nsView: WKWebView, context: Context) {
        // Tab webView is already preserved in memory
    }

    func makeCoordinator() -> Coordinator {
        Coordinator(tab: tab)
    }

    class Coordinator: NSObject, WKNavigationDelegate, WKUIDelegate {
        var tab: BrowserTab
        private var observers: [NSKeyValueObservation] = []

        init(tab: BrowserTab) {
            self.tab = tab
            super.init()
        }

        func setupObservers(for webView: WKWebView) {
            observers.removeAll()

            observers.append(webView.observe(\.title, options: .new) { [weak self] wv, _ in
                DispatchQueue.main.async {
                    if let title = wv.title, !title.isEmpty {
                        self?.tab.title = title
                    }
                }
            })

            observers.append(webView.observe(\.url, options: .new) { [weak self] wv, _ in
                DispatchQueue.main.async {
                    self?.tab.url = wv.url
                    self?.tab.urlString = wv.url?.absoluteString ?? ""
                }
            })

            observers.append(webView.observe(\.isLoading, options: .new) { [weak self] wv, _ in
                DispatchQueue.main.async {
                    self?.tab.isLoading = wv.isLoading
                }
            })

            observers.append(webView.observe(\.estimatedProgress, options: .new) { [weak self] wv, _ in
                DispatchQueue.main.async {
                    self?.tab.estimatedProgress = wv.estimatedProgress
                }
            })

            observers.append(webView.observe(\.canGoBack, options: .new) { [weak self] wv, _ in
                DispatchQueue.main.async {
                    self?.tab.canGoBack = wv.canGoBack
                }
            })

            observers.append(webView.observe(\.canGoForward, options: .new) { [weak self] wv, _ in
                DispatchQueue.main.async {
                    self?.tab.canGoForward = wv.canGoForward
                }
            })
        }

        func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
            if let url = webView.url?.absoluteString, !tab.isIncognito {
                let title = webView.title ?? url
                HistoryManager.shared.recordVisit(title: title, url: url)
            }
        }

        func webView(_ webView: WKWebView, createWebViewWith configuration: WKWebViewConfiguration, for navigationAction: WKNavigationAction, windowFeatures: WKWindowFeatures) -> WKWebView? {
            if navigationAction.targetFrame == nil {
                webView.load(navigationAction.request)
            }
            return nil
        }
    }
}
