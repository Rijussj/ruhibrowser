# RUHI Browser — Technical Architecture

## Recommended stack
- Kotlin
- Android Views + Material Components for the approved UI direction
- Android WebView as browser engine
- MVVM-style separation (Activity/Fragment or screen controller → ViewModel/state holder → repositories/services)
- Room for bookmarks/history if structured local persistence is needed
- DataStore for preferences/settings
- Android DownloadManager for standard downloads
- Android speech recognition APIs/intents for voice search
- Android `VpnService` only with a real VPN provider/backend
- A backend/proxy for AI provider access; no private provider secret embedded in the APK

If the existing project already uses a different UI toolkit, Cursor must report it during audit and ask before replacing the architecture.

## Suggested package structure
`com.ruhi.browser`
- `app/` — application class, app-level configuration
- `ui/home/` — home screen and shortcuts
- `ui/browser/` — browser screen, address bar, loading state
- `ui/tabs/` — tab switcher/grid
- `ui/bookmarks/` — bookmarks list and editing
- `ui/history/` — history list and clearing
- `ui/downloads/` — downloads UI
- `ui/settings/` — settings screens
- `ui/menu/` — menu actions
- `data/local/` — Room database, DAOs, entities
- `data/preferences/` — DataStore settings
- `data/repository/` — repository implementations
- `browser/` — WebView setup, URL resolver, navigation, site permissions
- `tabs/` — tab model and TabManager
- `privacy/` — incognito lifecycle and data clearing
- `blocking/` — ad/tracker rule model and request interception
- `downloads/` — DownloadManager bridge
- `voice/` — speech recognition integration
- `ai/` — optional AI interface/client (only configured provider)
- `vpn/` — optional VpnService/provider integration
- `util/` — safe URL helpers and common utilities

The package structure is a guide, not a reason to create empty classes for every folder. Keep the project as simple as possible while preserving separation of concerns.

## Data model (Room suggested)
### BookmarkEntity
- `id`: Long primary key, auto-generated
- `title`: String
- `url`: String
- `createdAt`: Long epoch milliseconds
- `position`: Int for shortcut/order if needed

### HistoryEntity
- `id`: Long primary key, auto-generated
- `title`: String
- `url`: String
- `visitedAt`: Long epoch milliseconds
- `visitCount`: Int optional
- Do not insert incognito visits.

### Download UI model
Use DownloadManager IDs/status from Android rather than duplicating all transfer state unnecessarily. Store metadata only if required for the app's UI.

### Tab model (in memory)
- `id`: stable unique ID
- `url`: String
- `title`: String
- `isIncognito`: Boolean
- `isLoading`: Boolean
- `progress`: Int
- `canGoBack`: Boolean
- `canGoForward`: Boolean
- WebView reference managed separately and lifecycle-safely

Do not serialize WebView objects. Persist only safe metadata for regular tabs if implementing restoration.

## URL/search resolution
- Trim input.
- Accept only explicitly supported schemes (normally `http` and `https`) for web navigation.
- If input looks like a hostname/domain, add `https://` according to a documented rule.
- Otherwise encode as a query parameter using the selected search engine template.
- Do not concatenate raw user input into a URL without proper encoding.
- Handle `intent:`, `mailto:`, `tel:`, and other external schemes through safe intent handling with user-visible failure fallback.
- Reject or safely handle `javascript:`, `file:`, `content:`, and untrusted local schemes from address-bar input.
- Do not disable TLS verification.

## WebView safety checklist
- JavaScript enabled only as required for normal browsing; document rationale.
- Do not expose a JavaScript interface to arbitrary websites unless there is a narrowly scoped, audited requirement.
- Handle SSL errors by canceling unsafe loads; never call `proceed()` on certificate errors.
- Restrict file/content access settings where possible.
- Handle permission requests with explicit user consent and origin awareness.
- Validate download URLs and MIME types; rely on Android DownloadManager for normal files.
- Handle external intents safely; do not auto-launch unknown apps.
- Test popup/new-window behavior deliberately.

## Ad/tracker filtering design
- Centralize rule matching.
- Use a licensed list and document source/update policy, or start with a small local ruleset.
- Avoid blocking top-level navigation without careful classification.
- Cache parsed rules appropriately.
- Add allowlist and toggle.
- Measure page breakage; allow user to disable filtering.
- Describe feature as best-effort.

## Incognito design
- Prefer an isolated WebView profile/context when supported by the target Android WebView APIs; otherwise implement a conservative dedicated incognito WebView lifecycle and clear cookies/cache/form data when the final private tab closes.
- Never write incognito visits to Room history or normal suggestions.
- Never mix incognito cookies with normal browsing state.
- Clear private state when all incognito tabs are closed and app lifecycle behavior requires it.
- Document limitations and test on multiple Android System WebView versions.

## Build discipline
- Inspect Gradle, AGP, Kotlin, SDK and current package before editing.
- Do not upgrade unrelated dependencies during feature work.
- Build after each phase; report exact build command and outcome.
- Do not delete project files or replace the whole app to implement a single feature.
