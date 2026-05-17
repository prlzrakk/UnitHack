const teamsGrid = document.getElementById("teamsGrid");

const openCreateTeamBtn = document.getElementById("openCreateTeam");

const createTeamOverlay = document.getElementById("createTeamOverlay");
const createTeamForm = document.getElementById("createTeamForm");
const createTeamClose = document.getElementById("createTeamClose");
const createTeamCancel = document.getElementById("createTeamCancel");
const newTeamName = document.getElementById("newTeamName");
const newTeamColor = document.getElementById("newTeamColor");
const addDraftMemberBtn = document.getElementById("addDraftMemberBtn");
const draftMembersBox = document.getElementById("draftMembersBox");

const addMemberOverlay = document.getElementById("addMemberOverlay");
const addMemberForm = document.getElementById("addMemberForm");
const addMemberTitle = document.getElementById("addMemberTitle");
const addMemberClose = document.getElementById("addMemberClose");
const addMemberCancel = document.getElementById("addMemberCancel");
const addMemberSubmit = document.getElementById("addMemberSubmit");
const memberSearchInput = document.getElementById("memberSearchInput");
const memberSearchResults = document.getElementById("memberSearchResults");

const createTeamTitle = document.getElementById("createTeamTitle");
const createTeamSubmit = document.getElementById("createTeamSubmit");

let createTeamRequest = null;
let loadCurrentUserRequest = null;
let loadWorkspaceRequest = null;
let renderSidebarNavigationRequest = null;
let selectProjectFromUrlRequest = null;

let activeProjectId = getProjectFromUrl();
let workspaceState = { teams: [], projects: [] };
let currentUser = null;
let teams = [];
let selectedTeamId = getTeamFromUrl();
let draftMembers = [];
let teamFormMode = "create";
let editingTeamId = null;
let memberOverlayMode = "team";
let targetTeamId = null;
let selectedMember = null;

const PEOPLE = [
    {
        id: "arina",
        name: "Кискина Арина",
        role: "Фронтенд",
        email: "arina@example.com",
    },
    {
        id: "katya",
        name: "Катя Смирнова",
        role: "Дизайн",
        email: "katya@example.com",
    },
    {
        id: "max",
        name: "Макс Иванов",
        role: "Бэкенд",
        email: "max@example.com",
    },
    {
        id: "danya",
        name: "Даня Орлов",
        role: "QA",
        email: "danya@example.com",
    },
    {
        id: "anya",
        name: "Аня Котова",
        role: "Менеджер",
        email: "anya@example.com",
    },
];

function getProjectFromUrl() {
    const params = new URLSearchParams(window.location.search);
    return params.get("project");
}

function getTeamFromUrl() {
    const params = new URLSearchParams(window.location.search);
    return params.get("team");
}

function getInitialTeams(sourceTeams = []) {
    const baseTeams = sourceTeams.length
        ? sourceTeams
        : [
            { name: "5 кать", color: "#42609f" },
            { name: "абоба", color: "#ef6a35" },
            { name: "какуля", color: "#ffe3d8" },
            { name: "пупуня", color: "#e3a5a8" },
        ];

    return baseTeams.map(normalizeTeam);
}

function normalizeTeam(team, index) {
    const id = readValue(team, "id", "Id") || slugify(readValue(team, "name", "Name")) || `team-${index}`;
    const members = readValue(team, "members", "Members");

    return {
        id,
        name: readValue(team, "name", "Name") || "Команда",
        color: readValue(team, "color", "Color") || pickTeamColor(index),
        members: Array.isArray(members)
            ? members.map(normalizeMember)
            : getDemoMembers(index),
    };
}

function normalizeMember(member, index) {
    const id = readValue(member, "id", "Id") || readValue(member, "userId", "UserId") || `member-${index}`;
    const name = readValue(member, "name", "Name") || `#${String(id).slice(0, 8)}`;

    return {
        id,
        name,
        role: readValue(member, "role", "Role") || "Участник",
    };
}

function pickTeamColor(index) {
    const colors = ["#42609f", "#ef6a35", "#ffe3d8", "#407d52", "#e3a5a8"];
    return colors[index % colors.length];
}

function readValue(source, camelKey, pascalKey) {
    return source?.[camelKey] ?? source?.[pascalKey] ?? null;
}

function getDemoMembers(index) {
    if (index === 0) {
        return [
            { id: "arina", name: "Кискина Арина", role: "Фронтенд" },
            { id: "katya", name: "Катя Смирнова", role: "Дизайн" },
        ];
    }

    return [
        { id: `demo-${index}-1`, name: "чччччччч", role: "Фронтенд" },
        { id: `demo-${index}-2`, name: "чччччччч", role: "Бэкенд" },
    ];
}

function renderTeams() {
    teamsGrid.innerHTML = "";

    teams.forEach((team) => {
        teamsGrid.appendChild(createTeamCard(team));
    });

    markSelectedTeam();
}

function createTeamCard(team) {
    const panel = document.createElement("article");
    panel.className = "team-panel";
    panel.dataset.teamId = team.id;

    panel.innerHTML = `
        <div class="team-panel-head">
            <span class="team-color-dot" style="background:${team.color}"></span>

            <h2 class="team-name">${escapeHtml(team.name)}</h2>

            <button class="team-icon-btn team-delete-btn" type="button" aria-label="Удалить команду">
                🗑
            </button>

            <button class="team-icon-btn team-edit-btn" type="button" aria-label="Редактировать команду">
                ✎
            </button>
        </div>

        <div class="team-members">
            ${
        team.members.length
            ? team.members.map((member) => memberRow(member, team.id)).join("")
            : `<div class="team-empty">участников пока нет</div>`
    }
        </div>
    `;

    panel.querySelector(".team-delete-btn").addEventListener("click", () => {
        deleteTeam(team.id);
    });

    panel.querySelector(".team-edit-btn").addEventListener("click", () => {
        editTeam(team.id);
    });

    panel.querySelectorAll("[data-delete-member]").forEach((button) => {
        button.addEventListener("click", () => {
            deleteMember(team.id, button.dataset.deleteMember);
        });
    });

    panel.addEventListener("click", (event) => {
        if (event.target.closest("button")) {
            return;
        }

        selectTeam(team.id);
    });

    return panel;
}

function memberRow(member, teamId) {
    return `
        <div class="team-member-row">
            <div class="team-member-name">${escapeHtml(member.name)}</div>
            <div class="team-member-role">${escapeHtml(member.role || "Участник")}</div>
            <button
                class="member-delete-btn"
                type="button"
                data-delete-member="${escapeAttr(member.id)}"
                aria-label="Удалить участника"
            >
                ×
            </button>
        </div>
    `;
}

function selectTeam(teamId) {
    selectedTeamId = teamId;

    const url = new URL(window.location.href);
    url.searchParams.set("team", teamId);
    window.history.pushState({}, "", url);

    markSelectedTeam();
}

function markSelectedTeam() {
    document.querySelectorAll(".team-panel").forEach((panel) => {
        panel.classList.toggle("is-selected", panel.dataset.teamId === selectedTeamId);
    });
}

function openCreateTeamOverlay() {
    teamFormMode = "create";
    editingTeamId = null;
    draftMembers = [getCurrentUserMember()];

    createTeamForm.reset();
    newTeamColor.value = "#42609f";

    createTeamTitle.textContent = "Создание команды";
    createTeamSubmit.textContent = "Сохранить изменения";
    updateCreateTeamSubmitState();

    renderDraftMembers();

    openOverlay(createTeamOverlay);

    setTimeout(() => {
        newTeamName.focus();
    }, 0);
}
function closeCreateTeamOverlay() {
    closeOverlay(createTeamOverlay);

    teamFormMode = "create";
    editingTeamId = null;
    draftMembers = [];
    updateCreateTeamSubmitState();
}
async function createTeamFromForm() {
    const name = newTeamName.value.trim();

    if (!name) {
        newTeamName.focus();
        return;
    }

    if (teamFormMode === "edit") {
        const team = getTeam(editingTeamId);

        if (!team) {
            return;
        }

        team.name = name;
        team.color = newTeamColor.value || "#f4864d";
        team.members = [...draftMembers];

        closeCreateTeamOverlay();
        renderTeams();
        selectTeam(team.id);

        showToast(`Команда «${name}» обновлена`);
        return;
    }

    if (!createTeamRequest) {
        showToast("API команд еще загружается");
        return;
    }

    try {
        const createdTeam = await createTeamRequest(name);
        const newTeam = normalizeTeam(
            {
                ...createdTeam,
                color: newTeamColor.value || "#f4864d",
                members: [...draftMembers],
            },
            teams.length
        );

        teams.push(newTeam);

        closeCreateTeamOverlay();
        renderTeams();
        renderSidebar();
        selectTeam(newTeam.id);

        showToast(`Команда «${name}» создана`);
    } catch (error) {
        console.error(error);
        showToast("Не удалось создать команду");
    }
}

function editTeam(teamId) {
    const team = getTeam(teamId);

    if (!team) {
        return;
    }

    teamFormMode = "edit";
    editingTeamId = teamId;

    newTeamName.value = team.name;
    newTeamColor.value = team.color || "#f4864d";
    draftMembers = [...team.members];

    createTeamTitle.textContent = `Изменение команды`;
    createTeamSubmit.textContent = "Сохранить изменения";
    updateCreateTeamSubmitState();

    renderDraftMembers();

    openOverlay(createTeamOverlay);

    setTimeout(() => {
        newTeamName.focus();
        newTeamName.select();
    }, 0);
}

function deleteTeam(teamId) {
    const team = getTeam(teamId);

    if (!team) {
        return;
    }

    const confirmed = confirm(`Удалить команду «${team.name}»?`);

    if (!confirmed) {
        return;
    }

    teams = teams.filter((item) => item.id !== teamId);

    if (selectedTeamId === teamId) {
        selectedTeamId = teams[0]?.id ?? null;
    }

    renderTeams();
    showToast(`Команда «${team.name}» удалена`);
}

function deleteMember(teamId, memberId) {
    const team = getTeam(teamId);

    if (!team) {
        return;
    }

    team.members = team.members.filter((member) => member.id !== memberId);

    renderTeams();
    showToast("Участник удалён");
}

function openAddMemberOverlay({ mode, teamId = null }) {
    memberOverlayMode = mode;
    targetTeamId = teamId;
    selectedMember = null;
    memberSearchInput.value = "";
    updateAddMemberSubmitState();

    const team = getTeam(teamId);

    addMemberTitle.textContent =
        mode === "draft"
            ? "Добавить участника в команду"
            : `Добавить участника в команду ${team?.name || ""}`;

    renderMemberSearchResults("");

    openOverlay(addMemberOverlay);

    setTimeout(() => {
        memberSearchInput.focus();
    }, 0);
}

function closeAddMemberOverlay() {
    closeOverlay(addMemberOverlay);

    selectedMember = null;
    targetTeamId = null;
    updateAddMemberSubmitState();
}

function renderMemberSearchResults(searchValue) {
    const query = searchValue.trim().toLowerCase();

    const people = PEOPLE.filter((person) => {
        return (
            person.name.toLowerCase().includes(query) ||
            person.email.toLowerCase().includes(query) ||
            person.role.toLowerCase().includes(query)
        );
    });

    memberSearchResults.innerHTML = "";

    if (people.length === 0) {
        memberSearchResults.innerHTML = `
            <div class="member-result">
                <span>Ничего не найдено</span>
                <span class="member-result-role">—</span>
            </div>
        `;
        return;
    }

    people.forEach((person) => {
        const button = document.createElement("button");
        button.className = `member-result ${
            selectedMember?.id === person.id ? "is-selected" : ""
        }`;
        button.type = "button";

        button.innerHTML = `
            <span>${escapeHtml(person.name)}</span>
            <span class="member-result-role">${escapeHtml(person.role)}</span>
        `;

        button.addEventListener("click", () => {
            selectedMember = person;
            updateAddMemberSubmitState();
            renderMemberSearchResults(memberSearchInput.value);
        });

        memberSearchResults.appendChild(button);
    });
}
function openOverlay(overlay) {
    overlay.classList.add("is-open");
    document.body.classList.add("modal-locked");
}

function closeOverlay(overlay) {
    overlay.classList.remove("is-open");

    const hasOpenedOverlay = document.querySelector(".team-modal-overlay.is-open");

    if (!hasOpenedOverlay) {
        document.body.classList.remove("modal-locked");
    }
}
function submitAddMember() {
    if (!selectedMember) {
        showToast("Выбери участника");
        return;
    }

    const member = {
        id: selectedMember.id,
        name: selectedMember.name,
        role: selectedMember.role,
    };

    if (memberOverlayMode === "draft") {
        if (!draftMembers.some((item) => item.id === member.id)) {
            draftMembers.push(member);
        }

        renderDraftMembers();
        closeAddMemberOverlay();
        return;
    }

    const team = getTeam(targetTeamId);

    if (!team) {
        return;
    }

    if (!team.members.some((item) => item.id === member.id)) {
        team.members.push(member);
    }

    closeAddMemberOverlay();
    renderTeams();
    showToast(`Участник добавлен в «${team.name}»`);
}

function updateAddMemberSubmitState() {
    const isReady = Boolean(selectedMember);

    addMemberSubmit.classList.toggle("is-waiting-selection", !isReady);
    addMemberSubmit.setAttribute("aria-disabled", String(!isReady));
}

function updateCreateTeamSubmitState() {
    const hasName = Boolean(newTeamName.value.trim());

    createTeamSubmit.disabled = !hasName;
    createTeamSubmit.classList.toggle("is-waiting-selection", !hasName);
    createTeamSubmit.setAttribute("aria-disabled", String(!hasName));
}

function renderDraftMembers() {
    draftMembersBox.innerHTML = "";

    if (draftMembers.length === 0) {
        draftMembersBox.innerHTML = `<span class="empty-team-text">участников пока нет</span>`;
        return;
    }

    draftMembers.forEach((member) => {
        const chip = document.createElement("span");
        chip.className = `draft-member-chip ${member.isCurrentUser ? "is-current-user" : ""}`;

        chip.innerHTML = `
            <span>${escapeHtml(member.name)}</span>
            <span>${escapeHtml(member.role)}</span>
            ${
                member.isCurrentUser
                    ? ""
                    : `<button type="button" aria-label="Убрать участника">×</button>`
            }
        `;

        chip.querySelector("button")?.addEventListener("click", () => {
            draftMembers = draftMembers.filter((item) => item.id !== member.id);
            renderDraftMembers();
        });

        draftMembersBox.appendChild(chip);
    });
}

function getCurrentUserMember() {
    const id = readValue(currentUser, "id", "Id") || "current-user";
    const name =
        readValue(currentUser, "name", "Name") ||
        readValue(currentUser, "userName", "UserName") ||
        readValue(currentUser, "email", "Email") ||
        "Вы";

    return {
        id,
        name,
        role: "Администратор",
        isCurrentUser: true,
    };
}

function getTeam(teamId) {
    return teams.find((team) => team.id === teamId);
}

function uniqueTeamId(name) {
    const base = slugify(name) || "team";
    let id = base;
    let counter = 2;

    while (teams.some((team) => team.id === id)) {
        id = `${base}-${counter}`;
        counter += 1;
    }

    return id;
}

function slugify(value) {
    return String(value)
        .trim()
        .toLowerCase()
        .replaceAll("ё", "e")
        .replaceAll(" ", "-")
        .replace(/[^a-zа-я0-9-]/g, "")
        .replace(/-+/g, "-");
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
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

function renderSidebar() {
    if (!renderSidebarNavigationRequest) {
        return;
    }

    if (!workspaceState.teams.length && !workspaceState.projects.length) {
        return;
    }

    renderSidebarNavigationRequest(workspaceState, activeProjectId, null);
}

async function loadApiModules() {
    const [teamApi, userApi, boardData] = await Promise.all([
        import("./api/teamApi.js"),
        import("./api/userApi.js"),
        import("./boardData.js"),
    ]);

    createTeamRequest = teamApi.createTeam;
    loadCurrentUserRequest = userApi.getMe;
    loadWorkspaceRequest = boardData.loadWorkspace;
    renderSidebarNavigationRequest = boardData.renderSidebarNavigation;
    selectProjectFromUrlRequest = boardData.selectProjectFromUrl;
}

async function init() {
    renderTeams();

    try {
        await loadApiModules();

        const [workspace, loadedUser] = await Promise.all([
            loadWorkspaceRequest(),
            loadCurrentUserRequest().catch(() => null),
        ]);
        const activeProject = selectProjectFromUrlRequest(workspace.projects);

        currentUser = loadedUser;
        workspaceState = workspace;
        activeProjectId = activeProject?.id ?? activeProjectId;
        teams = getInitialTeams(workspace.teams);

        if (!selectedTeamId || !teams.some((team) => team.id === selectedTeamId)) {
            selectedTeamId = teams[0]?.id ?? null;
        }

        renderTeams();
        renderSidebar();
    } catch (error) {
        console.error(error);
        teams = getInitialTeams();

        if (!selectedTeamId && teams[0]) {
            selectedTeamId = teams[0].id;
        }

        renderTeams();
        showToast("Не удалось загрузить команды из API");
    }
}

/* EVENTS */

openCreateTeamBtn.addEventListener("click", openCreateTeamOverlay);

newTeamName.addEventListener("input", updateCreateTeamSubmitState);

createTeamClose.addEventListener("click", closeCreateTeamOverlay);
createTeamCancel.addEventListener("click", closeCreateTeamOverlay);

createTeamOverlay.addEventListener("click", (event) => {
    if (event.target === createTeamOverlay) {
        closeCreateTeamOverlay();
    }
});

createTeamForm.addEventListener("submit", (event) => {
    event.preventDefault();
    createTeamFromForm();
});

addDraftMemberBtn.addEventListener("click", () => {
    openAddMemberOverlay({
        mode: "draft",
    });
});

addMemberClose.addEventListener("click", closeAddMemberOverlay);
addMemberCancel.addEventListener("click", closeAddMemberOverlay);

addMemberOverlay.addEventListener("click", (event) => {
    if (event.target === addMemberOverlay) {
        closeAddMemberOverlay();
    }
});

addMemberForm.addEventListener("submit", (event) => {
    event.preventDefault();
    submitAddMember();
});

memberSearchInput.addEventListener("input", () => {
    renderMemberSearchResults(memberSearchInput.value);
});

document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") {
        return;
    }

    if (addMemberOverlay.classList.contains("is-open")) {
        closeAddMemberOverlay();
        return;
    }

    if (createTeamOverlay.classList.contains("is-open")) {
        closeCreateTeamOverlay();
    }
});

window.addEventListener("team:selected", (event) => {
    selectTeam(event.detail.teamId);
});

window.addEventListener("sidebar:loaded", renderSidebar);

init();
