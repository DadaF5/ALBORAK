// Client helper: open aircraft selector modal, allow selection only for Available aircraft,
// call AssignAircraft endpoint, handle responses and update UI.
// Include this script in Views/Odvs/Index.cshtml (or layout) where #modal-placeholder exists.

(function () {
    'use strict';

    // Utility to get antiforgery token from a hidden form on the page (recommended)
    function getAntiForgeryToken() {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : null;
    }

    // Fetch and show the aircraft selector modal for a given sortieId
    async function openAircraftSelector(sortieId) {
        try {
            const url = '/Odvs/SelectAircraft?sortieId=' + encodeURIComponent(sortieId);
            const res = await fetch(url, {
                method: 'GET',
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (!res.ok) {
                const text = await res.text();
                throw new Error(text || 'Failed to load aircraft selector');
            }

            const html = await res.text();
            const placeholder = document.getElementById('modal-placeholder');
            if (!placeholder) {
                console.warn('No #modal-placeholder element found in DOM; cannot show modal.');
                // fallback: inject at end of body
                const body = document.body;
                const div = document.createElement('div');
                div.id = 'modal-placeholder';
                body.appendChild(div);
                document.getElementById('modal-placeholder').innerHTML = html;
            } else {
                placeholder.innerHTML = html;
            }

            // initialize bootstrap modal and events
            const modalEl = placeholder.querySelector('.modal');
            if (!modalEl) {
                console.warn('Selector partial did not contain .modal element');
                return;
            }

            const bsModal = new bootstrap.Modal(modalEl);
            bsModal.show();

            // hook up select buttons (delegated)
            modalEl.addEventListener('click', async function (ev) {
                const btn = ev.target.closest('.select-aircraft-btn');
                if (!btn) return;

                const aircraftId = btn.getAttribute('data-aircraft-id');
                if (!aircraftId) return;

                // call AssignAircraft endpoint
                try {
                    btn.disabled = true;
                    btn.textContent = 'Assigning...';

                    const fd = new FormData();
                    fd.append('sortieId', sortieId);
                    fd.append('aircraftId', aircraftId);

                    const token = getAntiForgeryToken();
                    if (token) fd.append('__RequestVerificationToken', token);

                    const assignRes = await fetch('/Odvs/AssignAircraft', {
                        method: 'POST',
                        credentials: 'same-origin',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        body: fd
                    });

                    if (assignRes.status === 409) {
                        const json = await assignRes.json().catch(() => null);
                        alert('Cannot assign aircraft: ' + (json?.error || 'conflict'));
                        btn.disabled = false;
                        btn.textContent = 'Select';
                        return;
                    }

                    if (!assignRes.ok) {
                        const txt = await assignRes.text();
                        throw new Error(txt || 'Server error during assignment');
                    }

                    const json = await assignRes.json();

                    if (json && json.success) {
                        // close modal and refresh ODV row (simplest). You could also update the DOM for the specific sortie
                        bsModal.hide();
                        // small delay to allow modal hide animation
                        setTimeout(function () { location.reload(); }, 200);
                    } else {
                        alert('Assign failed: ' + (json && json.error ? json.error : 'unknown'));
                        btn.disabled = false;
                        btn.textContent = 'Select';
                    }
                } catch (err) {
                    console.error(err);
                    alert('Error assigning aircraft: ' + err.message);
                    btn.disabled = false;
                    btn.textContent = 'Select';
                }
            }, { once: false });

            // close button handler will just remove modal HTML; Bootstrap handles hiding
            modalEl.addEventListener('hidden.bs.modal', function () {
                const placeholder2 = document.getElementById('modal-placeholder');
                if (placeholder2) placeholder2.innerHTML = '';
            }, { once: true });

        } catch (ex) {
            console.error(ex);
            alert('Failed to open aircraft selector: ' + ex.message);
        }
    }

    // Delegate click from page: any element with data-action="open-aircraft-selector"
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('[data-action="open-aircraft-selector"]');
        if (!btn) return;
        e.preventDefault();
        const sortieId = btn.getAttribute('data-sortie-id');
        if (!sortieId) return;
        openAircraftSelector(sortieId);
    });

    // expose for direct calls
    window.odvAircraftSelector = {
        open: openAircraftSelector
    };
})();