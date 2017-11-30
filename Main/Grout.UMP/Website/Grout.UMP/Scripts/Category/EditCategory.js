$(function () {
    $(document).on("keyup", ".text-field", function (e) {
        if ($(this).attr("id") === "category_name") {
            if (window.editData.Name !== $(this).val().trim()) {
                window.editData.IsNameChanged = true;
            } else {
                window.editData.IsNameChanged = false;
            }
        }
        if ($(this).attr("id") === "category_description") {
            if (window.editData.Description !== $(this).val().trim()) {
                window.editData.IsDescriptionChanged = true;
            } else {
                window.editData.IsDescriptionChanged = false;
            }
        }

    if ((window.editData.IsNameChanged || window.editData.IsDescriptionChanged)) {
        window.isEdited = true;
            $("#saveEditCategory").removeAttr("disabled");
        } else {
        window.isEdited = false;
            $("#saveEditCategory").attr("disabled", "disabled");
    }
    });

    $.validator.addMethod("isValidName", function (value, element) {
        return parent.IsValidName("name", value);
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !parent.isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $("#create_category_content").validate({
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
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').find("span.validation-message").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').find("span.validation-message").html(error.html());
        }
    });
});

function updateCategory() {
    if ($('#saveEditCategory').is('[disabled=disabled]')) {
        return false;
    }
    window.parent.$("#EditCategoryPopup_wrapper").ejWaitingPopup("show");
    if (!$("#create_category_content").valid()) {
        window.parent.$("#EditCategoryPopup_wrapper").ejWaitingPopup("hide");
        return;
    }
    else {
        var postData = getUpdatedCategoryFields();
        $.ajax({
            type: "POST",
            url: "/category/editcategory",
            data: postData,
            async: false,
            success: function (data) {
                if (data.NameExists) {
                    $("#validate-name").closest('td').addClass('has-error');
                    $("#validate-name").html("Category name already exists");
                }
                else {
                    if (data.Status) {
                        parent.$("#EditCategoryPopup").ejDialog("close");
                        $("#validate-name").closest('td').removeClass('has-error');
                        $("#validate-name").html("");
                        parent.messageBox("su-folder", "Update Category", "Category has been updated successfully.", "success", function () {
                            window.parent.location = "/reports?categoryName=" + postData.Name;
                        });
                    }
                    else {
                        $("#validate-name").closest('td').addClass('has-error');
                        $("#validate-name").html("Error in update category");
                    }
                }
                window.parent.$("#EditCategoryPopup_wrapper").ejWaitingPopup("hide");
            }
        });
    }
}

function getUpdatedCategoryFields() {
    var categoryname = $("#category_name").val();
    var categoryDescription = $("#category_description").val();
    var itemId = window.editData.ItemId;
    var postData = {
        IsNameChanged: window.editData.IsNameChanged,
        IsDescriptionChanged: window.editData.IsDescriptionChanged,
        IsDataSourceDefinitionChanged: window.editData.IsDataSourceDefinitionChanged,
        Name: categoryname,
        Description: categoryDescription,
        ItemId: itemId
    }
    return postData;
}

function closeEditCategoryPopup() {
    parent.$("#EditCategoryPopup").ejDialog("close");
}

