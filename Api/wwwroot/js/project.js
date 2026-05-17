import {
    escapeHtml,
    loadWorkspace,
    renderSidebarNavigation,
    selectProjectFromUrl,
} from "./boardData.js";

const projectTitle = document.getElementById("projectTitle");
const projectTeam = document.getElementById("projectTeam");
const kanbanList = document.getElementById("kanbanList");

let state = {
    workspace: {
        teams: [],
        projects: [],
    },
    activeProject: null,
};

async function init() {
    renderLoading();

    try {
        const workspace = await loadWorkspace();
        const activeProject = selectProjectFromUrl(workspace.projects);

        state.workspace = workspace;
        state.activeProject = activeProject;

        renderSidebar();
        renderProject();
    } catch (error) {
        console.error(error);
        renderError("Не удалось загрузить проекты. Проверь авторизацию и доступ к API.");
    }
}

function renderProject() {
    const project = state.activeProject;

    if (!project) {
        projectTitle.textContent = "Проект не найден";
        projectTeam.textContent = "Boardify";
        renderEmpty("Проектов пока нет");
        return;
    }

    projectTitle.textContent = project.name || "Проект";
    projectTeam.textContent = project.team?.name
        ? `Команда: ${project.team.name}`
        : "Проект";

    updateProjectUrl(project.id);
    renderKanbans(project);
}

function renderKanbans(project) {
    const kanbans = project.kanbans ?? [];

    kanbanList.innerHTML = "";

    if (kanbans.length === 0) {
        renderEmpty("У этого проекта пока нет канбан-досок");
        return;
    }

    kanbans.forEach((kanban, index) => {
        kanbanList.appendChild(createKanbanCard(project, kanban, index));
    });
}

function createKanbanCard(project, kanban, index) {
    const link = document.createElement("a");

    const color = pickColor(index);
    const href = `./kanban.html?project=${encodeURIComponent(project.id)}&kanban=${encodeURIComponent(kanban.id)}`;

    link.className = "kanban-card";
    link.href = href;

    link.innerHTML = `
        <span class="kanban-dot" style="background:${escapeHtml(color)}"></span>

        <span>
            <h2 class="kanban-card-title">${escapeHtml(kanban.name || "Kanban")}</h2>
            <span class="kanban-card-meta">Открыть доску проекта</span>
        </span>

        <span class="kanban-open" aria-hidden="true">→</span>
    `;

    return link;
}

function renderLoading() {
    projectTitle.textContent = "Доски проекта";
    projectTeam.textContent = "Загрузка...";
    kanbanList.innerHTML = `<div class="project-state">Загрузка канбанов...</div>`;
}

function renderEmpty(message) {
    kanbanList.innerHTML = `<div class="project-state">${escapeHtml(message)}</div>`;
}

function renderError(message) {
    projectTitle.textContent = "Ошибка";
    projectTeam.textContent = "Boardify";
    kanbanList.innerHTML = `<div class="project-state">${escapeHtml(message)}</div>`;
}

function renderSidebar() {
    renderSidebarNavigation(
        state.workspace,
        state.activeProject?.id ?? null,
        null
    );
}

function updateProjectUrl(projectId) {
    if (!projectId) {
        return;
    }

    const url = new URL(window.location.href);
    url.searchParams.set("project", projectId);
    window.history.replaceState({}, "", url);
}

function pickColor(index) {
    const colors = ["#f4864d", "#42609f", "#ffe3d8", "#407d52", "#e3a5a8"];
    return colors[index % colors.length];
}

window.addEventListener("sidebar:loaded", renderSidebar);

window.addEventListener("project:selected", (event) => {
    const project = state.workspace.projects.find(
        (item) => item.id === event.detail.projectId
    );

    if (!project) {
        return;
    }

    state.activeProject = project;
    renderSidebar();
    renderProject();
});

init();