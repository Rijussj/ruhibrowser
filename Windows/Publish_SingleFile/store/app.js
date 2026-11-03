/**
 * RUHI Web Store - Official Google Chrome Web Store Client Engine
 * High-performance, zero-bloat, native SVG rendering & WebView2 IPC
 */

// Embedded Extension Database (guarantees instant loading even over file:// protocol)
const EXTENSIONS_CATALOG = [
    {
        "id": "ojboogfmlehbcggdlbgacckibolpmcdh",
        "name": "Seller Lens by Flipkart",
        "developer": "Flipkart",
        "version": "1.4.2",
        "category": "shopping",
        "categoryLabel": "Shopping & Savings",
        "rating": 4.7,
        "reviewCount": "1.2K",
        "users": "100,000+",
        "badge": "Verified",
        "shortDescription": "Official marketplace analytics and product research tool for Flipkart sellers to track sales, competitor pricing, and keywords.",
        "description": "Seller Lens by Flipkart empowers e-commerce sellers with real-time product research and marketplace insights.\n\n• Analyze product demand, sales volume, and search trends on Flipkart\n• Monitor competitor pricing, ratings, and FSN details\n• Optimize product listings and boost search rank visibility\n• Calculate profitability, settlements, and margins directly in your browser.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#2874F0\"/><path d=\"M16 16H32V34C32 35.1 31.1 36 30 36H18C16.9 36 16 35.1 16 34V16Z\" fill=\"#FFE500\"/><path d=\"M20 16V13C20 10.8 21.8 9 24 9C26.2 9 28 10.8 28 13V16\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/><circle cx=\"24\" cy=\"26\" r=\"5\" stroke=\"#2874F0\" stroke-width=\"2.5\" fill=\"none\"/><line x1=\"28\" y1=\"30\" x2=\"31\" y2=\"33\" stroke=\"#2874F0\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "tabs",
            "*://*.flipkart.com/*"
        ],
        "size": "3.1 MB",
        "updated": "October 2026"
    },
    {
        "id": "cjpalhdlnbpafiamejdnhcphjbkeiagm",
        "name": "uBlock Origin",
        "developer": "Raymond Hill",
        "version": "1.62.0",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.9,
        "reviewCount": "298K",
        "users": "10,000,000+",
        "badge": "Featured",
        "shortDescription": "An efficient content blocker: easy on memory and CPU footprint, yet can load and enforce thousands more filters than other popular blockers.",
        "description": "uBlock Origin is not just an \"ad blocker\", it's a wide-spectrum content blocker with CPU and memory efficiency as its primary goal.\n\n• Point-and-click to block unwanted elements on any page\n• Out-of-the-box filter lists: EasyList, EasyPrivacy, Peter Lowe's ad servers, Malware domains\n• Dynamic filtering rules for advanced users\n• Dramatically speeds up page loading and saves network bandwidth.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#800000\"/><path d=\"M24 8L11 13V24C11 31.5 16.5 38.5 24 40C31.5 38.5 37 31.5 37 24V13L24 8Z\" fill=\"#B91C1C\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linejoin=\"round\"/><text x=\"24\" y=\"28\" fill=\"#FFFFFF\" font-size=\"14\" font-weight=\"bold\" font-family=\"Arial, sans-serif\" text-anchor=\"middle\">µB</text></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "tabs"
        ],
        "size": "3.8 MB",
        "updated": "September 2026",
        "zipUrl": "https://github.com/gorhill/uBlock/releases/download/1.62.0/uBlock0_1.62.0.chromium.zip"
    },
    {
        "id": "bgnkhhnnamicmpeenaelnjfhikgbkllg",
        "name": "AdGuard AdBlocker",
        "developer": "adguard.com",
        "version": "4.3.48",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.8,
        "reviewCount": "72K",
        "users": "10,000,000+",
        "badge": "Popular",
        "shortDescription": "Unmatched adblock extension against video ads, popups, banner ads, and tracking cookies across all platforms.",
        "description": "AdGuard effectively blocks all types of ads on web pages, including YouTube video ads, popups, and banners.\n\n• Blocks intrusive ads and popups\n• Protects your privacy by blocking third-party tracking systems\n• Speeds up web page loading and saves mobile data.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#68BC71\"/><path d=\"M24 10L14 14V23C14 29.5 18.5 35 24 37C29.5 35 34 29.5 34 23V14L24 10Z\" fill=\"#42954B\" stroke=\"#FFFFFF\" stroke-width=\"2\"/><path d=\"M20 23L23 26L28 20\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "4.2 MB",
        "updated": "August 2026"
    },
    {
        "id": "cfhdojbkjhnklbpkdaibdccddilifddb",
        "name": "Adblock Plus - free ad blocker",
        "developer": "adblockplus.org",
        "version": "3.24.1",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.5,
        "reviewCount": "178K",
        "users": "10,000,000+",
        "badge": "Popular",
        "shortDescription": "Block YouTube ads, pop-ups & fight malware. One of the world's most trusted open-source ad blockers.",
        "description": "Adblock Plus is one of the most popular ad blockers in the world.\n\n• Block annoying video ads, banners and pop-ups\n• Stop tracking and give yourself more privacy\n• Faster page loading times with reduced data usage.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#DC2626\"/><polygon points=\"24,10 36,16 36,32 24,38 12,32 12,16\" fill=\"#B91C1C\" stroke=\"#FFFFFF\" stroke-width=\"2\"/><text x=\"24\" y=\"29\" fill=\"#FFFFFF\" font-size=\"12\" font-weight=\"bold\" font-family=\"Arial, sans-serif\" text-anchor=\"middle\">ABP</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "3.2 MB",
        "updated": "July 2026"
    },
    {
        "id": "pkehgijcmpdhfbdbbnkijodmdjhbjlgp",
        "name": "Privacy Badger",
        "developer": "eff.org",
        "version": "2026.5.1",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "34K",
        "users": "2,000,000+",
        "badge": "Verified",
        "shortDescription": "Automatically learns to block invisible third-party trackers based on their behavior, created by the EFF.",
        "description": "Privacy Badger automatically discovers and blocks trackers based on whether they seem to be following you across different websites.\n\n• Created by the Electronic Frontier Foundation (EFF)\n• Algorithmic tracker learning and defense\n• Global Privacy Control (GPC) signal enabled\n• Zero tracking data collected from users.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1E293B\"/><path d=\"M24 10L12 16V25C12 32 17 38 24 40C31 38 36 32 36 25V16L24 10Z\" fill=\"#E2E8F0\"/><path d=\"M18 19L24 30L30 19H26L24 24L22 19H18Z\" fill=\"#0F172A\"/><circle cx=\"20\" cy=\"22\" r=\"1.8\" fill=\"#EF4444\"/><circle cx=\"28\" cy=\"22\" r=\"1.8\" fill=\"#EF4444\"/></svg>",
        "featured": false,
        "permissions": [
            "privacy",
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "1.8 MB",
        "updated": "June 2026"
    },
    {
        "id": "mlomiejdfkolichcflejclcbmpeaniij",
        "name": "Ghostery Tracker & Ad Blocker",
        "developer": "ghostery.com",
        "version": "10.4.8",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.6,
        "reviewCount": "42K",
        "users": "2,000,000+",
        "badge": "Verified",
        "shortDescription": "Comprehensive privacy suite: blocks trackers, speeds up websites, stops intrusive ads, and provides detailed site telemetry.",
        "description": "Ghostery inspects the hidden web and blocks trackers that monitor your browsing.\n\n• Advanced AI anti-tracking engine\n• Detailed breakdown of who is tracking you on every page\n• Speeds up page rendering significantly.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><path d=\"M16 20C16 15.5 19.5 12 24 12C28.5 12 32 15.5 32 20V32L28 29L24 32L20 29L16 32V20Z\" fill=\"#FFFFFF\"/><circle cx=\"21\" cy=\"20\" r=\"2.5\" fill=\"#0284C7\"/><circle cx=\"27\" cy=\"20\" r=\"2.5\" fill=\"#0284C7\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "5.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "bkdgflcldnnnapblkhphbgpggdiikppg",
        "name": "DuckDuckGo Privacy Essentials",
        "developer": "duckduckgo.com",
        "version": "2026.4.15",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "22K",
        "users": "6,000,000+",
        "badge": "Popular",
        "shortDescription": "Tracker radar, private search, and automatic smart HTTPS encryption for ultimate peace of mind.",
        "description": "Privacy simplified: DuckDuckGo Privacy Essentials provides seamless tracker protection, smarter encryption, and private search in one package.\n\n• Tracker Radar automatically blocks hidden trackers\n• Privacy Grade rating (A to F) for each site\n• Forces encrypted connections (HTTPS) whenever available.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#DE5833\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#FFFFFF\"/><circle cx=\"21\" cy=\"20\" r=\"2\" fill=\"#0F172A\"/><path d=\"M23 23L32 25L24 28Z\" fill=\"#F59E0B\"/><path d=\"M20 28C22 30 26 30 28 28\" stroke=\"#0F172A\" stroke-width=\"1.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "2.9 MB",
        "updated": "June 2026"
    },
    {
        "id": "ddkjiahejlhfcafbddmgiahcphecmpfh",
        "name": "uBlock Origin Lite",
        "developer": "Raymond Hill",
        "version": "2026.8.0",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.8,
        "reviewCount": "16K",
        "users": "1,000,000+",
        "badge": "Featured",
        "shortDescription": "Permission-less MV3 content blocker. Ultra lightweight ad blocking designed for next-generation Chromium browsers.",
        "description": "uBlock Origin Lite is an experimental Manifest V3 content blocker that does not require broad \"read and change all your data\" permissions.\n\n• Zero intrusive permissions required out-of-the-box\n• Uses native Chromium declarativeNetRequest filtering\n• Extremely light on system memory and battery.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#7F1D1D\"/><path d=\"M24 10L14 15V24C14 30.5 18.5 36.5 24 38C29.5 36.5 34 30.5 34 24V15L24 10Z\" fill=\"#DC2626\"/><text x=\"24\" y=\"27\" fill=\"#FFFFFF\" font-size=\"11\" font-weight=\"bold\" font-family=\"Arial, sans-serif\" text-anchor=\"middle\">uBOL</text></svg>",
        "featured": false,
        "permissions": [
            "declarativeNetRequest",
            "storage"
        ],
        "size": "1.4 MB",
        "updated": "August 2026"
    },
    {
        "id": "ihcjicgdanjaechkgeegckofjjedodee",
        "name": "Malwarebytes Browser Guard",
        "developer": "malwarebytes.com",
        "version": "3.0.4",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "28K",
        "users": "4,000,000+",
        "badge": "Verified",
        "shortDescription": "Crush unwanted content, stop tech support scams, phishing attempts, malware domains, and crypto-jackers.",
        "description": "Malwarebytes Browser Guard is an all-in-one browser protection extension that delivers a safer, faster web browsing experience.\n\n• Blocks third-party ads and trackers\n• Detects and blocks malicious phishing web pages\n• Prevents browser lockers and scam popups.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#005EE6\"/><path d=\"M14 16L24 10L34 16V26C34 33 24 38 24 38C24 38 14 33 14 26V16Z\" fill=\"#FFFFFF\"/><path d=\"M20 23L24 27L28 20\" stroke=\"#005EE6\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "6.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "nngceckbapebfimnlniiiahkandclblb",
        "name": "Bitwarden - Free Password Manager",
        "developer": "bitwarden.com",
        "version": "2026.8.0",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.9,
        "reviewCount": "112K",
        "users": "6,000,000+",
        "badge": "Verified",
        "shortDescription": "A secure and free password manager for all of your devices. Auto-fill credentials with end-to-end zero-knowledge encryption.",
        "description": "Bitwarden is the easiest and safest way to store all of your logins and passwords while conveniently keeping them synced between all of your devices.\n\n• Zero-knowledge AES-256 bit end-to-end encryption\n• Strong password & passkey generator\n• Secure storage for payment cards, identities, and private notes\n• Cross-platform synchronization across desktop and mobile.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#175DDC\"/><path d=\"M24 10C16 10 13 14 13 14V26C13 32.5 19.5 37 24 38C28.5 37 35 32.5 35 26V14C35 14 32 10 24 10Z\" fill=\"#0F47B2\" stroke=\"#FFFFFF\" stroke-width=\"2\"/><path d=\"M24 16V32M19 19H29M19 25H29\" stroke=\"#FFFFFF\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "clipboardWrite",
            "contextMenus",
            "tabs"
        ],
        "size": "5.6 MB",
        "updated": "September 2026"
    },
    {
        "id": "aeblfdkhhhdcdjpifhhbdiojplfjncoa",
        "name": "1Password - Password Manager",
        "developer": "1password.com",
        "version": "2.26.1",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.8,
        "reviewCount": "38K",
        "users": "4,000,000+",
        "badge": "Verified",
        "shortDescription": "Seamless password autofill, passkeys support, and credit card security for your daily online life.",
        "description": "1Password remembers all your passwords and sensitive credentials so you don't have to.\n\n• Full support for modern Passkeys (FIDO2 / WebAuthn)\n• Automatic 2FA code generator and autofill\n• Watchtower breach alerts for compromised passwords.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0A85EA\"/><circle cx=\"24\" cy=\"24\" r=\"14\" stroke=\"#FFFFFF\" stroke-width=\"3\"/><circle cx=\"24\" cy=\"21\" r=\"3\" fill=\"#FFFFFF\"/><rect x=\"22.5\" y=\"21\" width=\"3\" height=\"8\" rx=\"1\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "clipboardWrite",
            "<all_urls>"
        ],
        "size": "14.2 MB",
        "updated": "August 2026"
    },
    {
        "id": "hdokiejnpimakedhajhdlcegeplioahd",
        "name": "LastPass: Free Password Manager",
        "developer": "lastpass.com",
        "version": "4.133.0",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.4,
        "reviewCount": "31K",
        "users": "10,000,000+",
        "badge": "Popular",
        "shortDescription": "Store and autofill passwords, credit cards, and addresses anywhere across the web.",
        "description": "LastPass puts you in control of your digital life—making it easy to keep your critical information safe and secure.\n\n• Store logins, passwords, notes and payment profiles\n• Generate long, randomized passwords in 1-click\n• Secure digital vault with master password protection.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#D32D27\"/><circle cx=\"16\" cy=\"24\" r=\"3.5\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"24\" r=\"3.5\" fill=\"#FFFFFF\"/><circle cx=\"32\" cy=\"24\" r=\"3.5\" fill=\"#FFFFFF\"/><line x1=\"39\" y1=\"17\" x2=\"39\" y2=\"31\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "12.8 MB",
        "updated": "July 2026"
    },
    {
        "id": "oboonakemofpalcgghocfoadofidjkkk",
        "name": "KeePassXC-Browser",
        "developer": "keepassxc.org",
        "version": "1.9.4",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.8,
        "reviewCount": "8.2K",
        "users": "500,000+",
        "badge": "Verified",
        "shortDescription": "Connect your local offline KeePassXC password database directly into RUHI Browser for instant secure autofill.",
        "description": "KeePassXC-Browser is the official browser extension for the KeePassXC password manager, allowing full offline local database security without third-party cloud dependence.\n\n• 100% offline local database storage\n• Full TOTP two-factor authentication autofill\n• Automatic credential detection and entry saving.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#15803D\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#166534\"/><path d=\"M24 16C21 16 19 18 19 21C19 23 20 25 22 26V32H26V26C28 25 29 23 29 21C29 18 27 16 24 16Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "nativeMessaging",
            "storage",
            "tabs"
        ],
        "size": "1.1 MB",
        "updated": "May 2026"
    },
    {
        "id": "fjoaledfpmneenckfbpdfhkmimnjocfa",
        "name": "NordVPN - VPN Proxy for Chrome",
        "developer": "nordvpn.com",
        "version": "3.16.2",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.6,
        "reviewCount": "35K",
        "users": "2,000,000+",
        "badge": "Popular",
        "shortDescription": "Ultra-fast VPN proxy with Threat Protection, WebRTC leak shield, and 5000+ worldwide servers.",
        "description": "Experience internet freedom with NordVPN extension. Enjoy private browsing, change your virtual IP location in seconds, and protect your identity online.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#4687FF\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><path d=\"M24 15L16 30H20L24 23L28 30H32L24 15Z\" fill=\"#1E3A8A\"/></svg>",
        "featured": false,
        "permissions": [
            "proxy",
            "storage",
            "webRequest",
            "<all_urls>"
        ],
        "size": "4.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "jplgfhpmjnbigmhklmmbgecoobifkmpa",
        "name": "Proton VPN: Fast & Secure VPN",
        "developer": "protonvpn.com",
        "version": "1.4.1",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "12K",
        "users": "1,000,000+",
        "badge": "Verified",
        "shortDescription": "Swiss-based encrypted VPN proxy created by CERN scientists for high-speed, zero-log internet privacy.",
        "description": "Proton VPN gives you secure, private, and encrypted internet access with advanced security features from the creators of Proton Mail.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#6D4AFF\"/><path d=\"M24 12L34 20V28C34 33 29 36 24 37C19 36 14 33 14 28V20L24 12Z\" fill=\"#5333E0\"/><polygon points=\"25,18 19,26 24,26 23,32 29,24 24,24\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "proxy",
            "storage",
            "privacy"
        ],
        "size": "3.6 MB",
        "updated": "August 2026"
    },
    {
        "id": "ponfpcnoihnaknobfeghlldflbebgflb",
        "name": "Enhancer for YouTube™",
        "developer": "mrfdev.com",
        "version": "2.0.126",
        "category": "media",
        "categoryLabel": "Entertainment",
        "rating": 4.8,
        "reviewCount": "89K",
        "users": "4,000,000+",
        "badge": "Featured",
        "shortDescription": "Take control of YouTube: mouse wheel volume control, playback speed booster, custom filters, and cinema mode.",
        "description": "Built for YouTube lovers, Enhancer for YouTube provides extensive customizations:\n\n• Mouse wheel volume & speed adjustments\n• Video filters: brightness, contrast, hue, and sepia\n• Cinema mode and pop-up video player window\n• Auto-skip video annotations and choose default video quality.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#CC0000\"/><path d=\"M33.5 17.5C34.5 18.5 35 20 35 24C35 28 34.5 29.5 33.5 30.5C32.5 31.5 31 32 24 32C17 32 15.5 31.5 14.5 30.5C13.5 29.5 13 28 13 24C13 20 13.5 18.5 14.5 17.5C15.5 16.5 17 16 24 16C31 16 32.5 16.5 33.5 17.5Z\" fill=\"#FFFFFF\"/><polygon points=\"22,20 28,24 22,28\" fill=\"#CC0000\"/><circle cx=\"35\" cy=\"13\" r=\"5\" fill=\"#1F2937\" stroke=\"#FFFFFF\" stroke-width=\"1.5\"/><path d=\"M35 11V15M33 13H37\" stroke=\"#FFFFFF\" stroke-width=\"1.5\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "*://*.youtube.com/*"
        ],
        "size": "850 KB",
        "updated": "July 2026"
    },
    {
        "id": "mnjggcdmjocbbbhaepdhchncahnbgone",
        "name": "SponsorBlock for YouTube",
        "developer": "sponsor.ajay.app",
        "version": "5.6.8",
        "category": "media",
        "categoryLabel": "Entertainment",
        "rating": 4.9,
        "reviewCount": "52K",
        "users": "2,000,000+",
        "badge": "Featured",
        "shortDescription": "Skip sponsor segments, intros, outros, and subscribe reminders in YouTube videos automatically.",
        "description": "SponsorBlock is a crowd-sourced extension that lets anyone submit start and end times of sponsored segments in YouTube videos.\n\n• Saves hours of watching repetitive sponsorships\n• Automatically skips or lets you manually confirm\n• Color-coded progress bar showing sponsored sections\n• Fully configurable skip categories.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#D97706\"/><circle cx=\"24\" cy=\"24\" r=\"15\" fill=\"#B45309\"/><polygon points=\"20,16 32,24 20,32\" fill=\"#FFFFFF\"/><line x1=\"16\" y1=\"14\" x2=\"16\" y2=\"34\" stroke=\"#F59E0B\" stroke-width=\"2.5\" stroke-linecap=\"round\"/><line x1=\"34\" y1=\"14\" x2=\"34\" y2=\"34\" stroke=\"#F59E0B\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "*://*.youtube.com/*"
        ],
        "size": "980 KB",
        "updated": "September 2026"
    },
    {
        "id": "jghecgabfgfdldnmbfkhmffcabddioke",
        "name": "Volume Master - 600% Sound Booster",
        "developer": "peta-sitte.com",
        "version": "1.3.9",
        "category": "media",
        "categoryLabel": "Entertainment",
        "rating": 4.7,
        "reviewCount": "64K",
        "users": "8,000,000+",
        "badge": "Popular",
        "shortDescription": "Volume booster up to 600%. Individual volume control for every open tab with bass boost and stereo equalizer.",
        "description": "Volume Master gives you total volume amplification up to 600% per tab!\n\n• Individual volume sliders for any tab playing sound\n• Granular volume scale: 0% to 600%\n• One-click jump to any audio-playing tab.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#7C3AED\"/><path d=\"M14 20H18L24 14V34L18 28H14V20Z\" fill=\"#FFFFFF\"/><path d=\"M28 18C30 20 30 28 28 30\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/><path d=\"M32 14C36 17 36 31 32 34\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "tabCapture",
            "tabs",
            "storage"
        ],
        "size": "310 KB",
        "updated": "July 2026"
    },
    {
        "id": "gebbhagfogifgggkldgodflihgfeippi",
        "name": "Return YouTube Dislike",
        "developer": "returnyoutubedislike.com",
        "version": "3.0.0",
        "category": "media",
        "categoryLabel": "Entertainment",
        "rating": 4.8,
        "reviewCount": "58K",
        "users": "4,000,000+",
        "badge": "Popular",
        "shortDescription": "Brings back the dislike counter on YouTube videos using crowd-sourced user ratings and archived data.",
        "description": "Return YouTube Dislike restores the ability to view dislikes on YouTube videos using aggregated historical data and active user submissions.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#18181B\"/><path d=\"M15 28V16H19V28H15ZM31 16H23L24 12C24 10.5 22.5 10 21 11L19 16V28H28C29 28 30 27.5 30.5 26.5L33 21C33.5 20 33 19 32 19H31V16Z\" fill=\"#EF4444\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "*://*.youtube.com/*"
        ],
        "size": "420 KB",
        "updated": "August 2026"
    },
    {
        "id": "hkgfoiooedgoejojocmhlaklaeopbecg",
        "name": "Picture-in-Picture Extension (by Google)",
        "developer": "google.com",
        "version": "2.2.0",
        "category": "Productivity",
        "categoryLabel": "Entertainment",
        "rating": 4.2,
        "reviewCount": "14K",
        "users": "2,000,000+",
        "badge": "Essential",
        "shortDescription": "Watch video content in a floating window (always on top of other windows) while multitasking across web pages.",
        "description": "Watch HTML5 video in a floating, always-on-top window while you multitask, browse other tabs, or work across different desktop apps.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#4B5563\"/><rect x=\"10\" y=\"12\" width=\"28\" height=\"24\" rx=\"3\" stroke=\"#FFFFFF\" stroke-width=\"2.5\"/><rect x=\"24\" y=\"22\" width=\"12\" height=\"10\" rx=\"2\" fill=\"#3B82F6\"/></svg>",
        "featured": false,
        "permissions": [
            "activeTab"
        ],
        "size": "180 KB",
        "updated": "May 2026",
        "reviews": "1,750+",
        "author": "Google LLC",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#37474F'/><rect x='10' y='12' width='28' height='24' rx='3' fill='none' stroke='#ffffff' stroke-width='2.5'/><rect x='24' y='22' width='12' height='10' rx='2' fill='#4285F4'/></svg>",
        "features": [
            "One-click floating window for HTML5 videos",
            "Stays on top of all other windows and applications",
            "Play, pause, resize, and position anywhere on screen",
            "Lightweight extension built by Google"
        ]
    },
    {
        "id": "khncfooichmfjbepaaaebmommgaepoid",
        "name": "Unhook - Remove YouTube Recommended Videos",
        "developer": "unhook.app",
        "version": "1.6.4",
        "category": "media",
        "categoryLabel": "Entertainment",
        "rating": 4.9,
        "reviewCount": "25K",
        "users": "1,000,000+",
        "badge": "Featured",
        "shortDescription": "Hide YouTube homepage feed, recommendations sidebar, comments, and end screens to reclaim your focus.",
        "description": "Unhook removes distracting YouTube feeds and sidebars so you only watch what you came for without falling into rabbit holes.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0F172A\"/><circle cx=\"24\" cy=\"24\" r=\"14\" stroke=\"#64748B\" stroke-width=\"2\"/><line x1=\"16\" y1=\"16\" x2=\"32\" y2=\"32\" stroke=\"#EF4444\" stroke-width=\"2.5\"/><polygon points=\"21,18 28,24 21,30\" fill=\"#64748B\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "*://*.youtube.com/*"
        ],
        "size": "260 KB",
        "updated": "June 2026"
    },
    {
        "id": "mmnbenehknklpbendgmgngeaignppnbe",
        "name": "Teleparty (Netflix Party)",
        "developer": "teleparty.com",
        "version": "4.2.1",
        "category": "media",
        "categoryLabel": "Entertainment",
        "rating": 4.5,
        "reviewCount": "38K",
        "users": "10,000,000+",
        "badge": "Popular",
        "shortDescription": "Watch Netflix, YouTube, Disney+, and HBO together with friends in synchronized video playback with live chat.",
        "description": "Teleparty synchronizes video playback and adds group chat to your favorite streaming services.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#E11D48\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#BE123C\"/><text x=\"24\" y=\"30\" fill=\"#FFFFFF\" font-size=\"16\" font-weight=\"bold\" font-family=\"Arial\" text-anchor=\"middle\">TP</text></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "storage",
            "<all_urls>"
        ],
        "size": "3.4 MB",
        "updated": "July 2026"
    },
    {
        "id": "kbfnbcaeplbcioakkpcpgfkobkghlhen",
        "name": "Grammarly: AI Writing Assistant",
        "developer": "grammarly.com",
        "version": "14.1118.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "148K",
        "users": "10,000,000+",
        "badge": "Popular",
        "shortDescription": "Write with confidence. Real-time grammar, spelling, and tone checks across all your browser tabs and emails.",
        "description": "Grammarly helps you communicate clearly and effectively across Gmail, Google Docs, social media, and web forms.\n\n• Comprehensive spell check and grammar verification\n• Tone detector: friendly, confident, direct, and academic\n• Instant generative AI sentence rephrasing\n• Vocabulary enhancement suggestions.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#15C39A\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><path d=\"M28.5 18C26 15.5 21 16 18.5 18.5C16 21 16 26.5 18.5 29.5C21 32 26.5 32 29 29.5C30.5 28 31 26 31 24H24\" stroke=\"#15C39A\" stroke-width=\"3.2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "contextMenus",
            "<all_urls>"
        ],
        "size": "22.4 MB",
        "updated": "August 2026"
    },
    {
        "id": "knheggckgoiihginacbkhaalnibhilkk",
        "name": "Notion Web Clipper",
        "developer": "notion.so",
        "version": "1.0.9",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.4,
        "reviewCount": "8.9K",
        "users": "4,000,000+",
        "badge": "Featured",
        "shortDescription": "Save any website, article, or image directly into your Notion workspaces with one single click.",
        "description": "Capture the web directly into Notion. Save pages as clean database items, add tags, and read distraction-free.\n\n• 1-Click capture of articles, text, and images\n• Choose target workspace and database instantly\n• Synchronized across all Notion apps.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#000000\"/><path d=\"M16 14L28 14L32 18V34H26L20 22V34H16V14Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "storage",
            "<all_urls>"
        ],
        "size": "950 KB",
        "updated": "June 2026"
    },
    {
        "id": "oldceeleldhonbafppcapldpdifcinji",
        "name": "LanguageTool - Grammar Checker",
        "developer": "languagetool.org",
        "version": "8.1.1",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.8,
        "reviewCount": "21K",
        "users": "3,000,000+",
        "badge": "Verified",
        "shortDescription": "Multilingual grammar, style, and spell checker supporting English, German, French, Spanish, and 25+ other languages.",
        "description": "LanguageTool checks your text across websites with advanced AI language models, ensuring error-free writing in more than 30 languages.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><path d=\"M14 30L20 16H24L30 30H26L24.5 26H19.5L18 30H14ZM20.5 23H23.5L22 19L20.5 23Z\" fill=\"#FFFFFF\"/><circle cx=\"33\" cy=\"28\" r=\"4\" fill=\"#F59E0B\"/><path d=\"M31.5 28L32.5 29.5L35 26.5\" stroke=\"#FFFFFF\" stroke-width=\"1.2\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "3.1 MB",
        "updated": "July 2026"
    },
    {
        "id": "jldhpllghnbhlbpcmnajkpdmadaolakh",
        "name": "Todoist for Chrome",
        "developer": "todoist.com",
        "version": "10.4.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.7,
        "reviewCount": "51K",
        "users": "800,000+",
        "badge": "Popular",
        "shortDescription": "Organize work and life. Plan your day, add articles to your reading list, and check off tasks directly from your toolbar.",
        "description": "Todoist lets you organize your day, add websites as tasks, and keep tabs on projects wherever you browse.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#E44232\"/><path d=\"M16 24L22 30L34 18\" stroke=\"#FFFFFF\" stroke-width=\"4\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "contextMenus",
            "storage"
        ],
        "size": "2.1 MB",
        "updated": "September 2026"
    },
    {
        "id": "pioclpoplcdbaefihamjohnefbikjilc",
        "name": "Evernote Web Clipper",
        "developer": "evernote.com",
        "version": "7.35.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.5,
        "reviewCount": "135K",
        "users": "4,000,000+",
        "badge": "Popular",
        "shortDescription": "Clip web pages, articles, and PDFs directly to Evernote. Highlight key text and add personal notes.",
        "description": "Use Evernote Web Clipper to save interesting things you find across the web directly into your Evernote account.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#00A82D\"/><path d=\"M16 16C16 14 18 12 21 12H30C32 12 34 14 34 17V30C34 33 32 35 30 35H21C18 35 16 33 16 30V16Z\" fill=\"#FFFFFF\"/><path d=\"M20 20H30M20 24H30M20 28H26\" stroke=\"#00A82D\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "storage",
            "<all_urls>"
        ],
        "size": "4.8 MB",
        "updated": "May 2026"
    },
    {
        "id": "niloccemoadcdkdjlinkgdfekeahmflj",
        "name": "Save to Pocket",
        "developer": "getpocket.com",
        "version": "3.0.9",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "74K",
        "users": "2,000,000+",
        "badge": "Popular",
        "shortDescription": "The easiest, fastest way to capture articles, videos, and anything else you’d like to read later offline.",
        "description": "When you find something you want to view later, put it in Pocket. It automatically syncs to your phone, tablet, and computer.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#EF4056\"/><path d=\"M14 16H34V25C34 30.5 29.5 35 24 35C18.5 35 14 30.5 14 25V16Z\" fill=\"#FFFFFF\"/><path d=\"M19 23L24 28L29 23\" stroke=\"#EF4056\" stroke-width=\"3\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "storage",
            "<all_urls>"
        ],
        "size": "1.2 MB",
        "updated": "April 2026"
    },
    {
        "id": "chphlpgkkbolifaimnlloiipkdnihall",
        "name": "OneTab",
        "developer": "one-tab.com",
        "version": "1.84.0",
        "category": "utilities",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "41K",
        "users": "3,000,000+",
        "badge": "Editor's Choice",
        "shortDescription": "Save up to 95% memory and eliminate tab clutter by converting open tabs into a clean list.",
        "description": "Whenever you find yourself with too many tabs, click the OneTab icon to convert all of your tabs into a list. When you need to access the tabs again, you can restore them individually or all at once.\n\n• Frees up to 95% of active memory\n• Export and import tab sessions easily\n• Organizes cluttered windows into neat project groups.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><path d=\"M14 16H34M16 22H32M18 28H30M20 34H28\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "tabs",
            "storage"
        ],
        "size": "420 KB",
        "updated": "May 2026"
    },
    {
        "id": "ldgfbffkinooeloadekpmfoklnobpien",
        "name": "Raindrop.io - Smart Bookmarks",
        "developer": "raindrop.io",
        "version": "5.6.25",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.8,
        "reviewCount": "9.4K",
        "users": "400,000+",
        "badge": "Editor's Choice",
        "shortDescription": "All-in-one bookmark manager with tags, nested collections, full-text search, and visual cloud library.",
        "description": "Raindrop.io is a modern bookmarking app with beautiful visual boards, tag management, duplicate finder, and cloud backup.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#3B82F6\"/><path d=\"M24 10C24 10 14 24 14 29C14 34.5 18.5 39 24 39C29.5 39 34 34.5 34 29C34 24 24 10 24 10Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "storage",
            "<all_urls>"
        ],
        "size": "1.9 MB",
        "updated": "August 2026"
    },
    {
        "id": "pmjeegjhjdlccodhacdgbgfagbpmipfd",
        "name": "Clockify Time Tracker",
        "developer": "clockify.me",
        "version": "2.12.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.7,
        "reviewCount": "8.5K",
        "users": "600,000+",
        "badge": "Verified",
        "shortDescription": "Track work hours across web apps with a single click. Integrates with Trello, Asana, Jira, and GitHub.",
        "description": "Track time from anywhere on the web and analyze your productivity across projects.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><circle cx=\"24\" cy=\"24\" r=\"14\" stroke=\"#FFFFFF\" stroke-width=\"2.5\"/><polyline points=\"24,16 24,24 29,27\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "idle",
            "<all_urls>"
        ],
        "size": "3.2 MB",
        "updated": "July 2026"
    },
    {
        "id": "oejgcckgbcimfphegmdbdfobikigmgnj",
        "name": "Toggl Track: Time Tracker",
        "developer": "toggl.com",
        "version": "3.1.2",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "6.8K",
        "users": "400,000+",
        "badge": "Verified",
        "shortDescription": "Effortless time tracking and Pomodoro timer. Start tracking inside 100+ popular web tools.",
        "description": "Toggl Track browser extension adds a 1-click timer into web tools like Notion, Asana, Trello, Google Docs and GitHub.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#E57CD8\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#2C1338\"/><polygon points=\"21,18 29,24 21,30\" fill=\"#E57CD8\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "idle",
            "<all_urls>"
        ],
        "size": "4.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "nllcnknpjnininkchngahlicfpipnhmp",
        "name": "Wordtune - AI Writing Assistant",
        "developer": "wordtune.com",
        "version": "3.4.1",
        "category": "ai",
        "categoryLabel": "AI & Smart Tools",
        "rating": 4.6,
        "reviewCount": "13K",
        "users": "2,000,000+",
        "badge": "Popular",
        "shortDescription": "AI writing assistant that rephrases, shortens, expands, and changes the tone of your sentences instantly.",
        "description": "Say exactly what you mean with clear, compelling, and authentic writing powered by cutting-edge language models.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#7C3AED\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><path d=\"M16 18L21 30L24 22L27 30L32 18\" stroke=\"#7C3AED\" stroke-width=\"3\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "6.5 MB",
        "updated": "July 2026"
    },
    {
        "id": "fmkadmapgofadopljbjfkapdkoienihi",
        "name": "React Developer Tools",
        "developer": "react.dev",
        "version": "5.3.1",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.5,
        "reviewCount": "19K",
        "users": "4,000,000+",
        "badge": "Official",
        "shortDescription": "Inspect React component hierarchy, props, state, hooks, and performance profiler inside RUHI Browser DevTools.",
        "description": "React Developer Tools allows you to inspect React component trees, including props, state, and context values directly in DevTools.\n\n• Inspect React hierarchy and tree nodes\n• Real-time component profiler to detect slow re-renders\n• Inspect component hooks and state changes.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#20232A\"/><circle cx=\"24\" cy=\"24\" r=\"3.5\" fill=\"#61DAFB\"/><ellipse cx=\"24\" cy=\"24\" rx=\"14\" ry=\"5.5\" stroke=\"#61DAFB\" stroke-width=\"1.8\" transform=\"rotate(30 24 24)\"/><ellipse cx=\"24\" cy=\"24\" rx=\"14\" ry=\"5.5\" stroke=\"#61DAFB\" stroke-width=\"1.8\" transform=\"rotate(90 24 24)\"/><ellipse cx=\"24\" cy=\"24\" rx=\"14\" ry=\"5.5\" stroke=\"#61DAFB\" stroke-width=\"1.8\" transform=\"rotate(150 24 24)\"/></svg>",
        "featured": false,
        "permissions": [
            "<all_urls>"
        ],
        "size": "3.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "gbmdgpbipfallnflgajpaliibnhdgobh",
        "name": "JSON Viewer",
        "developer": "tulios.github.io",
        "version": "1.0.6",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.8,
        "reviewCount": "18K",
        "users": "1,000,000+",
        "badge": "Verified",
        "shortDescription": "The definitive tool to format and view JSON in the browser with collapsible nodes, syntax themes, and clickable URLs.",
        "description": "JSON Viewer renders raw JSON responses in a visually pleasing, human-readable format directly inside RUHI Browser tabs.\n\n• Syntax highlighting with dark and light themes\n• Interactive collapsible and expandable tree nodes\n• Scratchpad tab for pasting and testing JSON payloads.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0F172A\"/><rect x=\"4\" y=\"4\" width=\"40\" height=\"40\" rx=\"8\" stroke=\"#F59E0B\" stroke-width=\"1.5\"/><text x=\"24\" y=\"30\" fill=\"#F59E0B\" font-size=\"20\" font-weight=\"bold\" font-family=\"Courier New, monospace\" text-anchor=\"middle\">{;}</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "720 KB",
        "updated": "April 2026"
    },
    {
        "id": "lmhkpmbekcpmknklioeibfkpmmfibljd",
        "name": "Redux DevTools",
        "developer": "redux.js.org",
        "version": "3.1.6",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.8,
        "reviewCount": "11K",
        "users": "2,000,000+",
        "badge": "Official",
        "shortDescription": "Redux action logger, state inspector, and time-travel debugger for modern JavaScript web applications.",
        "description": "Inspect every state change in your Redux application, cancel actions, and replay them with time-travel debugging.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#764ABC\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"18\" r=\"3\" fill=\"#764ABC\"/><circle cx=\"18\" cy=\"28\" r=\"3\" fill=\"#764ABC\"/><circle cx=\"30\" cy=\"28\" r=\"3\" fill=\"#764ABC\"/></svg>",
        "featured": false,
        "permissions": [
            "<all_urls>"
        ],
        "size": "2.8 MB",
        "updated": "May 2026"
    },
    {
        "id": "nhdogjmejiglipccpnnnanhbledajbpd",
        "name": "Vue.js devtools",
        "developer": "vuejs.org",
        "version": "6.6.3",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "8.5K",
        "users": "2,000,000+",
        "badge": "Official",
        "shortDescription": "Official Chrome DevTools extension for debugging Vue.js 2 and Vue.js 3 applications and Pinia stores.",
        "description": "Explore Vue component trees, inspect reactive data, edit state on-the-fly, and track emitted component events.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#35495E\"/><polygon points=\"24,36 38,12 32,12 24,26 16,12 10,12\" fill=\"#42B883\"/><polygon points=\"24,28 31,16 27,16 24,21 21,16 17,16\" fill=\"#35495E\"/></svg>",
        "featured": false,
        "permissions": [
            "<all_urls>"
        ],
        "size": "3.9 MB",
        "updated": "June 2026"
    },
    {
        "id": "bkhaagajahlaobaripdfhedjdpmbhfen",
        "name": "Octotree - GitHub code tree",
        "developer": "octotree.com",
        "version": "10.1.2",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.8,
        "reviewCount": "16K",
        "users": "600,000+",
        "badge": "Featured",
        "shortDescription": "Fast IDE-like folder tree navigation sidebar for exploring GitHub repositories, pull requests, and files.",
        "description": "Navigate GitHub codebases just like in VS Code with a fast collapsible file tree sidebar.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#181717\"/><path d=\"M16 16H22V22H16V16ZM26 16H32V22H26V16ZM16 26H22V32H16V26ZM26 26H32V32H26V26Z\" fill=\"#10B981\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "*://github.com/*"
        ],
        "size": "1.7 MB",
        "updated": "July 2026"
    },
    {
        "id": "bhlhndldebchnkkcfljhfdcmkodflmpi",
        "name": "ColorZilla",
        "developer": "colorzilla.com",
        "version": "4.0.1",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.7,
        "reviewCount": "25K",
        "users": "3,000,000+",
        "badge": "Popular",
        "shortDescription": "Advanced eyedropper, color picker, gradient generator, and page color analyzer for web designers.",
        "description": "Sample colors from any webpage element and copy HEX, RGB, or HSL color codes with 1 click.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#4F46E5\"/><path d=\"M28 12L34 18L30 22L24 16L28 12ZM22 18L14 26V32H20L28 24L22 18Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "storage",
            "<all_urls>"
        ],
        "size": "820 KB",
        "updated": "March 2026"
    },
    {
        "id": "gppongmhjkpfnbhagpmjfkannfbllamg",
        "name": "Wappalyzer - Technology profiler",
        "developer": "wappalyzer.com",
        "version": "6.10.74",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "19K",
        "users": "2,000,000+",
        "badge": "Popular",
        "shortDescription": "Identify technologies used on websites: CMS, frameworks, eCommerce engines, web servers, and analytics.",
        "description": "Find out what software powers any website: WordPress, Shopify, React, Node.js, Cloudflare, and 2000+ more.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#2E1A47\"/><polygon points=\"14,16 24,34 34,16 28,16 24,24 20,16\" fill=\"#00D2D3\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "webNavigation",
            "<all_urls>"
        ],
        "size": "2.4 MB",
        "updated": "September 2026"
    },
    {
        "id": "aicmkgpgakddgnaphhhpliifpcfhicfo",
        "name": "Postman Interceptor",
        "developer": "postman.com",
        "version": "1.1.2",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.4,
        "reviewCount": "6.2K",
        "users": "1,000,000+",
        "badge": "Official",
        "shortDescription": "Capture browser network traffic and sync cookies directly with the Postman API testing client.",
        "description": "Send requests through the browser to intercept cookies, authentication sessions, and API payloads.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#FF6C37\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"24\" r=\"8\" fill=\"#FF6C37\"/><circle cx=\"24\" cy=\"24\" r=\"3\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "tabs",
            "cookies",
            "<all_urls>"
        ],
        "size": "890 KB",
        "updated": "June 2026"
    },
    {
        "id": "mbnbehikldjhnfehhnaidhjhoofhpehk",
        "name": "CSS Peeper",
        "developer": "csspeeper.com",
        "version": "0.5.8",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.8,
        "reviewCount": "7.1K",
        "users": "500,000+",
        "badge": "Featured",
        "shortDescription": "Smart CSS viewer tailored for designers. Inspect styles, colors, fonts, and assets in clean visual cards.",
        "description": "Extract colors, inspect line-height, fonts, and download website image assets without digging into complex DevTools.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0F172A\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#6366F1\"/><circle cx=\"24\" cy=\"24\" r=\"6\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "activeTab"
        ],
        "size": "1.1 MB",
        "updated": "May 2026"
    },
    {
        "id": "dhdgffkkebhmkfjojejmpbldmpobfkfo",
        "name": "Tampermonkey",
        "developer": "tampermonkey.net",
        "version": "5.3.3",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.7,
        "reviewCount": "128K",
        "users": "10,000,000+",
        "badge": "Popular",
        "shortDescription": "The world's most popular userscript manager. Customize web pages with millions of community scripts.",
        "description": "Tampermonkey allows you to run user scripts that customize website functionality, remove clutter, and add new capabilities.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1C1917\"/><circle cx=\"18\" cy=\"22\" r=\"5\" fill=\"#FFFFFF\"/><circle cx=\"30\" cy=\"22\" r=\"5\" fill=\"#FFFFFF\"/><circle cx=\"18\" cy=\"22\" r=\"2.5\" fill=\"#000000\"/><circle cx=\"30\" cy=\"22\" r=\"2.5\" fill=\"#000000\"/><path d=\"M19 32C22 35 26 35 29 32\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "2.6 MB",
        "updated": "August 2026"
    },
    {
        "id": "hlgbcneanomplepojfcnclggenpcoldo",
        "name": "Perplexity - AI Companion & Search",
        "developer": "perplexity.ai",
        "version": "2.1.4",
        "category": "ai",
        "categoryLabel": "AI & Smart Tools",
        "rating": 4.8,
        "reviewCount": "14K",
        "users": "1,000,000+",
        "badge": "Editor's Choice",
        "shortDescription": "AI companion for instant search answers, web page summaries, research synthesis, and inline question asking.",
        "description": "Perplexity is your AI assistant right inside the browser. Get instant answers with cited sources, summarize lengthy articles, and query the web effortlessly.\n\n• Instant page summaries in seconds\n• Real-time citations and source verification\n• Query current tab content effortlessly.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0F172A\"/><path d=\"M24 10V38M10 24H38M14 14L34 34M34 14L14 34\" stroke=\"#2DD4BF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "activeTab",
            "storage",
            "<all_urls>"
        ],
        "size": "2.4 MB",
        "updated": "September 2026"
    },
    {
        "id": "jgjaeacdkonaoafenlfkkkmbaopkbilf",
        "name": "ChatGPT for Google",
        "developer": "chatgpt4google.com",
        "version": "2.7.2",
        "category": "ai",
        "categoryLabel": "AI & Smart Tools",
        "rating": 4.7,
        "reviewCount": "32K",
        "users": "2,000,000+",
        "badge": "Popular",
        "shortDescription": "Display ChatGPT AI answers right alongside Google, Bing, and DuckDuckGo search engine results.",
        "description": "Supercharge your search engines: see intelligent AI responses side-by-side with your standard web search results.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#10A37F\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"24\" r=\"7\" stroke=\"#10A37F\" stroke-width=\"2.5\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "3.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "difoiogjjojoaoomphldepapgpbgkhkb",
        "name": "Sider - ChatGPT Sidebar + Vision",
        "developer": "sider.ai",
        "version": "4.8.0",
        "category": "ai",
        "categoryLabel": "AI & Smart Tools",
        "rating": 4.9,
        "reviewCount": "68K",
        "users": "4,000,000+",
        "badge": "Editor's Choice",
        "shortDescription": "All-in-one AI sidebar: ChatGPT, Claude 3.5, Gemini 1.5, image generation, and PDF document chat.",
        "description": "Your all-in-one AI assistant sidebar with support for Claude 3.5 Sonnet, GPT-4o, and Gemini Pro.\n\n• Chat with any web page, PDF, or YouTube video\n• Instant grammar, translation, and rewrite tools\n• Image creation and OCR text extraction.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#6366F1\"/><path d=\"M14 16C14 13.8 15.8 12 18 12H30C32.2 12 34 13.8 34 16V32C34 34.2 32.2 36 30 36H18C15.8 36 14 34.2 14 32V16Z\" fill=\"#4338CA\"/><circle cx=\"24\" cy=\"24\" r=\"5\" fill=\"#FFFFFF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "activeTab",
            "<all_urls>"
        ],
        "size": "8.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "eanggfilgoajaocelnaflolkadkeghjp",
        "name": "HARPA AI - Web Automation Agent",
        "developer": "harpa.ai",
        "version": "7.14.0",
        "category": "ai",
        "categoryLabel": "AI & Smart Tools",
        "rating": 4.9,
        "reviewCount": "29K",
        "users": "700,000+",
        "badge": "Featured",
        "shortDescription": "Hybrid AI engine & web automation agent: price drop tracking, competitor monitoring, web scraping, and copy creation.",
        "description": "HARPA AI connects hybrid AI models to web automation. Track prices, monitor webpage changes, summarize videos, and generate replies.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#09090B\"/><polygon points=\"24,10 38,36 10,36\" stroke=\"#EC4899\" stroke-width=\"2.5\" fill=\"none\"/><circle cx=\"24\" cy=\"24\" r=\"4\" fill=\"#EC4899\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "alarms",
            "<all_urls>"
        ],
        "size": "4.5 MB",
        "updated": "August 2026"
    },
    {
        "id": "mhnlakgilnojmhinhkckjpncpbhabphi",
        "name": "MaxAI.me - 1-Click AI Everywhere",
        "developer": "maxai.me",
        "version": "4.5.1",
        "category": "ai",
        "categoryLabel": "AI & Smart Tools",
        "rating": 4.8,
        "reviewCount": "44K",
        "users": "1,000,000+",
        "badge": "Popular",
        "shortDescription": "Use ChatGPT, Claude, and Gemini on any web page with 1 click. Write emails, summarize long articles, and brainstorm.",
        "description": "MaxAI.me brings multi-model AI capabilities to every input field, email client, and document editor online.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#8B5CF6\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#6D28D9\"/><path d=\"M18 20L24 26L30 20\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "5.4 MB",
        "updated": "September 2026"
    },
    {
        "id": "eimadpbcbfnmbkopoojfekhnkhdbieeh",
        "name": "Dark Reader",
        "developer": "darkreader.org",
        "version": "4.9.96",
        "category": "themes",
        "categoryLabel": "Appearance",
        "rating": 4.8,
        "reviewCount": "194K",
        "users": "5,000,000+",
        "badge": "Editor's Choice",
        "shortDescription": "Dark mode for every website. Care for your eyes, use dark theme for night and daily web browsing.",
        "description": "Dark Reader inverts bright colors on websites, making them high contrast and easy to read at night.\n\n• Real-time color analysis that doesn't break website layouts\n• Adjust brightness, contrast, sepia filter, and font settings\n• Whitelist and blacklist your favorite sites\n• Fully open source and privacy respecting.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1E1B4B\"/><circle cx=\"24\" cy=\"24\" r=\"16\" fill=\"#4338CA\"/><circle cx=\"19\" cy=\"22\" r=\"5\" fill=\"#111827\" stroke=\"#E0E7FF\" stroke-width=\"1.8\"/><circle cx=\"29\" cy=\"22\" r=\"5\" fill=\"#111827\" stroke=\"#E0E7FF\" stroke-width=\"1.8\"/><path d=\"M24 22H25\" stroke=\"#E0E7FF\" stroke-width=\"2\" stroke-linecap=\"round\"/><path d=\"M17 29C19 32 29 32 31 29\" stroke=\"#F59E0B\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "1.2 MB",
        "updated": "August 2026",
        "zipUrl": "https://github.com/darkreader/darkreader/releases/download/v4.9.96/darkreader-chrome.zip"
    },
    {
        "id": "aapbdbdomjkkjkaonfhkkikfgjllcleb",
        "name": "Google Translate",
        "developer": "google.com",
        "version": "2.0.14",
        "category": "utilities",
        "categoryLabel": "Utilities",
        "rating": 4.5,
        "reviewCount": "43K",
        "users": "10,000,000+",
        "badge": "Official",
        "shortDescription": "Highlight or right-click text to translate words or whole web pages instantly into 100+ languages.",
        "description": "Translate selected text, input fields, or entire web pages on the fly with Google Translate's official extension.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#4285F4\"/><path d=\"M16 20H32M24 16V20M20 20C20 25 17 29 13 31M22 25C20 28 17 30 14 31M26 31L31 21L36 31M28 28H34\" stroke=\"#FFFFFF\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "contextMenus",
            "<all_urls>"
        ],
        "size": "620 KB",
        "updated": "June 2026"
    },
    {
        "id": "pgjjikdiikihdfapbhapapcmnakmgdfa",
        "name": "Speedtest by Ookla",
        "developer": "ookla.com",
        "version": "1.1.18",
        "category": "utilities",
        "categoryLabel": "Utilities",
        "rating": 4.6,
        "reviewCount": "26K",
        "users": "3,000,000+",
        "badge": "Verified",
        "shortDescription": "Take a Speedtest directly from your toolbar to quickly test your internet ping, download, and upload speeds.",
        "description": "Test ping, download, and upload speeds in seconds directly from the browser without opening separate testing tabs.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#141B2D\"/><circle cx=\"24\" cy=\"24\" r=\"14\" stroke=\"#38BDF8\" stroke-width=\"3\" stroke-dasharray=\"60 30\"/><line x1=\"24\" y1=\"24\" x2=\"31\" y2=\"17\" stroke=\"#38BDF8\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "1.5 MB",
        "updated": "July 2026"
    },
    {
        "id": "bmnlcjabgnpnenekpadlanbbkooimhnj",
        "name": "PayPal Honey: Automatic Coupons & Cash Back",
        "developer": "PayPal Honey",
        "version": "16.8.2",
        "category": "shopping",
        "categoryLabel": "Shopping & Savings",
        "rating": 4.8,
        "reviewCount": "172K",
        "users": "10,000,000+",
        "badge": "Featured",
        "shortDescription": "Automatically search and apply coupon codes at 30,000+ online stores to find the best savings instantly.",
        "description": "Honey automatically finds and applies coupon codes at checkout with one click.\n\n• Works across 30,000+ popular shopping websites\n• Exclusive Honey Gold cashback rewards\n• Price history tracker to buy at the right time\n• Droplist alerts notify you when an item drops in price.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#E65100\"/><path d=\"M16 14H22V22H26V14H32V34H26V26H22V34H16V14Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "tabs",
            "cookies",
            "<all_urls>"
        ],
        "size": "8.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "neebplgakaahbhdphmkckjjcegoiijjo",
        "name": "Keepa - Amazon Price Tracker",
        "developer": "keepa.com",
        "version": "4.42",
        "category": "shopping",
        "categoryLabel": "Shopping & Savings",
        "rating": 4.7,
        "reviewCount": "48K",
        "users": "4,000,000+",
        "badge": "Popular",
        "shortDescription": "Comprehensive price history charts and price drop alerts for millions of products across all Amazon locales.",
        "description": "Keepa adds detailed price history charts directly to Amazon product pages.\n\n• Detailed price history graphs for Amazon and marketplace sellers\n• Price drop & availability alerts via email, telegram, and web\n• Compare prices across all international Amazon locales\n• No account required for basic tracking.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1E293B\"/><polyline points=\"10,34 18,24 26,28 38,14\" fill=\"none\" stroke=\"#38BDF8\" stroke-width=\"3.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/><circle cx=\"38\" cy=\"14\" r=\"4\" fill=\"#F59E0B\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "alarms",
            "*://*.amazon.com/*"
        ],
        "size": "2.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "ghnomdcacenbmilgjigehppbamfndblo",
        "name": "The Camelizer - Price History",
        "developer": "camelcamelcamel.com",
        "version": "3.0.15",
        "category": "shopping",
        "categoryLabel": "Shopping & Savings",
        "rating": 4.5,
        "reviewCount": "12K",
        "users": "900,000+",
        "badge": "Verified",
        "shortDescription": "Easily view historical price charts from camelcamelcamel.com right inside your browser with a single click.",
        "description": "The Camelizer lets you view historical pricing data without leaving Amazon product pages.\n\n• Displays lowest and highest historic prices\n• Set price watches and notifications\n• Works with Amazon US, UK, Germany, France, Japan, and Canada.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#B45309\"/><path d=\"M14 32C14 26 18 20 22 22C24 23 26 18 30 18C34 18 36 24 36 32\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/><circle cx=\"18\" cy=\"20\" r=\"2.5\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs"
        ],
        "size": "1.4 MB",
        "updated": "July 2026"
    },
    {
        "id": "hipbfijinpcgfogaopmgehiegacbhmob",
        "name": "Wise - Fast International Payments",
        "developer": "wise.com",
        "version": "2.8.1",
        "category": "shopping",
        "categoryLabel": "Shopping & Savings",
        "rating": 4.6,
        "reviewCount": "8.5K",
        "users": "600,000+",
        "badge": "Verified",
        "shortDescription": "Auto-fill card details and pay online in 40+ currencies at the real mid-market exchange rate without high fees.",
        "description": "Wise gives you fast, secure international payments at the real exchange rate.\n\n• Auto-fill digital debit card details safely\n• Convert money in real-time across 40+ currencies\n• Always get the mid-market exchange rate without markup.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#9FE870\"/><path d=\"M14 18L24 32L34 18H28L24 24L20 18H14Z\" fill=\"#163300\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "cookies"
        ],
        "size": "3.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "gighmmpiobklfepjocnamgkkbiglidom",
        "name": "AdBlock — best ad blocker",
        "developer": "getadblock.com",
        "version": "6.8.0",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.6,
        "reviewCount": "310K",
        "users": "10,000,000+",
        "badge": "Featured",
        "shortDescription": "Block YouTube ads, pop-ups, and annoying commercials across the web to speed up browsing and protect privacy.",
        "description": "AdBlock is the original ad blocker for modern web browsers.\n\n• Blocks pop-ups, video ads, and floating banners\n• Blocks third-party trackers to protect your personal data\n• Whitelist your favorite creators and websites with one click.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#DC2626\"/><path d=\"M24 10L12 22V34L24 38L36 34V22L24 10Z\" fill=\"#991B1B\" stroke=\"#FFFFFF\" stroke-width=\"2.5\"/><path d=\"M20 20L28 28M28 20L20 28\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "tabs"
        ],
        "size": "5.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "hnmpcagpplmpfojmgmnngilcnanddlhb",
        "name": "Windscribe - Free VPN and Ad Blocker",
        "developer": "windscribe.com",
        "version": "3.4.15",
        "category": "security",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "41K",
        "users": "3,000,000+",
        "badge": "Featured",
        "shortDescription": "Mask your IP address, bypass geographic censorship, and block malware, ads, and tracking beacons securely.",
        "description": "Windscribe combines an encrypted browser proxy with an intelligent tracking and ad firewall.\n\n• Connect to servers in over 60 countries\n• Spoof GPS location and browser timezone automatically\n• Strict zero-logging policy for total anonymity.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#102A45\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#00D2FF\"/><path d=\"M19 24L23 28L29 20\" stroke=\"#102A45\" stroke-width=\"3\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "proxy",
            "storage",
            "webRequest",
            "privacy"
        ],
        "size": "4.6 MB",
        "updated": "August 2026"
    },
    {
        "id": "omdakjcmkglenbhjadbccaookpfjihpa",
        "name": "TunnelBear VPN",
        "developer": "tunnelbear.com",
        "version": "4.2.8",
        "category": "security",
        "categoryLabel": "Privacy & Security",
        "rating": 4.6,
        "reviewCount": "22K",
        "users": "2,000,000+",
        "badge": "Verified",
        "shortDescription": "A simple, private VPN extension to browse securely on public WiFi and access content globally.",
        "description": "TunnelBear provides fast and easy VPN protection for your browser.\n\n• Connect in seconds to private networks in 45+ countries\n• Military-grade AES-256 bit encryption\n• Independently audited by security experts every year.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#B45309\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#D97706\"/><circle cx=\"19\" cy=\"21\" r=\"2\" fill=\"#1F2937\"/><circle cx=\"29\" cy=\"21\" r=\"2\" fill=\"#1F2937\"/><ellipse cx=\"24\" cy=\"26\" rx=\"4\" ry=\"3\" fill=\"#1F2937\"/></svg>",
        "featured": false,
        "permissions": [
            "proxy",
            "storage",
            "privacy"
        ],
        "size": "3.7 MB",
        "updated": "July 2026"
    },
    {
        "id": "doojmbjmlfjjnbmnoijecmcbfeoakpjm",
        "name": "NoScript - Advanced Script Blocker",
        "developer": "Giorgio Maone",
        "version": "12.0.8",
        "category": "security",
        "categoryLabel": "Privacy & Security",
        "rating": 4.5,
        "reviewCount": "14K",
        "users": "600,000+",
        "badge": "Verified",
        "shortDescription": "Maximum protection for your browser by allowing active content (JavaScript, Flash, Java) to execute only on trusted domains.",
        "description": "The NoScript security suite provides whitelist-based script execution protection for power users.\n\n• Blocks XSS attacks, clickjacking, and cross-site scripting\n• Granular per-domain JavaScript, WebGL, and Media controls\n• Light on memory and system resources.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1E1E2E\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#EF4444\" stroke-width=\"3\" fill=\"none\"/><line x1=\"15\" y1=\"15\" x2=\"33\" y2=\"33\" stroke=\"#EF4444\" stroke-width=\"3\"/><text x=\"24\" y=\"28\" fill=\"#FFFFFF\" font-size=\"13\" font-weight=\"bold\" font-family=\"Arial, sans-serif\" text-anchor=\"middle\">JS</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking"
        ],
        "size": "1.9 MB",
        "updated": "September 2026"
    },
    {
        "id": "pnlccmojcmeohlpggmfnbbiapkmbliob",
        "name": "RoboForm Password Manager",
        "developer": "roboform.com",
        "version": "9.5.8",
        "category": "security",
        "categoryLabel": "Privacy & Security",
        "rating": 4.6,
        "reviewCount": "16K",
        "users": "1,000,000+",
        "badge": "Verified",
        "shortDescription": "Award-winning password manager and form filler. Remember fewer passwords and log into sites with one click.",
        "description": "RoboForm makes logging into websites and filling out long online forms effortless.\n\n• Instant one-click logins across desktop and web\n• Enterprise-grade AES-256 encryption with PBKDF2 SHA-256\n• Cross-platform syncing across all your devices.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#38BDF8\"/><rect x=\"18\" y=\"18\" width=\"12\" height=\"12\" rx=\"3\" fill=\"#0C4A6E\"/><circle cx=\"24\" cy=\"24\" r=\"2.5\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "cookies",
            "autofill"
        ],
        "size": "6.2 MB",
        "updated": "August 2026"
    },
    {
        "id": "bfogiafebfohielmmehodmfbbebbbpei",
        "name": "Keeper Password Manager & Digital Vault",
        "developer": "Keeper Security",
        "version": "16.11.2",
        "category": "security",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "18K",
        "users": "1,000,000+",
        "badge": "Featured",
        "shortDescription": "Zero-knowledge security architecture password vault and multi-factor authenticator for personal and enterprise use.",
        "description": "Keeper protects passwords, passkeys, and confidential files with zero-knowledge encryption.\n\n• Auto-fills usernames, passwords, passkeys, and payment cards\n• Built-in two-factor authenticator (2FA)\n• Secure record sharing with emergency access options.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0F172A\"/><path d=\"M24 11L14 15V24C14 30.5 18.5 36.5 24 38C29.5 36.5 34 30.5 34 24V15L24 11Z\" fill=\"#F59E0B\"/><circle cx=\"24\" cy=\"23\" r=\"3\" fill=\"#0F172A\"/><path d=\"M24 26V29\" stroke=\"#0F172A\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "contextMenus"
        ],
        "size": "7.1 MB",
        "updated": "September 2026"
    },
    {
        "id": "bhghoamapcdpbohphigoooaddinpkbai",
        "name": "Authenticator - 2FA Code Generator",
        "developer": "authenticator.cc",
        "version": "7.1.4",
        "category": "security",
        "categoryLabel": "Privacy & Security",
        "rating": 4.8,
        "reviewCount": "25K",
        "users": "3,000,000+",
        "badge": "Featured",
        "shortDescription": "Generate 2-step verification (2FA) security codes directly in your browser. Encrypted backup and sync support.",
        "description": "Authenticator provides fast, secure two-factor authentication tokens in the browser.\n\n• Scan QR codes with one click to add accounts\n• Password-protected client-side encryption\n• Sync across Google Drive, Dropbox, or OneDrive seamlessly.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#2563EB\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><path d=\"M24 16V24L29 27\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/><circle cx=\"24\" cy=\"24\" r=\"3\" fill=\"#60A5FA\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "alarms",
            "contextMenus"
        ],
        "size": "2.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "liecbddmkiiihnedobmlmillhodjkdmb",
        "name": "Loom – Screen Recorder & Screen Capture",
        "developer": "loom.com",
        "version": "5.58.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.7,
        "reviewCount": "42K",
        "users": "6,000,000+",
        "badge": "Featured",
        "shortDescription": "Record your camera, microphone, and screen simultaneously. Share instant video links with zero upload waiting.",
        "description": "Loom is the easiest way to record asynchronous video messages and screen walkthroughs.\n\n• Record screen, camera, or both with audio narration\n• Instant sharing link generated as soon as you hit stop\n• Auto-generated transcripts and AI-powered video summaries.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#625DF5\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"24\" r=\"7\" fill=\"#FF5C5C\"/></svg>",
        "featured": true,
        "permissions": [
            "tabCapture",
            "storage",
            "tabs",
            "cookies"
        ],
        "size": "14.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "ghbmnnjooekpmoecnnnilnnbdlolhkhi",
        "name": "Google Docs Offline",
        "developer": "Google",
        "version": "1.87.1",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.4,
        "reviewCount": "58K",
        "users": "10,000,000+",
        "badge": "Featured",
        "shortDescription": "Edit, create, and view your documents, spreadsheets, and slides without an internet connection.",
        "description": "Google Docs Offline ensures productivity even when network connectivity drops.\n\n• View and edit Google Docs, Sheets, and Slides offline\n• Automatic background syncing when you reconnect\n• Pin files to keep them ready for offline travel.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#2563EB\"/><path d=\"M16 12H28L34 18V36H16V12Z\" fill=\"#FFFFFF\"/><path d=\"M28 12V18H34\" fill=\"#93C5FD\"/><line x1=\"20\" y1=\"22\" x2=\"28\" y2=\"22\" stroke=\"#2563EB\" stroke-width=\"2\"/><line x1=\"20\" y1=\"27\" x2=\"30\" y2=\"27\" stroke=\"#2563EB\" stroke-width=\"2\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "unlimitedStorage",
            "*://docs.google.com/*"
        ],
        "size": "4.8 MB",
        "updated": "August 2026"
    },
    {
        "id": "lpcaedmchfhocbbapmcbpinfpgnhiddi",
        "name": "Google Keep Chrome Extension",
        "developer": "Google",
        "version": "4.24.4",
        "category": "Productivity",
        "categoryLabel": "Productivity",
        "rating": 4.2,
        "reviewCount": "19K",
        "users": "7,000,000+",
        "badge": "Featured",
        "shortDescription": "Save website URLs, quotes, and images to Google Keep notes with a single click and sync across all devices.",
        "description": "Save URLs, images, and text quotes to Google Keep with a single click, and take notes anytime synced across all your devices.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#F59E0B\"/><path d=\"M24 13C19.5 13 16 16.5 16 21C16 23.5 17.5 25.5 19 27V31H29V27C30.5 25.5 32 23.5 32 21C32 16.5 28.5 13 24 13Z\" fill=\"#FFFFFF\"/><rect x=\"21\" y=\"32\" width=\"6\" height=\"2\" rx=\"1\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "activeTab",
            "*://keep.google.com/*"
        ],
        "size": "1.1 MB",
        "updated": "September 2026",
        "reviews": "8,900+",
        "author": "Google LLC",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#F9AB00'/><path d='M24 12c-5 0-9 4-9 9 0 3.1 1.6 5.8 4 7.3V31c0 .6.4 1 1 1h8c.6 0 1-.4 1-1v-2.7c2.4-1.5 4-4.2 4-7.3 0-5-4-9-9-9zm-3 23h6v2h-6zm0-4h6v1h-6z' fill='#ffffff'/></svg>",
        "features": [
            "Save website links, images, and selected text",
            "Add notes and labels directly to saved content",
            "Real-time sync to keep.google.com and mobile apps",
            "Keyboard shortcut support for rapid note capture"
        ]
    },
    {
        "id": "mgijmajocgfcbeboacabfgobmjgjcoja",
        "name": "Google Dictionary (by Google)",
        "developer": "Google",
        "version": "4.1.8",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "21K",
        "users": "4,000,000+",
        "badge": "Popular",
        "shortDescription": "View definitions and pronunciations easily as you browse the web by simply double-clicking any word on any page.",
        "description": "Double-click any word to view its definition, audio pronunciation, and translation in a clean pop-up bubble.\n\n• Supports multiple foreign language dictionaries\n• Store word history to build personal vocabulary\n• Keyboard shortcuts for instant full definitions.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#EA4335\"/><text x=\"24\" y=\"32\" fill=\"#FFFFFF\" font-size=\"22\" font-weight=\"bold\" font-family=\"Georgia, serif\" text-anchor=\"middle\">Aa</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "780 KB",
        "updated": "July 2026"
    },
    {
        "id": "laankejkbhbdhmipfmgcngdelahlfoji",
        "name": "StayFocusd - Distraction Blocker",
        "developer": "stayfocusd.com",
        "version": "1.6.4",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.5,
        "reviewCount": "10K",
        "users": "700,000+",
        "badge": "Popular",
        "shortDescription": "Increase productivity by limiting the amount of time that you spend on time-wasting social and entertainment websites.",
        "description": "StayFocusd is a productivity extension that restricts the time you can spend on distracting sites.\n\n• Set daily time limits on social media and news sites\n• The Nuclear Option blocks entire sites once time is up\n• Highly configurable with sub-domain and path filters.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0D9488\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><polyline points=\"24,16 24,24 29,24\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "1.2 MB",
        "updated": "August 2026"
    },
    {
        "id": "edacconmaakjimmfgnblocblbcdcpbko",
        "name": "Session Buddy - Tab Manager",
        "developer": "sessionbuddy.com",
        "version": "4.0.5",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.8,
        "reviewCount": "32K",
        "users": "1,500,000+",
        "badge": "Featured",
        "shortDescription": "Manage browser tabs and bookmarks with ease. Save open tabs as collections, recover crashed sessions, and free memory.",
        "description": "Session Buddy is the leading tab manager that helps you organize and preserve your tabs.\n\n• Save open tabs as collections that can be restored anytime\n• Automatic session crash recovery protects your work\n• Search across all saved tabs and windows instantly.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#3B82F6\"/><rect x=\"14\" y=\"14\" width=\"20\" height=\"6\" rx=\"2\" fill=\"#FFFFFF\"/><rect x=\"14\" y=\"23\" width=\"20\" height=\"6\" rx=\"2\" fill=\"#93C5FD\"/><rect x=\"14\" y=\"32\" width=\"20\" height=\"6\" rx=\"2\" fill=\"#BFDBFE\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "tabs",
            "sessions"
        ],
        "size": "3.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "jhnleheckmknfcgijgkadoemagpecfol",
        "name": "Auto Tab Discard - Memory Saver",
        "developer": "rNeomy",
        "version": "0.6.9",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.7,
        "reviewCount": "6.2K",
        "users": "300,000+",
        "badge": "Verified",
        "shortDescription": "Speed up your browser and free up RAM by automatically suspending inactive and background browser tabs.",
        "description": "Auto Tab Discard uses native Chromium tab discarding to drastically reduce memory usage.\n\n• Frees up to 80% RAM without losing your tab state\n• Whitelist pinned tabs, audible tabs, or specific domains\n• Instant waking when switching back to discarded tabs.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#047857\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#A7F3D0\" stroke-width=\"2.5\" fill=\"none\"/><path d=\"M24 16V24L29 27\" stroke=\"#A7F3D0\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "alarms"
        ],
        "size": "450 KB",
        "updated": "August 2026"
    },
    {
        "id": "bcjindcccaagfpapjjmafapmmgkkhgoa",
        "name": "Boomerang for Gmail",
        "developer": "baydin.com",
        "version": "1.5.1",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.5,
        "reviewCount": "13K",
        "users": "1,000,000+",
        "badge": "Verified",
        "shortDescription": "Schedule emails to send later, snooze messages, and get follow-up reminders right inside Gmail.",
        "description": "Boomerang provides smart email scheduling and productivity management for Gmail.\n\n• Schedule emails to be delivered at the optimal time\n• Snooze incoming emails until you are ready to handle them\n• AI assistance (Respondable) predicts email response rates.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><path d=\"M16 16C24 14 34 24 32 32C28 26 24 22 16 16Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "*://mail.google.com/*"
        ],
        "size": "2.3 MB",
        "updated": "July 2026"
    },
    {
        "id": "cmeakgjggjdlcpncigglobpjbkabhmjl",
        "name": "Hemingway Editor - Bold & Clear Writing",
        "developer": "hemingwayapp.com",
        "version": "2.1.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "4.8K",
        "users": "200,000+",
        "badge": "Verified",
        "shortDescription": "Make your writing clear, concise, and bold by highlighting passive voice, complex sentences, and common mistakes.",
        "description": "Hemingway Editor acts like a personal copy editor for your browser content.\n\n• Highlights lengthy, complex sentences and structural errors\n• Detects passive voice and weakens words\n• Readability grade level scoring in real time.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1C1917\"/><text x=\"24\" y=\"32\" fill=\"#FACC15\" font-size=\"24\" font-weight=\"bold\" font-family=\"Georgia, serif\" text-anchor=\"middle\">H</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "activeTab"
        ],
        "size": "1.6 MB",
        "updated": "June 2026"
    },
    {
        "id": "efaidnbmnnnibpcajpcglclefindmkaj",
        "name": "Adobe Acrobat - Edit, Convert & Sign PDF",
        "developer": "Adobe Inc.",
        "version": "24.9.15",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.4,
        "reviewCount": "65K",
        "users": "10,000,000+",
        "badge": "Featured",
        "shortDescription": "View, fill, sign, and annotate PDF documents directly in your browser with Adobe's official Acrobat extension.",
        "description": "The official Adobe Acrobat extension brings industry-standard PDF tools into the browser.\n\n• Fill out and sign PDF forms seamlessly\n• Add comments, highlights, and sticky notes\n• Convert web pages into high-fidelity PDF documents.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#E11D48\"/><path d=\"M15 34V14L24 26L33 14V34H28V22L24 28L20 22V34H15Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "webRequest",
            "downloads"
        ],
        "size": "11.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "eignhdfgaldabilaaegmdfbajngjmoke",
        "name": "GoFullPage - Full Page Screen Capture",
        "developer": "gofullpage.com",
        "version": "7.14",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.8,
        "reviewCount": "82K",
        "users": "7,000,000+",
        "badge": "Featured",
        "shortDescription": "Capture a screenshot of your current web page entirely and reliably without requesting any unnecessary permissions.",
        "description": "GoFullPage is the highest-rated screenshot tool for capturing complete web pages.\n\n• Scrolls through the entire page and stitches screenshots flawlessly\n• Export as high-resolution PNG, JPEG, or multi-page PDF\n• No bloat, no ads, zero tracking.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#4338CA\"/><circle cx=\"24\" cy=\"24\" r=\"10\" stroke=\"#FFFFFF\" stroke-width=\"3\" fill=\"none\"/><circle cx=\"24\" cy=\"24\" r=\"4\" fill=\"#38BDF8\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "activeTab"
        ],
        "size": "2.8 MB",
        "updated": "August 2026"
    },
    {
        "id": "nffaoalbilbmmfgbnbgppjihopabppdk",
        "name": "Video Speed Controller",
        "developer": "Ilya Grigorik",
        "version": "0.7.3",
        "category": "media",
        "categoryLabel": "Media & Streaming",
        "rating": 4.7,
        "reviewCount": "28K",
        "users": "4,000,000+",
        "badge": "Featured",
        "shortDescription": "Speed up, slow down, advance and rewind any HTML5 video using convenient keyboard shortcuts.",
        "description": "Optimize your video viewing experience across Netflix, YouTube, Coursera, and anywhere HTML5 video is played.\n\n• Speed up video playback from 0.1x to 16x with 0.1x step accuracy\n• Custom keyboard shortcuts for instant rewinding and fast-forwarding\n• Completely open-source and lightweight.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#7C3AED\"/><polygon points=\"18,16 28,24 18,32\" fill=\"#FFFFFF\"/><polygon points=\"26,16 36,24 26,32\" fill=\"#FFFFFF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "180 KB",
        "updated": "July 2026"
    },
    {
        "id": "kbmfpngjjgdllneeigpgjifpgocmfgmb",
        "name": "Reddit Enhancement Suite (RES)",
        "developer": "honestbleeps",
        "version": "5.24.6",
        "category": "media",
        "categoryLabel": "Media & Streaming",
        "rating": 4.6,
        "reviewCount": "44K",
        "users": "1,000,000+",
        "badge": "Popular",
        "shortDescription": "A suite of modules that enhance your Reddit browsing experience: infinite scroll, user tagger, inline image viewer, and more.",
        "description": "Reddit Enhancement Suite revolutionizes browsing Reddit on desktop.\n\n• Never-Ending Reddit seamlessly loads new pages automatically\n• Inline Image and Video Viewer without opening new tabs\n• Account switcher and user tagger for easy community management.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#FF4500\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#FFFFFF\"/><circle cx=\"19\" cy=\"22\" r=\"2.5\" fill=\"#FF4500\"/><circle cx=\"29\" cy=\"22\" r=\"2.5\" fill=\"#FF4500\"/><path d=\"M19 28C21 31 27 31 29 28\" stroke=\"#FF4500\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "*://*.reddit.com/*"
        ],
        "size": "6.9 MB",
        "updated": "September 2026"
    },
    {
        "id": "clngdbkpkpeebahjckkjfobafhncgmne",
        "name": "Stylus - Custom Web Themes",
        "developer": "stylus-org",
        "version": "1.5.52",
        "category": "appearance",
        "categoryLabel": "Appearance",
        "rating": 4.7,
        "reviewCount": "16K",
        "users": "900,000+",
        "badge": "Featured",
        "shortDescription": "Redesign your favorite websites with custom CSS themes, userstyles, and dark modes from UserStyles.world.",
        "description": "Stylus is a modern userstyles manager that lets you easily install and customize themes for any website.\n\n• Install custom themes from UserStyles.world with one click\n• Built-in advanced CSS editor with linting and autocomplete\n• Privacy-focused without analytics or telemetry.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#18181B\"/><path d=\"M16 32L30 18L32 20L18 34L14 34L16 32Z\" fill=\"#EC4899\"/><circle cx=\"33\" cy=\"17\" r=\"3\" fill=\"#F472B6\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webNavigation",
            "<all_urls>"
        ],
        "size": "1.8 MB",
        "updated": "August 2026"
    },
    {
        "id": "bhchdcejhohfmigjafbampogmaanbfkg",
        "name": "User-Agent Switcher and Manager",
        "developer": "Ray Lothian",
        "version": "0.5.8",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "8.9K",
        "users": "800,000+",
        "badge": "Featured",
        "shortDescription": "Spoof user-agent strings, navigator properties, and device headers to test mobile and browser responsiveness.",
        "description": "Quickly switch between user-agent strings from a vast database of real browser profiles.\n\n• Emulate iOS, Android, macOS, Linux, and Windows browsers\n• Spoofs both HTTP request headers and window.navigator JS properties\n• Custom rules per domain for automated switching.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0369A1\"/><rect x=\"14\" y=\"16\" width=\"20\" height=\"14\" rx=\"2\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><line x1=\"20\" y1=\"34\" x2=\"28\" y2=\"34\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "950 KB",
        "updated": "July 2026"
    },
    {
        "id": "dbepggeogbaibhgnhhndojpepiihcmeb",
        "name": "Vimium - The Hacker's Browser",
        "developer": "Phil Crosby",
        "version": "2.1.2",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.9,
        "reviewCount": "17K",
        "users": "600,000+",
        "badge": "Featured",
        "shortDescription": "The Hacker's Browser. Provides keyboard shortcuts for navigation and control in the spirit of the Vim editor.",
        "description": "Vimium brings keyboard-first navigation to the web without needing your mouse.\n\n• Press 'f' to generate keyboard hints on any clickable link or button\n• Smooth scrolling with j/k keys\n• Quick tab switching and omnibar navigation.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#065F46\"/><text x=\"24\" y=\"32\" fill=\"#34D399\" font-size=\"22\" font-weight=\"bold\" font-family=\"Courier, monospace\" text-anchor=\"middle\">Vim</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "620 KB",
        "updated": "August 2026"
    },
    {
        "id": "jlmpjdjjbgclbocgajdjefcidcncaied",
        "name": "daily.dev - The Homepage for Developers",
        "developer": "daily.dev",
        "version": "3.28.0",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.8,
        "reviewCount": "15K",
        "users": "400,000+",
        "badge": "Featured",
        "shortDescription": "Get the best developer news, articles, and engineering tutorials tailored to your tech stack in a modern new tab.",
        "description": "daily.dev curates top engineering articles from 600+ sources into a clean developer feed.\n\n• Stay updated on React, AI, Python, Rust, and DevOps trends\n• Upvote, bookmark, and discuss with global software engineers\n• Zero tracking or data selling.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#090A10\"/><polygon points=\"24,10 36,18 36,30 24,38 12,30 12,18\" fill=\"#E11D48\"/><circle cx=\"24\" cy=\"24\" r=\"5\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "3.9 MB",
        "updated": "September 2026"
    },
    {
        "id": "djflhoibgkdhkhhcedjiklpkjnoahfmg",
        "name": "User JavaScript and CSS",
        "developer": "Anatolii Shyyanov",
        "version": "1.4.1",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "3.1K",
        "users": "150,000+",
        "badge": "Verified",
        "shortDescription": "Easily inject custom JS scripts and CSS rules into any website with an instant live-reloading code editor.",
        "description": "User JavaScript and CSS lets you tweak and automate web pages on the fly.\n\n• Code mirror editor with syntax highlighting\n• Automatically runs your custom JS/CSS on specified URL patterns\n• Instant preview without refreshing.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1E293B\"/><text x=\"17\" y=\"27\" fill=\"#F59E0B\" font-size=\"12\" font-weight=\"bold\" font-family=\"monospace\">JS</text><text x=\"31\" y=\"27\" fill=\"#38BDF8\" font-size=\"12\" font-weight=\"bold\" font-family=\"monospace\">CSS</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "1.2 MB",
        "updated": "July 2026"
    },
    {
        "id": "fooolghllnmhmmndgjiamiiodkpenpbb",
        "name": "ColorPick Eyedropper",
        "developer": "vidsbee.com",
        "version": "0.5.12",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.5,
        "reviewCount": "5.4K",
        "users": "1,500,000+",
        "badge": "Popular",
        "shortDescription": "An eye-dropper & color-picker tool that allows you to select color values from web pages with pixel-level zoom.",
        "description": "Inspect colors with a built-in magnifying glass to grab hex, RGB, and HSL values from any pixel.\n\n• Zoomed preview grid for pixel-perfect sampling\n• Color palette history with clipboard copy\n• Supports picking colors from images, SVGs, and canvas elements.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#BE185D\"/><path d=\"M28 14L34 20L31 23L25 17L28 14Z\" fill=\"#F472B6\"/><path d=\"M24 18L16 26V32H22L30 24L24 18Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "activeTab"
        ],
        "size": "680 KB",
        "updated": "June 2026"
    },
    {
        "id": "fhbjgbiflinjbdggehcddcbncdddomop",
        "name": "Postman - Web API Companion",
        "developer": "Postman",
        "version": "5.5.4",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "8.2K",
        "users": "2,000,000+",
        "badge": "Featured",
        "shortDescription": "Seamlessly test HTTP APIs, capture browser network requests, and manage authentication cookies in Postman.",
        "description": "Postman browser companion bridges your browser requests into your Postman API workspace.\n\n• Capture browser requests and sync directly to API collections\n• Manage and sync authentication cookies automatically\n• Test REST and GraphQL endpoints directly.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#FF6C37\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"24\" r=\"9\" fill=\"#FF6C37\"/></svg>",
        "featured": true,
        "permissions": [
            "webRequest",
            "cookies",
            "<all_urls>"
        ],
        "size": "4.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "mpbjkejclgfgadiemmefgebjfooflfhl",
        "name": "Buster: Captcha Solver for Humans",
        "developer": "dessant",
        "version": "3.1.0",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "12K",
        "users": "900,000+",
        "badge": "Featured",
        "shortDescription": "Solve difficult reCAPTCHA audio challenges automatically using speech recognition with a single click.",
        "description": "Buster helps you bypass tedious image captchas by completing audio challenges automatically.\n\n• Solves reCAPTCHA audio challenges in seconds\n• Supports multiple speech recognition engines\n• Saves immense time when browsing behind strict firewalls.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#EA580C\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><path d=\"M20 20C20 17.8 21.8 16 24 16C26.2 16 28 17.8 28 20C28 22.2 26 23 24 25V27M24 31H24.02\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "<all_urls>"
        ],
        "size": "1.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "hlkenndednhfkekhgcdicdfddnkalmdm",
        "name": "Cookie-Editor",
        "developer": "Moustachauve",
        "version": "1.13.0",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.8,
        "reviewCount": "9.5K",
        "users": "1,000,000+",
        "badge": "Featured",
        "shortDescription": "Fast, powerful and intuitive cookie manager. Add, delete, edit, search, and export/import cookies in JSON.",
        "description": "Cookie-Editor is designed to be the fastest way to view, edit, and export cookies for the current tab.\n\n• Edit cookie values, domains, paths, and expiration dates\n• Export and import cookie sets in JSON or Netscape format\n• Great for developers testing session states and auth flows.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#D97706\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#B45309\"/><circle cx=\"19\" cy=\"19\" r=\"2.5\" fill=\"#78350F\"/><circle cx=\"28\" cy=\"20\" r=\"2\" fill=\"#78350F\"/><circle cx=\"23\" cy=\"28\" r=\"2.5\" fill=\"#78350F\"/></svg>",
        "featured": false,
        "permissions": [
            "cookies",
            "storage",
            "<all_urls>"
        ],
        "size": "820 KB",
        "updated": "September 2026"
    },
    {
        "id": "nkbihfbeogaeaoehlefnkodbefgpgknn",
        "name": "MetaMask",
        "developer": "Consensys Software Inc.",
        "version": "12.4.1",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.5,
        "reviewCount": "250K",
        "users": "10,000,000+",
        "badge": "Featured",
        "shortDescription": "The premier self-custodial crypto wallet and gateway to blockchain dApps on Ethereum, Arbitrum, Polygon, and more.",
        "description": "MetaMask is trusted by over 30 million users worldwide to explore web3.\n\n• Store, swap, and manage tokens and NFTs safely\n• Seamlessly connect with decentralized finance (DeFi) dApps\n• Industry-standard hardware wallet support (Ledger, Trezor).",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#F6851B\"/><polygon points=\"24,10 14,24 24,38 34,24\" fill=\"#E2761B\"/><polygon points=\"24,10 18,22 24,20\" fill=\"#CD6116\"/><polygon points=\"24,10 30,22 24,20\" fill=\"#CD6116\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "unlimitedStorage",
            "clipboardWrite"
        ],
        "size": "24.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "bfnaelmomeimhlpmgjnjophhpkkoljpa",
        "name": "Phantom - Solana, Ethereum & Bitcoin Wallet",
        "developer": "Phantom Technologies",
        "version": "24.18.0",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "85K",
        "users": "5,000,000+",
        "badge": "Featured",
        "shortDescription": "A friendly, secure multichain crypto wallet for Solana, Ethereum, Polygon, and Bitcoin with built-in swap.",
        "description": "Phantom turns your browser into a crypto wallet for the decentralized web.\n\n• Instant token swaps with the lowest fees and slippage protection\n• Gorgeous NFT gallery and collection viewer\n• Scam detection engine warns you before interacting with malicious sites.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#AB9FF2\"/><ellipse cx=\"24\" cy=\"24\" rx=\"14\" ry=\"12\" fill=\"#5841D8\"/><circle cx=\"19\" cy=\"22\" r=\"2.5\" fill=\"#FFFFFF\"/><circle cx=\"29\" cy=\"22\" r=\"2.5\" fill=\"#FFFFFF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "18.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "hnfanknocfeofbddgcijnmhnfnkdnaad",
        "name": "Coinbase Wallet Extension",
        "developer": "Coinbase",
        "version": "3.92.0",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.6,
        "reviewCount": "22K",
        "users": "2,000,000+",
        "badge": "Verified",
        "shortDescription": "Self-custody crypto wallet supporting hundreds of thousands of tokens and dApps across Ethereum, Solana, and L2s.",
        "description": "Coinbase Wallet is your passport to the decentralized web.\n\n• Support for Ethereum, Solana, Base, Arbitrum, Optimism, and Avalanche\n• Explore DeFi, collect NFTs, and bridge assets effortlessly\n• Backed by Coinbase's world-class security team.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0052FF\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#FFFFFF\"/><rect x=\"20\" y=\"20\" width=\"8\" height=\"8\" rx=\"2\" fill=\"#0052FF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "19.5 MB",
        "updated": "September 2026"
    },
    {
        "id": "aholpfdialjgjfhomihkjbmgjidlcdno",
        "name": "Exodus Web3 Wallet",
        "developer": "Exodus Movement, Inc.",
        "version": "24.38.1",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.7,
        "reviewCount": "14K",
        "users": "600,000+",
        "badge": "Verified",
        "shortDescription": "Multi-chain Web3 wallet for Solana, Ethereum, Cardano, and Bitcoin. Connect to dApps with complete peace of mind.",
        "description": "Exodus is beautifully designed for discovering web3 apps without complex interfaces.\n\n• Manage 50+ blockchain networks from one simple interface\n• Hardware wallet integration with Trezor\n• Built-in decentralized exchange with real-time portfolio tracking.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0B0E14\"/><polygon points=\"24,10 36,18 36,30 24,38 12,30 12,18\" stroke=\"#8952FF\" stroke-width=\"2.5\" fill=\"none\"/><line x1=\"16\" y1=\"18\" x2=\"32\" y2=\"30\" stroke=\"#8952FF\" stroke-width=\"2.5\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "21.0 MB",
        "updated": "August 2026"
    },
    {
        "id": "dmkamcknogkgcdfhhbddcghachkejeap",
        "name": "Keplr Wallet - Interchain Gateway",
        "developer": "chainapsis.com",
        "version": "0.12.98",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.7,
        "reviewCount": "18K",
        "users": "1,000,000+",
        "badge": "Featured",
        "shortDescription": "The leading wallet for Cosmos and the Interchain ecosystem. Stake tokens, transfer via IBC, and govern networks.",
        "description": "Keplr is the premiere non-custodial wallet for the Cosmos SDK and IBC ecosystem.\n\n• One-click staking and rewards claim across 50+ Cosmos networks\n• Seamless cross-chain transfers via Inter-Blockchain Communication (IBC)\n• Ledger hardware wallet connection supported.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0C0E14\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#00D0FF\"/><path d=\"M20 16V32M20 24L28 16M20 24L28 32\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "14.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "opcgpfmipidbgpenhmajoajpbobppdil",
        "name": "Sui Wallet",
        "developer": "Mysten Labs",
        "version": "24.9.2",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "29K",
        "users": "2,000,000+",
        "badge": "Featured",
        "shortDescription": "The official web3 wallet for the Sui blockchain ecosystem. Manage SUI tokens, stake, and connect to Sui dApps.",
        "description": "Sui Wallet is developed directly by Mysten Labs to provide the smoothest Sui experience.\n\n• Native support for zkLogin using Google or Twitch accounts\n• Mint, trade, and organize interactive Sui dynamic NFTs\n• Instant transaction execution with ultra-low gas fees.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#2A82E4\"/><path d=\"M24 10C24 10 14 22 14 28C14 33.5 18.5 38 24 38C29.5 38 34 33.5 34 28C34 22 24 10 24 10Z\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "12.4 MB",
        "updated": "September 2026"
    },
    {
        "id": "ejjladinnckdgjemekebdpeokbikhfci",
        "name": "Petra Aptos Wallet",
        "developer": "Aptos Labs",
        "version": "1.6.14",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "24K",
        "users": "1,500,000+",
        "badge": "Featured",
        "shortDescription": "The official Aptos blockchain wallet built by Aptos Labs. Explore Aptos DeFi, games, and NFT marketplaces.",
        "description": "Petra Wallet provides high security and lightning fast transaction speeds on Aptos.\n\n• Simulate transaction outcomes before signing\n• Native NFT gallery and in-app asset swapping\n• Hardware wallet integration with Ledger.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0D0D0D\"/><path d=\"M14 34L24 14L34 34H28L24 25L20 34H14Z\" fill=\"#F59E0B\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "10.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "phkbamefinggmakgklpkljjmgibohnba",
        "name": "Pontem Aptos Wallet",
        "developer": "Pontem Network",
        "version": "2.6.2",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.7,
        "reviewCount": "11K",
        "users": "600,000+",
        "badge": "Verified",
        "shortDescription": "Premier Web3 wallet for the Aptos ecosystem with native Liquidswap DEX integration and token swapping.",
        "description": "Pontem Wallet connects you with the fastest growing applications on the Aptos network.\n\n• Native integration with Liquidswap DEX for the lowest slippage\n• Multi-account management and biometric unlocking\n• Verified security audits by Kudelski and OtterSec.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#5A28D8\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><circle cx=\"24\" cy=\"24\" r=\"6\" fill=\"#F43F5E\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "9.6 MB",
        "updated": "July 2026"
    },
    {
        "id": "fijngjgcjhjmmpcmkeiomlglpeiijkld",
        "name": "Talisman - Polkadot & Ethereum Wallet",
        "developer": "Talisman",
        "version": "2.0.1",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "7.8K",
        "users": "300,000+",
        "badge": "Verified",
        "shortDescription": "The premier Web3 wallet for Polkadot, Kusama, and Ethereum. One wallet for all your sub-chains.",
        "description": "Talisman brings Polkadot and EVM chains together into a single, elegant interface.\n\n• Manage assets across 100+ parachains and Ethereum networks\n• Seamless cross-chain teleportation and IBC staking\n• Ledger and Keystone hardware wallet support.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#18181B\"/><polygon points=\"24,12 34,32 14,32\" fill=\"#D97706\"/><circle cx=\"24\" cy=\"24\" r=\"3\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "16.4 MB",
        "updated": "September 2026"
    },
    {
        "id": "acmacodkjbdgmoleebolmdjonilkdbch",
        "name": "Rabby Wallet - Multi-chain EVM Wallet",
        "developer": "DeBank",
        "version": "0.92.84",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.9,
        "reviewCount": "16K",
        "users": "700,000+",
        "badge": "Featured",
        "shortDescription": "The game-changing open source multi-chain wallet for Ethereum and all EVM chains with pre-transaction risk scanning.",
        "description": "Rabby is built by DeBank to offer the safest DeFi trading experience in Web3.\n\n• Pre-transaction security check simulates balance changes and warns of drainers\n• Automatically switches to the correct chain for every dApp\n• Works with 141+ chains out of the box.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#7084FF\"/><ellipse cx=\"24\" cy=\"24\" rx=\"12\" ry=\"10\" fill=\"#FFFFFF\"/><circle cx=\"20\" cy=\"22\" r=\"2\" fill=\"#7084FF\"/><circle cx=\"28\" cy=\"22\" r=\"2\" fill=\"#7084FF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "15.9 MB",
        "updated": "September 2026"
    },
    {
        "id": "bhhhlbepdkbapadjdnnojkbgioiodbic",
        "name": "Solflare - Solana Wallet",
        "developer": "Solflare",
        "version": "2.5.3",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.7,
        "reviewCount": "19K",
        "users": "1,500,000+",
        "badge": "Featured",
        "shortDescription": "Safe and powerful Solana wallet. Stake SOL, swap tokens with zero fees, and browse Solana dApps.",
        "description": "Solflare is the safest non-custodial wallet for Solana.\n\n• Anti-phishing protection warns against malicious contracts\n• Hardware wallet support (Ledger) with Bluetooth connectivity\n• In-app token burning to earn SOL back from spam NFTs.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#121212\"/><polygon points=\"24,10 34,24 24,38 14,24\" fill=\"#FC8030\"/><circle cx=\"24\" cy=\"24\" r=\"5\" fill=\"#FFE500\"/></svg>",
        "featured": true,
        "permissions": [
            "storage"
        ],
        "size": "13.6 MB",
        "updated": "September 2026"
    },
    {
        "id": "jiidiaalihmmhddjgbnbgdfflelocpak",
        "name": "Bitget Wallet (BitKeep)",
        "developer": "Bitget",
        "version": "8.14.0",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "35K",
        "users": "2,000,000+",
        "badge": "Featured",
        "shortDescription": "Ultimate multi-chain crypto wallet supporting 100+ mainnets, token swap, DApp browser, and NFT marketplace.",
        "description": "Bitget Wallet is a global multi-chain Web3 trading portal.\n\n• Aggregates liquidity from hundreds of decentralized exchanges\n• Backed by a $300 Million Bitget Protection Fund\n• Instant multi-chain gas fee payment with stablecoins.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#00F0FF\"/><path d=\"M16 14H24C28.4 14 32 17.6 32 22C32 24.2 31.1 26.2 29.6 27.6C31.7 29 33 31.4 33 34C33 38.4 29.4 42 25 42H16V14Z\" fill=\"#121212\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "22.1 MB",
        "updated": "September 2026"
    },
    {
        "id": "dlcobpjiigpikoobohmabehhmhfoodbb",
        "name": "Argent X - Starknet Smart Wallet",
        "developer": "argent.xyz",
        "version": "5.18.2",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "14K",
        "users": "800,000+",
        "badge": "Verified",
        "shortDescription": "The premier smart contract wallet for Starknet. Multicall transactions, account abstraction, and low fees.",
        "description": "Argent X is built exclusively for Starknet Layer-2 scalability.\n\n• Native Account Abstraction allows multicall bundle transactions in one click\n• Guardian recovery protects against loss of secret recovery phrases\n• Seamlessly explore games, DeFi, and NFTs on Starknet.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#FF5722\"/><circle cx=\"24\" cy=\"24\" r=\"13\" fill=\"#FFFFFF\"/><path d=\"M24 15L17 31H31L24 15Z\" fill=\"#FF5722\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "14.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "klghhnkeealcohjjanjjdaeeggmfmlpl",
        "name": "Zerion - Web3 Smart Wallet",
        "developer": "Zerion",
        "version": "1.12.0",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.8,
        "reviewCount": "9.2K",
        "users": "400,000+",
        "badge": "Verified",
        "shortDescription": "Track your entire crypto portfolio, trade across 15+ EVM chains, and connect to dApps with zero hassle.",
        "description": "Zerion gives you a unified view of your crypto holdings and DeFi positions.\n\n• Real-time P&L tracking, historical transactions, and gas fee analytics\n• Cross-chain bridging and swaps with optimized routing\n• Battle-tested security audited by leading firms.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#2962FF\"/><path d=\"M16 16H32L20 32H32\" stroke=\"#FFFFFF\" stroke-width=\"3.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "17.0 MB",
        "updated": "August 2026"
    },
    {
        "id": "kkpllkodjeloidieedojogacfhpaihoh",
        "name": "Enkrypt - Multichain Crypto Wallet",
        "developer": "MyEtherWallet Inc.",
        "version": "1.42.0",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.7,
        "reviewCount": "5.1K",
        "users": "250,000+",
        "badge": "Verified",
        "shortDescription": "Open-source multichain web3 wallet by MyEtherWallet (MEW). Support for Ethereum, Bitcoin, Polkadot, and Solana.",
        "description": "Enkrypt is an open-source non-custodial crypto wallet by the pioneers of Ethereum.\n\n• Native support for Ethereum, Bitcoin, Polkadot, Substrate, and Solana\n• In-app cross-chain token swap and bridging\n• 100% open-source and audited by security researchers.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#7C3AED\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#C4B5FD\" stroke-width=\"2.5\" fill=\"none\"/><path d=\"M18 24H30M24 18V30\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "15.1 MB",
        "updated": "July 2026"
    },
    {
        "id": "ibnejdfjmmkpcnlpebklmnkoeoihofec",
        "name": "TronLink - TRON Wallet",
        "developer": "tronlink.org",
        "version": "4.1.8",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.6,
        "reviewCount": "38K",
        "users": "3,000,000+",
        "badge": "Featured",
        "shortDescription": "The official TRON wallet for managing TRX, TRC-10, and TRC-20 tokens and interacting with the TRON DeFi ecosystem.",
        "description": "TronLink is the most widely adopted wallet for the TRON blockchain.\n\n• Transfer TRX, USDT (TRC-20), and tokens with instantaneous settlement\n• Freeze TRX for free bandwidth and energy to save on gas fees\n• Connect to SunSwap, JustLend, and all TRON dApps.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#DC2626\"/><polygon points=\"24,10 36,18 30,36 24,32 18,36 12,18\" fill=\"#FFFFFF\"/><polygon points=\"24,16 30,22 24,30 18,22\" fill=\"#DC2626\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "11.2 MB",
        "updated": "August 2026"
    },
    {
        "id": "egjidjbpglichdcondbcbdnbeeppgdph",
        "name": "Trust Wallet",
        "developer": "Trust Wallet",
        "version": "2.14.0",
        "category": "security",
        "categoryLabel": "Crypto & Web3",
        "rating": 4.6,
        "reviewCount": "42K",
        "users": "3,000,000+",
        "badge": "Featured",
        "shortDescription": "Official crypto wallet of Binance. Manage millions of assets across 100+ blockchains securely in your browser.",
        "description": "Trust Wallet brings true multi-chain asset self-custody to your desktop browser.\n\n• Manage 10+ million digital assets across 100+ blockchains\n• Built-in security scanner detects malicious token approvals\n• Connect to any Web3 dApp with universal auto-detection.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0500FF\"/><path d=\"M24 10L14 15V24C14 31 18.5 37 24 39C29.5 37 34 31 34 24V15L24 10Z\" fill=\"#FFFFFF\"/><path d=\"M24 13L16 17V24C16 29.5 19.5 34.5 24 36C28.5 34.5 32 29.5 32 24V17L24 13Z\" fill=\"#0500FF\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "unlimitedStorage"
        ],
        "size": "16.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "ifdepgnnjpnbkcgempionjablajancjc",
        "name": "FocusGuard: Block Websites & Stay Focused",
        "developer": "FocusGuard",
        "version": "2.3.1",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.8,
        "reviewCount": "8.4K",
        "users": "300,000+",
        "badge": "Featured",
        "shortDescription": "Block distracting websites, adult content, and social media to improve concentration and boost daily productivity.",
        "description": "FocusGuard helps you reclaim control of your digital time.\n\n• One-click blocking of distracting and adult websites\n• Pomodoro focus timer with automated break scheduling\n• Password protection prevents cheating during active work sessions\n• Detailed usage statistics and distraction analytics.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#047857\"/><path d=\"M24 10L14 15V24C14 31 18.5 37 24 39C29.5 37 34 31 34 24V15L24 10Z\" fill=\"#10B981\" stroke=\"#FFFFFF\" stroke-width=\"2\"/><circle cx=\"24\" cy=\"24\" r=\"5\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><line x1=\"24\" y1=\"16\" x2=\"24\" y2=\"19\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/><line x1=\"24\" y1=\"29\" x2=\"24\" y2=\"32\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" stroke-linecap=\"round\"/></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "webRequest",
            "tabs"
        ],
        "size": "2.4 MB",
        "updated": "October 2026"
    },
    {
        "id": "jkkfeklkdokfnlfodkdafododleokdoa",
        "name": "Adult Content Blocker - Safe Browsing",
        "developer": "SafeWeb Security",
        "version": "3.1.0",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "14K",
        "users": "500,000+",
        "badge": "Verified",
        "shortDescription": "Automatically block adult websites, explicit images, and NSFW domains with real-time AI filtering and PIN lock.",
        "description": "Keep your browser environment safe for family, study, and professional work.\n\n• Over 5,000,000+ known adult and explicit domains blocked automatically\n• Real-time text and keyword filtering on active web pages\n• PIN-code protection prevents unauthorized disabling\n• Strict SafeSearch enforcement on Google, Bing, and DuckDuckGo.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1E293B\"/><circle cx=\"24\" cy=\"24\" r=\"14\" fill=\"#EF4444\"/><line x1=\"16\" y1=\"16\" x2=\"32\" y2=\"32\" stroke=\"#FFFFFF\" stroke-width=\"3.5\" stroke-linecap=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "webRequestBlocking",
            "<all_urls>"
        ],
        "size": "3.2 MB",
        "updated": "October 2026"
    },
    {
        "id": "fheifbcbhmbfbebmidanhpeebappbfpd",
        "name": "Ninja Adult Blocker",
        "developer": "Ninja Security Labs",
        "version": "1.9.4",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.6,
        "reviewCount": "6.2K",
        "users": "200,000+",
        "badge": "Verified",
        "shortDescription": "Ultra-fast, stealth adult website filter with zero background memory bloat and tamper-proof protection.",
        "description": "Ninja Adult Blocker provides high-performance content filtering without slowing down your browser.\n\n• Instant domain lookups using high-speed local bloom filters\n• Blocks pop-up adult advertisements and redirect chains\n• Tamper-resistant uninstallation lock with recovery email.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0F172A\"/><polygon points=\"24,12 28,20 36,24 28,28 24,36 20,28 12,24 20,20\" fill=\"#E11D48\"/><circle cx=\"24\" cy=\"24\" r=\"4\" fill=\"#FFFFFF\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "declarativeNetRequest"
        ],
        "size": "1.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "jmalldnhdmffpmcmgiblnemchlgppaam",
        "name": "Safe View – Adult & Distraction Blocker",
        "developer": "SafeView Systems",
        "version": "2.0.8",
        "category": "privacy",
        "categoryLabel": "Privacy & Security",
        "rating": 4.7,
        "reviewCount": "9.1K",
        "users": "350,000+",
        "badge": "Verified",
        "shortDescription": "Intelligent visual and URL safety filter that blurs sensitive images and blocks harmful websites seamlessly.",
        "description": "Safe View creates a clean internet experience by inspecting web elements in real time.\n\n• Blurs explicit imagery before it renders on your screen\n• Whitelist and blacklist manager for customized parental control\n• Password-locked settings and scheduled browsing mode.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#0284C7\"/><path d=\"M14 24C14 24 18 16 24 16C30 16 34 24 34 24C34 24 30 32 24 32C18 32 14 24 14 24Z\" stroke=\"#FFFFFF\" stroke-width=\"2.5\" fill=\"none\"/><circle cx=\"24\" cy=\"24\" r=\"4\" fill=\"#38BDF8\"/><path d=\"M21 24L23 26L27 22\" stroke=\"#FFFFFF\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "2.6 MB",
        "updated": "October 2026"
    },
    {
        "id": "kjchkpkjpiloipaonppkmepcbhcncedo",
        "name": "Adobe Photoshop (Web Edition)",
        "developer": "Adobe Inc.",
        "version": "25.2.0",
        "category": "media",
        "categoryLabel": "Design & Media",
        "rating": 4.8,
        "reviewCount": "42K",
        "users": "4,000,000+",
        "badge": "Featured",
        "shortDescription": "Official Adobe Photoshop browser companion. Open, view, and edit PSD files directly in your web browser.",
        "description": "Bring the world's most powerful image editing tools directly into your browser tab.\n\n• View and edit Photoshop PSD and PSB files with full layer support\n• Generative Fill and Generative Expand powered by Adobe Firefly AI\n• Cloud document sync and collaborative commenting with teammates\n• Export directly to JPEG, PNG, TIFF, or WebP.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#001E36\"/><rect x=\"4\" y=\"4\" width=\"40\" height=\"40\" rx=\"9\" stroke=\"#31A8FF\" stroke-width=\"2\"/><text x=\"18\" y=\"32\" fill=\"#31A8FF\" font-size=\"20\" font-weight=\"bold\" font-family=\"Arial, sans-serif\">P</text><text x=\"30\" y=\"32\" fill=\"#31A8FF\" font-size=\"16\" font-weight=\"bold\" font-family=\"Arial, sans-serif\">s</text></svg>",
        "featured": true,
        "permissions": [
            "storage",
            "unlimitedStorage",
            "clipboardWrite"
        ],
        "size": "18.5 MB",
        "updated": "October 2026"
    },
    {
        "id": "kgmjjkfjacffaebgpkpcllakjifppnca",
        "name": "Adobe Experience Cloud Debugger",
        "developer": "Adobe Inc.",
        "version": "2.3.8",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.6,
        "reviewCount": "5.8K",
        "users": "200,000+",
        "badge": "Verified",
        "shortDescription": "Inspect, debug, and validate your Adobe Analytics, Adobe Target, Audience Manager, and Experience Platform tags.",
        "description": "The official debugger for digital marketing engineers and analytics specialists.\n\n• Real-time inspection of Adobe Experience Platform Web SDK requests\n• Trace Adobe Target activity rendering and audience qualifications\n• Export logs and server calls for audit and verification\n• Console logging tools for custom Edge configurations.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#FA0F00\"/><polygon points=\"24,10 36,36 30,36 27,28 21,28 18,36 12,36\" fill=\"#FFFFFF\"/><polygon points=\"24,19 25.8,24 22.2,24\" fill=\"#FA0F00\"/><circle cx=\"34\" cy=\"16\" r=\"3\" fill=\"#FFE600\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "webRequest",
            "<all_urls>"
        ],
        "size": "4.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "jjbnimknldmndoffkccbmjhfdlneiapp",
        "name": "Adobe Premiere Pro Extension",
        "developer": "Adobe Inc.",
        "version": "24.1.2",
        "category": "media",
        "categoryLabel": "Design & Media",
        "rating": 4.7,
        "reviewCount": "7.2K",
        "users": "500,000+",
        "badge": "Verified",
        "shortDescription": "Integrate online stock footage, music tracks, and web video assets directly into your active Adobe Premiere Pro projects.",
        "description": "Streamline video production workflow between the browser and desktop Premiere Pro.\n\n• One-click import of online assets into Premiere Pro timelines and bins\n• Review video cuts and timeline annotations collaboratively\n• Access Adobe Stock video and sound effects directly from your browser.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#00005B\"/><rect x=\"4\" y=\"4\" width=\"40\" height=\"40\" rx=\"9\" stroke=\"#9999FF\" stroke-width=\"2\"/><text x=\"18\" y=\"32\" fill=\"#9999FF\" font-size=\"20\" font-weight=\"bold\" font-family=\"Arial, sans-serif\">P</text><text x=\"30\" y=\"32\" fill=\"#9999FF\" font-size=\"16\" font-weight=\"bold\" font-family=\"Arial, sans-serif\">r</text></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "downloads"
        ],
        "size": "6.5 MB",
        "updated": "August 2026"
    },
    {
        "id": "igkmdomcgoebiipaifhmpfjhbjccggml",
        "name": "AEM Sidekick",
        "developer": "Adobe Inc.",
        "version": "3.2.14",
        "category": "developer",
        "categoryLabel": "Developer Tools",
        "rating": 4.7,
        "reviewCount": "4.1K",
        "users": "150,000+",
        "badge": "Verified",
        "shortDescription": "Author, preview, test, and publish web pages with Adobe Experience Manager (AEM) Edge Delivery Services.",
        "description": "AEM Sidekick is the companion for authoring modern, composable websites.\n\n• Switch between production, live, and preview environments with one click\n• Trigger instant cache-purging and Edge publication\n• Inspect performance and Core Web Vitals directly in your editing workflow.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#EB1000\"/><polygon points=\"24,12 36,34 29,34 24,24 19,34 12,34\" fill=\"#FFFFFF\"/><circle cx=\"35\" cy=\"15\" r=\"4\" fill=\"#14B8A6\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "<all_urls>"
        ],
        "size": "3.8 MB",
        "updated": "September 2026"
    },
    {
        "id": "lhdepbgeilldghlfnankdnponhljpgml",
        "name": "Adobe Workfront Review Tool",
        "developer": "Adobe Inc.",
        "version": "4.8.0",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.5,
        "reviewCount": "3.6K",
        "users": "100,000+",
        "badge": "Verified",
        "shortDescription": "Markup, proof, annotate, and approve web designs, documents, and marketing assets within Adobe Workfront.",
        "description": "Accelerate creative review cycles and collaborate across enterprise marketing teams.\n\n• Add point-and-click comments and visual annotations directly on web layouts\n• Review responsive views across mobile and desktop breakpoints\n• Submit decision approvals and manage tasks inside Adobe Workfront seamlessly.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#1D4ED8\"/><circle cx=\"24\" cy=\"24\" r=\"13\" stroke=\"#93C5FD\" stroke-width=\"2.5\" fill=\"none\"/><path d=\"M17 24L22 29L31 19\" stroke=\"#FFFFFF\" stroke-width=\"3\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "tabs",
            "activeTab"
        ],
        "size": "5.1 MB",
        "updated": "August 2026"
    },
    {
        "id": "jjkfbhajlmoeegbjgjipliamplidmbjb",
        "name": "Marketo Insights for Google",
        "developer": "Adobe Marketo",
        "version": "2.1.6",
        "category": "productivity",
        "categoryLabel": "Productivity",
        "rating": 4.6,
        "reviewCount": "2.9K",
        "users": "80,000+",
        "badge": "Verified",
        "shortDescription": "View Marketo Engage prospect intelligence, activity logs, and account insights directly within Google search and Gmail.",
        "description": "Marketo Insights puts actionable B2B marketing data at your fingertips.\n\n• Instant insight into account score, lead stage, and engagement history\n• View recent email clicks, web visits, and form submissions\n• Sync sales outreach directly with Adobe Marketo Engage campaigns.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#5C2D91\"/><path d=\"M16 32V20L24 14L32 20V32L24 26L16 32Z\" fill=\"#FFFFFF\"/><circle cx=\"24\" cy=\"20\" r=\"3\" fill=\"#F59E0B\"/></svg>",
        "featured": false,
        "permissions": [
            "storage",
            "*://mail.google.com/*"
        ],
        "size": "2.9 MB",
        "updated": "July 2026"
    },
    {
        "id": "ijliejcnnlephngnfmfefepancofbbom",
        "name": "Spring Flowers - Floral Theme & Backgrounds",
        "developer": "Chrome Themes Design",
        "version": "3.0.4",
        "category": "appearance",
        "categoryLabel": "Appearance",
        "rating": 4.8,
        "reviewCount": "16K",
        "users": "600,000+",
        "badge": "Popular",
        "shortDescription": "A beautiful, fresh high-resolution spring floral theme and wallpaper bringing vibrant colors and peaceful nature to your browser.",
        "description": "Personalize your browser with a soothing bouquet of fresh spring blossoms.\n\n• High-definition floral aesthetic background on new tabs\n• Elegant pastel color palette for tabs, toolbar, and menus\n• Optimized for light and dark modes with high visual clarity.",
        "svgIcon": "<svg viewBox=\"0 0 48 48\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\"><rect width=\"48\" height=\"48\" rx=\"12\" fill=\"#FDF2F8\"/><circle cx=\"24\" cy=\"18\" r=\"6\" fill=\"#F472B6\"/><circle cx=\"30\" cy=\"24\" r=\"6\" fill=\"#F472B6\"/><circle cx=\"24\" cy=\"30\" r=\"6\" fill=\"#F472B6\"/><circle cx=\"18\" cy=\"24\" r=\"6\" fill=\"#F472B6\"/><circle cx=\"24\" cy=\"24\" r=\"4\" fill=\"#FACC15\"/></svg>",
        "featured": false,
        "permissions": [
            "storage"
        ],
        "size": "4.2 MB",
        "updated": "September 2026"
    },
    {
        "id": "chhjbpecpncaggjpdakmflnfcopglcmi",
        "name": "Rakuten: Get Cash Back For Shopping",
        "description": "Earn Cash Back at over 3,500+ stores with automatic coupon codes, deals, and shopping rewards automatically applied.",
        "category": "Shopping",
        "rating": 4.8,
        "reviews": "82,400+",
        "users": "10,000,000+",
        "author": "Rakuten",
        "version": "6.12.0",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#7A1AC2'/><path d='M14 12h11c5.5 0 9 3 9 7.5 0 3.8-2.6 6.3-6.5 7.1L35 36h-6.2l-6.8-8.8H19.5V36H14V12zm5.5 4.8v6.2h5c2.6 0 4.2-1.2 4.2-3.1 0-1.9-1.6-3.1-4.2-3.1h-5z' fill='#ffffff'/></svg>",
        "features": [
            "Automatic Cash Back activation at 3,500+ stores",
            "Auto-applies best coupon codes at checkout",
            "Stack coupons with cash back for maximum savings",
            "Cash payout via PayPal or check"
        ]
    },
    {
        "id": "jjfblogammkiefalfpafidabbnamoknm",
        "name": "RetailMeNot: Deal Finder & Cash Back",
        "description": "Automatically test and apply coupon codes and earn cash back while shopping online at thousands of your favorite retailers.",
        "category": "Shopping",
        "rating": 4.7,
        "reviews": "15,800+",
        "users": "2,000,000+",
        "author": "RetailMeNot",
        "version": "4.8.2",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#E02947'/><path d='M16 14h16c1.1 0 2 .9 2 2v18l-10-5-10 5V16c0-1.1.9-2 2-2z' fill='#ffffff'/><circle cx='24' cy='22' r='3' fill='#E02947'/></svg>",
        "features": [
            "One-click coupon finder and promo code testing",
            "Cash Back savings on thousands of brands",
            "Exclusive shopping deals and discounts",
            "Instant checkout notifications"
        ]
    },
    {
        "id": "fllaojicojecljbmefodhfapmkghcbnh",
        "name": "Google Analytics Opt-out Add-on",
        "description": "Tells Google Analytics JavaScript (ga.js, analytics.js, gtag.js) not to send information about your visit to Google Analytics.",
        "category": "Privacy & Security",
        "rating": 4.3,
        "reviews": "1,250+",
        "users": "10,000,000+",
        "author": "Google LLC",
        "version": "1.0.8",
        "badge": "Essential",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#1A73E8'/><circle cx='24' cy='24' r='14' fill='none' stroke='#ffffff' stroke-width='3'/><path d='M14 14l20 20' stroke='#ffffff' stroke-width='3'/><path d='M20 28h2v-6h-2zm6 0h2v-10h-2z' fill='#ffffff' opacity='0.7'/></svg>",
        "features": [
            "Blocks Google Analytics tracking scripts across all websites",
            "Maintains complete browsing privacy from analytics trackers",
            "Zero configuration required – works automatically",
            "Officially developed and maintained by Google"
        ]
    },
    {
        "id": "gbkeegbaiigmenfmjfclcdgdpimamgkj",
        "name": "Office Editing for Docs, Sheets & Slides",
        "description": "View and edit Microsoft Word, Excel, and PowerPoint files with Google Docs, Sheets, and Slides directly in your browser without MS Office installed.",
        "category": "Productivity",
        "rating": 4,
        "reviews": "5,300+",
        "users": "10,000,000+",
        "author": "Google LLC",
        "version": "1.4.15",
        "badge": "Essential",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#0288D1'/><path d='M14 11h14l8 8v18c0 1.1-.9 2-2 2H14c-1.1 0-2-.9-2-2V13c0-1.1.9-2 2-2zm12 2v6h6M17 25h14M17 29h14M17 33h9' stroke='#ffffff' stroke-width='2.2' stroke-linecap='round' stroke-linejoin='round' fill='none'/></svg>",
        "features": [
            "Open Word (.docx, .doc), Excel (.xlsx, .xls), PowerPoint (.pptx, .ppt)",
            "Edit directly inside Google Docs without converting",
            "Save back to original Office formats",
            "Drag and drop documents directly into browser tabs"
        ]
    },
    {
        "id": "mclkkofklkfljcocdinagocijmpgbhab",
        "name": "Google Input Tools",
        "description": "Type in the language of your choice using virtual keyboards, IMEs, and handwriting input in over 90 languages with ease.",
        "category": "Productivity",
        "rating": 4.4,
        "reviews": "15,200+",
        "users": "10,000,000+",
        "author": "Google LLC",
        "version": "7.0.0",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#4285F4'/><path d='M12 16h24c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H12c-1.1 0-2-.9-2-2V18c0-1.1.9-2 2-2zm3 4v2h2v-2zm4 0v2h2v-2zm4 0v2h2v-2zm4 0v2h2v-2zm4 0v2h2v-2zm-16 4v2h2v-2zm4 0v2h2v-2zm4 0v2h2v-2zm4 0v2h2v-2zm4 0v2h2v-2zm-13 4v2h12v-2z' fill='#ffffff'/></svg>",
        "features": [
            "Virtual keyboards for 90+ international languages",
            "Phonetic transliteration (type Latin, get native script)",
            "Handwriting input with smart character recognition",
            "Quick keyboard shortcuts for switching languages"
        ]
    },
    {
        "id": "kejbdjndbnbjgmefkgdddjlbokphdefk",
        "name": "Tag Assistant Legacy (by Google)",
        "description": "Troubleshoot and verify installation of Google Analytics, Google Tag Manager, Ads Conversion Tracking, and remarketing tags.",
        "category": "Developer Tools",
        "rating": 4.2,
        "reviews": "2,100+",
        "users": "2,000,000+",
        "author": "Google LLC",
        "version": "4.1.3",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#009688'/><path d='M34.4 22.6L25.4 13.6c-.4-.4-.9-.6-1.4-.6H14c-1.1 0-2 .9-2 2v10c0 .5.2 1 .6 1.4l9 9c.4.4.9.6 1.4.6.5 0 1-.2 1.4-.6l10-10c.4-.4.6-.9.6-1.4 0-.5-.2-1-.6-1.4zM18 19c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2z' fill='#ffffff'/></svg>",
        "features": [
            "Diagnoses Google tags and scripts on any webpage",
            "Reports tag errors, warnings, and implementation suggestions",
            "Records browsing sessions to trace analytics hits",
            "Verifies Google Ads conversion and remarketing setup"
        ]
    },
    {
        "id": "akimgimeeoiognljlfchpbkpfbmeapkh",
        "name": "Google Arts & Culture",
        "description": "Discover masterpieces, cultural treasures, and historic landmarks from world-class museums every time you open a new browser tab.",
        "category": "Lifestyle",
        "rating": 4.4,
        "reviews": "3,600+",
        "users": "1,000,000+",
        "author": "Google LLC",
        "version": "2.4.0",
        "badge": "Featured",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#E91E63'/><path d='M12 36h24v3H12zm12-24l13 7H11zm-9 9h3v12h-3zm6 0h3v12h-3zm6 0h3v12h-3zm6 0h3v12h-3z' fill='#ffffff'/></svg>",
        "features": [
            "Displays world-famous artwork on every new tab",
            "Explore detailed stories and historical context",
            "Zoom into high-resolution brushstrokes and artifacts",
            "Curated collections from over 2,000 global museums"
        ]
    },
    {
        "id": "iklnnbgdcppplombffihcijanngoeifm",
        "name": "Google Vids Screen Recording",
        "description": "Record your screen, camera, and microphone directly to Google Vids for seamless video storytelling, presentations, and team collaboration.",
        "category": "Productivity",
        "rating": 4.6,
        "reviews": "1,850+",
        "users": "1,000,000+",
        "author": "Google LLC",
        "version": "1.2.0",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#7C4DFF'/><circle cx='24' cy='24' r='10' fill='#FF5252'/><circle cx='24' cy='24' r='5' fill='#ffffff'/><rect x='11' y='11' width='26' height='26' rx='4' fill='none' stroke='#ffffff' stroke-width='2.5'/></svg>",
        "features": [
            "High-definition screen capture with system audio",
            "Camera face-cam overlay with flexible positioning",
            "Direct integration with Google Vids and Google Drive",
            "Instant sharing link generation with access controls"
        ]
    },
    {
        "id": "gmbmikajjgmnabiglmofipeabaddhgne",
        "name": "Save to Google Drive",
        "description": "Save web content, full web pages, screen captures, and downloadable files directly to Google Drive with a single click.",
        "category": "Productivity",
        "rating": 4.1,
        "reviews": "6,600+",
        "users": "4,000,000+",
        "author": "Google Drive Team",
        "version": "3.0.4",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#0F9D58'/><path d='M19.5 15h9l8.5 15H28zm-2.5 1.5L9.5 30 14 38h15zm3 17.5h18l-4.5 8H15.5z' fill='#ffffff'/></svg>",
        "features": [
            "Save whole webpage as MHTML, PDF, or PNG image",
            "Save links, images, and audio/video files directly to Drive",
            "Quick folder destination chooser and rename prompt",
            "Auto-converts saved documents to Google Docs format if desired"
        ]
    },
    {
        "id": "iodihamcpbpeioajjeobimgagajmlibd",
        "name": "Secure Shell (SSH & SFTP)",
        "description": "Full-featured xterm-compatible terminal emulator and SSH client right in your browser tab, powered by hterm and OpenSSH.",
        "category": "Developer Tools",
        "rating": 4.3,
        "reviews": "1,220+",
        "users": "2,000,000+",
        "author": "Chromium OS Authors",
        "version": "0.58.0",
        "badge": "Popular",
        "icon": "<svg viewBox='0 0 48 48' width='48' height='48'><rect width='48' height='48' rx='10' fill='#212121'/><path d='M14 18l7 6-7 6M23 30h11' stroke='#00E676' stroke-width='3' stroke-linecap='round' stroke-linejoin='round' fill='none'/></svg>",
        "features": [
            "Full VT100 / xterm terminal emulation with UTF-8 support",
            "Direct SSH connect without needing local proxy",
            "SFTP client for secure file transfer and mounting",
            "Configurable color schemes, fonts, and keybindings"
        ]
    }
];

let allExtensions = [...EXTENSIONS_CATALOG];
let filteredExtensions = [...allExtensions];
let activeCategory = 'all';
let currentSearchTerm = '';
let currentSort = 'featured';
let installedExtensionIds = new Set();
let activeModalExtension = null;

// Star SVG Icon Template
const STAR_SVG = `<svg viewBox="0 0 24 24"><path d="M12 17.27L18.18 21L16.54 13.97L22 9.24L14.81 8.63L12 2L9.19 8.63L2 9.24L7.46 13.97L5.82 21L12 17.27Z"/></svg>`;

// Verified Badge SVG Template
const VERIFIED_SVG = `<svg class="verified-badge-icon" viewBox="0 0 24 24" title="Verified Publisher"><path d="M12 1L3 5V11C3 16.55 6.84 21.74 12 23C17.16 21.74 21 16.55 21 11V5L12 1ZM10 16L6 12L7.41 10.59L10 13.17L16.59 6.58L18 8L10 16Z"/></svg>`;

document.addEventListener('DOMContentLoaded', async () => {
  initTheme();
  loadSavedInstalledState();
  await loadCatalog();
  setupEventListeners();
  renderExtensions();
  requestLiveExtensionSync();
});

// Load Catalog with guaranteed instant fallback
async function loadCatalog() {
  allExtensions = [...EXTENSIONS_CATALOG];
  filteredExtensions = [...allExtensions];

  try {
    const res = await fetch('extensions.json');
    if (res.ok) {
      const data = await res.json();
      if (Array.isArray(data) && data.length > 0) {
        allExtensions = data;
        filteredExtensions = [...allExtensions];
      }
    }
  } catch (e) {
    // Keep instant embedded catalog
  }
}

// Setup Event Listeners
function setupEventListeners() {
  const searchInput = document.getElementById('storeSearchInput');
  const searchClearBtn = document.getElementById('searchClearBtn');

  if (searchInput) {
    searchInput.addEventListener('input', (e) => {
      currentSearchTerm = e.target.value.trim().toLowerCase();
      if (searchClearBtn) {
        searchClearBtn.style.display = currentSearchTerm ? 'inline-flex' : 'none';
      }
      applyFilters();
    });
  }

  if (searchClearBtn) {
    searchClearBtn.addEventListener('click', () => {
      searchInput.value = '';
      currentSearchTerm = '';
      searchClearBtn.style.display = 'none';
      searchInput.focus();
      applyFilters();
    });
  }

  // Category Chips
  const chips = document.querySelectorAll('.category-chip');
  chips.forEach(chip => {
    chip.addEventListener('click', () => {
      chips.forEach(c => c.classList.remove('active'));
      chip.classList.add('active');
      activeCategory = chip.dataset.category;
      applyFilters();
    });
  });

  // Sort Select
  const sortSelect = document.getElementById('sortSelect');
  if (sortSelect) {
    sortSelect.addEventListener('change', (e) => {
      currentSort = e.target.value;
      applyFilters();
    });
  }

  // Theme Toggle Button
  const themeToggleBtn = document.getElementById('themeToggleBtn');
  if (themeToggleBtn) {
    themeToggleBtn.addEventListener('click', toggleTheme);
  }

  // Manage Extensions Button
  const manageExtBtn = document.getElementById('manageExtBtn');
  if (manageExtBtn) {
    manageExtBtn.addEventListener('click', openExtensionsManager);
  }

  // Modal Close buttons
  const modalCloseBtn = document.getElementById('modalCloseBtn');
  const modalOverlay = document.getElementById('modalOverlay');
  if (modalCloseBtn) {
    modalCloseBtn.addEventListener('click', closeModal);
  }
  if (modalOverlay) {
    modalOverlay.addEventListener('click', (e) => {
      if (e.target === modalOverlay) closeModal();
    });
  }

  // Modal Tab Buttons
  const modalTabBtns = document.querySelectorAll('.modal-nav-tab');
  modalTabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      modalTabBtns.forEach(b => b.classList.remove('active'));
      document.querySelectorAll('.modal-tab-pane').forEach(c => c.classList.remove('active'));
      btn.classList.add('active');
      const targetId = btn.dataset.tab;
      const targetContent = document.getElementById(targetId);
      if (targetContent) targetContent.classList.add('active');
    });
  });

  window.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      closeModal();
      closeCustomInstallerModal();
    }
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      searchInput?.focus();
    }
  });
}

function applyFilters() {
  filteredExtensions = allExtensions.filter(ext => {
    const categoryMatch = (activeCategory === 'all') || (ext.category === activeCategory);
    let searchMatch = true;
    if (currentSearchTerm) {
      const searchTarget = `${ext.name} ${ext.developer} ${ext.categoryLabel} ${ext.shortDescription} ${ext.description}`.toLowerCase();
      searchMatch = searchTarget.includes(currentSearchTerm);
    }
    return categoryMatch && searchMatch;
  });

  if (currentSort === 'rating') {
    filteredExtensions.sort((a, b) => b.rating - a.rating);
  } else if (currentSort === 'name') {
    filteredExtensions.sort((a, b) => a.name.localeCompare(b.name));
  } else if (currentSort === 'popular') {
    filteredExtensions.sort((a, b) => parseUserCount(b.users) - parseUserCount(a.users));
  } else {
    filteredExtensions.sort((a, b) => (b.featured ? 1 : 0) - (a.featured ? 1 : 0));
  }

  renderExtensions();
}

function parseUserCount(str) {
  if (!str) return 0;
  const num = parseInt(str.replace(/[^0-9]/g, ''), 10);
  return isNaN(num) ? 0 : num;
}

// Render Chrome Web Store Tiles Grid
function renderExtensions() {
  const grid = document.getElementById('extensionsGrid');
  const countLabel = document.getElementById('resultsCount');

  if (countLabel) {
    countLabel.innerHTML = `Recommended extensions (${filteredExtensions.length})`;
  }

  if (!grid) return;
  grid.innerHTML = '';

  if (filteredExtensions.length === 0) {
    grid.innerHTML = `
      <div style="grid-column: 1 / -1; text-align: center; padding: 48px 20px; background: var(--bg-surface); border: 1px solid var(--border-color); border-radius: var(--radius-lg);">
        <h3 style="font-size: 1.15rem; font-weight: 600; margin-bottom: 6px;">No extensions found</h3>
        <p style="font-size: 0.9rem; color: var(--text-secondary); margin-bottom: 16px;">Try searching for a different keyword or category.</p>
        <button class="btn-header-action btn-header-secondary" style="margin: 0 auto;" onclick="resetFilters()">Reset search</button>
      </div>
    `;
    return;
  }

  filteredExtensions.forEach(ext => {
    const isInstalled = installedExtensionIds.has(ext.id);
    const tile = document.createElement('div');
    tile.className = 'extension-tile';

    tile.innerHTML = `
      <div class="tile-top">
        <div class="tile-icon-wrap">
          ${ext.svgIcon || '<svg viewBox="0 0 48 48"><rect width="48" height="48" rx="12" fill="#3B82F6"/></svg>'}
        </div>
        <div class="tile-title-box">
          <h3 class="tile-name" title="${escapeHtml(ext.name)}">${escapeHtml(ext.name)}</h3>
          <div class="tile-developer">
            ${escapeHtml(ext.developer)}
            ${VERIFIED_SVG}
          </div>
        </div>
      </div>

      <div class="tile-meta-row">
        <span class="tile-stars">${ext.rating.toFixed(1)} ${STAR_SVG}</span>
        <span>(${ext.reviewCount})</span>
        <span class="tile-users">${ext.users}</span>
      </div>

      <div class="tile-desc">
        ${escapeHtml(ext.shortDescription || ext.description)}
      </div>

      <div class="tile-bottom">
        <span class="tile-category-tag">${escapeHtml(ext.categoryLabel || 'Extension')}</span>
        <button class="btn-download-primary" onclick="event.stopPropagation(); downloadExtension('${ext.id}')" title="Download ${escapeHtml(ext.name)}">
          <svg viewBox="0 0 24 24" width="15" height="15" fill="currentColor"><path d="M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z"/></svg>
          <span>Download</span>
        </button>
      </div>
    `;

    tile.addEventListener('click', () => openModal(ext.id));
    grid.appendChild(tile);
  });
}

function resetFilters() {
  const input = document.getElementById('storeSearchInput');
  if (input) input.value = '';
  currentSearchTerm = '';
  activeCategory = 'all';
  document.querySelectorAll('.category-chip').forEach((c, idx) => {
    c.classList.toggle('active', idx === 0);
  });
  applyFilters();
}

// Extension Details Modal (Chrome Web Store Page View)
function openModal(extensionId) {
  const ext = allExtensions.find(e => e.id === extensionId);
  if (!ext) return;
  activeModalExtension = ext;

  const isInstalled = installedExtensionIds.has(ext.id);

  const modalIcon = document.getElementById('modalExtIcon');
  if (modalIcon) {
    modalIcon.innerHTML = ext.svgIcon || '';
  }

  document.getElementById('modalExtTitle').innerText = ext.name;
  document.getElementById('modalExtDev').innerHTML = `${escapeHtml(ext.developer)} ${VERIFIED_SVG}`;
  document.getElementById('modalExtRating').innerText = `${ext.rating.toFixed(1)} ★ (${ext.reviewCount})`;
  document.getElementById('modalExtUsers').innerText = ext.users;
  document.getElementById('modalExtCategory').innerText = ext.categoryLabel || 'Extension';
  document.getElementById('modalExtDescription').innerText = ext.description;

  document.getElementById('modalExtVersion').innerText = `v${ext.version || '1.0'}`;
  document.getElementById('modalExtSize').innerText = ext.size || '1.2 MB';
  document.getElementById('modalExtUpdated').innerText = ext.updated || '2026';
  document.getElementById('modalExtId').innerText = ext.id;

  const permList = document.getElementById('modalPermissionsList');
  if (permList) {
    permList.innerHTML = '';
    if (ext.permissions && ext.permissions.length > 0) {
      ext.permissions.forEach(p => {
        const row = document.createElement('div');
        row.className = 'permission-row';
        row.innerHTML = `<svg style="width:16px;height:16px;fill:var(--text-tertiary)" viewBox="0 0 24 24"><path d="M12 1L3 5V11C3 16.55 6.84 21.74 12 23C17.16 21.74 21 16.55 21 11V5L12 1Z"/></svg> <code>${escapeHtml(p)}</code>`;
        permList.appendChild(row);
      });
    } else {
      permList.innerHTML = '<div class="permission-row">No special permissions requested.</div>';
    }
  }

  const modalDownloadBtn = document.getElementById('modalDownloadBtn');
  if (modalDownloadBtn) {
    modalDownloadBtn.onclick = () => downloadExtension(ext.id);
  }

  document.querySelectorAll('.modal-nav-tab').forEach((b, idx) => b.classList.toggle('active', idx === 0));
  document.querySelectorAll('.modal-tab-pane').forEach((c, idx) => c.classList.toggle('active', idx === 0));

  document.getElementById('modalOverlay').classList.add('active');
}

function closeModal() {
  document.getElementById('modalOverlay').classList.remove('active');
  activeModalExtension = null;
}

// 1-Click Install to RUHI Browser
async function installExtension(extensionId) {
  const ext = allExtensions.find(e => e.id === extensionId);
  const extName = ext ? ext.name : extensionId;

  const cardBtn = document.getElementById(`btn-card-${extensionId}`);
  const modalBtn = document.getElementById('modalInstallBtn');

  // If already installed, offer to remove it
  if (installedExtensionIds.has(extensionId)) {
    const confirmRemove = confirm(`"${extName}" is currently installed in RUHI Browser.\n\nDo you want to remove this extension?`);
    if (confirmRemove) {
      uninstallExtension(extensionId);
    }
    return;
  }

  if (window.chrome && window.chrome.webview) {
    if (cardBtn) {
      cardBtn.innerText = 'Installing...';
      cardBtn.classList.add('loading');
    }
    if (modalBtn) {
      modalBtn.innerText = 'Installing...';
      modalBtn.classList.add('loading');
    }

    showToast(`Installing "${extName}"...`);

    try {
      window.chrome.webview.postMessage(JSON.stringify({
        type: 'install_chrome_extension',
        id: ext ? ext.id : extensionId,
        name: ext ? ext.name : extensionId,
        zipUrl: ext ? (ext.zipUrl || null) : null,
        title: extName
      }));

      setTimeout(() => {
        markAsInstalled(extensionId);
        showToast(`"${extName}" was added to RUHI Browser.`);
        requestLiveExtensionSync();
      }, 1400);
    } catch (err) {
      showToast(`Error: ${err.message}`);
      if (cardBtn) {
        cardBtn.innerText = 'Add to RUHI';
        cardBtn.classList.remove('loading');
      }
      if (modalBtn) {
        modalBtn.innerText = 'Add to RUHI';
        modalBtn.classList.remove('loading');
      }
    }
  } else {
    showToast(`Add-on "${extName}" ready. Open RUHI Browser to install in 1 click.`);
    markAsInstalled(extensionId);
  }
}

function uninstallExtension(extensionId) {
  const ext = allExtensions.find(e => e.id === extensionId);
  const extName = ext ? ext.name : extensionId;

  const cardBtn = document.getElementById(`btn-card-${extensionId}`);
  const modalBtn = document.getElementById('modalInstallBtn');

  if (cardBtn) {
    cardBtn.innerText = 'Removing...';
    cardBtn.classList.add('loading');
  }
  if (modalBtn) {
    modalBtn.innerText = 'Removing...';
    modalBtn.classList.add('loading');
  }

  showToast(`Removing "${extName}"...`);

  if (window.chrome && window.chrome.webview) {
    try {
      window.chrome.webview.postMessage(JSON.stringify({
        type: 'uninstall_chrome_extension',
        id: ext ? ext.id : extensionId,
        name: ext ? ext.name : extensionId
      }));
    } catch (e) {
      showToast(`Error: ${e.message}`);
    }
  }

  setTimeout(() => {
    markAsRemoved(extensionId);
    showToast(`"${extName}" was removed from RUHI Browser.`);
    requestLiveExtensionSync();
  }, 400);
}

function markAsRemoved(extId) {
  installedExtensionIds.delete(extId);
  saveInstalledState();

  const cardBtn = document.getElementById(`btn-card-${extId}`);
  if (cardBtn) {
    cardBtn.innerText = 'Add to RUHI';
    cardBtn.classList.remove('loading', 'installed');
    cardBtn.title = 'Add to RUHI Browser';
  }

  const modalBtn = document.getElementById('modalInstallBtn');
  if (modalBtn && activeModalExtension && activeModalExtension.id === extId) {
    modalBtn.innerText = 'Add to RUHI';
    modalBtn.classList.remove('loading', 'installed');
    modalBtn.title = 'Add to RUHI Browser';
  }
}

function markAsInstalled(extId) {
  installedExtensionIds.add(extId);
  saveInstalledState();

  const cardBtn = document.getElementById(`btn-card-${extId}`);
  if (cardBtn) {
    cardBtn.innerText = '✓ Added';
    cardBtn.classList.remove('loading');
    cardBtn.classList.add('installed');
    cardBtn.title = 'Installed in RUHI Browser (click to remove)';
  }

  const modalBtn = document.getElementById('modalInstallBtn');
  if (modalBtn && activeModalExtension && activeModalExtension.id === extId) {
    modalBtn.innerText = '✓ Added to RUHI';
    modalBtn.classList.remove('loading');
    modalBtn.classList.add('installed');
    modalBtn.title = 'Installed in RUHI Browser (click to remove)';
  }
}

function openExtensionsManager(event) {
  if (event && event.preventDefault) {
    event.preventDefault();
  }
  if (window.chrome && window.chrome.webview) {
    try {
      window.chrome.webview.postMessage(JSON.stringify({
        type: 'open_extensions_manager'
      }));
      return;
    } catch (e) {}
  }
  window.location.href = 'ruhi://extension';
}

function openCustomInstallerModal() {
  document.getElementById('customModalOverlay').classList.add('active');
  const input = document.getElementById('customExtInput');
  if (input) {
    input.value = '';
    input.focus();
  }
}

function closeCustomInstallerModal() {
  document.getElementById('customModalOverlay').classList.remove('active');
}

function handleCustomInstallSubmit() {
  const input = document.getElementById('customExtInput');
  const val = input ? input.value.trim() : '';
  if (!val) {
    showToast('Please enter an Extension ID.');
    return;
  }

  const match = val.match(/([a-z]{32})/i);
  if (!match) {
    showToast('Invalid Extension ID. Must be 32 characters.');
    return;
  }

  const extId = match[1].toLowerCase();
  closeCustomInstallerModal();
  installExtension(extId);
}

function handleCustomDownloadSubmit() {
  const input = document.getElementById('customExtInput');
  const val = input ? input.value.trim() : '';
  if (!val) {
    showToast('Please enter an Extension ID.');
    return;
  }

  const match = val.match(/([a-z]{32})/i);
  if (!match) {
    showToast('Invalid Extension ID. Must be 32 characters.');
    return;
  }

  const extId = match[1].toLowerCase();
  closeCustomInstallerModal();
  downloadExtension(extId);
}

function getExtensionDownloadUrl(ext) {
  if (!ext) return null;
  if (ext.zipUrl) return ext.zipUrl;
  return `https://clients2.google.com/service/update2/crx?response=redirect&os=win&arch=x86-64&os_arch=x86-64&nacl_arch=x86-64&prod=chromecrx&prodchannel=&prodversion=9999.0.9999.0&lang=en-US&acceptformat=crx2,crx3&x=id%3D${ext.id}%26installsource%3Dondemand%26uc`;
}

function downloadExtension(extensionId) {
  let ext = allExtensions.find(e => e.id === extensionId);
  if (!ext) {
    ext = {
      id: extensionId,
      name: `Extension_${extensionId.substring(0, 8)}`,
      zipUrl: null
    };
  }

  const url = getExtensionDownloadUrl(ext);
  const safeName = (ext.name || ext.id).replace(/[^a-zA-Z0-9_\-]/g, '_');
  const filename = ext.zipUrl ? `${safeName}.zip` : `${safeName}.crx`;

  showToast(`Downloading "${ext.name}"... Check RUHI Downloads Hub (Ctrl+J)`);

  // 1. Send IPC message to RUHI Browser native download manager
  if (window.chrome && window.chrome.webview) {
    try {
      window.chrome.webview.postMessage(JSON.stringify({
        type: 'download_file',
        url: url,
        filename: filename,
        name: ext.name
      }));
      return; // Handled natively by RUHI Browser; do not open a blank browser tab
    } catch (e) {}
  }

  // 2. Also trigger standard browser download via hidden anchor
  try {
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.target = '_blank';
    document.body.appendChild(a);
    a.click();
    setTimeout(() => a.remove(), 250);
  } catch (e) {
    window.open(url, '_blank');
  }
}

function downloadActiveModalExtension() {
  if (activeModalExtension) {
    downloadExtension(activeModalExtension.id);
  }
}

function showToast(message) {
  let shelf = document.getElementById('toastShelf');
  if (!shelf) {
    shelf = document.createElement('div');
    shelf.id = 'toastShelf';
    shelf.className = 'toast-shelf';
    document.body.appendChild(shelf);
  }

  const snackbar = document.createElement('div');
  snackbar.className = 'snackbar';
  snackbar.innerHTML = `<span>${escapeHtml(message)}</span>`;

  shelf.appendChild(snackbar);

  setTimeout(() => {
    snackbar.style.opacity = '0';
    snackbar.style.transition = 'opacity 0.2s ease';
    setTimeout(() => snackbar.remove(), 200);
  }, 3200);
}

function initTheme() {
  const saved = localStorage.getItem('ruhi_store_theme');
  const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
  const theme = saved || (prefersDark ? 'dark' : 'light');
  document.documentElement.setAttribute('data-theme', theme);
  updateThemeIcon(theme);
}

function toggleTheme() {
  const current = document.documentElement.getAttribute('data-theme') || 'dark';
  const next = current === 'dark' ? 'light' : 'dark';
  document.documentElement.setAttribute('data-theme', next);
  localStorage.setItem('ruhi_store_theme', next);
  updateThemeIcon(next);
}

function updateThemeIcon(theme) {
  const icon = document.getElementById('themeToggleIcon');
  if (icon) {
    icon.innerHTML = theme === 'dark'
      ? `<svg viewBox="0 0 24 24"><path d="M12 7C9.24 7 7 9.24 7 12C7 14.76 9.24 17 12 17C14.76 17 17 14.76 17 12C17 9.24 14.76 7 12 7ZM12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"/></svg>`
      : `<svg viewBox="0 0 24 24"><path d="M12 3C7.03 3 3 7.03 3 12C3 16.97 7.03 21 12 21C16.97 21 21 16.97 21 12C21 11.54 20.96 11.08 20.89 10.64C19.92 11.5 18.63 12 17.22 12C13.9 12 11.22 9.32 11.22 6C11.22 4.59 11.72 3.3 12.58 2.33C12.38 2.22 12.19 2.11 12 2V3Z"/></svg>`;
  }
}

function loadSavedInstalledState() {
  try {
    const raw = localStorage.getItem('ruhi_installed_extensions');
    if (raw) {
      installedExtensionIds = new Set(JSON.parse(raw));
    }
  } catch (e) {}
}

function saveInstalledState() {
  try {
    localStorage.setItem('ruhi_installed_extensions', JSON.stringify([...installedExtensionIds]));
  } catch (e) {}
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/[&<>'"]/g, tag => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    "'": '&#39;',
    '"': '&quot;'
  }[tag] || tag));
}

// ============================================================================
// LIVE SYNCHRONIZATION WITH RUHI BROWSER
// ============================================================================
window.syncInstalledExtensions = function(liveList) {
  if (!Array.isArray(liveList)) return;

  const liveInstalledIds = new Set();

  allExtensions.forEach(ext => {
    const extId = (ext.id || '').toLowerCase();
    const extName = (ext.name || '').toLowerCase().trim();

    const isInstalled = liveList.some(item => {
      if (!item) return false;
      const itemId = (item.id || '').toLowerCase();
      const itemName = (item.name || '').toLowerCase().trim();
      const itemFolder = (item.folder || '').toLowerCase();

      // Check ID match or folder match
      if (extId && (itemId === extId || itemFolder === extId || itemFolder.includes(extId))) {
        return true;
      }
      // Check Name match
      if (extName && itemName) {
        if (itemName === extName) return true;
        if (itemName.startsWith(extName) || extName.startsWith(itemName)) return true;
      }
      return false;
    });

    if (isInstalled) {
      liveInstalledIds.add(ext.id);
    }
  });

  // Reconcile accurately with actual browser state
  installedExtensionIds = liveInstalledIds;
  saveInstalledState();
  updateAllExtensionButtons();
};

function updateAllExtensionButtons() {
  allExtensions.forEach(ext => {
    const isInstalled = installedExtensionIds.has(ext.id);
    const cardBtn = document.getElementById(`btn-card-${ext.id}`);
    if (cardBtn) {
      if (isInstalled) {
        cardBtn.innerText = '✓ Added';
        cardBtn.classList.add('installed');
        cardBtn.classList.remove('loading');
        cardBtn.title = 'Installed in RUHI Browser (click to remove)';
      } else {
        cardBtn.innerText = 'Add to RUHI';
        cardBtn.classList.remove('installed', 'loading');
        cardBtn.title = 'Add to RUHI Browser';
      }
    }
  });

  if (activeModalExtension) {
    const isInstalled = installedExtensionIds.has(activeModalExtension.id);
    const modalBtn = document.getElementById('modalInstallBtn');
    if (modalBtn) {
      if (isInstalled) {
        modalBtn.innerText = '✓ Added to RUHI';
        modalBtn.classList.add('installed');
        modalBtn.classList.remove('loading');
        modalBtn.title = 'Installed in RUHI Browser (click to remove)';
      } else {
        modalBtn.innerText = 'Add to RUHI';
        modalBtn.classList.remove('installed', 'loading');
        modalBtn.title = 'Add to RUHI Browser';
      }
    }
  }
}

function requestLiveExtensionSync() {
  if (window.chrome && window.chrome.webview) {
    try {
      window.chrome.webview.postMessage(JSON.stringify({
        type: 'get_installed_extensions'
      }));
    } catch (e) {}
  }
}

window.requestExtensionSync = requestLiveExtensionSync;

window.addEventListener('focus', () => {
  requestLiveExtensionSync();
});

document.addEventListener('visibilitychange', () => {
  if (!document.hidden) {
    requestLiveExtensionSync();
  }
});

if (window.chrome && window.chrome.webview) {
  window.chrome.webview.addEventListener('message', (event) => {
    try {
      const data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
      if (data && data.type === 'sync_installed_extensions' && Array.isArray(data.extensions)) {
        window.syncInstalledExtensions(data.extensions);
      }
    } catch (e) {}
  });
}

// Initial sync call
requestLiveExtensionSync();

