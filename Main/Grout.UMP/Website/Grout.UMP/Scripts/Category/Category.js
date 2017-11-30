$(function() {
    $("#EditCategoryPopup").ejDialog({
        width: "600px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "285px",
        showHeader: false,
        enableModal: true,
        close: "OnEditCategoryDialogClose",
        closeOnEscape: false
    });
    $("#EditCategoryPopup_wrapper").ejWaitingPopup();
});

$(document).on('click', '.DeleteCategory', function (event) {
    $(this).parents(".dropdown").removeClass("open");
    $(this).parents(".dropdown").find(".dropdown-backdrop").remove();
    event.stopPropagation();
    $("#delete_item_name").html($(this).attr("data-name"));
    $("#delete_item").attr("data-item-id", $(this).attr("data-item-id"));
    $("#delete_item").attr("data-itemtype", $(this).attr("Category"));
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
            $("#item_delete_confirmation_wrapper").ejWaitingPopup("hide");
            $("#item_delete_confirmation").html(data);
        }
    });
});
$(document).on('click', '.EditCategory', function (event) {
    $(this).parents(".dropdown").removeClass("open");
    $(this).parents(".dropdown").find(".dropdown-backdrop").remove();
    event.stopPropagation();
    var itemId = $(this).attr("data-item-id");
    $("#EditCategoryPopup").ejDialog("open");
    ShowWaitingProgress("#EditCategoryPopup_wrapper", "show");
    $("#EditCategoryPopup_iframe").attr("src", "category/getcategorydetails?itemId=" + itemId);
});

function OnEditCategoryDialogClose() {
    $("#EditCategoryPopup").find("iframe").contents().find("html").html("");
}
