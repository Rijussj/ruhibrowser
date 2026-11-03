import SwiftUI
import WebKit

struct ClearDataSheetView: View {
    @Environment(\.presentationMode) var presentationMode

    @State private var clearHistory = true
    @State private var clearCookies = true
    @State private var clearCache = true
    @State private var statusMessage = ""
    @State private var isClearing = false

    var body: some View {
        VStack(spacing: 20) {
            // Header
            HStack(spacing: 12) {
                Image(systemName: "trash.circle.fill")
                    .font(.system(size: 36))
                    .foregroundColor(.red)

                VStack(alignment: .leading, spacing: 2) {
                    Text("Clear Browsing Data")
                        .font(.headline)
                    Text("Choose the browsing data you want to remove from RUHI Browser.")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }
                Spacer()
            }

            Divider()

            // Options
            VStack(alignment: .leading, spacing: 14) {
                Toggle(isOn: $clearHistory) {
                    VStack(alignment: .leading) {
                        Text("Browsing History")
                            .font(.body)
                        Text("Clears recently visited websites list")
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                }

                Toggle(isOn: $clearCookies) {
                    VStack(alignment: .leading) {
                        Text("Cookies and Site Data")
                            .font(.body)
                        Text("Signs you out of most websites")
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                }

                Toggle(isOn: $clearCache) {
                    VStack(alignment: .leading) {
                        Text("Cached Images and Files")
                            .font(.body)
                        Text("Frees up storage space and speeds up reload")
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                }
            }
            .padding(.horizontal, 8)

            if !statusMessage.isEmpty {
                Text(statusMessage)
                    .font(.callout)
                    .foregroundColor(.green)
            }

            Divider()

            // Action Buttons
            HStack {
                Button("Cancel") {
                    presentationMode.wrappedValue.dismiss()
                }
                .keyboardShortcut(.cancelAction)

                Spacer()

                Button(action: executeClearData) {
                    if isClearing {
                        ProgressView().scaleEffect(0.7)
                    } else {
                        Text("Clear Data Now")
                            .foregroundColor(.white)
                    }
                }
                .buttonStyle(.borderedProminent)
                .tint(.red)
                .disabled(isClearing || (!clearHistory && !clearCookies && !clearCache))
            }
        }
        .padding(24)
        .frame(width: 440)
    }

    private func executeClearData() {
        isClearing = true

        if clearHistory {
            HistoryManager.shared.clearHistory()
        }

        var dataTypes = Set<String>()
        if clearCookies {
            dataTypes.insert(WKWebsiteDataTypeCookies)
            dataTypes.insert(WKWebsiteDataTypeLocalStorage)
            dataTypes.insert(WKWebsiteDataTypeSessionStorage)
        }
        if clearCache {
            dataTypes.insert(WKWebsiteDataTypeDiskCache)
            dataTypes.insert(WKWebsiteDataTypeMemoryCache)
        }

        if !dataTypes.isEmpty {
            let store = WKWebsiteDataStore.default()
            let dateFrom = Date(timeIntervalSince1970: 0)
            store.removeData(ofTypes: dataTypes, modifiedSince: dateFrom) {
                DispatchQueue.main.async {
                    self.isClearing = false
                    self.statusMessage = "Browsing data cleared successfully!"
                    DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
                        self.presentationMode.wrappedValue.dismiss()
                    }
                }
            }
        } else {
            isClearing = false
            presentationMode.wrappedValue.dismiss()
        }
    }
}
