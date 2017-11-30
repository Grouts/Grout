var gridObj;
function fnCreate(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange });
}
function fnActionComplete(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange, checked: false });
}

function SaveActiveDirectoryUsers() {

    var userNames = "";
    var fullnames = "";
    var lastnames = "";
    var firstnames = "";
    var userIds = "";
    var emailIds = "";
    var mobileNos = "";

    var selectedUSerList = $("td.user-name.e-active").length;
    for (var i = 0; i < selectedUSerList; i++) {
        if (userNames == "") {
            userNames = encodeURIComponent($("td.user-name.e-active")[i].textContent);
            fullnames = $("td.full-name.e-active")[i].textContent;
            lastnames = $("td.last-name.e-active")[i].textContent;
            userIds = $("td.user-id.e-active")[i].textContent;
            firstnames = $("td.first-name.e-active")[i].textContent;
            emailIds = $("td.email-id.e-active")[i].textContent;
            mobileNos = $("td.mobile-no.e-active")[i].textContent;
        }
        else {
            userNames = userNames + "," + encodeURIComponent($("td.user-name.e-active")[i].textContent);
            fullnames = fullnames + "," + $("td.full-name.e-active")[i].textContent;
            lastnames = lastnames + "," + $("td.last-name.e-active")[i].textContent;
            userIds = userIds + "," + $("td.user-id.e-active")[i].textContent;
            firstnames = firstnames + "," + $("td.first-name.e-active")[i].textContent;
            emailIds = emailIds + "," + $("td.email-id.e-active")[i].textContent;
            mobileNos = mobileNos + "," + $("td.mobile-no.e-active")[i].textContent;
        }
    }
    if (selectedUSerList > 0) {
        $(".empty_validation").css("display", "none");
        showWaitingPopup("page_content_Div");
        $.ajax({
            type: "POST",
            url: "/UserManagement/SaveSelectedActiveDirectoryUser",
            data: "userNames=" + userNames + "&fullnames=" + fullnames + "&lastnames=" + lastnames + "&userIds=" + userIds + "&firstnames=" + firstnames + "&emailIds=" + emailIds + "&mobileNos=" + mobileNos,
            success: function (result) {
                if ($.type(result) == "object" && result.Data.length != 0) {
                    var gridObj = $("#Grid").data("ejGrid");
                    gridObj.showColumns("Error");
                    var nameObj = $(".user-name");


                    for (var i = 0; i < result.Data.length; i++) {
                        for (var j = 0; j < nameObj.length; j++) {
                            if (result.Data[i].UserName.toLowerCase() == $(nameObj[j]).text().toLowerCase() && result.Data[i].DisplayMessage != "") {
                                var obj = $(nameObj[j]).siblings(":last").find("span").show();
                                obj.attr("title", result.Data[i].DisplayMessage);
                            }
                        }
                    }

                    hideWaitingPopup("page_content_Div");
                    $(".grid_error_validation").css("display", "block");
                } else {
                    $(".grid_error_validation").css("display", "none");
                    $(".empty_validation").css("display", "none");
                    $("#Grid").ejGrid("option", { dataSource: [] });
                    $(".grid_validation").css("display", "block");
                    $(".userCount").html(selectedUSerList);
                    $(".SaveButtonClick").attr("disabled", "disabled");
                    hideWaitingPopup("page_content_Div");
                }
            }
        });
    } else {
        $(".grid_error_validation").css("display", "none");
        $(".empty_validation").css("display", "block");

    }
}

function SaveActiveDirectoryGroups() {

    var groupNames = "";
    var groupIds = "";
    var desc = "";
    var selectedgroupList = $("td.group-name.e-active").length;
    for (var i = 0; i < selectedgroupList; i++) {
        if (groupNames == "") {
            groupNames = encodeURIComponent($("td.group-name.e-active")[i].textContent);
            desc = encodeURIComponent($("td.desc.e-active")[i].textContent);
            groupIds = $("td.group-id.e-active")[i].textContent;
        }
        else {
            groupNames = groupNames + "," + encodeURIComponent($("td.group-name.e-active")[i].textContent);
            desc = desc + "," + encodeURIComponent($("td.desc.e-active")[i].textContent);
            groupIds = groupIds + "," + $("td.group-id.e-active")[i].textContent;
        }
    }
    if (selectedgroupList > 0) {
        $(".empty_validation").css("display", "none");
        showWaitingPopup("page_content_Div");
        $.ajax({
            type: "POST",
            url: "/Group/SaveSelectedActiveDirectoryGroups",
            data: "groupNames=" + groupNames + "&desc=" + desc + "&groupIds=" + groupIds,
            success: function (result) {
                if ($.type(result) == "object" && result.Data.length != 0) {
                    var gridObj = $("#Grid").data("ejGrid");
                    gridObj.showColumns("Error");
                    var nameObj = $(".group-name");


                    for (var i = 0; i < result.Data.length; i++) {
                        for (var j = 0; j < nameObj.length; j++) {
                            if (result.Data[i].GroupName.toLowerCase() == $(nameObj[j]).text().toLowerCase() && result.Data[i].DisplayMessage != "") {
                                var obj = $(nameObj[j]).siblings(":last").find("span").show();
                                obj.attr("title", result.Data[i].DisplayMessage);
                            }
                        }
                    }

                    hideWaitingPopup("page_content_Div");
                    $(".grid_error_validation").css("display", "block");
                } else {
                    $("#Grid").ejGrid("option", { dataSource: [] });
                    $(".grid_error_validation").css("display", "none");
                    $(".empty_validation").css("display", "none");
                    $(".groupcount").html(selectedgroupList);
                    $(".grid_validation").css("display", "block");
                    $(".SaveButtonClick").attr("disabled", "disabled");
                    hideWaitingPopup("page_content_Div");
                }
            }
        });
    } else {
        $(".grid_error_validation").css("display", "none");
        $(".empty_validation").css("display", "block");

    }
}
function headCheckboxOnChange(e) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    gridObj = $("#Grid").data("ejGrid");
    if ($("#checkboxHeader").is(':checked')) {
        $(".checkboxRow").ejCheckBox({ "checked": true });
        gridObj.multiSelectCtrlRequest = true;
        gridObj.selectRows(0, $(".checkboxRow").length);
    }
    else {
        $(".checkboxRow").ejCheckBox({ "checked": false });
        gridObj.clearSelection();
    }
}

function checkboxOnChange(e) {
    gridObj = $("#Grid").data("ejGrid");
    var rowCheck = $(".checkboxRow:checked");
    if (rowCheck.length == $(".checkboxRow").length)
        $("#checkboxHeader").ejCheckBox({ "checked": true });
    else
        $("#checkboxHeader").ejCheckBox({ "checked": false });

    if (($("#checkboxHeader").is(':checked')) && this.model.checked != true) {
        for (i = 0; i < rowCheck.length; i++) {
            gridObj.multiSelectCtrlRequest = true;
            gridObj.selectRows($(rowCheck[i]).parents("tr").index());
        }
    }
    if (this.model.checked == false) {
        $("#checkboxHeader").ejCheckBox({ "checked": false });
    }
    gridObj.multiSelectCtrlRequest = true;
}

function recordClick(args) {
    gridObj = $("#Grid").data("ejGrid");
    var isChecked = args.row.find(".checkboxRow").is(":checked");
    if (isChecked) {
        gridObj.multiSelectCtrlRequest = true;
        if (gridObj.selectedRowsIndexes.indexOf(args.row.index()) < 0) {
            gridObj.selectRows(args.row.index());
        }
    }
    else {
        gridObj.multiSelectCtrlRequest = true;
        gridObj.clearSelection(args.row.index());
    }
}