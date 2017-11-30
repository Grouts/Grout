var isKeyUp = false;
$(document).ready(function () {
    var isFirstRequest = false;
    addPlacehoder("#user_add_dialog");
    addPlacehoder("#searchArea");
    $("#user_add_dialog").ejDialog({
        width: "500px",
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "355px",
        title: "Add User",
        showHeader: false,
        enableModal: true,
        close: "onUserAddDialogClose",
        closeOnEscape: false,
        open: "onUserAddDialogOpen"
    });

    $("#singleuser_delete_confirmation").ejDialog({
        width: "400px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "187px",
        title: "Delete User",
        showHeader: false,
        enableModal: true,
        close: "onSingleDeleteDialogClose",
        closeOnEscape: false,
        open: "onSingleDeleteDialogOpen"
    });

    $("#user_delete_confirmation").ejDialog({
        width: "400px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "187px",
        showHeader: false,
        title: "Delete User",
        enableModal: true,
        close: "onDeleteDialogClose",
        closeOnEscape: false,
        open: "onDeleteDialogOpen"
    });

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $.validator.addMethod("isValidUserName", function (value, element) {
        return IsValidName("username", value)
    }, "Please avoid special characters");

    $.validator.addMethod("hasWhiteSpace", function (value, element) {
        return HasWhiteSpace(value)
    }, "Username should not contain white space");

    $.validator.addMethod("isValidEmail", function (value, element) {
        if (isKeyUp)
            return true;
        else
            return IsEmail(value)
    }, "Invalid email");

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value)
    }, "Please avoid special characters");

    $('.add-user-dialog-form').validate({
        errorElement: 'span',
        onkeyup: function (element, event) {
            if (event.keyCode != 9) {
                isKeyUp = true;
                $(element).valid();
                isKeyUp = false;
            }
            else
                true;
        },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "username": {
                isRequired: true,
                hasWhiteSpace: true,
                isValidUserName: true
            },
            "email-address": {
                isRequired: true,
                isValidName: true,
                isValidEmail: true
            },
            "first-name": {
                isRequired: true,
                isValidName: true
            }

        },
        highlight: function (element) {
            $(element).closest('div').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('div').removeClass('has-error');
            $(element).closest('div').find("span").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('div').find("span").html(error.html()).css("display", "block");
        },
        messages: {
            "username": {
                isRequired: "Please enter username"
            },
            "email-address": {
                isRequired: "Please enter email address"
            },
            "first-name": {
                isRequired: "Please enter first name"
            }
        }

    });

    if (document.getElementById("UserCount") != null) {
        if ($("#UserCount").attr("data-value").toLowerCase() == "true" && $("#csvFileError").attr("data-value").toLowerCase() != "error") {
            $("#grid_nodata_validation").css("display", "block");
            $("#SaveButtonClick").attr("disabled", "disabled");
        }
    }

    $(document).on("click", "#AddUserDash", function () {
        $("#ADDUSER").removeAttr("disabled");
        $(".form input[type='text']").val('');
        var usergrid = $('#user_grid').data("ejGrid");
        usergrid.clearSelection();
        $("#AddUserInGroup").removeClass("show").addClass("hide");
        $(".validation").closest("div").removeClass("has-error");
        $(".useradd_validation_messages").css("display", "none");
    });

    $(".MakeFlyDeleteUsers").on("click", function () {
        $("#user_delete_confirmation").ejDialog("open");
    });

    $("input#ADDUSER").on('click', function () {
        var userName = $("#username").val().trim();
        var firstName = $("#firstname").val().trim();
        var emailid = $('#mailid').val().trim();
        var isValid = $('.add-user-dialog-form').valid();
        if (isValid) {
            $(".useradd_validation_messages").css("display", "none");
            var g = $('#user_grid').data("ejGrid");

            showWaitingPopup("user_add_dialog_wrapper");

            var lastName = $('#lastname').val().trim();
            var values = "&username=" + userName + "&emailid=" + emailid + "&firstname=" + firstName + "&lastname=" + lastName;

            $.ajax({
                type: "POST", url: "/UserManagement/IsPresentusername", data: { userName: userName.toLowerCase() },
                success: function (data) {
                    if (data.toLowerCase() == "true") {
                        $('#username').val('');
                        $('#username').closest('div').addClass("has-error");
                        $("#invalid_username").html("Username already exist").css("display", "block");
                        $(".useradd_validation_messages").css("display", "block");
                        hideWaitingPopup("user_add_dialog_wrapper");
                        return;
                    }
                    else {
                        $.ajax({
                            type: "POST", url: "/UserManagement/IsPresentEmailId", data: { emailId: emailid.toLowerCase() },
                            success: function (data) {
                                if (data.toLowerCase() == "true") {
                                    $('#mailid').val('');
                                    $('#mailid').closest('div').addClass("has-error");
                                    $("#invalid_email").html("Email already exist").css("display", "block");
                                    $(".useradd_validation_messages").css("display", "block");
                                    hideWaitingPopup("user_add_dialog_wrapper");
                                    return; 
                                }
                                else {
                                    $.ajax({
                                        type: "POST", url: "/UserManagement/postaction", data: values,
                                        success: function (data, result) {
                                            if ($.type(data) == "object") {
                                                if (data.Data.result == "success") {
                                                    hideWaitingPopup("user_add_dialog_wrapper");
                                                    $("#ADDUSER").attr("disabled", "disabled");
                                                    $("#AddUserDash").removeClass("hide").addClass("show");
                                                    $(".form input[type='text']").val('');
                                                    onUserAddDialogClose();
                                                    messageBox("su-user-add", "Add User", "User has been added successfully.", "success", function () {
                                                        g.refreshContent();
                                                        onCloseMessageBox();
                                                    });
                                                    CheckMailSettingsAndNotify("User has been created successfully. As email settings are not configured we are not able to send activation email to the user.");
                                                    $(".e-image").hide();
                                                }
                                            }
                                            else {

                                            }

                                        }
                                    });
                                }
                            }
                        });
                    }
                }
            });
        }
        else {
            $(".useradd_validation_messages").css("display", "block");
        }
    });

    $(document).on("click", "#NewAddGroup", function () {
        $("#existing_group").hide();
        $("#NewAddGroup").hide();
        $("#ExistingAddGroup").show();
        $("#new_group").show();
    });

    $(document).on("click", "#ExistingAddGroup", function () {
        $(".group_validation").css("display", "none");
        $("#existing_group").show();
        $("#ExistingAddGroup").hide();
        $("#NewAddGroup").show();
        $("#new_group").hide();
    });

    $(document).on("click", "#CancelExistingUserButton,#group-Close-button", function () {
        parent.$(".modal,.modal-backdrop").remove();
    });

    $(document).on("click", "#AddExistingUserButton", function () {

        $("#group_name_validation").closest("div").addClass("has-error");
        var g = $('#user_grid').data("ejGrid");
        showWaitingPopup("MovableDialog");
        if ($("#new_group").css("display") == "none") {
            var GroupNameDropdown = $("#GroupNameDropdown").val();

            var GroupUsers = document.getElementsByName("hiddenUserName[]");
            var GrList = '';
            for (var t = 0; t < GroupUsers.length; t++) {
                if (GrList == '')
                    GrList = GroupUsers[t].value;
                else
                    GrList = GrList + "," + GroupUsers[t].value;
            }
            var values = "GroupId=" + GroupNameDropdown + "&GroupUsers=" + GrList
            var msg = '';

            if (GroupNameDropdown == '') {
                msg += "Test";
                $("#GroupNameDropdown").css("border", "1px solid #ff0000");
            } else
                $("#GroupNameDropdown").css("border", "");
            doAjaxPost("POST", "/UserManagement/UpdateUserIntoGroup", values, function (data) {
                hideWaitingPopup("MovableDialog");
                if ($.type(data) == "string") {

                    CloseGroup();
                    $("#success-message").append(data);
                    g.refreshContent();
                } else {
                }
            });
        } else if ($("#existing_group").css("display") == "none") {

            var g = $('#user_grid').data("ejGrid");
            var isValid = $('.new_group_form').valid();
            if (isValid) {
                doAjaxPost("POST", "/Group/CheckGroupname", { GroupName: $("#GroupName").val() }, function (data) {
                    if (data.toLowerCase() != "true") {
                        var GroupName = $("#GroupName").val();
                        var GroupColor = "";
                        var GroupDescription = $("#GroupDescription").val();
                        var GroupUsers = document.getElementsByName("hiddenUserName[]");
                        var GrList = '';
                        for (var t = 0; t < GroupUsers.length; t++) {
                            if (GrList == '')
                                GrList = GroupUsers[t].value;
                            else
                                GrList = GrList + "," + GroupUsers[t].value;
                        }
                        var values = "GroupName=" + GroupName + "&GroupColor=" + GroupColor + "&GroupDescription=" + GroupDescription + "&GroupUsers=" + GrList
                        doAjaxPost("POST", "/UserManagement/SaveUserIntoGroup", values, function (data) {
                            if ($.type(data) == "object") {
                                hideWaitingPopup("MovableDialog");
                                if (data.Data.status) {

                                    CloseGroup();
                                    $("#existing_group").show();
                                    $("#new_group").hide();
                                    g.refreshContent();
                                } else {

                                }
                            }
                        });
                    } else {
                        hideWaitingPopup("MovableDialog");
                        $("#group_name_validation").html("Group already exists with this name").css("display", "block");
                        $("#group_name_validation").closest("div").addClass("has-error");
                    }
                });
            }
            else {
                hideWaitingPopup("MovableDialog");
            }
        }
    });
    $(document).on("click", ".delete_class", function () {
        $(this).parent("li").addClass("Isdelete");
        $("#singleuser_delete_confirmation").ejDialog("open");
    });
});



function EmptyFile() {
    $("#grid_nodata_validation").css("display", "block");
}

function editUser(fulldata) {

    var specficuserdetails = fulldata;
    $("#user_name").val(specficuserdetails.UserName);
    $("#user_email").val(specficuserdetails.Email);
    var dtObj = new Date(parseInt((specficuserdetails.ModifiedDate).substring(6, ((specficuserdetails.ModifiedDate).length) - 2)));
    var formattedString = DateCustomFormat(window.dateFormat + " hh:mm:ss", dtObj);
    formattedString += (dtObj.getHours() >= 12) ? " PM" : " AM";
    $("#LastModified").html(formattedString);
    $("#profile-picture1").attr('src', "/UserManagement/Avatar?Username=" + specficuserdetails.UserName + "&ImageSize=150");
    $("#upload-picture").attr("data-filename", specficuserdetails.Avatar.replace("Content//images//ProfilePictures//" + specficuserdetails.UserName + "//", ""));

    if (fulldata.FirstName != null && fulldata.FirstName != "") {
        $("#user_firstname").val(fulldata.FirstName);
    }
    else {
        $("#user_firstname").val(specficuserdetails.FullName);
    }
    if (fulldata.LastName != null && fulldata.LastName != "") {
        $("#user_lastname").val(fulldata.LastName);
    }
    if (fulldata.ContactNumber != null && fulldata.ContactNumber != "") {
        $("#contact_no").val(fulldata.ContactNumber);
    }
}

function fnOnUserGridLoad(args) {
    args.model.dataSource.adaptor = new ej.UrlAdaptor();
    args.model.enableTouch = false;
}

function fnUserRowSelected(args) {
    var usergrid = $('#user_grid').data("ejGrid");
    var selectedUsers = usergrid.getSelectedRecords();

    if (usergrid.getSelectedRecords().length == 1) {
        jQuery.each(selectedUsers, function (index, record) {
            if (record.UserName.toLowerCase() == $(".MakeFlyDeleteUsers").attr("data-value").toLowerCase() || record.IsActiveDirectoryUser) {
                $('#AddUserInGroup').removeClass("hide").addClass("show");
                $(".MakeFlyDeleteUsers").css("display", "none");
            }
            else {
                $('#AddUserInGroup').removeClass("hide").addClass("show");
                $(".MakeFlyDeleteUsers").css("display", "block");
            }
        });
    }
    else if (usergrid.getSelectedRecords().length > 1) {
        $('#AddUserInGroup').removeClass("hide").addClass("show");
        $(".MakeFlyDeleteUsers").css("display", "none");
        jQuery.each(selectedUsers, function (index, record) {
            if (!record.IsActiveDirectoryUser && record.UserName.toLowerCase() != $(".MakeFlyDeleteUsers").attr("data-value").toLowerCase()) {
                $(".MakeFlyDeleteUsers").css("display", "block");
                return false;
            }
        });
    }
    else {
        $('#AddUserInGroup').removeClass("show").addClass("hide");
    }
}

function fnUserRecordClick(args) {
    var checkbox = args.row.find('.userList-grid-chkbx');
    checkbox.prop("checked", !checkbox.prop("checked"));
}

function fnOnUserGridActionBegin(args) {
    isFirstRequest = true;
    var searchValue = $("#searchUsers").val();
    this.model.query._params.push({ key: "searchKey", value: searchValue });
    var filerSettings = [], i;

    if (args.model.filterSettings.filteredColumns.length > 0) {
        for (i = 0; i < args.model.filterSettings.filteredColumns.length; i++) {
            var column = args.model.filterSettings.filteredColumns[i];
            filerSettings.push({ 'PropertyName': column.field, 'FilterType': column.operator, 'FilterKey': column.value });
        }

        this.model.query._params.push({ key: "filterCollection", value: filerSettings });
    }
}

function fnOnUserGridActionComplete(args) {
    if (args.model.currentViewData.length == 0) {
        rowBound(38);
    }
    var userCount = args.model.pageSettings.totalRecordsCount;
    $("#total_record_count").html(userCount);
    var usergrid = $('#user_grid').data("ejGrid");
    if (usergrid.getSelectedRecords().length != 0) {
        $('#AddUserInGroup').removeClass("hide").addClass("show");
    }
    else {
        $('#AddUserInGroup').removeClass("show").addClass("hide");
    }
}

function rowBound(height) {
    if (isFirstRequest) {
        isFirstRequest = false;
        refreshFooterPosition(height);
    }
}
function MakeFlyAddInGroup() {
    var container = '<div class="modal fade in" id="PopupContainer">' +
					'<div id="confirmModal" class="modal">' +
					'<div class="modal-dialog">' +
					'<div id="MovableDialog" class="modal-content">' +
					'<div class="modal-header dialog-header-confirm">' +
					'<a href="javascript:void(0);" id="group-Close-button" class="PopupClose"><span class="su su-close"></span></a>' +
					'<h4 class="modal-title PopupTitle"><span class="su su-open Head-icon" style="padding-right:7px;"></span><i class="su su-group"></i>Assign Group</h4><div class="subhead-menu col-xs-12 no-padding">Add the selected users to a Group - <span id="usersCount">' +
                    '</span> user(s) selected</div></div>' +
					'<div class="modal-body col-xs-12 content group_section">' +

					'<form id="new_group" class="col-xs-12 no-padding new_group_form" style="display: none" autocomplete="off">' +
					'<div class="col-xs-12 content no-left-padding no-right-padding paddingbottom15"><div class="col-xs-3 no-padding">Name<span class="Mandatory">*</span></div><div class="col-xs-9"><input id="GroupName" class="form-control" placeholder="Group name" type="text" name="groupname"><span id="group_name_validation" class="group_validation" title="Please enter group name" data-content="" style="display:block; color: #a94442; height: 20px;"></span></div></div>' +
					'<div class="col-xs-12 content no-left-padding no-right-padding"><div class="col-xs-3 no-padding">Description</div><div class="col-xs-9"><textarea id="GroupDescription" class="form-control" maxlength="1024"></textarea><span class="notification">*1024 characters maximum</span></div></div>' +
					'</form>' +

					'<div id="existing_group" class="col-xs-12 no-padding">' +
					'<div class="col-xs-12 no-padding"><div class="col-xs-3 no-padding">Group Name&nbsp;&nbsp;</div><div class="col-xs-9"><select id="GroupNameDropdown" class="form-control selectpicker" data-size="5" title="Select Group" data-live-search="true" data-live-search-placeholder="Search"></select></div></div>' +
					'</div></div><div id="modal-footer" class="modal-footer content" style="display:none;"><h4>Selected user(s)</h4><div id="UsersListToGroup"></div></div><div class="modal-footer dialogFooter content modal-footer-bottom-radius" style="text-align:right;">' +
                    '<input id="NewAddGroup" class="btn btn-default leftAlign" type="button" value="New Group">' +
                    '<input id="ExistingAddGroup" class="btn btn-default leftAlign" type="button" value="Existing Group">' +
                    '<input id="AddExistingUserButton" class="btn btn-primary rightAlign" type="button" value="Add">' +
                    '<input id="CancelExistingUserButton" class="btn btn-link rightAlign PopupClose" type="button" value="Cancel">' +
					'</div></div></div></div></div>' +
					'</div>' +

					'<div class="modal-backdrop fade in" style="z-index: 1035;"></div>';

    $("#base_content_Div").append(container);
    var usergrid = $('#user_grid').data("ejGrid");
    addPlacehoder("#new_group");

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value)
    }, "Please avoid special characters");

    $('.new_group_form').validate({
        errorElement: 'span',
        onkeyup: function (element) { $(element).valid(); },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "groupname": {
                isRequired: true,
                isValidName: true
            }
        },
        highlight: function (element) {
            $(element).closest('div').addClass("has-error");
        },
        unhighlight: function (element) {
            if ($(element).attr('name') == 'groupname') {
                $(element).closest('div').removeClass('has-error');
                $(element).closest('div').find("span").html("");
            }
        },
        errorPlacement: function (error, element) {
            $(element).closest('div').find("span").html(error.html());
        },
        messages: {
            "groupname": {
                isRequired: "Please enter group name"
            }
        }

    });
    var selectedUsers = usergrid.getSelectedRecords();
    var HiddenGroupId = $("#HiddenGroupId").val().split(',');
    var HiddenGroupName = $("#HiddenGroupName").val().split(',');
    var UserList = "";
    var GroupList = "";
    $("#usersCount").html(selectedUsers.length);
    $("#ExistingAddGroup").css("display", "none");
    jQuery.each(selectedUsers, function (index, record) {
        UserList += "<div class='RoleItems'><input type='hidden' name='hiddenUserName[]' value='" + record.UserId + "'>" + record.FirstName + " " + record.LastName + "</div>";
    });
    for (var g = 0; g < HiddenGroupId.length; g++) {
        GroupList += "<option value='" + HiddenGroupId[g] + "'>" + HiddenGroupName[g] + "</option>";
    }
    $("#GroupNameDropdown").append(GroupList);
    $("#GroupNameDropdown").selectpicker("refresh");
    $("#GroupNameDropdown").focus();
    $("#modal-footer").append(UserList);
    $("#confirmModal, .modal").css("display", "block");
    $(".modal-backdrop").fadeIn(function () {
        $(".modal-dialog").fadeIn();
        $(".modal.fade.in").css("display", "block");
    });
}

function MakeFlyDeleteUsers() {
    var userList = "";
    var usergrid = $('#user_grid').data("ejGrid");
    var selectedRecords = usergrid.getSelectedRecords();
    var SingleOrMultiple = "";
    if (selectedRecords.length > 1)
        SingleOrMultiple = "s";

    jQuery.each(selectedRecords, function (index, record) {
        if (record.UserName.toLowerCase() != $(".MakeFlyDeleteUsers").attr("data-value").toLowerCase() && !record.IsActiveDirectoryUser) {
            if (userList == "")
                userList = record.UserName;
            else
                userList = userList + "," + record.UserName;
        }
    });
    var values = "Users=" + userList;
    doAjaxPost("POST", "/UserManagement/DeleteFromUserList", values, function (data) {
        parent.messageBox("su-open", "Delete User(s)", "User(s) has been deleted successfully.", "success", function () {
            parent.onCloseMessageBox();
        });
        $("#user_delete_confirmation").ejDialog("close");
        onConfirmDeleteUser(selectedRecords.length);
    }, function () {
        parent.messageBox("su-open", "Delete User(s)", "Failed to delete user(s), please try again later.", "error", function () {
            parent.onCloseMessageBox();
        })
    });
}

function onConfirmDeleteUser(count) {
    var usergrid = $('#user_grid').data("ejGrid");
    var currentPage = usergrid.model.pageSettings.currentPage;
    var pageSize = usergrid.model.pageSettings.pageSize;
    var totalRecordsCount = usergrid.model.pageSettings.totalRecordsCount;
    var lastPageRecordCount = usergrid.model.pageSettings.totalRecordsCount % usergrid.model.pageSettings.pageSize;
    if (lastPageRecordCount != 0 && lastPageRecordCount <= count) {

        usergrid.model.pageSettings.currentPage = currentPage - 1;
    }
    usergrid.refreshContent();
}


function returntoUserPage() {
    window.location.href = "/administration/user-management/users";
}

function CloseGroup() {
    parent.$(".modal,.modal-backdrop").remove();
}

function onUserAddDialogClose() {
    $("#user_add_dialog").ejDialog("close");
}

function onUserAddDialogOpen() {
    $(".dropdown").removeClass("open");
    $("#user_add_dialog").ejDialog("open");
    $(".e-dialog-icon").attr("title", "Close")
}

function onDeleteDialogClose() {
    $("#user_delete_confirmation").ejDialog("close");
}

function onDeleteDialogOpen() {
    $("#user_delete_confirmation").ejDialog("open");
}

function onSingleDeleteDialogClose() {
    $("#singleuser_delete_confirmation").ejDialog("close");
}



function SaveUserListFromCSV() {
    $(".user-import-validation").hide();
    $("#grid_validation").css("display", "none");
    showWaitingPopup("page_content_Div");
    var allUserList = $("#AllUserList").val();
    var userNames = "";
    var emailIds = "";
    for (var i = 0; i < $("td.user-name").length; i++) {
        if (userNames == "") {
            userNames = $("td.user-name")[i].textContent;
            emailIds = $("td.email-id")[i].textContent;
        }
        else {
            userNames = userNames + "," + $("td.user-name")[i].textContent;
            emailIds = emailIds + "," + $("td.email-id")[i].textContent;
        }
    }
    $.ajax({
        type: "POST",
        url: "/UserManagement/SaveSelectedCSVUser",
        data: "&userNames=" + userNames + "&emailIds=" + emailIds + "&AllUSerList=" + allUserList,
        success: function (result) {
            if ($.type(result) == "object" && result.Data.length != 0) {
                var gridObj = $("#Grid").data("ejGrid");
                gridObj.showColumns("Error");
                var nameObj = $(".user-name");
                var emailObj = $(".email-id");
                for (var i = 0; i < result.Data.length; i++) {
                    for (var j = 0; j < nameObj.length; j++) {
                        if (result.Data[i].UserName.toLowerCase() == $(nameObj[j]).text().toLowerCase() && result.Data[i].Email.toLowerCase() == $(emailObj[j]).text().toLowerCase() && result.Data[i].DisplayMessage != "") {
                            var obj = $(nameObj[j]).siblings(":last").find("span").show();
                            obj.attr("title", result.Data[i].DisplayMessage);
                        }
                    }
                    for (var k = 0; k < emailObj.length; k++) {
                        if (result.Data[i].Email.toLowerCase() == $(emailObj[k]).text().toLowerCase() && result.Data[i].UserName.toLowerCase() == $(nameObj[k]).text().toLowerCase() && result.Data[i].DisplayMessage != "") {
                            var obj = $(emailObj[k]).siblings(":last").find("span").show();
                            obj.attr("title", result.Data[i].DisplayMessage);
                        }
                    }
                }
                hideWaitingPopup("page_content_Div");
                $("#grid_error_validation").css("display", "block");
                $("#SaveButtonClick").attr("disabled", "disabled");
            }
            else {
                $("#Grid").ejGrid("option", { dataSource: [] });
                $("#grid_validation").css("display", "block");
                $("#SaveButtonClick").attr("disabled", "disabled");
                hideWaitingPopup("page_content_Div");
            }
        }
    });
}

$(document).on("click", ".option-icon", function () {
    if ($(this).attr("data-content") == $(".MakeFlyDeleteUsers").attr("data-value")) {
        $(".delete_class").parent().css("display", "none");
    }
    else {
        $(".delete_class").parent().css("display", "block");
    }
});

function deleteSingleUser() {
    var userId = $(".Isdelete").attr("data-content");
    var usergrid = $('#user_grid').data("ejGrid");
    doAjaxPost("POST", "/UserManagement/DeleteSingleFromUserList", "UserId=" + userId, function (data) {
        if (data.toLowerCase() == "true") {
            parent.messageBox("su-open", "Delete User", "User has been deleted successfully.", "success", function () {

                parent.onCloseMessageBox();
            });
            onConfirmDeleteUser("1");
            $("#singleuser_delete_confirmation").ejDialog("close");
        } else {
            parent.messageBox("su-open", "Delete User", "Failed to delete user, please try again later.", "error", function () {

                parent.onCloseMessageBox();
            });
        }
    });
}

function HasWhiteSpace(value) {
    if (/\s/g.test(value)) {
        return false;
    }
    else {
        return true;
    }
}

$(document).on("click", "#triggerFile,#filename", function () {
    $("#filename").trigger("focus");
    $("#grid_validation_messages span").css("display", "none");
    $("#csvfile").trigger("click");
});

$(document).on("change", "#csvfile", function (e) {
    var value = $(this).val();
    $("#filename").val(value);
});


$(document).on("keydown", "#searchUsers", function (e) {
    if (e.keyCode == "13") {
        var gridObj = $("#user_grid").data("ejGrid");
        gridObj.refreshContent();
    }
});

$(document).on("click", ".search-user", function () {
    var gridObj = $("#user_grid").data("ejGrid");
    gridObj.refreshContent();
});
