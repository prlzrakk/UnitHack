import {
    getNotifications,
    getUnreadNotifications,
    readAllNotifications,
    readNotification,
} from "./api/notificationApi.js";

const state = {
    notifications: [],
    notificationsUnreadOnly: true,
    notificationUnreadCount: 0,
};

let refs = {};

async function loadHeader() {
    const mount = document.getElementById("headerMount");

    if (!mount) {
        return;
    }

    try {
        const response = await fetch("./components/header.html");

        if (!response.ok) {
            throw new Error(`Header request failed: ${response.status}`);
        }

        mount.innerHTML = await response.text();
        initHeader();
        window.dispatchEvent(new CustomEvent("header:loaded"));
    } catch (error) {
        console.error("Failed to load header:", error);
    }
}

function initHeader() {
    refs = {
        menuButton: document.getElementById("openSidebar"),
        notificationOpen: document.getElementById("notificationOpen"),
        notificationOverlay: document.getElementById("notificationOverlay"),
        notificationPanel: document.querySelector("#notificationOverlay .notification-panel"),
        notificationClose: document.getElementById("notificationClose"),
        notificationUnreadOnly: document.getElementById("notificationUnreadOnly"),
        notificationReadAll: document.getElementById("notificationReadAll"),
        notificationList: document.getElementById("notificationList"),
        notificationBadge: document.getElementById("notificationBadge"),
    };

    if (!document.getElementById("sidebarMount") && refs.menuButton) {
        refs.menuButton.hidden = true;
    }

    state.notificationsUnreadOnly = refs.notificationUnreadOnly?.checked ?? true;

    refs.notificationOpen?.addEventListener("click", openNotificationsOverlay);
    refs.notificationClose?.addEventListener("click", closeNotificationsOverlay);
    refs.notificationReadAll?.addEventListener("click", markAllNotificationsAsRead);
    refs.notificationUnreadOnly?.addEventListener("change", handleNotificationFilterChange);
    refs.notificationOverlay?.addEventListener("click", (event) => {
        if (event.target === refs.notificationOverlay) {
            closeNotificationsOverlay();
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeNotificationsOverlay();
        }
    });

    refreshUnreadNotificationsCount().catch((error) => {
        console.warn("Failed to refresh unread notifications:", error);
    });

    if (typeof window.connectNotifications === "function") {
        window.connectNotifications(handleRealtimeNotification);
    }
}

async function openNotificationsOverlay() {
    if (!refs.notificationOverlay) {
        return;
    }

    refs.notificationOverlay.classList.add("is-open");
    refs.notificationOverlay.setAttribute("aria-hidden", "false");

    setTimeout(() => {
        refs.notificationPanel?.focus();
    }, 0);

    await loadNotifications();
}

function closeNotificationsOverlay() {
    if (!refs.notificationOverlay || !isNotificationsOpen()) {
        return;
    }

    refs.notificationOverlay.classList.remove("is-open");
    refs.notificationOverlay.setAttribute("aria-hidden", "true");
    refs.notificationOpen?.focus();
}

function isNotificationsOpen() {
    return refs.notificationOverlay?.classList.contains("is-open") ?? false;
}

async function loadNotifications() {
    if (!refs.notificationList) {
        return;
    }

    renderNotificationsLoading();

    try {
        const notifications = state.notificationsUnreadOnly
            ? await getUnreadNotifications()
            : await getNotifications();

        state.notifications = mergeLocalNotifications(
            normalizeNotifications(notifications),
            getLocalNotifications()
        );
        if (state.notificationsUnreadOnly) {
            state.notifications = state.notifications.filter((notification) => !notification.isRead);
        }
        syncUnreadCountFromLoadedNotifications();
        updateNotificationButton();
        renderNotifications();
    } catch (error) {
        console.error(error);
        renderNotificationsError("Не удалось загрузить уведомления");
    }
}

async function refreshUnreadNotificationsCount() {
    const notifications = normalizeNotifications(await getUnreadNotifications());
    const localNotifications = getLocalNotifications();
    const localUnreadCount = localNotifications.filter((notification) => !notification.isRead).length;

    state.notificationUnreadCount =
        notifications.filter((notification) => !notification.isRead).length + localUnreadCount;
    updateNotificationButton();

    if (isNotificationsOpen() && state.notificationsUnreadOnly) {
        state.notifications = mergeLocalNotifications(notifications, localNotifications)
            .filter((notification) => !notification.isRead);
        renderNotifications();
    }
}

function renderNotificationsLoading() {
    if (!refs.notificationList) {
        return;
    }

    if (refs.notificationReadAll) {
        refs.notificationReadAll.disabled = true;
    }

    refs.notificationList.innerHTML = `<div class="notification-empty">Загрузка уведомлений...</div>`;
}

function renderNotificationsError(message) {
    if (!refs.notificationList) {
        return;
    }

    if (refs.notificationReadAll) {
        refs.notificationReadAll.disabled = state.notificationUnreadCount === 0;
    }

    refs.notificationList.innerHTML = `
        <div class="notification-error">
            ${escapeHtml(message)}
        </div>
    `;
}

function renderNotifications() {
    if (!refs.notificationList) {
        return;
    }

    if (refs.notificationReadAll) {
        refs.notificationReadAll.disabled = state.notificationUnreadCount === 0;
    }

    refs.notificationList.innerHTML = "";

    if (state.notifications.length === 0) {
        const emptyText = state.notificationsUnreadOnly
            ? "Непрочитанных уведомлений нет"
            : "Уведомлений пока нет";

        refs.notificationList.innerHTML = `
            <div class="notification-empty">
                ${escapeHtml(emptyText)}
            </div>
        `;
        return;
    }

    state.notifications.forEach((notification) => {
        const item = document.createElement("button");
        item.className = `notification-item ${notification.isRead ? "is-read" : "is-unread"}`;
        item.type = "button";
        item.dataset.notificationId = notification.id;
        item.setAttribute(
            "aria-label",
            notification.isRead
                ? `Уведомление: ${notification.name}`
                : `Непрочитанное уведомление: ${notification.name}`
        );

        item.innerHTML = `
            <span class="notification-check" aria-hidden="true"></span>
            <span class="notification-content">
                <span class="notification-title-row">
                    <strong class="notification-name">${escapeHtml(notification.name)}</strong>
                    <time class="notification-time" datetime="${escapeAttr(notification.createdAt)}">
                        ${escapeHtml(formatNotificationTime(notification.createdAt))}
                    </time>
                </span>
                <span class="notification-message">${escapeHtml(notification.message)}</span>
            </span>
        `;

        item.addEventListener("click", () => {
            markNotificationAsRead(notification.id);
        });

        refs.notificationList.appendChild(item);
    });
}

async function markNotificationAsRead(notificationId) {
    const notification = state.notifications.find((item) => item.id === notificationId);

    if (!notification || notification.isRead) {
        return;
    }

    if (!notification.isPersisted) {
        state.notifications = state.notifications
            .map((item) => (item.id === notificationId ? { ...item, isRead: true } : item))
            .filter((item) => !state.notificationsUnreadOnly || !item.isRead);

        state.notificationUnreadCount = Math.max(0, state.notificationUnreadCount - 1);
        updateNotificationButton();
        renderNotifications();
        return;
    }

    try {
        const readResponse = await readNotification(notificationId);
        const updatedNotification = readResponse
            ? normalizeNotification(readResponse)
            : notification;
        const nextNotification = {
            ...notification,
            ...updatedNotification,
            id: notification.id,
            isRead: true,
        };

        state.notifications = state.notifications
            .map((item) => (item.id === notificationId ? nextNotification : item))
            .filter((item) => !state.notificationsUnreadOnly || !item.isRead);

        state.notificationUnreadCount = Math.max(0, state.notificationUnreadCount - 1);
        updateNotificationButton();
        renderNotifications();
    } catch (error) {
        console.error("Failed to mark notification as read:", error);
    }
}

async function markAllNotificationsAsRead() {
    if (state.notificationUnreadCount === 0) {
        return;
    }

    try {
        const hasPersistedUnread = state.notifications.some(
            (notification) => notification.isPersisted && !notification.isRead
        );

        if (hasPersistedUnread) {
            await readAllNotifications();
        }

        state.notificationUnreadCount = 0;
        state.notifications = state.notifications
            .map((notification) => ({
                ...notification,
                isRead: true,
            }))
            .filter((notification) => !state.notificationsUnreadOnly || !notification.isRead);

        updateNotificationButton();
        renderNotifications();
    } catch (error) {
        console.error("Failed to mark all notifications as read:", error);
    }
}

function handleRealtimeNotification(notification) {
    const normalizedNotification = normalizeNotification(notification);

    if (!normalizedNotification.id) {
        return;
    }

    const alreadyExists = state.notifications.some(
        (item) => item.id === normalizedNotification.id
    );

    if (!alreadyExists && !normalizedNotification.isRead) {
        state.notificationUnreadCount += 1;
    }

    state.notifications = upsertNotification(
        state.notifications,
        normalizedNotification
    );

    if (isNotificationsOpen()) {
        const shouldShow =
            !state.notificationsUnreadOnly || !normalizedNotification.isRead;

        if (!shouldShow) {
            state.notifications = state.notifications.filter(
                (item) => item.id !== normalizedNotification.id
            );
        }

        renderNotifications();
    }

    updateNotificationButton();
}

function getLocalNotifications() {
    return state.notifications.filter((notification) => !notification.isPersisted);
}

function mergeLocalNotifications(remoteNotifications, localNotifications) {
    const localIds = new Set(localNotifications.map((notification) => notification.id));

    return [
        ...localNotifications,
        ...remoteNotifications.filter((notification) => !localIds.has(notification.id)),
    ];
}

function handleNotificationFilterChange() {
    state.notificationsUnreadOnly = refs.notificationUnreadOnly?.checked ?? false;

    if (isNotificationsOpen()) {
        loadNotifications();
    }
}

function syncUnreadCountFromLoadedNotifications() {
    const unreadCount = state.notifications.filter((notification) => !notification.isRead).length;
    state.notificationUnreadCount = unreadCount;
}

function updateNotificationButton() {
    if (!refs.notificationOpen || !refs.notificationBadge) {
        return;
    }

    const count = Math.max(0, Number(state.notificationUnreadCount) || 0);
    const countText = count > 99 ? "99+" : String(count);

    refs.notificationOpen.classList.toggle("has-unread", count > 0);
    refs.notificationOpen.dataset.unreadCount = countText;
    refs.notificationOpen.setAttribute(
        "aria-label",
        count > 0 ? `Уведомления, непрочитанных: ${countText}` : "Уведомления"
    );
    refs.notificationBadge.textContent = countText;
}

function upsertNotification(notifications, notification) {
    const nextNotifications = notifications.filter((item) => item.id !== notification.id);
    return [notification, ...nextNotifications];
}

function normalizeNotifications(notifications) {
    if (!Array.isArray(notifications)) {
        return [];
    }

    return notifications
        .map(normalizeNotification)
        .filter((notification) => notification.id || notification.name || notification.message);
}

function normalizeNotification(notification = {}) {
    const source = notification && typeof notification === "object" ? notification : {};
    const name = readNotificationValue(source, "name", "Name") || "Уведомление";
    const message =
        readNotificationValue(source, "message", "Message") ||
        "Текст уведомления";

    return {
        id: readNotificationValue(source, "id", "Id") || "",
        userId: readNotificationValue(source, "userId", "UserId") || "",
        teamId: readNotificationValue(source, "teamId", "TeamId") || "",
        projectId: readNotificationValue(source, "projectId", "ProjectId") || "",
        taskId: readNotificationValue(source, "taskId", "TaskId") || "",
        kanbanId: readNotificationValue(source, "kanbanId", "KanbanId") || "",
        type: readNotificationValue(source, "type", "Type", "eventType", "EventType") || "",
        name,
        message,
        isRead: Boolean(readNotificationValue(source, "isRead", "IsRead")),
        isPersisted: getNotificationPersistence(source),
        createdAt:
            readNotificationValue(source, "createdAt", "CreatedAt") ||
            new Date().toISOString(),
        readAt: readNotificationValue(source, "readAt", "ReadAt") || null,
    };
}

function readNotificationValue(source, ...keys) {
    for (const key of keys) {
        if (Object.prototype.hasOwnProperty.call(source, key)) {
            return source[key];
        }
    }

    return null;
}

function getNotificationPersistence(source) {
    const explicitValue = readNotificationValue(source, "isPersisted", "IsPersisted");

    if (explicitValue !== null) {
        return explicitValue === true || explicitValue === "true";
    }

    return Boolean(
        readNotificationValue(source, "taskId", "TaskId") &&
        readNotificationValue(source, "kanbanId", "KanbanId")
    );
}

function formatNotificationTime(value) {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return "";
    }

    const diffMs = Date.now() - date.getTime();
    const minuteMs = 60 * 1000;
    const hourMs = 60 * minuteMs;
    const dayMs = 24 * hourMs;

    if (diffMs < minuteMs) {
        return "только что";
    }

    if (diffMs < hourMs) {
        return `${Math.max(1, Math.floor(diffMs / minuteMs))} мин`;
    }

    if (diffMs < dayMs) {
        return `${Math.floor(diffMs / hourMs)} ч`;
    }

    if (diffMs < 7 * dayMs) {
        return `${Math.floor(diffMs / dayMs)} дн`;
    }

    return date.toLocaleDateString("ru-RU", {
        day: "2-digit",
        month: "short",
    });
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function escapeAttr(value) {
    return escapeHtml(value);
}

window.BoardifyHeader = {
    refreshUnreadNotificationsCount,
};

loadHeader();
