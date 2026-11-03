import SwiftUI

struct LanguageOption: Identifiable {
    let id: String
    let name: String
    let nativeName: String
    let code: String
}

struct TranslateSheetView: View {
    @Environment(\.presentationMode) var presentationMode
    let activeUrlString: String
    let onTranslate: (URL) -> Void

    @State private var selectedCode: String = "hi"
    @State private var searchText: String = ""

    let languages: [LanguageOption] = [
        LanguageOption(id: "hi", name: "Hindi", nativeName: "हिन्दी", code: "hi"),
        LanguageOption(id: "ur", name: "Urdu", nativeName: "اردو", code: "ur"),
        LanguageOption(id: "bn", name: "Bengali", nativeName: "বাংলা", code: "bn"),
        LanguageOption(id: "mr", name: "Marathi", nativeName: "मराठी", code: "mr"),
        LanguageOption(id: "te", name: "Telugu", nativeName: "తెలుగు", code: "te"),
        LanguageOption(id: "ta", name: "Tamil", nativeName: "தமிழ்", code: "ta"),
        LanguageOption(id: "gu", name: "Gujarati", nativeName: "ગુજરાતી", code: "gu"),
        LanguageOption(id: "pa", name: "Punjabi", nativeName: "ਪੰਜਾਬੀ", code: "pa"),
        LanguageOption(id: "en", name: "English", nativeName: "English", code: "en"),
        LanguageOption(id: "ar", name: "Arabic", nativeName: "العربية", code: "ar"),
        LanguageOption(id: "es", name: "Spanish", nativeName: "Español", code: "es"),
        LanguageOption(id: "fr", name: "French", nativeName: "Français", code: "fr"),
        LanguageOption(id: "de", name: "German", nativeName: "Deutsch", code: "de"),
        LanguageOption(id: "ru", name: "Russian", nativeName: "Русский", code: "ru"),
        LanguageOption(id: "ja", name: "Japanese", nativeName: "日本語", code: "ja"),
        LanguageOption(id: "zh-CN", name: "Chinese (Simplified)", nativeName: "简体中文", code: "zh-CN")
    ]

    var filteredLanguages: [LanguageOption] {
        if searchText.isEmpty { return languages }
        return languages.filter {
            $0.name.localizedCaseInsensitiveContains(searchText) ||
            $0.nativeName.localizedCaseInsensitiveContains(searchText) ||
            $0.code.localizedCaseInsensitiveContains(searchText)
        }
    }

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack(spacing: 12) {
                Image(systemName: "globe.asia.australia.fill")
                    .font(.system(size: 26))
                    .foregroundColor(.blue)

                VStack(alignment: .leading, spacing: 2) {
                    Text("Translate Webpage")
                        .font(.headline)
                    Text("Choose a target language to translate the current page")
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

            // Search
            HStack {
                Image(systemName: "magnifyingglass")
                    .foregroundColor(.secondary)
                TextField("Search language...", text: $searchText)
                    .textFieldStyle(.plain)
            }
            .padding(10)
            .background(Color(NSColor.controlBackgroundColor))

            Divider()

            // Language List
            List(filteredLanguages) { lang in
                HStack {
                    VStack(alignment: .leading, spacing: 2) {
                        Text(lang.name)
                            .font(.body)
                            .fontWeight(selectedCode == lang.code ? .semibold : .regular)
                        Text(lang.nativeName)
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }

                    Spacer()

                    if selectedCode == lang.code {
                        Image(systemName: "checkmark.circle.fill")
                            .foregroundColor(.blue)
                    }
                }
                .contentShape(Rectangle())
                .onTapGesture {
                    selectedCode = lang.code
                }
                .padding(.vertical, 4)
            }

            Divider()

            // Actions
            HStack {
                Button("Cancel") {
                    presentationMode.wrappedValue.dismiss()
                }
                .keyboardShortcut(.cancelAction)

                Spacer()

                Button(action: translatePage) {
                    Label("Translate Page", systemImage: "arrow.right.circle.fill")
                        .foregroundColor(.white)
                }
                .buttonStyle(.borderedProminent)
                .tint(.blue)
            }
            .padding(14)
            .background(Color(NSColor.windowBackgroundColor))
        }
        .frame(width: 440, height: 480)
    }

    private func translatePage() {
        guard let encodedUrl = activeUrlString.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) else {
            return
        }
        let translateUrlString = "https://translate.google.com/translate?sl=auto&tl=\(selectedCode)&u=\(encodedUrl)"
        if let targetUrl = URL(string: translateUrlString) {
            onTranslate(targetUrl)
            presentationMode.wrappedValue.dismiss()
        }
    }
}
