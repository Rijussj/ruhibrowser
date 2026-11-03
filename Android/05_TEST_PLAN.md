# RUHI Browser — Test Plan

## Build and startup
- Clean build succeeds.
- Fresh install launches without crash.
- Relaunch after force-stop.
- Rotate device during page load.
- Simulate low memory/process recreation.

## Navigation
- Load HTTPS URL.
- Enter domain without scheme.
- Search a phrase with spaces, punctuation, and non-English characters.
- Back/forward/refresh/stop.
- Offline and DNS failure states.
- Invalid URL and unsupported scheme.
- External mail/tel/intent link fallback.
- Certificate-error page is canceled safely; no bypass.

## Tabs
- Open several tabs and switch among them.
- Close active and inactive tabs.
- Verify tab count and previews.
- Rotate screen while tab switcher is open.
- Test memory behavior with many tabs.
- Ensure regular and incognito tabs remain separated.

## Bookmarks and history
- Add, open, edit, delete bookmark.
- Reopen app and confirm bookmark persistence.
- Open history item and clear one/all.
- Confirm incognito visits never appear in history or suggestions.

## Incognito
- Start private session and visit multiple sites.
- Close one private tab while another remains.
- Close final private tab and verify expected private state cleanup.
- Confirm regular cookies/history are not mixed with private session.
- Confirm privacy explanation is visible.

## Downloads
- Download a small safe test file.
- Test denied storage/notification permission where relevant.
- Test interrupted/failed download.
- Open completed file through Android's safe file handling.
- Check filename collisions and unsupported MIME types.

## Appearance and settings
- Switch System/Light/Dark.
- Restart and confirm preference persists.
- Change search engine and verify search URL.
- Reorder/hide home shortcuts and verify persistence.

## Permissions and security
- Microphone permission requested only when voice search is invoked.
- Web camera/microphone/location permissions require user consent and origin checks.
- No arbitrary JavaScript bridge exposed.
- No TLS bypass.
- No private API key in source or APK.
- No browsing URL/page content in production logs.
- External schemes do not silently launch unknown apps.

## Accessibility
- TalkBack labels for icon-only buttons.
- Touch targets appropriately sized.
- Text remains readable with system font scaling.
- Contrast works in light and dark themes.
