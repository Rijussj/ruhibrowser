# Cursor Prompt 05 — Phase 5: Downloads, privacy, ad/tracker filtering

Implement downloads, clear-browsing-data controls, incognito lifecycle, and best-effort ad/tracker filtering. Treat privacy correctness as higher priority than feature count.

Downloads:
- Integrate Android DownloadManager for standard file downloads.
- Validate URL/scheme and handle MIME type, filename collisions, failures, cancellation/status as supported.
- Provide clear progress/status and a safe way to open completed files.
- Request permissions only when required by Android version and use the platform APIs appropriately.

Incognito:
- Implement a dedicated private browsing lifecycle using APIs available in the target Android/WebView versions.
- Keep cookies/storage/history separate from regular browsing to the extent supported.
- Do not record private visits or show them in normal suggestions.
- Clear private state when the final private session/tab closes, using a documented safe strategy.
- Do not claim anonymity from websites, network operators, schools/employers, or ISPs.
- Add an explanatory UI and test isolation on multiple WebView versions.

Clear data:
- Provide user-initiated clearing of normal history, cache/cookies/site data as supported.
- Clearly state what will be removed before confirmation.

Ad/tracker filtering:
- Start with a small documented local ruleset or a legally usable, maintained filter list.
- Centralize URL matching and request interception.
- Add enable/disable control and allowlist/exception support.
- Avoid breaking top-level navigation and essential site resources.
- Do not claim all ads are blocked.
- Do not execute remote scripts or accept unvalidated remote rules.

Security:
- Never bypass TLS errors.
- Do not add unnecessary permissions or a broad JavaScript bridge.
- Do not log browsing URLs/page contents in production.
- Build, test and report limitations and false-positive handling.
