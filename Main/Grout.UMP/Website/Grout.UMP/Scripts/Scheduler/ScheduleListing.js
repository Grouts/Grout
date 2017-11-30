$(document).ready(function () {
	var isFirstRequest=false;
	var ScheduleId, ItemId, ItemName;
    $("#schedule_delete_confirmation").ejDialog({
        width: "450px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "282px",
        title: "Delete item",
        showHeader: false,
        enableModal: true,
        close: deleteScheduleDialogClose
    });
	
	$("#PopupContainer").ejDialog({
        allowDraggable: false,
        enableResize: false,
        enableModal: true,
        showHeader: false,
		showOnInit: false,
        close: "onSchedulerDialogClose",
		open: "onSchedulerDialogOpen",
        width: "875px",
        height: "600px"
    });

    $("#PopupContainer_wrapper").ejWaitingPopup();
    $("#schedule_delete_confirmation_wrapper").ejWaitingPopup();

    $(document).on("keydown", "#searchSchedules", function (e) {
        if (e.keyCode == "13") {
            var gridObj = $("#scheduleGrid").data("ejGrid");
            gridObj.search($("#searchSchedules").val());
        }
    });

    $(document).on("click", ".search-schedule", function () {
        var gridObj = $("#scheduleGrid").data("ejGrid");
        gridObj.search($("#searchSchedules").val());
    });

    $(window).resize(function () {
        var gridObj = $("#scheduleGrid").data("ejGrid");
        if (window.innerWidth < 1200) {
            gridObj.model.columns[5].width = 25
            gridObj._columnsWidthCollection[5] = 25;
            gridObj.hideColumns("Last Run");
        }
        else {
            gridObj.model.columns[5].width = 15
            gridObj._columnsWidthCollection[5] = 15;
            gridObj.showColumns("Last Run");
        }
        gridObj.setWidthToColumns();
        refreshFooterPosition();
        var scheduleDialogObj = $("#PopupContainer").data("ejDialog");
        if (scheduleDialogObj.isOpened()) {
            scheduleDialogObj._dialogPosition();
        }
    });
});

function manageSchedule(event) {
    var currentSelection = event.cell.context.className;
    var scheduleId = "";
    var itemId = "";
	var scheduleName=event.cell.parent().find(".item-name").text();
    if (event.cell.context.attributes.getNamedItem("data-scheduleid")) {
        scheduleId = event.cell.context.attributes.getNamedItem("data-scheduleid").value;
    }
    if (event.cell.context.attributes.getNamedItem("data-itemId")) {
        itemId = event.cell.context.attributes.getNamedItem("data-itemId").value;
    }

    switch (currentSelection) {
        case "edit-schedule":
        case "su su-edit":
            var itemName = event.cell.context.attributes.getNamedItem("data-itemname").value;
            ScheduleId=scheduleId;
			ItemId= itemId; 
			ItemName=itemName;
			$("#PopupContainer").ejDialog("open");
            break;
        case "remove-schedule":
        case "su su-delete":
            removeSchedule(scheduleId);
            break;
        case "enable-schedule":
        case "su su-folder85":
            enableSchedule(scheduleId);
            break;
        case "disable-schedule":
        case "su su-folder85":
            disableSchedule(scheduleId);
            break;
        case "su su-play":
        case "onDemand-schedule":
            ShowWaitingProgress("#schedulesGridContainer", "show");
            onDemandSchedule(scheduleId,scheduleName);
            break;
    }
}

function enableSchedule(id) {
    ShowWaitingProgress("#page_content_Div", "show");
    $.ajax({
        type: "POST",
        url: baseurl + "/scheduler/EnableSchedule",
        data: { scheduleId: id },
        async: false,
        success: function (data) {
            refreshScheduleGrid();
            ShowWaitingProgress(".share-popup-header", "hide");
        }
    });
}

function disableSchedule(id) {
    ShowWaitingProgress("#page_content_Div", "show");
    $.ajax({
        type: "POST",
        url: baseurl + "/scheduler/DisableSchedule",
        data: { scheduleId: id },
        async: false,
        success: function (data) {
            refreshScheduleGrid()
            ShowWaitingProgress(".share-popup-header", "hide");
        }
    });
}

function onDemandSchedule(id,scheduleName) {
    $.ajax({
        type: "POST",
        url: baseurl + "/scheduler/OnDemandSchedule",
        data: { scheduleId: id },
        success: function (data) {
            ShowWaitingProgress("#schedulesGridContainer", "hide");
			messageBox("su-play", "Run Now", "Schedule—"+scheduleName+" has been started successfully.")
        }
    });
}
function fnActionBegin(){
	isFirstRequest=true;
}
function rowBound(height){
	if(isFirstRequest){
		isFirstRequest=false;
		refreshFooterPosition(height);
	}
}
function removeSchedule(id) {
    $("#schedule_delete_confirmation").ejDialog("open");
    $("#schedule_delete_confirmation_wrapper").ejWaitingPopup("show");
    $.ajax({
        type: "POST",
        url: "/scheduler/_deleteschedule",
        data: {},
        async: false,
        success: function (data) {
            $("#schedule_delete_confirmation").html(data);
            $("#delete_item").attr("data-schedule-id", id);
            $("#schedule_delete_confirmation_wrapper").ejWaitingPopup("hide");
        }
    });
}

$(document).on("click", "#delete_item", function (e) {
    $("#schedule_delete_confirmation_wrapper").ejWaitingPopup("show");
    var id = $(this).attr("data-schedule-id");
    $.ajax({
        type: "POST",
        url: "/scheduler/RemoveSchedule",
        data: { scheduleId: id },
        async: false,
        success: function (data) {
            if (data) {
                $("#schedule_delete_confirmation_wrapper").ejWaitingPopup("hide");
                $(".dialogFooter .validationArea").css("display", "none");
                $(".dialogFooter .successArea").css("display", "block");
				$(".PopupClose").attr("onclick","deleteSuccess()");
                $(".deleteItem").html("Schedule has been deleted successfully.");
            }
        }
    });
});

function deleteScheduleDialogClose() {
    parent.$("#schedule_delete_confirmation").ejDialog("close");
}

$(document).on("click", ".PopupClose", function (event) {
    $("#schedule_delete_confirmation").ejDialog("close");
});

function deleteSuccess(){
	$(this).removeAttr("onclick");
	var scheduleGridObj = $("#scheduleGrid").data("ejGrid");
	var currentPage= scheduleGridObj.model.pageSettings.currentPage;
	var pageSize= scheduleGridObj.model.pageSettings.pageSize;
	var totalRecordsCount=scheduleGridObj.model.pageSettings.totalRecordsCount;
	var lastPageRecordCount=scheduleGridObj.model.pageSettings.totalRecordsCount%scheduleGridObj.model.pageSettings.pageSize;

	if (lastPageRecordCount!=0&&lastPageRecordCount<=1) {

		scheduleGridObj.model.pageSettings.currentPage =currentPage- 1;
	}
	refreshScheduleGrid();
}


function onSchedulerDialogClose() {
$("#PopupContainer").find("iframe").contents().find("html").html("");
	
}
function onSchedulerDialogOpen() {
    $("#scheduler_popup_iframe").attr("src", "scheduler/GetSchedulerDialog?itemName="+ItemName+"&&itemId="+ItemId+"&&scheduleId="+ScheduleId+"&&actionType=Edit");
	    $("#PopupContainer_wrapper").ejWaitingPopup("show");

}
 function refreshScheduleGrid() {
	var scheduleGridObj = $("#scheduleGrid").data("ejGrid");
	var currentPage = scheduleGridObj.model.pageSettings.currentPage;
	var sortingInfo = scheduleGridObj.model.sortSettings.sortedColumns;
	$.ajax({
		type: "POST",
		url: "/scheduler/getschedules",
		beforeSend: ShowWaitingProgress("#page_content_Div", "show"),
		success: function(result) {
			$("#scheduleGrid").ejGrid("option", "model.dataSource", result);
			var currentGridObj = $("#scheduleGrid").data("ejGrid");
			currentGridObj.gotoPage(currentPage);
			if (sortingInfo != null) {
				if (sortingInfo[0] != null) {
					currentGridObj.sortColumn(sortingInfo[0].field, sortingInfo[0].direction);
				}
			}
			ShowWaitingProgress("#page_content_Div", "hide");
		}
	});
}
$.views.converters("toLowerCase", function(name) {
  return name.toLowerCase();
});