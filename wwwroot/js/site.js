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

  var lastFocused = null;
  var focusableSelector = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

  var setMenu = function (open) {
    if (!mobile || !toggle) {
      return;
    }

    if (open) {
      lastFocused = document.activeElement;
      mobile.hidden = false;
      toggle.setAttribute('aria-expanded', 'true');
      toggle.setAttribute('aria-label', 'Fermer le menu');
      document.body.style.overflow = 'hidden';
      if (closeBtn) {
        closeBtn.focus();
      }
    } else {
      mobile.hidden = true;
      toggle.setAttribute('aria-expanded', 'false');
      toggle.setAttribute('aria-label', 'Ouvrir le menu');
      document.body.style.overflow = '';
      if (lastFocused && document.contains(lastFocused) && typeof lastFocused.focus === 'function') {
        lastFocused.focus();
      } else {
        toggle.focus();
      }
      lastFocused = null;
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
    });
  }
  if (mobile) {
    mobile.addEventListener('click', function (event) {
      var target = event.target;
      if (target === mobile) {
        setMenu(false);
        return;
      }
      if (target && target.closest && target.closest('a')) {
        setMenu(false);
      }
    });
  }
  document.addEventListener('keydown', function (event) {
    if (!mobile || mobile.hidden) {
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      setMenu(false);
      return;
    }

    if (event.key !== 'Tab') {
      return;
    }

    var focusable = Array.prototype.filter.call(
      mobile.querySelectorAll(focusableSelector),
      function (element) {
        return element.getClientRects().length > 0;
      }
    );

    if (!focusable.length) {
      return;
    }

    var first = focusable[0];
    var last = focusable[focusable.length - 1];

    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  });

  if (window.matchMedia) {
    var desktopQuery = window.matchMedia('(min-width: 981px)');
    var onDesktop = function (event) {
      if (event.matches && mobile && !mobile.hidden) {
        setMenu(false);
      }
    };

    if (desktopQuery.addEventListener) {
      desktopQuery.addEventListener('change', onDesktop);
    } else if (desktopQuery.addListener) {
      desktopQuery.addListener(onDesktop);
    }
  }

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

// Catalogue — filters, view switch, favourites
(function () {
  'use strict';

  /* Price range */
  var priceTrack = document.querySelector('[data-price-track]');
  if (priceTrack) {
    var rangeMin = priceTrack.querySelector('[data-price-min]');
    var rangeMax = priceTrack.querySelector('[data-price-max]');
    var hiddenMin = document.querySelector('[data-price-min-input]');
    var hiddenMax = document.querySelector('[data-price-max-input]');
    var labelMin = document.querySelector('[data-price-min-label]');
    var labelMax = document.querySelector('[data-price-max-label]');

    if (rangeMin && rangeMax) {
      var floor = parseFloat(rangeMin.min) || 0;
      var ceil = parseFloat(rangeMin.max) || 1000;
      var span = (ceil - floor) || 1;

      var formatPrice = function (value) {
        try {
          return new Intl.NumberFormat('fr-FR', { maximumFractionDigits: 0 }).format(value);
        } catch (error) {
          return String(Math.round(value));
        }
      };

      var paintPrice = function () {
        var min = parseFloat(rangeMin.value);
        var max = parseFloat(rangeMax.value);
        var start = ((min - floor) / span) * 100;
        var end = ((max - floor) / span) * 100;

        priceTrack.style.setProperty('--fill-start', Math.max(0, Math.min(100, start)) + '%');
        priceTrack.style.setProperty('--fill-end', Math.max(0, Math.min(100, end)) + '%');

        if (hiddenMin) {
          hiddenMin.value = min;
        }
        if (hiddenMax) {
          hiddenMax.value = max;
        }
        if (labelMin) {
          labelMin.textContent = formatPrice(min);
        }
        if (labelMax) {
          labelMax.textContent = formatPrice(max);
        }
      };

      rangeMin.addEventListener('input', function () {
        if (parseFloat(rangeMin.value) > parseFloat(rangeMax.value)) {
          rangeMin.value = rangeMax.value;
        }
        paintPrice();
      });

      rangeMax.addEventListener('input', function () {
        if (parseFloat(rangeMax.value) < parseFloat(rangeMin.value)) {
          rangeMax.value = rangeMin.value;
        }
        paintPrice();
      });

      paintPrice();
    }
  }

  /* Grid / list view */
  var catalogGrid = document.querySelector('[data-catalog-grid]');
  var viewButtons = Array.prototype.slice.call(document.querySelectorAll('[data-view]'));

  var applyView = function (mode) {
    if (catalogGrid) {
      catalogGrid.classList.toggle('is-list', mode === 'list');
    }

    viewButtons.forEach(function (button) {
      var active = button.getAttribute('data-view') === mode;
      button.classList.toggle('is-active', active);
      button.setAttribute('aria-pressed', active ? 'true' : 'false');
    });
  };

  if (catalogGrid && viewButtons.length) {
    var storedView = null;
    try {
      storedView = window.localStorage.getItem('tz-catalog-view');
    } catch (error) {
      storedView = null;
    }

    if (storedView === 'list') {
      applyView('list');
    }

    viewButtons.forEach(function (button) {
      button.addEventListener('click', function () {
        var mode = button.getAttribute('data-view') === 'list' ? 'list' : 'grid';
        applyView(mode);
        try {
          window.localStorage.setItem('tz-catalog-view', mode);
        } catch (error) {
          /* ignore storage failures */
        }
      });
    });
  }

  /* Favourites (local only) */
  var wishlistButtons = Array.prototype.slice.call(document.querySelectorAll('[data-wishlist]'));

  if (wishlistButtons.length) {
    var readWishlist = function () {
      try {
        var raw = window.localStorage.getItem('tz-wishlist');
        var parsed = raw ? JSON.parse(raw) : [];
        return Array.isArray(parsed) ? parsed.map(String) : [];
      } catch (error) {
        return [];
      }
    };

    var savedItems = readWishlist();
    var persistWishlist = function () {
      try {
        window.localStorage.setItem('tz-wishlist', JSON.stringify(savedItems));
      } catch (error) {
        /* ignore storage failures */
      }
    };

    wishlistButtons.forEach(function (button) {
      var id = String(button.getAttribute('data-wishlist'));
      var active = savedItems.indexOf(id) !== -1;

      button.classList.toggle('is-saved', active);
      button.setAttribute('aria-pressed', active ? 'true' : 'false');

      button.addEventListener('click', function () {
        var index = savedItems.indexOf(id);
        var isSaved = index === -1;

        if (isSaved) {
          savedItems.push(id);
        } else {
          savedItems.splice(index, 1);
        }

        button.classList.toggle('is-saved', isSaved);
        button.setAttribute('aria-pressed', isSaved ? 'true' : 'false');
        persistWishlist();
      });
    });
  }

  /* Sorting */
  var sortSelect = document.querySelector('[data-sort-select]');
  if (sortSelect) {
    sortSelect.addEventListener('change', function () {
      if (sortSelect.form) {
        if (sortSelect.form.requestSubmit) {
          sortSelect.form.requestSubmit();
        } else {
          sortSelect.form.submit();
        }
      }
    });
  }

  /* Auto-submit category / availability filters */
  var filterForm = document.querySelector('[data-filter-form]');
  if (filterForm) {
    Array.prototype.forEach.call(filterForm.querySelectorAll('[data-auto-submit]'), function (input) {
      input.addEventListener('change', function () {
        if (filterForm.requestSubmit) {
          filterForm.requestSubmit();
        } else {
          filterForm.submit();
        }
      });
    });
  }
})();

// Catalogue — uniform card heights and quick comparator
(function () {
  'use strict';

  var app = document.getElementById('catalog-app');
  var grid = document.querySelector('[data-catalog-grid]');
  var equalizeTimer = null;

  var getCards = function () {
    return grid ? Array.prototype.slice.call(grid.querySelectorAll('[data-product-card]')) : [];
  };

  var equalizeCards = function () {
    if (!grid) {
      return;
    }

    var cards = getCards();
    cards.forEach(function (card) {
      card.style.minHeight = '';
    });

    if (grid.classList.contains('is-list')) {
      return;
    }

    var tallest = 0;
    cards.forEach(function (card) {
      tallest = Math.max(tallest, card.offsetHeight);
    });

    if (tallest > 0) {
      cards.forEach(function (card) {
        card.style.minHeight = tallest + 'px';
      });
    }
  };

  var scheduleEqualize = function () {
    window.clearTimeout(equalizeTimer);
    equalizeTimer = window.setTimeout(equalizeCards, 60);
  };

  window.addEventListener('load', scheduleEqualize);
  window.addEventListener('resize', scheduleEqualize);

  if (document.fonts && document.fonts.ready) {
    document.fonts.ready.then(scheduleEqualize).catch(function () {
      /* ignore font loading failures */
    });
  }

  Array.prototype.forEach.call(document.querySelectorAll('[data-view]'), function (button) {
    button.addEventListener('click', function () {
      window.setTimeout(equalizeCards, 40);
    });
  });

  if (!app) {
    return;
  }

  var bar = app.querySelector('[data-compare-bar]');
  var itemsBox = app.querySelector('[data-compare-items]');
  var countLabel = app.querySelector('[data-compare-count]');
  var openButton = app.querySelector('[data-compare-open]');
  var clearButton = app.querySelector('[data-compare-clear]');
  var modal = app.querySelector('[data-compare-modal]');
  var table = app.querySelector('[data-compare-table]');
  var toast = app.querySelector('[data-compare-toast]');
  var dataScript = document.getElementById('tz-compare-data');
  var addForm = document.getElementById('tz-compare-add-form');

  if (!bar || !itemsBox || !openButton || !modal || !table) {
    return;
  }

  var MAX = parseInt(app.getAttribute('data-compare-max'), 10) || 3;
  var STORAGE_KEY = 'tz-compare';
  var isAuthenticated = app.getAttribute('data-authenticated') === 'true';
  var loginUrl = app.getAttribute('data-login-url') || '/Account/Login';
  var returnUrl = app.getAttribute('data-return-url') || (window.location.pathname + window.location.search);
  var toastTimer = null;

  var products = [];
  try {
    products = JSON.parse(dataScript ? dataScript.textContent : '[]');
  } catch (error) {
    products = [];
  }

  if (!Array.isArray(products)) {
    products = [];
  }

  var productById = {};
  products.forEach(function (product) {
    if (product && product.id !== null && product.id !== undefined) {
      productById[String(product.id)] = product;
    }
  });

  var formatPrice = function (value) {
    var number = Number(value);
    if (!isFinite(number)) {
      return '—';
    }

    try {
      return new Intl.NumberFormat('fr-FR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(number) + ' €';
    } catch (error) {
      return number.toFixed(2) + ' €';
    }
  };

  var normalizeProduct = function (item) {
    var fallback = productById[String(item.id)] || {};
    var price = item.price !== undefined && item.price !== null ? item.price : fallback.price;
    var inStock = item.inStock !== undefined ? item.inStock : fallback.inStock;

    return {
      id: String(item.id),
      name: item.name || fallback.name || 'Produit',
      category: item.category || fallback.category || '',
      description: item.description || fallback.description || '',
      price: price,
      priceLabel: item.priceLabel || fallback.priceLabel || formatPrice(price),
      stock: item.stock !== undefined && item.stock !== null ? item.stock : fallback.stock,
      inStock: inStock !== false,
      imageUrl: item.imageUrl || fallback.imageUrl || '',
      detailsUrl: item.detailsUrl || fallback.detailsUrl || '#'
    };
  };

  var readSelection = function () {
    try {
      var raw = window.localStorage.getItem(STORAGE_KEY);
      var parsed = raw ? JSON.parse(raw) : [];
      if (!Array.isArray(parsed)) {
        return [];
      }

      return parsed
        .filter(function (item) {
          return item && item.id !== null && item.id !== undefined && item.name;
        })
        .slice(0, MAX)
        .map(normalizeProduct);
    } catch (error) {
      return [];
    }
  };

  var selection = readSelection();

  var persistSelection = function () {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(selection));
    } catch (error) {
      /* ignore storage failures */
    }
  };

  var isSelected = function (id) {
    id = String(id);
    return selection.some(function (item) {
      return item.id === id;
    });
  };

  var showToast = function (message) {
    if (!toast) {
      return;
    }

    toast.textContent = message;
    toast.hidden = false;
    window.requestAnimationFrame(function () {
      toast.classList.add('is-visible');
    });

    window.clearTimeout(toastTimer);
    toastTimer = window.setTimeout(function () {
      toast.classList.remove('is-visible');
      window.setTimeout(function () {
        toast.hidden = true;
      }, 220);
    }, 2400);
  };

  var syncCompareButtons = function () {
    Array.prototype.forEach.call(document.querySelectorAll('[data-compare-toggle]'), function (button) {
      var id = String(button.getAttribute('data-compare-toggle'));
      var active = isSelected(id);
      var label = button.querySelector('[data-compare-label]');
      var product = productById[id];
      var name = product ? product.name : 'ce produit';

      button.classList.toggle('is-selected', active);
      button.setAttribute('aria-pressed', active ? 'true' : 'false');
      button.setAttribute(
        'aria-label',
        active ? 'Retirer ' + name + ' du comparateur' : 'Ajouter ' + name + ' au comparateur'
      );

      if (label) {
        label.textContent = active ? 'Sélectionné' : 'Comparer';
      }
    });
  };

  var makeElement = function (tag, className, text) {
    var element = document.createElement(tag);
    if (className) {
      element.className = className;
    }
    if (text !== undefined && text !== null) {
      element.textContent = text;
    }
    return element;
  };

  var addToCart = function (productId) {
    if (isAuthenticated && addForm) {
      var input = addForm.querySelector('input[name="productId"]');
      if (input) {
        input.value = productId;
      }
      addForm.submit();
      return;
    }

    var separator = loginUrl.indexOf('?') === -1 ? '?' : '&';
    window.location.href = loginUrl + separator + 'returnUrl=' + encodeURIComponent(returnUrl);
  };

  var renderBar = function () {
    var hasSelection = selection.length > 0;

    bar.hidden = !hasSelection;
    document.body.classList.toggle('has-comparebar', hasSelection);

    if (countLabel) {
      countLabel.textContent = String(selection.length);
    }

    if (openButton) {
      openButton.disabled = selection.length < 2;
      openButton.setAttribute('aria-disabled', selection.length < 2 ? 'true' : 'false');
      openButton.title = selection.length < 2
        ? 'Sélectionnez au moins 2 produits à comparer'
        : 'Comparer les produits sélectionnés';
    }

    itemsBox.textContent = '';

    selection.forEach(function (product) {
      var chip = makeElement('span', 'cat-compare-chip');
      var image = document.createElement('img');
      var name = makeElement('span', 'cat-compare-chip-name', product.name);
      var remove = makeElement('button', 'cat-compare-chip-remove', '×');

      image.src = product.imageUrl;
      image.alt = '';
      image.loading = 'lazy';
      name.title = product.name;

      remove.type = 'button';
      remove.setAttribute('aria-label', 'Retirer ' + product.name + ' du comparateur');

      remove.addEventListener('click', function () {
        removeProduct(product.id);
      });

      chip.appendChild(image);
      chip.appendChild(name);
      chip.appendChild(remove);
      itemsBox.appendChild(chip);
    });
  };

  var renderModal = function () {
    table.textContent = '';

    if (selection.length < 2) {
      return;
    }

    table.style.gridTemplateColumns = '138px repeat(' + selection.length + ', minmax(190px, 1fr))';
    table.appendChild(makeElement('div', 'cat-cmp-corner'));

    selection.forEach(function (product) {
      var head = makeElement('div', 'cat-cmp-head');
      var top = makeElement('div', 'cat-cmp-product-top');
      var category = makeElement('span', 'cat-cmp-cat', product.category || 'Produit');
      var remove = makeElement('button', 'cat-cmp-remove', '×');
      var image = document.createElement('img');
      var name = makeElement('h3', 'cat-cmp-name');
      var link = makeElement('a', '', product.name);

      remove.type = 'button';
      remove.setAttribute('aria-label', 'Retirer ' + product.name + ' du comparateur');
      remove.addEventListener('click', function () {
        removeProduct(product.id);
      });

      link.href = product.detailsUrl || '#';

      image.className = 'cat-cmp-image';
      image.src = product.imageUrl;
      image.alt = product.name;
      image.loading = 'lazy';

      top.appendChild(category);
      top.appendChild(remove);
      name.appendChild(link);
      head.appendChild(top);
      head.appendChild(image);
      head.appendChild(name);
      table.appendChild(head);
    });

    var addRow = function (label, renderCell) {
      table.appendChild(makeElement('div', 'cat-cmp-label', label));

      selection.forEach(function (product) {
        var cell = makeElement('div', 'cat-cmp-cell');
        renderCell(cell, product);
        table.appendChild(cell);
      });
    };

    addRow('Prix', function (cell, product) {
      cell.appendChild(makeElement('span', 'cat-cmp-price', product.priceLabel || formatPrice(product.price)));
    });

    addRow('Disponibilité', function (cell, product) {
      var availability = makeElement(
        'span',
        'cat-cmp-availability ' + (product.inStock ? 'is-in' : 'is-out'),
        product.inStock ? 'En stock' : 'Rupture de stock'
      );

      cell.appendChild(availability);

      if (product.inStock && product.stock > 0) {
        var stock = makeElement('span', 'cat-cmp-stock-note', ' · ' + product.stock + ' unités');
        cell.appendChild(stock);
      }
    });

    addRow('Description', function (cell, product) {
      cell.appendChild(
        makeElement('p', 'cat-cmp-description', product.description || 'Aucune description disponible.')
      );
    });

    addRow('Action', function (cell, product) {
      var action = makeElement('div', 'cat-cmp-action');
      var button = makeElement('button', 'cat-cmp-add', product.inStock ? 'Ajouter au panier' : 'Indisponible');
      var details = makeElement('a', 'cat-cmp-open-link', 'Voir la fiche');

      button.type = 'button';
      button.disabled = !product.inStock;

      if (product.inStock) {
        button.addEventListener('click', function () {
          addToCart(product.id);
        });
      }

      details.href = product.detailsUrl || '#';
      action.appendChild(button);
      action.appendChild(details);
      cell.appendChild(action);
    });
  };

  var openModal = function () {
    if (selection.length < 2) {
      showToast('Sélectionnez au moins 2 produits à comparer.');
      return;
    }

    renderModal();

    if (typeof modal.showModal === 'function') {
      if (!modal.open) {
        modal.showModal();
      }
    } else {
      modal.setAttribute('open', '');
    }
  };

  var closeModal = function () {
    if (!modal) {
      return;
    }

    if (typeof modal.close === 'function') {
      modal.close();
    } else {
      modal.removeAttribute('open');
    }
  };

  var removeProduct = function (productId) {
    productId = String(productId);
    selection = selection.filter(function (item) {
      return item.id !== productId;
    });

    persistSelection();
    syncCompareButtons();
    renderBar();

    if (modal.open) {
      if (selection.length >= 2) {
        renderModal();
      } else {
        closeModal();
      }
    }
  };

  var addProduct = function (productId) {
    productId = String(productId);

    if (isSelected(productId)) {
      return;
    }

    if (selection.length >= MAX) {
      showToast('Vous pouvez comparer jusqu’à ' + MAX + ' produits maximum.');
      return;
    }

    var product = productById[productId];
    if (!product) {
      return;
    }

    selection.push(normalizeProduct(product));
    persistSelection();
    syncCompareButtons();
    renderBar();
  };

  var toggleProduct = function (productId) {
    productId = String(productId);

    if (isSelected(productId)) {
      removeProduct(productId);
    } else {
      addProduct(productId);
    }
  };

  Array.prototype.forEach.call(document.querySelectorAll('[data-compare-toggle]'), function (button) {
    button.addEventListener('click', function () {
      toggleProduct(button.getAttribute('data-compare-toggle'));
    });
  });

  openButton.addEventListener('click', openModal);

  if (clearButton) {
    clearButton.addEventListener('click', function () {
      selection = [];
      persistSelection();
      syncCompareButtons();
      renderBar();
      closeModal();
    });
  }

  var closeButton = app.querySelector('[data-compare-close]');
  if (closeButton) {
    closeButton.addEventListener('click', closeModal);
  }

  modal.addEventListener('click', function (event) {
    if (event.target === modal) {
      closeModal();
    }
  });

  syncCompareButtons();
  renderBar();
})();

// Account — password visibility toggles
(function () {
  'use strict';

  var toggles = Array.prototype.slice.call(
    document.querySelectorAll('[data-password-toggle]')
  );

  toggles.forEach(function (button) {
    var targetId = button.getAttribute('aria-controls');
    var field = targetId ? document.getElementById(targetId) : null;

    if (!field) {
      return;
    }

    button.addEventListener('click', function () {
      var reveal = field.getAttribute('type') === 'password';

      field.setAttribute('type', reveal ? 'text' : 'password');
      button.classList.toggle('is-visible', reveal);
      button.setAttribute('aria-pressed', reveal ? 'true' : 'false');
      button.setAttribute(
        'aria-label',
        reveal ? 'Masquer le mot de passe' : 'Afficher le mot de passe'
      );
    });
  });
})();
