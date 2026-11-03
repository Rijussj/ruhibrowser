# RUHI Browser — Cursor Workflow

## Recommended sequence
1. Open the project folder in Cursor.
2. Add this kit to the workspace or keep it accessible in another folder.
3. Run Prompt 00 audit. It must not modify files.
4. Review the report and confirm Phase 1 scope.
5. Run Prompt 01; review the diff and build.
6. Run Prompts 02 through 06 one at a time. After every phase, review changed files and build/test.
7. Before Prompt 07, decide whether you truly have an AI provider and a VPN provider. If not, skip real integration and leave these features unavailable.
8. Run Prompt 08 for release readiness.
9. Keep version control enabled and commit a working baseline before each phase.

## Suggested messages to Cursor
- Before a phase: “First list the files you plan to change and the acceptance criteria. Do not edit until I approve.”
- After implementation: “Summarize every changed file, explain important decisions, run the build/tests, and report exact outcomes.”
- If something breaks: use `prompts/09_BUG_FIX_RULES.md`.

## Do not do this
- Do not ask Cursor to “build the whole app in one go”.
- Do not let it replace the entire project or change UI toolkit without approval.
- Do not put API keys, keystore passwords, or VPN credentials in prompts or source code.
- Do not trust a successful compile as proof of privacy/security correctness.
