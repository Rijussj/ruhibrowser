import Foundation
import SwiftUI

struct TabGroup: Identifiable, Codable, Equatable {
    let id: UUID
    var title: String
    var colorHex: String
    var isCollapsed: Bool

    init(
        id: UUID = UUID(),
        title: String = "Group",
        colorHex: String = "#3B82F6",
        isCollapsed: Bool = false
    ) {
        self.id = id
        self.title = title
        self.colorHex = colorHex
        self.isCollapsed = isCollapsed
    }

    var color: Color {
        Color(hex: colorHex)
    }

    static let presetColors: [(name: String, hex: String)] = [
        ("Blue", "#3B82F6"),
        ("Red", "#EF4444"),
        ("Yellow", "#EAB308"),
        ("Green", "#10B981"),
        ("Pink", "#EC4899"),
        ("Purple", "#8B5CF6"),
        ("Cyan", "#06B6D4"),
        ("Orange", "#F97316"),
        ("Gray", "#64748B")
    ]
}

extension Color {
    init(hex: String) {
        let hex = hex.trimmingCharacters(in: CharacterSet.alphanumerics.inverted)
        var int: UInt64 = 0
        Scanner(string: hex).scanHexInt64(&int)
        let a, r, g, b: UInt64
        switch hex.count {
        case 3: // RGB (12-bit)
            (a, r, g, b) = (255, (int >> 8) * 17, (int >> 4 & 0xF) * 17, (int & 0xF) * 17)
        case 6: // RGB (24-bit)
            (a, r, g, b) = (255, int >> 16, int >> 8 & 0xFF, int & 0xFF)
        case 8: // ARGB (32-bit)
            (a, r, g, b) = (int >> 24, int >> 16 & 0xFF, int >> 8 & 0xFF, int & 0xFF)
        default:
            (a, r, g, b) = (255, 59, 130, 246)
        }
        self.init(
            .sRGB,
            red: Double(r) / 255,
            green: Double(g) / 255,
            blue: Double(b) / 255,
            opacity: Double(a) / 255
        )
    }
}
