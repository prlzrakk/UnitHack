import { fetchJson } from "./apiClient.js";

export async function getTeams() {
    return fetchJson("/api/teams", {
        errorText: "Failed to fetch teams",
    });
}

export async function getTeam(teamId) {
    return fetchJson(`/api/teams/${teamId}`, {
        errorText: "Failed to fetch team",
    });
}

export async function createTeam(name) {
    return fetchJson("/api/teams", {
        method: "POST",
        body: { name },
        errorText: "Failed to create team",
    });
}

export async function addTeamMember(teamId, userId) {
    return fetchJson(`/api/teams/${teamId}/members`, {
        method: "POST",
        body: { userId },
        errorText: "Failed to add team member",
    });
}

export async function removeTeamMember(teamId, userId) {
    return fetchJson(`/api/teams/${teamId}/members/${userId}`, {
        method: "DELETE",
        errorText: "Failed to remove team member",
    });
}

export async function changeTeamMemberRole(teamId, userId, role) {
    return fetchJson(`/api/teams/${teamId}/members/${userId}/role`, {
        method: "PATCH",
        body: { role },
        errorText: "Failed to change team member role",
    });
}
