import AppKit

class AppDelegate: NSObject, NSApplicationDelegate {
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Configure native AppKit appearance if needed
        NSWindow.allowsAutomaticWindowTabbing = false
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
