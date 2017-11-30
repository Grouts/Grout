$(function () {
    $("#FixedLeftSection").ejWaitingPopup();

    $(document).on("click", ".item-edit", function(event) {
        $("#report_popup").ejDialog("open");
        ShowWaitingProgress("#report_popup_wrapper", "show");
        $("#report_iframe").attr("src", "reports/editreportview?itemId=" + $(this).attr("data-item-id"));
        $("#report_popup_title .e-title").html("Update RDL report");
    });

    $(window).resize(function () {
        var gridObj = $("#itemsGrid").data("ejGrid");
        if (window.innerWidth < 1200) {
            gridObj.model.columns[5].width = 25
            gridObj._columnsWidthCollection[5] = 25;
            gridObj.hideColumns("Owner");
        }
        else {
            gridObj.model.columns[5].width = 15
            gridObj._columnsWidthCollection[5] = 15;
            gridObj.showColumns("Owner");
        }
        gridObj.setWidthToColumns();
        initLayoutRender();
        var actionsDialogObj = $("#ItemAction").data("ejDialog");
        if (actionsDialogObj.isOpened()) {
            actionsDialogObj._dialogPosition();
        }
        var scheduleDialogObj = $("#PopupContainer").data("ejDialog");
        if (scheduleDialogObj.isOpened()) {
            scheduleDialogObj._dialogPosition();
        }
    });
    $(".collapseIcon").on("click", function (e) {
        ($(this).hasClass("collapse-category")) ? collapeGrid() : expandGrid();
    });
});

function collapeGrid() {
    $(".collapseIcon").removeClass("collapse-category");
    $(".collapseIcon").addClass("expand-category");
    $(".collapseIcon").find("span").text("<<");
    $(".item-listing").removeClass("expandedGrid");
    $("#base_footer_Div").removeClass("expandedGrid");
    $("#FixedLeftSection").removeClass("collapsed");
    refreshScroller();
}

function expandGrid() {
    $(".collapseIcon").removeClass("expand-category");
    $(".collapseIcon").addClass("collapse-category");
    $(".collapseIcon").find("span").text(">>");
    $(".item-listing").addClass("expandedGrid");
    $("#base_footer_Div").addClass("expandedGrid");
    $("#FixedLeftSection").addClass("collapsed");
}

function initLayoutRender(onResize) {
    if (window.innerWidth < 1200) {
        expandGrid();
        $(".collapseIcon").show();
    }
    else {
        $(".collapseIcon").hide();
        collapeGrid();
    }
    refreshFooterPosition();
}


