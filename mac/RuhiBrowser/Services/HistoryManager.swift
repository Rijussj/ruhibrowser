import Foundation
import Combine

class HistoryManager: ObservableObject {
    static let shared = HistoryManager()

    @Published var history: [HistoryItem] = []

    private var storageFile: URL {
        let appSupport = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        let dir = appSupport.appendingPathComponent("RuhiBrowser", isDirectory: true)
        try? FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true, attributes: nil)
        return dir.appendingPathComponent("history.json")
    }

    private init() {
        loadHistory()
    }

    func loadHistory() {
        do {
            let data = try Data(contentsOf: storageFile)
            let decoded = try JSONDecoder().decode([HistoryItem].self, from: data)
            self.history = decoded.sorted(by: { $0.visitedAt > $1.visitedAt })
        } catch {
            self.history = []
        }
    }

    func saveHistory() {
        do {
            let data = try JSONEncoder().encode(history)
            try data.write(to: storageFile, options: .atomic)
        } catch {
            print("Failed to save history: \(error)")
        }
    }

    func recordVisit(title: String, url: String) {
        let trimmedUrl = url.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmedUrl.isEmpty, !trimmedUrl.hasPrefix("about:"), !trimmedUrl.hasPrefix("ruhi:") else { return }

        let item = HistoryItem(title: title.isEmpty ? trimmedUrl : title, url: trimmedUrl, visitedAt: Date())
        history.insert(item, at: 0)

        // Keep maximum 1000 items locally
        if history.count > 1000 {
            history = Array(history.prefix(1000))
        }
        saveHistory()
    }

    func clearHistory() {
        history.removeAll()
        saveHistory()
    }

    func deleteHistoryItem(id: String) {
        history.removeAll { $0.id == id }
        saveHistory()
    }
}
