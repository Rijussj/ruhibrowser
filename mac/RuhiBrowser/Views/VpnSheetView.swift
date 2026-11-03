import SwiftUI

struct VpnSheetView: View {
    @Environment(\.presentationMode) var presentationMode
    @ObservedObject var vpnManager = VpnManager.shared

    @State private var exportMessage = ""

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack(spacing: 12) {
                Image(systemName: vpnManager.isConnected ? "shield.lefthalf.filled.badge.checkmark" : "shield.slash.fill")
                    .font(.system(size: 26))
                    .foregroundColor(vpnManager.isConnected ? .green : .blue)

                VStack(alignment: .leading, spacing: 2) {
                    Text("RUHI VPN & WireGuard")
                        .font(.headline)
                    Text(vpnManager.isConnected ? "Traffic protected with secure WireGuard encryption" : "Configure and toggle secure tunnel proxy")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()

                Button(action: { presentationMode.wrappedValue.dismiss() }) {
                    Image(systemName: "xmark.circle.fill")
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)
            }
            .padding(16)
            .background(Color(NSColor.windowBackgroundColor))

            Divider()

            // Status Card
            HStack {
                Circle()
                    .fill(vpnManager.isConnected ? Color.green : Color.orange)
                    .frame(width: 10, height: 10)

                VStack(alignment: .leading, spacing: 2) {
                    Text(vpnManager.isConnected ? "VPN Connected" : "VPN Disconnected")
                        .font(.subheadline)
                        .fontWeight(.semibold)
                    Text(vpnManager.isConnected ? "Server: \(vpnManager.serverAddress):\(vpnManager.serverPort)" : "Ready to connect")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()

                Button(action: { vpnManager.toggleConnection() }) {
                    Text(vpnManager.isConnected ? "Disconnect" : "Connect")
                        .font(.caption)
                        .fontWeight(.semibold)
                        .foregroundColor(.white)
                        .padding(.horizontal, 14)
                        .padding(.vertical, 6)
                        .background(vpnManager.isConnected ? Color.red : Color.green)
                        .cornerRadius(6)
                }
                .buttonStyle(.plain)
            }
            .padding(14)
            .background(vpnManager.isConnected ? Color.green.opacity(0.1) : Color.blue.opacity(0.08))

            Divider()

            // Configuration Form
            Form {
                Section(header: Text("WireGuard Server Settings").font(.caption).foregroundColor(.secondary)) {
                    TextField("Server Endpoint / IP", text: $vpnManager.serverAddress)
                    TextField("Port", text: $vpnManager.serverPort)
                    TextField("Client IPv4 / Mask", text: $vpnManager.clientAddress)
                    TextField("DNS Servers", text: $vpnManager.dns)
                }

                Section(header: Text("Cryptographic Keys").font(.caption).foregroundColor(.secondary)) {
                    TextField("Server Public Key", text: $vpnManager.publicKey)
                    SecureField("Client Private Key", text: $vpnManager.privateKey)
                }
            }
            .padding(16)

            if !exportMessage.isEmpty {
                Text(exportMessage)
                    .font(.caption)
                    .foregroundColor(.green)
                    .padding(.bottom, 8)
            }

            Divider()

            // Footer
            HStack {
                Button(action: exportConfig) {
                    Label("Export .conf File", systemImage: "arrow.down.doc.fill")
                        .font(.caption)
                }
                .buttonStyle(.bordered)

                Spacer()

                Button("Close") {
                    presentationMode.wrappedValue.dismiss()
                }
                .keyboardShortcut(.cancelAction)
            }
            .padding(14)
            .background(Color(NSColor.windowBackgroundColor))
        }
        .frame(width: 480, height: 460)
    }

    private func exportConfig() {
        if let dest = vpnManager.exportConfigFile() {
            exportMessage = "Exported to \(dest.lastPathComponent) in Downloads!"
            NSWorkspace.shared.activateFileViewerSelecting([dest])
        }
    }
}
