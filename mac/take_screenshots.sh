#!/bin/bash

mkdir -p screenshots

echo ">> Launching RUHI Browser in automated tour mode..."
open "Release_App/RUHI Browser.app" --args --tour || true

echo ">> Waiting 3s for browser window to render..."
sleep 3
echo ">> Capturing 01: Main Browser & Tab Groups..."
screencapture -x screenshots/01_main_browser_tab_groups.png || true

sleep 4
echo ">> Capturing 02: AI Assistant (Gemini)..."
screencapture -x screenshots/02_ai_assistant.png || true

sleep 4
echo ">> Capturing 03: Page Translator..."
screencapture -x screenshots/03_page_translator.png || true

sleep 4
echo ">> Capturing 04: RUHI VPN & WireGuard..."
screencapture -x screenshots/04_vpn_wireguard.png || true

sleep 4
echo ">> Capturing 05: Password Vault..."
screencapture -x screenshots/05_password_vault.png || true

sleep 4
echo ">> Capturing 06: PDF Tools & Viewer..."
screencapture -x screenshots/06_pdf_tools.png || true

sleep 4
echo ">> Capturing 07: RUHI Account & Cloud Sync..."
screencapture -x screenshots/07_ruhi_account_sync.png || true

sleep 4
echo ">> Capturing 08: Bookmarks Manager..."
screencapture -x screenshots/08_bookmarks_manager.png || true

sleep 4
echo ">> Capturing 09: Browsing History..."
screencapture -x screenshots/09_browsing_history.png || true

sleep 4
echo ">> Capturing 10: Clear Browsing Data Dialog..."
screencapture -x screenshots/10_clear_browsing_data.png || true

sleep 4
echo ">> Capturing 11: Browser Settings..."
screencapture -x screenshots/11_browser_settings.png || true

echo ">> Automated tour completed! All screenshots captured:"
ls -la screenshots
