$(document).ready(function () {
    parent.$("#DeleteGroupArea_wrapper").ejWaitingPopup("hide");
    $(document).on("click", ".PopupClose", function (e) {
        eDialog = parent.$("#DeleteGroupArea").data("ejDialog");
        eDialog.close();
        parent.$("#userList").selectpicker("refresh");
        parent.$("#DeleteGroupArea iframe").attr("src", "");
    });
    $(document).on("click", "#DeleteUser", function () {
        var hiddenId = $("#hiddenId").val();
        var hiddenName = $("#hiddenName").val();
        var hiddengroupId = $("#hiddengroupId").val();
        doAjaxPost("POST", "/Group/DeleteUserFromGroup", { groupId: hiddengroupId, userId: hiddenId },
                function (data) {
                    if (data == "True") {
                        eDialog = parent.$("#DeleteGroupArea").data("ejDialog");
                        eDialog.close();
                        parent.$("#userList").append("<option value='" + hiddenId + "'>" + hiddenName + "</option>");
                        parent.$("#userList").selectpicker("refresh");
                        var gridObj = parent.$("#Grid").ejGrid("instance");
                        var currentPage = gridObj.model.pageSettings.currentPage;
                        var pageSize = gridObj.model.pageSettings.pageSize;
                        var totalRecordsCount = gridObj.model.pageSettings.totalRecordsCount;
                        var lastPageRecordCount = gridObj.model.pageSettings.totalRecordsCount % gridObj.model.pageSettings.pageSize;

                        if (lastPageRecordCount != 0 && lastPageRecordCount <= 1) {

                            gridObj.model.pageSettings.currentPage = currentPage - 1;
                        }
                        gridObj.refreshContent()
                    } else {
                    }
                }
            );
    });
    $(document).on("click", "#DeleteGroup", function () {
        var hiddengroupId = $("#hiddenId").val();
        doAjaxPost("POST", "/Group/DeleteGroup", { "GroupId": hiddengroupId },
                function (data) {
                    if (data == "True") {
                        parent.location.href = "/administration/user-management/groups";
                    }
                }
            );
    });
});