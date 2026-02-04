// Function to load modal content
function loadModal(url, modalTitle) {
    console.log("Loading modal from:", url);
    $.get(url, function (data) {
        console.log("Modal content loaded successfully.");
        $('#modal-placeholder').html(data);

        var modalEl = document.getElementById('actionModal');
        if (!modalEl) {
            console.error("Modal container 'actionModal' not found!");
            alert("Error: Modal container not found.");
            return;
        }

        var modal = new bootstrap.Modal(modalEl);
        if (modalTitle) {
            $('#actionModalLabel').text(modalTitle);
        }
        modal.show();

        // Re-bind validation/forms specific plug-ins if needed
        bindModalForm();
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("Failed to load modal:", textStatus, errorThrown);
        console.error("Response:", jqXHR.responseText);
        alert("Failed to load modal. See console for details.\nStatus: " + textStatus + "\nError: " + errorThrown);
    });
}

function bindModalForm() {
    // Intercept form submission
    $('form.modal-form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var formData = form.serialize();

        $.post(url, formData, function (response) {
            if (response.success) {
                // Close modal
                var modalEl = document.getElementById('actionModal');
                var modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();

                // Show success message (optional)
                // alert('Saved successfully'); 

                // Reload page to show changes
                location.reload();
            } else {
                // If it returns HTML (Partial View with errors), replace the modal body
                // We assume the response IS the partial view content
                $('#modal-placeholder').html(response);
                bindModalForm(); // Re-bind for the new form
            }
        });
    });
}
