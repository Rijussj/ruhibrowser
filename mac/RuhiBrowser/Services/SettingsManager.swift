import Foundation
import Combine

class SettingsManager: ObservableObject {
    static let shared = SettingsManager()

    private let defaults = UserDefaults.standard

    private let keySearchEngine = "pref_search_engine"
    private let keyTheme = "pref_theme"
    private let keyHomepage = "pref_homepage"
    private let keyAdBlockEnabled = "pref_adblock_enabled"

    // Sync Preferences
    private let keySyncEnabled = "pref_sync_enabled"
    private let keySyncEmail = "pref_sync_email"
    private let keySyncDisplayName = "pref_sync_display_name"
    private let keySyncUserId = "pref_sync_user_id"
    private let keySyncAccessToken = "pref_sync_access_token"
    private let keySyncRefreshToken = "pref_sync_refresh_token"
    private let keySyncDeviceId = "pref_sync_device_id"
    private let keySyncDeviceName = "pref_sync_device_name"

    @Published var searchEngine: SearchEngine {
        didSet { defaults.set(searchEngine.rawValue, forKey: keySearchEngine) }
    }

    @Published var theme: AppThemeMode {
        didSet { defaults.set(theme.rawValue, forKey: keyTheme) }
    }

    @Published var homepageUrl: String {
        didSet { defaults.set(homepageUrl, forKey: keyHomepage) }
    }

    @Published var adBlockEnabled: Bool {
        didSet {
            defaults.set(adBlockEnabled, forKey: keyAdBlockEnabled)
            NotificationCenter.default.post(name: NSNotification.Name("RuhiAdBlockSettingsChanged"), object: nil)
        }
    }

    @Published var syncEnabled: Bool {
        didSet { defaults.set(syncEnabled, forKey: keySyncEnabled) }
    }

    @Published var syncEmail: String {
        didSet { defaults.set(syncEmail, forKey: keySyncEmail) }
    }

    @Published var syncDisplayName: String {
        didSet { defaults.set(syncDisplayName, forKey: keySyncDisplayName) }
    }

    @Published var syncUserId: String {
        didSet { defaults.set(syncUserId, forKey: keySyncUserId) }
    }

    // Encrypted in Apple Keychain
    @Published var syncAccessToken: String {
        didSet {
            if syncAccessToken.isEmpty {
                KeychainHelper.shared.delete(key: "sync_access_token")
            } else {
                KeychainHelper.shared.save(key: "sync_access_token", value: syncAccessToken)
            }
            defaults.removeObject(forKey: keySyncAccessToken)
        }
    }

    // Encrypted in Apple Keychain
    @Published var syncRefreshToken: String {
        didSet {
            if syncRefreshToken.isEmpty {
                KeychainHelper.shared.delete(key: "sync_refresh_token")
            } else {
                KeychainHelper.shared.save(key: "sync_refresh_token", value: syncRefreshToken)
            }
            defaults.removeObject(forKey: keySyncRefreshToken)
        }
    }

    @Published var syncDeviceId: String {
        didSet { defaults.set(syncDeviceId, forKey: keySyncDeviceId) }
    }

    @Published var syncDeviceName: String {
        didSet { defaults.set(syncDeviceName, forKey: keySyncDeviceName) }
    }

    private init() {
        let engineStr = defaults.string(forKey: keySearchEngine) ?? SearchEngine.google.rawValue
        self.searchEngine = SearchEngine(rawValue: engineStr) ?? .google

        let themeStr = defaults.string(forKey: keyTheme) ?? AppThemeMode.system.rawValue
        self.theme = AppThemeMode(rawValue: themeStr) ?? .system

        self.homepageUrl = defaults.string(forKey: keyHomepage) ?? "https://www.google.com"
        self.adBlockEnabled = defaults.object(forKey: keyAdBlockEnabled) != nil ? defaults.bool(forKey: keyAdBlockEnabled) : true

        self.syncEnabled = defaults.bool(forKey: keySyncEnabled)
        self.syncEmail = defaults.string(forKey: keySyncEmail) ?? ""
        self.syncDisplayName = defaults.string(forKey: keySyncDisplayName) ?? ""
        self.syncUserId = defaults.string(forKey: keySyncUserId) ?? ""

        // Secure migration from UserDefaults to Apple Keychain
        let keychainAccess = KeychainHelper.shared.read(key: "sync_access_token")
        let legacyAccess = defaults.string(forKey: keySyncAccessToken) ?? ""
        let resolvedAccess = keychainAccess ?? legacyAccess
        self.syncAccessToken = resolvedAccess
        if keychainAccess == nil && !legacyAccess.isEmpty {
            KeychainHelper.shared.save(key: "sync_access_token", value: legacyAccess)
            defaults.removeObject(forKey: keySyncAccessToken)
        }

        let keychainRefresh = KeychainHelper.shared.read(key: "sync_refresh_token")
        let legacyRefresh = defaults.string(forKey: keySyncRefreshToken) ?? ""
        let resolvedRefresh = keychainRefresh ?? legacyRefresh
        self.syncRefreshToken = resolvedRefresh
        if keychainRefresh == nil && !legacyRefresh.isEmpty {
            KeychainHelper.shared.save(key: "sync_refresh_token", value: legacyRefresh)
            defaults.removeObject(forKey: keySyncRefreshToken)
        }

        var devId = defaults.string(forKey: keySyncDeviceId) ?? ""
        if devId.isEmpty {
            devId = UUID().uuidString
            defaults.set(devId, forKey: keySyncDeviceId)
        }
        self.syncDeviceId = devId

        let hostName = Host.current().localizedName ?? "Mac"
        self.syncDeviceName = defaults.string(forKey: keySyncDeviceName) ?? "\(hostName) (Mac)"
    }
}
