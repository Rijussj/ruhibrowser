import AppKit

class AppDelegate: NSObject, NSApplicationDelegate {
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Configure native AppKit appearance if needed
        NSWindow.allowsAutomaticWindowTabbing = false

        // Automated UI Tour mode for testing & screenshots
        if CommandLine.arguments.contains("--tour") {
            DispatchQueue.main.asyncAfter(deadline: .now() + 3) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowSettings"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 7) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowBookmarks"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 11) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowAccount"), object: nil)
            }
            DispatchQueue.main.asyncAfter(deadline: .now() + 15) {
                NotificationCenter.default.post(name: NSNotification.Name("RuhiShowClearData"), object: nil)
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
