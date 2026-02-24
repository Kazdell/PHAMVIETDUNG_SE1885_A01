// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$(document).ready(function () {
  // 1. Global Loading Indicator for jQuery AJAX calls
  $(document).ajaxStart(function () {
    $("#global-loading-overlay").removeClass("d-none");
  });

  $(document).ajaxStop(function () {
    $("#global-loading-overlay").addClass("d-none");
  });
});

// 2. Global Loading Indicator for JS Fetch calls
const originalFetch = window.fetch;
window.fetch = async function (...args) {
  const loadingOverlay = document.getElementById("global-loading-overlay");
  if (loadingOverlay) {
    loadingOverlay.classList.remove("d-none");
  }

  try {
    const response = await originalFetch(...args);
    return response;
  } finally {
    if (loadingOverlay) {
      loadingOverlay.classList.add("d-none");
    }
  }
};
