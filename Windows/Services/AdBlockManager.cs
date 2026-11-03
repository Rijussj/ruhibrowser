using System;
using System.Collections.Generic;

namespace RuhiBrowser.Services
{
    public static class AdBlockManager
    {
        public static bool IsEnabled { get; set; } = true;
        public static int TotalBlockedCount { get; set; } = 0;
        public static readonly HashSet<string> AllowedDomains = new(StringComparer.OrdinalIgnoreCase);

        public static bool IsDomainAllowed(string? domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return false;
            string clean = domain.Trim().ToLowerInvariant();
            if (clean.StartsWith("www.")) clean = clean.Substring(4);
            return AllowedDomains.Contains(clean);
        }

        public static void SetDomainAllowed(string? domain, bool allowed)
        {
            if (string.IsNullOrWhiteSpace(domain)) return;
            string clean = domain.Trim().ToLowerInvariant();
            if (clean.StartsWith("www.")) clean = clean.Substring(4);
            if (allowed)
            {
                AllowedDomains.Add(clean);
            }
            else
            {
                AllowedDomains.Remove(clean);
            }
        }

        private static readonly HashSet<string> KnownAdDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "doubleclick.net",
            "googleadservices.com",
            "googlesyndication.com",
            "adservice.google.com",
            "pagead2.googlesyndication.com",
            "adnxs.com",
            "advertising.com",
            "scorecardresearch.com",
            "criteo.com",
            "criteo.net",
            "outbrain.com",
            "taboola.com",
            "amazon-adsystem.com",
            "popads.net",
            "propellerads.com",
            "adcolony.com",
            "admob.com",
            "unityads.unity3d.com",
            "ads.yahoo.com",
            "analytics.google.com",
            "google-analytics.com",
            "hotjar.com",
            "mixpanel.com",
            "chartbeat.com",
            "quantserve.com",
            "pubmatic.com",
            "rubiconproject.com",
            "casalemedia.com",
            "smartadserver.com",
            "openx.net",
            "applovin.com",
            "inmobi.com",
            "vungle.com",
            "trafficjunky.com",
            "revcontent.com",
            "mgid.com"
        };

        public static bool ShouldBlock(Uri? uri, string? currentSiteHost = null)
        {
            if (!IsEnabled || uri == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(currentSiteHost) && IsDomainAllowed(currentSiteHost))
            {
                return false;
            }

            var host = uri.Host;
            if (string.IsNullOrWhiteSpace(host) || IsDomainAllowed(host))
            {
                return false;
            }

            foreach (var domain in KnownAdDomains)
            {
                if (host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                    host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
