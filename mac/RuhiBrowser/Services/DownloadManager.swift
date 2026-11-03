import Foundation
import WebKit
import AppKit

struct DownloadProgressItem: Identifiable {
    let id = UUID()
    let filename: String
    let destinationUrl: URL
    var isCompleted: Bool = false
    var errorDescription: String?
}

class DownloadManager: NSObject, ObservableObject, WKDownloadDelegate {
    static let shared = DownloadManager()

    @Published var activeDownloads: [DownloadProgressItem] = []

    private override init() {
        super.init()
    }

    func download(
        _ download: WKDownload,
        decideDestinationUsing response: URLResponse,
        suggestedFilename: String,
        completionHandler: @escaping (URL?) -> Void
    ) {
        let downloadsDir = FileManager.default.urls(for: .downloadsDirectory, in: .userDomainMask).first!
        var destination = downloadsDir.appendingPathComponent(suggestedFilename)

        // Prevent overwriting if file already exists
        var counter = 1
        let name = (suggestedFilename as NSString).deletingPathExtension
        let ext = (suggestedFilename as NSString).pathExtension

        while FileManager.default.fileExists(atPath: destination.path) {
            let uniqueName = ext.isEmpty ? "\(name) (\(counter))" : "\(name) (\(counter)).\(ext)"
            destination = downloadsDir.appendingPathComponent(uniqueName)
            counter += 1
        }

        let item = DownloadProgressItem(filename: destination.lastPathComponent, destinationUrl: destination)
        DispatchQueue.main.async {
            self.activeDownloads.append(item)
        }

        completionHandler(destination)
    }

    func downloadDidFinish(_ download: WKDownload) {
        DispatchQueue.main.async {
            NSSound(named: "Glass")?.play()
            NotificationCenter.default.post(name: NSNotification.Name("RuhiDownloadCompleted"), object: nil)
        }
    }

    func download(_ download: WKDownload, didFailWithError error: Error, resumeData: Data?) {
        DispatchQueue.main.async {
            print("Download failed: \(error.localizedDescription)")
        }
    }
}
