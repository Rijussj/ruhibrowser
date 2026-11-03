# Cursor Prompt 01 — Phase 1: UI foundation

Implement **only Phase 1** of RUHI Browser, after the user has approved the audit.

Before changing anything, summarize the exact files you intend to create/edit and wait for approval if approval has not already been given in this conversation. Do not implement later phases.

Requirements:
- Preserve the existing project structure and working code.
- Use Kotlin and the UI toolkit already approved. Preferred: native Android Views/XML + Material Components. If the project is Compose-based, do not migrate toolkits; ask first.
- Set visible app name to RUHI Browser. Do not change an existing application ID without explicit approval.
- Create a clean home screen with address/search field, shortcut grid, recent-sites placeholder/state, bottom navigation for Home, Bookmarks, Tabs, Menu.
- Add a menu screen with New tab, New incognito tab, Bookmarks, History, Downloads, Desktop site, Find in page, AI Assistant, VPN, Settings. Items not implemented yet must be clearly disabled or hidden; do not make fake actions.
- Add settings sections: Appearance, Search, Privacy & Security, Downloads & Storage, AI & Advanced.
- Add light/dark/system theme support if compatible with existing setup.
- Use accessible labels and proper touch targets.
- Do not implement WebView browsing, Room, downloads, ad blocking, AI, or VPN in this phase.
- Do not introduce secrets, analytics, or unnecessary permissions.
- Build the project and fix only issues caused by this phase.
- Report files changed, UI behavior, build/test results, and remaining issues.
