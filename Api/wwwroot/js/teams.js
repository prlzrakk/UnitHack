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
let changeTeamMemberRoleRequest = null;
let removeTeamMemberRequest = null;
let addTeamMemberRequest = null;
let searchUsersRequest = null;

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
let memberSearchRequestId = 0;

const TEAM_ROLES = [
    { value: "Admin", label: "Админ" },
    { value: "Member", label: "Участник" },
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
    return sourceTeams.map(normalizeTeam);
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
            : [],
    };
}

function normalizeMember(member, index) {
    const id = readValue(member, "id", "Id") || readValue(member, "userId", "UserId") || `member-${index}`;
    const name =
        readValue(member, "name", "Name") ||
        readValue(member, "email", "Email") ||
        `#${String(id).slice(0, 8)}`;

    return {
        id,
        name,
        role: normalizeTeamRole(readValue(member, "role", "Role")),
        isCurrentUser: isCurrentUserId(id),
    };
}

function pickTeamColor(index) {
    const colors = ["#42609f", "#ef6a35", "#ffe3d8", "#407d52", "#e3a5a8"];
    return colors[index % colors.length];
}

function readValue(source, camelKey, pascalKey) {
    return source?.[camelKey] ?? source?.[pascalKey] ?? null;
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

    panel.querySelectorAll("[data-role-dropdown]").forEach((dropdown) => {
        dropdown.addEventListener("click", (event) => {
            event.stopPropagation();
        });
    });

    panel.querySelectorAll("[data-role-toggle]").forEach((button) => {
        button.addEventListener("click", () => {
            const dropdown = button.closest("[data-role-dropdown]");
            toggleRoleDropdown(dropdown);
        });
    });

    panel.querySelectorAll("[data-role-option]").forEach((button) => {
        button.addEventListener("click", () => {
            const dropdown = button.closest("[data-role-dropdown]");
            closeRoleDropdowns();
            changeMemberRole(team.id, dropdown.dataset.memberRole, button.dataset.roleOption);
        });
    });

    panel.addEventListener("click", (event) => {
        if (event.target.closest("button, [data-role-dropdown]")) {
            return;
        }

        selectTeam(team.id);
    });

    return panel;
}

function memberRow(member, teamId) {
    const role = normalizeTeamRole(member.role);
    const isCurrentUser = isCurrentUserMember(member);

    return `
        <div class="team-member-row">
            <div class="team-member-name">${escapeHtml(member.name)}</div>
            <div
                class="team-role-dropdown"
                data-role-dropdown
                data-member-role="${escapeAttr(member.id)}"
            >
                <button
                    class="team-role-toggle"
                    type="button"
                    data-role-toggle
                    aria-expanded="false"
                    aria-label="Роль участника ${escapeAttr(member.name)}"
                >
                    <span>${escapeHtml(getTeamRoleLabel(role))}</span>
                    <span class="team-role-chevron"></span>
                </button>
                <div class="team-role-menu" role="menu">
                    ${renderRoleOptions(role)}
                </div>
            </div>
            <button
                class="member-delete-btn"
                type="button"
                data-delete-member="${escapeAttr(member.id)}"
                aria-label="${isCurrentUser ? "Выйти из команды" : "Удалить участника"}"
            >
                ×
            </button>
        </div>
    `;
}

function selectTeam(teamId) {
    selectedTeamId = teamId;
    syncSelectedTeamUrl();

    markSelectedTeam();
}

function syncSelectedTeamUrl() {
    if (!selectedTeamId) {
        const url = new URL(window.location.href);
        url.searchParams.delete("team");
        window.history.pushState({}, "", url);
        return;
    }

    const url = new URL(window.location.href);
    url.searchParams.set("team", selectedTeamId);
    window.history.pushState({}, "", url);
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

        const previousTeam = {
            color: team.color,
            members: [...team.members],
            name: team.name,
        };

        try {
            team.name = name;
            team.color = newTeamColor.value || "#f4864d";
            team.members = await persistEditedTeamMembers(team, draftMembers);
        } catch (error) {
            console.error(error);
            team.name = previousTeam.name;
            team.color = previousTeam.color;
            team.members = previousTeam.members;
            renderTeams();
            showToast(getRequestErrorMessage(error, "Не удалось сохранить участников"));
            return;
        }

        closeCreateTeamOverlay();
        renderTeams();
        renderSidebar();
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
        const createdTeamId = readValue(createdTeam, "id", "Id");
        const persistedMembers = [getCurrentUserMember()];

        for (const member of draftMembers.filter((item) => !item.isCurrentUser)) {
            if (!addTeamMemberRequest || !isGuid(createdTeamId) || !isGuid(member.id)) {
                continue;
            }

            const addedMember = await addTeamMemberRequest(createdTeamId, member.id);
            persistedMembers.push({
                ...member,
                role: normalizeTeamRole(readValue(addedMember, "role", "Role") || "Member"),
            });
        }

        const newTeam = normalizeTeam(
            {
                ...createdTeam,
                color: newTeamColor.value || "#f4864d",
                members: persistedMembers,
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

async function persistEditedTeamMembers(team, nextMembers) {
    if (!isGuid(team.id)) {
        throw new Error("Нельзя сохранить участников локальной команды");
    }

    const previousMembers = team.members;
    const previousMemberIds = new Set(previousMembers.map((member) => String(member.id)));
    const nextMemberIds = new Set(nextMembers.map((member) => String(member.id)));
    const addedMembers = nextMembers.filter((member) => !previousMemberIds.has(String(member.id)));
    const removedMembers = previousMembers.filter((member) => !nextMemberIds.has(String(member.id)));

    if (addedMembers.some((member) => !isGuid(member.id))) {
        throw new Error("Добавление доступно только для пользователей из API");
    }

    if (removedMembers.some((member) => !isGuid(member.id))) {
        throw new Error("Удаление доступно только для пользователей из API");
    }

    if ((addedMembers.length && !addTeamMemberRequest) || (removedMembers.length && !removeTeamMemberRequest)) {
        throw new Error("API участников команды еще загружается");
    }

    const persistedMembers = [...nextMembers];

    for (const member of addedMembers) {
        const addedMember = await addTeamMemberRequest(team.id, member.id);
        const persistedMember = persistedMembers.find((item) => item.id === member.id);

        if (persistedMember) {
            persistedMember.role = normalizeTeamRole(readValue(addedMember, "role", "Role") || persistedMember.role);
        }
    }

    for (const member of removedMembers) {
        await removeTeamMemberRequest(team.id, member.id);
    }

    return persistedMembers;
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

async function deleteMember(teamId, memberId) {
    const team = getTeam(teamId);
    const member = team?.members.find((item) => item.id === memberId);

    if (!team || !member) {
        return;
    }

    if (isCurrentUserMember(member)) {
        await leaveTeam(team, member);
        return;
    }

    if (!canRemoveMember(team, member)) {
        alert("Нельзя удалить единственного админа команды");
        return;
    }

    await removeMemberFromTeam(team, member, {
        onSuccess: () => {
            showToast("Участник удалён");
        },
        errorText: "Не удалось удалить участника",
    });
}

async function leaveTeam(team, member) {
    if (!canRemoveMember(team, member)) {
        alert("Перед выходом назначьте другого участника админом");
        return;
    }

    const confirmed = confirm(`Вы уверены, что хотите выйти из команды «${team.name}»?`);

    if (!confirmed) {
        return;
    }

    await removeMemberFromTeam(team, member, {
        onSuccess: () => {
            teams = teams.filter((item) => item.id !== team.id);
            workspaceState = {
                ...workspaceState,
                teams: workspaceState.teams.filter((item) => item.id !== team.id),
            };

            if (selectedTeamId === team.id) {
                selectedTeamId = teams[0]?.id ?? null;
                syncSelectedTeamUrl();
            }

            renderTeams();
            renderSidebar();
            showToast(`Вы вышли из команды «${team.name}»`);
        },
        errorText: "Не удалось выйти из команды",
    });
}

async function removeMemberFromTeam(team, member, { onSuccess, errorText = "Не удалось удалить участника" }) {
    const previousMembers = [...team.members];

    team.members = team.members.filter((item) => item.id !== member.id);
    renderTeams();

    if (!removeTeamMemberRequest || !isGuid(team.id) || !isGuid(member.id)) {
        team.members = previousMembers;
        renderTeams();
        showToast(errorText);
        return;
    }

    try {
        await removeTeamMemberRequest(team.id, member.id);
        onSuccess();
    } catch (error) {
        console.error(error);
        team.members = previousMembers;
        renderTeams();
        showToast(getRequestErrorMessage(error, errorText));
    }
}

function canRemoveMember(team, member) {
    if (normalizeTeamRole(member.role) !== "Admin") {
        return true;
    }

    return team.members.some((item) => {
        return item.id !== member.id && normalizeTeamRole(item.role) === "Admin";
    });
}

function isCurrentUserMember(member) {
    return member.isCurrentUser || isCurrentUserId(member.id);
}

async function changeMemberRole(teamId, memberId, role) {
    const team = getTeam(teamId);
    const member = team?.members.find((item) => item.id === memberId);

    if (!team || !member) {
        return;
    }

    const nextRole = normalizeTeamRole(role);
    const previousRole = normalizeTeamRole(member.role);

    member.role = nextRole;
    renderTeams();

    if (!changeTeamMemberRoleRequest || !isGuid(teamId) || !isGuid(memberId)) {
        member.role = previousRole;
        renderTeams();
        showToast("Не удалось изменить роль через API");
        return;
    }

    try {
        const updatedMember = await changeTeamMemberRoleRequest(teamId, memberId, nextRole);
        member.role = normalizeTeamRole(readValue(updatedMember, "role", "Role") || nextRole);
        renderTeams();
        showToast("Роль обновлена");
    } catch (error) {
        console.error(error);
        member.role = previousRole;
        renderTeams();
        showToast(getRequestErrorMessage(error, "Не удалось изменить роль"));
    }
}

function renderRoleOptions(selectedRole) {
    return TEAM_ROLES.map((role) => `
        <button
            class="team-role-option ${role.value === selectedRole ? "is-selected" : ""}"
            type="button"
            data-role-option="${role.value}"
            role="menuitemradio"
            aria-checked="${role.value === selectedRole}"
        >
            ${role.label}
        </button>
    `).join("");
}

function toggleRoleDropdown(dropdown) {
    if (!dropdown) {
        return;
    }

    const shouldOpen = !dropdown.classList.contains("is-open");

    closeRoleDropdowns(dropdown);
    dropdown.classList.toggle("is-open", shouldOpen);
    dropdown.querySelector("[data-role-toggle]")?.setAttribute("aria-expanded", String(shouldOpen));
}

function closeRoleDropdowns(exceptDropdown = null) {
    document.querySelectorAll("[data-role-dropdown].is-open").forEach((dropdown) => {
        if (dropdown === exceptDropdown) {
            return;
        }

        dropdown.classList.remove("is-open");
        dropdown.querySelector("[data-role-toggle]")?.setAttribute("aria-expanded", "false");
    });
}

function normalizeTeamRole(role) {
    const normalized = String(role ?? "")
        .trim()
        .toLowerCase();

    if (normalized === "admin" || normalized === "админ" || normalized === "администратор" || normalized === "0") {
        return "Admin";
    }

    return "Member";
}

function getTeamRoleLabel(role) {
    const normalizedRole = normalizeTeamRole(role);
    return TEAM_ROLES.find((item) => item.value === normalizedRole)?.label ?? "Участник";
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

async function renderMemberSearchResults(searchValue) {
    const query = searchValue.trim();
    const requestId = ++memberSearchRequestId;

    memberSearchResults.innerHTML = "";

    if (query.length < 2) {
        memberSearchResults.innerHTML = `
            <div class="member-result">
                <span>Введите минимум 2 символа</span>
                <span class="member-result-role">—</span>
            </div>
        `;
        return;
    }

    memberSearchResults.innerHTML = `
        <div class="member-result">
            <span>Ищем...</span>
            <span class="member-result-role">—</span>
        </div>
    `;

    let people = [];

    try {
        await ensureUserSearchRequest();
        people = await searchUsersRequest(query);
    } catch (error) {
        console.error(error);

        if (requestId !== memberSearchRequestId) {
            return;
        }

        memberSearchResults.innerHTML = `
            <div class="member-result">
                <span>Не удалось найти пользователей</span>
                <span class="member-result-role">—</span>
            </div>
        `;
        return;
    }

    if (requestId !== memberSearchRequestId) {
        return;
    }

    memberSearchResults.innerHTML = "";

    if (!people.length) {
        memberSearchResults.innerHTML = `
            <div class="member-result">
                <span>Ничего не найдено</span>
                <span class="member-result-role">—</span>
            </div>
        `;
        return;
    }

    people.map(normalizeUserSearchResult).forEach((person) => {
        const button = document.createElement("button");
        button.className = `member-result ${
            selectedMember?.id === person.id ? "is-selected" : ""
        }`;
        button.type = "button";

        button.innerHTML = `
            <span>${escapeHtml(person.name)}</span>
            <span class="member-result-role">${escapeHtml(person.email)}</span>
        `;

        button.addEventListener("click", () => {
            selectedMember = person;
            updateAddMemberSubmitState();
            renderMemberSearchResults(memberSearchInput.value);
        });

        memberSearchResults.appendChild(button);
    });
}

async function ensureUserSearchRequest() {
    if (searchUsersRequest) {
        return;
    }

    const userApi = await import(`./api/userApi.js?v=${Date.now()}`);
    searchUsersRequest = userApi.searchUsers;

    if (!searchUsersRequest) {
        throw new Error("User search API is not available");
    }
}

function normalizeUserSearchResult(user) {
    const id = readValue(user, "id", "Id");

    return {
        id,
        name:
            readValue(user, "name", "Name") ||
            readValue(user, "email", "Email") ||
            `#${String(id).slice(0, 8)}`,
        email: readValue(user, "email", "Email") || "",
    };
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
async function submitAddMember() {
    if (!selectedMember) {
        showToast("Выбери участника");
        return;
    }

    const member = {
        id: selectedMember.id,
        name: selectedMember.name,
        role: "Member",
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

    if (team.members.some((item) => item.id === member.id)) {
        showToast("Пользователь уже в команде");
        return;
    }

    if (!addTeamMemberRequest || !isGuid(team.id) || !isGuid(member.id)) {
        showToast("Добавление доступно только для пользователей из API");
        return;
    }

    try {
        const addedMember = await addTeamMemberRequest(team.id, member.id);
        team.members.push({
            ...member,
            role: normalizeTeamRole(readValue(addedMember, "role", "Role") || "Member"),
        });
    } catch (error) {
        console.error(error);
        showToast(getRequestErrorMessage(error, "Не удалось добавить участника"));
        return;
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
            <span>${escapeHtml(getTeamRoleLabel(member.role))}</span>
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
    const id = getCurrentUserId() || "current-user";
    const name =
        readValue(currentUser, "name", "Name") ||
        readValue(currentUser, "userName", "UserName") ||
        readValue(currentUser, "email", "Email") ||
        "Вы";

    return {
        id,
        name,
        role: "Admin",
        isCurrentUser: true,
    };
}

function getCurrentUserId() {
    return readValue(currentUser, "id", "Id");
}

function isCurrentUserId(userId) {
    const currentUserId = getCurrentUserId();

    return Boolean(currentUserId && String(userId) === String(currentUserId));
}

function isGuid(value) {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(String(value));
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

function getRequestErrorMessage(error, fallbackText) {
    return error?.message || fallbackText;
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
    addTeamMemberRequest = teamApi.addTeamMember;
    changeTeamMemberRoleRequest = teamApi.changeTeamMemberRole;
    removeTeamMemberRequest = teamApi.removeTeamMember;
    loadCurrentUserRequest = userApi.getMe;
    searchUsersRequest = userApi.searchUsers;
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
        teams = [];
        selectedTeamId = null;
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

document.addEventListener("click", () => {
    closeRoleDropdowns();
});

document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") {
        return;
    }

    closeRoleDropdowns();

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
