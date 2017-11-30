$(function () {
    parent.$("#AddCategoryPopup_wrapper").ejWaitingPopup("hide");

    $.validator.addMethod("isValidName", function (value, element) {
        return parent.IsValidName("name",value);
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !parent.isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $("#addCategoryForm").validate({
        errorElement: 'span',
        onkeyup: function (element,event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "categoryName": {
                isRequired: true,
                isValidName: true
            }
        },
        messages: {
            "categoryName": {
                isRequired: "Please enter category name"
            }
        },
        highlight: function (element) {
            $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').find("span.validation-message").html(error.html());
        }
    });
});
function addItemValidate(e) {
    var canProceed = $("#addCategoryForm").valid();
    var categoryName = $("#category_name").val().trim();
    if (!canProceed) {
        return false;
    }
    else {
        parent.$("#AddCategoryPopup_wrapper").ejWaitingPopup("show");
        $.ajax({
            type: "POST",
            url: "/category/iscategoryexist",
            data: { CategoryName: categoryName },
            async: false,
            success: function (data) {
                if (data.Data) {
                    canProceed = false;
                    parent.$("#AddCategoryPopup_wrapper").ejWaitingPopup("hide");
                    $("#validate-name").parent().addClass("has-error");
                    $("#validate-name").html("Category name already exists");
                } else {
                    canProceed = true;
                    $("#validate-name").closest('td').removeClass("has-error");
                    $("#validate-name").text("");
                }
            }
        });
    }
    return canProceed;
}


function closeAddItemPopup() {
    parent.$("#AddCategoryPopup").ejDialog("close");
}

$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        $('.PopupClose').click();
        window.parent.$("#createButton").focus();
    }
});

$(document).on("keyup focusout", "#category_name", function (e) {
    $(this).closest('td').removeClass('has-error');
    $(this).closest('td').find("span.validation-message").html("");
    $("#addCategoryForm").valid();
});