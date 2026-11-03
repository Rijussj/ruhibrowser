import SwiftUI

struct BookmarksWindowView: View {
    @Environment(\.dismiss) var dismiss
    @ObservedObject var bookmarkManager = BookmarkManager.shared
    @State private var searchText: String = ""

    var onSelectUrl: (String) -> Void

    var filteredBookmarks: [BookmarkItem] {
        if searchText.isEmpty {
            return bookmarkManager.bookmarks
        }
        return bookmarkManager.bookmarks.filter {
            $0.title.localizedCaseInsensitiveContains(searchText) ||
            $0.url.localizedCaseInsensitiveContains(searchText)
        }
    }

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack {
                Text("Bookmarks (\(bookmarkManager.bookmarks.count))")
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

            // Search Bar
            HStack {
                Image(systemName: "magnifyingglass")
                    .foregroundColor(.secondary)
                TextField("Search bookmarks...", text: $searchText)
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

            // Bookmarks List
            if filteredBookmarks.isEmpty {
                VStack(spacing: 8) {
                    Spacer()
                    Image(systemName: "star.slash")
                        .font(.system(size: 32))
                        .foregroundColor(.secondary)
                    Text("No bookmarks found")
                        .foregroundColor(.secondary)
                    Spacer()
                }
            } else {
                List {
                    ForEach(filteredBookmarks) { item in
                        HStack(spacing: 10) {
                            Image(systemName: "star.fill")
                                .font(.system(size: 13))
                                .foregroundColor(.yellow)

                            VStack(alignment: .leading, spacing: 2) {
                                Text(item.title)
                                    .font(.system(size: 13, weight: .medium))
                                    .lineLimit(1)
                                Text(item.url)
                                    .font(.system(size: 11))
                                    .foregroundColor(.secondary)
                                    .lineLimit(1)
                            }
                            .contentShape(Rectangle())
                            .onTapGesture {
                                onSelectUrl(item.url)
                                dismiss()
                            }

                            Spacer()

                            Button(action: {
                                bookmarkManager.removeBookmark(id: item.id)
                            }) {
                                Image(systemName: "trash")
                                    .font(.system(size: 11))
                                    .foregroundColor(.red.opacity(0.8))
                            }
                            .buttonStyle(PlainButtonStyle())
                            .help("Delete bookmark")
                        }
                    }
                }
            }
        }
        .frame(width: 480, height: 480)
    }
}
