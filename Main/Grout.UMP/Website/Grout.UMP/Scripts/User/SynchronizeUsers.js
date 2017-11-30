var selecteduserIdValues = [];
function fnRecordClick(args) {
    gridObj = $("#user_grid").data("ejGrid");
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

    if (gridObj.getSelectedRecords().length > 0) {
        $('#synchronize-users').removeClass("hide").addClass("show");
    }
    else {
        $('#synchronize-users').removeClass("show").addClass("hide");
    }
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
    $("#user_grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange });

    if (args.requestType == "paging" || args.requestType == "sorting" || args.requestType == "refresh" || args.requestType == "filtering") {
        for (var i = 0; i < $("#user_grid .checkboxRow").length; i++) {
            var rowUniId = $("#user_grid .checkboxRow").eq(i).attr("data-checked-id");
            if ($.inArray(rowUniId, selecteduserIdValues) != -1) {
                $("#user_grid .checkboxRow#rowCheck" + rowUniId).ejCheckBox("model.checked", true);
                this.selectRows($($("#user_grid .checkboxRow#rowCheck" + rowUniId).closest("td").parent()).index());
            }


        }
    }

    var userCount = args.model.pageSettings.totalRecordsCount;
    $("#total_record_count").html(userCount);
    var usergrid = $('#user_grid').data("ejGrid");
    if (usergrid.getSelectedRecords().length != 0) {
        $('#synchronize-users').removeClass("hide").addClass("show");
    }
    else {
        $('#synchronize-users').removeClass("show").addClass("hide");
    }
}

function rowBound(height) {
    if (isFirstRequest) {
        isFirstRequest = false;
        refreshFooterPosition(height);
    }
}

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


function SynchronizeUsers() {
    showWaitingPopup("page_content_Div");
    var userList = "";
    var userIds = "";
    var usergrid = $('#user_grid').data("ejGrid");
    var selectedRecords = selecteduserIdValues;
    for (var i = 0; i < selectedRecords.length; i++) {
        if (userIds == "") {
            userIds = encodeURIComponent(selectedRecords[i]);
        } else {
            userIds = userIds + "," + encodeURIComponent(selectedRecords[i]);
        }
    }
    $.ajax({
        type: "POST",
        url: "/UserManagement/SynchronizeSelectedUsers",
        data: { usersList: userIds },
        success: function (data) {
            parent.messageBox("su-open", "Active Directory Synchronization", data.result, "success", function () {
                parent.onCloseMessageBox();
                parent.window.location.href = '/administration/user-management/users';
            });
            hideWaitingPopup("page_content_Div");
        },
        error: function () {
            parent.messageBox("su-open", "Active Directory Synchronization", "Synchronizing users with active directory has been failed.", "error", function() {
                parent.onCloseMessageBox();
            });
            hideWaitingPopup("page_content_Div");
        }
    });
}
function refreshTemplate(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange });
}
function headCheckboxOnChange(e) {
    $("#user_grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    gridObj = $("#user_grid").data("ejGrid");
    var pager = gridObj.model.pageSettings;
    if ($("#checkboxHeader").is(':checked')) {
        $(".checkboxRow").ejCheckBox({ "checked": true });
        gridObj.multiSelectCtrlRequest = true;
        gridObj.selectRows(0, $(".checkboxRow").length);

        var ind = gridObj.model.indexes[pager.currentPage] = [];
        var j = $("#user_grid div.e-gridcontent table.e-table tr").length;
        while (--j != -1) {
            var curUserId = $("#user_grid .checkboxRow").eq(j).closest("tr").find("td.user-id").text();
            if ($.inArray(curUserId, selecteduserIdValues)==-1) {
                selecteduserIdValues.push(curUserId);
            }
            ind.push(curUserId);
        }
        gridObj.model.indexes[pager.currentPage] = ind;

        if (gridObj.getSelectedRecords().length > 0) {
            $('#synchronize-users').removeClass("hide").addClass("show");
        }
    }
    else {
        $(".checkboxRow").ejCheckBox({ "checked": false });
        $('#synchronize-users').removeClass("show").addClass("hide");
        gridObj.clearSelection();

        gridObj.model.indexes[pager.currentPage] = [];
        var j = $("#user_grid div.e-gridcontent table.e-table tr").length;
        while (--j != -1) {
            selecteduserIdValues.splice(selecteduserIdValues.indexOf($("#user_grid .checkboxRow").eq(j).closest("tr").find("td.user-id").text()), 1);
        }
    }
}

function checkboxOnChange(e) {
    gridObj = $("#user_grid").data("ejGrid");

    var pager = gridObj.model.pageSettings;

    var rowCheck = $(".checkboxRow:checked"), cp = gridObj.model.pageSettings.currentPage;
    gridObj.model.indexes[cp] = gridObj.model.indexes[cp] || [];
    var ind = gridObj.model.indexes[cp], row = this.element.closest("tr").find("td.user-id").text();
    if (this.model.checked) {
        ind.push(row);
        if ($.inArray(this.element.closest("tr").find("td.user-id").text(), selecteduserIdValues)==-1) {
            selecteduserIdValues.push(this.element.closest("tr").find("td.user-id").text());
        }
    } else {
        ind.splice(ind.indexOf(row), 1);
        selecteduserIdValues.splice(selecteduserIdValues.indexOf(this.element.closest("tr").find("td.user-id").text()), 1);
    }
    gridObj.model.indexes[pager.currentPage] = ind;

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