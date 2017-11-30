$(function () {
    $("#comment").attr("readonly", true);
    $(document).on("keyup", ".text-field", function (e) {
        if ($(this).attr("id") === "file_name") {
            if (window.editData.Name !== $(this).val()) {
                $("#name_change_validation").val(true);
            }
            else {
                $("#name_change_validation").val(false);
            }
        }
        if ($(this).attr("id") === "file_description") {
            if (window.editData.Description !== $(this).val()) {
                $("#description_change_validation").val(true);
            }
            else {
                $("#description_change_validation").val(false);
            }
        }
        onChangeValidation();
    });
    $(document).on('change', '#browse_file', function (e) {
        var value = $(this).val();
        $('#name').val(value.substring(value.lastIndexOf('\\') + 1));
        $("#source_change_validation").val(true);
        $("#comment").attr("readonly", false);
        onChangeValidation();
    });
    window.parent.$('#EditFilePopup_wrapper').ejWaitingPopup("hide");

    $.validator.addMethod("isValidName", function (value, element) {
        return parent.IsValidName("name", value);
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !parent.isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $("#editItemForm").validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "Name": {
                isRequired: true,
                isValidName: true
            }     
        },
        messages: {
            "Name": {
                isRequired: "Please enter file name"
            }
        },
        highlight: function (element) {            
            $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {          
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').find("span.validation-message").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').find("span.validation-message").html(error.html());
        }
    });

});

function validateNewFileForm() {
    var isValid = $("#editItemForm").valid();
    if (isValid) {
        window.parent.$('#EditFilePopup_wrapper').ejWaitingPopup("show");
        $("form").submit();
    }
}

function closeEditFilePopup() {
    parent.$("#EditFilePopup").ejDialog("close");
}

$(document).on("click", "#filename", function () {
    $("#name").trigger("focus");
    $("#browse_file").trigger('click');
});


function onChangeValidation() {
    if ($("#name_change_validation").val() == "true" || $("#description_change_validation").val() == "true" || $("#source_change_validation").val() == "true") {
        $("#publish_file").attr("disabled", false);
    }
    else {
        $("#publish_file").attr("disabled", true);
    }
}


$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        $('.PopupClose').click();
        window.parent.focus();
    }
});