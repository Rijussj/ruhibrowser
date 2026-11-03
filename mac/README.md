# 🍏 RUHI Browser for macOS

A modern, fast, and privacy-focused web browser built natively for **macOS** using **Swift**, **SwiftUI**, and Apple's **WebKit (WKWebView)** engine. Fully integrated with **RUHI Supabase Cloud Sync** for seamless cross-platform syncing with Windows PC and Android phones.

---

## ✨ Key Features

1. **True Apple Native Experience**:
   - Ultra-fast startup and smooth scrolling.
   - Low battery & memory consumption.
   - Native macOS trackpad gestures (2-finger swipe to go back/forward, pinch to zoom).
   - Universal Binary (supports Apple Silicon M1/M2/M3/M4 & Intel Macs).

2. **Ruhi Cloud Sync (Cross-Device)**:
   - **Send Tabs**: Send tabs instantly between Mac, Windows PC, and Android Phone.
   - **Bookmarks 2-Way Sync**: Bookmarks added or deleted on Mac reflect on PC and Mobile.
   - **History Sync**: Real-time cross-device browsing history.
   - **Auto Token Refresh**: Auto-renews Supabase session with zero logout issues.
   - **Profile Card**: Displays account name and online status.

3. **Built-in AdBlock & Privacy**:
   - WebKit Content Rule List blocker for tracker scripts and intrusive ads.
   - Private / Incognito browsing with ephemeral data store (`.nonPersistent()`).

---

## 📂 Project Structure

```
D:\RUHI Apps\RUHI Browser\mac\
├── Package.swift                    # Swift Package Manager Manifest
├── build_mac.sh                     # 1-Click macOS & DMG build script
├── README.md                        # Documentation
└── RuhiBrowser/
    ├── App/
    │   ├── RuhiBrowserApp.swift     # SwiftUI @main entry point & menu bar
    │   └── AppDelegate.swift        # AppKit lifecycle & URL scheme handler
    ├── Models/
    │   ├── BrowserTab.swift         # Active tab & WKWebView model
    │   ├── BookmarkItem.swift       # Bookmark model
    │   ├── HistoryItem.swift        # History visit model
    │   ├── SyncModels.swift         # Device, tab, & password sync models
    │   └── UserSettings.swift       # Search engine & theme preferences
    ├── Services/
    │   ├── SyncManager.swift        # Supabase Cloud Sync & auto 401 refresh
    │   ├── BookmarkManager.swift    # Local JSON bookmarks persistence
    │   ├── HistoryManager.swift     # Local JSON history persistence
    │   ├── SettingsManager.swift    # UserDefaults settings storage
    │   └── AdBlockManager.swift     # Native WebKit Content Blocker
    ├── Views/
    │   ├── MainWindowView.swift     # Main browser UI container
    │   ├── AddressBarView.swift     # Omnibox URL bar, nav buttons & sync icons
    │   ├── TabStripView.swift       # Modern tab strip with close buttons
    │   ├── WebViewContainer.swift   # NSViewRepresentable wrapping WKWebView
    │   ├── AccountSheetView.swift   # Firefox-style Ruhi Account popup
    │   ├── BookmarksWindowView.swift# Bookmarks management sheet
    │   ├── HistoryWindowView.swift  # Browsing history sheet
    │   └── SettingsSheetView.swift  # Preferences window
    └── Resources/
        ├── Info.plist               # App metadata, bundle ID & URL schemes
        └── RuhiBrowser.entitlements # Sandbox & network entitlements
```

---

## 🛠️ How to Build and Run

### Method 1: Using Xcode on a Mac (Recommended for Development)
1. Copy or clone the `mac` folder to your Mac.
2. In Xcode: Select **File > Open** and select the `mac` folder (or double-click `Package.swift`).
3. Press **⌘R** (or click the Play button) to build and run RUHI Browser.

### Method 2: Command Line on Mac (Create `.dmg` Installer)
Open Terminal inside the `mac/` directory and run:
```bash
chmod +x build_mac.sh
./build_mac.sh
```
This will compile the Universal Binary and output **`RuhiBrowser_macOS.dmg`**.

### Method 3: Cloud Build via GitHub Actions (From Windows PC)
A pre-configured GitHub Actions workflow is provided at `.github/workflows/build-mac.yml`.
1. Push this repository to GitHub.
2. In your GitHub repository, click on the **Actions** tab.
3. Select **Build macOS App (RUHI Browser)** and click **Run workflow**.
4. Once completed, download the **`RuhiBrowser_macOS.dmg`** installer directly from the Artifacts section!
