import SwiftUI

@main
struct RuhiBrowserApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate
    @ObservedObject var settings = SettingsManager.shared

    var body: some Scene {
        WindowGroup {
            MainWindowView()
                .preferredColorScheme(colorScheme)
        }
        .windowStyle(HiddenTitleBarWindowStyle())
        .commands {
            // File Menu
            CommandGroup(replacing: .newItem) {
                Button("New Tab") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiNewTab"), object: nil)
                }
                .keyboardShortcut("t", modifiers: .command)

                Button("New Private Tab") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiNewPrivateTab"), object: nil)
                }
                .keyboardShortcut("t", modifiers: [.command, .shift])

                Divider()

                Button("Close Tab") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseTab"), object: nil)
                }
                .keyboardShortcut("w", modifiers: .command)
            }

            // View Menu
            CommandGroup(replacing: .toolbar) {
                Button("Reload Page") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiReloadPage"), object: nil)
                }
                .keyboardShortcut("r", modifiers: .command)
            }

            // Bookmarks Menu
            CommandMenu("Bookmarks") {
                Button("Bookmark Current Tab") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiBookmarkTab"), object: nil)
                }
                .keyboardShortcut("d", modifiers: .command)

                Button("Show All Bookmarks...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowBookmarks"), object: nil)
                }
                .keyboardShortcut("b", modifiers: [.command, .option])
            }

            // App Settings (Native macOS Cmd + ,)
            CommandGroup(replacing: .appSettings) {
                Button("Settings...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowSettings"), object: nil)
                }
            }

            // Account & Sync Menu
            CommandMenu("Cloud Sync") {
                Button("Supabase Cloud Account...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowAccount"), object: nil)
                }
            }

            // History Menu
            CommandMenu("History") {
                Button("Show All History...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowHistory"), object: nil)
                }
                .keyboardShortcut("y", modifiers: .command)

                Divider()

                Button("Clear Browsing Data...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowClearData"), object: nil)
                }
                .keyboardShortcut(.delete, modifiers: [.command, .shift])
            }

            // Tools Menu
            CommandMenu("Tools") {
                Button("AI Assistant...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowAi"), object: nil)
                }
                .keyboardShortcut("a", modifiers: [.command, .shift])

                Button("Translate Webpage...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowTranslate"), object: nil)
                }
                .keyboardShortcut("l", modifiers: [.command, .shift])

                Button("RUHI VPN & WireGuard...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowVpn"), object: nil)
                }
                .keyboardShortcut("v", modifiers: [.command, .shift])

                Button("Password Vault...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowPasswords"), object: nil)
                }
                .keyboardShortcut("p", modifiers: [.command, .shift])

                Button("PDF Tools...") {
                    NotificationCenter.default.post(name: NSNotification.Name("RuhiShowPdfTools"), object: nil)
                }
                .keyboardShortcut("d", modifiers: [.command, .shift])
            }
        }
    }

    private var colorScheme: ColorScheme? {
        switch settings.theme {
        case .system: return nil
        case .light: return .light
        case .dark: return .dark
        }
    }
}
