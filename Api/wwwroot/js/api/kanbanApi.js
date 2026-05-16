import { fetchJson } from "./apiClient.js";

export async function createKanban(projectId, name) {
    return fetchJson(`/api/projects/${projectId}/kanbans`, {
        method: "POST",
        body: { name },
        errorText: "Failed to create kanban",
    });
}

export async function deleteKanban(kanbanId) {
    return fetchJson(`/api/kanbans/${kanbanId}`, {
        method: "DELETE",
        errorText: "Failed to delete kanban",
    });
}

export async function changeKanban(kanbanId, name) {
    return fetchJson(`/api/kanbans/${kanbanId}`, {
        method: "PUT",
        body: { name },
        errorText: "Failed to change kanban",
    });
}

export async function getProjectKanbans(projectId) {
    return fetchJson(`/api/projects/${projectId}/kanbans`, {
        errorText: "Failed to fetch project kanbans",
    });
}

export async function getKanban(kanbanId) {
    return fetchJson(`/api/kanbans/${kanbanId}`, {
        errorText: "Failed to fetch kanban",
    });
}
