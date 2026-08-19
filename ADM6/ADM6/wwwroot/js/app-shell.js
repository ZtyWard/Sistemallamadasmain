(() => {
    const body = document.body;
    const button = document.querySelector("[data-menu-toggle]");

    const closeMenu = () => {
        body.classList.remove("menu-open");
        button?.setAttribute("aria-expanded", "false");
    };

    button?.addEventListener("click", () => {
        const open = body.classList.toggle("menu-open");
        button.setAttribute("aria-expanded", String(open));
    });

    document.querySelectorAll(".primary-nav .nav-item").forEach(link =>
        link.addEventListener("click", closeMenu));

    const normalizePath = value => {
        const path = (value || "/").toLowerCase().replace(/\/$/, "");
        return path || "/";
    };

    const currentPath = normalizePath(window.location.pathname);
    let bestMatch = null;

    document.querySelectorAll(".primary-nav .nav-item[href]").forEach(link => {
        const href = normalizePath(new URL(link.href, window.location.origin).pathname);
        const matches = currentPath === href
            || (href !== "/" && currentPath.startsWith(href + "/"));

        if (matches && (!bestMatch || href.length > bestMatch.href.length)) {
            bestMatch = { link, href };
        }
    });

    if (bestMatch) {
        bestMatch.link.classList.add("active");
        bestMatch.link.setAttribute("aria-current", "page");
    }

    document.addEventListener("keydown", event => {
        if (event.key === "Escape") closeMenu();
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth > 760) closeMenu();
    });
})();
