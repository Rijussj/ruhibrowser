# RUHI Browser — Cursor AI Development Kit

This ZIP contains the product plan, UI specification, architecture, database/data model, phased implementation prompts, testing checklist, and release checklist for building **RUHI Browser**.

## Important: how to use this kit
1. Open your Android project folder in Cursor.
2. Extract this ZIP somewhere easy to access.
3. Read `01_PRODUCT_REQUIREMENTS.md` and `02_UI_DESIGN_SPEC.md`.
4. Copy `prompts/00_WORKSPACE_AUDIT.md` into Cursor Chat first. This prompt is **inspection-only** and must not change files.
5. Review the audit report.
6. Run implementation prompts in order, one phase at a time. Review the diff and build/test after each phase.
7. Do not run every prompt at once. Never allow a phase to silently rewrite unrelated files.

## Product decisions
- App name: **RUHI Browser**
- Suggested Android application ID: `com.ruhi.browser` (confirm availability and use consistently before first release)
- Platform: Android
- Language: Kotlin
- Engine: Android WebView
- UI: Native Android Views + Material Components (not Jetpack Compose unless the existing project already uses Compose and the developer explicitly approves keeping it)
- Min SDK target for initial setup: API 26 (Android 8.0), subject to dependency compatibility
- Design: clean Chrome-like interface, with RUHI branding and practical customization

## Feature scope
Core browser: address/search bar, navigation, multiple tabs, tab grid previews, bookmarks, history, downloads, home shortcuts, dark mode, incognito, desktop site, find in page, voice search, settings, ad/tracker filtering, optional AI assistant, optional VPN integration.

## Critical reality checks
- A browser's built-in VPN needs a real, legally operated VPN backend/provider and Android `VpnService` implementation. A toggle alone is not a VPN.
- AI features require a selected provider/backend and credentials handled securely. Never hardcode private API secrets into the APK.
- WebView request interception can reduce some ads and trackers, but cannot block every ad or every first-party ad.
- Incognito mode must avoid persistent history/cookies/storage and must be tested carefully. Explain its limits to users; it does not make a user anonymous to websites, network operators, or internet providers.
- Never bypass TLS/certificate errors. Do not add unsafe SSL error handlers.
- Respect Android permissions, Google Play policies, privacy disclosures, and third-party licenses.
