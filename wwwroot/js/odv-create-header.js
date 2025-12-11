// Minimal script to create ODV header via AJAX then reveal sortie UI for that ODV.
// Depends on: odv-create.js (the existing dynamic add sortie JS) and that /Odvs/CreateHeader returns JSON { success:true, odvId: N }
// and /Odvs/OdvRowPartial?odvId=NN returns HTML partial for the created ODV.

(function () {
    'use strict';

    async function createOdvHeader() {
        // Collect form values from the header rows only (we post whole form but controller will only use header fields)
        var form = document.querySelector('#odv-create-form');
        if (!form) {
            alert('Form not found');
            return null;
        }
        // Use FormData so antiforgery token is included (form contains @Html.AntiForgeryToken())
        var fd = new FormData(form);

        try {
            var res = await fetch('/Odvs/CreateHeader', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: fd
            });

            if (res.status === 400) {
                var json = await res.json().catch(() => null);
                if (json && json.errors) {
                    var msgs = [];
                    for (var k in json.errors) msgs = msgs.concat(json.errors[k]);
                    alert('Validation: ' + msgs.join('; '));
                } else {
                    alert('Invalid header input. Please check required fields.');
                }
                return null;
            }
            if (!res.ok) {
                var txt = await res.text();
                alert('Server error: ' + txt);
                return null;
            }

            var json = await res.json();
            if (json && json.success) return json.odvId;
            alert('Failed to create ODV: ' + (json && json.error ? json.error : 'unknown'));
            return null;
        } catch (err) {
            console.error(err);
            alert('Network or server error creating ODV');
            return null;
        }
    }

    async function fetchAndInsertOdvRow(odvId) {
        try {
            var res = await fetch('/Odvs/OdvRowPartial?odvId=' + encodeURIComponent(odvId), {
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!res.ok) {
                console.warn('Failed to fetch ODV row partial');
                return;
            }
            var html = await res.text();
            // Insert partial at top of planned-odvs-container (or replace)
            var container = document.getElementById('planned-odvs-container');
            if (container) {
                container.innerHTML = html;
            } else {
                // fallback: append to body
                var wrapper = document.createElement('div');
                wrapper.innerHTML = html;
                document.body.appendChild(wrapper);
            }
        } catch (err) {
            console.error('fetchAndInsertOdvRow', err);
        }
    }

    document.addEventListener('click', function (e) {
        if (!e.target) return;
        var btn = e.target.closest('#btn-create-odv');
        if (!btn) return;
        e.preventDefault();
        (async function () {
            btn.disabled = true;
            btn.textContent = 'Creating...';
            var odvId = await createOdvHeader();
            if (!odvId) {
                btn.disabled = false;
                btn.textContent = 'Create ODV';
                return;
            }

            // Show the planned odv row partial for this created ODV
            await fetchAndInsertOdvRow(odvId);

            // Reveal the sorties form area for adding sorties:
            var sortiesForm = document.getElementById('sorties-form') || document.getElementById('sorties-container');
            if (sortiesForm) {
                sortiesForm.style.display = 'block';
            }

            // Set hidden field for downstream AddSorties path if you use it
            var hidden = document.getElementById('sorties-odvId') || document.querySelector('input[name="odvId"]');
            if (hidden) {
                if (hidden.tagName === 'INPUT') hidden.value = odvId;
                else {
                    // create hidden input inside #sorties-form
                    var sf = document.getElementById('sorties-form');
                    if (sf) {
                        var h = document.createElement('input');
                        h.type = 'hidden';
                        h.name = 'odvId';
                        h.id = 'sorties-odvId';
                        h.value = odvId;
                        sf.prepend(h);
                    }
                }
            }

            // Inform the existing AddSortie modal script that this ODV exists.
            // Optionally focus UI to the new ODV row.
            btn.textContent = 'Created';
            setTimeout(function () { btn.textContent = 'Create ODV'; btn.disabled = false; }, 1000);
        })();
    });
})();