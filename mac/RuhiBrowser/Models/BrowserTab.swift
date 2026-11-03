import Foundation
import WebKit
import AppKit

class BrowserTab: Identifiable, ObservableObject {
    let id: UUID
    @Published var title: String
    @Published var url: URL?
    @Published var urlString: String
    @Published var favicon: NSImage?
    @Published var isLoading: Bool = false
    @Published var estimatedProgress: Double = 0.0
    @Published var canGoBack: Bool = false
    @Published var canGoForward: Bool = false
    let isIncognito: Bool
    
    let webView: WKWebView

    init(
        id: UUID = UUID(),
        title: String = "New Tab",
        initialUrl: URL? = nil,
        isIncognito: Bool = false,
        configuration: WKWebViewConfiguration? = nil
    ) {
        self.id = id
        self.title = title
        self.url = initialUrl
        self.urlString = initialUrl?.absoluteString ?? ""
        self.isIncognito = isIncognito

        let config = configuration ?? WKWebViewConfiguration()
        if isIncognito {
            config.websiteDataStore = .nonPersistent()
        } else {
            config.websiteDataStore = .default()
        }
        
        // Native Apple WebKit preferences
        config.preferences.javaScriptCanOpenWindowsAutomatically = true
        config.defaultWebpagePreferences.allowsContentJavaScript = true

        self.webView = WKWebView(frame: .zero, configuration: config)
        self.webView.allowsBackForwardNavigationGestures = true
        
        // Set standard modern Safari desktop User Agent
        self.webView.customUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.5 Safari/605.1.15 RuhiBrowser/1.0"
        
        if let initialUrl = initialUrl {
            self.webView.load(URLRequest(url: initialUrl))
        }
    }
}
