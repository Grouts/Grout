$(document).ready(function () {
	var isFirstRequest=false;
    $("#permission_delete_confirmation").ejDialog({
        width: "400px",
        showOnInit: false,
        showHeader: false,
        allowDraggable: false,
        enableResize: false,
        height: "187px",
        title: "Delete item",
        enableModal: true,
        close: "onPermissionDialogClose"
    });
    $("#permission_delete_confirmation_wrapper").ejWaitingPopup();

    $("#AddPermissionButton,#AddPermissionButtonTop").click(function () {
        eDialog = $("#AddPermission").data("ejDialog");
        eDialog.open();
        var groupId = $("#groupIdHidden").val();
        $("#AddPermissionIframe").attr("src", "/permission/AddGroupPermissionView?groupId=" + groupId);
        $("#AddPermission_wrapper").ejWaitingPopup("show");
    });

    $(document).on("click", ".PopupClose", function (e) {
        eDialog = parent.$("#AddPermission").data("ejDialog");
        eDialog.close();
        parent.$("#AddPermission iframe").attr("src", "");
    });

    $("#AccessSelection").change(function () {
        $(".SuccessMessage, .ErrorMessage").html("");
        var accessValue = $(this).val();
        var options = "";
        $.ajax({
            type: "POST",
            url: "/permission/GetPermissionEntity",
            data: { accessMode: accessValue },
            async: false,
            success: function (result, data) {
                var entities = JSON.parse(result);
                for (var t = 0; t < entities.length; t++) {
                    options += '<option data-entity-type=' + entities[t].Type + ' value=' + entities[t].Id + '>' + entities[t].Name + '</option>';
                }
                $("#EntitySelection").html(options);
                $("#EntitySelection").attr("disabled", false);
                $("#EntitySelection").selectpicker("refresh");
                $("#ScopeSelection").html('<option value=""></option>');
                $("#ScopeSelection").attr("disabled", true);
                $("#SavePermission").attr("disabled", false);
                $("#ScopeSelection").selectpicker("refresh");
            }
        });
    });

    $("#EntitySelection").change(function () {
        $(".SuccessMessage, .ErrorMessage").html("");
        var entityType = $('option:selected', this).attr("data-entity-type");
        var entityModId = $(this).val();

        if (entityType == 2 || entityType == 0) {

            $.ajax({
                type: "POST",
                url: "/permission/GetItemScope",
                data: { entityId: entityModId },
                async: false,
                success: function (result, data) {
                    var items = JSON.parse(result);
                    if ($.isEmptyObject(items)) {
                        $("#SavePermission").attr("disabled", "disabled")
                    }
                    else {
                        $("#SavePermission").removeAttr("disabled")
                    }
                    var options = "";
                    for (var t = 0; t < items.length; t++) {
                        options += '<option value=' + items[t].Id + '>' + items[t].Name + '</option>';
                    }
                    $("#ScopeSelection").html(options);
                    $("#ScopeSelection").attr("disabled", false);
                    $("#ScopeSelection").selectpicker("refresh");

                }
            });
        }
        else {
            $("#ScopeSelection").html('<option value=""></option>');
            $("#ScopeSelection").attr("disabled", true);
            $("#ScopeSelection").selectpicker("refresh");
            $("#SavePermission").attr("disabled", false);
        }
    });

    $("#ScopeSelection").change(function () {
        $(".SuccessMessage, .ErrorMessage").html("");
        $("#SavePermission").attr("disabled", false);
    });

    $("#AddPermissionController").on("click", "#SavePermission", function () {
        $(".SuccessMessage, .ErrorMessage").html("");
        var accessMode = $("#AccessSelection").val();
        var entityModel = $("#EntitySelection").val();
        var scopeValue = $("#ScopeSelection").val();
        var group = $("#groupIdHidden").val();
        var entityType = $('option:selected', '#EntitySelection').attr("data-entity-type");
        parent.$("#AddPermission_wrapper").ejWaitingPopup("show");
        $.ajax({
            type: "POST",
            url: "/permission/addnewgrouppermission",
            data: { mode: accessMode, entity: entityModel, scopeId: scopeValue, groupId: group },
            async: false,
            success: function (result, data) {
                if (result.toLowerCase() == "true") {
                    var gridObj = parent.$("#Grid").ejGrid("instance");
                    gridObj.refreshContent();
                    var selectedEntity = $("#EntitySelection option:selected").text().replace("Specific ", "");
                    var message = $("#AccessSelection option:selected").text() + " permission for " + selectedEntity + " ";
                    if (entityType == 0 || entityType == 2) {
                        message += "— " + $("#ScopeSelection option:selected").text() + " ";
                    }
                    message += "has been added successfully.";
                    $(".SuccessMessage").attr("title", message);
                    if (message.length > 110) {
                        message = message.substr(0, 110);
                        message += "...";
                    }
                    $(".SuccessMessage").html(message);
                    $(".ErrorMessage").css("display", "none");
                    $(".SuccessMessage").css("display", "block");
                } else {
                    $(".ErrorMessage").html("Permission Entity already exists");
                    $(".SuccessMessage").css("display", "none");
                    $(".ErrorMessage").css("display", "block");
                }
                parent.$("#AddPermission_wrapper").ejWaitingPopup("hide");
            }
        });
    });

    $("#PermissionAppContainer").on("click", ".deletePermission", function () {
        var permId = $(this).attr("data-permission-id");
        $("#delete_permission").attr("permissionId", permId);
        $("#permission_delete_confirmation").ejDialog("open");
    });

})

$(document).on("click", "#delete_permission", function () {
    $("#permission_delete_confirmation_wrapper").ejWaitingPopup("show");
    var permId = $(this).attr("permissionid");
    $.ajax({
        type: "POST",
        url: "/permission/DeleteGroupPermission",
        data: { permissionId: permId },
        async: false,
        success: function (result, data) {
            if (result.toLowerCase() == "true") {
                var gridObj = parent.$("#Grid").ejGrid("instance");
              
				var currentPage= gridObj.model.pageSettings.currentPage;
				var pageSize= gridObj.model.pageSettings.pageSize;
				var totalRecordsCount=gridObj.model.pageSettings.totalRecordsCount;
				var lastPageRecordCount=gridObj.model.pageSettings.totalRecordsCount%gridObj.model.pageSettings.pageSize;
				
				if (lastPageRecordCount!=0&&lastPageRecordCount<=1) {

					gridObj.model.pageSettings.currentPage =currentPage- 1;
				}
				gridObj.refreshContent()
            }
            $("#permission_delete_confirmation_wrapper").ejWaitingPopup("hide");
            $("#permission_delete_confirmation").ejDialog("close");
        }
    });

});
function fnOnGroupPermissionActionBegin(){
	isFirstRequest=true;
	
}
function rowBound(height){
	if(isFirstRequest){
		isFirstRequest=false;
		refreshFooterPosition(height);
	}
}
function onPermissionDialogClose() {
    $("#permission_delete_confirmation").ejDialog("close");
}