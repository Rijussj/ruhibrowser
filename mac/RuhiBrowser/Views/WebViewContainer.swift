import SwiftUI
import WebKit

struct WebViewContainer: NSViewRepresentable {
    @ObservedObject var tab: BrowserTab

    func makeNSView(context: Context) -> WKWebView {
        tab.webView.navigationDelegate = context.coordinator
        tab.webView.uiDelegate = context.coordinator

        context.coordinator.setupObservers(for: tab.webView)

        // Apply Content Blocker
        AdBlockManager.shared.updateActiveController(tab.webView.configuration.userContentController)

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
        private var notificationTokens: [NSObjectProtocol] = []

        init(tab: BrowserTab) {
            self.tab = tab
            super.init()

            // Observe dynamic ad block toggling
            let adToken1 = NotificationCenter.default.addObserver(
                forName: NSNotification.Name("RuhiAdBlockSettingsChanged"),
                object: nil,
                queue: .main
            ) { [weak self] _ in
                guard let self = self else { return }
                AdBlockManager.shared.updateActiveController(self.tab.webView.configuration.userContentController)
            }

            let adToken2 = NotificationCenter.default.addObserver(
                forName: NSNotification.Name("RuhiAdBlockRulesReady"),
                object: nil,
                queue: .main
            ) { [weak self] _ in
                guard let self = self else { return }
                AdBlockManager.shared.updateActiveController(self.tab.webView.configuration.userContentController)
            }

            notificationTokens = [adToken1, adToken2]
        }

        deinit {
            for token in notificationTokens {
                NotificationCenter.default.removeObserver(token)
            }
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
                    let str = wv.url?.absoluteString ?? ""
                    self?.tab.urlString = str
                    if !str.isEmpty && str != "about:blank" && str != "ruhi://home" {
                        self?.tab.isHome = false
                    }
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

        // MARK: - Navigation & Download Handling

        func webView(
            _ webView: WKWebView,
            decidePolicyFor navigationAction: WKNavigationAction,
            decisionHandler: @escaping (WKNavigationActionPolicy) -> Void
        ) {
            if navigationAction.shouldPerformDownload {
                decisionHandler(.download)
                return
            }
            decisionHandler(.allow)
        }

        func webView(
            _ webView: WKWebView,
            decidePolicyFor navigationResponse: WKNavigationResponse,
            decisionHandler: @escaping (WKNavigationResponsePolicy) -> Void
        ) {
            // Automatically download files that WebKit cannot render natively (PDF downloads, ZIP, APK, DMG, EXE, etc.)
            if !navigationResponse.canShowMIMEType {
                decisionHandler(.download)
                return
            }
            decisionHandler(.allow)
        }

        func webView(_ webView: WKWebView, navigationAction: WKNavigationAction, didBecome download: WKDownload) {
            download.delegate = DownloadManager.shared
        }

        func webView(_ webView: WKWebView, navigationResponse: WKNavigationResponse, didBecome download: WKDownload) {
            download.delegate = DownloadManager.shared
        }

        func webView(
            _ webView: WKWebView,
            didReceive challenge: URLAuthenticationChallenge,
            completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void
        ) {
            // Secure standard SSL certificate handling
            completionHandler(.performDefaultHandling, nil)
        }

        func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
            if let url = webView.url?.absoluteString, !tab.isIncognito {
                let title = webView.title ?? url
                HistoryManager.shared.recordVisit(title: title, url: url)
            }
        }

        // MARK: - Popups & New Window Handling ("target=_blank")

        func webView(
            _ webView: WKWebView,
            createWebViewWith configuration: WKWebViewConfiguration,
            for navigationAction: WKNavigationAction,
            windowFeatures: WKWindowFeatures
        ) -> WKWebView? {
            if navigationAction.targetFrame == nil {
                // Open new link in a clean, separate browser tab rather than hijacking the active tab
                if let targetUrl = navigationAction.request.url {
                    DispatchQueue.main.async {
                        NotificationCenter.default.post(name: NSNotification.Name("RuhiNewTabWithUrl"), object: targetUrl)
                    }
                }
            }
            return nil
        }
    }
}
