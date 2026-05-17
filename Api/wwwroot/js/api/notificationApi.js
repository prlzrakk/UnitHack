import { fetchJson } from "./apiClient.js";

export async function getNotifications() {
    return fetchJson("/api/notifications", {
        errorText: "Failed to fetch notifications",
    });
}

export async function getUnreadNotifications() {
    return fetchJson("/api/notifications/unread", {
        errorText: "Failed to fetch unread notifications",
    });
}

export async function readNotification(notificationId) {
    return fetchJson(`/api/notifications/${notificationId}/read`, {
        method: "PATCH",
        errorText: "Failed to read notification",
    });
}

export async function readAllNotifications() {
    return fetchJson("/api/notifications/read-all", {
        method: "PUT",
        errorText: "Failed to read all notifications",
    });
}
