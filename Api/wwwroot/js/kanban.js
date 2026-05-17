import {
    escapeHtml,
    formatDate,
    formatTime,
    getBoardTasks,
    getTaskStats,
    loadBoard,
    loadWorkspace,
    renderSidebarNavigation,
    selectKanbanFromUrl,
    selectProjectFromUrl,
    updateBoardUrl,
} from "./boardData.js";

import {
    createColumn,
    deleteColumn as deleteColumnRequest,
    renameColumn,
} from "./api/columnApi.js";

import {
    createTask,
    deleteTask as deleteTaskRequest,
    moveTask,
    updateTask,
} from "./api/taskApi.js";

import { getMe } from "./api/userApi.js";

import {
    getNotifications,
    getUnreadNotifications,
    readAllNotifications,
    readNotification,
} from "./api/notificationApi.js";

/* =========================
   DOM
========================= */

const toggleKanbanSidebarBtn = document.getElementById("toggleKanbanSidebar");
const closeKanbanSidebarBtn = document.getElementById("closeKanbanSidebar");

const kanbanTitle = document.getElementById("kanbanTitle");
const kanbanBoard = document.getElementById("kanbanBoard");

/* Notifications */
const notificationOpen = document.getElementById("notificationOpen");
const notificationOverlay = document.getElementById("notificationOverlay");
const notificationPanel = notificationOverlay?.querySelector(".notification-panel");
const notificationClose = document.getElementById("notificationClose");
const notificationUnreadOnly = document.getElementById("notificationUnreadOnly");
const notificationReadAll = document.getElementById("notificationReadAll");
const notificationList = document.getElementById("notificationList");
const notificationBadge = document.getElementById("notificationBadge");

/* Reminder modal */
const reminderModal = document.getElementById("reminderModal");
const reminderText = document.getElementById("reminderText");
const reminderYes = document.getElementById("reminderYes");
const reminderNo = document.getElementById("reminderNo");
const reminderClose = document.getElementById("reminderClose");

/* Create task modal */
const createTaskOverlay = document.getElementById("createTaskOverlay");
const createTaskForm = document.getElementById("createTaskForm");
const createTaskClose = document.getElementById("createTaskClose");
const createTaskCancel = document.getElementById("createTaskCancel");

const newTaskTitle = document.getElementById("newTaskTitle");
const newTaskDescription = document.getElementById("newTaskDescription");
const newTaskAssigneeSearch = document.getElementById("newTaskAssigneeSearch");
const assigneeOptions = document.getElementById("assigneeOptions");
const selectedAssigneesContainer = document.getElementById("selectedAssignees");

const newTaskPriority = document.getElementById("newTaskPriority");
const newTaskComplexity = document.getElementById("newTaskComplexity");
const newTaskDeadline = document.getElementById("newTaskDeadline");

const newTaskTagInput = document.getElementById("newTaskTagInput");
const addTaskTagBtn = document.getElementById("addTaskTagBtn");
const createdTags = document.getElementById("createdTags");
const createSubtaskList = document.getElementById("createSubtaskList");
const createSubtaskInput = document.getElementById("createSubtaskInput");
const createSubtaskAssignee = document.getElementById("createSubtaskAssignee");
const createSubtaskAdd = document.getElementById("createSubtaskAdd");

const TASK_ASSIGNEES_STORAGE_KEY = "boardifyTaskAssignees";
const TASK_DRAG_THRESHOLD = 6;
const TASK_ORDER_STEP = 1000;
const TASK_SUBTASKS_STORAGE_KEY = "boardifyTaskSubtasks";
const REALTIME_BOARD_REFRESH_DELAY = 250;
const REALTIME_WORKSPACE_REFRESH_DELAY = 250;

/* Task detail modal */
const taskDetailOverlay = document.getElementById("taskDetailOverlay");
const taskDetailForm = document.getElementById("taskDetailForm");
const taskDetailClose = document.getElementById("taskDetailClose");
const taskDetailCancel = document.getElementById("taskDetailCancel");

const detailTaskTitle = document.getElementById("detailTaskTitle");
const detailTaskDescription = document.getElementById("detailTaskDescription");
const detailTaskAssignee = document.getElementById("detailTaskAssignee");
const detailTaskPriority = document.getElementById("detailTaskPriority");
const detailTaskComplexity = document.getElementById("detailTaskComplexity");
const detailTaskDeadline = document.getElementById("detailTaskDeadline");
const detailSubtaskList = document.getElementById("detailSubtaskList");
const detailSubtaskInput = document.getElementById("detailSubtaskInput");
const detailSubtaskAdd = document.getElementById("detailSubtaskAdd");

/* =========================
   STATE
========================= */

let state = {
    workspace: {
        teams: [],
        projects: [],
    },
    activeProject: null,
    activeKanban: null,
    board: null,
    currentUser: null,
    notifications: [],
    notificationsUnreadOnly: notificationUnreadOnly?.checked ?? true,
    notificationUnreadCount: 0,
};

let selectedReminder = null;
let selectedColumnForNewTask = null;
let selectedAssignees = [];
let selectedTags = [];
let createSubtasks = [];
let selectedTaskForDetail = null;
let detailSubtasks = [];
let draggedTaskId = null;
let dragPlaceholder = null;
let dragPreview = null;
let pointerDragState = null;
let suppressTaskClick = false;
const movingTaskIds = new Set();
let realtimeBoardRefreshTimer = null;
let realtimeWorkspaceRefreshTimer = null;
let workspaceRefreshRequest = null;

/* =========================
   SIDEBAR
========================= */

function openKanbanSidebar() {
    document.body.classList.remove("sidebar-closed");
}

function closeKanbanSidebar() {
    document.body.classList.add("sidebar-closed");
}

function toggleKanbanSidebar() {
    document.body.classList.toggle("sidebar-closed");
}

toggleKanbanSidebarBtn?.addEventListener("click", toggleKanbanSidebar);
closeKanbanSidebarBtn?.addEventListener("click", closeKanbanSidebar);

/* =========================
   INIT
========================= */

async function init() {
    renderLoading();

    try {
        const [workspace, currentUser] = await Promise.all([
            loadWorkspace(),
            getMe().catch(() => null),
        ]);

        state.workspace = workspace;
        state.currentUser = currentUser;
        state.activeProject = selectProjectFromUrl(workspace.projects);

        await loadActiveBoard(selectKanbanFromUrl(state.activeProject)?.id ?? null);
        refreshUnreadNotificationsCount().catch((error) => {
            console.warn("Не удалось загрузить счетчик уведомлений:", error);
        });
    } catch (error) {
        console.error(error);
        renderError("Не удалось загрузить канбан. Проверь авторизацию и доступ к API.");
    }
}

async function loadActiveBoard(kanbanId = null) {
    if (!state.activeProject) {
        state.board = null;
        renderSidebar();
        renderEmpty("Проектов пока нет");
        return;
    }

    renderLoading();

    state.activeKanban =
        selectKanbanFromUrl(state.activeProject) ??
        state.activeProject.kanbans?.[0] ??
        null;

    if (kanbanId) {
        state.activeKanban =
            state.activeProject.kanbans?.find((kanban) => kanban.id === kanbanId) ??
            state.activeKanban;
    }

    state.board = hydrateBoardTaskAssignees(
        await loadBoard(state.activeProject, state.activeKanban?.id ?? null)
    );
    state.activeKanban = state.board.selectedKanban ?? state.activeKanban;

    updateBoardUrl(state.activeProject, state.activeKanban);

    renderSidebar();
    renderKanban(state.board);
}

async function reloadBoard(toastText = null) {
    await loadActiveBoard(state.activeKanban?.id ?? null);

    if (toastText) {
        showToast(toastText);
    }
}

/* =========================
   BASIC RENDER
========================= */

function renderSidebar() {
    const tasks = getBoardTasks(state.board);
    renderSidebarNavigation(
        state.workspace,
        state.activeProject?.id,
        getTaskStats(tasks)
    );
}

function renderLoading() {
    kanbanTitle.textContent = "Kanban";
    kanbanBoard.innerHTML = `<div class="kanban-state">Загрузка канбана...</div>`;
}

function renderError(message) {
    kanbanTitle.textContent = "Kanban";
    kanbanBoard.innerHTML = `
        <div class="kanban-state kanban-state-error">
            ${escapeHtml(message)}
        </div>
    `;
}

function renderEmpty(message) {
    kanbanTitle.textContent = "Kanban";
    kanbanBoard.innerHTML = `
        <div class="kanban-state">
            ${escapeHtml(message)}
        </div>
    `;
}

/* =========================
   KANBAN RENDER
========================= */

function renderKanban(board) {
    kanbanTitle.textContent = board.title || "Kanban";
    kanbanBoard.innerHTML = "";
    const canRenameColumns = isActiveProjectTeamAdmin();

    if (!board.id) {
        renderEmpty("У проекта пока нет канбанов");
        return;
    }

    board.columns.forEach((column, columnIndex) => {
        const columnEl = document.createElement("section");
        columnEl.className = "kanban-column";
        columnEl.dataset.columnId = column.id;

        columnEl.innerHTML = `
            <div class="column-head">
                <h2
                    class="column-title"
                    data-column-title
                    data-column-id="${escapeAttr(column.id)}"
                >
                    ${escapeHtml(column.title || "Без названия")}
                </h2>

                <span class="column-count">${column.tasks.length}</span>

                <button
                    class="column-icon column-delete-btn"
                    type="button"
                    aria-label="Удалить колонку"
                >
                    <img
                        class="column-icon-img"
                        src="components/images/delete.svg"
                        alt="удалить"
                    >
                </button>

                <button
                    class="column-icon column-edit-btn"
                    type="button"
                    aria-label="Редактировать колонку"
                >
                    ✎
                </button>
            </div>
        `;

        column.tasks.forEach((task, taskIndex) => {
            columnEl.appendChild(createTaskCard(task, column, columnIndex, taskIndex));
        });

        const addTask = document.createElement("button");
        addTask.className = "add-task-btn";
        addTask.type = "button";
        addTask.innerHTML = `<span>＋</span><span>Добавить</span>`;

        addTask.addEventListener("click", () => {
            openCreateTaskModal(column);
        });

        columnEl.appendChild(addTask);

        const deleteColumnBtn = columnEl.querySelector(".column-delete-btn");
        const editColumnBtn = columnEl.querySelector(".column-edit-btn");
        const columnTitle = columnEl.querySelector("[data-column-title]");

        if (!canRenameColumns) {
            editColumnBtn?.remove();
        }

        deleteColumnBtn.addEventListener("click", (event) => {
            event.stopPropagation();
            deleteColumn(column);
        });

        editColumnBtn?.addEventListener("click", (event) => {
            event.stopPropagation();
            startEditColumnTitle(columnTitle, column);
        });

        kanbanBoard.appendChild(columnEl);
    });

    const addColumn = document.createElement("button");
    addColumn.className = "add-column-btn";
    addColumn.type = "button";
    addColumn.innerHTML = `<span>＋</span><span>Добавить</span>`;

    addColumn.addEventListener("click", addColumnToBoard);

    kanbanBoard.appendChild(addColumn);
}

/* =========================
   COLUMNS
========================= */

async function addColumnToBoard() {
    if (!state.board?.id) {
        showToast("Сначала выбери канбан");
        return;
    }

    try {
        const createdColumn = await createColumn(state.board.id, "Без названия");

        await reloadBoard("Колонка «Без названия» добавлена");

        setTimeout(() => {
            const columnId = createdColumn?.id ?? createdColumn?.Id;
            const titleEl = columnId
                ? document.querySelector(`[data-column-id="${columnId}"]`)
                : document.querySelectorAll("[data-column-title]").item(
                    document.querySelectorAll("[data-column-title]").length - 1
                );

            const column = state.board?.columns?.find((item) => item.id === columnId) ??
                state.board?.columns?.[state.board.columns.length - 1];

            if (titleEl && column) {
                startEditColumnTitle(titleEl, column);
            }

            kanbanBoard.scrollTo({
                left: kanbanBoard.scrollWidth,
                behavior: "smooth",
            });
        }, 0);
    } catch (error) {
        handleActionError(error, "Не удалось добавить колонку");
    }
}

function startEditColumnTitle(titleEl, column) {
    if (!isActiveProjectTeamAdmin()) {
        showToast("Переименовывать колонки может только админ команды");
        return;
    }

    if (!titleEl || !column) {
        return;
    }

    const oldTitle = column.title || "Без названия";

    titleEl.contentEditable = "true";
    titleEl.classList.add("is-editing");
    titleEl.focus();

    selectElementText(titleEl);

    async function save() {
        const newTitle = titleEl.textContent.trim() || "Без названия";

        titleEl.contentEditable = "false";
        titleEl.classList.remove("is-editing");

        cleanup();

        if (newTitle === oldTitle) {
            titleEl.textContent = oldTitle;
            return;
        }

        try {
            await renameColumn(column.id, newTitle);
            await reloadBoard(`Колонка переименована в «${newTitle}»`);
        } catch (error) {
            titleEl.textContent = oldTitle;
            handleActionError(error, "Не удалось переименовать колонку");
        }
    }

    function cancel() {
        titleEl.contentEditable = "false";
        titleEl.classList.remove("is-editing");
        titleEl.textContent = oldTitle;

        cleanup();
    }

    function handleKeydown(event) {
        if (event.key === "Enter") {
            event.preventDefault();
            save();
        }

        if (event.key === "Escape") {
            event.preventDefault();
            cancel();
        }
    }

    function handleBlur() {
        save();
    }

    function cleanup() {
        titleEl.removeEventListener("keydown", handleKeydown);
        titleEl.removeEventListener("blur", handleBlur);
    }

    titleEl.addEventListener("keydown", handleKeydown);
    titleEl.addEventListener("blur", handleBlur);
}

async function deleteColumn(column) {
    const confirmed = confirm(
        `Удалить колонку «${column.title}» вместе с задачами: ${column.tasks.length}?`
    );

    if (!confirmed) {
        return;
    }

    try {
        await deleteColumnRequest(column.id);
        await reloadBoard(`Колонка «${column.title}» удалена`);
    } catch (error) {
        handleActionError(error, "Не удалось удалить колонку");
    }
}

function selectElementText(element) {
    const range = document.createRange();
    range.selectNodeContents(element);

    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
}

/* =========================
   TASK CARDS
========================= */

function createTaskCard(task, column, columnIndex, taskIndex) {
    const color = task.color || column.color || "#ef6a35";
    const doneLabel = column.done ? "Дата выполнения" : "Дедлайн";

    const tags = [
        task.priority,
        task.time,
        ...normalizeTaskTags(task.tags),
    ].filter(Boolean);

    const card = document.createElement("article");
    card.className = "task-card is-draggable";
    card.style.setProperty("--task-color", color);
    card.dataset.taskId = task.id;
    card.dataset.columnId = column.id;
    card.draggable = false;
    card.tabIndex = 0;
    card.setAttribute("role", "button");
    if (movingTaskIds.has(task.id)) {
        card.classList.add("is-moving");
        card.setAttribute("aria-busy", "true");
    }
    card.setAttribute("aria-label", `Открыть задачу ${task.title}`);

    card.innerHTML = `
        <div class="task-top">
            <button
                class="task-icon-btn task-edit-btn"
                type="button"
                aria-label="Редактировать задачу"
            >
                ✎
            </button>

            <h3 class="task-title">${escapeHtml(task.title)}</h3>

            <button
                class="task-icon-btn task-delete-btn"
                type="button"
                aria-label="Удалить задачу"
                data-column-index="${columnIndex}"
                data-task-index="${taskIndex}"
            >
                <img
                    class="task-icon-img"
                    src="components/images/delete.svg"
                    alt="удалить"
                >
            </button>
        </div>

        <div class="task-dates">
            <div class="date-label">Дата создания</div>
            <div class="date-label">${doneLabel}</div>

            <div class="date-pills">
                <span class="date-pill">${escapeHtml(formatDate(task.createdAt))}</span>
                <span class="date-pill">${escapeHtml(formatTime(task.createdAt))}</span>
            </div>

            <div class="date-pills">
                <span class="date-pill">${escapeHtml(formatDate(task.deadline))}</span>
                <span class="date-pill">${escapeHtml(formatTime(task.deadline))}</span>
            </div>
        </div>

        <div class="task-description">
            ${escapeHtml(task.description || "").replace(/\n/g, "<br>")}
        </div>

        <div class="task-footer">
            <div class="task-tags">
                ${tags
        .map((tag) => {
            const text = String(tag).startsWith("#") ? tag : tag;
            return `<span class="task-tag">${escapeHtml(text)}</span>`;
        })
        .join("")}
            </div>

            <div class="task-users">
                ${getTaskUsers(task.users)
        .map((user) => `
                        <button
                            class="task-user-wrap"
                            type="button"
                            data-user-name="${escapeAttr(user.name)}"
                            data-task-title="${escapeAttr(task.title)}"
                            title="${escapeAttr(user.name)}"
                            aria-label="Напомнить пользователю ${escapeAttr(user.name)}"
                        >
                            <img
                                class="task-user"
                                src="components/images/user.svg"
                                alt="${escapeAttr(user.name)}"
                            >
                        </button>
                    `)
        .join("")}
            </div>
        </div>
    `;

    card.querySelector(".task-edit-btn").addEventListener("click", (event) => {
        event.stopPropagation();
        openTaskDetailOverlay(task);
    });

    card.querySelector(".task-delete-btn").addEventListener("click", (event) => {
        event.stopPropagation();
        deleteTask(task);
    });

    card.querySelectorAll(".task-user-wrap").forEach((userButton) => {
        userButton.addEventListener("click", (event) => {
            event.stopPropagation();

            openReminderModal({
                userName: userButton.dataset.userName,
                taskTitle: userButton.dataset.taskTitle,
            });
        });
    });

    card.addEventListener("pointerdown", (event) => {
        startTaskPointerDrag(event, task, card);
    });

    card.addEventListener("click", () => {
        if (draggedTaskId || suppressTaskClick) {
            return;
        }

        openTaskDetailOverlay(task);
    });

    card.addEventListener("keydown", (event) => {
        if (event.key !== "Enter" && event.key !== " ") {
            return;
        }

        event.preventDefault();
        openTaskDetailOverlay(task);
    });

    return card;
}

function startTaskPointerDrag(event, task, card) {
    const startedOnInteractiveElement =
        event.target instanceof Element &&
        event.target.closest("button, input, textarea, select, a");

    if (event.button !== 0 || movingTaskIds.has(task.id) || startedOnInteractiveElement) {
        return;
    }

    pointerDragState = {
        active: false,
        card,
        pointerId: event.pointerId,
        startX: event.clientX,
        startY: event.clientY,
        taskId: task.id,
        offsetX: 0,
        offsetY: 0,
    };

    document.addEventListener("pointermove", handleTaskPointerMove);
    document.addEventListener("pointerup", handleTaskPointerUp);
    document.addEventListener("pointercancel", cancelTaskPointerDrag);
}

function handleTaskPointerMove(event) {
    if (!pointerDragState || pointerDragState.pointerId !== event.pointerId) {
        return;
    }

    const deltaX = event.clientX - pointerDragState.startX;
    const deltaY = event.clientY - pointerDragState.startY;
    const distance = Math.hypot(deltaX, deltaY);

    if (!pointerDragState.active) {
        if (distance < TASK_DRAG_THRESHOLD) {
            return;
        }

        pointerDragState.active = true;
        draggedTaskId = pointerDragState.taskId;
        suppressTaskClick = true;
        beginTaskPointerDrag(event);
    }

    event.preventDefault();
    updateTaskDragPreview(event.clientX, event.clientY);
    autoScrollKanbanBoard(event.clientX);

    const target = document.elementFromPoint(event.clientX, event.clientY);
    const columnEl = getClosestKanbanColumn(target);

    if (!columnEl || !state.board?.columns?.some((column) => column.id === columnEl.dataset.columnId)) {
        clearTaskDropTarget();
        return;
    }

    moveTaskPlaceholder(columnEl, event.clientY);
}

async function handleTaskPointerUp(event) {
    if (!pointerDragState || pointerDragState.pointerId !== event.pointerId) {
        return;
    }

    const { active, taskId } = pointerDragState;
    stopTaskPointerDragListeners();

    if (!active) {
        pointerDragState = null;
        return;
    }

    event.preventDefault();

    const target = document.elementFromPoint(event.clientX, event.clientY);
    const columnEl = dragPlaceholder?.parentElement ?? getClosestKanbanColumn(target);
    const toColumnId = columnEl?.dataset.columnId ?? null;
    const dropIndex = columnEl ? getTaskDropIndex(columnEl) : null;

    pointerDragState = null;
    cleanupTaskDrag();

    if (!toColumnId || dropIndex === null) {
        return;
    }

    await moveTaskToColumn(taskId, toColumnId, dropIndex);
}

function cancelTaskPointerDrag() {
    stopTaskPointerDragListeners();
    pointerDragState = null;
    cleanupTaskDrag();
}

function stopTaskPointerDragListeners() {
    document.removeEventListener("pointermove", handleTaskPointerMove);
    document.removeEventListener("pointerup", handleTaskPointerUp);
    document.removeEventListener("pointercancel", cancelTaskPointerDrag);
}

function beginTaskPointerDrag(event) {
    if (!pointerDragState) {
        return;
    }

    const { card } = pointerDragState;
    const rect = card.getBoundingClientRect();

    pointerDragState.offsetX = event.clientX - rect.left;
    pointerDragState.offsetY = event.clientY - rect.top;

    card.classList.add("is-dragging");

    dragPreview = card.cloneNode(true);
    dragPreview.classList.remove("is-dragging");
    dragPreview.classList.add("task-drag-preview");
    dragPreview.setAttribute("aria-hidden", "true");
    dragPreview.style.width = `${rect.width}px`;

    document.body.appendChild(dragPreview);
    updateTaskDragPreview(event.clientX, event.clientY);
}

function updateTaskDragPreview(clientX, clientY) {
    if (!dragPreview || !pointerDragState) {
        return;
    }

    const left = clientX - pointerDragState.offsetX;
    const top = clientY - pointerDragState.offsetY;
    dragPreview.style.transform = `translate3d(${Math.round(left)}px, ${Math.round(top)}px, 0)`;
}

function autoScrollKanbanBoard(clientX) {
    const rect = kanbanBoard.getBoundingClientRect();
    const scrollZone = 88;
    const maxSpeed = 24;

    if (clientX < rect.left + scrollZone) {
        kanbanBoard.scrollLeft -= maxSpeed;
        return;
    }

    if (clientX > rect.right - scrollZone) {
        kanbanBoard.scrollLeft += maxSpeed;
    }
}

function handleTaskDragOver(event) {
    if (!draggedTaskId) {
        return;
    }

    const columnEl = getClosestKanbanColumn(event.target);

    if (!columnEl || !state.board?.columns?.some((column) => column.id === columnEl.dataset.columnId)) {
        return;
    }

    event.preventDefault();
    event.dataTransfer.dropEffect = "move";

    moveTaskPlaceholder(columnEl, event.clientY);
}

function handleTaskDragLeave(event) {
    const columnEl = getClosestKanbanColumn(event.target);

    if (!columnEl) {
        return;
    }

    if (event.relatedTarget instanceof Node && columnEl.contains(event.relatedTarget)) {
        return;
    }

    columnEl.classList.remove("is-drag-over");
}

async function handleTaskDrop(event) {
    if (!draggedTaskId) {
        return;
    }

    const columnEl = getClosestKanbanColumn(event.target);

    if (!columnEl) {
        cleanupTaskDrag();
        return;
    }

    event.preventDefault();

    if (dragPlaceholder?.parentElement !== columnEl) {
        moveTaskPlaceholder(columnEl, event.clientY);
    }

    const taskId = draggedTaskId;
    const toColumnId = columnEl.dataset.columnId;
    const dropIndex = getTaskDropIndex(columnEl);

    cleanupTaskDrag();

    await moveTaskToColumn(taskId, toColumnId, dropIndex);
}

function getClosestKanbanColumn(target) {
    return target instanceof Element ? target.closest(".kanban-column") : null;
}

function moveTaskPlaceholder(columnEl, pointerY) {
    const placeholder = getTaskPlaceholder();
    const afterElement = getTaskDragAfterElement(columnEl, pointerY);
    const addTaskButton = columnEl.querySelector(".add-task-btn");

    document.querySelectorAll(".kanban-column.is-drag-over").forEach((item) => {
        if (item !== columnEl) {
            item.classList.remove("is-drag-over");
        }
    });

    columnEl.classList.add("is-drag-over");

    if (afterElement) {
        columnEl.insertBefore(placeholder, afterElement);
        return;
    }

    if (addTaskButton) {
        columnEl.insertBefore(placeholder, addTaskButton);
        return;
    }

    columnEl.appendChild(placeholder);
}

function getTaskPlaceholder() {
    if (!dragPlaceholder) {
        dragPlaceholder = document.createElement("div");
        dragPlaceholder.className = "task-drop-placeholder";
        dragPlaceholder.setAttribute("aria-hidden", "true");
    }

    return dragPlaceholder;
}

function getTaskDragAfterElement(columnEl, pointerY) {
    const cards = [...columnEl.querySelectorAll(".task-card:not(.is-dragging)")];

    return cards.reduce(
        (closest, card) => {
            const box = card.getBoundingClientRect();
            const offset = pointerY - box.top - box.height / 2;

            if (offset < 0 && offset > closest.offset) {
                return { offset, element: card };
            }

            return closest;
        },
        { offset: Number.NEGATIVE_INFINITY, element: null }
    ).element;
}

function getTaskDropIndex(columnEl) {
    let index = 0;

    for (const child of columnEl.children) {
        if (child === dragPlaceholder) {
            return index;
        }

        if (child.classList?.contains("task-card") && !child.classList.contains("is-dragging")) {
            index += 1;
        }
    }

    return index;
}

async function moveTaskToColumn(taskId, toColumnId, dropIndex) {
    const location = getTaskLocation(taskId);
    const toColumn = state.board?.columns?.find((column) => column.id === toColumnId);

    if (!location || !toColumn || dropIndex === null) {
        return;
    }

    const { task, column: fromColumn, index: fromIndex } = location;
    const targetTasks = toColumn.tasks.filter((item) => item.id !== taskId);
    const normalizedDropIndex = Math.max(0, Math.min(dropIndex, targetTasks.length));

    if (fromColumn.id === toColumn.id && normalizedDropIndex === fromIndex) {
        return;
    }

    const previousColumns = createBoardSnapshot();
    const nextOrder = getTaskOrderForDrop(targetTasks, normalizedDropIndex);

    movingTaskIds.add(taskId);

    fromColumn.tasks = fromColumn.tasks.filter((item) => item.id !== taskId);
    task.columnId = toColumn.id;
    task.order = nextOrder;

    const nextTasks = toColumn.tasks.filter((item) => item.id !== taskId);
    nextTasks.splice(normalizedDropIndex, 0, task);
    toColumn.tasks = nextTasks;

    renderSidebar();
    renderKanban(state.board);

    try {
        await moveTask(task.id, toColumn.id, nextOrder);
    } catch (error) {
        restoreBoardSnapshot(previousColumns);
        handleActionError(error, "Не удалось переместить задачу");
    } finally {
        movingTaskIds.delete(taskId);
        renderSidebar();
        renderKanban(state.board);
    }
}

function getTaskLocation(taskId) {
    for (const column of state.board?.columns ?? []) {
        const index = column.tasks.findIndex((task) => task.id === taskId);

        if (index !== -1) {
            return {
                column,
                index,
                task: column.tasks[index],
            };
        }
    }

    return null;
}

function getTaskOrderForDrop(tasks, index) {
    const previousOrder = tasks[index - 1]?.order ?? null;
    const nextOrder = tasks[index]?.order ?? null;

    if (previousOrder === null && nextOrder === null) {
        return TASK_ORDER_STEP;
    }

    if (previousOrder === null) {
        return nextOrder > 1 ? Math.floor(nextOrder / 2) : nextOrder - TASK_ORDER_STEP;
    }

    if (nextOrder === null) {
        return previousOrder + TASK_ORDER_STEP;
    }

    if (nextOrder - previousOrder > 1) {
        return Math.floor((previousOrder + nextOrder) / 2);
    }

    return previousOrder + 1;
}

function createBoardSnapshot() {
    return (state.board?.columns ?? []).map((column) => ({
        column,
        tasks: column.tasks.map((task) => ({
            task,
            columnId: task.columnId,
            order: task.order,
        })),
    }));
}

function restoreBoardSnapshot(snapshot) {
    snapshot.forEach(({ column, tasks }) => {
        column.tasks = tasks.map(({ task, columnId, order }) => {
            task.columnId = columnId;
            task.order = order;
            return task;
        });
    });
}

function cleanupTaskDrag() {
    draggedTaskId = null;

    dragPlaceholder?.remove();
    dragPreview?.remove();
    dragPreview = null;
    window.setTimeout(() => {
        suppressTaskClick = false;
    }, 0);

    document.querySelectorAll(".task-card.is-dragging").forEach((card) => {
        card.classList.remove("is-dragging");
    });

    document.querySelectorAll(".kanban-column.is-drag-over").forEach((column) => {
        column.classList.remove("is-drag-over");
    });
}

function clearTaskDropTarget() {
    dragPlaceholder?.remove();

    document.querySelectorAll(".kanban-column.is-drag-over").forEach((column) => {
        column.classList.remove("is-drag-over");
    });
}

function openTaskDetailOverlay(task) {
    selectedTaskForDetail = task;
    detailSubtasks = getTaskSubtasks(task);

    detailTaskTitle.value = task.title || "";
    detailTaskDescription.value = task.description || "";
    detailTaskAssignee.textContent = getTaskAssigneeName(task);
    detailTaskPriority.value = normalizePriorityForSelect(task.priority);
    detailTaskComplexity.value = task.complexity || "";
    syncKanbanSelect(detailTaskPriority);
    syncKanbanSelect(detailTaskComplexity);
    detailTaskDeadline.value = toDateTimeLocalValueSafe(task.deadline);

    renderDetailSubtasks();

    taskDetailOverlay.classList.add("is-open");
    taskDetailOverlay.setAttribute("aria-hidden", "false");

    setTimeout(() => {
        detailTaskTitle.focus();
    }, 0);
}
function syncKanbanSelect(input) {
    if (!input) {
        return;
    }

    const dropdown = document.querySelector(`[data-kanban-select="${input.id}"]`);

    if (!dropdown) {
        return;
    }

    const label = dropdown.querySelector("[data-kanban-select-label]");
    const options = dropdown.querySelectorAll(".kanban-select-option");

    let selectedOption = null;

    options.forEach((option) => {
        const isSelected = option.dataset.value === input.value;
        option.classList.toggle("is-selected", isSelected);

        if (isSelected) {
            selectedOption = option;
        }
    });

    if (label && selectedOption) {
        label.textContent = selectedOption.textContent.trim();
    }
}
function closeTaskDetailOverlay() {
    taskDetailOverlay.classList.remove("is-open");
    taskDetailOverlay.setAttribute("aria-hidden", "true");
    selectedTaskForDetail = null;
    detailSubtasks = [];
}

async function saveTaskDetailFromForm() {
    if (!selectedTaskForDetail) {
        return;
    }

    const name = detailTaskTitle.value.trim();
    const description = detailTaskDescription.value.trim();
    const userId = selectedTaskForDetail.userId || getCurrentUserId();

    if (!name) {
        detailTaskTitle.focus();
        return;
    }

    if (!userId) {
        showToast("Нужна авторизация или назначенный исполнитель");
        return;
    }

    try {
        await updateTask(selectedTaskForDetail.id, {
            name,
            description,
            priority: detailTaskPriority.value || "Medium",

            deadline: detailTaskDeadline.value
                ? new Date(detailTaskDeadline.value).toISOString()
                : selectedTaskForDetail.deadline,

            userId,
        });

        rememberTaskSubtasks(selectedTaskForDetail.id, detailSubtasks);
        closeTaskDetailOverlay();

        await reloadBoard(`Задача «${name}» обновлена`);
    } catch (error) {
        handleActionError(error, "Не удалось обновить задачу");
    }
}

function getTaskSubtasks(task) {
    const subtasks =
        task.subtasks ||
        task.subTasks ||
        task.Subtasks ||
        [];

    if (!Array.isArray(subtasks)) {
        return [];
    }

    return normalizeSubtasks(subtasks);
}

function normalizeSubtasks(subtasks) {
    if (!Array.isArray(subtasks)) {
        return [];
    }

    return subtasks
        .map((subtask, index) => normalizeSubtask(subtask, index))
        .filter((subtask) => subtask.title);
}

function normalizeSubtask(subtask, index = 0) {
    if (typeof subtask === "string") {
        return {
            id: `subtask-${index}`,
            title: subtask.trim(),
            done: false,
            assignee: null,
        };
    }

    const title = String(subtask?.title ?? subtask?.name ?? "Подзадача").trim();

    return {
        id: subtask?.id ?? subtask?.Id ?? `subtask-${index}`,
        title,
        done: Boolean(subtask?.done ?? subtask?.isDone ?? subtask?.completed),
        assignee: getSubtaskAssignee(subtask),
    };
}

function getSubtaskAssignee(subtask) {
    const assignee =
        subtask?.assignee ??
        subtask?.Assignee ??
        subtask?.user ??
        subtask?.User ??
        null;

    if (assignee) {
        return normalizeAssignee(assignee);
    }

    const assigneeId =
        subtask?.assigneeId ??
        subtask?.AssigneeId ??
        subtask?.userId ??
        subtask?.UserId ??
        null;

    return assigneeId ? resolveAssigneeById(assigneeId) : null;
}

function renderDetailSubtasks() {
    detailSubtaskList.innerHTML = "";

    if (!detailSubtasks.length) {
        detailSubtaskList.innerHTML = `<div class="subtask-empty">Подзадач пока нет</div>`;
        return;
    }

    detailSubtasks.forEach((subtask) => {
        const row = document.createElement("div");
        const assigneeName = subtask.assignee?.name ?? "Не назначен";

        row.className = `subtask-item ${subtask.done ? "is-done" : ""}`;

        row.innerHTML = `
            <input
                class="subtask-checkbox"
                type="checkbox"
                ${subtask.done ? "checked" : ""}
                aria-label="Отметить подзадачу"
            >
            <span class="subtask-title">${escapeHtml(subtask.title)}</span>
            <span class="subtask-assignee">${escapeHtml(assigneeName)}</span>
            <button class="subtask-remove-btn" type="button" aria-label="Удалить подзадачу">×</button>
        `;

        row.querySelector(".subtask-checkbox").addEventListener("change", (event) => {
            subtask.done = event.target.checked;
            renderDetailSubtasks();
        });

        row.querySelector(".subtask-remove-btn").addEventListener("click", () => {
            detailSubtasks = detailSubtasks.filter((item) => item.id !== subtask.id);
            renderDetailSubtasks();
        });

        detailSubtaskList.appendChild(row);
    });
}

function addDetailSubtask() {
    const title = detailSubtaskInput.value.trim();

    if (!title) {
        detailSubtaskInput.focus();
        return;
    }

    detailSubtasks.push({
        id: window.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`,
        title,
        done: false,
        assignee: getTaskUsers(selectedTaskForDetail?.users)[0] ?? null,
    });

    detailSubtaskInput.value = "";
    renderDetailSubtasks();
}

function getTaskAssigneeName(task) {
    const user = getTaskUsers(task.users)[0];

    if (user?.name) {
        return user.name;
    }

    return task.userId
        ? `#${String(task.userId).slice(0, 8)}`
        : "Не назначен";
}

function normalizePriorityForSelect(priority) {
    const value = String(priority || "").toLowerCase();

    if (value === "low") {
        return "Low";
    }

    if (value === "high") {
        return "High";
    }

    return "Medium";
}

function toDateTimeLocalValueSafe(value) {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return getDefaultDateTimeLocal();
    }

    return toDateTimeLocalValue(date);
}

async function deleteTask(task) {
    const confirmed = confirm(`Удалить задачу «${task.title}»?`);

    if (!confirmed) {
        return;
    }

    try {
        await deleteTaskRequest(task.id);
        forgetTaskAssignees(task.id);
        forgetTaskSubtasks(task.id);
        await reloadBoard(`Задача «${task.title}» удалена`);
    } catch (error) {
        handleActionError(error, "Не удалось удалить задачу");
    }
}

/* =========================
   CREATE TASK MODAL
========================= */

function openCreateTaskModal(column) {
    selectedColumnForNewTask = column;
    selectedAssignees = [];
    selectedTags = [];
    createSubtasks = [];

    createTaskForm.reset();

    newTaskDeadline.value = getDefaultDateTimeLocal();
    newTaskAssigneeSearch.value = "";

    renderCreatedTags();
    renderSelectedAssignees();
    renderCreateSubtasks();
    renderCreateSubtaskAssigneeOptions();
    renderAssigneeOptions("");

    createTaskOverlay.classList.add("is-open");

    setTimeout(() => {
        newTaskTitle.focus();
    }, 0);
}

function closeCreateTaskModal() {
    createTaskOverlay.classList.remove("is-open");
    selectedColumnForNewTask = null;
    selectedAssignees = [];
    selectedTags = [];
    createSubtasks = [];
    closeSubtaskAssigneeDropdowns();
}

async function createTaskFromForm() {
    const title = newTaskTitle.value.trim();
    const description = newTaskDescription.value.trim();
    const priority = newTaskPriority.value;
    const complexity = newTaskComplexity.value;
    const deadline = newTaskDeadline.value;

    if (!title) {
        newTaskTitle.focus();
        return;
    }

    if (!selectedColumnForNewTask?.id) {
        showToast("Колонка не выбрана");
        return;
    }

    if (!state.board?.id) {
        showToast("Канбан не выбран");
        return;
    }

    const subtasks = normalizeSubtasks(createSubtasks);
    const hasUnassignedSubtasks = subtasks.some((subtask) => !subtask.assignee?.id);

    if (hasUnassignedSubtasks) {
        showToast("Назначь исполнителя каждой подзадаче");
        return;
    }

    const userId = selectedAssignees[0]?.id ?? getCurrentUserId();

    if (!userId) {
        showToast("Нужна авторизация или выбранный исполнитель");
        return;
    }

    const finalDescription = buildDescriptionWithTags(description, selectedTags);

    try {
        const createdTask = await createTask(state.board.id, {
            name: title,
            description: finalDescription || "Описание не добавлено",
            priority: mapPriorityToApi(priority),
            deadline: deadline ? new Date(deadline).toISOString() : getDefaultDeadline(),
            userId,
            columnId: selectedColumnForNewTask.id,
            order: null,
            tagIds: [],
        });

        const createdTaskId = readTaskId(createdTask);

        rememberTaskAssignees(
            createdTaskId,
            selectedAssignees.length
                ? selectedAssignees
                : [resolveAssigneeById(userId)]
        );
        rememberTaskSubtasks(createdTaskId, subtasks);

        closeCreateTaskModal();
        await reloadBoard(`Задача «${title}» создана`);
    } catch (error) {
        handleActionError(error, "Не удалось создать задачу");
    }
}

function renderCreateSubtasks() {
    if (!createSubtaskList) {
        return;
    }

    createSubtaskList.innerHTML = "";

    if (!createSubtasks.length) {
        createSubtaskList.innerHTML = `<div class="create-subtask-empty">Подзадач пока нет</div>`;
        return;
    }

    createSubtasks.forEach((subtask) => {
        const row = document.createElement("div");
        const assigneeId = subtask.assignee?.id ? String(subtask.assignee.id) : "";
        const assigneeName = subtask.assignee?.name ?? "Выбери исполнителя";

        row.className = "create-subtask-item";
        row.innerHTML = `
            <span class="create-subtask-check" aria-hidden="true">✓</span>
            <span class="create-subtask-title">${escapeHtml(subtask.title)}</span>
            <div class="subtask-assignee-dropdown create-subtask-item-assignee" data-subtask-row-assignee-dropdown>
                <button
                    class="subtask-assignee-toggle"
                    type="button"
                    data-subtask-row-assignee-toggle
                    aria-expanded="false"
                    aria-label="Исполнитель подзадачи"
                >
                    <span>${escapeHtml(assigneeName)}</span>
                    <span class="subtask-assignee-chevron" aria-hidden="true"></span>
                </button>
                <div class="subtask-assignee-menu" role="menu">
                    ${getCreateSubtaskAssigneeMenuHtml(assigneeId)}
                </div>
            </div>
            <button class="create-subtask-remove-btn" type="button" aria-label="Удалить подзадачу">×</button>
        `;

        const assigneeDropdown = row.querySelector("[data-subtask-row-assignee-dropdown]");
        const assigneeToggle = row.querySelector("[data-subtask-row-assignee-toggle]");

        assigneeDropdown.classList.toggle("is-disabled", selectedAssignees.length === 0);
        assigneeToggle.disabled = selectedAssignees.length === 0;
        assigneeToggle.addEventListener("click", () => {
            toggleSubtaskAssigneeDropdown(assigneeDropdown);
        });

        row.querySelectorAll("[data-subtask-assignee-option]").forEach((button) => {
            button.addEventListener("click", () => {
                subtask.assignee = getSelectedAssigneeById(button.dataset.subtaskAssigneeOption);
                closeSubtaskAssigneeDropdowns();
                renderCreateSubtasks();
            });
        });

        row.querySelector(".create-subtask-remove-btn").addEventListener("click", () => {
            createSubtasks = createSubtasks.filter((item) => item.id !== subtask.id);
            renderCreateSubtasks();
        });

        createSubtaskList.appendChild(row);
    });
}

function renderCreateSubtaskAssigneeOptions() {
    if (!createSubtaskAssignee) {
        return;
    }

    const previousValue = createSubtaskAssignee.dataset.assigneeId ?? "";
    const toggle = createSubtaskAssignee.querySelector("[data-subtask-assignee-toggle]");
    const label = createSubtaskAssignee.querySelector("[data-subtask-assignee-label]");
    const menu = createSubtaskAssignee.querySelector("[data-subtask-assignee-menu]");

    if (!toggle || !label || !menu) {
        return;
    }

    if (!selectedAssignees.length) {
        createSubtaskAssignee.dataset.assigneeId = "";
        createSubtaskAssignee.classList.add("is-disabled");
        toggle.disabled = true;
        label.textContent = "Сначала выбери исполнителя";
        menu.innerHTML = "";
        return;
    }

    const nextAssignee =
        getSelectedAssigneeById(previousValue) ??
        normalizeAssignee(selectedAssignees[0]);

    createSubtaskAssignee.dataset.assigneeId = String(nextAssignee.id);
    createSubtaskAssignee.classList.remove("is-disabled");
    toggle.disabled = false;
    toggle.setAttribute("aria-expanded", "false");
    label.textContent = nextAssignee.name;
    menu.innerHTML = getCreateSubtaskAssigneeMenuHtml(nextAssignee.id);

    menu.querySelectorAll("[data-subtask-assignee-option]").forEach((button) => {
        button.addEventListener("click", () => {
            const assignee = getSelectedAssigneeById(button.dataset.subtaskAssigneeOption);

            if (!assignee) {
                return;
            }

            createSubtaskAssignee.dataset.assigneeId = String(assignee.id);
            label.textContent = assignee.name;
            renderCreateSubtaskAssigneeOptions();
            closeSubtaskAssigneeDropdowns();
        });
    });
}

function getCreateSubtaskAssigneeMenuHtml(selectedId = "") {
    if (!selectedAssignees.length) {
        return "";
    }

    return selectedAssignees.map((assignee) => {
        const value = String(assignee.id);
        const isSelected = String(selectedId) === value;

        return `
            <button
                class="subtask-assignee-option ${isSelected ? "is-selected" : ""}"
                type="button"
                data-subtask-assignee-option="${escapeAttr(value)}"
                role="menuitemradio"
                aria-checked="${isSelected}"
            >
                ${escapeHtml(assignee.name)}
            </button>
        `;
    }).join("");
}

function toggleSubtaskAssigneeDropdown(dropdown) {
    if (!dropdown || dropdown.classList.contains("is-disabled")) {
        return;
    }

    const shouldOpen = !dropdown.classList.contains("is-open");

    closeSubtaskAssigneeDropdowns(dropdown);
    dropdown.classList.toggle("is-open", shouldOpen);
    dropdown.querySelector(".subtask-assignee-toggle")?.setAttribute("aria-expanded", String(shouldOpen));
}

function closeSubtaskAssigneeDropdowns(exceptDropdown = null) {
    document.querySelectorAll(".subtask-assignee-dropdown.is-open").forEach((dropdown) => {
        if (dropdown === exceptDropdown) {
            return;
        }

        dropdown.classList.remove("is-open");
        dropdown.querySelector(".subtask-assignee-toggle")?.setAttribute("aria-expanded", "false");
    });
}

function addCreateSubtask() {
    const title = createSubtaskInput.value.trim();

    if (!title) {
        createSubtaskInput.focus();
        return;
    }

    if (!selectedAssignees.length) {
        showToast("Сначала выбери исполнителя задачи");
        newTaskAssigneeSearch?.focus();
        return;
    }

    const assignee = getSelectedAssigneeById(createSubtaskAssignee.dataset.assigneeId);

    if (!assignee) {
        showToast("Выбери исполнителя подзадачи");
        createSubtaskAssignee.querySelector("[data-subtask-assignee-toggle]")?.focus();
        return;
    }

    createSubtasks.push({
        id: window.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random()}`,
        title,
        done: false,
        assignee,
    });

    createSubtaskInput.value = "";
    renderCreateSubtasks();
    renderCreateSubtaskAssigneeOptions();
    createSubtaskInput.focus();
}

function getSelectedAssigneeById(assigneeId) {
    const assignee = selectedAssignees.find((item) =>
        item.id && String(item.id) === String(assigneeId)
    );

    return assignee ? normalizeAssignee(assignee) : null;
}

function syncCreateSubtasksWithSelectedAssignees() {
    const selectedIds = new Set(selectedAssignees.map((assignee) => String(assignee.id)));

    createSubtasks = createSubtasks.map((subtask) => {
        if (!subtask.assignee?.id || !selectedIds.has(String(subtask.assignee.id))) {
            return {
                ...subtask,
                assignee: null,
            };
        }

        return subtask;
    });
}

function getDefaultDateTimeLocal() {
    const date = new Date();
    date.setHours(date.getHours() + 2);
    date.setMinutes(0, 0, 0);

    return toDateTimeLocalValue(date);
}

function toDateTimeLocalValue(date) {
    const offset = date.getTimezoneOffset();
    const localDate = new Date(date.getTime() - offset * 60 * 1000);

    return localDate.toISOString().slice(0, 16);
}

function mapPriorityToApi(priority) {
    const map = {
        LOW: "Low",
        MID: "Medium",
        HIGH: "High",
    };

    return map[priority] ?? "Medium";
}

function buildDescriptionWithTags(description, tags) {
    const cleanDescription = description.trim();

    if (!tags.length) {
        return cleanDescription;
    }

    const tagLine = tags.map((tag) => `#${tag}`).join(" ");

    return cleanDescription
        ? `${cleanDescription}\n\nТеги: ${tagLine}`
        : `Теги: ${tagLine}`;
}

/* =========================
   ASSIGNEE PICKER
========================= */

function getProjectAssignees() {
    const assignees = [];
    const projectTeam = state.activeProject?.team;
    const members = projectTeam?.members ?? projectTeam?.Members ?? [];

    members.forEach((member) => {
        const memberId =
            member.userId ??
            member.UserId ??
            member.memberId ??
            member.MemberId ??
            member.id ??
            member.Id ??
            null;

        if (!memberId) {
            return;
        }

        assignees.push({
            id: memberId,
            name:
                member.name ||
                member.Name ||
                member.userName ||
                member.UserName ||
                member.email ||
                member.Email ||
                formatAssigneeFallbackName(memberId),
            color: projectTeam?.color || "#42609f",
        });
    });

    const currentUserId = getCurrentUserId();
    const hasCurrentUser = assignees.some((assignee) =>
        assignee.id && String(assignee.id) === String(currentUserId)
    );

    if (state.currentUser && !hasCurrentUser) {
        assignees.push({
            id: currentUserId,
            name:
                state.currentUser.name ||
                state.currentUser.userName ||
                state.currentUser.email ||
                formatAssigneeFallbackName(currentUserId),
            color: "#f4864d",
        });
    }

    return removeDuplicateAssignees(assignees);
}

function removeDuplicateAssignees(assignees) {
    const seen = new Set();

    return assignees.filter((item) => {
        const key = item.id ? String(item.id) : `none-${item.name}`;

        if (seen.has(key)) {
            return false;
        }

        seen.add(key);
        return true;
    });
}

function renderAssigneeOptions(searchValue) {
    const query = searchValue.trim().toLowerCase();

    assigneeOptions.innerHTML = "";

    if (!query) {
        return;
    }

    const assignees = getProjectAssignees().filter((item) => {
        const isMatch = item.name.toLowerCase().includes(query);
        const isAlreadySelected = selectedAssignees.some((assignee) =>
            getAssigneeKey(assignee) === getAssigneeKey(item)
        );

        return isMatch && !isAlreadySelected;
    });

    if (assignees.length === 0) {
        const empty = document.createElement("div");
        empty.className = "assignee-empty";
        empty.textContent = "Участники не найдены";
        assigneeOptions.appendChild(empty);
        return;
    }

    assignees.forEach((assignee) => {
        const button = document.createElement("button");

        button.className = "assignee-option";

        button.type = "button";

        button.innerHTML = `
            <span>${escapeHtml(assignee.name)}</span>
            <span class="assignee-dot" style="background:${assignee.color}"></span>
        `;

        button.addEventListener("click", () => {
            addSelectedAssignee(assignee);
            newTaskAssigneeSearch.value = "";
            assigneeOptions.innerHTML = "";
        });

        assigneeOptions.appendChild(button);
    });
}

function getAssigneeKey(assignee) {
    return `${assignee.id ?? "none"}-${assignee.name}`;
}

function addSelectedAssignee(assignee) {
    if (!assignee.id) {
        return;
    }

    const alreadySelected = selectedAssignees.some((item) =>
        getAssigneeKey(item) === getAssigneeKey(assignee)
    );

    if (alreadySelected) {
        return;
    }

    if (selectedAssignees.length >= 4) {
        showToast("Можно выбрать не больше 4 исполнителей");
        return;
    }

    // selectedAssignees.push(assignee);
    selectedAssignees.push(normalizeAssignee(assignee));
    renderSelectedAssignees();
    renderCreateSubtaskAssigneeOptions();
    renderCreateSubtasks();
}

function removeSelectedAssignee(assigneeKey) {
    selectedAssignees = selectedAssignees.filter((assignee) =>
        getAssigneeKey(assignee) !== assigneeKey
    );
    syncCreateSubtasksWithSelectedAssignees();
    renderSelectedAssignees();
    renderCreateSubtaskAssigneeOptions();
    renderCreateSubtasks();
}

function renderSelectedAssignees() {
    if (!selectedAssigneesContainer) {
        return;
    }

    selectedAssigneesContainer.innerHTML = "";

    if (selectedAssignees.length === 0) {
        selectedAssigneesContainer.innerHTML = `
            <span class="selected-assignees-empty">Можно выбрать до 4 исполнителей</span>
        `;
        return;
    }

    selectedAssignees.forEach((assignee) => {
        const chip = document.createElement("div");
        const key = getAssigneeKey(assignee);

        chip.className = "selected-assignee";
        chip.innerHTML = `
            <span>${escapeHtml(assignee.name)}</span>
            <button type="button" aria-label="Убрать исполнителя">×</button>
        `;

        chip.querySelector("button").addEventListener("click", () => {
            removeSelectedAssignee(key);
        });

        selectedAssigneesContainer.appendChild(chip);
    });
}

function hydrateBoardTaskAssignees(board) {
    if (!board?.columns) {
        return board;
    }

    board.columns.forEach((column) => {
        column.tasks = (column.tasks ?? []).map((task) => ({
            ...task,
            users: getStoredTaskAssignees(task.id) ?? getFallbackTaskAssignees(task),
            subtasks: getStoredTaskSubtasks(task.id) ?? getTaskSubtasks(task),
        }));
    });

    return board;
}

function getFallbackTaskAssignees(task) {
    if (!task?.userId) {
        return task?.users ?? [];
    }

    return [resolveAssigneeById(task.userId)];
}

function resolveAssigneeById(userId) {
    const assignee = getProjectAssignees().find((item) =>
        item.id && String(item.id) === String(userId)
    );

    if (assignee) {
        return normalizeAssignee(assignee);
    }

    return {
        id: userId,
        name: formatAssigneeFallbackName(userId),
        color: "#838383",
    };
}

function formatAssigneeFallbackName(userId) {
    const shortId = userId ? String(userId).slice(0, 8) : "";
    return shortId ? `Участник ${shortId}` : "Исполнитель";
}

function readTaskId(task) {
    return task?.id ?? task?.Id ?? null;
}

function rememberTaskAssignees(taskId, assignees) {
    if (!taskId) {
        return;
    }

    const cleanAssignees = assignees
        .filter((assignee) => assignee?.id)
        .slice(0, 4)
        .map(normalizeAssignee);

    if (cleanAssignees.length === 0) {
        return;
    }

    const store = readTaskAssigneeStore();
    store[taskId] = cleanAssignees;
    writeTaskAssigneeStore(store);
}

function forgetTaskAssignees(taskId) {
    if (!taskId) {
        return;
    }

    const store = readTaskAssigneeStore();
    delete store[taskId];
    writeTaskAssigneeStore(store);
}

function rememberTaskSubtasks(taskId, subtasks) {
    if (!taskId) {
        return;
    }

    const cleanSubtasks = normalizeSubtasks(subtasks).map((subtask) => ({
        ...subtask,
        assignee: subtask.assignee ? normalizeAssignee(subtask.assignee) : null,
    }));
    const store = readTaskSubtaskStore();

    if (cleanSubtasks.length === 0) {
        delete store[taskId];
    } else {
        store[taskId] = cleanSubtasks;
    }

    writeTaskSubtaskStore(store);
}

function forgetTaskSubtasks(taskId) {
    if (!taskId) {
        return;
    }

    const store = readTaskSubtaskStore();
    delete store[taskId];
    writeTaskSubtaskStore(store);
}

function getStoredTaskSubtasks(taskId) {
    if (!taskId) {
        return null;
    }

    const subtasks = readTaskSubtaskStore()[taskId];

    if (!Array.isArray(subtasks)) {
        return null;
    }

    return normalizeSubtasks(subtasks);
}

function getStoredTaskAssignees(taskId) {
    if (!taskId) {
        return null;
    }

    const assignees = readTaskAssigneeStore()[taskId];

    if (!Array.isArray(assignees) || assignees.length === 0) {
        return null;
    }

    return assignees.slice(0, 4).map(normalizeAssignee);
}

function normalizeAssignee(assignee = {}) {
    assignee = assignee ?? {};

    const id =
        assignee.id ??
        assignee.userId ??
        assignee.UserId ??
        null;
    const rawName =
        assignee.name ??
        assignee.userName ??
        assignee.email ??
        "";
    const name = String(rawName).trim();

    return {
        id,
        name: name && !name.startsWith("#") ? name : formatAssigneeFallbackName(id),
        color: assignee.color || "#838383",
    };
}

function readTaskAssigneeStore() {
    try {
        return JSON.parse(localStorage.getItem(TASK_ASSIGNEES_STORAGE_KEY) || "{}");
    } catch {
        return {};
    }
}

function writeTaskAssigneeStore(store) {
    localStorage.setItem(TASK_ASSIGNEES_STORAGE_KEY, JSON.stringify(store));
}

function readTaskSubtaskStore() {
    try {
        return JSON.parse(localStorage.getItem(TASK_SUBTASKS_STORAGE_KEY) || "{}");
    } catch {
        return {};
    }
}

function writeTaskSubtaskStore(store) {
    localStorage.setItem(TASK_SUBTASKS_STORAGE_KEY, JSON.stringify(store));
}

/* =========================
   CUSTOM TAGS
========================= */

function addCustomTag() {
    const rawTag = newTaskTagInput.value.trim();

    if (!rawTag) {
        return;
    }

    const normalizedTag = rawTag.replace(/^#/, "");

    if (!normalizedTag) {
        newTaskTagInput.value = "";
        return;
    }

    const alreadyExists = selectedTags.some(
        (tag) => tag.toLowerCase() === normalizedTag.toLowerCase()
    );

    if (!alreadyExists) {
        selectedTags.push(normalizedTag);
    }

    newTaskTagInput.value = "";
    renderCreatedTags();
}

function removeCustomTag(tagToRemove) {
    selectedTags = selectedTags.filter((tag) => tag !== tagToRemove);
    renderCreatedTags();
}

function renderCreatedTags() {
    createdTags.innerHTML = "";

    if (selectedTags.length === 0) {
        createdTags.innerHTML = `<span class="empty-tags">Тегов пока нет</span>`;
        return;
    }

    selectedTags.forEach((tag) => {
        const button = document.createElement("button");
        button.className = "created-tag";
        button.type = "button";

        button.innerHTML = `
            <span>#${escapeHtml(tag)}</span>
            <span>×</span>
        `;

        button.addEventListener("click", () => {
            removeCustomTag(tag);
        });

        createdTags.appendChild(button);
    });
}

/* =========================
   NOTIFICATIONS
========================= */

async function openNotificationsOverlay() {
    if (!notificationOverlay) {
        return;
    }

    notificationOverlay.classList.add("is-open");
    notificationOverlay.setAttribute("aria-hidden", "false");

    setTimeout(() => {
        notificationPanel?.focus();
    }, 0);

    await loadNotifications();
}

function closeNotificationsOverlay() {
    if (!notificationOverlay || !isNotificationsOpen()) {
        return;
    }

    notificationOverlay.classList.remove("is-open");
    notificationOverlay.setAttribute("aria-hidden", "true");
    notificationOpen?.focus();
}

function isNotificationsOpen() {
    return notificationOverlay?.classList.contains("is-open") ?? false;
}

async function loadNotifications() {
    if (!notificationList) {
        return;
    }

    renderNotificationsLoading();

    try {
        const notifications = state.notificationsUnreadOnly
            ? await getUnreadNotifications()
            : await getNotifications();

        state.notifications = normalizeNotifications(notifications);
        syncUnreadCountFromLoadedNotifications();
        updateNotificationButton();
        renderNotifications();
    } catch (error) {
        console.error(error);
        renderNotificationsError("Не удалось загрузить уведомления");
    }
}

async function refreshUnreadNotificationsCount() {
    const notifications = normalizeNotifications(await getUnreadNotifications());

    state.notificationUnreadCount = notifications.filter((notification) => !notification.isRead).length;
    updateNotificationButton();

    if (isNotificationsOpen() && state.notificationsUnreadOnly) {
        state.notifications = notifications;
        renderNotifications();
    }
}

function renderNotificationsLoading() {
    if (!notificationList) {
        return;
    }

    if (notificationReadAll) {
        notificationReadAll.disabled = true;
    }
    notificationList.innerHTML = `<div class="notification-empty">Загрузка уведомлений...</div>`;
}

function renderNotificationsError(message) {
    if (!notificationList) {
        return;
    }

    if (notificationReadAll) {
        notificationReadAll.disabled = state.notificationUnreadCount === 0;
    }
    notificationList.innerHTML = `
        <div class="notification-error">
            ${escapeHtml(message)}
        </div>
    `;
}

function renderNotifications() {
    if (!notificationList) {
        return;
    }

    if (notificationReadAll) {
        notificationReadAll.disabled = state.notificationUnreadCount === 0;
    }
    notificationList.innerHTML = "";

    if (state.notifications.length === 0) {
        const emptyText = state.notificationsUnreadOnly
            ? "Непрочитанных уведомлений нет"
            : "Уведомлений пока нет";

        notificationList.innerHTML = `
            <div class="notification-empty">
                ${escapeHtml(emptyText)}
            </div>
        `;
        return;
    }

    state.notifications.forEach((notification) => {
        const item = document.createElement("button");
        item.className = `notification-item ${notification.isRead ? "is-read" : "is-unread"}`;
        item.type = "button";
        item.dataset.notificationId = notification.id;
        item.setAttribute(
            "aria-label",
            notification.isRead
                ? `Уведомление: ${notification.name}`
                : `Непрочитанное уведомление: ${notification.name}`
        );

        item.innerHTML = `
            <span class="notification-check" aria-hidden="true"></span>
            <span class="notification-content">
                <span class="notification-title-row">
                    <strong class="notification-name">${escapeHtml(notification.name)}</strong>
                    <time class="notification-time" datetime="${escapeAttr(notification.createdAt)}">
                        ${escapeHtml(formatNotificationTime(notification.createdAt))}
                    </time>
                </span>
                <span class="notification-message">${escapeHtml(notification.message)}</span>
            </span>
        `;

        item.addEventListener("click", () => {
            markNotificationAsRead(notification.id);
        });

        notificationList.appendChild(item);
    });
}

async function markNotificationAsRead(notificationId) {
    const notification = state.notifications.find((item) => item.id === notificationId);

    if (!notification || notification.isRead) {
        return;
    }

    try {
        const readResponse = await readNotification(notificationId);
        const updatedNotification = readResponse
            ? normalizeNotification(readResponse)
            : notification;
        const nextNotification = {
            ...notification,
            ...updatedNotification,
            id: notification.id,
            isRead: true,
        };

        state.notifications = state.notifications
            .map((item) => (item.id === notificationId ? nextNotification : item))
            .filter((item) => !state.notificationsUnreadOnly || !item.isRead);

        state.notificationUnreadCount = Math.max(0, state.notificationUnreadCount - 1);
        updateNotificationButton();
        renderNotifications();
    } catch (error) {
        handleActionError(error, "Не удалось прочитать уведомление");
    }
}

async function markAllNotificationsAsRead() {
    if (state.notificationUnreadCount === 0) {
        return;
    }

    try {
        await readAllNotifications();

        state.notificationUnreadCount = 0;
        state.notifications = state.notifications
            .map((notification) => ({
                ...notification,
                isRead: true,
            }))
            .filter((notification) => !state.notificationsUnreadOnly || !notification.isRead);

        updateNotificationButton();
        renderNotifications();
        showToast("Все уведомления прочитаны");
    } catch (error) {
        handleActionError(error, "Не удалось прочитать уведомления");
    }
}

function handleRealtimeNotification(notification) {
    const normalizedNotification = normalizeNotification(notification);

    syncRealtimeStateForNotification(normalizedNotification);
    showToast(normalizedNotification.message || normalizedNotification.name || "Новое уведомление");
}

function syncRealtimeStateForNotification(notification) {
    if (shouldRefreshBoardForNotification(notification)) {
        scheduleRealtimeBoardRefresh();
    }

    if (shouldRefreshWorkspaceForNotification(notification)) {
        scheduleRealtimeWorkspaceRefresh();
    }
}

function shouldRefreshBoardForNotification(notification) {
    return Boolean(notification.taskId) &&
        Boolean(notification.kanbanId) &&
        sameId(notification.kanbanId, state.activeKanban?.id ?? state.board?.id);
}

function shouldRefreshWorkspaceForNotification(notification) {
    return Boolean(notification.teamId) ||
        Boolean(notification.projectId) ||
        notification.type === "TeamMemberAdded" ||
        notification.type === "ProjectCreated" ||
        notification.type === "KanbanCreated" ||
        notification.name === "Team Member Added";
}

function scheduleRealtimeBoardRefresh() {
    window.clearTimeout(realtimeBoardRefreshTimer);
    realtimeBoardRefreshTimer = window.setTimeout(() => {
        refreshActiveBoardFromRealtime().catch((error) => {
            console.warn("Failed to refresh board from realtime notification:", error);
        });
    }, REALTIME_BOARD_REFRESH_DELAY);
}

function scheduleRealtimeWorkspaceRefresh() {
    window.clearTimeout(realtimeWorkspaceRefreshTimer);
    realtimeWorkspaceRefreshTimer = window.setTimeout(() => {
        refreshWorkspaceFromRealtime().catch((error) => {
            console.warn("Failed to refresh workspace from realtime notification:", error);
        });
    }, REALTIME_WORKSPACE_REFRESH_DELAY);
}

async function refreshActiveBoardFromRealtime() {
    if (!state.activeProject || !state.activeKanban?.id) {
        return;
    }

    const scrollLeft = kanbanBoard.scrollLeft;
    const refreshedBoard = hydrateBoardTaskAssignees(
        await loadBoard(state.activeProject, state.activeKanban.id)
    );

    state.board = refreshedBoard;
    state.activeKanban = refreshedBoard.selectedKanban ?? state.activeKanban;

    const refreshedSelectedTask = selectedTaskForDetail
        ? getTaskLocation(selectedTaskForDetail.id)?.task ?? null
        : null;

    if (refreshedSelectedTask && !taskDetailOverlay?.classList.contains("is-open")) {
        selectedTaskForDetail = refreshedSelectedTask;
    }

    renderSidebar();
    renderKanban(state.board);
    kanbanBoard.scrollLeft = scrollLeft;
}

async function refreshWorkspaceFromRealtime() {
    if (workspaceRefreshRequest) {
        return workspaceRefreshRequest;
    }

    workspaceRefreshRequest = (async () => {
        const previousProjectId = state.activeProject?.id ?? null;
        const previousKanbanId = state.activeKanban?.id ?? null;
        const workspace = await loadWorkspace();
        const activeProject =
            workspace.projects.find((project) => sameId(project.id, previousProjectId)) ??
            selectProjectFromUrl(workspace.projects) ??
            workspace.projects[0] ??
            null;

        state.workspace = workspace;
        state.activeProject = activeProject;

        if (activeProject && previousKanbanId) {
            state.activeKanban =
                activeProject.kanbans?.find((kanban) => sameId(kanban.id, previousKanbanId)) ??
                state.activeKanban;
        }

        const activeKanbanStillExists =
            state.activeKanban?.id &&
            activeProject?.kanbans?.some((kanban) => sameId(kanban.id, state.activeKanban.id));

        if (activeProject && (!state.activeKanban?.id || !activeKanbanStillExists)) {
            await loadActiveBoard(activeProject.kanbans?.[0]?.id ?? null);
            return;
        }

        if (state.board) {
            state.board.project = activeProject;
        }

        renderSidebar();
    })().finally(() => {
        workspaceRefreshRequest = null;
    });

    return workspaceRefreshRequest;
}

function handleNotificationFilterChange() {
    state.notificationsUnreadOnly = notificationUnreadOnly?.checked ?? false;

    if (isNotificationsOpen()) {
        loadNotifications();
    }
}

function syncUnreadCountFromLoadedNotifications() {
    const unreadCount = state.notifications.filter((notification) => !notification.isRead).length;

    if (state.notificationsUnreadOnly) {
        state.notificationUnreadCount = unreadCount;
        return;
    }

    state.notificationUnreadCount = unreadCount;
}

function updateNotificationButton() {
    if (!notificationOpen || !notificationBadge) {
        return;
    }

    const count = Math.max(0, Number(state.notificationUnreadCount) || 0);
    const countText = count > 99 ? "99+" : String(count);

    notificationOpen.classList.toggle("has-unread", count > 0);
    notificationOpen.dataset.unreadCount = countText;
    notificationOpen.setAttribute(
        "aria-label",
        count > 0 ? `Уведомления, непрочитанных: ${countText}` : "Уведомления"
    );
    notificationBadge.textContent = countText;
}

function upsertNotification(notifications, notification) {
    const nextNotifications = notifications.filter((item) => item.id !== notification.id);
    return [notification, ...nextNotifications];
}

function normalizeNotifications(notifications) {
    if (!Array.isArray(notifications)) {
        return [];
    }

    return notifications
        .map(normalizeNotification)
        .filter((notification) => notification.id || notification.name || notification.message);
}

function normalizeNotification(notification = {}) {
    const source = notification && typeof notification === "object" ? notification : {};
    const name = readNotificationValue(source, "name", "Name") || "Уведомление";
    const message =
        readNotificationValue(source, "message", "Message") ||
        "Текст уведомления";

    return {
        id: readNotificationValue(source, "id", "Id") || "",
        userId: readNotificationValue(source, "userId", "UserId") || "",
        teamId: readNotificationValue(source, "teamId", "TeamId") || "",
        projectId: readNotificationValue(source, "projectId", "ProjectId") || "",
        taskId: readNotificationValue(source, "taskId", "TaskId") || "",
        kanbanId: readNotificationValue(source, "kanbanId", "KanbanId") || "",
        type: readNotificationValue(source, "type", "Type", "eventType", "EventType") || "",
        name,
        message,
        isRead: Boolean(readNotificationValue(source, "isRead", "IsRead")),
        isPersisted: getNotificationPersistence(source),
        createdAt:
            readNotificationValue(source, "createdAt", "CreatedAt") ||
            new Date().toISOString(),
        readAt: readNotificationValue(source, "readAt", "ReadAt") || null,
    };
}

function readNotificationValue(source, ...keys) {
    for (const key of keys) {
        if (Object.prototype.hasOwnProperty.call(source, key)) {
            return source[key];
        }
    }

    return null;
}

function getNotificationPersistence(source) {
    const explicitValue = readNotificationValue(source, "isPersisted", "IsPersisted");

    if (explicitValue !== null) {
        return explicitValue === true || explicitValue === "true";
    }

    return Boolean(
        readNotificationValue(source, "taskId", "TaskId") &&
        readNotificationValue(source, "kanbanId", "KanbanId")
    );
}

function formatNotificationTime(value) {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return "";
    }

    const diffMs = Date.now() - date.getTime();
    const minuteMs = 60 * 1000;
    const hourMs = 60 * minuteMs;
    const dayMs = 24 * hourMs;

    if (diffMs < minuteMs) {
        return "только что";
    }

    if (diffMs < hourMs) {
        return `${Math.max(1, Math.floor(diffMs / minuteMs))} мин`;
    }

    if (diffMs < dayMs) {
        return `${Math.floor(diffMs / hourMs)} ч`;
    }

    if (diffMs < 7 * dayMs) {
        return `${Math.floor(diffMs / dayMs)} дн`;
    }

    return date.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "short",
    });
}

/* =========================
   REMINDER MODAL
========================= */

function openReminderModal({ userName, taskTitle }) {
    selectedReminder = {
        userName,
        taskTitle,
    };

    reminderText.textContent = `Напомнить ${userName} о задаче «${taskTitle}»?`;
    reminderModal.classList.add("is-open");
}

function closeReminderModal() {
    reminderModal.classList.remove("is-open");
    selectedReminder = null;
}

reminderNo?.addEventListener("click", closeReminderModal);
reminderClose?.addEventListener("click", closeReminderModal);

reminderModal?.addEventListener("click", (event) => {
    if (event.target === reminderModal) {
        closeReminderModal();
    }
});

reminderYes?.addEventListener("click", () => {
    if (!selectedReminder) {
        return;
    }

    showToast(
        `Напоминание для ${selectedReminder.userName}: «${selectedReminder.taskTitle}»`
    );

    closeReminderModal();
});

/* =========================
   EVENT LISTENERS
========================= */

createTaskClose?.addEventListener("click", closeCreateTaskModal);
createTaskCancel?.addEventListener("click", closeCreateTaskModal);

createTaskOverlay?.addEventListener("click", (event) => {
    if (event.target === createTaskOverlay) {
        closeCreateTaskModal();
    }
});

createTaskForm?.addEventListener("submit", (event) => {
    event.preventDefault();
    createTaskFromForm();
});

taskDetailClose?.addEventListener("click", closeTaskDetailOverlay);
taskDetailCancel?.addEventListener("click", closeTaskDetailOverlay);

taskDetailOverlay?.addEventListener("click", (event) => {
    if (event.target === taskDetailOverlay) {
        closeTaskDetailOverlay();
    }
});

taskDetailForm?.addEventListener("submit", (event) => {
    event.preventDefault();
    saveTaskDetailFromForm();
});

detailSubtaskAdd?.addEventListener("click", addDetailSubtask);

detailSubtaskInput?.addEventListener("keydown", (event) => {
    if (event.key !== "Enter") {
        return;
    }

    event.preventDefault();
    addDetailSubtask();
});

createSubtaskAdd?.addEventListener("click", addCreateSubtask);

createSubtaskInput?.addEventListener("keydown", (event) => {
    if (event.key !== "Enter") {
        return;
    }

    event.preventDefault();
    addCreateSubtask();
});

createSubtaskAssignee
    ?.querySelector("[data-subtask-assignee-toggle]")
    ?.addEventListener("click", () => {
        toggleSubtaskAssigneeDropdown(createSubtaskAssignee);
    });

newTaskAssigneeSearch?.addEventListener("input", () => {
    renderAssigneeOptions(newTaskAssigneeSearch.value);
});

newTaskAssigneeSearch?.addEventListener("focus", () => {
    renderAssigneeOptions(newTaskAssigneeSearch.value);
});

document.addEventListener("click", (event) => {
    if (!event.target.closest(".assignee-picker")) {
        assigneeOptions.innerHTML = "";
    }

    if (!event.target.closest(".subtask-assignee-dropdown")) {
        closeSubtaskAssigneeDropdowns();
    }

    if (!event.target.closest(".kanban-select-dropdown")) {
        closeKanbanSelects();
    }
});

addTaskTagBtn?.addEventListener("click", addCustomTag);

newTaskTagInput?.addEventListener("keydown", (event) => {
    if (event.key === "Enter" || event.key === ",") {
        event.preventDefault();
        addCustomTag();
    }
});

document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") {
        return;
    }

    closeReminderModal();

    if (createTaskOverlay?.classList.contains("is-open")) {
        closeCreateTaskModal();
    }

    if (taskDetailOverlay?.classList.contains("is-open")) {
        closeTaskDetailOverlay();
    }
});

window.addEventListener("project:selected", async (event) => {
    const project = state.workspace.projects.find(
        (item) => item.id === event.detail.projectId
    );

    if (!project) {
        return;
    }

    state.activeProject = project;
    await loadActiveBoard(project.kanbans?.[0]?.id ?? null);
});

window.addEventListener("sidebar:loaded", renderSidebar);

kanbanBoard.addEventListener("dragover", handleTaskDragOver);
kanbanBoard.addEventListener("dragleave", handleTaskDragLeave);
kanbanBoard.addEventListener("drop", handleTaskDrop);

kanbanBoard.addEventListener(
    "wheel",
    (event) => {
        const isVerticalWheel = Math.abs(event.deltaY) > Math.abs(event.deltaX);

        if (!isVerticalWheel) {
            return;
        }

        event.preventDefault();

        kanbanBoard.scrollBy({
            left: event.deltaY,
            behavior: "smooth",
        });
    },
    { passive: false }
);

/* =========================
   HELPERS
========================= */

function normalizeTaskTags(tags) {
    if (!Array.isArray(tags)) {
        return [];
    }

    return tags
        .map((tag) => {
            if (typeof tag === "string") {
                return tag;
            }

            return tag.name ?? tag.title ?? "";
        })
        .filter(Boolean);
}

function getTaskUsers(users) {
    if (Array.isArray(users)) {
        return users
            .map((user, index) => {
                if (typeof user === "string") {
                    const name = user.trim();

                    return {
                        name: name.startsWith("#")
                            ? `Участник ${name.slice(1)}`
                            : name || `Участник ${index + 1}`,
                    };
                }

                const assignee = normalizeAssignee(user);

                return {
                    name: assignee.name || `Участник ${index + 1}`,
                };
            })
            .filter(Boolean);
    }

    const count = Number(users) || 0;

    return Array.from({ length: count }).map((_, index) => ({
        name: `Участник ${index + 1}`,
    }));
}

function sameId(left, right) {
    return Boolean(left && right) && String(left) === String(right);
}

function getCurrentUserId() {
    return state.currentUser?.id ?? state.currentUser?.Id ?? null;
}

function isActiveProjectTeamAdmin() {
    const team = state.activeProject?.team ?? state.activeProject?.Team ?? null;

    if (normalizeTeamRoleValue(team?.role ?? team?.Role) === "Admin") {
        return true;
    }

    const currentUserId = getCurrentUserId();
    const members = team?.members ?? team?.Members ?? [];
    const currentMember = members.find((member) => {
        const memberId =
            member.userId ??
            member.UserId ??
            member.memberId ??
            member.MemberId ??
            member.id ??
            member.Id ??
            null;

        return currentUserId && memberId && String(memberId) === String(currentUserId);
    });

    return normalizeTeamRoleValue(currentMember?.role ?? currentMember?.Role) === "Admin";
}

function normalizeTeamRoleValue(role) {
    const normalized = String(role ?? "")
        .trim()
        .toLowerCase();

    if (normalized === "admin" || normalized === "админ" || normalized === "администратор" || normalized === "0") {
        return "Admin";
    }

    return "Member";
}

function getDefaultDeadline() {
    const date = new Date();
    date.setDate(date.getDate() + 1);
    return date.toISOString();
}

function escapeAttr(value) {
    return escapeHtml(value);
}

function showToast(text) {
    const toast = document.getElementById("toast");
    const toastText = document.getElementById("toastText");

    if (!toast || !toastText) {
        return;
    }

    toastText.textContent = text;
    toast.classList.add("is-visible");

    clearTimeout(showToast.timer);

    showToast.timer = setTimeout(() => {
        toast.classList.remove("is-visible");
    }, 2200);
}

function handleActionError(error, fallbackMessage) {
    console.error(error);
    showToast(fallbackMessage);
}

/* =========================
   START
========================= */
initKanbanSelects();
init();

try {
    if (typeof window.connectNotifications === "function") {
        window.connectNotifications(function (notification) {
            console.log("Получено уведомление:", notification);
            handleRealtimeNotification(notification);
        });
    } else {
        console.warn("connectNotifications не найден");
    }
} catch (error) {
    console.error("Не удалось подключить уведомления:", error);
}
function initKanbanSelects() {
    document.querySelectorAll("[data-kanban-select]").forEach((dropdown) => {
        const inputId = dropdown.dataset.kanbanSelect;
        const input = document.getElementById(inputId);
        const toggle = dropdown.querySelector(".kanban-select-toggle");
        const label = dropdown.querySelector("[data-kanban-select-label]");
        const options = dropdown.querySelectorAll(".kanban-select-option");

        toggle?.addEventListener("click", (event) => {
            event.stopPropagation();

            const shouldOpen = !dropdown.classList.contains("is-open");
            closeKanbanSelects(dropdown);

            dropdown.classList.toggle("is-open", shouldOpen);
            toggle.setAttribute("aria-expanded", String(shouldOpen));
        });

        options.forEach((option) => {
            option.addEventListener("click", (event) => {
                event.stopPropagation();

                input.value = option.dataset.value;
                label.textContent = option.textContent.trim();

                options.forEach((item) => {
                    item.classList.toggle("is-selected", item === option);
                });

                closeKanbanSelects();
            });
        });
    });
}

function closeKanbanSelects(exceptDropdown = null) {
    document.querySelectorAll(".kanban-select-dropdown.is-open").forEach((dropdown) => {
        if (dropdown === exceptDropdown) {
            return;
        }

        dropdown.classList.remove("is-open");
        dropdown.querySelector(".kanban-select-toggle")?.setAttribute("aria-expanded", "false");
    });
}
