let notificationConnection = null;

window.connectNotifications = function (onNotification) {
    const token = localStorage.getItem("accessToken");

    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications", {
            accessTokenFactory: function () {
                return localStorage.getItem("accessToken") || "";
            }
        })
        .withAutomaticReconnect()
        .build();

    notificationConnection.on("notification.received", function (notification) {
        console.log("Новое уведомление:", notification);

        if (onNotification) {
            onNotification(notification);
        }
    });

    notificationConnection.start()
        .then(function () {
            console.log("SignalR notifications connected");
        })
        .catch(function (error) {
            console.error("SignalR connection error:", error);
        });
};

window.disconnectNotifications = function () {
    if (notificationConnection) {
        notificationConnection.stop();
        notificationConnection = null;
    }
};