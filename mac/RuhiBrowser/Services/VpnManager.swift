import Foundation
import AppKit

class VpnManager: ObservableObject {
    static let shared = VpnManager()

    private let defaults = UserDefaults.standard

    @Published var isConnected: Bool = false
    @Published var serverAddress: String {
        didSet { defaults.set(serverAddress, forKey: "vpn_server_address") }
    }
    @Published var serverPort: String {
        didSet { defaults.set(serverPort, forKey: "vpn_server_port") }
    }
    @Published var clientAddress: String {
        didSet { defaults.set(clientAddress, forKey: "vpn_client_address") }
    }
    @Published var dns: String {
        didSet { defaults.set(dns, forKey: "vpn_dns") }
    }
    @Published var publicKey: String {
        didSet { defaults.set(publicKey, forKey: "vpn_public_key") }
    }
    @Published var privateKey: String {
        didSet { defaults.set(privateKey, forKey: "vpn_private_key") }
    }

    private init() {
        self.serverAddress = defaults.string(forKey: "vpn_server_address") ?? "us-east.wireguard.net"
        self.serverPort = defaults.string(forKey: "vpn_server_port") ?? "51820"
        self.clientAddress = defaults.string(forKey: "vpn_client_address") ?? "10.0.0.2/32"
        self.dns = defaults.string(forKey: "vpn_dns") ?? "1.1.1.1, 1.0.0.1"
        self.publicKey = defaults.string(forKey: "vpn_public_key") ?? ""
        self.privateKey = defaults.string(forKey: "vpn_private_key") ?? ""
    }

    func toggleConnection() {
        isConnected.toggle()
    }

    func exportConfigFile() -> URL? {
        let conf = """
        [Interface]
        PrivateKey = \(privateKey.isEmpty ? "<YOUR_PRIVATE_KEY>" : privateKey)
        Address = \(clientAddress)
        DNS = \(dns)

        [Peer]
        PublicKey = \(publicKey.isEmpty ? "<SERVER_PUBLIC_KEY>" : publicKey)
        Endpoint = \(serverAddress):\(serverPort)
        AllowedIPs = 0.0.0.0/0, ::/0
        PersistentKeepalive = 25
        """

        let downloads = FileManager.default.urls(for: .downloadsDirectory, in: .userDomainMask).first!
        let dest = downloads.appendingPathComponent("RUHI_WireGuard_Profile.conf")
        do {
            try conf.write(to: dest, atomically: true, encoding: .utf8)
            return dest
        } catch {
            return nil
        }
    }
}
