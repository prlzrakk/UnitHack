import { getKanban, getProjectKanbans } from "./api/kanbanApi.js";
import { getTeamProjects } from "./api/projectApi.js";
import { getTeam, getTeams } from "./api/teamApi.js";

const COLORS = ["#ef6a35", "#4668ad", "#ffe3d8", "#407d52", "#e6a0a6"];

export async function loadWorkspace() {
    const teamItems = await getTeams();
    const teamGroups = await Promise.all((teamItems ?? []).map(loadTeamGroup));

    return {
        teams: teamGroups.map((group) => group.team),
        projects: teamGroups.flatMap((group) => group.projects),
    };
}

export async function loadBoard(project, kanbanId = null) {
    const selectedKanban = selectKanban(project, kanbanId);

    if (!selectedKanban) {
        return createEmptyBoard(project);
    }

    const board = await getKanban(selectedKanban.id);
    return normalizeBoard(project, board);
}

export function selectProjectFromUrl(projects) {
    const params = new URLSearchParams(window.location.search);
    const projectId = params.get("project");

    return projects.find((project) => project.id === projectId) ?? projects[0] ?? null;
}

export function selectKanbanFromUrl(project) {
    const params = new URLSearchParams(window.location.search);
    const kanbanId = params.get("kanban");

    return selectKanban(project, kanbanId);
}

export function updateBoardUrl(project, kanban) {
    const url = new URL(window.location.href);

    if (project?.id) {
        url.searchParams.set("project", project.id);
    }

    if (kanban?.id) {
        url.searchParams.set("kanban", kanban.id);
    } else {
        url.searchParams.delete("kanban");
    }

    window.history.pushState({}, "", url);
}

export function renderSidebarNavigation(workspace, activeProjectId, taskStats = null) {
    renderSidebarProjects(workspace.projects, activeProjectId);
    renderSidebarTeams(workspace.teams);
    renderSidebarTaskStats(taskStats);
    window.BoardifySidebar?.refreshProjectLinks?.();
}

export function getBoardTasks(board) {
    return (board?.columns ?? []).flatMap((column) => column.tasks ?? []);
}

export function getTaskStats(tasks) {
    const now = new Date();
    const weekEnd = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);

    return {
        urgent: tasks.filter((task) => {
            const deadline = parseDate(task.deadline);
            return deadline && deadline <= weekEnd;
        }).length,
        week: tasks.filter((task) => {
            const deadline = parseDate(task.deadline);
            return deadline && deadline >= now && deadline <= weekEnd;
        }).length,
    };
}

export function formatDate(value) {
    const date = parseDate(value);

    if (!date) {
        return "—";
    }

    return date.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "short",
        year: "numeric",
    });
}

export function formatTime(value) {
    const date = parseDate(value);

    if (!date) {
        return "—";
    }

    return date.toLocaleTimeString("ru-RU", {
        hour: "2-digit",
        minute: "2-digit",
    });
}

export function formatTimeLeft(value) {
    const date = parseDate(value);

    if (!date) {
        return "без срока";
    }

    const diffMs = date.getTime() - Date.now();
    const absMs = Math.abs(diffMs);
    const dayMs = 24 * 60 * 60 * 1000;
    const hourMs = 60 * 60 * 1000;

    if (absMs < hourMs) {
        return diffMs >= 0 ? "меньше часа" : "просрочено";
    }

    if (absMs < dayMs) {
        const hours = Math.ceil(absMs / hourMs);
        return diffMs >= 0 ? `${hours} ч` : `-${hours} ч`;
    }

    const days = Math.ceil(absMs / dayMs);
    return diffMs >= 0 ? `${days} дн` : `-${days} дн`;
}

export function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function selectKanban(project, kanbanId) {
    if (!project) {
        return null;
    }

    return project.kanbans.find((kanban) => kanban.id === kanbanId) ?? project.kanbans[0] ?? null;
}

async function loadTeamGroup(team, index) {
    const teamId = readId(team);
    const [details, projects] = await Promise.all([
        getTeam(teamId).catch(() => null),
        getTeamProjects(teamId).catch(() => []),
    ]);

    const teamModel = {
        id: teamId,
        name: readText(team, "name", "Name") || "Команда",
        role: readText(team, "role", "Role"),
        members: details?.members ?? details?.Members ?? [],
        color: COLORS[index % COLORS.length],
    };

    const projectModels = await Promise.all((projects ?? []).map((project, projectIndex) =>
        loadProject(teamModel, project, projectIndex)
    ));

    return {
        team: teamModel,
        projects: projectModels,
    };
}

async function loadProject(team, project, index) {
    const projectId = readId(project);
    const kanbanResponse = await getProjectKanbans(projectId).catch(() => ({ kanbans: [] }));
    const kanbans = (kanbanResponse?.kanbans ?? kanbanResponse?.Kanbans ?? []).map((kanban) => ({
        id: readId(kanban),
        projectId,
        name: readText(kanban, "name", "Name") || "Kanban",
    }));

    return {
        id: projectId,
        teamId: team.id,
        team,
        name: readText(project, "name", "Name") || "Проект",
        color: COLORS[index % COLORS.length],
        meta: kanbans.length === 1 ? "1 доска" : `${kanbans.length} досок`,
        kanbans,
    };
}

function normalizeBoard(project, board) {
    const kanbanId = readId(board);
    const columns = (board?.columns ?? board?.Columns ?? [])
        .slice()
        .sort((a, b) => Number(a.order ?? a.Order ?? 0) - Number(b.order ?? b.Order ?? 0))
        .map((column, index) => normalizeColumn(column, index));

    return {
        id: kanbanId,
        projectId: readText(board, "projectId", "ProjectId") || project.id,
        title: readText(board, "name", "Name") || project.name,
        project,
        selectedKanban: project.kanbans.find((kanban) => kanban.id === kanbanId) ?? null,
        columns,
    };
}

function normalizeColumn(column, index) {
    const title = readText(column, "name", "Name") || "Колонка";

    return {
        id: readId(column),
        title,
        order: Number(column?.order ?? column?.Order ?? index),
        color: COLORS[index % COLORS.length],
        done: title.trim().toLowerCase() === "done",
        tasks: (column?.tasks ?? column?.Tasks ?? [])
            .slice()
            .sort((a, b) => Number(a.order ?? a.Order ?? 0) - Number(b.order ?? b.Order ?? 0))
            .map(normalizeTask),
    };
}

function normalizeTask(task) {
    const userId = readText(task, "userId", "UserId");
    const deadline = readText(task, "deadline", "Deadline");
    const createdAt = readText(task, "createdAt", "CreatedAt");
    const tags = (task?.tags ?? task?.Tags ?? []).map((tag) => ({
        id: readId(tag),
        name: readText(tag, "name", "Name") || "tag",
    }));

    return {
        id: readId(task),
        kanbanId: readText(task, "kanbanId", "KanbanId"),
        columnId: readText(task, "columnId", "ColumnId"),
        userId,
        title: readText(task, "name", "Name") || "Задача",
        description: readText(task, "description", "Description") || "",
        priority: readText(task, "priority", "Priority") || "Medium",
        createdAt,
        deadline,
        order: Number(task?.order ?? task?.Order ?? 0),
        time: formatTimeLeft(deadline),
        tags,
        users: [{
            id: userId,
            name: userId ? `Участник ${userId.slice(0, 8)}` : "Исполнитель",
        }],
    };
}

function createEmptyBoard(project) {
    return {
        id: null,
        projectId: project?.id ?? null,
        title: project?.name ?? "Kanban",
        project,
        selectedKanban: null,
        columns: [],
    };
}

function renderSidebarProjects(projects, activeProjectId) {
    const list = document.querySelector("[data-sidebar-projects]") ?? document.querySelector(".project-list");

    if (!list) {
        return;
    }

    if (!projects.length) {
        list.innerHTML = `<div class="sidebar-empty">Проектов пока нет</div>`;
        return;
    }

    list.innerHTML = projects.map((project) => `
        <button class="project-item ${project.id === activeProjectId ? "active" : ""}" data-project="${escapeHtml(project.id)}">
            <span class="project-color" style="background:${escapeHtml(project.color)}"></span>
            <span class="project-name">${escapeHtml(project.name)}</span>
            <span class="project-meta ${project.id === activeProjectId ? "active-meta" : ""}">${escapeHtml(project.meta)}</span>
        </button>
    `).join("");
}

function renderSidebarTeams(teams) {
    const list = document.querySelector("[data-sidebar-teams]") ?? document.querySelector(".team-grid");

    if (!list) {
        return;
    }

    if (!teams.length) {
        list.innerHTML = `<div class="sidebar-empty">Команд пока нет</div>`;
        return;
    }

    list.innerHTML = teams.map((team) => `
        <button class="team-card" data-team="${escapeHtml(team.id)}">
            <span>${escapeHtml(team.name)}</span>
            <span class="team-dot" style="background:${escapeHtml(team.color)}"></span>
        </button>
    `).join("");
}

function renderSidebarTaskStats(taskStats) {
    const list = document.querySelector("[data-sidebar-tasks]") ?? document.querySelector(".my-tasks-list");

    if (!list) {
        return;
    }

    if (!taskStats) {
        list.innerHTML = `
            <button class="project-item" data-my-tasks-filter="all">
                <span class="project-color orange"></span>
                <span class="project-name">Все задачи</span>
                <span class="project-meta">→</span>
            </button>
        `;
        return;
    }

    list.innerHTML = `
        <button class="project-item" data-my-tasks-filter="all">
            <span class="project-color peach"></span>
            <span class="project-name">Все задачи</span>
            <span class="project-meta">→</span>
        </button>

        <button class="project-item" data-my-tasks-filter="urgent">
            <span class="project-color orange"></span>
            <span class="project-name">Срочные</span>
            <span class="project-meta">${taskStats.urgent}</span>
        </button>

        <button class="project-item" data-my-tasks-filter="week">
            <span class="project-color blue"></span>
            <span class="project-name">На неделе</span>
            <span class="project-meta">${taskStats.week}</span>
        </button>

        <button class="project-item" data-my-tasks-filter="overdue">
            <span class="project-color pink"></span>
            <span class="project-name">Просроченные</span>
            <span class="project-meta">!</span>
        </button>
    `;
}
function readId(record) {
    return readText(record, "id", "Id");
}

function readText(record, ...keys) {
    for (const key of keys) {
        const value = record?.[key];

        if (value !== undefined && value !== null) {
            return String(value);
        }
    }

    return "";
}

function parseDate(value) {
    if (!value) {
        return null;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
}
