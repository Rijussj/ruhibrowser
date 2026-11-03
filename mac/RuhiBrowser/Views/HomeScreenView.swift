import SwiftUI
import AppKit

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
    @State private var clockTime: String = "12:00"
    @State private var clockSeconds: String = ":00"
    @State private var clockAmPm: String = "PM"
    @State private var greetingText: String = "Good afternoon, Welcome to RUHI  •  📅 Tuesday, 3 Nov"
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

    var body: some View {
        ScrollView(.vertical, showsIndicators: true) {
            ZStack(alignment: .top) {
                // Subtle Windows-style scenic ambient gradient background
                LinearGradient(
                    colors: [
                        Color(hex: "#0B132B"),
                        Color(hex: "#1C2541"),
                        Color(hex: "#181818"),
                        Color(hex: "#181818")
                    ],
                    startPoint: .top,
                    endPoint: .bottom
                )
                .ignoresSafeArea()

                VStack(spacing: 20) {
                    // Private / Incognito Banner (if active)
                    if tab.isIncognito {
                        incognitoBanner
                            .padding(.top, 16)
                    }

                    // Productive Header: Digital Clock, Live Seconds & Greeting Pill
                    VStack(spacing: 6) {
                        HStack(alignment: .bottom, spacing: 8) {
                            Text(clockTime)
                                .font(.system(size: 60, weight: .bold, design: .rounded))
                                .foregroundColor(.white)

                            VStack(alignment: .leading, spacing: 3) {
                                Text(clockSeconds)
                                    .font(.system(size: 13, weight: .bold, design: .monospaced))
                                    .foregroundColor(Color(hex: "#38BDF8"))
                                    .padding(.horizontal, 6)
                                    .padding(.vertical, 2)
                                    .background(Color(hex: "#383838"))
                                    .cornerRadius(6)

                                Text(clockAmPm)
                                    .font(.system(size: 12.5, weight: .bold))
                                    .foregroundColor(Color(hex: "#94A3B8"))
                            }
                            .padding(.bottom, 10)
                        }

                        // Greeting & Date Pill (Identical to Windows)
                        HStack(spacing: 6) {
                            Text(greetingText)
                                .font(.system(size: 12.5, weight: .semibold))
                                .foregroundColor(Color(hex: "#E2E8F0"))
                        }
                        .padding(.horizontal, 14)
                        .padding(.vertical, 6)
                        .background(Color(hex: "#141E33"))
                        .cornerRadius(16)
                        .overlay(
                            RoundedRectangle(cornerRadius: 16)
                                .stroke(Color(hex: "#25354F"), lineWidth: 1)
                        )

                        // Motivational Focus Quote
                        Text("“Focus on being productive instead of busy.”")
                            .font(.system(size: 12))
                            .italic()
                            .foregroundColor(Color(hex: "#64748B"))
                            .padding(.top, 2)
                    }
                    .padding(.top, tab.isIncognito ? 8 : 22)

                    // Brand Header: App Icon + Title + Subtitle
                    VStack(spacing: 4) {
                        appIconView
                            .frame(width: 64, height: 64)
                            .padding(.bottom, 4)

                        Text("RUHI Browser")
                            .font(.system(size: 30, weight: .bold))
                            .foregroundColor(.white)

                        Text("Fast, Secure & Private Browsing")
                            .font(.system(size: 13))
                            .foregroundColor(Color(hex: "#94A3B8"))
                    }

                    // Windows-Style Big Central Search Bar
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

                    // Feature Hub & Quick Launch Workspace (From Windows BorderHomeProductiveTools)
                    VStack(alignment: .leading, spacing: 10) {
                        Text("⚡ Quick Launch Feature Hub")
                            .font(.system(size: 12, weight: .semibold))
                            .foregroundColor(Color(hex: "#94A3B8"))

                        LazyVGrid(columns: Array(repeating: GridItem(.flexible(), spacing: 8), count: 4), spacing: 8) {
                            featurePill(title: "🤖 AI Assistant", action: onOpenAi)
                            featurePill(title: "🛡️ RUHI VPN", action: onOpenVpn)
                            featurePill(title: "🔑 Password Vault", action: onOpenPasswords)
                            featurePill(title: "🌐 Page Translator", action: onOpenTranslate)
                            featurePill(title: "📑 PDF Tools", action: onOpenPdfTools)
                            featurePill(title: "☁️ RUHI Account", action: onOpenAccount)
                            featurePill(title: "🔖 Bookmarks", action: onOpenBookmarks)
                            featurePill(title: "🕒 History", action: onOpenHistory)
                        }
                    }
                    .padding(14)
                    .frame(width: 680)
                    .background(Color(hex: "#242424"))
                    .cornerRadius(14)
                    .overlay(
                        RoundedRectangle(cornerRadius: 14)
                            .stroke(Color(hex: "#383838"), lineWidth: 1)
                    )

                    // Shortcuts Section Header & Tiles (Identical to Windows WpShortcuts)
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

                        // 8 Shortcuts in Windows Grid Layout
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

                    // Recent Visits Section (Identical to Windows BorderRecentVisits)
                    if !historyManager.history.isEmpty && !tab.isIncognito {
                        VStack(alignment: .leading, spacing: 10) {
                            HStack {
                                Text("Recent Visits")
                                    .font(.system(size: 14, weight: .semibold))
                                    .foregroundColor(Color(hex: "#94A3B8"))
                                Spacer()
                                Button("See All History", action: onOpenHistory)
                                    .font(.system(size: 11))
                                    .foregroundColor(Color(hex: "#38BDF8"))
                                    .buttonStyle(PlainButtonStyle())
                            }

                            VStack(spacing: 6) {
                                ForEach(historyManager.history.prefix(3)) { item in
                                    Button(action: {
                                        if let url = URL(string: item.url) {
                                            onNavigate(url)
                                        }
                                    }) {
                                        HStack(spacing: 12) {
                                            Image(systemName: "globe")
                                                .foregroundColor(Color(hex: "#64748B"))
                                                .font(.system(size: 14))

                                            VStack(alignment: .leading, spacing: 2) {
                                                Text(item.title)
                                                    .font(.system(size: 13, weight: .medium))
                                                    .foregroundColor(.white)
                                                    .lineLimit(1)
                                                Text(item.url)
                                                    .font(.system(size: 11))
                                                    .foregroundColor(Color(hex: "#94A3B8"))
                                                    .lineLimit(1)
                                            }
                                            Spacer()
                                            Image(systemName: "chevron.right")
                                                .font(.system(size: 10))
                                                .foregroundColor(Color(hex: "#64748B"))
                                        }
                                        .padding(.vertical, 8)
                                        .padding(.horizontal, 12)
                                        .background(Color(hex: "#1C1C1C"))
                                        .cornerRadius(8)
                                    }
                                    .buttonStyle(PlainButtonStyle())
                                }
                            }
                        }
                        .padding(16)
                        .frame(width: 680)
                        .background(Color(hex: "#242424"))
                        .cornerRadius(12)
                        .overlay(
                            RoundedRectangle(cornerRadius: 12)
                                .stroke(Color(hex: "#383838"), lineWidth: 1)
                        )
                        .padding(.bottom, 24)
                    }
                }
                .frame(maxWidth: .infinity)
                .padding(.bottom, 30)
            }
        }
        .background(Color(hex: "#181818"))
        .onAppear {
            updateClockAndGreeting()
        }
        .onReceive(timer) { _ in
            updateClockAndGreeting()
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
                // High-fidelity vector fallback of the RUHI icon
                ZStack {
                    RoundedRectangle(cornerRadius: 16)
                        .fill(LinearGradient(colors: [Color(hex: "#3B82F6"), Color(hex: "#1D4ED8")], startPoint: .topLeading, endPoint: .bottomTrailing))
                    Image(systemName: "globe.americas.fill")
                        .font(.system(size: 34))
                        .foregroundColor(.white)
                }
            }
        }
    }

    private func featurePill(title: String, action: @escaping () -> Void) -> some View {
        Button(action: action) {
            Text(title)
                .font(.system(size: 11.5, weight: .medium))
                .foregroundColor(Color(hex: "#E2E8F0"))
                .padding(.horizontal, 8)
                .padding(.vertical, 6)
                .frame(maxWidth: .infinity)
                .background(Color(hex: "#1C1C1C"))
                .cornerRadius(8)
                .overlay(
                    RoundedRectangle(cornerRadius: 8)
                        .stroke(Color(hex: "#383838"), lineWidth: 1)
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

    private func updateClockAndGreeting() {
        let formatter = DateFormatter()
        formatter.dateFormat = "h:mm"
        clockTime = formatter.string(from: Date())

        formatter.dateFormat = ":ss"
        clockSeconds = formatter.string(from: Date())

        formatter.dateFormat = "a"
        clockAmPm = formatter.string(from: Date())

        let dateFmt = DateFormatter()
        dateFmt.dateFormat = "EEEE, d MMM"
        let datePart = dateFmt.string(from: Date())

        let hour = Calendar.current.component(.hour, from: Date())
        let greet: String
        if hour < 12 {
            greet = "Good morning, Welcome to RUHI"
        } else if hour < 17 {
            greet = "Good afternoon, Welcome to RUHI"
        } else {
            greet = "Good evening, Welcome to RUHI"
        }
        greetingText = "\(greet)  •  📅 \(datePart)"
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
