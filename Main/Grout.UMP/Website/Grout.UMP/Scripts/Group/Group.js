
var gridObj;
var selectedgroupIdValues = [];
var selectedActivedirectorygroupIdValues = [];

function fnCreateGrid(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange });
}
function dataBound(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange });
    this.model.indexes = {}; /* Additional property*/
}
function refreshTemplate(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange });
}
function actionbegin(args) {
    //Stores the selected index on paging starts.
    if (args.requestType == "paging" || args.requestType == "sorting") {
        //if (this.selectedRowsIndexes.length > 0)
        //    this.model.indexes[args.previousPage] = this.selectedRowsIndexes.slice(0, 20);
    }
}
function fnActionComplete(args) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    $("#checkboxHeader").ejCheckBox({ "change": headCheckboxOnChange, checked: false });

    if (args.requestType == "paging" || args.requestType == "sorting" || args.requestType == "refresh" || args.requestType == "filtering") {
        var hc = $("#checkboxHeader").ejCheckBox("model.checked");
         
            for (var i = 0; i < $("#Grid .checkboxRow").length; i++) {
                var rowUniId = $("#Grid .checkboxRow").eq(i).attr("data-checked-id");
                if ($.inArray(rowUniId, selectedgroupIdValues) != -1) {
                    $("#Grid .checkboxRow#rowCheck" + rowUniId).ejCheckBox("model.checked", true);
                    this.selectRows($($("#Grid .checkboxRow#rowCheck" + rowUniId).closest("td").parent()).index());
                }

                
            }
        
    }
}
function headCheckboxOnChange(e) {
    $("#Grid .checkboxRow").ejCheckBox({ "change": checkboxOnChange });
    gridObj = $("#Grid").data("ejGrid");
    var pager = gridObj.model.pageSettings;
    if ($("#checkboxHeader").is(':checked')) {
        $(".checkboxRow").ejCheckBox({ "checked": true });
        gridObj.multiSelectCtrlRequest = true;
        gridObj.selectRows(0, $(".checkboxRow").length);
        var ind = gridObj.model.indexes[pager.currentPage] = [];
        var j = $("#Grid div.e-gridcontent table.e-table tr").length;
        while (--j != -1) {
            var curGroupId = $("#Grid .checkboxRow").eq(j).closest("tr").find("td.group-id").text();
            if ($.inArray(curGroupId, selectedgroupIdValues)==-1) {
                selectedgroupIdValues.push($("#Grid .checkboxRow").eq(j).closest("tr").find("td.group-id").text());
                selectedActivedirectorygroupIdValues.push($("#Grid .checkboxRow").eq(j).closest("tr").find("td.activedirectory-group-id").text());
            }
            ind.push($("#Grid .checkboxRow").eq(j).closest("tr").find("td.group-id").text());
        }
        gridObj.model.indexes[pager.currentPage] = ind;
    }
    else {
        $(".checkboxRow").ejCheckBox({ "checked": false });
        gridObj.clearSelection();
        gridObj.model.indexes[pager.currentPage] = [];
        var j = $("#Grid div.e-gridcontent table.e-table tr").length;
        while (--j != -1) {
            selectedgroupIdValues.splice(selectedgroupIdValues.indexOf($("#Grid .checkboxRow").eq(j).closest("tr").find("td.group-id").text()), 1);
            selectedActivedirectorygroupIdValues.splice(selectedActivedirectorygroupIdValues.indexOf($("#Grid .checkboxRow").eq(j).closest("tr").find("td.activedirectory-group-id").text()), 1);
        }
    }
    enablesyncbutton();
}

function checkboxOnChange(e) {
    gridObj = $("#Grid").data("ejGrid");

    var pager = gridObj.model.pageSettings;

    var rowCheck = $(".checkboxRow:checked"), cp = gridObj.model.pageSettings.currentPage;
    gridObj.model.indexes[cp] = gridObj.model.indexes[cp] || [];
    var ind = gridObj.model.indexes[cp], row = this.element.closest("tr").find("td.group-id").text();
    if (this.model.checked) {
        ind.push(row);
        if ($.inArray(this.element.closest("tr").find("td.group-id").text(), selectedgroupIdValues)==-1) {
            selectedActivedirectorygroupIdValues.push(this.element.closest("tr").find("td.activedirectory-group-id").text());
            selectedgroupIdValues.push(this.element.closest("tr").find("td.group-id").text());
        }
    } else {
        ind.splice(ind.indexOf(row), 1);
        selectedActivedirectorygroupIdValues.splice(selectedActivedirectorygroupIdValues.indexOf(this.element.closest("tr").find("td.activedirectory-group-id").text()), 1);
        selectedgroupIdValues.splice(selectedgroupIdValues.indexOf(this.element.closest("tr").find("td.group-id").text()), 1);
    }
    gridObj.model.indexes[pager.currentPage] = ind;

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
    enablesyncbutton();
}
function enablesyncbutton() {
    var groupgrid = $('#Grid').data("ejGrid");
    var selectedData = 0;
    var pager = groupgrid.model.pageSettings;
    for (var t = 1; t <= pager.totalPages; t++) {
        if (typeof groupgrid.model.indexes[t] != "undefined") {
            if (groupgrid.model.indexes[t].length > 0)
                selectedData++;
        }
    }
    if (selectedData > 0) {
        $("#Syncbutton").removeClass("TabInvisible").addClass("VisibleTabContainer");
    }
    else {
        $("#Syncbutton").removeClass("VisibleTabContainer").addClass("TabInvisible");
    }
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
    enablesyncbutton();
}

$(document).on('click', "#AddGroup", function () {
    var groupName = $("#GroupName").val().trim();
    $("#group_name").removeClass("has-error");
    var isValid = $('.group_form').valid();
    
    if (isValid) {
            parent.$("#NewGroupArea_wrapper").ejWaitingPopup("show");
            doAjaxPost("POST", "/Group/CheckGroupname", { GroupName: groupName }, function (data) {
                if (data != "True") {
                    var values = "groupName=" + groupName + "&groupDescription=" + $("#GroupDescription").val() + "&groupColor=";
                    doAjaxPost("POST", "/Group/AddGroup", values, function(data, result) {
                        if (data != "null") {
                            parent.$("#NewGroupArea").ejDialog("close");
                            parent.$("#NewGroupArea_wrapper").ejWaitingPopup("hide");
                            parent.messageBox("su-group-1", "Add Group", "New group has been created successfully.", "success", function () {
                                var count = parent.$("#GroupCountText").val();
                                var currentVal = parseInt(count) + 1;
                                parent.$("#GroupCount").html(currentVal);
                                parent.$("#GroupCountText").val(currentVal);
                                var gridObj = parent.$("#Grid").data("ejGrid");
                                RefreshCurrentDataOfGroupList(gridObj);
                                parent.onCloseMessageBox();
                            });
                        } else {
                        }
                    });
                } else {
                    parent.$("#NewGroupArea_wrapper").ejWaitingPopup("hide");
                    $("#group_name").addClass("has-error");
                    $(".ErrorMessage").html("Group already exists with this name");
                }
            });
        }
    });

    //Events
   

    $(document).on('click', '.delete', function (e) {
        var groupId = $(this).attr('data-groupid');
        var groupName = $(this).attr('data-groupname');
        eDialog = $("#DeleteGroupArea").data("ejDialog");
        eDialog.open();
        $("#DeleteGroupIframe").attr("src", "/group/DeleteGroupView?group=" + groupId+"&name="+groupName);
        $("#DeleteGroupArea_wrapper").ejWaitingPopup("show");
    });

   
   

    function RefreshCurrentDataOfGroupList(gridObj) {
        gridObj.refreshContent();
    }

    $(document).ready(function () {

        $.validator.addMethod("isValidName", function (value, element) {
            return IsValidName("name", value)
        }, "Please avoid special characters");

        $.validator.addMethod("isRequired", function (value, element) {
            return !isEmptyOrWhitespace(value);
        }, "Please enter the name");

        $('.group_form').validate({
            errorElement: 'span',
            onkeyup: function (element,event) { if (event.keyCode != 9) $(element).valid(); else true;},
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
                $(element).closest('div').find("span").html("");
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

        var isFirstRequest = false;
        addPlacehoder("#group_name");
        $(document).on("click", "#NewGroupButton", function () {
            eDialog = $("#NewGroupArea").data("ejDialog");
            eDialog.open();
            $("#newGroupIframe").attr("src", "/group/AddGroupView");
            $("#NewGroupArea_wrapper").ejWaitingPopup("show");
        });

        $(document).on("click", "#DeleteGroup", function () {
            var hiddengroupId=$("#hiddenId").val();
            doAjaxPost("POST", "/Group/DeleteGroup", { "GroupId": hiddengroupId },
                function (data) {
                    if (data == "True") {
                        var gridObj = parent.$("#Grid").ejGrid("instance");
                        var gridName = parent.$("#Grid").attr("data-gridName").toLowerCase();
                        var count = parent.$("#GroupCountText").val();
                        var currentVal = parseInt(count) - 1;
                        parent.$("#GroupCount").html(currentVal);
                        parent.$("#GroupCountText").val(currentVal);
                        eDialog = parent.$("#DeleteGroupArea").data("ejDialog");
                        eDialog.close();
                        if (gridName == "editgroup")
                            parent.location.href = "/administration/user-management/groups";
                        else
                            onSuccessDeleteUser(gridObj);                         
                    }
                }
            );
        });
        $(document).on("click", "#SyncronizeGroup", function () {
            var groupAdIds = "";
            var groupIds = "";
            showWaitingPopup("page_content_Div");


            var selectedGroupList = selectedActivedirectorygroupIdValues.length;
            var selectedGroupIdList = selectedgroupIdValues.length;
            for (var i = 0; i < selectedGroupList; i++) {
                if (groupAdIds == "") {
                    groupIds = encodeURIComponent(selectedgroupIdValues[i]);
                    groupAdIds = encodeURIComponent(selectedActivedirectorygroupIdValues[i]);
                } else {
                    groupIds = groupIds + "," + encodeURIComponent(selectedgroupIdValues[i]);
                    groupAdIds = groupAdIds + "," + encodeURIComponent(selectedActivedirectorygroupIdValues[i]);
                }
            }

            $.ajax({
                type: "POST",
                url: "/Group/SynchronizeActiveDirectoryGroup",
                data: "&groupAdIds=" + groupAdIds + "&groupIds=" + groupIds,
                success: function (result) {
                    var output = result;
                    var message = "";
                    if (result.Data.status == 'true') {
                        hideWaitingPopup("page_content_Div");
                        $("#SyncronizeGroup").attr("disabled", true);
                        if (output.Data.ModifiedGroupCount > 0)
                            message += output.Data.ModifiedGroupCount + " group(s) details are modified. ";

                        if (output.Data.DeletedGroupCount>0)
                            message += output.Data.DeletedGroupCount + " group(s) are deleted.";
                        
                        parent.messageBox("su-group-1", "Synchronize Group", " Synchronizing groups with active directory has been successful. <br/>" + message, "success", function () {
                            parent.onCloseMessageBox();
                            window.location.href = "/administration/user-management/groups";
                        });
                    } else {
                        parent.messageBox("su-group-1", "Synchronize Group", "Error occured while synchronizing the group.", "error", function () {
                            parent.onCloseMessageBox();
                            window.location.href = "/administration/user-management/groups";
                        });
                    }
                }
            });
        });
		});

	function onSuccessDeleteUser(gridObj){
	var currentPage= gridObj.model.pageSettings.currentPage;
	var pageSize= gridObj.model.pageSettings.pageSize;
	var totalRecordsCount=gridObj.model.pageSettings.totalRecordsCount;
	var lastPageRecordCount=gridObj.model.pageSettings.totalRecordsCount%gridObj.model.pageSettings.pageSize;
	
	if (lastPageRecordCount!=0&&lastPageRecordCount<=1) {

	gridObj.model.pageSettings.currentPage =currentPage- 1;
	}
	gridObj.refreshContent()
	}

    function fnOnGroupGridActionBegin(args) {
	isFirstRequest=true;
    var searchValue = $("#searchGroups").val();
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
function rowBound(height){
	if(isFirstRequest){
		isFirstRequest=false;
		refreshFooterPosition(height);
		 if (location.pathname.toLowerCase() === "/" || location.pathname.toLowerCase() === "/reports") {
			refreshScroller();
         }
	}
}

$(document).on("keydown", "#searchGroups", function (e) {
    if (e.keyCode == "13") {
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    }
});

$(document).on("click", ".search-group", function () {
    var gridObj = $("#Grid").data("ejGrid");
    gridObj.refreshContent();
});
