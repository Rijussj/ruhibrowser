import SwiftUI

struct AddressBarView: View {
    @ObservedObject var tab: BrowserTab
    @ObservedObject var syncManager = SyncManager.shared
    @ObservedObject var bookmarkManager = BookmarkManager.shared

    @State private var inputUrl: String = ""
    @FocusState private var isAddressBarFocused: Bool

    var onOpenAccount: () -> Void
    var onOpenBookmarks: () -> Void
    var onOpenHistory: () -> Void
    var onOpenSettings: () -> Void
    var onSendTab: () -> Void
    var onOpenAi: () -> Void
    var onOpenTranslate: () -> Void
    var onOpenVpn: () -> Void
    var onOpenPasswords: () -> Void
    var onOpenPdfTools: () -> Void
    var onOpenClearData: () -> Void

    var body: some View {
        VStack(spacing: 0) {
            HStack(spacing: 8) {
                // Back Button
                Button(action: { tab.webView.goBack() }) {
                    Image(systemName: "chevron.left")
                        .font(.system(size: 13, weight: .semibold))
                }
                .disabled(!tab.canGoBack)
                .buttonStyle(ToolbarIconButtonStyle())
                .help("Back (⌘[)")

                // Forward Button
                Button(action: { tab.webView.goForward() }) {
                    Image(systemName: "chevron.right")
                        .font(.system(size: 13, weight: .semibold))
                }
                .disabled(!tab.canGoForward)
                .buttonStyle(ToolbarIconButtonStyle())
                .help("Forward (⌘])")

                // Reload / Stop Button
                Button(action: {
                    if tab.isLoading {
                        tab.webView.stopLoading()
                    } else {
                        tab.webView.reload()
                    }
                }) {
                    Image(systemName: tab.isLoading ? "xmark" : "arrow.clockwise")
                        .font(.system(size: 13, weight: .semibold))
                }
                .buttonStyle(ToolbarIconButtonStyle())
                .help("Reload (⌘R)")

                // Unified Smart Address & Search Field
                HStack(spacing: 6) {
                    // Security lock or shield icon
                    Image(systemName: tab.urlString.hasPrefix("https://") ? "lock.fill" : "globe")
                        .font(.system(size: 11))
                        .foregroundColor(tab.urlString.hasPrefix("https://") ? .green : .secondary)
                        .padding(.leading, 6)

                    TextField("Search or enter web address", text: $inputUrl, onCommit: {
                        navigate(to: inputUrl)
                    })
                    .textFieldStyle(PlainTextFieldStyle())
                    .font(.system(size: 13))
                    .focused($isAddressBarFocused)
                    .onChange(of: tab.urlString) { newUrl in
                        if !isAddressBarFocused {
                            inputUrl = newUrl
                        }
                    }

                    // Bookmark Star
                    let bookmarked = bookmarkManager.isBookmarked(url: tab.urlString)
                    Button(action: {
                        if bookmarked {
                            bookmarkManager.removeBookmark(url: tab.urlString)
                        } else {
                            bookmarkManager.addBookmark(title: tab.title, url: tab.urlString)
                        }
                    }) {
                        Image(systemName: bookmarked ? "star.fill" : "star")
                            .font(.system(size: 12))
                            .foregroundColor(bookmarked ? .yellow : .secondary)
                    }
                    .buttonStyle(PlainButtonStyle())
                    .help(bookmarked ? "Remove Bookmark" : "Bookmark this tab (⌘D)")
                    .padding(.trailing, 6)
                }
                .frame(height: 30)
                .background(Color(NSColor.controlBackgroundColor))
                .cornerRadius(8)
                .overlay(
                    RoundedRectangle(cornerRadius: 8)
                        .stroke(isAddressBarFocused ? Color.accentColor : Color.secondary.opacity(0.2), lineWidth: 1)
                )

                // AI Assistant Button (✨)
                Button(action: onOpenAi) {
                    Image(systemName: "sparkles")
                        .font(.system(size: 13, weight: .semibold))
                        .foregroundColor(.purple)
                }
                .buttonStyle(ToolbarIconButtonStyle())
                .help("RUHI AI Assistant")

                // Page Translator Button (🌐)
                Button(action: onOpenTranslate) {
                    Image(systemName: "globe.asia.australia.fill")
                        .font(.system(size: 13, weight: .semibold))
                        .foregroundColor(.blue)
                }
                .buttonStyle(ToolbarIconButtonStyle())
                .help("Translate Webpage")

                // Send to Phone / Devices (📱)
                Button(action: onSendTab) {
                    Image(systemName: "iphone.and.arrow.forward")
                        .font(.system(size: 13, weight: .semibold))
                        .foregroundColor(syncManager.isLoggedIn ? .accentColor : .secondary)
                }
                .buttonStyle(ToolbarIconButtonStyle())
                .help("Send Tab to Phone / PC (Ruhi Sync)")

                // Firefox-Style Ruhi Account Profile Avatar
                Button(action: onOpenAccount) {
                    ZStack(alignment: .bottomTrailing) {
                        Circle()
                            .fill(LinearGradient(colors: [Color.blue, Color.purple], startPoint: .topLeading, endPoint: .bottomTrailing))
                            .frame(width: 26, height: 26)
                        
                        Text(avatarInitial)
                            .font(.system(size: 11, weight: .bold))
                            .foregroundColor(.white)
                            .frame(width: 26, height: 26)

                        if syncManager.isLoggedIn {
                            Circle()
                                .fill(Color.green)
                                .frame(width: 8, height: 8)
                                .overlay(Circle().stroke(Color.white, lineWidth: 1))
                                .offset(x: 2, y: 2)
                        }
                    }
                }
                .buttonStyle(PlainButtonStyle())
                .help(syncManager.isLoggedIn ? "Ruhi Account (\(syncManager.currentDisplayName.isEmpty ? syncManager.currentEmail : syncManager.currentDisplayName))" : "Sign in to Ruhi Account")

                // Menu (3-dots)
                Menu {
                    Button(action: onOpenAi) {
                        Label("AI Assistant", systemImage: "sparkles")
                    }
                    Button(action: onOpenTranslate) {
                        Label("Translate Webpage...", systemImage: "globe.asia.australia.fill")
                    }
                    Button(action: onOpenVpn) {
                        Label("RUHI VPN & WireGuard...", systemImage: "shield.lefthalf.filled.badge.checkmark")
                    }
                    Button(action: onOpenPasswords) {
                        Label("Password Vault...", systemImage: "key.fill")
                    }
                    Button(action: onOpenPdfTools) {
                        Label("PDF Tools...", systemImage: "doc.richtext.fill")
                    }
                    Divider()
                    Button("Bookmarks (⌥⌘B)", action: onOpenBookmarks)
                    Button("History (⌘Y)", action: onOpenHistory)
                    Button("Clear Browsing Data... (⇧⌘⌫)", action: onOpenClearData)
                    Divider()
                    Button("RUHI Account", action: onOpenAccount)
                    Button("Preferences / Settings... (⌘,)", action: onOpenSettings)
                } label: {
                    Image(systemName: "ellipsis")
                        .font(.system(size: 14, weight: .bold))
                        .rotationEffect(.degrees(90))
                        .frame(width: 28, height: 28)
                }
                .menuStyle(BorderlessButtonMenuStyle())
                .frame(width: 28, height: 28)
            }
            .padding(.horizontal, 10)
            .padding(.vertical, 4)

            // Progress bar
            if tab.isLoading && tab.estimatedProgress < 1.0 {
                ProgressView(value: tab.estimatedProgress)
                    .progressViewStyle(LinearProgressViewStyle(tint: .accentColor))
                    .frame(height: 2)
            } else {
                Divider()
            }
        }
        .background(Color(NSColor.windowBackgroundColor))
        .onAppear {
            inputUrl = tab.urlString
        }
    }

    private var avatarInitial: String {
        if syncManager.isLoggedIn {
            let name = syncManager.currentDisplayName.trimmingCharacters(in: .whitespacesAndNewlines)
            if let first = name.first { return String(first).uppercased() }
            let email = syncManager.currentEmail.trimmingCharacters(in: .whitespacesAndNewlines)
            if let first = email.first { return String(first).uppercased() }
        }
        return "R"
    }

    private func navigate(to text: String) {
        var query = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !query.isEmpty else { return }

        if !query.contains("://") && !query.hasPrefix("about:") {
            if query.contains(".") && !query.contains(" ") {
                query = "https://" + query
            } else {
                let template = SettingsManager.shared.searchEngine.searchUrlTemplate
                let encoded = query.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? query
                query = String(format: template, encoded)
            }
        }

        if let targetUrl = URL(string: query) {
            tab.webView.load(URLRequest(url: targetUrl))
        }
    }
}

struct ToolbarIconButtonStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .frame(width: 28, height: 28)
            .background(configuration.isPressed ? Color.secondary.opacity(0.2) : Color.clear)
            .clipShape(RoundedRectangle(cornerRadius: 6))
            .foregroundColor(.primary)
    }
}
