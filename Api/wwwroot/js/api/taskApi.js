import { fetchJson } from "./apiClient.js";

export async function createTask(kanbanId, task) {
    return fetchJson(`/api/kanbans/${kanbanId}/tasks`, {
        method: "POST",
        body: task,
        errorText: "Failed to create task",
    });
}

export async function updateTask(taskId, task) {
    return fetchJson(`/api/tasks/${taskId}`, {
        method: "PUT",
        body: task,
        errorText: "Failed to update task",
    });
}

export async function moveTask(taskId, toColumnId, order) {
    return fetchJson(`/api/tasks/${taskId}`, {
        method: "PATCH",
        body: { toColumnId, order },
        errorText: "Failed to move task",
    });
}

export async function deleteTask(taskId) {
    return fetchJson(`/api/tasks/${taskId}`, {
        method: "DELETE",
        errorText: "Failed to delete task",
    });
}
