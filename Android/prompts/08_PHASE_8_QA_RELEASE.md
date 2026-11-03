# Cursor Prompt 08 — Phase 8: QA and release readiness

Perform a release-readiness pass for RUHI Browser. Do not sign, upload, publish, or change release credentials without explicit user approval.

Tasks:
- Run available unit and instrumentation tests.
- Build debug variant and report exact commands/results.
- Review navigation, tabs, bookmarks, history, downloads, permissions, incognito, dark mode, and error handling.
- Check accessibility labels and touch targets.
- Search source/configuration for API keys, passwords, tokens, test endpoints and unsafe debug logging. Do not print secret values; report file/line and redact values.
- Confirm SSL errors are canceled and no unsafe bypass exists.
- Confirm incognito visits are excluded from normal history and suggestions.
- Review manifest permissions for least privilege.
- Verify app label RUHI Browser and application ID.
- Review dependencies and licenses; do not upgrade unrelated dependencies automatically.
- Produce a checklist of privacy-policy and Play Console Data safety disclosures that the developer must complete accurately.
- Prepare release build instructions, but do not generate/sign/upload the release unless explicitly requested.
- Report blockers, severity, tests run, tests not run, and recommended fixes.
