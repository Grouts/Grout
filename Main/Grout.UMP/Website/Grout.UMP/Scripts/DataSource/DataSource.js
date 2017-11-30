$(function() {
    $("#datasource_edit_popup").ejDialog({
        width: "800px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: window.innerHeight * 0.9 + "px",
        showHeader: false,
        enableModal: true,
        close: "onDataSourceEditDialogClose",
        closeOnEscape: false,
        open: "onDataSourceEditDialogOpen"
    });
    $("#datasource_edit_popup_wrapper").ejWaitingPopup();
    $(document).on("click", ".item-edit", function(e) {
        var itemId = $(this).attr("data-item-id");
        $("#datasource_edit_popup").ejDialog("open");
        ShowWaitingProgress("#datasource_edit_popup_wrapper", "show");
        $("#datasource_edit_popup_iframe").attr("src", "datasources/editdatasourceview?itemId=" + itemId);
    });
    $(window).resize(function () {
        var gridObj = $("#itemsGrid").data("ejGrid");
        (window.innerWidth < 1200) ? gridObj.hideColumns("Owner") : gridObj.showColumns("Owner");
        refreshFooterPosition();
    });
});

function onDataSourceEditDialogClose() {
    $("#datasource_edit_popup").find("iframe").contents().find("html").html("");
    $("#datasource_edit_popup").ejDialog("close");
}
