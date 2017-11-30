$(document).ready(function () {
    $(document).on("click", "#VersionTab", function (e) {
        $(".downArrow").css("left", "50px");
        $(this).removeClass("inactive-content").addClass("active-content");
        $("#ItemLogTab").removeClass("active-content").addClass("inactive-content");
        $("#ItemVersionContainer").removeClass("hide-content").addClass("visible-content");
        $("#ItemLogContainer").removeClass("visible-content").addClass("hide-content");
    });

    $(document).on("click", "#ItemLogTab", function (e) {
        $(".downArrow").css("left", "185px");
        $(this).removeClass("inactive-content").addClass("active-content");
        $("#VersionTab").removeClass("active-content").addClass("inactive-content");
        $("#ItemVersionContainer").removeClass("visible-content").addClass("hide-content");
        $("#ItemLogContainer").removeClass("hide-content").addClass("visible-content");
    });

    $(document).on("click", ".versionPopupClose", function (e) {
        $("#ItemVersionController").css("display", "none");
        eDialog = parent.$("#verionWindowContainer").data("ejDialog");
        eDialog.close();
        $("#verionWindowContainer iframe").attr("src", "");
    });


    $(document).on("click", ".ItemRollback", function (e) {
        var versionId = $(this).attr("data-item-version");
        var itemId = $(this).attr("data-item-id");

        doAjaxPost("POST", "/items/rollbackItem", { ItemId: itemId, versionId: versionId },
            function (data, result) {
                var gridObj = $("#Grid").ejGrid("instance");
                gridObj.refreshContent();
                var loggridObj = $("#LogGrid").ejGrid("instance");
                loggridObj.refreshContent();
            }, null, null, null, null, null, true);
    });

});

$(document).on("click", ".PopupClose", function () {
    window.parent.$("#verionWindowContainer").ejDialog("close");

});
$(document).on("click", "#CloseButton", function () {
    window.parent.$("#verionWindowContainer").ejDialog("close");

});