#!/bin/bash

mkdir -p screenshots

echo ">> Launching RUHI Browser in tour mode..."
open "Release_App/RUHI Browser.app" --args --tour || true

echo ">> Capturing 1: Main Browser..."
sleep 2.5
screencapture -x screenshots/1_main_browser.png || true

echo ">> Capturing 2: Settings..."
sleep 4
screencapture -x screenshots/2_settings_window.png || true

echo ">> Capturing 3: Bookmarks..."
sleep 4
screencapture -x screenshots/3_bookmarks_window.png || true

echo ">> Capturing 4: Cloud Sync..."
sleep 4
screencapture -x screenshots/4_supabase_sync_account.png || true

echo ">> Capturing 5: Clear Browsing Data Dialog..."
sleep 4
screencapture -x screenshots/5_clear_browsing_data.png || true

echo ">> Completed! Files in screenshots:"
ls -la screenshots
