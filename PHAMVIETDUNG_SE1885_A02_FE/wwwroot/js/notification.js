"use strict";

// Get current user ID for API calls
function getCurrentUserId() {
    const el = document.getElementById("currentAccountId");
    return el ? el.value : null;
}

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

    // 2. Refresh List from DB (simplest way to ensure sync)
    loadNotifications();
}

notificationConnection.on("ReceiveNotification", function (message) {
    showNotification("Notification", message, "bi-bell", "text-primary");
});

notificationConnection.on("ReceiveArticleUpdate", function (message) {
    showNotification("Article Updated", `The article "<strong>${message}</strong>" has been updated.`, "bi-pencil-square", "text-warning");
});

notificationConnection.on("ReceiveMessage", function (user, message) {
    showNotification("Notification", message);
});

// Load notifications from DB
function loadNotifications() {
    const userId = getCurrentUserId();
    if (!userId) return; // Not logged in

    fetch(`http://localhost:5000/api/Notification/Me?userId=${userId}`)
        .then(response => {
            if (!response.ok) return [];
            return response.json();
        })
        .then(data => {
            var list = document.getElementById("notificationList");
            var badge = document.getElementById("notificationBadge");
            var noNotif = document.getElementById("no-notifications");

            if (!list) return;

            list.innerHTML = ''; // Clear current

            if (data.length === 0) {
                if (badge) badge.style.display = 'none';
                // Optional: Show empty state
                list.innerHTML = '<div id="no-notifications" class="text-center p-3 text-muted small">No new notifications</div>';
                return;
            }

            let unreadCount = 0;

            data.forEach(note => {
                if (!note.isRead) unreadCount++;

                // Determine link: if articleId exists, link to article detail page with notification ID
                const articleLink = note.articleId ? `/Home/Details/${note.articleId}?fromNotification=${note.notificationId}` : '#';
                const hasLink = note.articleId != null;

                var itemHtml = `
                    <a href="${articleLink}" onclick="markAsRead(event, ${note.notificationId}, ${hasLink})" class="list-group-item list-group-item-action border-0 border-bottom p-3 small ${note.isRead ? 'opacity-75' : 'bg-light fw-bold'}">
                        <div class="d-flex w-100 justify-content-between mb-1">
                            <h6 class="mb-0 ${note.isRead ? '' : 'text-primary'}">${note.title || 'Notification'}</h6>
                            <small class="text-muted">${new Date(note.createdDate).toLocaleString()}</small>
                        </div>
                        <p class="mb-0 text-dark">${note.message}</p>
                        ${hasLink ? '<small class="text-info"><i class="bi bi-arrow-right"></i> Click to view article</small>' : ''}
                    </a>
                `;
                var tempDiv = document.createElement('div');
                tempDiv.innerHTML = itemHtml;
                list.appendChild(tempDiv.firstElementChild);
            });

            if (badge) {
                if (unreadCount > 0) {
                    badge.innerText = unreadCount > 9 ? "9+" : unreadCount;
                    badge.style.display = "block";
                } else {
                    badge.style.display = "none";
                }
            }
        })
        .catch(err => console.error("Error loading notifications:", err));
}

function markAsRead(event, id, hasLink = false) {
    // Only prevent default if no article link
    if (!hasLink) {
        event.preventDefault();
    }

    // Optimistic UI update
    var item = event.currentTarget;
    item.classList.remove("bg-light", "fw-bold");
    item.classList.add("opacity-75");
    var title = item.querySelector("h6");
    if (title) title.classList.remove("text-primary");

    // Decrement badge
    var badge = document.getElementById("notificationBadge");
    if (badge && badge.style.display !== 'none') {
        var count = parseInt(badge.innerText);
        if (!isNaN(count) && count > 0) {
            count--;
            badge.innerText = count > 0 ? (count > 9 ? "9+" : count) : "0";
            if (count === 0) badge.style.display = 'none';
        }
    }

    fetch(`http://localhost:5000/api/Notification/MarkRead/${id}`, { method: 'POST' }).catch(console.error);
}

// Initial Load
document.addEventListener('DOMContentLoaded', function () {
    loadNotifications();

    // Mark all read button (if exists)
    var markAllBtn = document.getElementById('markAllReadBtn');
    if (markAllBtn) {
        markAllBtn.addEventListener('click', function () {
            fetch('http://localhost:5000/api/Notification/MarkAllRead', { method: 'POST' })
                .then(() => loadNotifications());
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
