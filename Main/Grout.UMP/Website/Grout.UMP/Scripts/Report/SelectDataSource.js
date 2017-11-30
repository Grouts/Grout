$(function () {
    window.isEditPopup = false;
    $("#rdl_new_tab_arrow").css("left", "50px");
    $("#enable_datasource").ejCheckBox({ checked: true, size: "medium" });
    $("#enable_windows_prompt").ejCheckBox({ size: "medium" });
    $("#enable_impersonate").ejCheckBox({ size: "medium"});
    $("#enable_windows_stored").ejCheckBox({ size: "medium" });
    $("#connect_option_prompt").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#connect_option_store").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#connect_option_windows").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#connect_option_none").ejRadioButton({ size: "medium", change: onConnectionTypeChange });
    $("#datasource_type").on("change", function (e) {
        $("#datasource_connstring_validation_error").html("");
        if (this.value == "XML") {
            $("#test_connection").attr("disabled", "disabled");
        }
        else if ($('input:radio[name=connect_using]:checked').val() != "Prompt") {
            $("#test_connection").removeAttr("disabled");
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
        if (connectUsing != "None" && value == "" && $(element).closest('table').find("#datasource_type").val() != "XML")
            return false;
        else
            return true;

    }, "Connection string should not be empty");

    $.validator.addMethod("isValidCredential", function (value, element) {
        var credentialTable = $(element).closest('table');
        var storedUserName = $(credentialTable).find("#connection_stored_username").val();
        var storedPassword = $(credentialTable).find("#connection_stored_password").val();
        if (storedUserName == "" || storedPassword == "") {
            return false;
        } else {
            return true;
        }
    }, "Credentials are required");

    $("#new_datasource_tab_content").validate({
        errorElement: 'span',
        onkeyup: function (element,event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "new_datasource_name": {
                isRequired: true,
                isValidName: true
            },
            "new_datasource_connectionstring": {
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



    $("#available_datasource_table").on("click", "tr", function (e) {
        if (!$(this).hasClass("available-datasource-select") && !$(this).hasClass("partition-headers-validate")) {
            $(".available-datasource-select").removeClass("available-datasource-select");
            $(this).addClass("available-datasource-select");
            $("#avl_datasource_tab").attr("data-isselected", "true");
            $("#save_rdl_with_datasource").removeAttr("disabled");
        }
    });
    });

function onSubmitDataSource() {
    $("#datasource_validation").css("display", "none");
    window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("show");
    var activeTab = $("#datasource_tabs").find("span.datasource-tab-active").attr("id");
    var selectedDataSource = new Object();
    var selectedDataSouceItem = window.parent.$('#report_iframe').contents().find("table#datasource_list_table tr").filter("[data-key='" + $("#publish_datasource").attr("data-mapid") + "']");
    var isMapping = $("#publish_datasource").attr("data-mapid") != undefined ? true : false;
    switch (activeTab) {
        case "avl_datasource_tab":
            if ($("#available_datasource_table").find(".available-datasource-select").length == 0) {
                window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
                $("#datasource_validation").html("Please select the data source").css("display", "block");
                //window.parent.umpObj.toast("Please select data source from the list", "error");
                return;
            }
            selectedDataSource = { Id: $("#available_datasource_table").find(".available-datasource-select").attr("data-sourceid"), Name: $("#available_datasource_table").find(".available-datasource-select").find("td:first-child span").text() };
            closeNewDataSourcePopup();
            break;
        case "new_datasource_select_tab":
            selectedDataSource = addNewDataSource();
            if (!selectedDataSource.isValid) {
                window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
                return;
            }
            break;
    }
    if (isMapping) {
        window.parent.document.getElementById("report_iframe").contentWindow.onSelectionOfDataSource(selectedDataSouceItem, selectedDataSource);
    }
    window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
}

function onAvailableDataSourceTabSelect() {
    $(".rdl-datasource-tab-contents").hide();
    $("#avl_datasource_tab_content").show();
    $("#datasource_tabs").find(".datasource-tab-active").removeClass("datasource-tab-active");
    $("#datasource_tabs").find("#avl_datasource_tab").addClass("datasource-tab-active");
    $("#rdl_new_tab_arrow").css("left", "50px");
    $("#publish_datasource").attr("value", "Select");
}

function onNewDataSourceTabSelect() {
    $("#datasource_validation").css("display", "none");
    $("#datasource_type").selectpicker("refresh");
    $(".rdl-datasource-tab-contents").hide();
    $("#new_datasource_tab_content").show();
    $("#datasource_tabs").find(".datasource-tab-active").removeClass("datasource-tab-active");
    $("#datasource_tabs").find("#new_datasource_select_tab").addClass("datasource-tab-active");
    $("#rdl_new_tab_arrow").css("left", "180px");
        $("#publish_datasource").attr("value", "Add");
    }

function onConnectionTypeChange(args) {
    switch (args.model.id) {
        case "connect_option_prompt":
            $(".connection-type-server").attr("disabled", "disabled");
            $(".connection-type-prompt").removeAttr("disabled");
            $(".connection-type-server").closest('td').removeClass("has-error");
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
    var result = new Object();
    if (!$("#new_datasource_tab_content").valid()) {
        result.isValid = false;
        return result;
    }
    else {
        result.isValid = true;
        window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("show");
            var postData = getNewDataSourceFields();
            result.Name = postData.Name;
            $.ajax({
                type: "POST",
                url: "/datasources/createdatasource",
                data: postData,
                async: false,
                success: function (data) {
                    window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
                    if (data.result.IsNameExist) {
                        result.isValid = false;
                        $("#datasource_name_validation_error").closest('td').removeClass("has-error");
                        $("#datasource_name_validation_error").html("Data Source name already exists");
                    }
                    else {
                        if (data.result.ConnectionStringStatus) {
                            if (data.result.Status) {
                                result.isValid = true;
                                result.Id = data.result.PublishedDataSourceId;
                                closeNewDataSourcePopup();
                            }
                            else {
                                result.isValid = false;
                            $("#connection_test_validation_error").html("Error while creating data source");
                            }
                            $("#datasource_name_validation_error").closest('td').removeClass("has-error");
                            $("#datasource_name_validation_error").html("");
                        }
                        else {
                            result.isValid = false;
                            $("#datasource_connstring_validation_error").closest('td').addClass("has-error");
                            $("#datasource_connstring_validation_error").html(data.result.Message);
                        }
                    }
                }
            });
        return result;
    }
}

function validateNewDataSourceForm() {
    var isValid = true;
    var dataSourceName = $("#new_datasource_name").val();
    if (dataSourceName == "") {
        $("#datasource_name_validation_error").html('Name should not be empty');
        isValid = false;
    }
    else if (!parent.IsValidName("name",dataSourceName)) {
        $("#datasource_name_validation_error").html('Name should not contain ;?:@&=+$,\*<>|"/,');
        isValid = false;
    }
    var connectionString = $("#new_datasource_connectionstring").val();
    var connectUsing = $('input:radio[name=connect_using]:checked').val();
    if (connectUsing != "None" && connectionString == "" && $("#datasource_type").val() != "XML") {
        $("#datasource_connstring_validation_error").html('Connection string should not be empty');
        isValid = false;
    }
    var storedUserName = $("#connection_stored_username").val();
    var storedPassword = $("#connection_stored_password").val();
    if (connectUsing == "Store") {
        if (window.isEditPopup) {
            var needPassword = validateDataSourceDefinitionChange();
            if (needPassword)
                isValid = false;
        }
        else {
            if (storedUserName == "" || storedPassword == "") {
                $("#datasource_credential_validation_error").html('Credentials are required');
                isValid = false;
            }
        }
    }
    return isValid;
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
    var categoryId = $("#selected_category option:selected").val();
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
        SelectedCategoryId: categoryId
    }
    return postData;
}

function getUpdatedDataSourceFields() {
    var dataSourceName = $("#new_datasource_name").val();
    var dataSourceDescription = $("#new_datasource_description").val();
    var itemId = window.editData.ItemId;
    if (window.editData.IsDataSourceDefinitionChanged) {
        var connectionString = $("#new_datasource_connectionstring").val();
        var connectUsing = $('input:radio[name=connect_using]:checked').val();
        var storedUserName = $("#connection_stored_username").val();
        var storedPassword = $("#connection_stored_password").val();
        var promptText = $("#prompt_text").val();
        var promptWindowsEnabled = $("#enable_windows_prompt").data("ejCheckBox").model.checked;
        var storedWindowsEnabled = $("#enable_windows_stored").data("ejCheckBox").model.checked;
        var userImpersonate = $("#enable_impersonate").data("ejCheckBox").model.checked;
        var dataSourceType = $("#datasource_type").val();
        var postData = {
            IsNameChanged: window.editData.IsNameChanged,
            IsDescriptionChanged: window.editData.IsDescriptionChanged,
            IsDataSourceDefinitionChanged: window.editData.IsDataSourceDefinitionChanged,
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
            ItemId: itemId    
           
        }
    }
    else {
        var postData = {
            IsNameChanged: window.editData.IsNameChanged,
            IsDescriptionChanged: window.editData.IsDescriptionChanged,
            IsDataSourceDefinitionChanged: window.editData.IsDataSourceDefinitionChanged,
            Name: dataSourceName,
            Description: dataSourceDescription,
            ItemId: itemId            
        }
    }
    return postData;
};

function closeNewDataSourcePopup(e) {
    window.isEditPopup = false;
    $("#publish_datasource").removeAttr("disabled");
    $("#publish_datasource").removeAttr("data-mapId");
    $("#file_name").val("");
    $("#file_description").val("");
    $(".datasource-validation-messages").html("");
    $("#createReportPopupHolder").find("input:not('#prompt_text')").filter("[type='text']").val("");
    $("#createReportPopupHolder").find("textarea").val("");
    $("#createReportPopupHolder").find("input").filter("[type='password']").val("");
    $("#enable_datasource").ejCheckBox({ checked: true });
    $("#enable_windows_prompt").ejCheckBox({ checked: false });
    $("#enable_impersonate").ejCheckBox({ checked: false });
    $("#enable_windows_stored").ejCheckBox({ checked: false });
    $('#datasource_type').val("SQL");
    $('#datasource_type').selectpicker("refresh");
    (!$("#version_comment_tr").hasClass("hidden-visibility")) ? $("#version_comment_tr").addClass("hidden-visibility") : "";
    $("#comment").val("");
    $("#comment").attr("readonly", true);
    $("#connect_option_prompt").ejRadioButton({ checked: true });
    $("#prompt_text").val("Type or enter a username and password to access the Data Source:");
    window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
    window.parent.$("#select_datasource_popup").ejDialog("close");
}

function onTestDataSourceConnection() {
    window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("show");
    $("#connection_test_validation_error").html("");
    var isValid = true;
    var connectionString = $("#new_datasource_connectionstring").val();
    var connectUsing = $('input:radio[name=connect_using]:checked').val();
    if (connectUsing != "None" && connectionString == "") {
        $("#datasource_connstring_validation_error").html('Connection string should not be empty');
        isValid = false;
    }
    var storedUserName = $("#connection_stored_username").val();
    var storedPassword = $("#connection_stored_password").val();
    if (connectUsing == "Store") {
        if (storedUserName == "" || storedPassword == "") {
            $("#datasource_credential_validation_error").html('Credentials are required');
            isValid = false;
        }
    }
    if (isValid) {
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
        doAjaxPost("POST", "/datasources/testdatasourceconnection", postData, function (data) {
            window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
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
        });
    }
    else {
        window.parent.$('#select_datasource_popup_wrapper').ejWaitingPopup("hide");
    }
}