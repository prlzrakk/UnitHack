import {
    escapeHtml,
    formatDate,
    formatTime,
    formatTimeLeft,
    getTaskStats,
    loadBoard,
    loadWorkspace,
    renderSidebarNavigation,
} from "./boardData.js";

import { getMe } from "./api/userApi.js";

const myTasksUser = document.getElementById("myTasksUser");
const myTasksTitle = document.getElementById("myTasksTitle");
const myTasksGrid = document.getElementById("myTasksGrid");
const filterButtons = document.querySelectorAll("[data-filter]");

let state = {
    workspace: {
        teams: [],
        projects: [],
    },
    currentUser: null,
    tasks: [],
    activeFilter: getFilterFromUrl(),
};

async function init() {
    renderLoading();

    try {
        const [workspace, currentUser] = await Promise.all([
            loadWorkspace(),
            getMe().catch(() => null),
        ]);

        state.workspace = workspace;
        state.currentUser = currentUser;
        state.tasks = await loadMyTasks(workspace, currentUser);

        renderSidebar();
        renderPage();
    } catch (error) {
        console.error(error);
        renderError("Не удалось загрузить мои задачи. Проверь авторизацию и доступ к API.");
    }
}

async function loadMyTasks(workspace, currentUser) {
    const currentUserId = readId(currentUser);

    const allTasks = [];

    const boardRequests = workspace.projects.flatMap((project) => {
        const kanbans = project.kanbans?.length ? project.kanbans : [null];

        return kanbans.map(async (kanban) => {
            const board = await loadBoard(project, kanban?.id ?? null).catch(() => null);

            if (!board?.columns) {
                return;
            }

            board.columns.forEach((column) => {
                (column.tasks ?? []).forEach((task) => {
                    if (!isMyTask(task, currentUserId)) {
                        return;
                    }

                    allTasks.push({
                        ...task,
                        project,
                        kanban: board.selectedKanban ?? kanban,
                        boardTitle: board.title,
                        column,
                    });
                });
            });
        });
    });

    await Promise.all(boardRequests);

    return allTasks.sort((a, b) => {
        const dateA = new Date(a.deadline).getTime();
        const dateB = new Date(b.deadline).getTime();

        if (Number.isNaN(dateA) && Number.isNaN(dateB)) {
            return 0;
        }

        if (Number.isNaN(dateA)) {
            return 1;
        }

        if (Number.isNaN(dateB)) {
            return -1;
        }

        return dateA - dateB;
    });
}

function isMyTask(task, currentUserId) {
    if (!currentUserId) {
        return true;
    }

    const taskUserId = task.userId ?? task.UserId ?? null;

    if (!taskUserId) {
        return false;
    }

    return String(taskUserId) === String(currentUserId);
}

function renderPage() {
    const userName =
        readValue(state.currentUser, "name", "Name") ||
        readValue(state.currentUser, "userName", "UserName") ||
        readValue(state.currentUser, "email", "Email") ||
        "Пользователь";

    myTasksUser.textContent = userName;
    myTasksTitle.textContent = getTitleByFilter(state.activeFilter);

    updateFilterButtons();
    updateUrlFilter();

    const tasks = filterTasks(state.tasks, state.activeFilter);

    if (tasks.length === 0) {
        renderEmpty(getEmptyText(state.activeFilter));
        return;
    }

    myTasksGrid.innerHTML = "";
    tasks.forEach((task) => {
        myTasksGrid.appendChild(createTaskCard(task));
    });
}

function createTaskCard(task) {
    const link = document.createElement("a");

    const projectId = task.project?.id ?? "";
    const kanbanId = task.kanban?.id ?? "";
    const href = `./kanban.html?project=${encodeURIComponent(projectId)}&kanban=${encodeURIComponent(kanbanId)}`;

    link.className = "my-task-card";
    link.href = href;

    const description = task.description || "Описание не добавлено";
    const projectName = task.project?.name || "Проект";
    const kanbanName = task.kanban?.name || task.boardTitle || "Kanban";
    const columnName = task.column?.title || "Колонка";

    link.innerHTML = `
        <div class="my-task-main">
            <h2 class="my-task-title">${escapeHtml(task.title || "Задача")}</h2>

            <p class="my-task-description">
                ${escapeHtml(description)}
            </p>

            <div class="my-task-meta">
                <span class="my-task-pill orange">${escapeHtml(task.priority || "Medium")}</span>
                <span class="my-task-pill blue">${escapeHtml(projectName)}</span>
                <span class="my-task-pill">${escapeHtml(kanbanName)}</span>
                <span class="my-task-pill green">${escapeHtml(columnName)}</span>
                <span class="my-task-pill">${escapeHtml(formatDate(task.deadline))}</span>
                <span class="my-task-pill">${escapeHtml(formatTime(task.deadline))}</span>
            </div>
        </div>

        <div class="my-task-side">
            <div class="my-task-deadline">
                <span>Дедлайн через</span>
                <span>${escapeHtml(formatTimeLeft(task.deadline))}</span>
            </div>

            <span class="my-task-open" aria-hidden="true">→</span>
        </div>
    `;

    return link;
}

function filterTasks(tasks, filter) {
    const now = new Date();
    const weekEnd = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    const urgentEnd = new Date(now.getTime() + 3 * 24 * 60 * 60 * 1000);

    return tasks.filter((task) => {
        const deadline = new Date(task.deadline);

        if (filter === "all") {
            return true;
        }

        if (Number.isNaN(deadline.getTime())) {
            return false;
        }

        if (filter === "overdue") {
            return deadline < now;
        }

        if (filter === "urgent") {
            return deadline >= now && deadline <= urgentEnd;
        }

        if (filter === "week") {
            return deadline >= now && deadline <= weekEnd;
        }

        return true;
    });
}

function renderLoading() {
    myTasksUser.textContent = "Загрузка...";
    myTasksTitle.textContent = "Мои задачи";
    myTasksGrid.innerHTML = `<div class="my-task-state">Загрузка задач...</div>`;
}

function renderEmpty(message) {
    myTasksGrid.innerHTML = `<div class="my-task-state">${escapeHtml(message)}</div>`;
}

function renderError(message) {
    myTasksUser.textContent = "Boardify";
    myTasksTitle.textContent = "Ошибка";
    myTasksGrid.innerHTML = `<div class="my-task-state">${escapeHtml(message)}</div>`;
}

function renderSidebar() {
    renderSidebarNavigation(
        state.workspace,
        null,
        getTaskStats(state.tasks)
    );
}

function getFilterFromUrl() {
    const params = new URLSearchParams(window.location.search);
    const filter = params.get("filter");

    if (["all", "urgent", "week", "overdue"].includes(filter)) {
        return filter;
    }

    return "all";
}

function updateUrlFilter() {
    const url = new URL(window.location.href);
    url.searchParams.set("filter", state.activeFilter);
    window.history.replaceState({}, "", url);
}

function updateFilterButtons() {
    filterButtons.forEach((button) => {
        button.classList.toggle("is-active", button.dataset.filter === state.activeFilter);
    });
}

function getTitleByFilter(filter) {
    const titles = {
        all: "Мои задачи",
        urgent: "Срочные задачи",
        week: "Задачи на неделе",
        overdue: "Просроченные задачи",
    };

    return titles[filter] ?? "Мои задачи";
}

function getEmptyText(filter) {
    const messages = {
        all: "У тебя пока нет задач",
        urgent: "Срочных задач пока нет",
        week: "Задач на этой неделе пока нет",
        overdue: "Просроченных задач нет",
    };

    return messages[filter] ?? "Задач пока нет";
}

function readId(source) {
    return readValue(source, "id", "Id");
}

function readValue(source, camelKey, pascalKey) {
    return source?.[camelKey] ?? source?.[pascalKey] ?? "";
}

filterButtons.forEach((button) => {
    button.addEventListener("click", () => {
        state.activeFilter = button.dataset.filter || "all";
        renderPage();
    });
});

window.addEventListener("sidebar:loaded", renderSidebar);

window.addEventListener("mytasks:selected", (event) => {
    state.activeFilter = event.detail.filter || "all";
    renderPage();
});

init();