import SwiftUI

struct RenameGroupDialogView: View {
    @Environment(\.presentationMode) var presentationMode
    @Binding var group: TabGroup
    let onSave: (TabGroup) -> Void

    @State private var groupTitle: String
    @State private var selectedColorHex: String

    init(group: Binding<TabGroup>, onSave: @escaping (TabGroup) -> Void) {
        self._group = group
        self.onSave = onSave
        self._groupTitle = State(initialValue: group.wrappedValue.title)
        self._selectedColorHex = State(initialValue: group.wrappedValue.colorHex)
    }

    var body: some View {
        VStack(spacing: 16) {
            // Header
            HStack {
                Circle()
                    .fill(Color(hex: selectedColorHex))
                    .frame(width: 14, height: 14)
                Text("Edit Tab Group")
                    .font(.headline)
                Spacer()
            }

            // Title input
            TextField("Group Name", text: $groupTitle)
                .textFieldStyle(.roundedBorder)

            // Color picker swatches
            VStack(alignment: .leading, spacing: 8) {
                Text("Group Color")
                    .font(.caption)
                    .foregroundColor(.secondary)

                HStack(spacing: 8) {
                    ForEach(TabGroup.presetColors, id: \.hex) { item in
                        Circle()
                            .fill(Color(hex: item.hex))
                            .frame(width: 24, height: 24)
                            .overlay(
                                Circle()
                                    .stroke(Color.primary, lineWidth: selectedColorHex == item.hex ? 2 : 0)
                            )
                            .onTapGesture {
                                selectedColorHex = item.hex
                            }
                    }
                }
            }

            Divider()

            // Action buttons
            HStack {
                Button("Cancel") {
                    presentationMode.wrappedValue.dismiss()
                }
                .keyboardShortcut(.cancelAction)

                Spacer()

                Button("Save") {
                    var updated = group
                    updated.title = groupTitle.isEmpty ? "Group" : groupTitle
                    updated.colorHex = selectedColorHex
                    onSave(updated)
                    presentationMode.wrappedValue.dismiss()
                }
                .buttonStyle(.borderedProminent)
            }
        }
        .padding(20)
        .frame(width: 320)
    }
}
