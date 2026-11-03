# Cursor Prompt 03 — Phase 3: Tab management

Implement multi-tab support only.

Requirements:
- Introduce a small, testable TabManager and stable tab model.
- Create new regular tabs, switch active tab, close a tab, and handle closing the final tab gracefully.
- Implement the approved grid-card tab switcher with title, URL/domain, favicon or preview where safely available, active state, close action, and New Tab action.
- Show a correct tab count.
- Manage WebView lifecycle carefully; avoid leaking Activities, WebViews, or callbacks.
- Do not serialize WebView objects. Persist only metadata if restoration is implemented.
- Separate regular and incognito tab types in the model, but do not claim full incognito privacy until its dedicated implementation is complete.
- Keep navigation behavior working in each tab.
- Add tests for TabManager state transitions.
- Build and report files changed, tests, and known memory limitations.
