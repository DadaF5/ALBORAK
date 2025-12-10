// Simple JS helpers to dynamically add/remove sorties and crew rows and maintain correct name indexes
(function () {
    'use strict';

    function indexToLabel(idx) {
        return idx + 1;
    }

    function reindexSorties() {
        const container = document.getElementById('sorties-container');
        const blocks = container.querySelectorAll('.sortie-block');
        blocks.forEach((block, sIdx) => {
            block.setAttribute('data-sortie-index', sIdx);
            const title = block.querySelector('.card-title');
            if (title) title.textContent = 'Sortie ' + indexToLabel(sIdx);

            // Fix all input/select name attributes inside this block:
            block.querySelectorAll('[name]').forEach(el => {
                const name = el.getAttribute('name');
                if (!name) return;
                // Replace Sorties[OLD]. with Sorties[SIDX].
                const newName = name.replace(/Sorties\[\d+\]/, `Sorties[${sIdx}]`);
                el.setAttribute('name', newName);
            });

            // Reindex crew rows inside this sortie
            const crewRows = block.querySelectorAll('tbody tr');
            crewRows.forEach((row, cIdx) => {
                row.setAttribute('data-crew-index', cIdx);
                row.querySelectorAll('[name]').forEach(el => {
                    const name = el.getAttribute('name');
                    if (!name) return;
                    // Replace Crew[OLD] with Crew[CIDX]
                    const newName = name.replace(/Crew\[\d+\]/, `Crew[${cIdx}]`);
                    el.setAttribute('name', newName);
                });
            });

            // Update the add-crew button's data-sortie-index
            const addCrewBtn = block.querySelector('.btn-add-crew');
            if (addCrewBtn) addCrewBtn.setAttribute('data-sortie-index', sIdx);
        });
    }

    // Add a new sortie using the template
    document.getElementById('btn-add-sortie')?.addEventListener('click', function (e) {
        const container = document.getElementById('sorties-container');
        const template = document.getElementById('template-sortie');
        if (!container || !template) return;

        // compute new index (append)
        const newIndex = container.querySelectorAll('.sortie-block').length;

        // clone template content
        let html = template.innerHTML;
        html = html.replace(/__IDX__/g, String(newIndex));
        html = html.replace(/__NUM__/g, String(newIndex + 1));

        const wrapper = document.createElement('div');
        wrapper.innerHTML = html;
        container.appendChild(wrapper.firstElementChild);
        reindexSorties();
    });

    // Delegate remove sortie
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-remove-sortie');
        if (!btn) return;
        e.preventDefault();
        const block = btn.closest('.sortie-block');
        if (block) block.remove();
        reindexSorties();
    });

    // Delegate add crew row
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-add-crew');
        if (!btn) return;
        e.preventDefault();

        const sortieIndex = btn.getAttribute('data-sortie-index');
        const block = document.querySelector(`.sortie-block[data-sortie-index="${sortieIndex}"]`);
        if (!block) return;
        const tbody = block.querySelector('tbody');
        if (!tbody) return;

        // Determine next crew index
        const nextCrewIndex = tbody.querySelectorAll('tr').length;

        // Create a new row using the first row as template if available
        const firstRow = tbody.querySelector('tr');
        if (!firstRow) return;

        const newRow = firstRow.cloneNode(true);
        newRow.setAttribute('data-crew-index', nextCrewIndex);

        // Fix names inside the cloned row
        newRow.querySelectorAll('[name]').forEach(el => {
            const name = el.getAttribute('name');
            if (!name) return;
            const newName = name.replace(/Crew\[\d+\]/, `Crew[${nextCrewIndex}]`);
            el.setAttribute('name', newName);

            // Reset value
            if (el.tagName === 'INPUT') {
                if (el.type === 'checkbox') el.checked = false;
                else el.value = '';
            } else if (el.tagName === 'SELECT') {
                el.selectedIndex = 0;
            } else if (el.tagName === 'TEXTAREA') {
                el.value = '';
            }
        });

        tbody.appendChild(newRow);
    });

    // Delegate remove crew row
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-remove-crew');
        if (!btn) return;
        e.preventDefault();
        const row = btn.closest('tr');
        const tbody = row?.closest('tbody');
        row?.remove();
        // after removal reindex sorties (which reindexes crew)
        reindexSorties();
    });

    // Ensure initial reindex on page load
    document.addEventListener('DOMContentLoaded', function () {
        reindexSorties();
    });
})();