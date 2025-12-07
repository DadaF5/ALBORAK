// Simple duplicate-check helper for Qualifications Create/Edit forms.
// Usage: call initQualificationDuplicateCheck(options) in the view Scripts section.
//
// options:
//  - checkUrl: URL to IsDuplicate endpoint (string)
//  - nameSelector: selector for Name input (string)
//  - typeSelector: selector for QualificationType select (string)
//  - duplicateWarningSelector: selector where duplicate message will be shown (string)
//  - submitBtnSelector: selector for submit button (string)
//  - formSelector: selector for form (string)
//  - excludeId: numeric id to exclude from duplicate check (0 for create, existing id for edit)

function initQualificationDuplicateCheck(options) {
    if (!options || !options.checkUrl) return;

    const nameEl = document.querySelector(options.nameSelector);
    const typeEl = document.querySelector(options.typeSelector);
    const warningEl = document.querySelector(options.duplicateWarningSelector);
    const submitBtn = document.querySelector(options.submitBtnSelector);
    const formEl = document.querySelector(options.formSelector);
    const excludeId = options.excludeId || 0;

    if (!nameEl || !typeEl || !warningEl || !submitBtn || !formEl) return;

    let lastCheck = 0;
    let isDuplicate = false;
    let pending = null;
    const debounceMs = 400;

    function setWarning(msg) {
        if (msg) {
            warningEl.textContent = msg;
            warningEl.style.display = 'block';
        } else {
            warningEl.textContent = '';
            warningEl.style.display = 'none';
        }
    }

    function setSubmitEnabled(enabled) {
        submitBtn.disabled = !enabled;
    }

    async function checkDuplicate() {
        const name = nameEl.value.trim();
        const type = typeEl.value || 'Other';

        if (!name) {
            isDuplicate = false;
            setWarning('');
            setSubmitEnabled(true);
            return;
        }

        // Cancel previous pending check
        if (pending && typeof pending.cancel === 'function') {
            pending.cancel();
        }

        const controller = new AbortController();
        pending = controller;
        const url = `${options.checkUrl}?name=${encodeURIComponent(name)}&type=${encodeURIComponent(type)}&excludeId=${encodeURIComponent(excludeId)}`;

        lastCheck = Date.now();
        try {
            const resp = await fetch(url, { method: 'GET', credentials: 'same-origin', signal: controller.signal });
            if (!resp.ok) {
                // non-fatal: don't block user if endpoint has an issue
                isDuplicate = false;
                setWarning('');
                setSubmitEnabled(true);
                return;
            }
            const exists = await resp.json();
            isDuplicate = !!exists;
            if (isDuplicate) {
                setWarning('A qualification with the same name and type already exists.');
                setSubmitEnabled(false);
            } else {
                setWarning('');
                setSubmitEnabled(true);
            }
        } catch (err) {
            if (err.name === 'AbortError') {
                // aborted due to debounce/newer request; ignore
                return;
            }
            // on error allow submit (server-side will still protect)
            isDuplicate = false;
            setWarning('');
            setSubmitEnabled(true);
        } finally {
            pending = null;
        }
    }

    // debounce wrapper
    let timer = null;
    function scheduleCheck() {
        if (timer) clearTimeout(timer);
        timer = setTimeout(() => checkDuplicate(), debounceMs);
    }

    nameEl.addEventListener('input', scheduleCheck);
    typeEl.addEventListener('change', scheduleCheck);

    // final guard on submit: perform synchronous check if last check not recent
    formEl.addEventListener('submit', async function (e) {
        // if submit button disabled already, block
        if (submitBtn.disabled) {
            e.preventDefault();
            return;
        }
        // do a last quick check synchronously (await)
        await checkDuplicate();
        if (isDuplicate) {
            e.preventDefault();
            nameEl.focus();
        }
    });

    // initial check (useful on edit)
    scheduleCheck();
}