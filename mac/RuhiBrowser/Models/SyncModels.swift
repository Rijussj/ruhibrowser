import Foundation

struct SyncedDevice: Identifiable, Codable, Equatable {
    var id: String { deviceId }
    let deviceId: String
    let deviceName: String
    let deviceType: String
    let lastActive: String
    let activeTabs: [String]

    enum CodingKeys: String, CodingKey {
        case deviceId = "device_id"
        case deviceName = "device_name"
        case deviceType = "device_type"
        case lastActive = "last_active"
        case activeTabs = "active_tabs"
    }
}

struct SentTab: Identifiable, Codable, Equatable {
    let id: String
    let senderDeviceId: String
    let targetDeviceId: String?
    let url: String
    let title: String
    let sentAt: String
    let isOpened: Bool

    enum CodingKeys: String, CodingKey {
        case id
        case senderDeviceId = "sender_device_id"
        case targetDeviceId = "target_device_id"
        case url
        case title
        case sentAt = "sent_at"
        case isOpened = "is_opened"
    }
}

struct SyncedPassword: Identifiable, Codable, Equatable {
    let id: String
    let websiteUrl: String
    let websiteTitle: String
    let username: String
    let encryptedPassword: String

    enum CodingKeys: String, CodingKey {
        case id
        case websiteUrl = "website_url"
        case websiteTitle = "website_title"
        case username
        case encryptedPassword = "encrypted_password"
    }
}
