import {
    escapeHtml,
    loadWorkspace,
    renderSidebarNavigation,
    selectProjectFromUrl,
} from "./boardData.js";
import { createKanban } from "./api/kanbanApi.js";
import { createProject } from "./api/projectApi.js";

const projectsGrid = document.getElementById("projectsGrid");
const openCreateProjectBtn = document.getElementById("openCreateProject");
const createProjectOverlay = document.getElementById("createProjectOverlay");
const createProjectForm = document.getElementById("createProjectForm");
const createProjectClose = document.getElementById("createProjectClose");
const createProjectCancel = document.getElementById("createProjectCancel");
const newProjectName = document.getElementById("newProjectName");
const newProjectTeam = document.getElementById("newProjectTeam");
const createProjectSubmit = document.getElementById("createProjectSubmit");

const createBoardOverlay = document.getElementById("createBoardOverlay");
const createBoardForm = document.getElementById("createBoardForm");
const createBoardTitle = document.getElementById("createBoardTitle");
const createBoardClose = document.getElementById("createBoardClose");
const createBoardCancel = document.getElementById("createBoardCancel");
const newBoardName = document.getElementById("newBoardName");
const createBoardSubmit = document.getElementById("createBoardSubmit");

const PROJECT_COLORS = ["#ef6a35", "#4668ad", "#ffe3d8", "#407d52", "#e6a0a6"];

let state = {
    workspace: {
        teams: [],
        projects: [],
    },
    selectedProjectId: getProjectFromUrl(),
    targetBoardProjectId: null,
};

async function init() {
    renderLoading();

    try {
        const workspace = await loadWorkspace();
        const selectedProject = selectProjectFromUrl(workspace.projects);

        state.workspace = workspace;
        state.selectedProjectId = selectedProject?.id ?? null;

        syncSelectedProjectUrl();
        renderSidebar();
        renderProjects();
        scrollSelectedProjectIntoView({ behavior: "auto" });
    } catch (error) {
        console.error(error);
        renderError("Не удалось загрузить проекты. Проверь авторизацию и доступ к API.");
    }
}

function renderProjects() {
    const projects = state.workspace.projects ?? [];

    projectsGrid.innerHTML = "";

    if (projects.length === 0) {
        renderEmpty("Проектов пока нет");
        return;
    }

    projects.forEach((project) => {
        projectsGrid.appendChild(createProjectCard(project));
    });

    markSelectedProject();
}

function createProjectCard(project) {
    const panel = document.createElement("article");

    panel.className = "project-panel";
    panel.dataset.projectCard = project.id;

    panel.innerHTML = `
        <div class="project-panel-head">
            <div class="project-team-inline">
                <span class="project-color-dot" style="background:${escapeHtml(project.color)}"></span>
                <p class="project-team-name">${escapeHtml(project.team?.name || "Без команды")}</p>
            </div>

            <h2 class="project-card-name">${escapeHtml(project.name || "Проект")}</h2>

            <span class="project-board-count">${escapeHtml(formatBoardCount(project.kanbans?.length ?? 0))}</span>
        </div>

        <div class="project-boards">
            ${renderBoardRows(project)}
        </div>

        <button
                class="project-add-board"
                data-create-board="${escapeHtml(project.id)}"
                type="button"
        >
            <span>＋</span>
            <span>Добавить доску</span>
        </button>
    `;

    panel.querySelector("[data-create-board]")?.addEventListener("click", () => {
        openCreateBoardOverlay(project.id);
    });

    panel.addEventListener("click", (event) => {
        if (event.target.closest("a, button")) {
            return;
        }

        selectProject(project.id, { scroll: false });
    });

    return panel;
}

function renderBoardRows(project) {
    const kanbans = project.kanbans ?? [];

    if (kanbans.length === 0) {
        return `<div class="project-empty">досок пока нет</div>`;
    }

    return kanbans.map((kanban) => {
        const href = `./kanban.html?project=${encodeURIComponent(project.id)}&kanban=${encodeURIComponent(kanban.id)}`;

        return `
            <a class="project-board-row" href="${href}">
                <span class="project-board-name">${escapeHtml(kanban.name || "Kanban")}</span>
                <span class="project-board-meta">Открыть доску проекта</span>
                <span class="project-board-open" aria-hidden="true">→</span>
            </a>
        `;
    }).join("");
}

function openCreateProjectOverlay() {
    if (!state.workspace.teams.length) {
        showToast("Сначала создайте команду");
        return;
    }

    createProjectForm.reset();
    renderProjectTeamOptions();
    updateCreateProjectSubmitState();

    openOverlay(createProjectOverlay);

    setTimeout(() => {
        newProjectName.focus();
    }, 0);
}

function closeCreateProjectOverlay() {
    closeOverlay(createProjectOverlay);
    updateCreateProjectSubmitState();
}

async function submitCreateProject() {
    const name = newProjectName.value.trim();
    const teamId = newProjectTeam.value;
    const team = state.workspace.teams.find((item) => item.id === teamId);

    if (!name) {
        newProjectName.focus();
        return;
    }

    if (!team) {
        showToast("Выберите команду");
        return;
    }

    setProjectSubmitBusy(createProjectSubmit, true);

    try {
        const createdProject = await createProject(team.id, name);
        const project = normalizeCreatedProject(createdProject, team);

        state.workspace.projects.push(project);
        state.selectedProjectId = project.id;

        closeCreateProjectOverlay();
        renderSidebar();
        renderProjects();
        syncSelectedProjectUrl();
        scrollSelectedProjectIntoView();
        showToast(`Проект «${project.name}» создан`);
    } catch (error) {
        console.error(error);
        showToast(getRequestErrorMessage(error, "Не удалось создать проект"));
    } finally {
        setProjectSubmitBusy(createProjectSubmit, false);
    }
}

function renderProjectTeamOptions() {
    const selectedProject = getSelectedProject();
    const selectedTeamId = selectedProject?.teamId ?? state.workspace.teams[0]?.id ?? "";

    newProjectTeam.innerHTML = state.workspace.teams.map((team) => `
        <option value="${escapeHtml(team.id)}" ${team.id === selectedTeamId ? "selected" : ""}>
            ${escapeHtml(team.name)}
        </option>
    `).join("");
}

function openCreateBoardOverlay(projectId) {
    const project = state.workspace.projects.find((item) => item.id === projectId);

    if (!project) {
        return;
    }

    state.targetBoardProjectId = project.id;
    state.selectedProjectId = project.id;

    createBoardForm.reset();
    createBoardTitle.textContent = `Создание доски: ${project.name}`;
    updateCreateBoardSubmitState();
    syncSelectedProjectUrl();
    renderSidebar();
    markSelectedProject();

    openOverlay(createBoardOverlay);

    setTimeout(() => {
        newBoardName.focus();
    }, 0);
}

function closeCreateBoardOverlay() {
    closeOverlay(createBoardOverlay);
    state.targetBoardProjectId = null;
    updateCreateBoardSubmitState();
}

async function submitCreateBoard() {
    const name = newBoardName.value.trim();
    const project = state.workspace.projects.find((item) => item.id === state.targetBoardProjectId);

    if (!name) {
        newBoardName.focus();
        return;
    }

    if (!project) {
        showToast("Проект не найден");
        return;
    }

    setProjectSubmitBusy(createBoardSubmit, true);

    try {
        const createdBoard = await createKanban(project.id, name);
        const kanban = normalizeCreatedKanban(createdBoard, project);

        project.kanbans = [...(project.kanbans ?? []), kanban];
        project.meta = formatBoardCount(project.kanbans.length);
        state.selectedProjectId = project.id;

        closeCreateBoardOverlay();
        renderSidebar();
        renderProjects();
        syncSelectedProjectUrl();
        scrollSelectedProjectIntoView();
        showToast(`Доска «${kanban.name}» создана`);
    } catch (error) {
        console.error(error);
        showToast(getRequestErrorMessage(error, "Не удалось создать доску"));
    } finally {
        setProjectSubmitBusy(createBoardSubmit, false);
    }
}

function normalizeCreatedProject(project, team) {
    const id = readValue(project, "id", "Id");
    const teamId = readValue(project, "teamId", "TeamId") || team.id;
    const projectIndex = state.workspace.projects.length;

    return {
        id,
        teamId,
        team,
        name: readValue(project, "name", "Name") || "Проект",
        color: PROJECT_COLORS[projectIndex % PROJECT_COLORS.length],
        meta: formatBoardCount(0),
        kanbans: [],
    };
}

function normalizeCreatedKanban(kanban, project) {
    return {
        id: readValue(kanban, "id", "Id"),
        projectId: readValue(kanban, "projectId", "ProjectId") || project.id,
        name: readValue(kanban, "name", "Name") || "Kanban",
    };
}

function selectProject(projectId, { scroll = true } = {}) {
    const project = state.workspace.projects.find((item) => item.id === projectId);

    if (!project) {
        return;
    }

    state.selectedProjectId = project.id;

    syncSelectedProjectUrl();
    renderSidebar();
    markSelectedProject();

    if (scroll) {
        scrollSelectedProjectIntoView();
    }
}

function getSelectedProject() {
    return state.workspace.projects.find((item) => item.id === state.selectedProjectId) ?? null;
}

function markSelectedProject() {
    document.querySelectorAll("[data-project-card]").forEach((panel) => {
        panel.classList.toggle("is-selected", panel.dataset.projectCard === state.selectedProjectId);
    });
}

function scrollSelectedProjectIntoView(options = {}) {
    if (!state.selectedProjectId) {
        return;
    }

    const panel = document.querySelector(`[data-project-card="${cssEscape(state.selectedProjectId)}"]`);

    panel?.scrollIntoView({
        block: "center",
        behavior: options.behavior ?? "smooth",
    });
}

function renderLoading() {
    projectsGrid.innerHTML = `<div class="project-state">Загрузка проектов...</div>`;
}

function renderEmpty(message) {
    projectsGrid.innerHTML = `<div class="project-state">${escapeHtml(message)}</div>`;
}

function renderError(message) {
    projectsGrid.innerHTML = `<div class="project-state">${escapeHtml(message)}</div>`;
}

function renderSidebar() {
    renderSidebarNavigation(
        state.workspace,
        state.selectedProjectId,
        null
    );
}

function updateCreateProjectSubmitState() {
    const isReady = Boolean(newProjectName.value.trim() && newProjectTeam.value);

    createProjectSubmit.disabled = !isReady;
    createProjectSubmit.classList.toggle("is-waiting-selection", !isReady);
    createProjectSubmit.setAttribute("aria-disabled", String(!isReady));
}

function updateCreateBoardSubmitState() {
    const isReady = Boolean(newBoardName.value.trim());

    createBoardSubmit.disabled = !isReady;
    createBoardSubmit.classList.toggle("is-waiting-selection", !isReady);
    createBoardSubmit.setAttribute("aria-disabled", String(!isReady));
}

function setProjectSubmitBusy(button, isBusy) {
    button.disabled = isBusy;
    button.classList.toggle("is-busy", isBusy);
}

function openOverlay(overlay) {
    overlay.classList.add("is-open");
    document.body.classList.add("modal-locked");
}

function closeOverlay(overlay) {
    overlay.classList.remove("is-open");

    const hasOpenedOverlay = document.querySelector(".project-modal-overlay.is-open");

    if (!hasOpenedOverlay) {
        document.body.classList.remove("modal-locked");
    }
}

function showToast(text) {
    const toast = document.getElementById("toast");
    const toastText = document.getElementById("toastText");

    toastText.textContent = text;
    toast.classList.add("is-visible");

    clearTimeout(showToast.timer);

    showToast.timer = setTimeout(() => {
        toast.classList.remove("is-visible");
    }, 2200);
}

function getRequestErrorMessage(error, fallbackText) {
    return error?.message || fallbackText;
}

function syncSelectedProjectUrl() {
    const url = new URL(window.location.href);

    if (state.selectedProjectId) {
        url.searchParams.set("project", state.selectedProjectId);
    } else {
        url.searchParams.delete("project");
    }

    window.history.replaceState({}, "", url);
}

function getProjectFromUrl() {
    const params = new URLSearchParams(window.location.search);
    return params.get("project");
}

function readValue(source, camelKey, pascalKey) {
    return source?.[camelKey] ?? source?.[pascalKey] ?? null;
}

function formatBoardCount(count) {
    const mod10 = count % 10;
    const mod100 = count % 100;

    if (mod10 === 1 && mod100 !== 11) {
        return `${count} доска`;
    }

    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) {
        return `${count} доски`;
    }

    return `${count} досок`;
}

function cssEscape(value) {
    if (window.CSS?.escape) {
        return window.CSS.escape(value);
    }

    return String(value).replaceAll('"', '\\"');
}

window.addEventListener("sidebar:loaded", renderSidebar);

window.addEventListener("project:selected", (event) => {
    selectProject(event.detail.projectId);
});

openCreateProjectBtn.addEventListener("click", openCreateProjectOverlay);
createProjectClose.addEventListener("click", closeCreateProjectOverlay);
createProjectCancel.addEventListener("click", closeCreateProjectOverlay);
newProjectName.addEventListener("input", updateCreateProjectSubmitState);
newProjectTeam.addEventListener("change", updateCreateProjectSubmitState);

createProjectOverlay.addEventListener("click", (event) => {
    if (event.target === createProjectOverlay) {
        closeCreateProjectOverlay();
    }
});

createProjectForm.addEventListener("submit", (event) => {
    event.preventDefault();
    submitCreateProject();
});

createBoardClose.addEventListener("click", closeCreateBoardOverlay);
createBoardCancel.addEventListener("click", closeCreateBoardOverlay);
newBoardName.addEventListener("input", updateCreateBoardSubmitState);

createBoardOverlay.addEventListener("click", (event) => {
    if (event.target === createBoardOverlay) {
        closeCreateBoardOverlay();
    }
});

createBoardForm.addEventListener("submit", (event) => {
    event.preventDefault();
    submitCreateBoard();
});

document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") {
        return;
    }

    if (createBoardOverlay.classList.contains("is-open")) {
        closeCreateBoardOverlay();
        return;
    }

    if (createProjectOverlay.classList.contains("is-open")) {
        closeCreateProjectOverlay();
    }
});

init();
