#!/bin/bash
set -e

echo "=================================================="
echo "      RUHI BROWSER - MACOS APP BUILD SCRIPT       "
echo "=================================================="

# 1. Configuration
APP_NAME="RUHI Browser"
BUNDLE_ID="com.ruhi.browser.mac"
BUILD_DIR=".build"
RELEASE_DIR="Release_App"
DMG_NAME="RuhiBrowser_macOS.dmg"

echo ">> Cleaning previous builds..."
rm -rf "$RELEASE_DIR" "$DMG_NAME"
mkdir -p "$RELEASE_DIR/$APP_NAME.app/Contents/MacOS"
mkdir -p "$RELEASE_DIR/$APP_NAME.app/Contents/Resources"

echo ">> Compiling Swift binary (Universal: Apple Silicon & Intel)..."
swift build -c release --arch arm64 --arch x86_64

echo ">> Copying executable..."
cp "$BUILD_DIR/apple/Products/Release/RuhiBrowser" "$RELEASE_DIR/$APP_NAME.app/Contents/MacOS/RuhiBrowser"
chmod +x "$RELEASE_DIR/$APP_NAME.app/Contents/MacOS/RuhiBrowser"

echo ">> Installing Info.plist and Entitlements..."
cp "RuhiBrowser/Resources/Info.plist" "$RELEASE_DIR/$APP_NAME.app/Contents/Info.plist"
echo "APPL????" > "$RELEASE_DIR/$APP_NAME.app/Contents/PkgInfo"

echo ">> Creating DMG Installer..."
if command -v hdiutil &> /dev/null; then
    hdiutil create -volname "$APP_NAME" -srcfolder "$RELEASE_DIR" -ov -format UDZO "$DMG_NAME"
    echo ">> Successfully created $DMG_NAME!"
else
    echo ">> (hdiutil not available, skipping DMG creation)"
fi

echo "=================================================="
echo "BUILD SUCCESSFUL! App ready at: $RELEASE_DIR/$APP_NAME.app"
echo "=================================================="
