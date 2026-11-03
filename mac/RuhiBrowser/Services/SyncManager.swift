import Foundation
import Combine
import AppKit

class SyncManager: ObservableObject {
    static let shared = SyncManager()

    static let supabaseUrl = "https://voyzembamrvyrkrdnbsk.supabase.co"
    static let anonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InZveXplbWJhbXJ2eXJrcmRuYnNrIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTM2MDgwNDIsImV4cCI6MjEwOTE4NDA0Mn0.4rmbKyD3HiV70ndDve-eUJg3WYkV7IDDdqOJRrPamXU"

    private let settings = SettingsManager.shared
    private var cancellables = Set<AnyCancellable>()
    private var syncTimer: Timer?
    private var tabCheckTimer: Timer?

    @Published var isLoggedIn: Bool = false
    @Published var currentEmail: String = ""
    @Published var currentDisplayName: String = ""
    @Published var isSyncing: Bool = false
    @Published var lastSyncTime: Date?

    var onTabReceived: ((_ title: String, _ urlString: String) -> Void)?

    private init() {
        updateLoginState()

        // Periodic Tab Checking every 2.5 seconds
        tabCheckTimer = Timer.scheduledTimer(withTimeInterval: 2.5, repeats: true) { [weak self] _ in
            self?.checkIncomingTabs()
        }

        // Periodic Full Sync every 30 seconds
        syncTimer = Timer.scheduledTimer(withTimeInterval: 30.0, repeats: true) { [weak self] _ in
            self?.performFullSync()
        }

        if isLoggedIn {
            DispatchQueue.main.asyncAfter(deadline: .now() + 2.0) { [weak self] in
                self?.performFullSync()
            }
        }
    }

    func updateLoginState() {
        currentEmail = settings.syncEmail
        currentDisplayName = settings.syncDisplayName

        let hasToken = !settings.syncAccessToken.isEmpty || !settings.syncRefreshToken.isEmpty
        isLoggedIn = settings.syncEnabled && hasToken
    }

    // MARK: - JWT Token Helpers

    func isTokenExpired(_ token: String) -> Bool {
        guard !token.isEmpty else { return true }
        let parts = token.components(separatedBy: ".")
        guard parts.count >= 2 else { return true }

        var base64 = parts[1]
            .replacingOccurrences(of: "-", with: "+")
            .replacingOccurrences(of: "_", with: "/")
        while base64.count % 4 != 0 {
            base64.append("=")
        }

        guard let data = Data(base64Encoded: base64),
              let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
              let exp = json["exp"] as? Double else {
            return true
        }

        let now = Date().timeIntervalSince1970
        return now >= (exp - 60)
    }

    // MARK: - Network Request with 401 Auto-Refresh

    private func createRequest(path: String, method: String, body: Data? = nil, headers: [String: String]? = nil) -> URLRequest {
        let url = URL(string: Self.supabaseUrl + path)!
        var req = URLRequest(url: url)
        req.httpMethod = method
        req.timeoutInterval = 12.0
        req.setValue(Self.anonKey, forHTTPHeaderField: "apikey")
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")

        if !settings.syncAccessToken.isEmpty {
            req.setValue("Bearer \(settings.syncAccessToken)", forHTTPHeaderField: "Authorization")
        }

        headers?.forEach { req.setValue($1, forHTTPHeaderField: $0) }
        req.httpBody = body
        return req
    }

    private func sendRequest(path: String, method: String, body: Data? = nil, headers: [String: String]? = nil) async -> (Data?, HTTPURLResponse?) {
        let req = createRequest(path: path, method: method, body: body, headers: headers)
        
        do {
            let (data, response) = try await URLSession.shared.data(for: req)
            guard let httpResp = response as? HTTPURLResponse else { return (nil, nil) }

            // If 401 Unauthorized, refresh the token and retry once
            if httpResp.statusCode == 401 && !settings.syncRefreshToken.isEmpty {
                let refreshed = await refreshSession()
                if refreshed {
                    let retryReq = createRequest(path: path, method: method, body: body, headers: headers)
                    if let (retryData, retryResp) = try? await URLSession.shared.data(for: retryReq),
                       let retryHttp = retryResp as? HTTPURLResponse {
                        return (retryData, retryHttp)
                    }
                } else {
                    await MainActor.run {
                        settings.syncAccessToken = ""
                        updateLoginState()
                    }
                }
            }

            return (data, httpResp)
        } catch {
            return (nil, nil)
        }
    }

    // MARK: - Auth: Refresh Session

    func refreshSession() async -> Bool {
        let rf = settings.syncRefreshToken
        guard !rf.isEmpty else { return false }

        let payload: [String: Any] = ["refresh_token": rf]
        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return false }

        let url = URL(string: "\(Self.supabaseUrl)/auth/v1/token?grant_type=refresh_token")!
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.timeoutInterval = 12.0
        req.setValue(Self.anonKey, forHTTPHeaderField: "apikey")
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.httpBody = body

        do {
            let (data, resp) = try await URLSession.shared.data(for: req)
            guard let http = resp as? HTTPURLResponse, (200...299).contains(http.statusCode) else {
                return false
            }

            if let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
                let newAccess = json["access_token"] as? String ?? ""
                let newRefresh = json["refresh_token"] as? String ?? ""
                let user = json["user"] as? [String: Any]
                let meta = user?["user_metadata"] as? [String: Any]
                let dName = meta?["display_name"] as? String ?? ""

                await MainActor.run {
                    if !newAccess.isEmpty { settings.syncAccessToken = newAccess }
                    if !newRefresh.isEmpty { settings.syncRefreshToken = newRefresh }
                    if !dName.isEmpty { settings.syncDisplayName = dName }
                    updateLoginState()
                }
                return true
            }
        } catch { }

        return false
    }

    // MARK: - Auth: Login & Sign Up

    func login(email: String, pass: String) async -> (Bool, String) {
        let payload: [String: Any] = ["email": email, "password": pass]
        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return (false, "Serialization error") }

        let url = URL(string: "\(Self.supabaseUrl)/auth/v1/token?grant_type=password")!
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.timeoutInterval = 12.0
        req.setValue(Self.anonKey, forHTTPHeaderField: "apikey")
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.httpBody = body

        do {
            let (data, resp) = try await URLSession.shared.data(for: req)
            guard let http = resp as? HTTPURLResponse else { return (false, "Network error") }

            if (200...299).contains(http.statusCode) {
                if let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
                    let user = json["user"] as? [String: Any]
                    let uid = user?["id"] as? String ?? ""
                    let access = json["access_token"] as? String ?? ""
                    let refresh = json["refresh_token"] as? String ?? ""
                    let meta = user?["user_metadata"] as? [String: Any]
                    let dName = meta?["display_name"] as? String ?? ""

                    await MainActor.run {
                        settings.syncEnabled = true
                        settings.syncEmail = email
                        settings.syncUserId = uid
                        settings.syncAccessToken = access
                        settings.syncRefreshToken = refresh
                        if !dName.isEmpty { settings.syncDisplayName = dName }
                        updateLoginState()
                        performFullSync()
                    }
                    return (true, "Logged in successfully!")
                }
            } else {
                let errJson = (try? JSONSerialization.jsonObject(with: data) as? [String: Any]) ?? [:]
                let msg = errJson["error_description"] as? String ?? errJson["msg"] as? String ?? "Login failed."
                return (false, msg)
            }
        } catch {
            return (false, error.localizedDescription)
        }
        return (false, "Unknown error")
    }

    func signUp(email: String, pass: String, displayName: String) async -> (Bool, String) {
        var payload: [String: Any] = ["email": email, "password": pass]
        if !displayName.isEmpty {
            payload["data"] = ["display_name": displayName]
        }
        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return (false, "Serialization error") }

        let url = URL(string: "\(Self.supabaseUrl)/auth/v1/signup")!
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.timeoutInterval = 12.0
        req.setValue(Self.anonKey, forHTTPHeaderField: "apikey")
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.httpBody = body

        do {
            let (data, resp) = try await URLSession.shared.data(for: req)
            guard let http = resp as? HTTPURLResponse else { return (false, "Network error") }

            if (200...299).contains(http.statusCode) {
                if let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
                    let user = json["user"] as? [String: Any]
                    let uid = user?["id"] as? String ?? ""
                    let access = json["access_token"] as? String ?? ""
                    let refresh = json["refresh_token"] as? String ?? ""

                    if !access.isEmpty {
                        await MainActor.run {
                            settings.syncEnabled = true
                            settings.syncEmail = email
                            settings.syncUserId = uid
                            settings.syncAccessToken = access
                            settings.syncRefreshToken = refresh
                            if !displayName.isEmpty { settings.syncDisplayName = displayName }
                            updateLoginState()
                            performFullSync()
                        }
                        return (true, "Account created & synced!")
                    } else {
                        // Email verification required or auto-login
                        return await login(email: email, pass: pass)
                    }
                }
            } else {
                let errJson = (try? JSONSerialization.jsonObject(with: data) as? [String: Any]) ?? [:]
                let msg = errJson["msg"] as? String ?? errJson["error_description"] as? String ?? "Sign up failed."
                return (false, msg)
            }
        } catch {
            return (false, error.localizedDescription)
        }
        return (false, "Unknown error")
    }

    func logout() {
        settings.syncEnabled = false
        settings.syncEmail = ""
        settings.syncDisplayName = ""
        settings.syncUserId = ""
        settings.syncAccessToken = ""
        settings.syncRefreshToken = ""
        updateLoginState()
    }

    // MARK: - Display Name Sync

    func updateDisplayName(name: String) async -> Bool {
        await MainActor.run {
            settings.syncDisplayName = name
            currentDisplayName = name
        }
        guard isLoggedIn else { return true }

        let payload: [String: Any] = ["data": ["display_name": name]]
        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return false }

        let (_, resp) = await sendRequest(path: "/auth/v1/user", method: "PUT", body: body)
        return resp?.statusCode ?? 0 >= 200 && resp?.statusCode ?? 0 <= 299
    }

    func refreshUserProfile() async {
        guard isLoggedIn else { return }
        let (data, resp) = await sendRequest(path: "/auth/v1/user", method: "GET")
        guard let data = data, let http = resp, (200...299).contains(http.statusCode) else { return }

        if let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
           let meta = json["user_metadata"] as? [String: Any],
           let dName = meta["display_name"] as? String, !dName.isEmpty {
            await MainActor.run {
                settings.syncDisplayName = dName
                currentDisplayName = dName
            }
        }
    }

    // MARK: - Tab Sending & Receiving

    func sendTabToDevice(targetDeviceId: String?, url: String, title: String) async -> Bool {
        guard isLoggedIn else { return false }

        var payload: [String: Any] = [
            "user_id": settings.syncUserId,
            "sender_device_id": settings.syncDeviceId,
            "url": url,
            "title": title,
            "is_opened": false
        ]
        if let target = targetDeviceId, !target.isEmpty, target != "null" {
            payload["target_device_id"] = target
        }

        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return false }
        let (_, resp) = await sendRequest(path: "/rest/v1/ruhi_send_tabs", method: "POST", body: body)
        return resp?.statusCode ?? 0 >= 200 && resp?.statusCode ?? 0 <= 299
    }

    func checkIncomingTabs() {
        guard isLoggedIn else { return }
        Task {
            let path = "/rest/v1/ruhi_send_tabs?select=*&is_opened=eq.false&user_id=eq.\(settings.syncUserId)&sender_device_id=neq.\(settings.syncDeviceId)"
            let (data, resp) = await sendRequest(path: path, method: "GET")
            guard let data = data, let http = resp, (200...299).contains(http.statusCode) else { return }

            guard let arr = try? JSONSerialization.jsonObject(with: data) as? [[String: Any]] else { return }

            for item in arr {
                let target = item["target_device_id"] as? String ?? ""
                if target.isEmpty || target == "null" || target == settings.syncDeviceId {
                    let tabId = item["id"] as? String ?? ""
                    let urlStr = item["url"] as? String ?? ""
                    let titleStr = item["title"] as? String ?? ""

                    // Mark as opened
                    _ = await markTabOpened(tabId: tabId)

                    await MainActor.run {
                        onTabReceived?(titleStr, urlStr)
                    }
                }
            }
        }
    }

    private func markTabOpened(tabId: String) async -> Bool {
        let payload: [String: Any] = ["is_opened": true]
        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return false }
        let (_, resp) = await sendRequest(path: "/rest/v1/ruhi_send_tabs?id=eq.\(tabId)", method: "PATCH", body: body)
        return resp?.statusCode ?? 0 >= 200 && resp?.statusCode ?? 0 <= 299
    }

    // MARK: - Bookmarks & History Sync

    func performFullSync() {
        guard isLoggedIn, !isSyncing else { return }
        Task {
            await MainActor.run { isSyncing = true }

            await refreshUserProfile()
            await syncBookmarks()
            await syncHistory()
            await updateHeartbeat()

            await MainActor.run {
                isSyncing = false
                lastSyncTime = Date()
            }
        }
    }

    func syncBookmarks() async {
        guard isLoggedIn else { return }

        // 1. Pull Remote Bookmarks
        let (pullData, pullResp) = await sendRequest(path: "/rest/v1/ruhi_sync_bookmarks?select=*", method: "GET")
        var remoteDeleted = Set<String>()
        if let data = pullData, let http = pullResp, (200...299).contains(http.statusCode),
           let arr = try? JSONSerialization.jsonObject(with: data) as? [[String: Any]] {

            let bmManager = BookmarkManager.shared
            for obj in arr {
                let urlStr = (obj["url"] as? String ?? "").trimmingCharacters(in: .whitespacesAndNewlines)
                let isDel = obj["is_deleted"] as? Bool ?? false
                let title = obj["title"] as? String ?? urlStr

                if isDel {
                    remoteDeleted.insert(urlStr.lowercased())
                    await MainActor.run { bmManager.removeBookmark(url: urlStr) }
                } else if !urlStr.isEmpty && !remoteDeleted.contains(urlStr.lowercased()) {
                    await MainActor.run {
                        if !bmManager.isBookmarked(url: urlStr) {
                            bmManager.addBookmark(title: title, url: urlStr)
                        }
                    }
                }
            }
        }

        // 2. Push Local Bookmarks
        let localList = BookmarkManager.shared.bookmarks.filter { !remoteDeleted.contains($0.url.lowercased()) }
        if !localList.isEmpty {
            let payload = localList.map { b in
                [
                    "id": "mac_\(b.id)",
                    "user_id": settings.syncUserId,
                    "url": b.url,
                    "title": b.title,
                    "folder": "",
                    "is_deleted": false
                ] as [String: Any]
            }

            if let body = try? JSONSerialization.data(withJSONObject: payload) {
                _ = await sendRequest(
                    path: "/rest/v1/ruhi_sync_bookmarks?on_conflict=id",
                    method: "POST",
                    body: body,
                    headers: ["Prefer": "resolution=merge-duplicates"]
                )
            }
        }
    }

    func syncHistory() async {
        guard isLoggedIn else { return }

        // 1. Pull remote history
        let (data, resp) = await sendRequest(path: "/rest/v1/ruhi_sync_history?select=*&order=visited_at.desc&limit=100", method: "GET")
        if let data = data, let http = resp, (200...299).contains(http.statusCode),
           let arr = try? JSONSerialization.jsonObject(with: data) as? [[String: Any]] {
            let histManager = HistoryManager.shared
            for obj in arr {
                let urlStr = obj["url"] as? String ?? ""
                let titleStr = obj["title"] as? String ?? urlStr
                if !urlStr.isEmpty && !histManager.history.contains(where: { $0.url.caseInsensitiveCompare(urlStr) == .orderedSame }) {
                    await MainActor.run {
                        histManager.recordVisit(title: titleStr, url: urlStr)
                    }
                }
            }
        }

        // 2. Push local history
        let local = HistoryManager.shared.history.prefix(50)
        if !local.isEmpty {
            let isoFormatter = ISO8601DateFormatter()
            let payload = local.map { h in
                [
                    "id": "mac_hist_\(h.id)",
                    "user_id": settings.syncUserId,
                    "url": h.url,
                    "title": h.title,
                    "visited_at": isoFormatter.string(from: h.visitedAt)
                ] as [String: Any]
            }

            if let body = try? JSONSerialization.data(withJSONObject: payload) {
                _ = await sendRequest(
                    path: "/rest/v1/ruhi_sync_history?on_conflict=id",
                    method: "POST",
                    body: body,
                    headers: ["Prefer": "resolution=merge-duplicates"]
                )
            }
        }
    }

    func updateHeartbeat(activeUrls: [String] = []) async {
        guard isLoggedIn else { return }

        let payload: [String: Any] = [
            "device_id": settings.syncDeviceId,
            "user_id": settings.syncUserId,
            "device_name": settings.syncDeviceName,
            "device_type": "macOS",
            "active_tabs": activeUrls
        ]
        guard let body = try? JSONSerialization.data(withJSONObject: payload) else { return }

        _ = await sendRequest(
            path: "/rest/v1/ruhi_sync_devices?on_conflict=device_id",
            method: "POST",
            body: body,
            headers: ["Prefer": "resolution=merge-duplicates"]
        )
    }

    func getLinkedDevices() async -> [SyncedDevice] {
        guard isLoggedIn else { return [] }
        let (data, resp) = await sendRequest(path: "/rest/v1/ruhi_sync_devices?select=*&order=last_active.desc", method: "GET")
        guard let data = data, let http = resp, (200...299).contains(http.statusCode) else { return [] }

        return (try? JSONDecoder().decode([SyncedDevice].self, from: data)) ?? []
    }

    func pullPasswords() async -> [SyncedPassword] {
        guard isLoggedIn else { return [] }
        let (data, resp) = await sendRequest(path: "/rest/v1/ruhi_sync_passwords?select=*&order=created_at.desc", method: "GET")
        guard let data = data, let http = resp, (200...299).contains(http.statusCode) else { return [] }

        return (try? JSONDecoder().decode([SyncedPassword].self, from: data)) ?? []
    }
}
