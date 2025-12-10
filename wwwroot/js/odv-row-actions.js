// Requires Bootstrap 5 modal markup in the page: a single #globalModal element where we inject partial HTML
(function () {
    'use strict';

    function showModal(html) {
        // ensure there's a modal container
        let container = document.getElementById('global-modal');
        if (!container) {
            container = document.createElement('div');
            container.id = 'global-modal';
            document.body.appendChild(container);
        }
        container.innerHTML = html;
        const modalEl = container.querySelector('.modal');
        if (!modalEl) return;
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
        return { container, modalEl, modal };
    }

    async function fetchPartial(url) {
        const res = await fetch(url, { credentials: 'same-origin', headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!res.ok) throw new Error('Network error');
        return await res.text();
    }

    // Click handler to open Add Sortie modal
    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-add-sortie');
        if (!btn) return;
        e.preventDefault();
        const odvId = btn.getAttribute('data-odv-id');
        const html = await fetchPartial('/Odvs/AddSortieModal?odvId=' + encodeURIComponent(odvId));
        const { modal } = showModal(html);

        // handle submit inside modal
        modal._element.querySelector('#add-sortie-form')?.addEventListener('submit', async function (ev) {
            ev.preventDefault();
            const form = ev.target;
            const fd = new FormData(form);
            // include odvId if not present
            if (!fd.get('odvId')) fd.append('odvId', odvId);
            const token = fd.get('__RequestVerificationToken');
            const res = await fetch('/Odvs/AddSortie', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: fd
            });
            if (res.ok) {
                const json = await res.json();
                if (json.success) {
                    // append sortieHtml to the ODV's table or refresh the ODV row
                    // Try to find the ODV row container:
                    const odvRow = document.querySelector('.odv-row[data-odv-id="' + odvId + '"]');
                    if (odvRow) {
                        // if there's a table body, append the sortie row
                        const tbody = odvRow.querySelector('table tbody');
                        if (tbody && json.sortieHtml) {
                            // sortieHtml is a <tr> fragment; append safely
                            const wrapper = document.createElement('tbody');
                            wrapper.innerHTML = json.sortieHtml;
                            // move children into existing tbody
                            Array.from(wrapper.children).forEach(ch => tbody.appendChild(ch));
                        } else {
                            // fallback: refresh the whole ODV row from server
                            const odvHtml = await fetchPartial('/Odvs/OdvRowPartial?odvId=' + encodeURIComponent(odvId));
                            const container = document.querySelector('.odv-row[data-odv-id="' + odvId + '"]');
                            if (container) container.outerHTML = odvHtml;
                        }
                    } else {
                        // if ODV row not in DOM, optionally reload list
                        location.reload();
                    }
                    modal.hide();
                } else {
                    alert('Error: ' + (json.error || 'unknown'));
                }
            } else {
                const txt = await res.text();
                alert('Server error: ' + txt);
            }
        }, { once: true });
    });

    // Click handler to open Add Crew modal (delegated)
    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-add-crew');
        if (!btn) return;
        e.preventDefault();
        const sortieId = btn.getAttribute('data-sortie-id');
        const html = await fetchPartial('/Sorties/AddCrewModal?sortieId=' + encodeURIComponent(sortieId));
        const { modal } = showModal(html);

        modal._element.querySelector('#add-crew-form')?.addEventListener('submit', async function (ev) {
            ev.preventDefault();
            const form = ev.target;
            const fd = new FormData(form);
            const res = await fetch('/Sorties/AddCrew', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: fd
            });
            if (res.ok) {
                const json = await res.json();
                if (json.success) {
                    // update the sortie's crew HTML
                    const row = document.querySelector('tr[data-sortie-id="' + sortieId + '"]');
                    if (row && json.crewHtml) {
                        // replace crew cell content; find the cell that contains crew list
                        const crewCell = row.querySelector('td:nth-child(4)');
                        if (crewCell) crewCell.innerHTML = json.crewHtml;
                    } else {
                        // fallback: refresh entire ODV row or reload page
                        location.reload();
                    }
                    modal.hide();
                } else {
                    alert('Error: ' + (json.error || 'unknown'));
                }
            } else {
                const txt = await res.text();
                alert('Server error: ' + txt);
            }
        }, { once: true });
    });
})();