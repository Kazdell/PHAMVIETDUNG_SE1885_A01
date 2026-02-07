"use strict";

var notificationConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/notifications")
    .withAutomaticReconnect()
    .build();

// Helper to show notification
function showNotification(title, message, icon = "bi-info-circle", colorClass = "text-primary") {
    // 1. Show Toast
    var toastHtml = `
        <div class="toast show animate__animated animate__fadeInRight" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header">
                <strong class="me-auto"><i class="bi ${icon} me-1 ${colorClass}"></i> ${title}</strong>
                <small class="text-muted">Just now</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    var toastContainer = document.getElementById("toast-container");
    if (toastContainer) {
        var div = document.createElement("div");
        div.innerHTML = toastHtml;
        toastContainer.appendChild(div.firstElementChild);

        var currentToasts = toastContainer.querySelectorAll(".toast.show");
        if (currentToasts.length > 10) currentToasts[0].remove();
    }

    // 2. Update Notification Center Dropdown
    var list = document.getElementById("notificationList");
    var badge = document.getElementById("notificationBadge");
    var noNotif = document.getElementById("no-notifications");

    if (list) {
        if (noNotif) noNotif.remove();

        var itemHtml = `
            <a href="#" class="list-group-item list-group-item-action border-0 border-bottom p-3 small">
                <div class="d-flex w-100 justify-content-between mb-1">
                    <h6 class="mb-0 fw-bold ${colorClass}">${title}</h6>
                    <small class="text-muted">Just now</small>
                </div>
                <p class="mb-0 text-dark">${message}</p>
            </a>
        `;

        var tempDiv = document.createElement('div');
        tempDiv.innerHTML = itemHtml;
        list.insertBefore(tempDiv.firstElementChild, list.firstChild);

        var items = list.querySelectorAll(".list-group-item");
        if (items.length > 10) items[items.length - 1].remove();

        // 3. Update Badge
        if (badge) {
            var count = parseInt(badge.innerText) || 0;
            count++;
            badge.innerText = count;
            badge.style.display = "block";
        }
    }
}

notificationConnection.on("ReceiveNotification", function (message) {
    // message is a string like "New Article Published: Title"
    showNotification("Notification", message, "bi-bell", "text-primary");
});

notificationConnection.on("ReceiveArticleUpdate", function (message) {
    showNotification("Article Updated", `The article "<strong>${message}</strong>" has been updated.`, "bi-pencil-square", "text-warning");
});

notificationConnection.on("ReceiveMessage", function (user, message) {
    showNotification("Notification", message);
});

// Reset badge on open
document.addEventListener('DOMContentLoaded', function () {
    var dropdown = document.getElementById('notificationDropdown');
    if (dropdown) {
        dropdown.addEventListener('show.bs.dropdown', function () {
            var badge = document.getElementById('notificationBadge');
            if (badge) {
                badge.innerText = "0";
                badge.style.display = "none";
            }
        });
    }
});

// Connection Lifecycle Handlers
notificationConnection.onreconnecting(error => {
    console.warn("SignalR Reconnecting...", error);
});

notificationConnection.onreconnected(connectionId => {
    console.log("SignalR Reconnected. Id: " + connectionId);
});

notificationConnection.onclose(error => {
    console.error("SignalR Closed. Retrying...", error);
    setTimeout(startConnection, 5000); // Retry logic
});

async function startConnection() {
    try {
        await notificationConnection.start();
        console.log("SignalR Connected to /hubs/notifications!");
    } catch (err) {
        console.error("SignalR Connection Error: " + err.toString());
        setTimeout(startConnection, 5000);
    }
}

// Start
startConnection();
