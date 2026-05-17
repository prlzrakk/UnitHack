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
    updateTask,
} from "./api/taskApi.js";

import { getMe } from "./api/userApi.js";

/* =========================
   DOM
========================= */

const toggleKanbanSidebarBtn = document.getElementById("toggleKanbanSidebar");
const closeKanbanSidebarBtn = document.getElementById("closeKanbanSidebar");

const kanbanTitle = document.getElementById("kanbanTitle");
const kanbanBoard = document.getElementById("kanbanBoard");

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

const TASK_ASSIGNEES_STORAGE_KEY = "boardifyTaskAssignees";

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
};

let selectedReminder = null;
let selectedColumnForNewTask = null;
let selectedAssignees = [];
let selectedTags = [];
let selectedTaskForDetail = null;
let detailSubtasks = [];

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
    } catch (error) {
        console.error(error);
        renderError("Не удалось загрузить доску. Проверь авторизацию и доступ к API.");
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
    kanbanBoard.innerHTML = `<div class="kanban-state">Загрузка доски...</div>`;
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

    if (!board.id) {
        renderEmpty("У проекта пока нет досок");
        return;
    }

    board.columns.forEach((column, columnIndex) => {
        const columnEl = document.createElement("section");
        columnEl.className = "kanban-column";

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

        deleteColumnBtn.addEventListener("click", (event) => {
            event.stopPropagation();
            deleteColumn(column);
        });

        editColumnBtn.addEventListener("click", (event) => {
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
        showToast("Сначала выбери доску");
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
    card.className = "task-card";
    card.style.setProperty("--task-color", color);
    card.tabIndex = 0;
    card.setAttribute("role", "button");
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

    card.addEventListener("click", () => {
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

function openTaskDetailOverlay(task) {
    selectedTaskForDetail = task;
    detailSubtasks = getTaskSubtasks(task);

    detailTaskTitle.value = task.title || "";
    detailTaskDescription.value = task.description || "";
    detailTaskAssignee.textContent = getTaskAssigneeName(task);
    detailTaskPriority.value = normalizePriorityForSelect(task.priority);
    detailTaskComplexity.value = task.complexity || "";
    detailTaskDeadline.value = toDateTimeLocalValueSafe(task.deadline);

    renderDetailSubtasks();

    taskDetailOverlay.classList.add("is-open");
    taskDetailOverlay.setAttribute("aria-hidden", "false");

    setTimeout(() => {
        detailTaskTitle.focus();
    }, 0);
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

        closeTaskDetailOverlay();
        await reloadBoard(`Задача «${name}» обновлена`);
    } catch (error) {
        handleActionError(error, "Не удалось обновить задачу");
    }
}

function getTaskSubtasks(task) {
    if (!Array.isArray(task.subtasks)) {
        return [];
    }

    return task.subtasks.map((subtask, index) => {
        if (typeof subtask === "string") {
            return {
                id: `subtask-${index}`,
                title: subtask,
                done: false,
            };
        }

        return {
            id: subtask.id ?? `subtask-${index}`,
            title: subtask.title ?? subtask.name ?? "Подзадача",
            done: Boolean(subtask.done ?? subtask.isDone ?? subtask.completed),
        };
    });
}

function renderDetailSubtasks() {
    detailSubtaskList.innerHTML = "";

    if (!detailSubtasks.length) {
        detailSubtaskList.innerHTML = `<div class="subtask-empty">Подзадач пока нет</div>`;
        return;
    }

    detailSubtasks.forEach((subtask) => {
        const row = document.createElement("div");
        row.className = `subtask-item ${subtask.done ? "is-done" : ""}`;

        row.innerHTML = `
            <input
                class="subtask-checkbox"
                type="checkbox"
                ${subtask.done ? "checked" : ""}
                aria-label="Отметить подзадачу"
            >
            <span class="subtask-title">${escapeHtml(subtask.title)}</span>
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
    });

    detailSubtaskInput.value = "";
    renderDetailSubtasks();
}

function getTaskAssigneeName(task) {
    const user = getTaskUsers(task.users)[0];

    if (user?.name) {
        return user.name;
    }

    return task.userId ? `#${task.userId.slice(0, 8)}` : "Не назначен";
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

    createTaskForm.reset();

    newTaskDeadline.value = getDefaultDateTimeLocal();
    newTaskAssigneeSearch.value = "";

    renderCreatedTags();
    renderSelectedAssignees();
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
        showToast("Доска не выбрана");
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

        rememberTaskAssignees(
            readTaskId(createdTask),
            selectedAssignees.length
                ? selectedAssignees
                : [resolveAssigneeById(userId)]
        );

        closeCreateTaskModal();
        await reloadBoard(`Задача «${title}» создана`);
    } catch (error) {
        handleActionError(error, "Не удалось создать задачу");
    }
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

    selectedAssignees.push(assignee);
    renderSelectedAssignees();
}

function removeSelectedAssignee(assigneeKey) {
    selectedAssignees = selectedAssignees.filter((assignee) =>
        getAssigneeKey(assignee) !== assigneeKey
    );
    renderSelectedAssignees();
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

newTaskAssigneeSearch?.addEventListener("input", () => {
    renderAssigneeOptions(newTaskAssigneeSearch.value);
});

newTaskAssigneeSearch?.addEventListener("focus", () => {
    renderAssigneeOptions(newTaskAssigneeSearch.value);
});

document.addEventListener("click", (event) => {
    if (!newTaskAssigneeSearch || !assigneeOptions) {
        return;
    }

    if (!event.target.closest(".assignee-picker")) {
        assigneeOptions.innerHTML = "";
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

function getCurrentUserId() {
    return state.currentUser?.id ?? state.currentUser?.Id ?? null;
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

init();
