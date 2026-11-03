import Foundation
import WebKit

class AdBlockManager {
    static let shared = AdBlockManager()

    private var contentRuleList: WKContentRuleList?

    private init() {
        compileRules()
    }

    func compileRules() {
        // High-performance WebKit Content Blocker JSON rules
        let rules = """
        [
            {
                "trigger": {
                    "url-filter": ".*(doubleclick\\\\.net|googlesyndication\\\\.com|adnxs\\\\.com|adservice\\\\.google\\\\.|popads\\\\.net|exoclick\\\\.com|taboola\\\\.com|outbrain\\\\.com).*"
                },
                "action": {
                    "type": "block"
                }
            },
            {
                "trigger": {
                    "url-filter": ".*",
                    "if-domain": ["*googleanalytics.com", "*hotjar.com"]
                },
                "action": {
                    "type": "block"
                }
            }
        ]
        """

        WKContentRuleListStore.default().compileContentRuleList(
            forIdentifier: "RuhiContentBlocker",
            encodedContentRuleList: rules
        ) { [weak self] list, error in
            if let list = list {
                self?.contentRuleList = list
            }
        }
    }

    func apply(to configuration: WKWebViewConfiguration) {
        guard SettingsManager.shared.adBlockEnabled, let ruleList = contentRuleList else { return }
        configuration.userContentController.add(ruleList)
    }
}
