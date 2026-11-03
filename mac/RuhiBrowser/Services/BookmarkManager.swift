import Foundation
import Combine

class BookmarkManager: ObservableObject {
    static let shared = BookmarkManager()

    @Published var bookmarks: [BookmarkItem] = []

    private var storageFile: URL {
        let appSupport = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        let dir = appSupport.appendingPathComponent("RuhiBrowser", isDirectory: true)
        try? FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true, attributes: nil)
        return dir.appendingPathComponent("bookmarks.json")
    }

    private init() {
        loadBookmarks()
        if bookmarks.isEmpty {
            // Default sample bookmarks
            addBookmark(title: "Google", url: "https://www.google.com")
            addBookmark(title: "Wikipedia", url: "https://www.wikipedia.org")
            addBookmark(title: "GitHub", url: "https://github.com")
        }
    }

    func loadBookmarks() {
        do {
            let data = try Data(contentsOf: storageFile)
            let decoded = try JSONDecoder().decode([BookmarkItem].self, from: data)
            self.bookmarks = decoded.filter { !$0.isDeleted }
        } catch {
            self.bookmarks = []
        }
    }

    func saveBookmarks() {
        do {
            let data = try JSONEncoder().encode(bookmarks)
            try data.write(to: storageFile, options: .atomic)
        } catch {
            print("Failed to save bookmarks: \(error)")
        }
    }

    func addBookmark(title: String, url: String) {
        let trimmedUrl = url.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmedUrl.isEmpty else { return }
        
        if let existingIndex = bookmarks.firstIndex(where: { $0.url.caseInsensitiveCompare(trimmedUrl) == .orderedSame }) {
            bookmarks[existingIndex].title = title.isEmpty ? trimmedUrl : title
        } else {
            let item = BookmarkItem(title: title.isEmpty ? trimmedUrl : title, url: trimmedUrl)
            bookmarks.insert(item, at: 0)
        }
        saveBookmarks()
    }

    func isBookmarked(url: String) -> Bool {
        let trimmed = url.trimmingCharacters(in: .whitespacesAndNewlines)
        return bookmarks.contains { $0.url.caseInsensitiveCompare(trimmed) == .orderedSame }
    }

    func removeBookmark(url: String) {
        let trimmed = url.trimmingCharacters(in: .whitespacesAndNewlines)
        bookmarks.removeAll { $0.url.caseInsensitiveCompare(trimmed) == .orderedSame }
        saveBookmarks()
    }

    func removeBookmark(id: String) {
        bookmarks.removeAll { $0.id == id }
        saveBookmarks()
    }
}
