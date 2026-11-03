import Foundation

class AiAssistantClient {
    static let shared = AiAssistantClient()
    private let session = URLSession(configuration: .default)

    private init() {}

    func askAi(
        prompt: String,
        pageTitle: String,
        pageUrl: String,
        apiKey: String
    ) async -> String {
        let trimmedKey = apiKey.trimmingCharacters(in: .whitespacesAndNewlines)

        var contextPrompt = "You are RUHI AI Assistant, a helpful assistant built into RUHI Browser."
        if !pageTitle.isEmpty || !pageUrl.isEmpty {
            contextPrompt += "\n[Current Page Context: Title=\(pageTitle), URL=\(pageUrl)]"
        }
        contextPrompt += "\nUser Query: \(prompt)"

        // If user configured a Hugging Face API key
        if !trimmedKey.isEmpty {
            let endpoint = "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.2"
            guard let url = URL(string: endpoint) else { return "Invalid AI endpoint URL." }

            var request = URLRequest(url: url)
            request.httpMethod = "POST"
            request.setValue("Bearer \(trimmedKey)", forHTTPHeaderField: "Authorization")
            request.setValue("application/json", forHTTPHeaderField: "Content-Type")

            let payload: [String: Any] = [
                "inputs": contextPrompt,
                "parameters": [
                    "max_new_tokens": 512,
                    "temperature": 0.7,
                    "return_full_text": false
                ]
            ]

            guard let bodyData = try? JSONSerialization.data(withJSONObject: payload) else {
                return "Failed to serialize AI request."
            }
            request.httpBody = bodyData

            do {
                let (data, response) = try await session.data(for: request)
                if let http = response as? HTTPURLResponse, (200...299).contains(http.statusCode) {
                    if let arr = try? JSONSerialization.jsonObject(with: data) as? [[String: Any]],
                       let first = arr.first,
                       let generated = first["generated_text"] as? String {
                        return generated.trimmingCharacters(in: .whitespacesAndNewlines)
                    }
                    if let dict = try? JSONSerialization.jsonObject(with: data) as? [String: Any],
                       let text = dict["generated_text"] as? String {
                        return text.trimmingCharacters(in: .whitespacesAndNewlines)
                    }
                }
            } catch {
                // Fall back to built-in helper below
            }
        }

        // Built-in intelligent assistant responses if key not set or offline
        let lower = prompt.lowercased()
        if lower.contains("summar") || lower.contains("summary") {
            return "📄 **Page Summary for \(pageTitle.isEmpty ? "Current Webpage" : pageTitle)**:\n\n• URL: \(pageUrl.isEmpty ? "Current tab" : pageUrl)\n• Overview: This webpage contains online content, documentation, or interactive web apps.\n• Tip: To enable live cloud LLM inference, configure your free API token in AI settings."
        } else if lower.contains("who are you") || lower.contains("what is ruhi") {
            return "👋 I am **RUHI AI Assistant**, built directly into RUHI Browser for macOS, Windows, and Android! I can help you summarize articles, answer questions, and navigate the web."
        } else {
            return "🤖 **RUHI AI Assistant**\n\nQuery: \"\(prompt)\"\n\nTo connect live high-speed AI models (Mistral / Llama / Gemini), enter your free Hugging Face API key in the AI Assistant settings.\n\nAlternatively, you can open Claude, ChatGPT, or Google Gemini directly in a new RUHI Browser tab!"
        }
    }
}
