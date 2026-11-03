import Foundation
import Combine

struct SavedCredentialItem: Identifiable, Codable {
    var id: String
    var domain: String
    var username: String
    var createdAt: Date

    init(id: String = UUID().uuidString, domain: String, username: String, createdAt: Date = Date()) {
        self.id = id
        self.domain = domain
        self.username = username
        self.createdAt = createdAt
    }
}

class PasswordVaultManager: ObservableObject {
    static let shared = PasswordVaultManager()

    @Published var credentials: [SavedCredentialItem] = []

    private var storageFile: URL {
        let appSupport = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        let dir = appSupport.appendingPathComponent("RuhiBrowser", isDirectory: true)
        try? FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true, attributes: nil)
        return dir.appendingPathComponent("vault_meta.json")
    }

    private init() {
        loadCredentials()
        if credentials.isEmpty {
            // Default sample demo entries
            saveCredential(domain: "google.com", username: "user@example.com", password: "Password123!")
            saveCredential(domain: "github.com", username: "ruhidev321", password: "GitHub@Secret2026")
        }
    }

    func loadCredentials() {
        guard let data = try? Data(contentsOf: storageFile),
              let decoded = try? JSONDecoder().decode([SavedCredentialItem].self, from: data) else {
            return
        }
        self.credentials = decoded
    }

    private func persist() {
        if let data = try? JSONEncoder().encode(credentials) {
            try? data.write(to: storageFile)
        }
    }

    func saveCredential(domain: String, username: String, password: String) {
        let cleanDomain = domain.lowercased().trimmingCharacters(in: .whitespacesAndNewlines)
        let cleanUser = username.trimmingCharacters(in: .whitespacesAndNewlines)

        guard !cleanDomain.isEmpty, !cleanUser.isEmpty else { return }

        // Remove if existing
        deleteCredential(domain: cleanDomain, username: cleanUser)

        let item = SavedCredentialItem(domain: cleanDomain, username: cleanUser)
        credentials.append(item)
        persist()

        // Securely store password in Apple Keychain
        let keychainKey = "vault_\(item.id)"
        KeychainHelper.shared.save(key: keychainKey, value: password)
    }

    func retrievePassword(for item: SavedCredentialItem) -> String {
        let keychainKey = "vault_\(item.id)"
        return KeychainHelper.shared.read(key: keychainKey) ?? "••••••••"
    }

    func deleteCredential(domain: String, username: String) {
        if let idx = credentials.firstIndex(where: { $0.domain == domain && $0.username == username }) {
            let item = credentials[idx]
            KeychainHelper.shared.delete(key: "vault_\(item.id)")
            credentials.remove(at: idx)
            persist()
        }
    }

    func deleteCredential(id: String) {
        if let idx = credentials.firstIndex(where: { $0.id == id }) {
            KeychainHelper.shared.delete(key: "vault_\(id)")
            credentials.remove(at: idx)
            persist()
        }
    }
}
