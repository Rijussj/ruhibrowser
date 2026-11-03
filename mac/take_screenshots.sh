#!/bin/bash

mkdir -p screenshots

echo ">> Launching RUHI Browser with --tour mode..."
open -a "Release_App/RUHI Browser.app" --args --tour 2>/dev/null || open "Release_App/RUHI Browser.app" --args --tour 2>/dev/null || ./Release_App/RUHI\ Browser.app/Contents/MacOS/RuhiBrowser --tour &

echo ">> Capturing 1: Main Browser..."
sleep 3
screencapture -x screenshots/1_main_browser.png

echo ">> Capturing 2: Settings Window..."
sleep 4
screencapture -x screenshots/2_settings_window.png

echo ">> Capturing 3: Bookmarks Manager..."
sleep 4
screencapture -x screenshots/3_bookmarks_window.png

echo ">> Capturing 4: Supabase Cloud Sync Dialog..."
sleep 4
screencapture -x screenshots/4_supabase_sync_account.png

echo ">> Done! Generated Screenshots:"
ls -la screenshots
