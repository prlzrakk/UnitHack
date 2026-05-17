import { fetchJson } from "./apiClient.js";

export async function getKanbanTags(kanbanId) {
    return fetchJson(`/api/kanbans/${kanbanId}/tags`, {
        errorText: "Failed to fetch kanban tags",
    });
}

export async function createTag(kanbanId, name) {
    return fetchJson(`/api/kanbans/${kanbanId}/tags`, {
        method: "POST",
        body: { name },
        errorText: "Failed to create tag",
    });
}

export async function renameTag(tagId, name) {
    return fetchJson(`/api/tags/${tagId}`, {
        method: "PUT",
        body: { name },
        errorText: "Failed to rename tag",
    });
}

export async function deleteTag(tagId) {
    return fetchJson(`/api/tags/${tagId}`, {
        method: "DELETE",
        errorText: "Failed to delete tag",
    });
}

export async function getTaskTags(taskId) {
    return fetchJson(`/api/tasks/${taskId}/tags`, {
        errorText: "Failed to fetch task tags",
    });
}

export async function attachTaskTag(taskId, tagId) {
    return fetchJson(`/api/tasks/${taskId}/tags/${tagId}`, {
        method: "POST",
        errorText: "Failed to attach task tag",
    });
}

export async function detachTaskTag(taskId, tagId) {
    return fetchJson(`/api/tasks/${taskId}/tags/${tagId}`, {
        method: "DELETE",
        errorText: "Failed to detach task tag",
    });
}
