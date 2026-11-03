#!/bin/bash

mkdir -p screenshots

echo ">> Launching RUHI Browser in tour mode..."
open "Release_App/RUHI Browser.app" --args --tour || true

echo ">> Capturing 1: AI Assistant..."
sleep 3
screencapture -x screenshots/1_ai_assistant.png || true

echo ">> Capturing 2: Page Translator..."
sleep 3
screencapture -x screenshots/2_translator.png || true

echo ">> Capturing 3: VPN & WireGuard..."
sleep 3
screencapture -x screenshots/3_vpn_wireguard.png || true

echo ">> Capturing 4: Password Vault..."
sleep 3
screencapture -x screenshots/4_password_vault.png || true

echo ">> Capturing 5: PDF Tools..."
sleep 3
screencapture -x screenshots/5_pdf_tools.png || true

echo ">> Completed! Files in screenshots:"
ls -la screenshots
