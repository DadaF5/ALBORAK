// Updated modal loader to avoid duplicate modals and ensure a single injected modal instance.
// - Removes any previously injected modal(s) before inserting the new one.
// - Marks injected modal with data-injected="true" so we only remove injected instances (not modals that may belong to the page).
// - Also removes any stray modals with id starting with "cvModal-" to be safe (helps if the partial was accidentally left on the page).
// - Keeps Bootstrap4 and Bootstrap5 compatibility and proper tab handling.
// Replace your existing crewmember-details-modal.js with this file and hard-refresh the browser.
(function () {
    function findInjectedModal() {
        var container = document.getElementById('modalContainer');
        if (!container) return null;
        return container.querySelector('.modal');
    }

    function markInjected(modalEl) {
        try { modalEl.setAttribute('data-injected', 'true'); } catch (e) { }
    }

    function removeInjectedModals() {
        // 1) Remove any modal elements inside our modalContainer
        var container = document.getElementById('modalContainer');
        if (container) {
            var injected = container.querySelectorAll('.modal');
            injected.forEach(function (m) {
                try {
                    // hide cleanly if bootstrap instance exists
                    if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                        var inst = window.bootstrap.Modal.getInstance(m);
                        if (inst) try { inst.hide(); } catch (e) { }
                    } else if (window.jQuery) {
                        try { window.jQuery(m).modal('hide'); } catch (e) { }
                    }
                } catch (e) { }
                try { m.remove(); } catch (e) { }
            });
        }

        // 2) Remove any other modals that were previously injected and marked data-injected="true"
        document.querySelectorAll('.modal[data-injected="true"]').forEach(function (m) {
            try {
                if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
                    var inst = window.bootstrap.Modal.getInstance(m);
                    if (inst) try { inst.hide(); } catch (e) { }
                } else if (window.jQuery) {
                    try { window.jQuery(m).modal('hide'); } catch (e) { }
                }
            } catch (e) { }
            try { m.remove(); } catch (e) { }
        });

        // 3) Defensive: remove any stray modals with ids that start with cvModal- to avoid duplicate UI
        document.querySelectorAll('[id^="cvModal-"]').forEach(function (m) {
            // only remove if it looks injected (no server-side reason)
            try { m.remove(); } catch (e) { }
        });
    }

    function removeDismissAttributes(el) {
        if (!el) return;
        el.querySelectorAll('.modal-close').forEach(function (btn) {
            try {
                btn.removeAttribute('data-dismiss');
                btn.removeAttribute('data-bs-dismiss');
            } catch (e) { /* ignore */ }
        });
    }

    function wireCloseButtons(modalEl, instance) {
        if (!modalEl) return;
        modalEl.querySelectorAll('.modal-close').forEach(function (btn) {
            btn.onclick = null;
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                try {
                    if (instance && typeof instance.hide === 'function') instance.hide();
                    else if (window.jQuery) window.jQuery(modalEl).modal('hide');
                } catch (err) {
                    try { if (window.jQuery) window.jQuery(modalEl).modal('hide'); } catch (e) { }
                }
            }, { once: true });
        });
    }

    function initTabs(modalEl) {
        if (!modalEl) return;
        // Bootstrap 5
        if (window.bootstrap && typeof window.bootstrap.Tab === 'function') {
            modalEl.querySelectorAll('[data-bs-toggle="tab"], [data-toggle="tab"]').forEach(function (tabLink) {
                tabLink.addEventListener('click', function (e) {
                    e.preventDefault();
                    try {
                        var tab = new window.bootstrap.Tab(tabLink);
                        tab.show();
                    } catch (ex) { /* ignore */ }
                });
            });
            return;
        }
        // Bootstrap 4 (jQuery)
        if (window.jQuery) {
            try {
                window.jQuery(modalEl).find('[data-toggle="tab"]').off('click.modalTab').on('click.modalTab', function (e) {
                    e.preventDefault();
                    window.jQuery(this).tab('show');
                });
            } catch (ex) { /* ignore */ }
        }
    }

    function showModal(modalEl) {
        if (!modalEl) return;

        // ensure only injected modals are manipulated
        markInjected(modalEl);
        removeDismissAttributes(modalEl);

        // Bootstrap 5
        if (window.bootstrap && typeof window.bootstrap.Modal === 'function') {
            var instance = window.bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: true, keyboard: true });

            wireCloseButtons(modalEl, instance);

            modalEl.addEventListener('hidden.bs.modal', function () {
                try { modalEl.remove(); } catch (e) { }
            }, { once: true });

            initTabs(modalEl);
            instance.show();
            return;
        }

        // Bootstrap 4 + jQuery
        if (window.jQuery && typeof window.jQuery(modalEl).modal === 'function') {
            var $modal = window.jQuery(modalEl);

            $modal.off('click.modalClose').on('click.modalClose', '.modal-close', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $modal.modal('hide');
            });

            $modal.one('hidden.bs.modal', function () {
                try { $modal.remove(); } catch (e) { }
            });

            initTabs(modalEl);
            $modal.modal({ backdrop: true, keyboard: true, show: true });
            return;
        }

        console.warn('No Bootstrap modal API detected. Ensure Bootstrap JS is loaded.');
    }

    // main handler: intercept clicks on .open-details-modal
    document.addEventListener('click', function (e) {
        var target = e.target;
        while (target && target !== document) {
            if (target.matches && target.matches('a.open-details-modal')) break;
            target = target.parentNode;
        }
        if (!target || target === document) return;

        e.preventDefault();
        var url = target.getAttribute('href');
        if (!url) return;

        // ensure container exists
        var container = document.getElementById('modalContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'modalContainer';
            document.body.appendChild(container);
        }

        // remove any previously injected modal(s)
        removeInjectedModals();

        // Fetch the modal partial
        fetch(url, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (resp) {
                if (!resp.ok) throw new Error('Failed to load modal partial');
                return resp.text();
            })
            .then(function (html) {
                container.innerHTML = html;

                // Find the modal we just injected
                var modalEl = findInjectedModal();
                if (!modalEl) {
                    // defensive: try to find any modal inside container
                    modalEl = container.querySelector('.modal');
                }
                if (!modalEl) throw new Error('No modal found in injected HTML');

                showModal(modalEl);
            })
            .catch(function (err) {
                console.error('Modal load failed', err);
                // fallback to full page navigation
                window.location.href = url.replace(/[?&]modal=true/i, '');
            });
    }, false);
})();