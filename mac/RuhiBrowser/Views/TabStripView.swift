import SwiftUI

struct TabStripView: View {
    @Binding var tabs: [BrowserTab]
    @Binding var selectedTabId: UUID
    var onNewTab: () -> Void
    var onCloseTab: (UUID) -> Void

    var body: some View {
        HStack(spacing: 2) {
            ScrollView(.horizontal, showsIndicators: false) {
                HStack(spacing: 4) {
                    ForEach(tabs) { tab in
                        TabItemView(
                            tab: tab,
                            isSelected: tab.id == selectedTabId,
                            onSelect: {
                                selectedTabId = tab.id
                            },
                            onClose: {
                                onCloseTab(tab.id)
                            }
                        )
                    }
                }
                .padding(.horizontal, 4)
            }

            // New Tab '+' Button
            Button(action: onNewTab) {
                Image(systemName: "plus")
                    .font(.system(size: 12, weight: .bold))
                    .foregroundColor(.secondary)
                    .frame(width: 28, height: 28)
                    .contentShape(Rectangle())
            }
            .buttonStyle(PlainButtonStyle())
            .help("New Tab (⌘T)")
            .padding(.trailing, 8)
        }
        .frame(height: 36)
        .background(Color(NSColor.windowBackgroundColor))
    }
}

struct TabItemView: View {
    @ObservedObject var tab: BrowserTab
    let isSelected: Bool
    let onSelect: () -> Void
    let onClose: () -> Void

    @State private var isHovering = false

    var body: some View {
        HStack(spacing: 6) {
            // Incognito or Favicon / Globe icon
            if tab.isIncognito {
                Image(systemName: "eye.slash.fill")
                    .font(.system(size: 11))
                    .foregroundColor(.purple)
            } else if tab.isLoading {
                ProgressView()
                    .scaleEffect(0.5)
                    .frame(width: 14, height: 14)
            } else {
                Image(systemName: "globe")
                    .font(.system(size: 11))
                    .foregroundColor(.secondary)
            }

            // Tab Title
            Text(tab.title.isEmpty ? "New Tab" : tab.title)
                .font(.system(size: 12, weight: isSelected ? .medium : .regular))
                .foregroundColor(isSelected ? .primary : .secondary)
                .lineLimit(1)
                .frame(maxWidth: 160, alignment: .leading)

            Spacer()

            // Close button (visible on hover or when selected)
            Button(action: onClose) {
                Image(systemName: "xmark")
                    .font(.system(size: 9, weight: .bold))
                    .foregroundColor(isHovering ? .primary : .secondary)
                    .frame(width: 16, height: 16)
                    .background(isHovering ? Color.secondary.opacity(0.2) : Color.clear)
                    .clipShape(Circle())
            }
            .buttonStyle(PlainButtonStyle())
            .opacity(isHovering || isSelected ? 1.0 : 0.4)
        }
        .padding(.horizontal, 10)
        .frame(width: 180, height: 30)
        .background(
            RoundedRectangle(cornerRadius: 8)
                .fill(isSelected ? Color(NSColor.controlBackgroundColor) : (isHovering ? Color.secondary.opacity(0.08) : Color.clear))
        )
        .overlay(
            RoundedRectangle(cornerRadius: 8)
                .stroke(isSelected ? Color.secondary.opacity(0.15) : Color.clear, lineWidth: 1)
        )
        .contentShape(Rectangle())
        .onTapGesture {
            onSelect()
        }
        .onHover { hovering in
            isHovering = hovering
        }
    }
}
