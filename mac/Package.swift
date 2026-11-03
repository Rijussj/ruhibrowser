// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "RuhiBrowser",
    platforms: [
        .macOS(.v12)
    ],
    products: [
        .executable(
            name: "RuhiBrowser",
            targets: ["RuhiBrowser"]
        )
    ],
    targets: [
        .executableTarget(
            name: "RuhiBrowser",
            path: "RuhiBrowser",
            exclude: ["Resources/Info.plist", "Resources/RuhiBrowser.entitlements"]
        )
    ]
)
