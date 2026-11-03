import SwiftUI
import AppKit

struct PasswordsSheetView: View {
    @Environment(\.presentationMode) var presentationMode
    @ObservedObject var vault = PasswordVaultManager.shared

    @State private var searchText = ""
    @State private var revealedId: String? = nil
    @State private var showAddSheet = false

    // New item fields
    @State private var newDomain = ""
    @State private var newUsername = ""
    @State private var newPassword = ""

    var filteredItems: [SavedCredentialItem] {
        if searchText.isEmpty {
            return vault.credentials
        }
        return vault.credentials.filter {
            $0.domain.localizedCaseInsensitiveContains(searchText) ||
            $0.username.localizedCaseInsensitiveContains(searchText)
        }
    }

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack(spacing: 12) {
                Image(systemName: "key.fill")
                    .font(.system(size: 24))
                    .foregroundColor(.yellow)

                VStack(alignment: .leading, spacing: 2) {
                    Text("Password Vault")
                        .font(.headline)
                    Text("Stored securely with Apple Keychain hardware encryption")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()

                Button(action: { showAddSheet.toggle() }) {
                    Label("Add", systemImage: "plus")
                }
                .buttonStyle(.bordered)

                Button(action: { presentationMode.wrappedValue.dismiss() }) {
                    Image(systemName: "xmark.circle.fill")
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)
            }
            .padding(16)
            .background(Color(NSColor.windowBackgroundColor))

            Divider()

            // Add form disclosure
            if showAddSheet {
                VStack(spacing: 10) {
                    TextField("Website Domain (e.g. google.com)", text: $newDomain)
                        .textFieldStyle(.roundedBorder)
                    TextField("Username / Email", text: $newUsername)
                        .textFieldStyle(.roundedBorder)
                    SecureField("Password", text: $newPassword)
                        .textFieldStyle(.roundedBorder)

                    HStack {
                        Button("Cancel") { showAddSheet = false }
                        Spacer()
                        Button("Save to Keychain") {
                            vault.saveCredential(domain: newDomain, username: newUsername, password: newPassword)
                            newDomain = ""
                            newUsername = ""
                            newPassword = ""
                            showAddSheet = false
                        }
                        .buttonStyle(.borderedProminent)
                        .disabled(newDomain.isEmpty || newUsername.isEmpty || newPassword.isEmpty)
                    }
                }
                .padding(14)
                .background(Color.yellow.opacity(0.1))
                Divider()
            }

            // Search Bar
            HStack {
                Image(systemName: "magnifyingglass")
                    .foregroundColor(.secondary)
                TextField("Search saved logins...", text: $searchText)
                    .textFieldStyle(.plain)
            }
            .padding(10)
            .background(Color(NSColor.controlBackgroundColor))

            Divider()

            // Passwords List
            if filteredItems.isEmpty {
                VStack(spacing: 12) {
                    Spacer()
                    Image(systemName: "lock.shield")
                        .font(.system(size: 36))
                        .foregroundColor(.secondary)
                    Text("No Passwords Saved")
                        .font(.headline)
                        .foregroundColor(.secondary)
                    Text("Logins you save in RUHI Browser will be protected here.")
                        .font(.caption)
                        .foregroundColor(.secondary)
                    Spacer()
                }
                .frame(maxWidth: .infinity)
            } else {
                List {
                    ForEach(filteredItems) { item in
                        HStack(spacing: 12) {
                            Image(systemName: "globe")
                                .foregroundColor(.blue)

                            VStack(alignment: .leading, spacing: 2) {
                                Text(item.domain)
                                    .font(.subheadline)
                                    .fontWeight(.medium)
                                Text(item.username)
                                    .font(.caption)
                                    .foregroundColor(.secondary)
                            }

                            Spacer()

                            // Password display
                            if revealedId == item.id {
                                Text(vault.retrievePassword(for: item))
                                    .font(.system(.caption, design: .monospaced))
                                    .foregroundColor(.purple)
                            } else {
                                Text("••••••••")
                                    .font(.caption)
                                    .foregroundColor(.secondary)
                            }

                            Button(action: {
                                revealedId = (revealedId == item.id) ? nil : item.id
                            }) {
                                Image(systemName: revealedId == item.id ? "eye.slash" : "eye")
                                    .foregroundColor(.secondary)
                            }
                            .buttonStyle(.plain)
                            .help("Show/Hide password")

                            Button(action: {
                                let pw = vault.retrievePassword(for: item)
                                NSPasteboard.general.clearContents()
                                NSPasteboard.general.setString(pw, forType: .string)
                            }) {
                                Image(systemName: "doc.on.doc")
                                    .foregroundColor(.secondary)
                            }
                            .buttonStyle(.plain)
                            .help("Copy Password")

                            Button(action: { vault.deleteCredential(id: item.id) }) {
                                Image(systemName: "trash")
                                    .foregroundColor(.red.opacity(0.8))
                            }
                            .buttonStyle(.plain)
                            .help("Delete")
                        }
                        .padding(.vertical, 4)
                    }
                }
            }

            Divider()

            // Footer
            HStack {
                Text("\(vault.credentials.count) saved login(s)")
                    .font(.caption)
                    .foregroundColor(.secondary)
                Spacer()
                Button("Done") { presentationMode.wrappedValue.dismiss() }
                    .keyboardShortcut(.cancelAction)
            }
            .padding(14)
            .background(Color(NSColor.windowBackgroundColor))
        }
        .frame(width: 520, height: 460)
    }
}
