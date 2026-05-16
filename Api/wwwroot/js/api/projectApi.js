import { fetchJson } from "./apiClient.js";

export async function createProject(teamId, name) {
    return fetchJson(`/api/teams/${teamId}/projects`, {
        method: "POST",
        body: { name },
        errorText: "Failed to create project",
    });
}

export async function getProject(projectId) {
    return fetchJson(`/api/projects/${projectId}`, {
        errorText: "Failed to fetch project",
    });
}

export async function getTeamProjects(teamId) {
    return fetchJson(`/api/teams/${teamId}/projects`, {
        errorText: "Failed to fetch team projects",
    });
}

export async function deleteProject(projectId) {
    return fetchJson(`/api/projects/${projectId}`, {
        method: "DELETE",
        errorText: "Failed to delete project",
    });
}
