import SwiftUI

struct SettingsSheetView: View {
    @Environment(\.dismiss) var dismiss
    @ObservedObject var settings = SettingsManager.shared

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                Text("Preferences")
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

            Form {
                Section(header: Text("General").font(.subheadline).bold()) {
                    Picker("Default Search Engine", selection: $settings.searchEngine) {
                        ForEach(SearchEngine.allCases) { engine in
                            Text(engine.rawValue).tag(engine)
                        }
                    }

                    Picker("Appearance Theme", selection: $settings.theme) {
                        ForEach(AppThemeMode.allCases) { theme in
                            Text(theme.rawValue).tag(theme)
                        }
                    }

                    TextField("Homepage URL", text: $settings.homepageUrl)
                        .textFieldStyle(RoundedBorderTextFieldStyle())
                }

                Divider().padding(.vertical, 8)

                Section(header: Text("Privacy & Security").font(.subheadline).bold()) {
                    Toggle("Enable Built-in AdBlock & Tracker Protection", isOn: $settings.adBlockEnabled)
                        .help("Blocks popups, tracking scripts, and intrusive ads using WebKit Content Rules.")
                }

                Divider().padding(.vertical, 8)

                Section(header: Text("Device Identity").font(.subheadline).bold()) {
                    TextField("Device Name", text: $settings.syncDeviceName)
                        .textFieldStyle(RoundedBorderTextFieldStyle())
                    Text("Device ID: \(settings.syncDeviceId)")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }
            }
            .padding()

            Spacer()
        }
        .frame(width: 460, height: 420)
    }
}
