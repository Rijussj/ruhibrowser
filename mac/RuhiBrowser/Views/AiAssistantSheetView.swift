import SwiftUI

struct AiAssistantSheetView: View {
    @Environment(\.presentationMode) var presentationMode
    let pageTitle: String
    let pageUrl: String

    @AppStorage("ai_api_key") private var apiKey = ""
    @State private var userPrompt = ""
    @State private var includePage = true
    @State private var isThinking = false
    @State private var responseText = ""
    @State private var showSettings = false

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack(spacing: 12) {
                Image(systemName: "sparkles")
                    .font(.system(size: 24))
                    .foregroundColor(.purple)

                VStack(alignment: .leading, spacing: 2) {
                    Text("RUHI AI Assistant")
                        .font(.headline)
                    Text("Smart webpage assistant & general AI knowledge")
                        .font(.caption)
                        .foregroundColor(.secondary)
                }

                Spacer()

                Button(action: { showSettings.toggle() }) {
                    Image(systemName: "gearshape")
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)
                .help("Configure AI API Key")

                Button(action: { presentationMode.wrappedValue.dismiss() }) {
                    Image(systemName: "xmark.circle.fill")
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)
            }
            .padding(16)
            .background(Color(NSColor.windowBackgroundColor))

            Divider()

            // Settings disclosure
            if showSettings {
                VStack(alignment: .leading, spacing: 8) {
                    Text("Hugging Face API Token (Optional)")
                        .font(.caption)
                        .fontWeight(.semibold)
                    SecureField("Paste hf_... API token", text: $apiKey)
                        .textFieldStyle(.roundedBorder)
                    Text("Get a free token from huggingface.co/settings/tokens to enable full cloud LLM generation.")
                        .font(.system(size: 11))
                        .foregroundColor(.secondary)
                }
                .padding(12)
                .background(Color.purple.opacity(0.08))
                Divider()
            }

            // Page context toggle
            if !pageTitle.isEmpty || !pageUrl.isEmpty {
                HStack {
                    Toggle(isOn: $includePage) {
                        HStack(spacing: 4) {
                            Text("Include page context:")
                                .font(.caption)
                                .foregroundColor(.secondary)
                            Text(pageTitle.isEmpty ? pageUrl : pageTitle)
                                .font(.caption)
                                .lineLimit(1)
                                .truncationMode(.tail)
                        }
                    }
                    .toggleStyle(.checkbox)
                    Spacer()
                }
                .padding(.horizontal, 16)
                .padding(.vertical, 8)
                .background(Color(NSColor.controlBackgroundColor).opacity(0.5))
                Divider()
            }

            // Response Box
            ScrollView {
                VStack(alignment: .leading, spacing: 12) {
                    if isThinking {
                        HStack(spacing: 8) {
                            ProgressView()
                                .scaleEffect(0.8)
                            Text("RUHI AI is thinking...")
                                .font(.callout)
                                .foregroundColor(.secondary)
                        }
                        .padding(20)
                    } else if !responseText.isEmpty {
                        Text(responseText)
                            .font(.system(size: 13))
                            .textSelection(.enabled)
                            .lineSpacing(4)
                            .padding(16)
                    } else {
                        VStack(spacing: 12) {
                            Spacer(minLength: 40)
                            Image(systemName: "bubble.left.and.bubble.right.fill")
                                .font(.system(size: 40))
                                .foregroundColor(.purple.opacity(0.6))
                            Text("What would you like help with?")
                                .font(.headline)
                            Text("Ask a question, request a summary, or get key insights about the active webpage.")
                                .font(.caption)
                                .foregroundColor(.secondary)
                                .multilineTextAlignment(.center)
                                .frame(maxWidth: 320)

                            // Quick Suggestions
                            HStack(spacing: 8) {
                                QuickButton(title: "Summarize Page") {
                                    userPrompt = "Please summarize this webpage in 3 concise bullet points."
                                    askAi()
                                }
                                QuickButton(title: "Explain Simply") {
                                    userPrompt = "Explain the core concept of this page in simple terms."
                                    askAi()
                                }
                            }
                            .padding(.top, 8)
                            Spacer(minLength: 40)
                        }
                        .frame(maxWidth: .infinity)
                    }
                }
            }
            .background(Color(NSColor.textBackgroundColor))

            Divider()

            // Input Bar
            HStack(spacing: 10) {
                TextField("Ask RUHI AI anything...", text: $userPrompt)
                    .textFieldStyle(.roundedBorder)
                    .onSubmit { askAi() }

                Button(action: askAi) {
                    Image(systemName: "arrow.up.circle.fill")
                        .font(.system(size: 24))
                        .foregroundColor(userPrompt.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty ? .gray : .purple)
                }
                .buttonStyle(.plain)
                .disabled(userPrompt.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty || isThinking)
            }
            .padding(16)
            .background(Color(NSColor.windowBackgroundColor))
        }
        .frame(width: 520, height: 480)
    }

    private func askAi() {
        let prompt = userPrompt.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !prompt.isEmpty else { return }

        isThinking = true
        responseText = ""

        Task {
            let res = await AiAssistantClient.shared.askAi(
                prompt: prompt,
                pageTitle: includePage ? pageTitle : "",
                pageUrl: includePage ? pageUrl : "",
                apiKey: apiKey
            )
            await MainActor.run {
                self.responseText = res
                self.isThinking = false
            }
        }
    }
}

private struct QuickButton: View {
    let title: String
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            Text(title)
                .font(.caption)
                .fontWeight(.medium)
                .padding(.horizontal, 10)
                .padding(.vertical, 6)
                .background(Color.purple.opacity(0.15))
                .foregroundColor(.purple)
                .cornerRadius(12)
                .overlay(
                    RoundedRectangle(cornerRadius: 12)
                        .stroke(Color.purple.opacity(0.3), lineWidth: 1)
                )
        }
        .buttonStyle(.plain)
    }
}
