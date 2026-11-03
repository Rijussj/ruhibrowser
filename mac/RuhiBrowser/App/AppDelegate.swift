import AppKit

class AppDelegate: NSObject, NSApplicationDelegate {
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Configure native AppKit appearance if needed
        NSWindow.allowsAutomaticWindowTabbing = false

        // Automated UI Tour mode for testing & screenshots
        if CommandLine.arguments.contains("--tour") {
            DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowAi"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 5) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowTranslate"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 8) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowVpn"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 11) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowPasswords"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 14) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowPdfTools"), object: nil)
            }
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
