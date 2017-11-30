$(document).ready(function () {

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value)
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $('.group_form').validate({
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
            $(element).closest('div').removeClass('has-error');
            $(element).closest('div').next("div").find("span").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('div').next("div").find("span").html(error.html());
        },
        messages: {
            "groupname": {
                isRequired: "Please enter group name"
            }
        }

    });

    $("#userList").selectpicker();
    addPlacehoder("#searchArea");
    addPlacehoder("#group_name");
    $(document).on("click", "#NewGroupButton", function () {
        eDialog = $("#NewGroupArea").data("ejDialog");
        eDialog.open();
        $("#newGroupIframe").attr("src", "/group/AddGroupView");
        $("#NewGroupArea_wrapper").ejWaitingPopup("show");
    });

    $(document).on("change", "#userList", function () {
        var userVal = $("#userList").val();
        if (userVal != null)
            $("#UserSaveButton").attr("disabled", false);
        else
            $("#UserSaveButton").attr("disabled", true);
    });

    $(document).on('click', '#GroupTItleContainerSaveButton', function (e) {
        $("#group_edit_confirmation").html("");
        var groupName = $("#GroupInfoName").val().trim();
        $("#GroupInfoName").closest("div").removeClass("has-error");
        var isValid = $('.group_form').valid();

        if (isValid) {
            $("#invalid_Groupname").css("display", "none");
            showWaitingPopup($("#base_container"));
            doAjaxPost("POST", "/Group/SaveGroupSettings", { groupInfo: JSON.stringify({ "GroupName": $('#GroupInfoName').val(), "GroupColor": "", "GroupDescription": $('#GroupInfoDescription').val(), "GroupId": $('#hiddengroupId').val() }) },
                function (data) {
                    hideWaitingPopup($("#base_container"));
                    if (data.status) {
                        messageBox("su-edi", "Edit Group", "Group has been updated successfully.", "success", function () {
                            var scope = angular.element($("#base_container")).scope();
                            scope.$apply(function () {
                                scope.allGroupDetails.GroupName = $('#GroupInfoName').val();
                            });
                            onCloseMessageBox();
                        });
                    } else {
                        if (data.key == "name") {
                            $("#invalid_Groupname").html("Group already exists with this name").css("display", "block");
                            $("#GroupInfoName").closest("div").addClass("has-error");
                        }
                        else
                            $("#group_edit_confirmation").html(data.value);
                    }
                }
            );
        } 
    });



    $(document).on('click', '.delete', function (e) {
        var groupId = $(this).attr('data-groupid');
        var groupName = $(this).attr('data-groupname');
        eDialog = $("#DeleteGroupArea").data("ejDialog");
        eDialog.open();
        $("#DeleteGroupIframe").attr("src", "/group/DeleteGroupView?group=" + groupId + "&name=" + groupName);
        $("#DeleteGroupArea_wrapper").ejWaitingPopup("show");
    });

    $(document).on('click', '.deleteUser', function (e) {
        var userId = $(this).attr('data-userid');
        var userName = $(this).attr('data-username');
        eDialog = $("#DeleteGroupArea").data("ejDialog");
        eDialog.open();
        $("#DeleteGroupIframe").attr("src", "/group/DeleteGroupUserView?userId=" + userId + "&groupId=" + $('#hiddengroupId').val() + "&userName=" + userName);
        $("#DeleteGroupArea_wrapper").ejWaitingPopup("show");
    });

    $(document).on('click', '#UserSaveButton', function (e) {
        doAjaxPost("POST", "/Group/AddUserInGroup", { groupId: $('#hiddengroupId').val(), userId: JSON.stringify($('#userList').val()) },
                   function (data) {
                       if (data == "True") {
                           $("#userList").find('option:selected').remove();
                           $("#userList").selectpicker("refresh");
                           var gridObj = $("#Grid").ejGrid("instance");
                           RefreshGroupUsers($('#hiddengroupId').val(), gridObj);
                       } else {
                       }
                   }
              );
    });
});


function RefreshGroupUsers(groupId, gridObj) {
    gridObj.refreshContent();
}

function fnOnEditGroupActionBegin(args) {
    var searchValue = $("#searchGroupUsers").val();
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

$(document).on("keydown", "#searchGroupUsers", function (e) {
    if (e.keyCode == "13") {
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    }
});

$(document).on("click", ".search-group-users", function () {
    var gridObj = $("#Grid").data("ejGrid");
    gridObj.refreshContent();
});
$(document).on('hide.bs.dropdown', '#PeopleContainer', function (e) {
    if ($("#PeopleContainer").hasClass("valueChanged")) {
        $("#PeopleContainer").removeClass("valueChanged");
        e.preventDefault();
    }
});
$(document).on("change", "#userList", function () {
    $("#PeopleContainer").addClass("valueChanged");
});