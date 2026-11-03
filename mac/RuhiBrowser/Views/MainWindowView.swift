import SwiftUI

struct MainWindowView: View {
    @StateObject private var syncManager = SyncManager.shared
    @StateObject private var settings = SettingsManager.shared

    @State private var tabs: [BrowserTab] = []
    @State private var selectedTabId: UUID = UUID()

    @State private var showAccountSheet = false
    @State private var showBookmarksSheet = false
    @State private var showHistorySheet = false
    @State private var showSettingsSheet = false

    var selectedTab: BrowserTab? {
        tabs.first(where: { $0.id == selectedTabId }) ?? tabs.first
    }

    var body: some View {
        VStack(spacing: 0) {
            // Tabs Bar
            TabStripView(
                tabs: $tabs,
                selectedTabId: $selectedTabId,
                onNewTab: { addNewTab() },
                onCloseTab: { id in closeTab(id: id) }
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
                    }
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
