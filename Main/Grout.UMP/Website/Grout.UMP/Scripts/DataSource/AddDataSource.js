$(function () {
    $("#new_datasource_tab_content").css("height", window.innerHeight - 113 + "px");
    $("#enable_windows_prompt").ejCheckBox({ size: "medium" });
    $("#enable_impersonate").ejCheckBox({ size: "medium" });
    $("#enable_windows_stored").ejCheckBox({ size: "medium" });
    $("#connect_option_prompt").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#connect_option_store").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#connect_option_windows").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#connect_option_none").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    parent.$("#datasource_popup_wrapper").ejWaitingPopup("hide");
    $("#datasource_type").on("change", function (e) {
        $("#datasource_connstring_validation_error").html("");
        if (this.value == "XML") {
            $("#test_connection").attr("disabled", "disabled");
        }
        else if ($('input:radio[name=connect_using]:checked').val() != "Prompt") {
            $("#test_connection").removeAttr("disabled");
        }
    });

    $("#datasource_popup_module input").keypress(function (e) {
        if (e.which == 13 && $("#publish_datasource").attr("disabled") != "disabled") {
            addNewDataSource();
        }
    });

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value);
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");


    $.validator.addMethod("requiredConnectionString", function (value, element) {
        var connectUsing = $(element).closest('table').find('input:radio[name=connect_using]:checked').val();
        if (connectUsing != "None" && $.trim(value) == '' && $(element).closest('table').find("#datasource_type").val() != "XML")
            return false;
        else
            return true;

    }, "Connection string should not be empty");

    $(".showHidePassword").on("mousedown", function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        }
        else {
            $(this).siblings("input").attr('type', 'password');
        }
    });

    $(".showHidePassword").on("mouseup", function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        }
        else {
            $(this).siblings("input").attr('type', 'password');
        }
    });

    $("#datasource_popup_module").validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "new_datasource_name": {
                isRequired: true,
                isValidName: true
            },
            "new_datasource_connectionstring": {
                isRequired: true,
                requiredConnectionString: true
            },
            "connection_stored_username": {
                isRequired: true

            },
            "connection_stored_password": {
                required: true
            }
        },
        messages: {
            "new_datasource_name": {
                isRequired: "Please enter data source name"
            },
            "new_datasource_connectionstring": {
                isRequired: "Connection string should not be empty"
            },
            "connection_stored_username": {
                isRequired: "Credentials are required"

            },
            "connection_stored_password": {
                required: "Credentials are required"
            }

        },
        highlight: function (element) {            
            $(element).closest('td').addClass("has-error");          
        },
        unhighlight: function (element) {
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').find("span.datasource-validation-messages").html("");
            if ($(element).attr('id') == "connection_stored_username")
                $(element).closest('div').find("span#datasource_credential_validation_error1").html("");
            else if ($(element).attr('id') == "connection_stored_password")
                $(element).closest('div').find("span#datasource_credential_validation_error2").html("");
        },
        errorPlacement: function (error, element) {            
            if ($(element).attr('id') == "connection_stored_username") {
                $(element).closest('div').find("span#datasource_credential_validation_error2").html("");
                $(element).closest('div').find("span#datasource_credential_validation_error1").html(error.html());
            }
            else if ($(element).attr('id') == "connection_stored_password") {
                $(element).closest('div').find("span#datasource_credential_validation_error1").html("");
                $(element).closest('div').find("span#datasource_credential_validation_error2").html(error.html());
            }
            else
                $(element).closest('td').find("span.datasource-validation-messages").html(error.html());
        }
    });

});

function onConnectionTypeChange(args) {
    switch (args.model.id) {
        case "connect_option_prompt":
            $(".connection-type-server").attr("disabled", "disabled");
            $(".connection-type-server").closest('td').removeClass("has-error");
            $(".connection-type-prompt").removeAttr("disabled");
            $("#enable_windows_prompt").data("ejCheckBox").enable();
            $("#enable_impersonate").data("ejCheckBox").disable();
            $("#enable_windows_stored").data("ejCheckBox").disable();
            if ($("#datasource_type").val() != "XML") {
                $("#test_connection").attr("disabled", "disabled");
            }
            $("#datasource_credential_validation_error").html("");
            break;
        case "connect_option_store":
            $(".connection-type-server").removeAttr("disabled");
            $(".connection-type-prompt").attr("disabled", "disabled");
            $("#enable_windows_prompt").data("ejCheckBox").disable();
            $("#enable_impersonate").data("ejCheckBox").enable();
            $("#enable_windows_stored").data("ejCheckBox").enable();
            if ($("#datasource_type").val() != "XML") {
                $("#test_connection").removeAttr("disabled");
            }
            break;
        case "connect_option_windows":
            $(".connection-type-server").attr("disabled", "disabled");
            $(".connection-type-server").closest('td').removeClass("has-error");
            $(".connection-type-prompt").attr("disabled", "disabled");
            $("#enable_windows_prompt").data("ejCheckBox").disable();
            $("#enable_impersonate").data("ejCheckBox").disable();
            $("#enable_windows_stored").data("ejCheckBox").disable();
            if ($("#datasource_type").val() != "XML") {
                $("#test_connection").removeAttr("disabled");
            }
            $("#datasource_credential_validation_error").html("");
            break;
        case "connect_option_none":
            $(".connection-type-server").attr("disabled", "disabled");
            $(".connection-type-server").closest('td').removeClass("has-error");
            $(".connection-type-prompt").attr("disabled", "disabled");
            $("#enable_windows_prompt").data("ejCheckBox").disable();
            $("#enable_impersonate").data("ejCheckBox").disable();
            $("#enable_windows_stored").data("ejCheckBox").disable();
            if ($("#datasource_type").val() != "XML") {
                $("#test_connection").removeAttr("disabled");
            }
            $("#datasource_credential_validation_error").html("");
            break;
    }
}

function addNewDataSource() {

    if (!$("#datasource_popup_module").valid()) {
        return;
    }
    else {
        parent.$("#datasource_popup_wrapper").ejWaitingPopup("show");
        var postData = getNewDataSourceFields();
        $.ajax({
            type: "POST",
            url: "/datasources/createdatasource",
            data: postData,
            async: false,
            success: function (data) {
                parent.$("#datasource_popup_wrapper").ejWaitingPopup("hide");
                if (data.result.IsNameExist) {
                    $("#datasource_name_validation_error").closest('td').addClass("has-error");
                    $("#datasource_name_validation_error").html("Data Source name already exists");
                }
                else {
                    if (data.result.ConnectionStringStatus) {
                        if (data.result.Status) {
                            parent.onDataSourceDialogClose();
                            parent.messageBox("su-datasource", "Add Data Source", "Data Source has been added successfully.", "success", function () {
                                var gridName = window.parent.$('#itemGridContainer').attr("data-grid-name");
                                if (gridName == "datasources") {
                                    parent.ResetGrid();
                                }
                                parent.onCloseMessageBox();
                            });
                            $("#datasource_name_validation_error").closest('td').removeClass("has-error");
                            $("#datasource_name_validation_error").html("");
                        }
                        else {
                            $("#connection_test_validation_error").html("Error while creating data source");
                        }
                    }
                    else {
                        $("#datasource_connstring_validation_error").closest('td').addClass("has-error");
                        $("#datasource_connstring_validation_error").html(data.result.Message);
                    }
                }
            }
        });
    }
}


function getNewDataSourceFields() {
    var dataSourceName = $("#new_datasource_name").val();
    var connectionString = $("#new_datasource_connectionstring").val();
    var connectUsing = $('input:radio[name=connect_using]:checked').val();
    var storedUserName = $("#connection_stored_username").val();
    var storedPassword = $("#connection_stored_password").val();
    var promptText = $("#prompt_text").val();
    var promptWindowsEnabled = $("#enable_windows_prompt").data("ejCheckBox").model.checked;
    var storedWindowsEnabled = $("#enable_windows_stored").data("ejCheckBox").model.checked;
    var userImpersonate = $("#enable_impersonate").data("ejCheckBox").model.checked;
    var dataSourceType = $("#datasource_type").val();
    var dataSourceDescription = $("#new_datasource_description").val();
    var postData = {
        Name: dataSourceName,
        Description: dataSourceDescription,
        DataSourceType: dataSourceType,
        ConnectionString: connectionString,
        ConnectUsing: connectUsing,
        UserName: storedUserName,
        Password: storedPassword,
        PromptText: promptText,
        EnablePromptWindowsAuth: promptWindowsEnabled,
        EnableStoredWindowsAuth: storedWindowsEnabled,
        ImpersonateUser: userImpersonate,
    }
    return postData;
}

function onTestDataSourceConnection() {
    $("#connection_test_validation_error").html("");
    parent.$("#datasource_popup_wrapper").ejWaitingPopup("show");
    var connectionString = $("#new_datasource_connectionstring").val();
    var connectUsing = $('input:radio[name=connect_using]:checked').val();
    var storedUserName = $("#connection_stored_username").val();
    var storedPassword = $("#connection_stored_password").val();
    if ($("#datasource_popup_module").valid()) {
        var storedWindowsEnabled = $("#enable_windows_stored").data("ejCheckBox").model.checked;
        var dataSourceType = $("#datasource_type").val();
        var postData = {
            DataSourceType: dataSourceType,
            ConnectionString: connectionString,
            ConnectUsing: connectUsing,
            UserName: storedUserName,
            Password: storedPassword,
            EnableStoredWindowsAuth: storedWindowsEnabled
        }

        $.ajax({
            type: "POST",
            url: "/datasources/testdatasourceconnection",
            data: postData,
            async: false,
            success: function (data) {
                parent.$("#datasource_popup_wrapper").ejWaitingPopup("hide");
                if (data.result.ConnectionStringStatus) {
                    if (data.result.Status) {
                        $("#connection_test_validation_error").html("<span style='color:green'>" + data.result.Message + "</span>");
                    }
                    else {
                        $("#connection_test_validation_error").html("<span style='color:red'>" + data.result.Message + "</span>");
                    }
                }
                else {
                    $("#datasource_connstring_validation_error").html(data.result.Message);
                }
            }
        });
    }
    else {
        parent.$("#datasource_popup_wrapper").ejWaitingPopup("hide");
    }
}
$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        $('.PopupClose').click();
        window.parent.$("#createButton").focus();
    }
});