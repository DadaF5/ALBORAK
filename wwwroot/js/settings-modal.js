async function openSettingsModal(url, title) {
  const modalEl = document.getElementById('settingsCrudModal');
  const titleEl = document.getElementById('settingsCrudModalTitle');
  const bodyEl = document.getElementById('settingsCrudModalBody');

  titleEl.textContent = title || 'Paramètres';
  bodyEl.innerHTML = 'Chargement...';

  // Load HTML
  const resp = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });
  bodyEl.innerHTML = await resp.text();

  // Show modal (Bootstrap 5)
  const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
  modal.show();
}