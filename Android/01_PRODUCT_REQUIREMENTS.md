# RUHI Browser — Product Requirements

## 1. Product vision
Build a reliable, fast, privacy-conscious Android web browser with a familiar layout and useful customization. It should prioritize safe browsing and core browser reliability before optional advanced features.

## 2. Primary user journeys
1. Open browser → see home page → search or enter URL → browse → use Back/Forward/Refresh.
2. Open a new tab → switch between tabs using a grid of cards → close one or all tabs.
3. Bookmark a page → open it from Bookmarks → edit/remove bookmark.
4. Review or clear browsing history.
5. Download a file with progress/notification where supported → open it or find it in Downloads.
6. Enable dark appearance and customize home shortcuts.
7. Open an incognito tab and browse without adding the session to regular history.
8. Request desktop site, find text in page, or use voice input.
9. Configure ad/tracker filtering, privacy controls, and optional AI/VPN features.

## 3. MVP requirements (must work before advanced features)
- App launches without crash.
- Home screen includes address/search field, shortcuts, and recent/frequently visited items (with a clear privacy-aware design).
- URL/search resolution: recognize valid URLs; otherwise use a configurable search engine.
- WebView browsing with page loading progress, page title, URL, navigation, refresh, stop loading, external intent handling.
- Back button navigates WebView history before leaving the page.
- New tab, close tab, switch tab, restore active tab after ordinary configuration changes.
- Bookmarks: add, list, open, edit title/URL, delete.
- History: list, open, clear all, clear selected entry.
- Downloads: safe Android DownloadManager integration, progress/status, failure handling.
- Settings persisted across restarts.
- Light/dark/system appearance.
- Safe handling of permissions, file chooser, external schemes, and SSL errors.
- Basic accessibility: meaningful labels, touch targets, TalkBack-friendly controls, text scaling where possible.
- No fake buttons or placeholder controls presented as working features.

## 4. Advanced requirements
### Tabs
- Tab grid cards with title, favicon/preview if available, active state, close button.
- Tab count badge.
- New-tab action.
- Handle many tabs gracefully; avoid retaining every WebView in memory unnecessarily.
- Restore ordinary tabs safely after process recreation if feasible.

### Privacy
- Incognito mode with separate non-persistent browsing context as far as WebView supports.
- Do not store incognito history, bookmarks automatically, or normal-session suggestions.
- Clear incognito WebView state on closing the final incognito tab/session.
- Explain that incognito does not hide activity from websites, employers/schools, network administrators, or ISPs.
- Clear browsing data action for regular profile.
- Permission controls for camera, microphone, location, notifications, and file access.
- Do not grant WebView permissions automatically; request only when needed and after user consent.

### Ad and tracker filtering
- Use a maintained, licensed filter list or a well-defined local rule set.
- Apply request interception carefully; allow users to disable filtering and manage exceptions.
- Avoid breaking essential site functionality. Show that blocking is best-effort, not universal.
- Do not download and execute arbitrary rules or scripts without validation.

### Customization
- Change home shortcut order and visibility.
- Select default search engine from a defined supported list.
- Select theme: System, Light, Dark.
- Optionally set homepage URL after validating scheme.
- Settings should be persisted with DataStore or the existing project’s appropriate persistence layer.

### Voice search
- Use Android speech recognition intent/API with graceful fallback if unavailable.
- Request microphone permission only when needed.
- Do not record or upload audio in the background.

### Desktop site
- Toggle WebView user agent / desktop mode in a controlled way and reload page.
- Restore mobile mode when disabled; do not globally alter unrelated tabs unexpectedly.

### Find in page
- Use WebView find-in-page APIs.
- Display match count if practical and next/previous controls.

### AI assistant (optional, Phase 7)
- Clearly separate assistant UI from webpage content.
- Use a backend/proxy or secure provider integration; never embed long-lived private API keys in the Android client.
- Obtain user consent before sending page text/URLs to an AI service.
- Minimize data sent; provide a way to disable the feature.
- Do not claim that AI responses are guaranteed correct.

### VPN (optional, Phase 7)
- Only implement after choosing a real VPN provider/backend and obtaining its technical/API documentation.
- Use Android `VpnService` with explicit user consent and system VPN permission flow.
- Include connect/disconnect/error states, privacy policy, and clear provider disclosure.
- Never present a local UI toggle as a functioning VPN.
- If no backend/provider has been selected, build only a clearly labeled integration interface and leave the feature disabled; do not fake connectivity.

## 5. Non-functional requirements
- Kotlin, modular and maintainable code.
- Avoid main-thread blocking and memory leaks.
- Use lifecycle-aware components.
- Keep WebView instances and callbacks lifecycle-safe.
- No hardcoded secrets, insecure certificate handling, or broad permissions without need.
- Unit tests for URL resolution, settings, bookmark/history repositories where practical.
- Manual test plan covering supported Android versions and screen sizes.
- Add logging for development but avoid logging sensitive browsing URLs or page content in production.
- All third-party libraries must be maintained, licensed, and version-compatible.

## 6. Out of scope unless separately approved
- Desktop/Windows version.
- Account sync and cloud browsing history.
- Built-in password manager or autofill service.
- Cryptocurrency/wallet functions.
- Custom Chromium engine; Android WebView is the initial engine.
- Circumventing paywalls, certificate errors, or site security restrictions.
