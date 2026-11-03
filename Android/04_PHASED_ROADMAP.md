# RUHI Browser — Implementation Roadmap

## Phase 0 — Workspace audit (read-only)
Output: project inventory, current build state, architecture recommendation, risks, files likely to change. No file modifications.

## Phase 1 — App foundation and UI shell
- Confirm application ID and app label RUHI Browser.
- Theme, colors, launcher icon placeholder if necessary.
- Home screen, address/search bar, bottom navigation, menu, settings shell.
- Use clear disabled states for features not implemented.
- Build and test.

## Phase 2 — Single-tab browsing
- WebView configuration, URL/search resolver, navigation controls, loading progress.
- Back/forward/refresh/stop.
- Safe external intent handling and SSL-error cancellation.
- Build and test.

## Phase 3 — Multi-tab management
- TabManager, tab creation/close/switch.
- Tab grid cards and active tab.
- Ensure WebViews are lifecycle-safe and avoid memory leaks.
- Build and test.

## Phase 4 — Bookmarks, history, settings persistence
- Room entities/DAOs/repositories if needed.
- Bookmark CRUD, history list/clear, settings with DataStore.
- Exclude private visits.
- Build and test.

## Phase 5 — Downloads, privacy, ad/tracker filtering
- DownloadManager integration and UI.
- Clear browsing data, site permissions, incognito lifecycle.
- Best-effort ad/tracker filtering with toggle and allowlist.
- Security review and test.
- Do not claim complete ad blocking.

## Phase 6 — Voice, desktop site, find in page, home customization
- Voice search with permission/fallback.
- Desktop site toggle.
- Find-in-page UI.
- Editable shortcuts and default search engine.
- Build and test.

## Phase 7 — Optional AI and VPN integrations
- First ask which AI provider/backend and which VPN provider/backend are selected.
- If not selected, do not create fake functionality or connect to random services.
- AI: secure server-side integration, consent, minimal data transfer, disable control.
- VPN: provider documentation, `VpnService`, explicit system consent, connect/disconnect/status/error handling.
- Build and security test.

## Phase 8 — QA, accessibility, privacy, release
- Run unit/instrumentation tests.
- Test on multiple Android versions, phone sizes, network states, process death, rotation.
- Check privacy policy, data safety disclosures, permission rationale, licensing and app icon.
- Generate signed release build only after user confirms signing/release settings.
- Never ask the user to paste signing passwords or private keys into chat.
