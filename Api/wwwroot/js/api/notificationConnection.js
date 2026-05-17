let notificationConnection = null;
let notificationStartRequest = null;
const notificationHandlers = new Set();
const ACCESS_TOKEN_KEYS = [
    "accessToken",
    "AccessToken",
    "boardify.accessToken",
    "jwt",
    "token",
];

window.connectNotifications = function (onNotification) {
    if (typeof onNotification === "function") {
        notificationHandlers.add(onNotification);
    }

    if (notificationConnection) {
        return notificationStartRequest ?? Promise.resolve(notificationConnection);
    }

    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications", {
            accessTokenFactory: function () {
                return getStoredAccessToken() || "";
            }
        })
        .withAutomaticReconnect()
        .build();

    notificationConnection.on("notification.received", function (notification) {
        console.log("Новое уведомление:", notification);

        if (!isNotificationForCurrentUser(notification)) {
            return;
        }

        notificationHandlers.forEach((handler) => {
            try {
                handler(notification);
            } catch (error) {
                console.error("Notification handler failed:", error);
            }
        });
    });

    notificationStartRequest = notificationConnection.start()
        .then(function () {
            console.log("SignalR notifications connected");
            return notificationConnection;
        })
        .catch(function (error) {
            console.error("SignalR connection error:", error);
            notificationConnection = null;
            return null;
        })
        .finally(function () {
            notificationStartRequest = null;
        });

    return notificationStartRequest;
};

window.disconnectNotifications = function () {
    if (notificationConnection) {
        notificationConnection.stop();
        notificationConnection = null;
    }

    notificationHandlers.clear();
};

window.disconnectNotificationHandler = function (onNotification) {
    notificationHandlers.delete(onNotification);
};

function isNotificationForCurrentUser(notification) {
    const notificationUserId = readNotificationValue(notification, "userId", "UserId");

    if (!notificationUserId) {
        return true;
    }

    const currentUserId = getCurrentUserIdFromToken();

    return Boolean(currentUserId) &&
        String(notificationUserId).toLowerCase() === currentUserId.toLowerCase();
}

function getCurrentUserIdFromToken() {
    const token = getStoredAccessToken();

    if (!token) {
        return "";
    }

    try {
        const payload = token.split(".")[1];
        const normalizedPayload = payload.replace(/-/g, "+").replace(/_/g, "/");
        const paddedPayload = normalizedPayload.padEnd(
            normalizedPayload.length + (4 - normalizedPayload.length % 4) % 4,
            "="
        );
        const json = decodeURIComponent(
            atob(paddedPayload)
                .split("")
                .map(function (char) {
                    return "%" + ("00" + char.charCodeAt(0).toString(16)).slice(-2);
                })
                .join("")
        );
        const claims = JSON.parse(json);

        return readNotificationValue(claims, "user_id", "nameid") || "";
    } catch {
        return "";
    }
}

function readNotificationValue(source, camelKey, pascalKey) {
    return source?.[camelKey] ?? source?.[pascalKey] ?? "";
}

function getStoredAccessToken() {
    for (const key of ACCESS_TOKEN_KEYS) {
        const value = localStorage.getItem(key) || sessionStorage.getItem(key);

        if (value) {
            return value;
        }
    }

    return "";
}
