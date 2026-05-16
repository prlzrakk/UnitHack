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

    project.columns.forEach((column) => {
        const columnEl = document.createElement("section");
        columnEl.className = "kanban-column";

        columnEl.innerHTML = `
      <div class="column-head">
        <h2 class="column-title">${column.title}</h2>
        <span class="column-count">${column.tasks.length}</span>
        <button class="column-icon" aria-label="Удалить колонку">⌫</button>
        <button class="column-icon" aria-label="Редактировать колонку">✎</button>
      </div>
    `;

        column.tasks.forEach((task) => {
            columnEl.appendChild(createTaskCard(task, column));
        });

        const addTask = document.createElement("button");
        addTask.className = "add-task-btn";
        addTask.innerHTML = `<span>＋</span><span>Добавить</span>`;
        columnEl.appendChild(addTask);

        kanbanBoard.appendChild(columnEl);
    });

    const addColumn = document.createElement("button");
    addColumn.className = "add-column-btn";
    addColumn.innerHTML = `<span>＋</span><span>Добавить</span>`;
    kanbanBoard.appendChild(addColumn);
}

function createTaskCard(task, column) {
    const color = task.color || column.color;
    const doneLabel = column.done ? "Дата выполнения" : "Дедлайн";

    const card = document.createElement("article");
    card.className = "task-card";
    card.style.setProperty("--task-color", color);

    card.innerHTML = `
    <div class="task-top">
      <button class="task-icon-btn" aria-label="Редактировать задачу">✎</button>
      <h3 class="task-title">${task.title}</h3>
      <button class="task-icon-btn" aria-label="Удалить задачу">⌫</button>
    </div>

    <div class="task-dates">
      <div class="date-label">Дата создания</div>
      <div class="date-label">${doneLabel}</div>

      <div class="date-pills">
        <span class="date-pill">Apr 1, 2025</span>
        <span class="date-pill">9:41 AM</span>
      </div>

      <div class="date-pills">
        <span class="date-pill">Apr 1, 2025</span>
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
        ${Array.from({ length: task.users })
        .map(() => `<span class="task-user">⌾</span>`)
        .join("")}
      </div>
    </div>
  `;
    
    card.addEventListener("dblclick", () => {
        showToast(`напомнить об "${task.title}"`);
    });

    return card;
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
