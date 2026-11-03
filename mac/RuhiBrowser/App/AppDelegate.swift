import AppKit

class AppDelegate: NSObject, NSApplicationDelegate {
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Configure native AppKit appearance if needed
        NSWindow.allowsAutomaticWindowTabbing = false

        // Automated UI Tour mode for testing & screenshots of all 11 screens
        if CommandLine.arguments.contains("--tour") {
            // Second 1: Setup demo tabs, tab groups & sample history
            DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiDemoGroups"), object: nil)
            }
            // Screen 01: Main Browser & Tab Groups (captured at ~3s)

            // Screen 02: AI Assistant
            DispatchQueue.main.asyncAfter(deadline: .now() + 4.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowAi"), object: nil)
            }
            // (captured at ~7s)

            // Screen 03: Page Translator
            DispatchQueue.main.asyncAfter(deadline: .now() + 8.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 9.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowTranslate"), object: nil)
            }
            // (captured at ~11s)

            // Screen 04: RUHI VPN & WireGuard
            DispatchQueue.main.asyncAfter(deadline: .now() + 12.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 13.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowVpn"), object: nil)
            }
            // (captured at ~15s)

            // Screen 05: Password Vault
            DispatchQueue.main.asyncAfter(deadline: .now() + 16.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 17.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowPasswords"), object: nil)
            }
            // (captured at ~19s)

            // Screen 06: PDF Tools & Viewer
            DispatchQueue.main.asyncAfter(deadline: .now() + 20.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 21.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowPdfTools"), object: nil)
            }
            // (captured at ~23s)

            // Screen 07: RUHI Account & Cloud Sync
            DispatchQueue.main.asyncAfter(deadline: .now() + 24.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 25.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowAccount"), object: nil)
            }
            // (captured at ~27s)

            // Screen 08: Bookmarks Manager
            DispatchQueue.main.asyncAfter(deadline: .now() + 28.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 29.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowBookmarks"), object: nil)
            }
            // (captured at ~31s)

            // Screen 09: Browsing History
            DispatchQueue.main.asyncAfter(deadline: .now() + 32.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 33.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowHistory"), object: nil)
            }
            // (captured at ~35s)

            // Screen 10: Clear Browsing Data Dialog
            DispatchQueue.main.asyncAfter(deadline: .now() + 36.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 37.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowClearData"), object: nil)
            }
            // (captured at ~39s)

            // Screen 11: Browser Settings
            DispatchQueue.main.asyncAfter(deadline: .now() + 40.5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiCloseAllSheets"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 41.0) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowSettings"), object: nil)
            }
            // (captured at ~43s)
        }
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        return false // Standard macOS browser behavior (stays open in dock)
    }

    func application(_ application: NSApplication, open urls: [URL]) {
        for url in urls {
            // Forward external URL to sync/tab manager
            NotificationCenter.default.post(name: NSNotification.Name("RuhiOpenExternalUrl"), object: url)
        }
    }
}
