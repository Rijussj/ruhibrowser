# RUHI Browser — UI Design Specification v1.0

## Design direction
Clean, familiar browser layout inspired by modern mobile browsers, but use original RUHI branding and avoid copying proprietary assets. Prioritize legibility, fast access, and one-handed use.

## Main screen
- Top toolbar: RUHI Browser branding or current page title; optional tab count / menu entry depending on available width.
- Address/search field: rounded rectangle; shows URL while browsing, search prompt on home screen; clear/loading state; secure-site indicator where appropriate.
- Home content: editable shortcut grid, optional recent/frequently visited sites, and optional privacy-safe content area. Do not display sensitive browsing suggestions from incognito sessions.
- Web content area: WebView fills remaining space.
- Bottom navigation: Home, Bookmarks, Tabs, Menu.
- Loading progress bar at top of web content.
- Error page: friendly offline/connection message, Retry action, and no deceptive error claims.

## Tabs screen
- Grid cards with title, favicon/preview, URL/domain where useful, close button, active-tab indication.
- New tab button.
- Empty state with clear call to action.
- Incognito tabs visually distinct but not stigmatizing; do not mix incognito and regular tab state.

## Menu
Suggested items:
1. New tab
2. New incognito tab
3. Bookmarks
4. History
5. Downloads
6. Desktop site
7. Find in page
8. AI Assistant (show only if configured/enabled)
9. VPN (show only if a real provider integration is configured; otherwise keep unavailable/hidden)
10. Settings

## Settings categories
- Appearance: System/Light/Dark, homepage, shortcut customization.
- Search: default search engine.
- Privacy & security: incognito explanation, clear browsing data, ad/tracker filtering, site permissions.
- Downloads & storage: download location/status where supported, clear cache/site data.
- AI and advanced features: assistant enable/disable and privacy explanation; VPN only when real integration exists.
- About: version, open-source notices, privacy policy, licenses.

## Visual system
- Use Material Components compatible with the chosen project/dependency versions.
- Rounded corners, restrained elevation, consistent spacing, readable contrast.
- Use RUHI brand colors only after selecting a palette; if no palette is specified, define neutral surfaces with one accessible accent color and document it.
- Support light and dark themes and system theme.
- Avoid overusing gradients, blur, and animation; motion should be subtle and respect system animation settings.
- Use vector drawables or properly licensed assets.
- Ensure buttons have content descriptions and appropriate touch target sizes.

## Responsive behavior
- Support small phones and large phones.
- Do not assume a specific notch/status-bar size.
- Handle portrait and landscape orientation.
- Avoid fixed pixel sizing; use dp/sp and responsive layouts.
