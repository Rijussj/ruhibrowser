import Foundation

struct BookmarkItem: Identifiable, Codable, Equatable {
    var id: String
    var title: String
    var url: String
    var createdAt: Date
    var isDeleted: Bool

    init(id: String = UUID().uuidString, title: String, url: String, createdAt: Date = Date(), isDeleted: Bool = false) {
        self.id = id
        self.title = title
        self.url = url
        self.createdAt = createdAt
        self.isDeleted = isDeleted
    }
}
