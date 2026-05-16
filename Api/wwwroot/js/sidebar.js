async function loadSidebar() {
    const mount = document.getElementById("sidebarMount");

    if (!mount) {
        return;
    }

    const response = await fetch("./components/sidebar.html");
    const html = await response.text();

    mount.innerHTML = html;

    initSidebar();
}

function initSidebar() {
    const sidebar = document.getElementById("sidebar");
    const overlay = document.getElementById("sidebarOverlay");
    const openBtn = document.getElementById("openSidebar");
    const closeBtn = document.getElementById("closeSidebar");

    if (!sidebar || !overlay || !openBtn || !closeBtn) {
        return;
    }

    function openSidebar() {
        sidebar.classList.add("is-open");
        overlay.classList.add("is-open");
        document.body.style.overflow = "hidden";
    }

    function closeSidebar() {
        sidebar.classList.remove("is-open");
        overlay.classList.remove("is-open");
        document.body.style.overflow = "";
    }

    openBtn.addEventListener("click", openSidebar);
    closeBtn.addEventListener("click", closeSidebar);
    overlay.addEventListener("click", closeSidebar);

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeSidebar();
        }
    });

    document.querySelectorAll("[data-collapse]").forEach((button) => {
        button.addEventListener("click", () => {
            const section = button.closest(".sidebar-section");
            section.classList.toggle("is-collapsed");
        });
    });

    document.querySelectorAll("[data-project]").forEach((button) => {
        button.addEventListener("click", () => {
            const projectId = button.dataset.project;

            if (document.body.classList.contains("kanban-body")) {
                window.dispatchEvent(
                    new CustomEvent("project:selected", {
                        detail: { projectId },
                    })
                );

                closeSidebar();
                return;
            }

            window.location.href = `./kanban.html?project=${projectId}`;
        });
    });

    markActiveProject();
}

function markActiveProject() {
    const params = new URLSearchParams(window.location.search);
    const activeProject = params.get("project");

    if (!activeProject) {
        return;
    }

    document.querySelectorAll("[data-project]").forEach((button) => {
        button.classList.toggle("active", button.dataset.project === activeProject);
    });
}

loadSidebar();