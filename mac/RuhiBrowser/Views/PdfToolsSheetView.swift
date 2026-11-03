import SwiftUI
import PDFKit
import AppKit

struct PdfKitRepresentedView: NSViewRepresentable {
    let document: PDFDocument?

    func makeNSView(context: Context) -> PDFView {
        let pdfView = PDFView()
        pdfView.autoScales = true
        pdfView.displayMode = .singlePageContinuous
        pdfView.displayDirection = .vertical
        pdfView.document = document
        return pdfView
    }

    func updateNSView(_ nsView: PDFView, context: Context) {
        nsView.document = document
    }
}

struct PdfToolsSheetView: View {
    @Environment(\.presentationMode) var presentationMode
    let initialPdfUrl: String?

    @State private var pdfDocument: PDFDocument? = nil
    @State private var pdfPath: String = ""
    @State private var pageCount: Int = 0
    @State private var statusMessage: String = ""

    var body: some View {
        VStack(spacing: 0) {
            // Header
            HStack(spacing: 12) {
                Image(systemName: "doc.richtext.fill")
                    .font(.system(size: 24))
                    .foregroundColor(.red)

                VStack(alignment: .leading, spacing: 2) {
                    Text("RUHI PDF Tools")
                        .font(.headline)
                    Text(pdfPath.isEmpty ? "View, rotate, print, and manage PDF documents" : "\(pdfPath) • \(pageCount) pages")
                        .font(.caption)
                        .foregroundColor(.secondary)
                        .lineLimit(1)
                }

                Spacer()

                Button(action: openFileDialog) {
                    Label("Open PDF...", systemImage: "folder.badge.plus")
                        .font(.caption)
                }
                .buttonStyle(.bordered)

                Button(action: { presentationMode.wrappedValue.dismiss() }) {
                    Image(systemName: "xmark.circle.fill")
                        .foregroundColor(.secondary)
                }
                .buttonStyle(.plain)
            }
            .padding(16)
            .background(Color(NSColor.windowBackgroundColor))

            Divider()

            // Toolbar actions
            if pdfDocument != nil {
                HStack(spacing: 16) {
                    Button(action: rotateClockwise) {
                        Label("Rotate Right", systemImage: "rotate.right")
                            .font(.caption)
                    }
                    .buttonStyle(.plain)

                    Button(action: rotateCounterClockwise) {
                        Label("Rotate Left", systemImage: "rotate.left")
                            .font(.caption)
                    }
                    .buttonStyle(.plain)

                    Divider().frame(height: 16)

                    Button(action: printPdf) {
                        Label("Print Document", systemImage: "printer")
                            .font(.caption)
                    }
                    .buttonStyle(.plain)

                    Button(action: savePdfAs) {
                        Label("Save Copy...", systemImage: "square.and.arrow.down")
                            .font(.caption)
                    }
                    .buttonStyle(.plain)

                    Spacer()

                    if !statusMessage.isEmpty {
                        Text(statusMessage)
                            .font(.caption)
                            .foregroundColor(.green)
                    }
                }
                .padding(.horizontal, 16)
                .padding(.vertical, 8)
                .background(Color(NSColor.controlBackgroundColor))

                Divider()
            }

            // PDF Viewer Area
            if let doc = pdfDocument {
                PdfKitRepresentedView(document: doc)
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
            } else {
                VStack(spacing: 12) {
                    Spacer()
                    Image(systemName: "doc.text.fill")
                        .font(.system(size: 48))
                        .foregroundColor(.red.opacity(0.6))
                    Text("No PDF Document Loaded")
                        .font(.headline)
                        .foregroundColor(.secondary)
                    Text("Click 'Open PDF...' to load any PDF document from your Mac.")
                        .font(.caption)
                        .foregroundColor(.secondary)

                    Button("Browse Files...") {
                        openFileDialog()
                    }
                    .buttonStyle(.borderedProminent)
                    .padding(.top, 8)
                    Spacer()
                }
                .frame(maxWidth: .infinity, maxHeight: .infinity)
            }

            Divider()

            // Footer
            HStack {
                Text(pdfDocument != nil ? "Native Apple PDFKit Engine" : "")
                    .font(.caption)
                    .foregroundColor(.secondary)
                Spacer()
                Button("Done") { presentationMode.wrappedValue.dismiss() }
                    .keyboardShortcut(.cancelAction)
            }
            .padding(14)
            .background(Color(NSColor.windowBackgroundColor))
        }
        .frame(width: 680, height: 560)
        .onAppear {
            loadInitialPdf()
        }
    }

    private func loadInitialPdf() {
        if let initial = initialPdfUrl, !initial.isEmpty {
            if let url = URL(string: initial) {
                if url.isFileURL, let doc = PDFDocument(url: url) {
                    self.pdfDocument = doc
                    self.pdfPath = url.lastPathComponent
                    self.pageCount = doc.pageCount
                    return
                }
            }
        }
    }

    private func openFileDialog() {
        let panel = NSOpenPanel()
        panel.allowedContentTypes = [.pdf]
        panel.allowsMultipleSelection = false
        panel.canChooseDirectories = false
        if panel.runModal() == .OK, let url = panel.url {
            if let doc = PDFDocument(url: url) {
                self.pdfDocument = doc
                self.pdfPath = url.lastPathComponent
                self.pageCount = doc.pageCount
                self.statusMessage = "Loaded \(doc.pageCount) pages"
            }
        }
    }

    private func rotateClockwise() {
        guard let doc = pdfDocument else { return }
        for i in 0..<doc.pageCount {
            if let page = doc.page(at: i) {
                page.rotation = (page.rotation + 90) % 360
            }
        }
        statusMessage = "Rotated 90° Clockwise"
    }

    private func rotateCounterClockwise() {
        guard let doc = pdfDocument else { return }
        for i in 0..<doc.pageCount {
            if let page = doc.page(at: i) {
                page.rotation = (page.rotation + 270) % 360
            }
        }
        statusMessage = "Rotated 90° Counter-Clockwise"
    }

    private func printPdf() {
        guard let doc = pdfDocument else { return }
        let printInfo = NSPrintInfo.shared
        printInfo.horizontalPagination = .fit
        printInfo.verticalPagination = .fit
        let printOp = doc.printOperation(for: printInfo, scalingMode: .pageScaleToFit, autoRotate: true)
        printOp?.run()
    }

    private func savePdfAs() {
        guard let doc = pdfDocument else { return }
        let panel = NSSavePanel()
        panel.allowedContentTypes = [.pdf]
        panel.nameFieldStringValue = "Modified_\(pdfPath)"
        if panel.runModal() == .OK, let dest = panel.url {
            doc.write(to: dest)
            statusMessage = "Saved to \(dest.lastPathComponent)!"
        }
    }
}
