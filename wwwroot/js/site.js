// TechZone — interface interactions
(function () {
  'use strict';

  var reduceMotion = window.matchMedia
    ? window.matchMedia('(prefers-reduced-motion: reduce)').matches
    : false;

  /* ----------------------------------------------------------
     Product image fallback (missing/corrupted assets)
     ---------------------------------------------------------- */
  var FALLBACK_IMAGE = '/images/placeholder-tech.svg';

  var applyFallback = function (img) {
    if (
      img &&
      img.tagName === 'IMG' &&
      !img.dataset.tzFallback &&
      img.src.indexOf(FALLBACK_IMAGE) === -1
    ) {
      img.dataset.tzFallback = '1';
      img.src = FALLBACK_IMAGE;
    }
  };

  document.addEventListener(
    'error',
    function (event) {
      applyFallback(event.target);
    },
    true
  );

  // Catch images that failed before this script was parsed.
  window.addEventListener('load', function () {
    Array.prototype.forEach.call(document.images, function (img) {
      if (img.complete && img.naturalWidth === 0) {
        applyFallback(img);
      }
    });
  });

  /* ----------------------------------------------------------
     Announcement bar rotation
     ---------------------------------------------------------- */
  var track = document.querySelector('[data-announcement]');
  if (track) {
    var messages = Array.prototype.slice.call(
      track.querySelectorAll('.tz-announcement-msg')
    );

    if (messages.length > 1 && !reduceMotion) {
      var index = 0;
      var paused = false;

      var show = function (next) {
        messages[index].classList.remove('is-active');
        index = next % messages.length;
        messages[index].classList.add('is-active');
      };

      var timer = window.setInterval(function () {
        if (!paused) {
          show(index + 1);
        }
      }, 4200);

      var bar = track.closest('.tz-announcement');
      if (bar) {
        bar.addEventListener('mouseenter', function () { paused = true; });
        bar.addEventListener('mouseleave', function () { paused = false; });
        bar.addEventListener('focusin', function () { paused = true; });
        bar.addEventListener('focusout', function () { paused = false; });
      }

      document.addEventListener('visibilitychange', function () {
        paused = document.hidden;
      });

      window.addEventListener('pagehide', function () {
        window.clearInterval(timer);
      });
    }
  }

  /* ----------------------------------------------------------
     Header shadow on scroll
     ---------------------------------------------------------- */
  var header = document.querySelector('[data-header]');
  if (header) {
    var onScroll = function () {
      if (window.scrollY > 8) {
        header.classList.add('is-scrolled');
      } else {
        header.classList.remove('is-scrolled');
      }
    };
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
  }

  /* ----------------------------------------------------------
     Mobile menu
     ---------------------------------------------------------- */
  var mobile = document.querySelector('[data-mobile]');
  var toggle = document.querySelector('[data-menu-toggle]');
  var closeBtn = document.querySelector('[data-menu-close]');

  var setMenu = function (open) {
    if (!mobile || !toggle) {
      return;
    }

    if (open) {
      mobile.hidden = false;
      toggle.setAttribute('aria-expanded', 'true');
      document.body.style.overflow = 'hidden';
      if (closeBtn) {
        closeBtn.focus();
      }
    } else {
      mobile.hidden = true;
      toggle.setAttribute('aria-expanded', 'false');
      document.body.style.overflow = '';
    }
  };

  if (toggle) {
    toggle.addEventListener('click', function () {
      setMenu(mobile && mobile.hidden);
    });
  }
  if (closeBtn) {
    closeBtn.addEventListener('click', function () {
      setMenu(false);
      if (toggle) {
        toggle.focus();
      }
    });
  }
  if (mobile) {
    mobile.addEventListener('click', function (event) {
      var target = event.target;
      if (target && target.closest && target.closest('a')) {
        setMenu(false);
      }
    });
  }
  document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape' && mobile && !mobile.hidden) {
      setMenu(false);
      if (toggle) {
        toggle.focus();
      }
    }
  });

  /* ----------------------------------------------------------
     Close open <details> dropdowns on outside click / Escape
     ---------------------------------------------------------- */
  var detailsList = Array.prototype.slice.call(
    document.querySelectorAll('.tz-account, .tz-nav-drop')
  );

  if (detailsList.length) {
    document.addEventListener('click', function (event) {
      detailsList.forEach(function (details) {
        if (details.open && !details.contains(event.target)) {
          details.open = false;
        }
      });
    });
    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape') {
        detailsList.forEach(function (details) {
          details.open = false;
        });
      }
    });
  }

  /* ----------------------------------------------------------
     FAQ accordion
     ---------------------------------------------------------- */
  var faqItems = Array.prototype.slice.call(
    document.querySelectorAll('.home-faq-item')
  );

  faqItems.forEach(function (item) {
    var trigger = item.querySelector('.home-faq-trigger');
    var panel = item.querySelector('.home-faq-panel');
    if (!trigger || !panel) {
      return;
    }

    trigger.addEventListener('click', function () {
      var isOpen = item.classList.contains('is-open');

      faqItems.forEach(function (other) {
        var otherTrigger = other.querySelector('.home-faq-trigger');
        var otherPanel = other.querySelector('.home-faq-panel');
        if (other !== item && other.classList.contains('is-open')) {
          other.classList.remove('is-open');
          if (otherPanel) {
            otherPanel.style.maxHeight = '0px';
          }
          if (otherTrigger) {
            otherTrigger.setAttribute('aria-expanded', 'false');
          }
        }
      });

      if (isOpen) {
        item.classList.remove('is-open');
        panel.style.maxHeight = '0px';
        trigger.setAttribute('aria-expanded', 'false');
      } else {
        item.classList.add('is-open');
        panel.style.maxHeight = panel.scrollHeight + 'px';
        trigger.setAttribute('aria-expanded', 'true');
      }
    });
  });

  // Keep open panels sized correctly on resize.
  window.addEventListener('resize', function () {
    faqItems.forEach(function (item) {
      if (item.classList.contains('is-open')) {
        var panel = item.querySelector('.home-faq-panel');
        if (panel) {
          panel.style.maxHeight = panel.scrollHeight + 'px';
        }
      }
    });
  });
})();
