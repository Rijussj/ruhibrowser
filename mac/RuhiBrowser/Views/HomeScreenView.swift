import SwiftUI

struct QuickShortcutItem: Identifiable, Codable {
    var id: UUID = UUID()
    var title: String
    var url: String
    var iconName: String
    var colorHex: String
}

struct HomeScreenView: View {
    @ObservedObject var tab: BrowserTab
    @ObservedObject var settings = SettingsManager.shared
    @ObservedObject var historyManager = HistoryManager.shared
    @ObservedObject var syncManager = SyncManager.shared

    @State private var searchInput: String = ""
    @State private var currentTimeString: String = ""
    @State private var currentDateString: String = ""
    @State private var greeting: String = "Welcome to RUHI Browser"
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
        QuickShortcutItem(title: "YouTube", url: "https://www.youtube.com", iconName: "play.rectangle.fill", colorHex: "#FF0000"),
        QuickShortcutItem(title: "GitHub", url: "https://github.com", iconName: "chevron.left.forwardslash.chevron.right", colorHex: "#24292E"),
        QuickShortcutItem(title: "Wikipedia", url: "https://www.wikipedia.org", iconName: "book.closed.fill", colorHex: "#636466"),
        QuickShortcutItem(title: "Reddit", url: "https://www.reddit.com", iconName: "bubble.left.and.bubble.right.fill", colorHex: "#FF4500"),
        QuickShortcutItem(title: "X / Twitter", url: "https://x.com", iconName: "at", colorHex: "#1DA1F2"),
        QuickShortcutItem(title: "ChatGPT", url: "https://chatgpt.com", iconName: "cpu", colorHex: "#10A37F"),
        QuickShortcutItem(title: "WhatsApp", url: "https://web.whatsapp.com", iconName: "message.fill", colorHex: "#25D366")
    ]

    var body: some View {
        ScrollView(.vertical, showsIndicators: true) {
            VStack(spacing: 24) {
                // Private / Incognito Notice (if active)
                if tab.isIncognito {
                    incognitoBanner
                        .padding(.top, 16)
                }

                // Header with Digital Clock & Greeting
                VStack(spacing: 6) {
                    Text(currentTimeString)
                        .font(.system(size: 48, weight: .bold, design: .rounded))
                        .foregroundColor(.primary)

                    Text(currentDateString)
                        .font(.subheadline)
                        .foregroundColor(.secondary)

                    HStack(spacing: 6) {
                        Image(systemName: "shield.checkered")
                            .foregroundColor(.blue)
                        Text(greeting)
                            .font(.headline)
                            .foregroundColor(.primary)
                    }
                    .padding(.top, 4)
                }
                .padding(.top, tab.isIncognito ? 8 : 28)

                // Search Bar
                HStack(spacing: 12) {
                    Image(systemName: "magnifyingglass")
                        .foregroundColor(.secondary)
                        .font(.system(size: 16))

                    TextField("Search the web or type a URL...", text: $searchInput, onCommit: {
                        performSearch()
                    })
                    .textFieldStyle(PlainTextFieldStyle())
                    .font(.system(size: 14))

                    Button(action: performSearch) {
                        Image(systemName: "arrow.right.circle.fill")
                            .foregroundColor(.accentColor)
                            .font(.system(size: 20))
                    }
                    .buttonStyle(PlainButtonStyle())
                }
                .padding(.horizontal, 16)
                .padding(.vertical, 12)
                .frame(maxWidth: 620)
                .background(Color(NSColor.controlBackgroundColor))
                .cornerRadius(24)
                .shadow(color: Color.black.opacity(0.08), radius: 6, x: 0, y: 2)

                // Supercharged Feature Launchpad (Har ek Feature)
                VStack(alignment: .leading, spacing: 12) {
                    HStack {
                        Image(systemName: "square.grid.2x2.fill")
                            .foregroundColor(.accentColor)
                        Text("RUHI Browser Feature Hub")
                            .font(.headline)
                            .fontWeight(.semibold)
                        Spacer()
                    }

                    LazyVGrid(columns: [GridItem(.adaptive(minimum: 155), spacing: 12)], spacing: 12) {
                        FeatureCard(title: "AI Assistant", subtitle: "Gemini Chat & Summary", iconName: "sparkles", iconColor: .purple, action: onOpenAi)
                        FeatureCard(title: "RUHI VPN", subtitle: "WireGuard Fast & Secure", iconName: "shield.lefthalf.filled.badge.checkmark", iconColor: .blue, action: onOpenVpn)
                        FeatureCard(title: "Password Vault", subtitle: "Credential Locker", iconName: "key.fill", iconColor: .orange, action: onOpenPasswords)
                        FeatureCard(title: "Page Translator", subtitle: "Instant Translation", iconName: "globe.asia.australia.fill", iconColor: .green, action: onOpenTranslate)
                        FeatureCard(title: "PDF Tools", subtitle: "Viewer & Annotator", iconName: "doc.richtext.fill", iconColor: .red, action: onOpenPdfTools)
                        FeatureCard(title: "RUHI Account", subtitle: syncManager.isLoggedIn ? "Cloud Sync Active" : "Sign In to Sync", iconName: "person.crop.circle.badge.checkmark", iconColor: .indigo, action: onOpenAccount)
                        FeatureCard(title: "Bookmarks", subtitle: "Quick Access Sites", iconName: "star.fill", iconColor: .yellow, action: onOpenBookmarks)
                        FeatureCard(title: "History", subtitle: "Browsing Timeline", iconName: "clock.arrow.circlepath", iconColor: .teal, action: onOpenHistory)
                    }
                }
                .frame(maxWidth: 720)
                .padding(.horizontal, 20)

                // Quick Shortcuts / Speed Dial
                VStack(alignment: .leading, spacing: 12) {
                    HStack {
                        Image(systemName: "link.circle.fill")
                            .foregroundColor(.secondary)
                        Text("Top Sites & Shortcuts")
                            .font(.headline)
                            .fontWeight(.semibold)
                        Spacer()
                    }

                    LazyVGrid(columns: [GridItem(.adaptive(minimum: 80), spacing: 16)], spacing: 16) {
                        ForEach(defaultShortcuts) { shortcut in
                            Button(action: {
                                if let targetUrl = URL(string: shortcut.url) {
                                    onNavigate(targetUrl)
                                }
                            }) {
                                VStack(spacing: 8) {
                                    ZStack {
                                        RoundedRectangle(cornerRadius: 14)
                                            .fill(Color(hex: shortcut.colorHex).opacity(0.15))
                                            .frame(width: 54, height: 54)
                                        Image(systemName: shortcut.iconName)
                                            .font(.system(size: 22))
                                            .foregroundColor(Color(hex: shortcut.colorHex))
                                    }
                                    Text(shortcut.title)
                                        .font(.caption)
                                        .foregroundColor(.primary)
                                        .lineLimit(1)
                                }
                                .frame(width: 72)
                            }
                            .buttonStyle(PlainButtonStyle())
                        }
                    }
                }
                .frame(maxWidth: 720)
                .padding(.horizontal, 20)

                // Recent Visits
                if !historyManager.history.isEmpty && !tab.isIncognito {
                    VStack(alignment: .leading, spacing: 10) {
                        HStack {
                            Image(systemName: "clock")
                                .foregroundColor(.secondary)
                            Text("Recent Visits")
                                .font(.headline)
                                .fontWeight(.semibold)
                            Spacer()
                            Button("See All History", action: onOpenHistory)
                                .font(.caption)
                                .foregroundColor(.accentColor)
                                .buttonStyle(PlainButtonStyle())
                        }

                        VStack(spacing: 6) {
                            ForEach(historyManager.history.prefix(4)) { item in
                                Button(action: {
                                    if let url = URL(string: item.url) {
                                        onNavigate(url)
                                    }
                                }) {
                                    HStack(spacing: 12) {
                                        Image(systemName: "globe")
                                            .foregroundColor(.secondary)
                                            .frame(width: 18)
                                        VStack(alignment: .leading, spacing: 2) {
                                            Text(item.title)
                                                .font(.system(size: 13, weight: .medium))
                                                .foregroundColor(.primary)
                                                .lineLimit(1)
                                            Text(item.url)
                                                .font(.system(size: 11))
                                                .foregroundColor(.secondary)
                                                .lineLimit(1)
                                        }
                                        Spacer()
                                        Image(systemName: "chevron.right")
                                            .font(.system(size: 10))
                                            .foregroundColor(.secondary.opacity(0.5))
                                    }
                                    .padding(.vertical, 8)
                                    .padding(.horizontal, 12)
                                    .background(Color(NSColor.controlBackgroundColor).opacity(0.6))
                                    .cornerRadius(8)
                                }
                                .buttonStyle(PlainButtonStyle())
                            }
                        }
                    }
                    .frame(maxWidth: 720)
                    .padding(.horizontal, 20)
                    .padding(.bottom, 24)
                }
            }
            .frame(maxWidth: .infinity)
        }
        .background(Color(NSColor.windowBackgroundColor))
        .onAppear {
            updateClockAndGreeting()
        }
        .onReceive(timer) { _ in
            updateClockAndGreeting()
        }
    }

    private var incognitoBanner: some View {
        HStack(spacing: 12) {
            Image(systemName: "eyeglasses")
                .font(.system(size: 24))
                .foregroundColor(.primary)

            VStack(alignment: .leading, spacing: 2) {
                Text("You've gone Incognito")
                    .font(.headline)
                Text("RUHI Browser won't save browsing history, cookies, or site data for this tab.")
                    .font(.caption)
                    .foregroundColor(.secondary)
            }
            Spacer()
        }
        .padding()
        .frame(maxWidth: 720)
        .background(Color(NSColor.controlBackgroundColor))
        .cornerRadius(12)
        .overlay(RoundedRectangle(cornerRadius: 12).stroke(Color.secondary.opacity(0.3), lineWidth: 1))
    }

    private func updateClockAndGreeting() {
        let formatter = DateFormatter()
        formatter.timeStyle = .short
        currentTimeString = formatter.string(from: Date())

        formatter.timeStyle = .none
        formatter.dateStyle = .full
        currentDateString = formatter.string(from: Date())

        let hour = Calendar.current.component(.hour, from: Date())
        if hour < 12 {
            greeting = "Good morning, Welcome to RUHI Browser"
        } else if hour < 17 {
            greeting = "Good afternoon, Welcome to RUHI Browser"
        } else {
            greeting = "Good evening, Welcome to RUHI Browser"
        }
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
            let engineUrl = settings.searchEngine.searchUrl(query: encoded)
            targetUrl = URL(string: engineUrl) ?? URL(string: "https://www.google.com/search?q=\(encoded)")!
        }
        onNavigate(targetUrl)
    }
}

struct FeatureCard: View {
    let title: String
    let subtitle: String
    let iconName: String
    let iconColor: Color
    let action: () -> Void

    @State private var isHovered = false

    var body: some View {
        Button(action: action) {
            VStack(alignment: .leading, spacing: 6) {
                HStack {
                    ZStack {
                        Circle()
                            .fill(iconColor.opacity(0.15))
                            .frame(width: 32, height: 32)
                        Image(systemName: iconName)
                            .font(.system(size: 15, weight: .semibold))
                            .foregroundColor(iconColor)
                    }
                    Spacer()
                    Image(systemName: "arrow.up.right")
                        .font(.system(size: 10))
                        .foregroundColor(.secondary.opacity(0.6))
                }

                Text(title)
                    .font(.system(size: 13, weight: .semibold))
                    .foregroundColor(.primary)

                Text(subtitle)
                    .font(.system(size: 10))
                    .foregroundColor(.secondary)
                    .lineLimit(1)
            }
            .padding(10)
            .background(Color(NSColor.controlBackgroundColor))
            .cornerRadius(12)
            .overlay(
                RoundedRectangle(cornerRadius: 12)
                    .stroke(isHovered ? iconColor.opacity(0.6) : Color.secondary.opacity(0.15), lineWidth: 1)
            )
            .shadow(color: Color.black.opacity(isHovered ? 0.08 : 0.02), radius: 4, x: 0, y: 1)
        }
        .buttonStyle(PlainButtonStyle())
        .onHover { isHovered = $0 }
    }
}
