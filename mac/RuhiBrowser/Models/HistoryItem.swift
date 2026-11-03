import Foundation

struct HistoryItem: Identifiable, Codable, Equatable {
    var id: String
    var title: String
    var url: String
    var visitedAt: Date

    init(id: String = UUID().uuidString, title: String, url: String, visitedAt: Date = Date()) {
        self.id = id
        self.title = title
        self.url = url
        self.visitedAt = visitedAt
    }
}
