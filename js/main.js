document.addEventListener('DOMContentLoaded', () => {
  // 1. FAQ Accordion Logic
  const faqItems = document.querySelectorAll('.faq-item');
  faqItems.forEach(item => {
    const question = item.querySelector('.faq-question');
    if (question) {
      question.addEventListener('click', () => {
        const isActive = item.classList.contains('active');
        faqItems.forEach(i => i.classList.remove('active'));
        if (!isActive) {
          item.classList.add('active');
        }
      });
    }
  });

  // 2. Smooth Scrolling for Anchor Links
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
      const targetId = this.getAttribute('href');
      if (targetId && targetId !== '#') {
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
          e.preventDefault();
          targetElement.scrollIntoView({ behavior: 'smooth' });
        }
      }
    });
  });

  // 3. Current year in footer
  const yearSpan = document.getElementById('current-year');
  if (yearSpan) {
    yearSpan.textContent = new Date().getFullYear();
  }

  // 4. Automatic OS Detection & Platform Isolation
  const detectedOS = detectClientOS();
  setPlatformView(detectedOS);

  // 5. Mobile Hamburger Navigation Toggle
  const navToggle = document.getElementById('nav-toggle');
  const navLinks = document.querySelector('.nav-links');
  if (navToggle && navLinks) {
    navToggle.addEventListener('click', (e) => {
      e.stopPropagation();
      navLinks.classList.toggle('is-open');
    });

    navLinks.querySelectorAll('a').forEach(link => {
      link.addEventListener('click', () => {
        navLinks.classList.remove('is-open');
      });
    });

    document.addEventListener('click', (e) => {
      if (!navLinks.contains(e.target) && !navToggle.contains(e.target)) {
        navLinks.classList.remove('is-open');
      }
    });
  }
});

// OS Detection Function
function detectClientOS() {
  const ua = navigator.userAgent || navigator.vendor || window.opera || '';
  const platform = navigator.platform || '';

  if (/android/i.test(ua)) return 'android';
  if (/iPad|iPhone|iPod/.test(ua) && !window.MSStream) return 'android';
  if (/Macintosh|MacIntel|MacPPC|Mac68K|Mac OS X/i.test(ua) || /Mac/i.test(platform)) return 'mac';
  if (/Win32|Win64|Windows|WinCE/i.test(ua) || /Win/i.test(platform)) return 'windows';
  return 'windows';
}

// Switch Platform Presentation Dynamically (Complete Separation by OS)
window.setPlatformView = function(os) {
  if (!['windows', 'android', 'mac'].includes(os)) {
    os = 'windows';
  }

  // 1. Set dataset on body (triggers CSS isolation instantaneously)
  document.body.dataset.platform = os;

  // 2. Sync All Platform Tabs
  document.querySelectorAll('.platform-tab-btn').forEach(btn => {
    btn.classList.toggle('active', btn.getAttribute('data-os') === os);
  });
};
