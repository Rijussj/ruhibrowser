# Cursor Prompt — Safe bug-fix rules

When fixing a RUHI Browser bug:
1. Reproduce it and identify the root cause before editing.
2. State which files need changes and why.
3. Make the smallest focused change.
4. Preserve unrelated functionality and user data.
5. Never remove tests to make the build pass.
6. Never suppress errors, bypass TLS checks, or hardcode secrets.
7. Run the narrow relevant tests, then build the app.
8. Report changed files, test/build outcomes, and any remaining uncertainty.
9. If the fix requires a dependency upgrade, database migration, application-ID change, permissions change, or privacy behavior change, ask for approval first.
