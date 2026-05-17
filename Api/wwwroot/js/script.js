import {
    escapeHtml,
    formatTimeLeft,
    getBoardTasks,
    getTaskStats,
    loadBoard,
    loadWorkspace,
    renderSidebarNavigation,
    selectProjectFromUrl,
} from "./boardData.js";

import { showFirstLoginTutorial } from "./tutorial.js";

const matrix = document.querySelector("#taskMatrix");

const GROUPS = [
    { label: "Важно / Не срочно", important: true, urgent: false },
    { label: "Важно / Срочно", important: true, urgent: true },
    { label: "Не важно / Не срочно", important: false, urgent: false },
    { label: "Не важно / Срочно", important: false, urgent: true },
];

let state = {
    workspace: { teams: [], projects: [] },
    activeProjectId: null,
    tasks: [],
};
const REALTIME_WORKSPACE_REFRESH_DELAY = 250;
let realtimeWorkspaceRefreshTimer = null;
let realtimeWorkspaceRefreshRequest = null;

function row(task) {
    const el = document.createElement("article");
    el.className = "task-row";
    el.innerHTML = `
    <div class="task-title">${escapeHtml(task.title)}</div>
    <div class="divider"></div>
    <div class="task-meta"><span>приоритет</span><span>${escapeHtml(task.priority)}</span></div>
    <div class="divider"></div>
    <div class="task-meta"><span>Дедлайн через</span><span>${escapeHtml(formatTimeLeft(task.deadline))}</span></div>
  `;
    return el;
}

function renderMatrix(tasks) {
    matrix.innerHTML = "";

    GROUPS.forEach((group) => {
        const q = document.createElement("section");
        q.className = "quadrant";
        q.dataset.label = group.label;

        const groupTasks = tasks.filter((task) =>
            isImportant(task) === group.important &&
            isUrgent(task) === group.urgent
        );

        if (groupTasks.length === 0) {
            q.appendChild(emptyRow("Нет задач"));
        } else {
            groupTasks.slice(0, 4).forEach((task) => {
                q.appendChild(row(task));
            });
        }

        matrix.appendChild(q);
    });
}

function emptyRow(text) {
    const el = document.createElement("p");
    el.className = "empty-state";
    el.textContent = text;

    return el;
}

function isImportant(task) {
    return String(task.priority).toLowerCase() === "high";
}

function isUrgent(task) {
    const deadline = new Date(task.deadline);

    if (Number.isNaN(deadline.getTime())) {
        return false;
    }

    const diffMs = deadline.getTime() - Date.now();

    return diffMs <= 3 * 24 * 60 * 60 * 1000;
}

async function init() {
    renderMatrix([]);

    try {
        const workspace = await loadWorkspace();

        const activeProject = selectProjectFromUrl(workspace.projects);
        
        const boards = await Promise.all(
            workspace.projects.map((project) =>
                loadBoard(project).catch(() => null)
            )
        );

        const tasks = boards
            .filter(Boolean)
            .flatMap((board) =>
                getBoardTasks(board).map((task) => ({
                    ...task,
                    projectId: board.projectId || board.project?.id,
                    project: board.project,
                    kanbanId: board.selectedKanban?.id,
                    kanban: board.selectedKanban,
                }))
            );
        state = {
            workspace,
            activeProjectId: activeProject?.id ?? null,
            tasks,
        };

        renderSidebar();
        renderMatrix(tasks);

        showFirstLoginTutorial();
    } catch (error) {
        console.error(error);

        matrix.innerHTML = "";

        GROUPS.forEach((group) => {
            const q = document.createElement("section");
            q.className = "quadrant";
            q.dataset.label = group.label;

            q.appendChild(emptyRow("Не удалось загрузить API"));

            matrix.appendChild(q);
        });
    }
}

function renderSidebar() {
    renderSidebarNavigation(
        state.workspace,
        state.activeProjectId,
        getTaskStats(state.tasks)
    );
}

function handleRealtimeNotification(notification = {}) {
    const normalizedNotification = normalizeRealtimeNotification(notification);

    if (!shouldRefreshWorkspaceForNotification(normalizedNotification)) {
        return;
    }

    scheduleRealtimeWorkspaceRefresh();
}

function normalizeRealtimeNotification(notification = {}) {
    const source = notification && typeof notification === "object" ? notification : {};

    return {
        teamId: readValue(source, "teamId", "TeamId") || "",
        projectId: readValue(source, "projectId", "ProjectId") || "",
        kanbanId: readValue(source, "kanbanId", "KanbanId") || "",
        type: readValue(source, "type", "Type") || readValue(source, "eventType", "EventType") || "",
        name: readValue(source, "name", "Name") || "",
    };
}

function shouldRefreshWorkspaceForNotification(notification) {
    return notification.type === "TeamMemberAdded" ||
        notification.type === "ProjectCreated" ||
        notification.type === "KanbanCreated" ||
        notification.name === "Team Member Added" ||
        Boolean(notification.teamId && (notification.projectId || notification.kanbanId));
}

function scheduleRealtimeWorkspaceRefresh() {
    window.clearTimeout(realtimeWorkspaceRefreshTimer);
    realtimeWorkspaceRefreshTimer = window.setTimeout(() => {
        refreshWorkspaceFromRealtime().catch((error) => {
            console.warn("Failed to refresh workspace from realtime notification:", error);
        });
    }, REALTIME_WORKSPACE_REFRESH_DELAY);
}

async function refreshWorkspaceFromRealtime() {
    if (realtimeWorkspaceRefreshRequest) {
        return realtimeWorkspaceRefreshRequest;
    }

    realtimeWorkspaceRefreshRequest = (async () => {
        const workspace = await loadWorkspace();
        const activeProject = selectProjectFromUrl(workspace.projects);
        const boards = await Promise.all(workspace.projects.map((project) => loadBoard(project).catch(() => null)));
        const tasks = boards.filter(Boolean).flatMap(getBoardTasks);

        state = {
            workspace,
            activeProjectId: activeProject?.id ?? null,
            tasks,
        };

        renderSidebar();
        renderMatrix(tasks);
    })().finally(() => {
        realtimeWorkspaceRefreshRequest = null;
    });

    return realtimeWorkspaceRefreshRequest;
}

function readValue(source, camelKey, pascalKey) {
    return source?.[camelKey] ?? source?.[pascalKey] ?? null;
}

window.addEventListener("sidebar:loaded", renderSidebar);

init();

try {
    if (typeof window.connectNotifications === "function") {
        window.connectNotifications(handleRealtimeNotification);
    }
} catch (error) {
    console.error("Не удалось подключить уведомления:", error);
}
