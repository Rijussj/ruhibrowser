import SwiftUI

struct TabStripView: View {
    @Binding var tabs: [BrowserTab]
    @Binding var tabGroups: [TabGroup]
    @Binding var selectedTabId: UUID
    var onNewTab: () -> Void
    var onCloseTab: (UUID) -> Void
    var onAddTabToGroup: (UUID, UUID?) -> Void
    var onEditGroup: (TabGroup) -> Void

    var body: some View {
        HStack(spacing: 2) {
            ScrollView(.horizontal, showsIndicators: false) {
                HStack(spacing: 4) {
                    // Render tab groups and ungrouped tabs
                    ForEach(tabGroups) { group in
                        GroupHeaderBadge(
                            group: group,
                            onEdit: { onEditGroup(group) },
                            onToggleCollapse: {
                                if let idx = tabGroups.firstIndex(where: { $0.id == group.id }) {
                                    tabGroups[idx].isCollapsed.toggle()
                                }
                            }
                        )

                        if !group.isCollapsed {
                            ForEach(tabs.filter { $0.groupId == group.id }) { tab in
                                TabItemView(
                                    tab: tab,
                                    groupColorHex: group.colorHex,
                                    isSelected: tab.id == selectedTabId,
                                    tabGroups: tabGroups,
                                    onSelect: { selectedTabId = tab.id },
                                    onClose: { onCloseTab(tab.id) },
                                    onAssignGroup: { gId in onAddTabToGroup(tab.id, gId) }
                                )
                            }
                        }
                    }

                    // Ungrouped tabs
                    ForEach(tabs.filter { $0.groupId == nil }) { tab in
                        TabItemView(
                            tab: tab,
                            groupColorHex: nil,
                            isSelected: tab.id == selectedTabId,
                            tabGroups: tabGroups,
                            onSelect: { selectedTabId = tab.id },
                            onClose: { onCloseTab(tab.id) },
                            onAssignGroup: { gId in onAddTabToGroup(tab.id, gId) }
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

struct GroupHeaderBadge: View {
    let group: TabGroup
    let onEdit: () -> Void
    let onToggleCollapse: () -> Void

    var body: some View {
        HStack(spacing: 4) {
            Circle()
                .fill(group.color)
                .frame(width: 8, height: 8)

            Text(group.title)
                .font(.system(size: 11, weight: .semibold))
                .foregroundColor(.primary)

            Image(systemName: group.isCollapsed ? "chevron.right" : "chevron.down")
                .font(.system(size: 9))
                .foregroundColor(.secondary)
        }
        .padding(.horizontal, 8)
        .padding(.vertical, 4)
        .background(group.color.opacity(0.15))
        .cornerRadius(6)
        .overlay(
            RoundedRectangle(cornerRadius: 6)
                .stroke(group.color.opacity(0.4), lineWidth: 1)
        )
        .onTapGesture {
            onEdit()
        }
        .contextMenu {
            Button("Edit Group...") { onEdit() }
            Button(group.isCollapsed ? "Expand Group" : "Collapse Group") { onToggleCollapse() }
        }
    }
}

struct TabItemView: View {
    @ObservedObject var tab: BrowserTab
    let groupColorHex: String?
    let isSelected: Bool
    let tabGroups: [TabGroup]
    let onSelect: () -> Void
    let onClose: () -> Void
    let onAssignGroup: (UUID?) -> Void

    @State private var isHovering = false

    var body: some View {
        HStack(spacing: 6) {
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

            Text(tab.title.isEmpty ? "New Tab" : tab.title)
                .font(.system(size: 12, weight: isSelected ? .medium : .regular))
                .foregroundColor(isSelected ? .primary : .secondary)
                .lineLimit(1)
                .frame(maxWidth: 160, alignment: .leading)

            Spacer()

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
                .stroke(
                    groupColorHex != nil ? Color(hex: groupColorHex!).opacity(0.6) : (isSelected ? Color.secondary.opacity(0.15) : Color.clear),
                    lineWidth: groupColorHex != nil ? 1.5 : 1
                )
        )
        .contentShape(Rectangle())
        .onTapGesture {
            onSelect()
        }
        .onHover { hovering in
            isHovering = hovering
        }
        .contextMenu {
            Menu("Add to Group") {
                Button("Create New Group...") {
                    onAssignGroup(UUID())
                }
                Divider()
                ForEach(tabGroups) { g in
                    Button(g.title) {
                        onAssignGroup(g.id)
                    }
                }
            }

            if tab.groupId != nil {
                Button("Remove from Group") {
                    onAssignGroup(nil)
                }
            }

            Divider()

            Button("Close Tab") { onClose() }
        }
    }
}
