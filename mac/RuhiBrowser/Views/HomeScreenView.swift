import SwiftUI
import AppKit

struct NewsArticleItem: Identifiable {
    var id: UUID = UUID()
    var title: String
    var description: String
    var source: String
    var timeAgo: String
    var category: String
    var url: String
    var bannerColors: [Color]
    var emoji: String
}

struct HomeScreenView: View {
    @ObservedObject var tab: BrowserTab
    @ObservedObject var settings = SettingsManager.shared
    @ObservedObject var historyManager = HistoryManager.shared
    @ObservedObject var syncManager = SyncManager.shared

    @State private var searchInput: String = ""
    @State private var activeCategory: String = "top"
    @State private var dateGlanceString: String = "🕒 4:30 PM  •  📅 Tuesday, 3 Nov"
    @State private var timer = Timer.publish(every: 1.0, on: .main, in: .common).autoconnect()

    var onOpenAi: () -> Void
    var onOpenTranslate: () -> Void
    var onOpenVpn: () -> Void
    var onOpenPasswords: () -> Void
    var onOpenPdfTools: () -> Void
    var onOpenAccount: () -> Void
    var onOpenBookmarks: () -> Void
    var onOpenHistory: () -> Void
    var onNavigate: (URL) -> Void

    let defaultShortcuts: [QuickShortcutItem] = [
        QuickShortcutItem(title: "Google", url: "https://www.google.com", iconName: "magnifyingglass", colorHex: "#4285F4"),
        QuickShortcutItem(title: "YouTube", url: "https://www.youtube.com", iconName: "play.rectangle.fill", colorHex: "#EF4444"),
        QuickShortcutItem(title: "GitHub", url: "https://github.com", iconName: "chevron.left.forwardslash.chevron.right", colorHex: "#94A3B8"),
        QuickShortcutItem(title: "Wikipedia", url: "https://www.wikipedia.org", iconName: "book.closed.fill", colorHex: "#A855F7"),
        QuickShortcutItem(title: "Reddit", url: "https://www.reddit.com", iconName: "bubble.left.and.bubble.right.fill", colorHex: "#F97316"),
        QuickShortcutItem(title: "X / Twitter", url: "https://x.com", iconName: "at", colorHex: "#38BDF8"),
        QuickShortcutItem(title: "ChatGPT", url: "https://chatgpt.com", iconName: "cpu", colorHex: "#10B981"),
        QuickShortcutItem(title: "WhatsApp", url: "https://web.whatsapp.com", iconName: "message.fill", colorHex: "#22C55E")
    ]

    let sampleArticles: [NewsArticleItem] = [
        NewsArticleItem(
            title: "Next-Gen AI Systems Transform Web Browsing & Autonomous Workflows",
            description: "New multimodal models bring on-device reasoning and privacy-first web navigation across platforms.",
            source: "TechWire",
            timeAgo: "15m ago",
            category: "tech",
            url: "https://news.google.com",
            bannerColors: [Color(hex: "#0F172A"), Color(hex: "#0284C7")],
            emoji: "💻"
        ),
        NewsArticleItem(
            title: "Global Financial Markets Gain Momentum Amid Innovation Boom",
            description: "Positive earnings and semiconductor manufacturing expansion drive technology sector rally.",
            source: "Financial Times",
            timeAgo: "28m ago",
            category: "business",
            url: "https://news.google.com",
            bannerColors: [Color(hex: "#0B132B"), Color(hex: "#059669")],
            emoji: "💼"
        ),
        NewsArticleItem(
            title: "Deep Space Telescope Mission Relays First High-Resolution Telemetry",
            description: "Astronomers discover unprecedented exoplanet atmosphere compositions in nearby star cluster.",
            source: "Science Daily",
            timeAgo: "42m ago",
            category: "world",
            url: "https://news.google.com",
            bannerColors: [Color(hex: "#0F172A"), Color(hex: "#DC2626")],
            emoji: "⚡"
        ),
        NewsArticleItem(
            title: "International Championship Announces Venues & Tournament Schedule",
            description: "World ranking teams finalize squads for upcoming international sporting competition.",
            source: "Sports Central",
            timeAgo: "1h ago",
            category: "sports",
            url: "https://news.google.com",
            bannerColors: [Color(hex: "#1C1917"), Color(hex: "#D97706")],
            emoji: "🏏"
        ),
        NewsArticleItem(
            title: "Post-Quantum Security Standard Rolled Out to Global Web Networks",
            description: "Leading security consortium enforces zero-trust encryption protocols across web infrastructure.",
            source: "Cyber Defense",
            timeAgo: "2h ago",
            category: "tech",
            url: "https://news.google.com",
            bannerColors: [Color(hex: "#0F172A"), Color(hex: "#2563EB")],
            emoji: "🛡️"
        ),
        NewsArticleItem(
            title: "Blockbuster Cinematic Release Sets Global Streaming Records",
            description: "Groundbreaking visual effects and original score achieve critical acclaim and record viewership.",
            source: "Entertainment Weekly",
            timeAgo: "3h ago",
            category: "entertainment",
            url: "https://news.google.com",
            bannerColors: [Color(hex: "#1E1B4B"), Color(hex: "#C026D3")],
            emoji: "🎬"
        )
    ]

    var filteredArticles: [NewsArticleItem] {
        if activeCategory == "top" {
            return sampleArticles
        }
        return sampleArticles.filter { $0.category == activeCategory }
    }

    var body: some View {
        ScrollView(.vertical, showsIndicators: true) {
            VStack(spacing: 24) {
                // Private / Incognito Banner (if active)
                if tab.isIncognito {
                    incognitoBanner
                        .padding(.top, 16)
                }

                // Brand Header (Exact Windows SpHomeBrandHeader)
                VStack(spacing: 6) {
                    appIconView
                        .frame(width: 72, height: 72)
                        .padding(.bottom, 2)

                    Text("RUHI Browser")
                        .font(.system(size: 30, weight: .bold))
                        .foregroundColor(.white)

                    Text("Fast, Secure & Private Browsing")
                        .font(.system(size: 13))
                        .foregroundColor(Color(hex: "#94A3B8"))
                }
                .padding(.top, tab.isIncognito ? 8 : 28)

                // Big Central Search Bar (Exact Windows BorderHomeSearch)
                HStack(spacing: 12) {
                    Text("🔍")
                        .font(.system(size: 14))
                        .foregroundColor(Color(hex: "#64748B"))

                    TextField("Search Google or enter URL...", text: $searchInput, onCommit: {
                        performSearch()
                    })
                    .textFieldStyle(PlainTextFieldStyle())
                    .font(.system(size: 14))
                    .foregroundColor(.white)

                    Button(action: performSearch) {
                        Text("Search")
                            .font(.system(size: 13, weight: .semibold))
                            .foregroundColor(.white)
                            .padding(.horizontal, 16)
                            .padding(.vertical, 7)
                            .background(Color(hex: "#2563EB"))
                            .cornerRadius(16)
                    }
                    .buttonStyle(PlainButtonStyle())
                }
                .padding(.leading, 18)
                .padding(.trailing, 6)
                .padding(.vertical, 4)
                .frame(width: 680, height: 48)
                .background(Color(hex: "#202124"))
                .cornerRadius(24)
                .overlay(
                    RoundedRectangle(cornerRadius: 24)
                        .stroke(Color(hex: "#3C4043"), lineWidth: 1)
                )

                // Shortcuts Section (Exact Windows WpShortcuts)
                VStack(spacing: 10) {
                    HStack {
                        Text("Quick Shortcuts")
                            .font(.system(size: 13, weight: .semibold))
                            .foregroundColor(Color(hex: "#94A3B8"))
                        Spacer()
                        Text("(Right-click shortcut to edit/delete)")
                            .font(.system(size: 11))
                            .foregroundColor(Color(hex: "#64748B"))
                    }
                    .frame(width: 680)

                    LazyVGrid(columns: Array(repeating: GridItem(.fixed(110), spacing: 10), count: 4), spacing: 10) {
                        ForEach(defaultShortcuts) { item in
                            Button(action: {
                                if let url = URL(string: item.url) {
                                    onNavigate(url)
                                }
                            }) {
                                VStack(spacing: 8) {
                                    ZStack {
                                        Circle()
                                            .fill(Color(hex: item.colorHex).opacity(0.18))
                                            .frame(width: 36, height: 36)

                                        Image(systemName: item.iconName)
                                            .font(.system(size: 16, weight: .medium))
                                            .foregroundColor(Color(hex: item.colorHex))
                                    }

                                    Text(item.title)
                                        .font(.system(size: 12, weight: .medium))
                                        .foregroundColor(.white)
                                        .lineLimit(1)
                                }
                                .frame(width: 110, height: 86)
                                .background(Color(hex: "#242424"))
                                .cornerRadius(12)
                                .overlay(
                                    RoundedRectangle(cornerRadius: 12)
                                        .stroke(Color(hex: "#383838"), lineWidth: 1)
                                    )
                            }
                            .buttonStyle(PlainButtonStyle())
                        }
                    }
                    .frame(width: 680)
                }

                // Open Edge-Style MSN News Feed (Exact Windows BorderHomeNews)
                VStack(spacing: 16) {
                    // Category Navigation Pills + Live Date Glance
                    HStack {
                        // Category Pills
                        HStack(spacing: 6) {
                            categoryPill(title: "🔥 Top Stories", category: "top")
                            categoryPill(title: "💻 Tech", category: "tech")
                            categoryPill(title: "💼 Finance", category: "business")
                            categoryPill(title: "🏏 Sports", category: "sports")
                            categoryPill(title: "🎬 Entertainment", category: "entertainment")
                            categoryPill(title: "⚡ Breaking", category: "world")
                        }

                        Spacer()

                        // Right Glance: Date Briefing & Refresh
                        HStack(spacing: 8) {
                            HStack(spacing: 6) {
                                Circle()
                                    .fill(Color(hex: "#10B981"))
                                    .frame(width: 7, height: 7)
                                Text("LIVE")
                                    .font(.system(size: 10, weight: .bold))
                                    .foregroundColor(Color(hex: "#10B981"))
                                Text(dateGlanceString)
                                    .font(.system(size: 11.5, weight: .semibold))
                                    .foregroundColor(Color(hex: "#E2E8F0"))
                            }
                            .padding(.horizontal, 12)
                            .padding(.vertical, 6)
                            .background(Color(hex: "#141E33"))
                            .cornerRadius(18)
                            .overlay(
                                RoundedRectangle(cornerRadius: 18)
                                    .stroke(Color(hex: "#25354F"), lineWidth: 1)
                            )

                            Button(action: {
                                updateDateGlance()
                            }) {
                                HStack(spacing: 4) {
                                    Text("🔄")
                                        .font(.system(size: 10))
                                    Text("Refresh")
                                        .font(.system(size: 11, weight: .semibold))
                                }
                                .foregroundColor(Color(hex: "#94A3B8"))
                                .padding(.horizontal, 10)
                                .padding(.vertical, 6)
                                .background(Color(hex: "#1E293B"))
                                .cornerRadius(14)
                                .overlay(
                                    RoundedRectangle(cornerRadius: 14)
                                        .stroke(Color(hex: "#334155"), lineWidth: 1)
                                )
                            }
                            .buttonStyle(PlainButtonStyle())
                        }
                    }
                    .frame(width: 680)

                    // Hero Featured Article Card (Edge Hero Article)
                    if let hero = filteredArticles.first {
                        Button(action: {
                            if let url = URL(string: hero.url) {
                                onNavigate(url)
                            }
                        }) {
                            HStack(spacing: 16) {
                                // Left visual banner
                                ZStack {
                                    LinearGradient(colors: hero.bannerColors, startPoint: .topLeading, endPoint: .bottomTrailing)
                                    Text(hero.emoji)
                                        .font(.system(size: 40))
                                }
                                .frame(width: 220, height: 140)
                                .cornerRadius(10)

                                // Right story details
                                VStack(alignment: .leading, spacing: 6) {
                                    HStack {
                                        Text("FEATURED STORY")
                                            .font(.system(size: 10, weight: .bold))
                                            .foregroundColor(Color(hex: "#38BDF8"))
                                            .padding(.horizontal, 6)
                                            .padding(.vertical, 2)
                                            .background(Color(hex: "#0F2847"))
                                            .cornerRadius(4)

                                        Spacer()

                                        Text("\(hero.source) • \(hero.timeAgo)")
                                            .font(.system(size: 11))
                                            .foregroundColor(Color(hex: "#64748B"))
                                    }

                                    Text(hero.title)
                                        .font(.system(size: 15, weight: .bold))
                                        .foregroundColor(.white)
                                        .lineLimit(2)

                                    Text(hero.description)
                                        .font(.system(size: 12))
                                        .foregroundColor(Color(hex: "#94A3B8"))
                                        .lineLimit(2)

                                    HStack(spacing: 4) {
                                        Text("Read Full Coverage")
                                            .font(.system(size: 11.5, weight: .semibold))
                                            .foregroundColor(Color(hex: "#38BDF8"))
                                        Image(systemName: "arrow.up.right")
                                            .font(.system(size: 9))
                                            .foregroundColor(Color(hex: "#38BDF8"))
                                    }
                                    .padding(.top, 2)
                                }
                                .padding(.trailing, 14)
                            }
                            .padding(10)
                            .frame(width: 680, height: 160)
                            .background(Color(hex: "#162032"))
                            .cornerRadius(14)
                            .overlay(
                                RoundedRectangle(cornerRadius: 14)
                                    .stroke(Color(hex: "#24334C"), lineWidth: 1)
                            )
                        }
                        .buttonStyle(PlainButtonStyle())
                    }

                    // 3-Column Articles Uniform Grid
                    LazyVGrid(columns: Array(repeating: GridItem(.fixed(218), spacing: 13), count: 3), spacing: 13) {
                        ForEach(filteredArticles.dropFirst()) { art in
                            Button(action: {
                                if let url = URL(string: art.url) {
                                    onNavigate(url)
                                }
                            }) {
                                VStack(alignment: .leading, spacing: 8) {
                                    // Card Banner
                                    ZStack {
                                        LinearGradient(colors: art.bannerColors, startPoint: .topLeading, endPoint: .bottomTrailing)
                                        Text(art.emoji)
                                            .font(.system(size: 24))
                                    }
                                    .frame(height: 70)
                                    .cornerRadius(8)

                                    Text(art.title)
                                        .font(.system(size: 12.5, weight: .bold))
                                        .foregroundColor(.white)
                                        .lineLimit(2)

                                    Text(art.description)
                                        .font(.system(size: 11))
                                        .foregroundColor(Color(hex: "#94A3B8"))
                                        .lineLimit(2)

                                    Spacer(minLength: 0)

                                    HStack {
                                        Text(art.source)
                                            .font(.system(size: 10, weight: .semibold))
                                            .foregroundColor(Color(hex: "#64748B"))
                                        Spacer()
                                        Text(art.timeAgo)
                                            .font(.system(size: 10))
                                            .foregroundColor(Color(hex: "#64748B"))
                                    }
                                }
                                .padding(10)
                                .frame(width: 218, height: 195)
                                .background(Color(hex: "#242424"))
                                .cornerRadius(12)
                                .overlay(
                                    RoundedRectangle(cornerRadius: 12)
                                        .stroke(Color(hex: "#383838"), lineWidth: 1)
                                )
                            }
                            .buttonStyle(PlainButtonStyle())
                        }
                    }
                    .frame(width: 680)
                }
                .padding(.bottom, 36)
            }
            .frame(maxWidth: .infinity)
        }
        .background(Color(hex: "#181818"))
        .onAppear {
            updateDateGlance()
        }
        .onReceive(timer) { _ in
            updateDateGlance()
        }
    }

    private var appIconView: some View {
        Group {
            if let imagePath = Bundle.main.path(forResource: "app_icon", ofType: "png"),
               let nsImage = NSImage(contentsOfFile: imagePath) {
                Image(nsImage: nsImage)
                    .resizable()
                    .aspectRatio(contentMode: .fit)
                    .cornerRadius(16)
            } else {
                ZStack {
                    RoundedRectangle(cornerRadius: 16)
                        .fill(LinearGradient(colors: [Color(hex: "#3B82F6"), Color(hex: "#1D4ED8")], startPoint: .topLeading, endPoint: .bottomTrailing))
                    Image(systemName: "globe.americas.fill")
                        .font(.system(size: 38))
                        .foregroundColor(.white)
                }
            }
        }
    }

    private func categoryPill(title: String, category: String) -> some View {
        let isSel = (activeCategory == category)
        return Button(action: {
            activeCategory = category
        }) {
            Text(title)
                .font(.system(size: 11, weight: isSel ? .bold : .semibold))
                .foregroundColor(isSel ? .white : Color(hex: "#94A3B8"))
                .padding(.horizontal, 9)
                .padding(.vertical, 5)
                .background(
                    isSel ?
                    AnyView(LinearGradient(colors: [Color(hex: "#1D4ED8"), Color(hex: "#2563EB")], startPoint: .leading, endPoint: .trailing)) :
                    AnyView(Color(hex: "#141E33"))
                )
                .cornerRadius(12)
                .overlay(
                    RoundedRectangle(cornerRadius: 12)
                        .stroke(isSel ? Color(hex: "#60A5FA") : Color(hex: "#25354F"), lineWidth: 1)
                )
        }
        .buttonStyle(PlainButtonStyle())
    }

    private var incognitoBanner: some View {
        HStack(spacing: 12) {
            Image(systemName: "eyeglasses")
                .font(.system(size: 24))
                .foregroundColor(.white)

            VStack(alignment: .leading, spacing: 2) {
                Text("You've gone Incognito")
                    .font(.headline)
                    .foregroundColor(.white)
                Text("RUHI Browser won't save browsing history, cookies, or site data for this tab.")
                    .font(.caption)
                    .foregroundColor(Color(hex: "#94A3B8"))
            }
            Spacer()
        }
        .padding()
        .frame(width: 680)
        .background(Color(hex: "#242424"))
        .cornerRadius(12)
        .overlay(RoundedRectangle(cornerRadius: 12).stroke(Color(hex: "#383838"), lineWidth: 1))
    }

    private func updateDateGlance() {
        let formatter = DateFormatter()
        formatter.dateFormat = "h:mm a"
        let timePart = formatter.string(from: Date())

        formatter.dateFormat = "EEEE, d MMM"
        let datePart = formatter.string(from: Date())

        dateGlanceString = "🕒 \(timePart)  •  📅 \(datePart)"
    }

    private func performSearch() {
        let query = searchInput.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !query.isEmpty else { return }

        let targetUrl: URL
        if query.hasPrefix("http://") || query.hasPrefix("https://") {
            targetUrl = URL(string: query) ?? URL(string: "https://www.google.com/search?q=\(query)")!
        } else if query.contains(".") && !query.contains(" ") {
            targetUrl = URL(string: "https://\(query)") ?? URL(string: "https://www.google.com/search?q=\(query)")!
        } else {
            let encoded = query.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? query
            let template = settings.searchEngine.searchUrlTemplate
            let engineUrl = String(format: template, encoded)
            targetUrl = URL(string: engineUrl) ?? URL(string: "https://www.google.com/search?q=\(encoded)")!
        }
        onNavigate(targetUrl)
    }
}
