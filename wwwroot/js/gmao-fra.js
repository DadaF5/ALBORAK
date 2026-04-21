(function () {
    const key = "gmao.sidebarCollapsed";

    function init() {
        const body = document.body;
        const btn = document.getElementById("sidebarToggle");

        // restore state
        try {
            const v = localStorage.getItem(key);
            if (v === "1") body.classList.add("sidebar-collapsed");
            else body.classList.remove("sidebar-collapsed");
        } catch { }

        if (!btn) return;

        btn.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            body.classList.toggle("sidebar-collapsed");

            try {
                localStorage.setItem(
                    key,
                    body.classList.contains("sidebar-collapsed") ? "1" : "0"
                );
            } catch { }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();