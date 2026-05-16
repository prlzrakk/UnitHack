import { fetchJson } from "./apiClient.js";

export async function createColumn(kanbanId, name, order = null) {
    return fetchJson(`/api/kanbans/${kanbanId}/columns`, {
        method: "POST",
        body: { name, order },
        errorText: "Failed to create column",
    });
}

export async function renameColumn(columnId, name) {
    return fetchJson(`/api/columns/${columnId}`, {
        method: "PUT",
        body: { name },
        errorText: "Failed to rename column",
    });
}

export async function deleteColumn(columnId) {
    return fetchJson(`/api/columns/${columnId}`, {
        method: "DELETE",
        errorText: "Failed to delete column",
    });
}
