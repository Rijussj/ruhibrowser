import Foundation
import SwiftUI
import Combine

class NewsFeedManager: NSObject, ObservableObject, XMLParserDelegate {
    static let shared = NewsFeedManager()

    @Published var articles: [NewsArticleItem] = []
    @Published var isLoading: Bool = false
    @Published var lastFetched: Date? = nil

    private let feedUrl = URL(string: "https://taazawire.in/feed/")!

    // XML parsing state
    private var currentElement: String = ""
    private var currentTitle: String = ""
    private var currentLink: String = ""
    private var currentDescription: String = ""
    private var currentPubDate: String = ""
    private var currentCategory: String = ""
    private var currentSource: String = ""
    private var parsedItems: [NewsArticleItem] = []

    override private init() {
        super.init()
        // Load default curated items initially so UI is instant
        self.articles = defaultFallbackArticles
    }

    func fetchTrendingNews(force: Bool = false) async {
        if !force, let last = lastFetched, Date().timeIntervalSince(last) < 900, !articles.isEmpty {
            return // Use 15m memory cache
        }

        await MainActor.run { self.isLoading = true }

        do {
            var request = URLRequest(url: feedUrl, cachePolicy: .reloadIgnoringLocalCacheData, timeoutInterval: 10)
            request.setValue("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.5 Safari/605.1.15 RuhiBrowser/1.0", forHTTPHeaderField: "User-Agent")

            let (data, response) = try await URLSession.shared.data(for: request)
            guard let httpResponse = response as? HTTPURLResponse, httpResponse.statusCode == 200 else {
                await MainActor.run { self.isLoading = false }
                return
            }

            let parser = XMLParser(data: data)
            parser.delegate = self
            parsedItems.removeAll()

            if parser.parse(), !parsedItems.isEmpty {
                let items = parsedItems
                await MainActor.run {
                    self.articles = items
                    self.lastFetched = Date()
                    self.isLoading = false
                }
            } else {
                await MainActor.run { self.isLoading = false }
            }
        } catch {
            await MainActor.run { self.isLoading = false }
        }
    }

    // MARK: - XMLParserDelegate

    func parser(_ parser: XMLParser, didStartElement elementName: String, namespaceURI: String?, qualifiedName qName: String?, attributes attributeDict: [String : String] = [:]) {
        currentElement = elementName
        if elementName == "item" {
            currentTitle = ""
            currentLink = ""
            currentDescription = ""
            currentPubDate = ""
            currentCategory = ""
            currentSource = ""
        }
    }

    func parser(_ parser: XMLParser, foundCharacters string: String) {
        switch currentElement {
        case "title":
            currentTitle += string
        case "link":
            currentLink += string
        case "description":
            currentDescription += string
        case "pubDate":
            currentPubDate += string
        case "category":
            if currentCategory.isEmpty {
                currentCategory += string
            }
        case "source":
            currentSource += string
        default:
            break
        }
    }

    func parser(_ parser: XMLParser, didEndElement elementName: String, namespaceURI: String?, qualifiedName qName: String?) {
        if elementName == "item" {
            var title = currentTitle.trimmingCharacters(in: .whitespacesAndNewlines)
            if title.hasSuffix(" - TaazaWire") {
                title = String(title.dropLast(" - TaazaWire".count)).trimmingCharacters(in: .whitespacesAndNewlines)
            }

            var desc = currentDescription.replacingOccurrences(of: "<[^>]+>", with: "", options: .regularExpression)
            desc = desc.trimmingCharacters(in: .whitespacesAndNewlines)
            if desc.count > 160 {
                desc = String(desc.prefix(157)) + "..."
            }

            let cat = currentCategory.trimmingCharacters(in: .whitespacesAndNewlines).lowercased()
            let timeAgo = parseRelativeTime(from: currentPubDate.trimmingCharacters(in: .whitespacesAndNewlines))
            let theme = categoryTheme(for: cat)

            let article = NewsArticleItem(
                title: title.isEmpty ? "Trending Story" : title,
                description: desc,
                source: currentSource.isEmpty ? "TaazaWire" : currentSource,
                timeAgo: timeAgo,
                category: resolveCategoryKey(cat),
                url: currentLink.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty ? "https://taazawire.in/" : currentLink.trimmingCharacters(in: .whitespacesAndNewlines),
                bannerColors: theme.colors,
                emoji: theme.emoji
            )
            parsedItems.append(article)
        }
    }

    private func resolveCategoryKey(_ cat: String) -> String {
        if cat.contains("tech") || cat.contains("phone") || cat.contains("ai") { return "tech" }
        if cat.contains("finance") || cat.contains("business") || cat.contains("yojna") { return "business" }
        if cat.contains("sport") || cat.contains("cricket") { return "sports" }
        if cat.contains("entertain") || cat.contains("cinema") || cat.contains("film") { return "entertainment" }
        if cat.contains("world") || cat.contains("break") { return "world" }
        return "top"
    }

    private func categoryTheme(for cat: String) -> (colors: [Color], emoji: String) {
        if cat.contains("tech") {
            return ([Color(hex: "#0F172A"), Color(hex: "#0284C7")], "💻")
        } else if cat.contains("finance") || cat.contains("business") || cat.contains("yojna") {
            return ([Color(hex: "#0B132B"), Color(hex: "#059669")], "💼")
        } else if cat.contains("sport") || cat.contains("cricket") {
            return ([Color(hex: "#1C1917"), Color(hex: "#D97706")], "🏏")
        } else if cat.contains("entertain") || cat.contains("cinema") {
            return ([Color(hex: "#1E1B4B"), Color(hex: "#C026D3")], "🎬")
        } else if cat.contains("world") || cat.contains("break") {
            return ([Color(hex: "#0F172A"), Color(hex: "#DC2626")], "⚡")
        }
        return ([Color(hex: "#0B132B"), Color(hex: "#2563EB")], "🔥")
    }

    private func parseRelativeTime(from pubDateStr: String) -> String {
        let formatter = DateFormatter()
        formatter.locale = Locale(identifier: "en_US_POSIX")
        formatter.dateFormat = "EEE, dd MMM yyyy HH:mm:ss zzz"

        guard let date = formatter.date(from: pubDateStr) else {
            return "Just now"
        }

        let seconds = Date().timeIntervalSince(date)
        if seconds < 3600 {
            let mins = max(1, Int(seconds / 60))
            return "\(mins)m ago"
        } else if seconds < 86400 {
            let hrs = Int(seconds / 3600)
            return "\(hrs)h ago"
        } else {
            let days = Int(seconds / 86400)
            return "\(days)d ago"
        }
    }

    var defaultFallbackArticles: [NewsArticleItem] {
        [
            NewsArticleItem(
                title: "Next-Gen AI Systems Transform Web Browsing & Autonomous Workflows",
                description: "New multimodal models bring on-device reasoning and privacy-first web navigation across platforms.",
                source: "TaazaWire",
                timeAgo: "15m ago",
                category: "tech",
                url: "https://taazawire.in/",
                bannerColors: [Color(hex: "#0F172A"), Color(hex: "#0284C7")],
                emoji: "💻"
            ),
            NewsArticleItem(
                title: "Global Financial Markets Gain Momentum Amid Innovation Boom",
                description: "Positive earnings and semiconductor manufacturing expansion drive technology sector rally.",
                source: "TaazaWire",
                timeAgo: "28m ago",
                category: "business",
                url: "https://taazawire.in/",
                bannerColors: [Color(hex: "#0B132B"), Color(hex: "#059669")],
                emoji: "💼"
            ),
            NewsArticleItem(
                title: "Deep Space Telescope Mission Relays First High-Resolution Telemetry",
                description: "Astronomers discover unprecedented exoplanet atmosphere compositions in nearby star cluster.",
                source: "TaazaWire",
                timeAgo: "42m ago",
                category: "world",
                url: "https://taazawire.in/",
                bannerColors: [Color(hex: "#0F172A"), Color(hex: "#DC2626")],
                emoji: "⚡"
            ),
            NewsArticleItem(
                title: "International Championship Announces Venues & Tournament Schedule",
                description: "World ranking teams finalize squads for upcoming international sporting competition.",
                source: "TaazaWire",
                timeAgo: "1h ago",
                category: "sports",
                url: "https://taazawire.in/",
                bannerColors: [Color(hex: "#1C1917"), Color(hex: "#D97706")],
                emoji: "🏏"
            ),
            NewsArticleItem(
                title: "Post-Quantum Security Standard Rolled Out to Global Web Networks",
                description: "Leading security consortium enforces zero-trust encryption protocols across web infrastructure.",
                source: "TaazaWire",
                timeAgo: "2h ago",
                category: "tech",
                url: "https://taazawire.in/",
                bannerColors: [Color(hex: "#0F172A"), Color(hex: "#2563EB")],
                emoji: "🛡️"
            ),
            NewsArticleItem(
                title: "Blockbuster Cinematic Release Sets Global Streaming Records",
                description: "Groundbreaking visual effects and original score achieve critical acclaim and record viewership.",
                source: "TaazaWire",
                timeAgo: "3h ago",
                category: "entertainment",
                url: "https://taazawire.in/",
                bannerColors: [Color(hex: "#1E1B4B"), Color(hex: "#C026D3")],
                emoji: "🎬"
            )
        ]
    }
}
