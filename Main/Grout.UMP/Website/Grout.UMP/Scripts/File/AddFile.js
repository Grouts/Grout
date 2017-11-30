$(function () {
    parent.$("#addFileDom_wrapper").ejWaitingPopup("hide");

    $("#PopupContainer input").keypress(function (e) {
        if (e.which == 13) {
            addfileValidate();
        }
    });

    $.validator.addMethod("isRequired", function (value, element) {
        return !parent.isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $.validator.addMethod("isValidName", function (value, element) {
        return parent.IsValidName("name", value);
    }, "Please avoid special characters");

    $("#addItemForm").validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "fileName": {
                isRequired: true,
                isValidName: true
            }
        },
        messages: {
            "fileName": {
                isRequired: "Please enter file name"
            }
        },
        highlight: function (element) {           
                $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {
            if ($(element).attr('id') == 'file_name') {
                $(element).closest('td').removeClass('has-error');
                $(element).closest('td').find("span.validation-message").html("");
            }
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').find("span.validation-message").html(error.html());
        }
    });
    $(document).on("focusout", "#filename", function (event) {
        $("#addItemForm").valid();
    });

});

function addfileValidate() {
    var canProceed = false;
    var fileName = $("#file_name").val().trim();
    ValidateFile();
    if ($("#addItemForm").valid()&& ValidateFile()) {
        parent.$("#addFileDom_wrapper").ejWaitingPopup("show");
        $.ajax({
            type: "POST",
            url: "/files/isfileexist",
            data: { filename: fileName },
            async: false,
            success: function (data) {
                if (data.Data) {
                    $("#validate-name").closest('td').addClass("has-error");
                    $("#validate-name").text("File name already exists");
                    $("#validate-name").css("display", "block");
                    $("#validate-file").closest('td').removeClass("has-error");
                    $("#validate-file").css("display", "none");
                    canProceed = false;
                }
                else {
                    $("#validate-name").css("display", "none");
                    $("#validate-file").css("display", "none");
                    canProceed = true;
                }
            }
        });
    }
    if (canProceed) {
        $('form').submit();
    }
    else {
        parent.$("#addFileDom_wrapper").ejWaitingPopup("hide");
    }
}


function closeAddItemPopup() {
    parent.$("#addFileDom").ejDialog("close");
}
function ValidateFile()
{
    var isValid = true;
    if($('#browse_file').val()=='')
    {
        $(".fileUpload").addClass("error-file-upload");
        $(".fileUpload").addClass("no-left-border");
        $("#filename").addClass('error-file-upload');
        $("#validate-file").html("Please upload a file");
        $("#filename").val("Browse file path");
        isValid = false;
    }
    else {        
        $(".fileUpload").removeClass("error-file-upload");
        $(".fileUpload").removeClass("no-left-border");
        $("#filename").removeClass('error-file-upload');
        $("#filename").closest('td').find("span.validation-message").html("");
        $('.imagepath').removeClass('has-error');
        $("#filename").trigger("focus");
        isValid = true;
    }
    return isValid;
}


$(document).on("change", "#browse_file", function () {
    if ($(this).val() != '') {
        var value = $(this).val() == "" ? "Browse file path" : $(this).val();
        $("#filename").val(value.substring(value.lastIndexOf('\\') + 1));       
        $(".fileUpload").removeClass("error-file-upload");
        $("#filename").removeClass('error-file-upload');
        $("#filename").closest('td').find("span.validation-message").html("");
        $('.imagepath').removeClass('has-error');
        $("#filename").trigger("focus");
    }
    else {
        $(".fileUpload").addClass("error-file-upload");
        $("#filename").addClass('error-file-upload');
        $("#validate-file").text("Please upload a valid file");
        $("#filename").val("Browse file path");
        $("#browse_file").val("");
        isValid = false;
    }

});
$(document).on("click", "#filename", function () {
    $("#browse_file").trigger("click");
});


$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        $('.PopupClose').click();
        window.parent.$("#createButton").focus();
    }
    if (e.keyCode == 13) {
        e.preventDefault();
        e.stopPropagation();
        addfileValidate();
    }
});