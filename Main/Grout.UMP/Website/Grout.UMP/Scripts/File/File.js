$(function() {
    $("#EditFilePopup").ejDialog({
        width: "650px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "385px",
        title: "Update File",
        enableModal: true,
        showHeader: false,
        close: "OnEditFileDialogClose",
        closeOnEscape: true,
    });
    $("#EditFilePopup_wrapper").ejWaitingPopup();
    $(window).resize(function () {
        var gridObj = $("#itemsGrid").data("ejGrid");
        (window.innerWidth < 1200) ? gridObj.hideColumns("Owner") : gridObj.showColumns("Owner");
        refreshFooterPosition();
    });
});

$(document).on('click', '.item-edit', function () {
    var itemId = $(this).attr("data-item-id");
    $("#EditFilePopup").ejDialog("open");
    ShowWaitingProgress("#EditFilePopup_wrapper", "show");
    $("#EditFilePopup_iframe").attr("src", "files/getfiledetails?itemId=" + itemId);
});


function editFilePopup(Id, Name, Description) {
    $("#EditFilePopup").ejDialog("open");
    var iframe = $("#EditCategoryPopup_iframe").contents();
    iframe.find("#file_name").val(Name);
    iframe.find("#file_description").val(Description);
}

function OnEditFileDialogClose() {
    $("#EditFilePopup").find("iframe").contents().find("html").html("");
}

