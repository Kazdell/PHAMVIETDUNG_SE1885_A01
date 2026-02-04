"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/notificationHub")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveMessage", function (user, message) {
    // Create toast HTML
    var toastHtml = `
        <div class="toast show" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header">
                <strong class="me-auto">New Notification</strong>
                <small class="text-muted">Just now</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    // Append to container
    var container = document.getElementById("toast-container");
    if (container) {
        var div = document.createElement("div");
        div.innerHTML = toastHtml;
        container.appendChild(div.firstElementChild);
    }
});

connection.start().then(function () {
    console.log("SignalR Connected!");
}).catch(function (err) {
    return console.error(err.toString());
});
