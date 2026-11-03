import SwiftUI
import Combine

struct MainWindowView: View {
    @StateObject private var syncManager = SyncManager.shared
    @StateObject private var settings = SettingsManager.shared

    @State private var tabs: [BrowserTab] = []
    @State private var tabGroups: [TabGroup] = []
    @State private var selectedTabId: UUID = UUID()

    @State private var showAccountSheet = false
    @State private var showBookmarksSheet = false
    @State private var showHistorySheet = false
    @State private var showSettingsSheet = false
    @State private var showClearDataSheet = false

    // New Supercharged Feature Sheets
    @State private var showAiSheet = false
    @State private var showTranslateSheet = false
    @State private var showVpnSheet = false
    @State private var showPasswordsSheet = false
    @State private var showPdfToolsSheet = false
    @State private var editingGroup: TabGroup = TabGroup()
    @State private var showRenameGroupSheet = false

    var selectedTab: BrowserTab? {
        tabs.first(where: { $0.id == selectedTabId }) ?? tabs.first
    }

    var body: some View {
        VStack(spacing: 0) {
            // Tabs Bar with Tab Groups
            TabStripView(
                tabs: $tabs,
                tabGroups: $tabGroups,
                selectedTabId: $selectedTabId,
                onNewTab: { addNewTab() },
                onCloseTab: { id in closeTab(id: id) },
                onAddTabToGroup: { tabId, groupId in
                    if let idx = tabs.firstIndex(where: { $0.id == tabId }) {
                        if let gid = groupId {
                            if !tabGroups.contains(where: { $0.id == gid }) {
                                let newG = TabGroup(
                                    id: gid,
                                    title: "Group \(tabGroups.count + 1)",
                                    colorHex: TabGroup.presetColors[tabGroups.count % TabGroup.presetColors.count].hex
                                )
                                tabGroups.append(newG)
                            }
                            tabs[idx].groupId = gid
                        } else {
                            tabs[idx].groupId = nil
                        }
                    }
                },
                onEditGroup: { group in
                    editingGroup = group
                    showRenameGroupSheet = true
                }
            )

            // Address & Toolbar
            if let activeTab = selectedTab {
                AddressBarView(
                    tab: activeTab,
                    onOpenAccount: { showAccountSheet = true },
                    onOpenBookmarks: { showBookmarksSheet = true },
                    onOpenHistory: { showHistorySheet = true },
                    onOpenSettings: { showSettingsSheet = true },
                    onSendTab: {
                        Task {
                            _ = await syncManager.sendTabToDevice(targetDeviceId: nil, url: activeTab.urlString, title: activeTab.title)
                            showAccountSheet = true
                        }
                    },
                    onOpenAi: { showAiSheet = true },
                    onOpenTranslate: { showTranslateSheet = true },
                    onOpenVpn: { showVpnSheet = true },
                    onOpenPasswords: { showPasswordsSheet = true },
                    onOpenPdfTools: { showPdfToolsSheet = true },
                    onOpenClearData: { showClearDataSheet = true }
                )

                // Active Web View
                ZStack {
                    ForEach(tabs) { tab in
                        WebViewContainer(tab: tab)
                            .opacity(tab.id == selectedTabId ? 1.0 : 0.0)
                            .allowsHitTesting(tab.id == selectedTabId)
                    }
                }
            } else {
                VStack {
                    Spacer()
                    Text("No Tabs Open")
                        .foregroundColor(.secondary)
                    Button("Open New Tab") { addNewTab() }
                        .padding(.top, 4)
                    Spacer()
                }
            }
        }
        .frame(minWidth: 800, minHeight: 600)
        // Dialogs & Sheets
        .sheet(isPresented: $showAccountSheet) {
            AccountSheetView(
                currentTabTitle: selectedTab?.title ?? "",
                currentTabUrl: selectedTab?.urlString ?? ""
            )
        }
        .sheet(isPresented: $showBookmarksSheet) {
            BookmarksWindowView { url in
                selectedTab?.webView.load(URLRequest(url: URL(string: url)!))
            }
        }
        .sheet(isPresented: $showHistorySheet) {
            HistoryWindowView { url in
                selectedTab?.webView.load(URLRequest(url: URL(string: url)!))
            }
        }
        .sheet(isPresented: $showSettingsSheet) {
            SettingsSheetView()
        }
        .sheet(isPresented: $showClearDataSheet) {
            ClearDataSheetView()
        }
        .sheet(isPresented: $showAiSheet) {
            AiAssistantSheetView(
                pageTitle: selectedTab?.title ?? "",
                pageUrl: selectedTab?.urlString ?? ""
            )
        }
        .sheet(isPresented: $showTranslateSheet) {
            TranslateSheetView(
                activeUrlString: selectedTab?.urlString ?? "",
                onTranslate: { targetUrl in
                    selectedTab?.webView.load(URLRequest(url: targetUrl))
                }
            )
        }
        .sheet(isPresented: $showVpnSheet) {
            VpnSheetView()
        }
        .sheet(isPresented: $showPasswordsSheet) {
            PasswordsSheetView()
        }
        .sheet(isPresented: $showPdfToolsSheet) {
            PdfToolsSheetView(initialPdfUrl: selectedTab?.urlString)
        }
        .sheet(isPresented: $showRenameGroupSheet) {
            RenameGroupDialogView(group: $editingGroup) { updated in
                if let idx = tabGroups.firstIndex(where: { $0.id == updated.id }) {
                    tabGroups[idx] = updated
                }
            }
        }
        // Notification Listeners
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowClearData"))) { _ in
            closeAllSheets()
            showClearDataSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowAccount"))) { _ in
            closeAllSheets()
            showAccountSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowSettings"))) { _ in
            closeAllSheets()
            showSettingsSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowBookmarks"))) { _ in
            closeAllSheets()
            showBookmarksSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowHistory"))) { _ in
            closeAllSheets()
            showHistorySheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowAi"))) { _ in
            closeAllSheets()
            showAiSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowTranslate"))) { _ in
            closeAllSheets()
            showTranslateSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowVpn"))) { _ in
            closeAllSheets()
            showVpnSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowPasswords"))) { _ in
            closeAllSheets()
            showPasswordsSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiShowPdfTools"))) { _ in
            closeAllSheets()
            showPdfToolsSheet = true
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiCloseAllSheets"))) { _ in
            closeAllSheets()
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiDemoGroups"))) { _ in
            let gWork = TabGroup(title: "Work", colorHex: "#3B82F6")
            let gMedia = TabGroup(title: "Media", colorHex: "#EF4444")
            tabGroups = [gWork, gMedia]
            if let first = tabs.first { first.groupId = gWork.id }
            let secondTab = BrowserTab(
                title: "Google Gemini",
                initialUrl: URL(string: "https://gemini.google.com"),
                groupId: gMedia.id
            )
            tabs.append(secondTab)

            // Populate sample history for tour if empty
            if HistoryManager.shared.history.isEmpty {
                HistoryManager.shared.recordVisit(title: "Google Search", url: "https://www.google.com")
                HistoryManager.shared.recordVisit(title: "GitHub - RUHI Browser Repository", url: "https://github.com/ruhidev321/ruhi-browser")
                HistoryManager.shared.recordVisit(title: "Wikipedia: The Free Encyclopedia", url: "https://www.wikipedia.org")
            }
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiNewTab"))) { _ in
            addNewTab()
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiNewTabWithUrl"))) { notif in
            if let targetUrl = notif.object as? URL {
                addNewTab(url: targetUrl)
            }
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiNewPrivateTab"))) { _ in
            addNewTab(isIncognito: true)
        }
        .onReceive(NotificationCenter.default.publisher(for: NSNotification.Name("RuhiCloseTab"))) { _ in
            closeTab(id: selectedTabId)
        }
        .onAppear {
            if tabs.isEmpty {
                let initialUrl = URL(string: settings.homepageUrl)
                let firstTab = BrowserTab(title: "Home", initialUrl: initialUrl)
                tabs.append(firstTab)
                selectedTabId = firstTab.id
            }

            // Handle incoming tab from phone / PC
            syncManager.onTabReceived = { title, urlString in
                if let targetUrl = URL(string: urlString) {
                    let newTab = BrowserTab(title: title.isEmpty ? "Linked Tab" : title, initialUrl: targetUrl)
                    tabs.append(newTab)
                    selectedTabId = newTab.id
                    NSSound.beep()
                }
            }
        }
    }

    private func closeAllSheets() {
        showAccountSheet = false
        showBookmarksSheet = false
        showHistorySheet = false
        showSettingsSheet = false
        showClearDataSheet = false
        showAiSheet = false
        showTranslateSheet = false
        showVpnSheet = false
        showPasswordsSheet = false
        showPdfToolsSheet = false
        showRenameGroupSheet = false
    }

    private func addNewTab(url: URL? = nil, isIncognito: Bool = false) {
        let targetUrl = url ?? URL(string: settings.homepageUrl)
        let tab = BrowserTab(
            title: isIncognito ? "Private Tab" : "New Tab",
            initialUrl: targetUrl,
            isIncognito: isIncognito
        )
        tabs.append(tab)
        selectedTabId = tab.id
    }

    private func closeTab(id: UUID) {
        guard let index = tabs.firstIndex(where: { $0.id == id }) else { return }
        tabs.remove(at: index)

        if tabs.isEmpty {
            addNewTab()
        } else if selectedTabId == id {
            let nextIndex = max(0, index - 1)
            selectedTabId = tabs[nextIndex].id
        }
    }
}
