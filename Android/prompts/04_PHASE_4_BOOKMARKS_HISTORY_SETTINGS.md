# Cursor Prompt 04 — Phase 4: Bookmarks, history, settings

Implement only bookmarks, normal browsing history, and persistent settings.

Requirements:
- Use Room if compatible with the current project. Add entities, DAOs, database and repository with migrations as needed; do not delete existing user data.
- Bookmark fields: id, title, URL, createdAt, optional position/order.
- History fields: id, title, URL, visitedAt, optional visit count.
- Add, list, open, edit and delete bookmarks.
- List history, open an item, clear one item and clear all history.
- Record regular tab navigation only. Never write incognito visits to history.
- Persist theme, default search engine, homepage, and shortcut configuration using DataStore or the existing approved preference solution.
- Handle empty, loading, error and invalid-data states.
- Add repository tests where practical.
- Do not implement downloads, ad blocking, AI or VPN in this phase.
- Build and report schema/migration details and tests.
