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
import { createColumn, deleteColumn as deleteColumnRequest, renameColumn } from "./api/columnApi.js";
import { createTask, deleteTask as deleteTaskRequest, updateTask } from "./api/taskApi.js";
import { getMe } from "./api/userApi.js";

const toggleKanbanSidebarBtn = document.getElementById("toggleKanbanSidebar");
const closeKanbanSidebarBtn = document.getElementById("closeKanbanSidebar");
const kanbanTitle = document.getElementById("kanbanTitle");
const kanbanBoard = document.getElementById("kanbanBoard");

const reminderModal = document.getElementById("reminderModal");
const reminderText = document.getElementById("reminderText");
const reminderYes = document.getElementById("reminderYes");
const reminderNo = document.getElementById("reminderNo");
const reminderClose = document.getElementById("reminderClose");

let state = {
    workspace: { teams: [], projects: [] },
    activeProject: null,
    activeKanban: null,
    board: null,
    currentUser: null,
};

let selectedReminder = null;

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

reminderNo.addEventListener("click", closeReminderModal);
reminderClose.addEventListener("click", closeReminderModal);

reminderModal.addEventListener("click", (event) => {
    if (event.target === reminderModal) {
        closeReminderModal();
    }
});

reminderYes.addEventListener("click", () => {
    if (!selectedReminder) {
        return;
    }

    showToast(`Напоминание для ${selectedReminder.userName}: «${selectedReminder.taskTitle}»`);
    closeReminderModal();
});

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
        closeReminderModal();
    }
});

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
    state.activeKanban = selectKanbanFromUrl(state.activeProject) ?? state.activeProject.kanbans[0] ?? null;

    if (kanbanId) {
        state.activeKanban = state.activeProject.kanbans.find((kanban) => kanban.id === kanbanId) ?? state.activeKanban;
    }

    state.board = await loadBoard(state.activeProject, state.activeKanban?.id ?? null);
    state.activeKanban = state.board.selectedKanban ?? state.activeKanban;
    updateBoardUrl(state.activeProject, state.activeKanban);
    renderSidebar();
    renderKanban(state.board);
}

function renderSidebar() {
    const tasks = getBoardTasks(state.board);
    renderSidebarNavigation(state.workspace, state.activeProject?.id, getTaskStats(tasks));
}

function renderLoading() {
    kanbanTitle.textContent = "Kanban";
    kanbanBoard.innerHTML = `<div class="kanban-state">Загрузка доски...</div>`;
}

function renderError(message) {
    kanbanTitle.textContent = "Kanban";
    kanbanBoard.innerHTML = `<div class="kanban-state kanban-state-error">${escapeHtml(message)}</div>`;
}

function renderEmpty(message) {
    kanbanTitle.textContent = "Kanban";
    kanbanBoard.innerHTML = `<div class="kanban-state">${escapeHtml(message)}</div>`;
}

function renderKanban(board) {
    kanbanTitle.textContent = board.title;
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
        <h2 class="column-title">${escapeHtml(column.title)}</h2>
        <span class="column-count">${column.tasks.length}</span>

        <button
          class="column-icon column-delete-btn"
          type="button"
          aria-label="Удалить колонку"
          data-column-index="${columnIndex}"
        >
          <img class="column-icon-img" src="components/images/delete.svg" alt="удалить">
        </button>

        <button
          class="column-icon column-edit-btn"
          type="button"
          aria-label="Редактировать колонку"
          data-column-index="${columnIndex}"
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
        addTask.addEventListener("click", () => addTaskToColumn(column));
        columnEl.appendChild(addTask);

        columnEl.querySelector(".column-delete-btn").addEventListener("click", (event) => {
            event.stopPropagation();
            deleteColumn(column);
        });

        columnEl.querySelector(".column-edit-btn").addEventListener("click", (event) => {
            event.stopPropagation();
            editColumn(column);
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

async function addColumnToBoard() {
    const name = prompt("Название колонки");

    if (!name?.trim() || !state.board?.id) {
        return;
    }

    try {
        await createColumn(state.board.id, name.trim());
        await reloadBoard(`Колонка «${name.trim()}» добавлена`);
    } catch (error) {
        handleActionError(error, "Не удалось добавить колонку");
    }
}

async function editColumn(column) {
    const name = prompt("Новое название колонки", column.title);

    if (!name?.trim() || name.trim() === column.title) {
        return;
    }

    try {
        await renameColumn(column.id, name.trim());
        await reloadBoard(`Колонка переименована в «${name.trim()}»`);
    } catch (error) {
        handleActionError(error, "Не удалось переименовать колонку");
    }
}

async function deleteColumn(column) {
    const confirmed = confirm(`Удалить колонку «${column.title}» вместе с задачами: ${column.tasks.length}?`);

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

async function addTaskToColumn(column) {
    const currentUserId = getCurrentUserId();

    if (!currentUserId) {
        showToast("Нужна авторизация для создания задачи");
        return;
    }

    const name = prompt("Название задачи");

    if (!name?.trim()) {
        return;
    }

    const description = prompt("Описание задачи", "") ?? "";

    try {
        await createTask(state.board.id, {
            name: name.trim(),
            description: description.trim(),
            priority: "Medium",
            deadline: getDefaultDeadline(),
            userId: currentUserId,
            columnId: column.id,
            order: null,
            tagIds: [],
        });
        await reloadBoard(`Задача «${name.trim()}» добавлена`);
    } catch (error) {
        handleActionError(error, "Не удалось добавить задачу");
    }
}

async function editTask(task) {
    const name = prompt("Название задачи", task.title);

    if (!name?.trim()) {
        return;
    }

    const description = prompt("Описание задачи", task.description) ?? "";

    try {
        await updateTask(task.id, {
            name: name.trim(),
            description: description.trim(),
            priority: task.priority,
            deadline: task.deadline,
            userId: task.userId,
        });
        await reloadBoard(`Задача «${name.trim()}» обновлена`);
    } catch (error) {
        handleActionError(error, "Не удалось обновить задачу");
    }
}

async function deleteTask(task) {
    const confirmed = confirm(`Удалить задачу «${task.title}»?`);

    if (!confirmed) {
        return;
    }

    try {
        await deleteTaskRequest(task.id);
        await reloadBoard(`Задача «${task.title}» удалена`);
    } catch (error) {
        handleActionError(error, "Не удалось удалить задачу");
    }
}

function createTaskCard(task, column, columnIndex, taskIndex) {
    const color = column.color;
    const doneLabel = column.done ? "Дата выполнения" : "Дедлайн";
    const tags = [
        task.priority,
        task.time,
        ...task.tags.map((tag) => tag.name),
    ];

    const card = document.createElement("article");
    card.className = "task-card";
    card.style.setProperty("--task-color", color);

    card.innerHTML = `
    <div class="task-top">
      <button class="task-icon-btn task-edit-btn" type="button" aria-label="Редактировать задачу">✎</button>
      <h3 class="task-title">${escapeHtml(task.title)}</h3>
      <button
          class="task-icon-btn task-delete-btn"
          type="button"
          aria-label="Удалить задачу"
          data-column-index="${columnIndex}"
          data-task-index="${taskIndex}"
        >
          <img class="task-icon-img" src="components/images/delete.svg" alt="удалить">
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
      ${escapeHtml(task.description).replace(/\n/g, "<br>")}
    </div>

    <div class="task-footer">
      <div class="task-tags">
        ${tags.map((tag) => `<span class="task-tag">${escapeHtml(tag)}</span>`).join("")}
      </div>
      <div class="task-users">
        ${getTaskUsers(task.users)
        .map((user) => `
      <button
        class="task-user-wrap"
        type="button"
        data-user-name="${escapeAttr(user.name)}"
        data-task-title="${escapeAttr(task.title)}"
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
        editTask(task);
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

    return card;
}

async function reloadBoard(toastText = null) {
    await loadActiveBoard(state.activeKanban?.id ?? null);

    if (toastText) {
        showToast(toastText);
    }
}

function getTaskUsers(users) {
    if (Array.isArray(users)) {
        return users;
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

window.addEventListener("project:selected", async (event) => {
    const project = state.workspace.projects.find((item) => item.id === event.detail.projectId);

    if (!project) {
        return;
    }

    state.activeProject = project;
    await loadActiveBoard(project.kanbans[0]?.id ?? null);
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

init();
