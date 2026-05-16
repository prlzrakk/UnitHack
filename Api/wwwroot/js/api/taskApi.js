import { fetchJson } from "./apiClient.js";

export async function createTask(kanbanId, task) {
    return fetchJson(`/api/kanbans/${kanbanId}/tasks`, {
        method: "POST",
        body: buildCreateTaskPayload(task),
        errorText: "Failed to create task",
    });
}

export async function updateTask(taskId, task) {
    return fetchJson(`/api/tasks/${taskId}`, {
        method: "PUT",
        body: buildUpdateTaskPayload(task),
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

function buildCreateTaskPayload(task) {
    return {
        ...task,
        tagIds: Array.isArray(task?.tagIds) ? task.tagIds : [],
    };
}

function buildUpdateTaskPayload(task) {
    const payload = { ...task };

    if (!Object.prototype.hasOwnProperty.call(payload, "tagIds")) {
        return payload;
    }

    payload.tagIds = Array.isArray(payload.tagIds) ? payload.tagIds : null;
    return payload;
}
