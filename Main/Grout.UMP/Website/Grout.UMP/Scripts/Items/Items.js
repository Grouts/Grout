$(function () {
	var isFirstRequest=false;
	var ScheduleId, ItemId, ItemName;
    $("#datasource_popup").ejDialog({
        width: "800px",
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: window.innerHeight * 0.9 + "px",
        showHeader: false,
        enableModal: true,
        close: "onDataSourceDialogClose",
        open: "onDataSourceDialogOpen"
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
    $("#report_popup").ejDialog({
        width: "800px",
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "530px",
        showHeader: false,
        enableModal: true,
        closeOnEscape: false,
        open: 'onReportDialogOpen',
        close: 'onReportDialogClose'
    });

    $("#select_datasource_popup").ejDialog({
        width: "800px",
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "530px",
        showHeader: false,
        enableModal: true,
        closeOnEscape: false,
        close: 'closeNewDataSourcePopup'
    });
    $("#AddCategoryPopup").ejDialog({
        width: "600px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "285px",
        showHeader: false,
        enableModal: true,
        closeOnEscape: true,
        close: "onCategoryDialogBoxClose",
        open: "onCategoryDialogOpen"
    });
    $("#addFileDom").ejDialog({
        width: "650px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "325px",
        title: "Add File",
        enableModal: true,
        showHeader: false,
        open: "onNewFileDialogOpen",
        close: "onNewFileDialogClose"
    });
    $("#item_delete_confirmation").ejDialog({
        width: "450px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "282px",
        title: "Delete item",
        showHeader: false,
        enableModal: true,
        close: "onDataSourceDeleteDialogClose",
        open: "onDataSourceDeleteDialogOpen"
    });
    $("#verionWindowContainer").ejDialog({
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "615px",
        width: "900px",
        title: "",
        showHeader: false,
        enableModal: true,
        close: "DialogBoxClose",
        closeOnEscape: true
    });
    $("#ItemAction").ejDialog({
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "275px",
        width: "525px",
        title: "",
        showHeader: false,
        enableModal: true,
        close: "itemActionEmpty",
        closeOnEscape: true
    });
    $("#report_popup_wrapper").ejWaitingPopup();
    $("#select_datasource_popup_wrapper").ejWaitingPopup();
    $("#addFileDom_wrapper").ejWaitingPopup();
    $("#item_delete_confirmation_wrapper").ejWaitingPopup();
    $("#AddCategoryPopup_wrapper").ejWaitingPopup();
    $("#datasource_popup_wrapper").ejWaitingPopup();
    $("#ItemAction_wrapper").ejWaitingPopup();

    $(document).on("click", ".item-delete", function (e) {

        $("#delete_item_name").html($(this).attr("data-name"));
        $("#delete_item").attr("data-item-id", $(this).attr("data-item-id"));
        var itemId = $(this).attr("data-item-id");
        var itemName = $(this).attr("data-name");
        var itemTypeId = $(this).attr("data-itemtype");
        $("#item_delete_confirmation").ejDialog("open");
        $("#item_delete_confirmation_wrapper").ejWaitingPopup("show");
        $.ajax({
            type: "POST",
            url: "/items/deleteconfirmation",
            data: { itemId: itemId, itemTypeId: itemTypeId, itemName: itemName },
            async: false,
            success: function (data) {
                $("#item_delete_confirmation").html(data);
                $("#item_delete_confirmation_wrapper").ejWaitingPopup("hide");
            }
        });
    });

    $(document).on("click", "#delete_item", function (e) {
        $("#item_delete_confirmation_wrapper").ejWaitingPopup("show");
        var itemId = $(this).attr("data-item-id");
        var itemtype = $(this).attr("data-itemtype");
        $.ajax({
            type: "POST",
            url: "/items/deleteitem",
            data: { itemId: itemId },
            async: false,
            success: function (data) {
                if (data) {
                    $("#item_delete_confirmation_wrapper").ejWaitingPopup("hide");
                    $(".dialogFooter .validationArea").css("display", "none");
                    $(".dialogFooter .successArea").css("display", "block");
                    $(".deleteItem").html(itemtype +" has been deleted successfully.");
                    if (itemtype == "Category") {
                        RefreshCategoryList();
                    }
                    else if (itemtype == "Report") {
                        onSuccessDeleteItem();
                        RefreshCategoryList();
                    } else {
                        onSuccessDeleteItem();
                    }
                }
            }
        });
    });
    addPlacehoder("#searchArea");

    $(document).on("click", ".versionButton", function () {
        $("#verionWindowContainer iframe").attr("src", "/items/itemversion?itemId=" + $(this).attr("data-itemId"));
        $("#verionWindowContainer").ejDialog("open");
        ShowWaitingProgress("#verionWindowContainer_wrapper", "show");
    });

    $(document).on("click", ".moveItem", function () {
        $("#ItemAction iframe").attr("src", "/items/moveviewitem?itemId=" + $(this).attr("data-itemId") + "&itemAction=" + $(this).attr("data-action"));
        $("#ItemAction").ejDialog("open");
        ShowWaitingProgress("#ItemAction_wrapper", "show");
    });

    $(document).on("click", ".copyItem", function () {
        var itemAction = $(this).attr("data-action");
        $("#ItemAction iframe").attr("src", "/items/copyviewitem?itemId=" + $(this).attr("data-itemId") + "&itemAction=" + $(this).attr("data-action"));
        $("#ItemAction").ejDialog("open");
        ShowWaitingProgress("#ItemAction_wrapper", "show");
    });

    $(window).resize(function () {
        var versionDialogObj = $("#verionWindowContainer").data("ejDialog");
        if (versionDialogObj.isOpened()) {
            versionDialogObj._dialogPosition();
        }
    });

});

function onSuccessDeleteItem() {
    var gridObj = $("#itemsGrid").data("ejGrid");
    gridObj.model.sortSettings.sortedColumns = [];
    gridObj.model.filterSettings.filteredColumns = [];
    $("#searchItems").find("input[type=text]").val('');
    var currentPage = gridObj.model.pageSettings.currentPage;
    var pageSize = gridObj.model.pageSettings.pageSize;
    var totalRecordsCount = gridObj.model.pageSettings.totalRecordsCount;
    var lastPageRecordCount = gridObj.model.pageSettings.totalRecordsCount % gridObj.model.pageSettings.pageSize;

    if (lastPageRecordCount != 0 && lastPageRecordCount <= 1) {

        gridObj.model.pageSettings.currentPage = currentPage - 1;
    }
    gridObj.refreshContent()
}
function onNewFileDialogOpen() {
    $("#addfile").attr("src", "files/addfile");
}

function onSchedulerDialogClose() {
$("#PopupContainer").find("iframe").contents().find("html").html("");
	
}
function onSchedulerDialogOpen() {
    $("#scheduler_popup_iframe").attr("src", "scheduler/GetSchedulerDialog?itemName="+ItemName+"&&itemId="+ItemId+"&&scheduleId="+ScheduleId+"&&actionType=Create");
	    $("#PopupContainer_wrapper").ejWaitingPopup("show");

}
function onNewFileDialogClose() {
    $("#addFileDom").find("iframe").contents().find("html").html("");
}

function onReportDialogClose() {
    $("#report_popup").find("iframe").contents().find("html").html("");
}

function openNewDataSourcePopup() {
    $("#datasource_popup").ejDialog("open");
    $("#datasource_popup_wrapper").ejWaitingPopup("show");
}

function openNewCategoryPopup() {
    $("#AddCategoryPopup").ejDialog("open");
    $("#AddCategoryPopup_wrapper").ejWaitingPopup("show");
}

function openNewFilePopup() {
    $("#addFileDom").ejDialog("open");
    $("#addFileDom_wrapper").ejWaitingPopup("show");
}

function onCategoryDialogOpen() {
    $("#AddCategoryPopup_iframe").attr("src", "category/addcategory");
}

function onCategoryDialogBoxClose() {
    $("#AddCategoryPopup").find("iframe").contents().find("html").html("");
    $("#AddCategoryPopup").ejDialog("close");
}

function onDataSourceDialogOpen() {
    $("#datasource_popup_iframe").attr("src", "datasources/adddatasource");
}

function onDataSourceDialogClose() {
    $("#datasource_popup").find("iframe").contents().find("html").html("");
    $("#datasource_popup").ejDialog("close");
}

function onDeleteItemDialogClose() {
    $("#item_delete_confirmation").ejDialog("close");
}

function onReportDialogOpen() {
    //$("#report_popup_wrapper").ejWaitingPopup("hide");
}

function openNewReportPopup() {
    $("#report_popup_title .e-title").html("Add New Report");
    $("#report_popup").ejDialog("open");
    $("#report_iframe").attr("src", "reports/addreport");
    $("#datasource_list").hide();
    $("#version_comment").hide();
    $("#report_popup_wrapper").ejWaitingPopup("show");
}

function fnActionBegin(args) {
	isFirstRequest=true;
    var searchValue = $("#searchItems").val();

    if (searchValue != "Search") {
        this.model.query._params.push({ key: "searchKey", value: searchValue });
    }
    var filerSettings = [], i;

    if (args.model.filterSettings.filteredColumns.length > 0) {
        for (i = 0; i < args.model.filterSettings.filteredColumns.length; i++) {
            var column = args.model.filterSettings.filteredColumns[i];
            filerSettings.push({ 'PropertyName': column.field, 'FilterType': column.operator, 'FilterKey': column.value });
        }

        this.model.query._params.push({ key: "filterCollection", value: filerSettings });
    }

    args.model.query._params.push({
        key: "isAllCategorySearch",
        value: $("#FixedLeftSection #listing li.active").length > 0 && $("#FixedLeftSection #listing li.active").attr("data-name") == "All Category"
    });
}

function fnActionComplete(args) {
    var gridName = $('#itemGridContainer').attr("data-grid-name");
    if (gridName == "reports") {
        if (args.model.currentViewData.length == 0) {
            rowBound(38);
        }
        var gridObj = $("#itemsGrid").data("ejGrid");
        //ineffable if and ele statements - Do not change this. Grid flickers when a invisible column is made invisible again and vice versa. Consult before changing.
        if ($("#FixedLeftSection #listing li.active").length > 0 &&
            $("#FixedLeftSection #listing li.active").attr("data-name") == "All Category" &&
            gridObj.model.columns[2].visible == false) {
            gridObj.showColumns("Category");
        } else if ($("#FixedLeftSection #listing li.active").length > 0 &&
            $("#FixedLeftSection #listing li.active").attr("data-name") != "All Category" &&
            gridObj.model.columns[2].visible) {
            gridObj.hideColumns("Category");
        }
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
$(document).on("click", ".category-name", function () {
    $(".category-name").removeClass("activeCategorySetting");
    $(this).parents("ul").find("li").removeClass("active");
    $(this).addClass("active");

    $("#categoryName").html($(this).attr("data-name")).attr("title", $(this).attr("data-name"));
    var categoryDescription = $(this).attr("data-description");
    $("#categoryDescription").html(categoryDescription);
    $("#categoryDescription").attr("title", $(this).attr("data-description"));

    var gridObj = $("#itemsGrid").data("ejGrid");
    gridObj.model.sortSettings.sortedColumns = [];
    $("#searchItems").find("input[type=text]").val('');
    gridObj.model.pageSettings.currentPage = 1;
    gridObj.model.filterSettings.filteredColumns = [{ field: "CategoryName", operator: "equal", value: $(this).attr("data-name") }];
    gridObj.refreshContent();
});

$(document).on("click", ".category-link", function (e) {
    $('.e-filtericon').removeClass('e-filteredicon e-filternone');
    e.preventDefault();
});

$(document).on("click", ".PopupClose", function () {
    window.parent.$("#item_delete_confirmation").ejDialog("close");
});

function ResetGrid() {
    var gridObj = $("#itemsGrid").data("ejGrid");
    gridObj.model.sortSettings.sortedColumns = [];
    gridObj.model.filterSettings.filteredColumns = [];
    $("#searchItems").find("input[type=text]").val('');
    gridObj.refreshContent();
    $('.e-filtericon').removeClass('e-filteredicon e-filternone');
}

$(document).on("keydown", "#searchItems", function (e) {
    if (e.keyCode == "13") {
        var gridObj = $("#itemsGrid").data("ejGrid");
        gridObj.refreshContent();
    }
});

$(document).on("click", ".search-item", function () {
    var gridObj = $("#itemsGrid").data("ejGrid");
    gridObj.refreshContent();
});

$(document).on("click", ".ScheduleReports", function () {
	ItemId=$(this).attr("data-itemid")
	ItemName=$(this).attr("data-itemname");
	ScheduleId="";
	$("#PopupContainer").ejDialog("open");
});
