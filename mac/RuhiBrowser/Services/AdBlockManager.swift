import Foundation
import WebKit

class AdBlockManager {
    static let shared = AdBlockManager()

    private(set) var contentRuleList: WKContentRuleList?

    private init() {
        compileRules()
    }

    func compileRules() {
        // First check if cached compiled rules already exist
        WKContentRuleListStore.default().lookUpContentRuleList(forIdentifier: "RuhiContentBlocker") { [weak self] list, error in
            if let list = list {
                self?.contentRuleList = list
                NotificationCenter.default.post(name: NSNotification.Name("RuhiAdBlockRulesReady"), object: nil)
            } else {
                self?.compileFreshRules()
            }
        }
    }

    private func compileFreshRules() {
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
                NotificationCenter.default.post(name: NSNotification.Name("RuhiAdBlockRulesReady"), object: nil)
            }
        }
    }

    func apply(to configuration: WKWebViewConfiguration) {
        guard SettingsManager.shared.adBlockEnabled, let ruleList = contentRuleList else { return }
        configuration.userContentController.add(ruleList)
    }

    func updateActiveController(_ controller: WKUserContentController) {
        guard let ruleList = contentRuleList else { return }
        controller.remove(ruleList)
        if SettingsManager.shared.adBlockEnabled {
            controller.add(ruleList)
        }
    }
}
