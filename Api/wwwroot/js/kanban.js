const toggleKanbanSidebarBtn = document.getElementById("toggleKanbanSidebar");
const closeKanbanSidebarBtn = document.getElementById("closeKanbanSidebar");

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

let activeProjectId = getProjectFromUrl();

const kanbanTitle = document.getElementById("kanbanTitle");
const kanbanBoard = document.getElementById("kanbanBoard");
const projectList = document.getElementById("kanbanProjectList");
const teamList = document.getElementById("kanbanTeamList");

const reminderModal = document.getElementById("reminderModal");
const reminderText = document.getElementById("reminderText");
const reminderYes = document.getElementById("reminderYes");
const reminderNo = document.getElementById("reminderNo");
const reminderClose = document.getElementById("reminderClose");
const createTaskOverlay = document.getElementById("createTaskOverlay");
const createTaskForm = document.getElementById("createTaskForm");
const createTaskClose = document.getElementById("createTaskClose");
const createTaskCancel = document.getElementById("createTaskCancel");

const newTaskTitle = document.getElementById("newTaskTitle");
const newTaskDescription = document.getElementById("newTaskDescription");
const newTaskAssigneeSearch = document.getElementById("newTaskAssigneeSearch");
const assigneeOptions = document.getElementById("assigneeOptions");

const newTaskTagInput = document.getElementById("newTaskTagInput");
const addTaskTagBtn = document.getElementById("addTaskTagBtn");
const createdTags = document.getElementById("createdTags");

let selectedAssignee = null;
let selectedTags = [];
const newTaskUser = document.getElementById("newTaskUser");
const newTaskPriority = document.getElementById("newTaskPriority");
const newTaskComplexity = document.getElementById("newTaskComplexity");
const newTaskDeadline = document.getElementById("newTaskDeadline");

let selectedColumnForNewTask = null;
let selectedReminder = null;

function getProjectFromUrl() {
    const params = new URLSearchParams(window.location.search);
    const project = params.get("project");

    if (project && PROJECTS[project]) {
        return project;
    }

    return "hackathon";
}

function setProject(projectId) {
    activeProjectId = projectId;

    const url = new URL(window.location.href);
    url.searchParams.set("project", projectId);
    window.history.pushState({}, "", url);

    renderPage();
}
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
function renderPage() {
    const project = PROJECTS[activeProjectId];

    if (projectList) {
        renderProjectMenu();
    }

    if (teamList) {
        renderTeams(project);
    }

    renderKanban(project);
}

function renderProjectMenu() {
    projectList.innerHTML = "";

    Object.entries(PROJECTS).forEach(([projectId, project]) => {
        const button = document.createElement("button");
        button.className = `project-item ${projectId === activeProjectId ? "active" : ""}`;

        button.innerHTML = `
      <span class="project-color" style="background:${project.color}"></span>
      <span class="project-name">${project.name}</span>
      <span class="project-meta ${projectId === activeProjectId ? "active-meta" : ""}">
        ${project.meta}
      </span>
    `;

        button.addEventListener("click", () => {
            setProject(projectId);
        });

        projectList.appendChild(button);
    });
}

function renderTeams(project) {
    teamList.innerHTML = "";

    project.teams.forEach((team) => {
        const button = document.createElement("button");
        button.className = "team-card";

        button.innerHTML = `
      <span>${team.name}</span>
      <span class="team-dot" style="background:${team.color}"></span>
    `;

        teamList.appendChild(button);
    });
}

function renderKanban(project) {
    kanbanTitle.textContent = project.title;
    kanbanBoard.innerHTML = "";

    project.columns.forEach((column, columnIndex) => {
        const columnEl = document.createElement("section");
        columnEl.className = "kanban-column";

        columnEl.innerHTML = `
      <div class="column-head">
        <h2 
          class="column-title" 
          data-column-title 
          data-column-index="${columnIndex}"
        >
          ${column.title || "Без названия"}
        </h2>

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

        addTask.addEventListener("click", () => {
            openCreateTaskModal(columnIndex);
        });

        columnEl.appendChild(addTask);
        const deleteColumnBtn = columnEl.querySelector(".column-delete-btn");
        const editColumnBtn = columnEl.querySelector(".column-edit-btn");
        const columnTitle = columnEl.querySelector("[data-column-title]");

        deleteColumnBtn.addEventListener("click", (event) => {
            event.stopPropagation();
            deleteColumn(columnIndex);
        });

        editColumnBtn.addEventListener("click", (event) => {
            event.stopPropagation();
            startEditColumnTitle(columnTitle, columnIndex);
        });

        kanbanBoard.appendChild(columnEl);
    });

    const addColumn = document.createElement("button");
    addColumn.className = "add-column-btn";
    addColumn.type = "button";
    addColumn.innerHTML = `<span>＋</span><span>Добавить</span>`;

    addColumn.addEventListener("click", addEmptyColumn);

    kanbanBoard.appendChild(addColumn);
}

function openCreateTaskModal(columnIndex) {
    selectedColumnForNewTask = columnIndex;
    selectedAssignee = null;
    selectedTags = [];

    createTaskForm.reset();

    newTaskDeadline.value = getDefaultDateTimeLocal();
    newTaskAssigneeSearch.value = "";

    renderCreatedTags();
    renderAssigneeOptions("");

    createTaskOverlay.classList.add("is-open");

    setTimeout(() => {
        newTaskTitle.focus();
    }, 0);
}

function closeCreateTaskModal() {
    createTaskOverlay.classList.remove("is-open");
    selectedColumnForNewTask = null;
    selectedAssignee = null;
    selectedTags = [];
}

function createTaskFromForm() {
    const title = newTaskTitle.value.trim();
    const description = newTaskDescription.value.trim();
    const priority = newTaskPriority.value;
    const complexity = newTaskComplexity.value;
    const deadline = newTaskDeadline.value;

    if (!title) {
        newTaskTitle.focus();
        return;
    }

    const project = PROJECTS[activeProjectId];
    const column = project.columns[selectedColumnForNewTask];

    if (!column) {
        return;
    }

    const newTask = {
        title,
        description: description || "Описание не добавлено",
        priority,
        time: complexity,
        deadline,
        createdAt: new Date().toISOString(),
        users: selectedAssignee ? [{ name: selectedAssignee.name }] : [],
        tags: [...selectedTags],
    };

    column.tasks.push(newTask);

    closeCreateTaskModal();
    renderPage();

    showToast(`Задача «${title}» создана`);
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
createTaskClose.addEventListener("click", closeCreateTaskModal);
createTaskCancel.addEventListener("click", closeCreateTaskModal);

createTaskOverlay.addEventListener("click", (event) => {
    if (event.target === createTaskOverlay) {
        closeCreateTaskModal();
    }
});

createTaskForm.addEventListener("submit", (event) => {
    event.preventDefault();
    createTaskFromForm();
});

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && createTaskOverlay.classList.contains("is-open")) {
        closeCreateTaskModal();
    }
});
function getProjectAssignees() {
    const project = PROJECTS[activeProjectId];

    const assignees = [];

    if (Array.isArray(project.teams)) {
        project.teams.forEach((team) => {
            assignees.push({
                name: team.name,
                color: team.color || "#f4864d",
            });
        });
    }

    assignees.unshift({
        name: "Не назначен",
        color: "#838383",
    });

    return assignees;
}

function renderAssigneeOptions(searchValue) {
    const query = searchValue.trim().toLowerCase();
    const assignees = getProjectAssignees().filter((item) =>
        item.name.toLowerCase().includes(query)
    );

    assigneeOptions.innerHTML = "";

    assignees.forEach((assignee) => {
        const button = document.createElement("button");
        button.className = `assignee-option ${
            selectedAssignee?.name === assignee.name ? "is-selected" : ""
        }`;
        button.type = "button";

        button.innerHTML = `
            <span>${escapeHtml(assignee.name)}</span>
            <span class="assignee-dot" style="background:${assignee.color}"></span>
        `;

        button.addEventListener("click", () => {
            selectedAssignee = assignee.name === "Не назначен" ? null : assignee;
            newTaskAssigneeSearch.value = assignee.name;
            renderAssigneeOptions(assignee.name);
        });

        assigneeOptions.appendChild(button);
    });
}
function getProjectAssignees() {
    const project = PROJECTS[activeProjectId];

    const assignees = [];

    if (Array.isArray(project.teams)) {
        project.teams.forEach((team) => {
            assignees.push({
                name: team.name,
                color: team.color || "#f4864d",
            });
        });
    }

    assignees.unshift({
        name: "Не назначен",
        color: "#838383",
    });

    return assignees;
}

function renderAssigneeOptions(searchValue) {
    const query = searchValue.trim().toLowerCase();
    const assignees = getProjectAssignees().filter((item) =>
        item.name.toLowerCase().includes(query)
    );

    assigneeOptions.innerHTML = "";

    assignees.forEach((assignee) => {
        const button = document.createElement("button");
        button.className = `assignee-option ${
            selectedAssignee?.name === assignee.name ? "is-selected" : ""
        }`;
        button.type = "button";

        button.innerHTML = `
            <span>${escapeHtml(assignee.name)}</span>
            <span class="assignee-dot" style="background:${assignee.color}"></span>
        `;

        button.addEventListener("click", () => {
            selectedAssignee = assignee.name === "Не назначен" ? null : assignee;
            newTaskAssigneeSearch.value = assignee.name;
            renderAssigneeOptions(assignee.name);
        });

        assigneeOptions.appendChild(button);
    });
}
createTaskClose.addEventListener("click", closeCreateTaskModal);
createTaskCancel.addEventListener("click", closeCreateTaskModal);

createTaskOverlay.addEventListener("click", (event) => {
    if (event.target === createTaskOverlay) {
        closeCreateTaskModal();
    }
});

createTaskForm.addEventListener("submit", (event) => {
    event.preventDefault();
    createTaskFromForm();
});

newTaskAssigneeSearch.addEventListener("input", () => {
    renderAssigneeOptions(newTaskAssigneeSearch.value);
});

newTaskAssigneeSearch.addEventListener("focus", () => {
    renderAssigneeOptions(newTaskAssigneeSearch.value);
});

addTaskTagBtn.addEventListener("click", addCustomTag);

newTaskTagInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter" || event.key === ",") {
        event.preventDefault();
        addCustomTag();
    }
});

document.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && createTaskOverlay.classList.contains("is-open")) {
        closeCreateTaskModal();
    }
});
function addEmptyColumn() {
    const project = PROJECTS[activeProjectId];

    project.columns.push({
        title: "Без названия",
        color: "#ef6a35",
        tasks: [],
    });

    renderPage();

    const newColumnIndex = project.columns.length - 1;
    const newColumnTitle = document.querySelector(
        `[data-column-title][data-column-index="${newColumnIndex}"]`
    );

    if (newColumnTitle) {
        startEditColumnTitle(newColumnTitle, newColumnIndex);
    }

    kanbanBoard.scrollTo({
        left: kanbanBoard.scrollWidth,
        behavior: "smooth",
    });

    showToast("Добавлена новая колонка");
}

function startEditColumnTitle(titleEl, columnIndex) {
    if (!titleEl) {
        return;
    }

    const oldTitle = titleEl.textContent.trim();

    titleEl.contentEditable = "true";
    titleEl.classList.add("is-editing");
    titleEl.focus();

    selectElementText(titleEl);

    function save() {
        const newTitle = titleEl.textContent.trim() || "Без названия";

        PROJECTS[activeProjectId].columns[columnIndex].title = newTitle;

        titleEl.contentEditable = "false";
        titleEl.classList.remove("is-editing");
        titleEl.textContent = newTitle;

        cleanup();
        showToast(`Колонка переименована: «${newTitle}»`);
    }

    function cancel() {
        titleEl.contentEditable = "false";
        titleEl.classList.remove("is-editing");
        titleEl.textContent = oldTitle || "Без названия";

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

function selectElementText(element) {
    const range = document.createRange();
    range.selectNodeContents(element);

    const selection = window.getSelection();
    selection.removeAllRanges();
    selection.addRange(range);
}
function deleteColumn(columnIndex) {
    const project = PROJECTS[activeProjectId];
    const column = project.columns[columnIndex];

    if (!column) {
        return;
    }

    const confirmed = confirm(
        `Удалить колонку «${column.title}» вместе с задачами: ${column.tasks.length}?`
    );

    if (!confirmed) {
        return;
    }

    project.columns.splice(columnIndex, 1);
    renderPage();
    showToast(`Колонка «${column.title}» удалена`);
}

function deleteTask(columnIndex, taskIndex) {
    const project = PROJECTS[activeProjectId];
    const column = project.columns[columnIndex];

    if (!column) {
        return;
    }

    const task = column.tasks[taskIndex];

    if (!task) {
        return;
    }

    const confirmed = confirm(`Удалить задачу «${task.title}»?`);

    if (!confirmed) {
        return;
    }

    column.tasks.splice(taskIndex, 1);
    renderPage();
    showToast(`Задача «${task.title}» удалена`);
}
function createTaskCard(task, column, columnIndex, taskIndex) {
    const color = task.color || column.color;
    const doneLabel = column.done ? "Дата выполнения" : "Дедлайн";
    const createdParts = formatDateTimeParts(task.createdAt || "2025-04-01T09:41");
    const deadlineParts = formatDateTimeParts(task.deadline || "2025-04-02T09:41");

    const card = document.createElement("article");
    card.className = "task-card";
    card.style.setProperty("--task-color", color);

    card.innerHTML = `
    <div class="task-top">
      <button class="task-icon-btn" aria-label="Редактировать задачу">✎</button>
      <h3 class="task-title">${task.title}</h3>
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
      <span class="date-pill">${createdParts.date}</span>
      <span class="date-pill">${createdParts.time}</span>
    </div>
    
    <div class="date-pills">
      <span class="date-pill">${deadlineParts.date}</span>
      <span class="date-pill">${deadlineParts.time}</span>
    </div>
    </div>
    <div class="task-tags">
  <span class="task-tag">${task.priority}</span>
  <span class="task-tag">${task.time}</span>

  ${normalizeTaskTags(task.tags)
        .map((tag) => `<span class="task-tag task-custom-tag">#${escapeHtml(tag)}</span>`)
        .join("")}
</div>
    <div class="task-description">
      ${task.description.replace(/\n/g, "<br>")}
    </div>

    <div class="task-footer">
      <div class="task-tags">
        <span class="task-tag">${task.priority}</span>
        <span class="task-tag">${task.time}</span>
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
    const deleteTaskBtn = card.querySelector(".task-delete-btn");

    deleteTaskBtn.addEventListener("click", (event) => {
        event.stopPropagation();
        deleteTask(columnIndex, taskIndex);
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
function getTaskUsers(users) {
    if (Array.isArray(users)) {
        return users;
    }

    const count = Number(users) || 0;

    return Array.from({ length: count }).map((_, index) => ({
        name: `Участник ${index + 1}`,
    }));
}

function escapeAttr(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll('"', "&quot;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;");
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

renderPage();

window.addEventListener("project:selected", (event) => {
    setProject(event.detail.projectId);
});
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
function normalizeTaskTags(tags) {
    if (!Array.isArray(tags)) {
        return [];
    }

    return tags;
}

function formatDateTimeParts(value) {
    if (!value) {
        return {
            date: "Без даты",
            time: "--:--",
        };
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return {
            date: value,
            time: "--:--",
        };
    }

    return {
        date: date.toLocaleDateString("ru-RU", {
            day: "2-digit",
            month: "short",
            year: "numeric",
        }),
        time: date.toLocaleTimeString("ru-RU", {
            hour: "2-digit",
            minute: "2-digit",
        }),
    };
}
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
function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}
function normalizeUsers(users) {
    if (Array.isArray(users)) {
        return users;
    }

    const count = Number(users) || 0;

    return Array.from({ length: count }).map((_, index) => ({
        name: `Участник ${index + 1}`,
    }));
}

function getUserInitial(name) {
    return name.trim().charAt(0).toUpperCase();
}

function formatTaskDate(dateValue) {
    if (!dateValue) {
        return "Без даты";
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
        return dateValue;
    }

    return date.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "short",
        year: "numeric",
    });
}