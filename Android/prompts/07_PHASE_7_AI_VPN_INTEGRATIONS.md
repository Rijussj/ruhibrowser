# Cursor Prompt 07 — Phase 7: Optional AI and VPN integrations

Before implementation, STOP and ask the developer to identify:
- AI provider/backend, API documentation, data-processing expectations, and where credentials will be stored.
- VPN provider/backend, protocol/API documentation, server configuration, legal/privacy terms, and testing environment.

Do not guess a provider, embed a private API key in the APK, download random VPN configuration, or present mock connectivity as real functionality.

AI requirements after provider is confirmed:
- Prefer a secure backend/proxy for long-lived provider credentials.
- Obtain explicit user consent before sending page text, URL, or selected content.
- Minimize data transfer and provide disable controls.
- Clearly label generated content and its limitations.
- Handle rate limits, errors, timeouts, and offline state.
- Never scrape private content without user action.

VPN requirements after provider is confirmed:
- Implement Android VpnService only according to the selected provider's official technical documentation.
- Trigger Android's system VPN consent flow.
- Show accurate disconnected/connecting/connected/error states.
- Implement safe disconnect and lifecycle handling.
- Explain what provider can observe and link to its privacy policy.
- Do not claim a VPN is active unless the system tunnel is actually established and verified.
- If no real provider is selected, keep the VPN feature hidden or explicitly unavailable.

Run a security review, build and tests, and report all data flows and remaining risks.
