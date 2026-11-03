import SwiftUI

struct AccountSheetView: View {
    @Environment(\.dismiss) var dismiss
    @ObservedObject var syncManager = SyncManager.shared

    @State private var isSignUpMode: Bool = false
    @State private var emailInput: String = ""
    @State private var passwordInput: String = ""
    @State private var displayNameInput: String = ""
    @State private var errorMessage: String = ""
    @State private var isLoading: Bool = false

    @State private var editNameText: String = ""
    @State private var isSavingName: Bool = false
    @State private var saveNameStatus: String = ""
    @State private var linkedDevices: [SyncedDevice] = []

    let currentTabTitle: String
    let currentTabUrl: String

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                Text("RUHI Account")
                    .font(.headline)
                Spacer()
                Button(action: { dismiss() }) {
                    Image(systemName: "xmark.circle.fill")
                        .foregroundColor(.secondary)
                        .font(.title3)
                }
                .buttonStyle(PlainButtonStyle())
            }
            .padding()
            .background(Color(NSColor.windowBackgroundColor))

            Divider()

            ScrollView {
                VStack(spacing: 18) {
                    if syncManager.isLoggedIn {
                        dashboardView
                    } else {
                        authView
                    }
                }
                .padding()
            }
        }
        .frame(width: 440, height: 560)
        .onAppear {
            emailInput = syncManager.currentEmail
            editNameText = syncManager.currentDisplayName
            if syncManager.isLoggedIn {
                Task {
                    await syncManager.refreshUserProfile()
                    editNameText = syncManager.currentDisplayName
                    linkedDevices = await syncManager.getLinkedDevices()
                }
            }
        }
    }

    // MARK: - Dashboard View (Logged In)

    private var dashboardView: some View {
        VStack(spacing: 16) {
            // Profile Card (Firefox Style)
            HStack(spacing: 14) {
                ZStack(alignment: .bottomTrailing) {
                    Circle()
                        .fill(LinearGradient(colors: [Color.blue, Color.purple], startPoint: .topLeading, endPoint: .bottomTrailing))
                        .frame(width: 48, height: 48)

                    Text(syncManager.currentDisplayName.isEmpty ? "R" : String(syncManager.currentDisplayName.prefix(1)).uppercased())
                        .font(.system(size: 20, weight: .bold))
                        .foregroundColor(.white)
                        .frame(width: 48, height: 48)

                    Circle()
                        .fill(Color.green)
                        .frame(width: 12, height: 12)
                        .overlay(Circle().stroke(Color.white, lineWidth: 2))
                }

                VStack(alignment: .leading, spacing: 3) {
                    Text(syncManager.currentDisplayName.isEmpty ? "Ruhi User" : syncManager.currentDisplayName)
                        .font(.system(size: 15, weight: .semibold))
                    Text(syncManager.currentEmail)
                        .font(.system(size: 12))
                        .foregroundColor(.secondary)
                    HStack(spacing: 4) {
                        Circle().fill(Color.green).frame(width: 6, height: 6)
                        Text("Connected to RUHI Supabase Cloud")
                            .font(.system(size: 11))
                            .foregroundColor(.green)
                    }
                }
                Spacer()
            }
            .padding(12)
            .background(Color(NSColor.controlBackgroundColor))
            .cornerRadius(10)

            // Display Name Editor
            VStack(alignment: .leading, spacing: 6) {
                Text("Account Display Name")
                    .font(.caption)
                    .foregroundColor(.secondary)

                HStack {
                    TextField("Enter your name", text: $editNameText)
                        .textFieldStyle(RoundedBorderTextFieldStyle())

                    Button(action: saveDisplayName) {
                        if isSavingName {
                            ProgressView().scaleEffect(0.6).frame(width: 40)
                        } else {
                            Text("Save Name")
                        }
                    }
                    .disabled(isSavingName || editNameText.trimmingCharacters(in: .whitespaces).isEmpty)
                }

                if !saveNameStatus.isEmpty {
                    Text(saveNameStatus)
                        .font(.caption)
                        .foregroundColor(.green)
                }
            }

            // Send Current Tab Button
            VStack(alignment: .leading, spacing: 8) {
                Text("Send Current Tab")
                    .font(.caption)
                    .foregroundColor(.secondary)

                Button(action: {
                    Task {
                        _ = await syncManager.sendTabToDevice(targetDeviceId: nil, url: currentTabUrl, title: currentTabTitle)
                        saveNameStatus = "Tab sent to your phone and PC!"
                    }
                }) {
                    HStack {
                        Image(systemName: "paperplane.fill")
                        Text("Send Tab to All Devices")
                        Spacer()
                    }
                    .padding(8)
                    .frame(maxWidth: .infinity)
                }
                .disabled(currentTabUrl.isEmpty || currentTabUrl.hasPrefix("about:"))
            }

            // Linked Devices List
            VStack(alignment: .leading, spacing: 8) {
                HStack {
                    Text("Linked Devices")
                        .font(.caption)
                        .foregroundColor(.secondary)
                    Spacer()
                    Button("Refresh") {
                        Task { linkedDevices = await syncManager.getLinkedDevices() }
                    }
                    .font(.caption)
                }

                if linkedDevices.isEmpty {
                    Text("No other devices found. Log in on Windows or Android to link them.")
                        .font(.caption)
                        .foregroundColor(.secondary)
                        .padding(8)
                } else {
                    ForEach(linkedDevices) { dev in
                        let isThisDevice = dev.deviceId == SettingsManager.shared.syncDeviceId
                        HStack {
                            Image(systemName: dev.deviceType == "Android" ? "iphone" : (dev.deviceType == "Windows" ? "display" : "laptopcomputer"))
                                .font(.title3)
                                .foregroundColor(.accentColor)
                                .frame(width: 24)

                            VStack(alignment: .leading, spacing: 2) {
                                Text(isThisDevice ? "\(dev.deviceName) (This Mac)" : dev.deviceName)
                                    .font(.system(size: 13, weight: .medium))
                                Text("Type: \(dev.deviceType) • \(dev.activeTabs.count) tabs open")
                                    .font(.system(size: 11))
                                    .foregroundColor(.secondary)
                            }

                            Spacer()

                            if !isThisDevice {
                                Button("Send Tab") {
                                    Task {
                                        _ = await syncManager.sendTabToDevice(targetDeviceId: dev.deviceId, url: currentTabUrl, title: currentTabTitle)
                                        saveNameStatus = "Sent to \(dev.deviceName)!"
                                    }
                                }
                                .font(.caption)
                            }
                        }
                        .padding(8)
                        .background(Color(NSColor.controlBackgroundColor))
                        .cornerRadius(8)
                    }
                }
            }

            // Sync Now & Sign Out Buttons
            HStack {
                Button(action: {
                    syncManager.performFullSync()
                    saveNameStatus = "Syncing with cloud..."
                }) {
                    HStack {
                        Image(systemName: "arrow.triangle.2.circlepath")
                        Text("Sync Now")
                    }
                }

                Spacer()

                Button("Sign Out") {
                    syncManager.logout()
                }
                .foregroundColor(.red)
            }
            .padding(.top, 8)
        }
    }

    // MARK: - Auth View (Login / Sign Up)

    private var authView: some View {
        VStack(spacing: 16) {
            // Mode Switcher (Sign In vs Sign Up)
            Picker("", selection: $isSignUpMode) {
                Text("Sign In").tag(false)
                Text("Create Account").tag(true)
            }
            .pickerStyle(SegmentedPickerStyle())

            if isSignUpMode {
                TextField("Display Name", text: $displayNameInput)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
            }

            TextField("Email address", text: $emailInput)
                .textFieldStyle(RoundedBorderTextFieldStyle())

            SecureField("Password (min 6 characters)", text: $passwordInput)
                .textFieldStyle(RoundedBorderTextFieldStyle())

            if !errorMessage.isEmpty {
                Text(errorMessage)
                    .font(.caption)
                    .foregroundColor(.red)
                    .multilineTextAlignment(.center)
            }

            Button(action: performAuth) {
                if isLoading {
                    ProgressView().scaleEffect(0.6).frame(maxWidth: .infinity)
                } else {
                    Text(isSignUpMode ? "Create Free Sync Account" : "Sign In to Ruhi Sync")
                        .font(.system(size: 13, weight: .semibold))
                        .frame(maxWidth: .infinity)
                }
            }
            .buttonStyle(BorderedProminentButtonStyle())
            .controlSize(.large)
            .disabled(isLoading || emailInput.isEmpty || passwordInput.isEmpty)

            Text("Sync tabs, bookmarks, history and passwords seamlessly across Windows, Android, and Mac.")
                .font(.caption)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)
                .padding(.top, 8)
        }
    }

    private func performAuth() {
        errorMessage = ""
        isLoading = true

        Task {
            if isSignUpMode {
                let (ok, msg) = await syncManager.signUp(email: emailInput, pass: passwordInput, displayName: displayNameInput)
                await MainActor.run {
                    isLoading = false
                    if !ok { errorMessage = msg }
                }
            } else {
                let (ok, msg) = await syncManager.login(email: emailInput, pass: passwordInput)
                await MainActor.run {
                    isLoading = false
                    if !ok { errorMessage = msg }
                }
            }
        }
    }

    private func saveDisplayName() {
        let name = editNameText.trimmingCharacters(in: .whitespaces)
        guard !name.isEmpty else { return }

        isSavingName = true
        saveNameStatus = ""

        Task {
            let ok = await syncManager.updateDisplayName(name: name)
            await MainActor.run {
                isSavingName = false
                saveNameStatus = ok ? "Saved to Supabase Cloud!" : "Saved locally."
            }
        }
    }
}
