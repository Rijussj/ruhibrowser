# Cursor Prompt 02 — Phase 2: Single-tab browsing

Implement only single-tab browsing for RUHI Browser on top of the approved foundation.

Requirements:
- Implement a WebView browser screen and address/search field.
- Resolve input safely: valid http/https URL navigates; domain-like input follows documented scheme policy; other text becomes a properly URL-encoded search query using the selected search engine.
- Add Back, Forward, Refresh, Stop, and page loading progress.
- Update displayed title/URL and secure-site indicator accurately.
- Handle offline/network errors with a useful retry UI.
- Handle external schemes only through safe, user-visible Android intent handling.
- Cancel SSL/certificate errors. Never bypass certificate errors or call proceed().
- Do not expose a JavaScript interface to arbitrary web pages.
- Keep JavaScript and file access settings conservative and documented.
- Do not implement multiple tabs, bookmarks/history persistence, downloads, AI, or VPN yet.
- Respect lifecycle and avoid WebView leaks.
- Add unit tests for URL/search resolution where practical.
- Build and report changed files and test results.
