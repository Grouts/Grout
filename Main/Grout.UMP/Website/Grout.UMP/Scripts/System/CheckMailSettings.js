function CheckMailSettingsAndNotify(args,selection,successMessage) {
    ShowWaitingProgress(".Sticky-header", "show");
    $.ajax({
        type: "POST",
        url: "/Administration/checkmailsettingsexist",
        success: function (result) {
            if (result.result == "success") {
                if (typeof selection != 'undefined') {
                    selection.html("<span class='SuccessMessage'>" + successMessage + "</span>");
                }
            }
            else if (result.result == "failure" && result.isAdmin == true) {
                if (typeof selection != 'undefined') {
                    selection.html("<span class='ErrorMessage'>" + args + "</span>");
                }
            }
            else {                
            }
        }
    });
}