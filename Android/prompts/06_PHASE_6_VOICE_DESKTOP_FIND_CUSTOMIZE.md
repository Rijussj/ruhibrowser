# Cursor Prompt 06 — Phase 6: Voice search, desktop site, find-in-page, home customization

Implement only the following:
1. Voice search through Android speech recognition intent/API. Request microphone permission only when user taps voice search; provide fallback when unsupported or denied. No background recording.
2. Desktop site toggle for the current tab, changing user agent/viewport behavior in a controlled way and reloading safely. Restore mobile behavior when disabled.
3. Find in page using WebView APIs with next/previous controls and match count if available.
4. Home shortcut editing: add/remove/reorder shortcuts and persist settings.
5. Search engine selection from a small documented list with safely encoded query templates.

Keep existing navigation, tabs, bookmarks, history, and privacy behavior working. Add tests for search-engine URL generation and state persistence where practical. Build and report changed files and test outcomes. Do not implement AI or VPN yet.
