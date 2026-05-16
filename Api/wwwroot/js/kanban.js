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
        <h2 class="column-title">${column.title}</h2>
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
        columnEl.appendChild(addTask);

        const deleteColumnBtn = columnEl.querySelector(".column-delete-btn");

        deleteColumnBtn.addEventListener("click", (event) => {
            event.stopPropagation();
            deleteColumn(columnIndex);
        });

        kanbanBoard.appendChild(columnEl);
    });

    const addColumn = document.createElement("button");
    addColumn.className = "add-column-btn";
    addColumn.type = "button";
    addColumn.innerHTML = `<span>＋</span><span>Добавить</span>`;
    kanbanBoard.appendChild(addColumn);
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
        <span class="date-pill">Apr 1, 2025</span>
        <span class="date-pill">9:41 AM</span>
      </div>

      <div class="date-pills">
        <span class="date-pill">Apr 2, 2025</span>
        <span class="date-pill">9:41 AM</span>
      </div>
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