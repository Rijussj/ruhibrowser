# Cursor Prompt 00 — Read-only workspace audit

You are the senior Android engineer auditing a project for **RUHI Browser**.

IMPORTANT: This prompt is inspection-only. Do not create, edit, delete, move, rename, or format any file. Do not run destructive commands. Do not start implementation.

Inspect the current workspace and report:
1. Whether this is an Android project and which build system it uses.
2. Application ID, namespace, app label, min/target/compile SDK, Kotlin, Android Gradle Plugin and Gradle versions.
3. Existing UI toolkit (Views/XML or Compose), entry activities, manifests, themes, dependencies, and current screens.
4. Current build status if a safe build command is available; do not change dependencies to make it build.
5. Any existing code that should be preserved.
6. Missing or risky configuration.
7. A proposed minimal file-by-file plan for Phase 1 only.
8. Exact commands to build and test after Phase 1.

Product decisions:
- App name: RUHI Browser
- Suggested application ID: com.ruhi.browser, but do not change an existing ID automatically.
- Kotlin Android app; WebView browser engine.
- Clean Chrome-like UI with home/search field, shortcut grid, recent sites, tabs grid, bottom navigation (Home, Bookmarks, Tabs, Menu), and settings categories.
- Features will be implemented in phases.

Return an audit report with assumptions and questions. Stop and wait for explicit approval before editing any file.
