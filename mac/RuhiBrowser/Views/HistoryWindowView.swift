import SwiftUI

struct HistoryWindowView: View {
    @Environment(\.dismiss) var dismiss
    @ObservedObject var historyManager = HistoryManager.shared
    @State private var searchText: String = ""

    var onSelectUrl: (String) -> Void

    var filteredHistory: [HistoryItem] {
        if searchText.isEmpty {
            return historyManager.history
        }
        return historyManager.history.filter {
            $0.title.localizedCaseInsensitiveContains(searchText) ||
            $0.url.localizedCaseInsensitiveContains(searchText)
        }
    }

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                Text("Browsing History (\(historyManager.history.count))")
                    .font(.headline)
                Spacer()

                if !historyManager.history.isEmpty {
                    Button("Clear History") {
                        historyManager.clearHistory()
                    }
                    .foregroundColor(.red)
                    .font(.caption)
                }

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

            // Search Bar
            HStack {
                Image(systemName: "magnifyingglass")
                    .foregroundColor(.secondary)
                TextField("Search history...", text: $searchText)
                    .textFieldStyle(PlainTextFieldStyle())
                if !searchText.isEmpty {
                    Button(action: { searchText = "" }) {
                        Image(systemName: "xmark.circle.fill")
                            .foregroundColor(.secondary)
                    }
                    .buttonStyle(PlainButtonStyle())
                }
            }
            .padding(8)
            .background(Color(NSColor.controlBackgroundColor))
            .cornerRadius(8)
            .padding(.horizontal)
            .padding(.top, 8)

            // History List
            if filteredHistory.isEmpty {
                VStack(spacing: 8) {
                    Spacer()
                    Image(systemName: "clock")
                        .font(.system(size: 32))
                        .foregroundColor(.secondary)
                    Text("No browsing history")
                        .foregroundColor(.secondary)
                    Spacer()
                }
            } else {
                List {
                    ForEach(filteredHistory) { item in
                        HStack(spacing: 10) {
                            Image(systemName: "clock.arrow.circlepath")
                                .font(.system(size: 13))
                                .foregroundColor(.secondary)

                            VStack(alignment: .leading, spacing: 2) {
                                Text(item.title)
                                    .font(.system(size: 13, weight: .medium))
                                    .lineLimit(1)
                                Text(item.url)
                                    .font(.system(size: 11))
                                    .foregroundColor(.secondary)
                                    .lineLimit(1)
                            }

                            Spacer()

                            Text(timeString(from: item.visitedAt))
                                .font(.system(size: 10))
                                .foregroundColor(.secondary)

                            Button(action: {
                                historyManager.deleteHistoryItem(id: item.id)
                            }) {
                                Image(systemName: "trash")
                                    .font(.system(size: 11))
                                    .foregroundColor(.secondary)
                            }
                            .buttonStyle(PlainButtonStyle())
                        }
                        .contentShape(Rectangle())
                        .onTapGesture {
                            onSelectUrl(item.url)
                            dismiss()
                        }
                    }
                }
            }
        }
        .frame(width: 520, height: 500)
    }

    private func timeString(from date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateStyle = .short
        formatter.timeStyle = .short
        return formatter.string(from: date)
    }
}
