var oldUserSelected = [];
var oldGroupSelected = [];

function createSchedule(itemId, itemName) {
    ShowWaitingProgress("#page_content_Div", "show");
    oldUserSelected = [];
    oldGroupSelected = [];
    var reportItemName = itemName;
    var reportItemId = itemId;
    getAllStaticData();
    renderSchedulePopUp(reportItemName, reportItemId);
    parent.$("#PopupContainer_wrapper").ejWaitingPopup("hide");
    $("select#UserSearch option").each(function (i) {
        if ($(this).val().toLowerCase() == parent.$("#userName").val().toLowerCase()) {
            var currentuser = $(this).text();
            $(this).attr("selected", true);
            $('#UserSearch').selectpicker("refresh");
            var userTile = $("<div>").attr("id", $(this).val()).attr("data-searchtype", "userSearch").addClass("SelectedShareItems");
            userTile.html("<div class='InstantSearch'><span class='details'>" + currentuser
                + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
            $("#SelectedUsers").append(userTile);
        }
    });

    selectedItemsCount();

    $("#schedule_Submit").on('click', function () {
        var scheduleItem = {};
        scheduleItem.ScheduleName = $('#schedule_name').val();
        scheduleItem.ItemId = $(this).attr("data-report-id");
        scheduleItem.ExportType = $('input[name=exportFormats]:checked', '#exportFormatContainer').val().toString();
        switch ($('#recurrenceType').val().toString()) {
            case "Daily":
                if ($('#dailyEveryXdays').data('ejRadioButton').model.checked) {
                    scheduleItem.RecurrenceType = "Daily";
                    scheduleItem.RecurrenceInterval = $("#everyXDays").val();
                }
                else {
                    scheduleItem.RecurrenceType = "DailyWeekDay";
                }
                break;
            case "Weekly":
                scheduleItem.RecurrenceType = "Weekly";
                scheduleItem.RecurrenceInterval = $("#everyXWeeks").val();
                scheduleItem.Sunday = $("#sun").data("ejCheckBox").model.checked;
                scheduleItem.Monday = $("#mon").data("ejCheckBox").model.checked;
                scheduleItem.Tuesday = $("#tues").data("ejCheckBox").model.checked;
                scheduleItem.Wednesday = $("#wed").data("ejCheckBox").model.checked;
                scheduleItem.Thursday = $("#thu").data("ejCheckBox").model.checked;
                scheduleItem.Friday = $("#fri").data("ejCheckBox").model.checked;
                scheduleItem.Saturday = $("#sat").data("ejCheckBox").model.checked;
                break;
            case "Monthly":
                if ($('#monthly').data('ejRadioButton').model.checked) {
                    scheduleItem.RecurrenceType = "Monthly";
                    scheduleItem.DaysOfMonth = $('#monthlyDate').val();
                    scheduleItem.RecurrenceInterval = $('#monthlyEveryXMonths').val();
                }
                else {
                    scheduleItem.RecurrenceType = "MonthlyDOW";
                    scheduleItem.WeekOfMonth = $("#monthlyDOWWeek").val();
                    scheduleItem.DayOfWeek = $("#monthlyDOWDay").val();
                    scheduleItem.RecurrenceInterval = $('#monthlyDOWEveryXMonths').val();
                }
                break;
            case "Yearly":
                scheduleItem.RecurrenceInterval = $('#everyXYears').val();
                if ($('#yearly').data('ejRadioButton').model.checked) {
                    scheduleItem.RecurrenceType = "Yearly";
                    scheduleItem.DaysOfMonth = $('#yearlyDay').val();
                    scheduleItem.MonthOfYear = $('#yearlyMonth').val();
                }
                else {
                    scheduleItem.RecurrenceType = "YearlyDOW";
                    scheduleItem.WeekOfMonth = $("#yearlyDOWWeek").val();
                    scheduleItem.DayOfWeek = $("#yearlyDOWDay").val();
                    scheduleItem.MonthOfYear = $('#yearlyDOWMonth').val();
                }
                break;
        }

        scheduleItem.IsEnabled = $("#enableSchedule").data("ejCheckBox").model.checked;

        scheduleItem.StartDate = $('#StartDate').val();

        switch ($('input[name=EndType]:checked', '#scheduleEndType').val().toString()) {
            case "never":
                scheduleItem.ScheduleEndType = "NoEnd";
                break;
            case "endAfter":
                scheduleItem.ScheduleEndType = "EndAfter";
                scheduleItem.RecurrenceFactor = $("#occurenceCount").val();
                break;
            case "endBy":
                scheduleItem.ScheduleEndType = "EndBy";
                scheduleItem.EndDate = $('#EndDate').val();
                break;
        }

        var selectedItems = $(".selected-recipients").children();

        var userlist = Array();
        var grouplist = Array();
        var a = 0, b = 0, c = 0;
        for (var i = 0; i < selectedItems.length; i++) {
            if (selectedItems[i].getAttribute("data-searchtype") == "userSearch") {
                if (a != -1) {
                    userlist[a] = selectedItems[i].id;
                    a++;
                }

            }
            if (selectedItems[i].getAttribute("data-searchtype") == "groupSearch") {
                if (b != -1) {
                    grouplist[b] = selectedItems[i].id;
                    b++;
                }
            }
        }
        if (selectedItems.length) {

            $.ajax({
                type: "POST",
                url: parent.baseurl + "/scheduler/AddSchedule",
                data: { scheduleList: JSON.stringify({ ScheduleItem: scheduleItem, UserList: userlist, GroupList: grouplist }) },
                beforeSend: function () {
                    parent.$("#PopupContainer_wrapper").ejWaitingPopup("show");
                },
                success: function (data) {
                    parent.$("#PopupContainer_wrapper").ejWaitingPopup("hide");
                    var scheduleName = $(".SchedulePopupTitle").text();
                    closePopupContainer();
                    parent.messageBox("su-calendar-1", scheduleName, "Report has been scheduled successfully. ", "success", function () {
                        parent.onCloseMessageBox();
                    });
                }
            });
        }
        else {
            $("#selectedUsersValidation").css("visibility", "visible");
        }

    });

    $(".share-popup .bs-deselect-all").after("<div class='bs-select-all-custom'><span>Select All</span><span class='bs-select-custom-tick glyphicon glyphicon-ok' ></span></div>");

    $("#userSearch_container").on('click', '.bs-select-all-custom', function (e) {
        $("#userSearch_container").addClass("valueChanged");
        $('#UserSearch').data("selectpicker").selectAll();
        $(this).removeClass('bs-select-all-custom').addClass('bs-deselect-all-custom');
        $($(this).children("span")[0]).text("Clear All");
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#groupSearch_container").on('click', '.bs-select-all-custom', function (e) {
        $("#groupSearch_container").addClass("valueChanged");
        $('#GroupSearch').data("selectpicker").selectAll();
        $(this).removeClass('bs-select-all-custom').addClass('bs-deselect-all-custom');
        $($(this).children("span")[0]).text("Clear All");
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#userSearch_container").on('click', '.bs-deselect-all-custom', function (e) {
        $("#userSearch_container").addClass("valueChanged");
        $('#UserSearch').data("selectpicker").deselectAll();
        $(this).removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        $($(this).children("span")[0]).text("Select All");
        $(".SelectedShareItems[data-searchtype='userSearch']").remove();
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#groupSearch_container").on('click', '.bs-deselect-all-custom', function (e) {
        $("#groupSearch_container").addClass("valueChanged");
        $('#GroupSearch').data("selectpicker").deselectAll();
        $(this).removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        $($(this).children("span")[0]).text("Select All");
        $(".SelectedShareItems[data-searchtype='groupSearch']").remove();
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#userSearch_container").on('click', '.bootstrap-select li a', function (e) {
        $("#userSearch_container").addClass("valueChanged");;
        var selectedCount = $("#userSearch_container .bootstrap-select li.selected").length;
        var allListCount = $("#userSearch_container .bootstrap-select li").length;

        if (selectedCount == allListCount) {
            $($('#userSearch_container div.bs-select-all-custom').children("span")[0]).text("Clear All");
            $('#userSearch_container div.bs-select-all-custom').removeClass("bs-select-all-custom").addClass("bs-deselect-all-custom");
        }
        if ($(this).parent().hasClass("selected")) {
            var selectedUser = $("#UserSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            var userTile = $("<div>").attr("id", $(selectedUser).val()).attr("data-searchtype", "userSearch").addClass("SelectedShareItems");
            userTile.html("<div class='InstantSearch'><span class='details'>" + $(selectedUser).text() + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
            $("#SelectedUsers").append(userTile);
        }
        else {
            var selectedUser = $("#UserSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            $(".SelectedShareItems[id='" + $(selectedUser).val() + "']").remove();
            $($('#userSearch_container .bs-deselect-all-custom').children("span")[0]).text("Select All");
            $("#userSearch_container .bs-deselect-all-custom").removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        }
        $(this).parent().addClass("active");
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#groupSearch_container").on('click', '.bootstrap-select .dropdown-menu .selectpicker li a', function (e) {
        $("#groupSearch_container").addClass("valueChanged");;
        var selectedCount = $("#groupSearch_container .bootstrap-select li.selected").length;
        var allListCount = $("#groupSearch_container .bootstrap-select li").length;
        if (selectedCount == allListCount) {
            $($('#groupSearch_container div.bs-select-all-custom').children("span")[0]).text("Clear All");
            $('#groupSearch_container div.bs-select-all-custom').removeClass("bs-select-all-custom").addClass("bs-deselect-all-custom");
        }

        if ($(this).parent().hasClass("selected")) {
            var selectedGroup = $("#GroupSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            var groupTile = $("<div>").attr("id", $(selectedGroup).val()).attr("data-searchtype", "groupSearch").addClass("SelectedShareItems");
            groupTile.html("<div class='InstantSearch'><span class='details'>" + $(selectedGroup).text() + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
            $("#SelectedUsers").append(groupTile);
        }
        else {
            var selectedGroup = $("#GroupSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            $(".SelectedShareItems").filter("[data-searchtype='groupSearch']").filter("#" + $(selectedGroup).val()).remove();
            $($('#groupSearch_container .bs-deselect-all-custom').children("span")[0]).text("Select All");
            $("#groupSearch_container .bs-deselect-all-custom").removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        }
        $(this).parent().addClass("active");
        selectedItemsCount();
        e.stopPropagation();
    });



    parent.$("#PopupContainer_wrapper").ejWaitingPopup("hide");
}

$("#scheduleSearch_global").on("keyup", "#scheduleSearch_formfield", function () {
    var searchText = $(this).val();
    if (searchText.length > 2) {
        $("#scheduleGrid").data("ejGrid").search(searchText);
    } else {
        $("#scheduleGrid").data("ejGrid").search("");
    }
});







function renderSchedulePopUp(itemName, reportItemId) {
    //remove Dialog box and its elements
    $("#StartDate_popup").remove();
    $("#EndDate_popup").remove();
    $("#PopupContainer_wrapper").remove();
    $("#PopupContainer_overLay").remove();

    var recurrenceTypeList = "";
    var days = "";
    var weeks = "";
    var months = "";
    var zoneDateTime = "";

    $.ajax({
        type: "POST",
        url: "/scheduler/GetRecurrenceType",
        async: false,
        success: function (data) {
            ShowWaitingProgress(".share-popup-header", "hide");
            for (var t = 0; t < data.RecurrenceType.length; t++) {
                recurrenceTypeList += "<option value= " + data.RecurrenceType[t] + ">" + data.RecurrenceType[t] + "</option>";
            }

            for (var t = 0; t < data.Days.length; t++) {
                if (data.Days[t].toString().toLowerCase() == "weekendday"
                    || data.Days[t].toString().toLowerCase() == "day"
                    || data.Days[t].toString().toLowerCase() == "weekday") {
                    if (data.Days[t].toString().toLowerCase() == "weekendday") {
                        days += "<option value= " + data.Days[t] + ">weekend day</option>";
                    } else {
                        days += "<option value= " + data.Days[t] + ">" + data.Days[t] + "</option>";
                    }
                }
            }

            for (var t = 0; t < data.Days.length; t++) {
                if (data.Days[t].toString().toLowerCase() != "weekendday"
                    && data.Days[t].toString().toLowerCase() != "day"
                    && data.Days[t].toString().toLowerCase() != "weekday") {
                    days += "<option value= " + data.Days[t] + ">" + data.Days[t] + "</option>";
                }
            }

            for (var t = 0; t < data.Weeks.length; t++) {
                weeks += "<option value= " + data.Weeks[t] + ">" + data.Weeks[t] + "</option>";
            }
            for (var t = 0; t < data.Months.length; t++) {
                months += "<option value= " + data.Months[t] + ">" + data.Months[t] + "</option>";
            }

            zoneDateTime = data.TimeZoneDateTime.toString();
        }
    });


    $(".SchedulePopupTitle").html(" " + itemName + "—Schedule");
    $('#recurrenceType').append(recurrenceTypeList);


    $('#monthlyDOWWeek').append(weeks);
    $("#monthlyDOWDay").append(days);

    $("#yearlyMonth").append(months);

    $("#yearlyDOWWeek").append(weeks);
    $("#yearlyDOWDay").append(days)
    $("#yearlyDOWMonth").append(months);


    $("#UserSearch").append(window.userList);
    $("#GroupSearch").append(window.groupList)

    $("#PopupContainer_wrapper").ejWaitingPopup("hide");
    $('#schedule_Submit').attr("data-report-id", reportItemId);

    $("#dailyEveryXdays").ejRadioButton({ size: "medium", checked: true });
    $("#dailyWeekdays").ejRadioButton({ size: "medium" });


    $("#monthly").ejRadioButton({ size: "medium", checked: true });
    $("#monthlyDOW").ejRadioButton({ size: "medium" });


    $("#yearly").ejRadioButton({ size: "medium", checked: true });
    $("#yearlyDOW").ejRadioButton({ size: "medium" });

    $("#wordExport").ejRadioButton({ size: "medium" });
    $("#pdfExport").ejRadioButton({ size: "medium", checked: true });
    $("#excelExport").ejRadioButton({ size: "medium" });
    $("#htmlExport").ejRadioButton({ size: "medium" });

    $("#noEndDate").ejRadioButton({ size: "medium" });
    $("#endAfter").ejRadioButton({ size: "medium" });
    $("#endBy").ejRadioButton({ size: "medium" });

    $("#sun").ejCheckBox({ size: "small" });
    $("#mon").ejCheckBox({ size: "small" });
    $("#tues").ejCheckBox({ size: "small" });
    $("#wed").ejCheckBox({ size: "small" });
    $("#thu").ejCheckBox({ size: "small" });
    $("#fri").ejCheckBox({ size: "small" });
    $("#sat").ejCheckBox({ size: "small" });
    $("#enableSchedule").ejCheckBox({ size: "small" });

    $('#everyXDays').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 31, width: "65px", height: "34px" });
    $('#everyXWeeks').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 99, width: "65px", height: "34px" });
    $('#monthlyDate').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 31, width: "65px", height: "34px" });
    $('#monthlyEveryXMonths').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 99, width: "65px", height: "34px" });
    $('#monthlyDOWEveryXMonths').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 99, width: "65px", height: "34px" });
    $('#everyXYears').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 99, width: "65px", height: "34px" });
    $('#yearlyDay').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 31, width: "65px", height: "34px" });
    $('#occurenceCount').ejNumericTextbox({ name: "numeric", value: 1, minValue: 1, maxValue: 999, width: "65px", height: "34px" });

    $('#StartDate').ejDateTimePicker({
        interval: 10,
        value: zoneDateTime,
        change: validateDateTimePicker,
        enableStrictMode: false,
        dateTimeFormat: "MM/dd/yyyy hh:mm tt",
        timePopupWidth: 108
    });
    $('#EndDate').ejDateTimePicker({
        interval: 10,
        value: zoneDateTime,
        enableStrictMode: false,
        dateTimeFormat: "MM/dd/yyyy hh:mm tt",
        timePopupWidth: 108
    });

    $("#StartDate").attr("disabled", "disabled");
    $("#EndDate").attr("disabled", "disabled");
    $("#StartDate").css({ cursor: "default" });
    $("#EndDate").css({ cursor: "default" });

    $("#recurrenceType").selectpicker("refresh");
    $('#UserSearch').selectpicker("refresh");
    $('#GroupSearch').selectpicker("refresh");
    $('#monthlyDOWWeek').selectpicker("refresh");
    $('#monthlyDOWDay').selectpicker("refresh");
    $('#yearlyDOWWeek').selectpicker("refresh");
    $('#yearlyDOWDay').selectpicker("refresh");
    $('#yearlyDOWMonth').selectpicker("refresh");
    $('#yearlyMonth').selectpicker("refresh");
}

function editSchedule(id, itemId, itemName) {


    getAllStaticData();
    renderSchedulePopUp(itemName);
    $("#schedule_Submit").attr("data-schedule-id", id);
    $("#schedule_Submit").attr("data-item-id", itemId);

    var item = "";
    var recurrenceType = "";
    $.ajax({
        type: "POST",
        url: parent.baseurl + "/scheduler/GetScheduleInfo",
        data: { scheduleId: id },
        success: function (data) {

            item = data.ScheduleItem;
            $('#schedule_name').val(item.Name);
            $('#dailyScheduleOption').css('display', 'none');
            $('#weeklyScheduleOption').css('display', 'none');
            $('#monthlyScheduleOption').css('display', 'none');
            $('#yearlyScheduleOption').css('display', 'none');
            switch (item.RecurrenceType) {
                case "Daily":
                    recurrenceType = "Daily";
                    $('#dailyScheduleOption').css('display', 'block');
                    $("#dailyEveryXdays").ejRadioButton({ checked: true });
                    var everyXDaysObj = $("#everyXDays").data('ejNumericTextbox');
                    everyXDaysObj.option("value", item.Recurrence.DailySchedule.DaysInterval);
                    break;

                case "DailyWeekDay":
                    recurrenceType = "Daily";
                    $('#dailyScheduleOption').css('display', 'block');
                    $("#dailyWeekdays").ejRadioButton({ checked: true });
                    break;

                case "Weekly":
                    recurrenceType = "Weekly";
                    $('#weeklyScheduleOption').css('display', 'block');

                    var everyXWeeksObj = $("#everyXWeeks").data('ejNumericTextbox');
                    everyXWeeksObj.option("value", item.Recurrence.WeeklySchedule.WeeksInterval);
                    $("#sun").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Sunday });
                    $("#mon").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Monday });
                    $("#tues").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Tuesday });
                    $("#wed").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Wednesday });
                    $("#thu").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Thursday });
                    $("#fri").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Friday });
                    $("#sat").ejCheckBox({ checked: item.Recurrence.WeeklySchedule.DaysOfWeek.Saturday });

                    break;
                case "Monthly":
                    recurrenceType = "Monthly";
                    $('#monthlyScheduleOption').css('display', 'block');

                    $("#monthly").ejRadioButton({ checked: true });

                    var monthlyDateObj = $("#monthlyDate").data('ejNumericTextbox');
                    monthlyDateObj.option("value", item.Recurrence.MonthlySchedule.DayOfMonth);

                    var monthlyEveryXMonthsObj = $("#monthlyEveryXMonths").data('ejNumericTextbox');
                    monthlyEveryXMonthsObj.option("value", item.Recurrence.MonthlySchedule.Months);

                    break;
                case "MonthlyDOW":
                    recurrenceType = "Monthly";
                    $('#monthlyScheduleOption').css('display', 'block');
                    $("#monthlyDOW").ejRadioButton({ checked: true });
                    $("#monthlyDOWWeek option[value='" + item.Recurrence.MonthlyDowSchedule.WeeksOfTheMonth + "']").attr("selected", true);
                    $('#monthlyDOWWeek').selectpicker("refresh");

                    $("#monthlyDOWDay option[value='" + item.Recurrence.MonthlyDowSchedule.DaysOfTheWeek + "']").attr("selected", true);
                    $('#monthlyDOWDay').selectpicker("refresh");

                    var monthlyDOWEveryXMonthsObj = $("#monthlyDOWEveryXMonths").data('ejNumericTextbox');
                    monthlyDOWEveryXMonthsObj.option("value", item.Recurrence.MonthlyDowSchedule.Months);

                    break;
                case "Yearly":
                    recurrenceType = "Yearly";
                    $('#yearlyScheduleOption').css('display', 'block');
                    $("#yearly").ejRadioButton({ checked: true });

                    var everyXYearsObj = $("#everyXYears").data('ejNumericTextbox');
                    everyXYearsObj.option("value", item.Recurrence.YearlySchedule.YearsInterval);

                    $("#yearlyMonth option[value='" + item.Recurrence.YearlySchedule.MonthsOfTheYear + "']").attr("selected", true);
                    $('#yearlyMonth').selectpicker("refresh");

                    var yearlyDayObj = $("#yearlyDay").data('ejNumericTextbox');
                    yearlyDayObj.option("value", item.Recurrence.YearlySchedule.DayOfMonth);

                    break;
                case "YearlyDOW":
                    recurrenceType = "Yearly";
                    $('#yearlyScheduleOption').css('display', 'block');
                    $("#yearlyDOW").ejRadioButton({ checked: true });

                    var everyXYearsObj = $("#everyXYears").data('ejNumericTextbox');
                    everyXYearsObj.option("value", item.Recurrence.YearlyDowSchedule.YearsInterval);

                    $("#yearlyDOWWeek option[value='" + item.Recurrence.YearlyDowSchedule.WeeksOfTheMonth + "']").attr("selected", true);
                    $('#yearlyDOWWeek').selectpicker("refresh");

                    $("#yearlyDOWDay option[value='" + item.Recurrence.YearlyDowSchedule.DaysOfTheWeek + "']").attr("selected", true);
                    $('#yearlyDOWDay').selectpicker("refresh");

                    $("#yearlyDOWMonth option[value='" + item.Recurrence.YearlyDowSchedule.MonthsOfTheYear + "']").attr("selected", true);
                    $('#yearlyDOWMonth').selectpicker("refresh");

                    break;
            }
            $("#recurrenceType option[value='" + recurrenceType + "']").attr("selected", true);
            $('#recurrenceType').selectpicker("refresh");

            $("#enableSchedule").ejCheckBox({ checked: item.IsEnabled });
            $('#StartDate').ejDateTimePicker({ value: item.StartDateString });

            switch (item.EndType) {
                case "NoEndDate":
                    $("#noEndDate").ejRadioButton({ checked: true });
                    var startObj = $('#StartDate').data('ejDateTimePicker').model.value;
                    break;
                case "EndDate":
                    $("#endBy").ejRadioButton({ checked: true });
                    $('#EndDate').ejDateTimePicker({ value: item.EndDateString });
                    break;
                case "EndAfter":
                    $("#endAfter").ejRadioButton({ checked: true });
                    var occurenceCountObj = $("#occurenceCount").data('ejNumericTextbox');
                    occurenceCountObj.option("value", item.EndAfter);
                    break;
            }

            for (var i = 0; i < data.SubscribedUser.length; i++) {
                $("#UserSearch option[value='" + data.SubscribedUser[i] + "']").attr("selected", true);
                $('#UserSearch').selectpicker("refresh");
                var user = $("#UserSearch option[value='" + data.SubscribedUser[i] + "']").text();
                var userTile = $("<div>").attr("id", data.SubscribedUser[i]).attr("data-searchtype", "userSearch").addClass("SelectedShareItems");
                userTile.html("<div class='InstantSearch'><span class='details'>" + user + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
                $("#SelectedUsers").append(userTile);
            }

            oldUserSelected = $("#UserSearch").val();

            for (var i = 0; i < data.SubscribedGroup.length; i++) {
                $("#GroupSearch option[value='" + data.SubscribedGroup[i] + "']").attr("selected", true);
                $('#GroupSearch').selectpicker("refresh");
                var group = $("#GroupSearch option[value='" + data.SubscribedGroup[i] + "']").text();
                var groupTile = $("<div>").attr("id", data.SubscribedGroup[i]).attr("data-searchtype", "groupSearch").addClass("SelectedShareItems");
                groupTile.html("<div class='InstantSearch'><span class='details'>" + group + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
                $("#SelectedUsers").append(groupTile);
            }
            oldGroupSelected = $("#GroupSearch").val();

            var selectedCountGroup = $("#groupSearch_container .bootstrap-select li.selected").length;
            var allListCountGroup = $("#groupSearch_container .bootstrap-select li").length;
            var selectedCountUser = $("#userSearch_container .bootstrap-select li.selected").length;
            var allListCountUser = $("#userSearch_container .bootstrap-select li").length;
            if (selectedCountGroup === allListCountGroup) {
                $("#groupSearch_container .bs-select-all-custom").removeClass('bs-select-all-custom').addClass('bs-deselect-all-custom');
            }
            if (selectedCountUser === allListCountUser) {
                $("#userSearch_container .bs-select-all-custom").removeClass('bs-select-all-custom').addClass('bs-deselect-all-custom');
            }

            selectedItemsCount();
            switch (item.ExportType.toLowerCase()) {
                case "pdf":
                    $("#pdfExport").ejRadioButton({ checked: true });
                    break;
                case "word":
                    $("#wordExport").ejRadioButton({ checked: true });
                    break;
                case "excel":
                    $("#excelExport").ejRadioButton({ checked: true });
                    break;
                case "html":
                    $("#htmlExport").ejRadioButton({ checked: true });
                    break;
            }
            parent.$("#PopupContainer_wrapper").ejWaitingPopup("hide");
        }
    });

    $("#schedule_Submit").on('click', function () {
        if ($(".selected-recipients").children().length) {
            $("#PopupContainer_wrapper").ejWaitingPopup("show");
        }
        var scheduleItem = {};
        scheduleItem.ScheduleId = $(this).attr("data-schedule-id");
        scheduleItem.ItemId = $(this).attr("data-item-id");
        scheduleItem.ScheduleName = $('#schedule_name').val();
        scheduleItem.ExportType = $('input[name=exportFormats]:checked', '#exportFormatContainer').val().toString();
        switch ($('#recurrenceType').val().toString()) {
            case "Daily":
                if ($('#dailyEveryXdays').data('ejRadioButton').model.checked) {
                    scheduleItem.RecurrenceType = "Daily";
                    scheduleItem.RecurrenceInterval = $("#everyXDays").val();
                }
                else {
                    scheduleItem.RecurrenceType = "DailyWeekDay";
                }
                break;
            case "Weekly":
                scheduleItem.RecurrenceType = "Weekly";
                scheduleItem.RecurrenceInterval = $("#everyXWeeks").val();
                scheduleItem.Sunday = $("#sun").data("ejCheckBox").model.checked;
                scheduleItem.Monday = $("#mon").data("ejCheckBox").model.checked;
                scheduleItem.Tuesday = $("#tues").data("ejCheckBox").model.checked;
                scheduleItem.Wednesday = $("#wed").data("ejCheckBox").model.checked;
                scheduleItem.Thursday = $("#thu").data("ejCheckBox").model.checked;
                scheduleItem.Friday = $("#fri").data("ejCheckBox").model.checked;
                scheduleItem.Saturday = $("#sat").data("ejCheckBox").model.checked;
                break;
            case "Monthly":
                if ($('#monthly').data('ejRadioButton').model.checked) {
                    scheduleItem.RecurrenceType = "Monthly";
                    scheduleItem.DaysOfMonth = $('#monthlyDate').val();
                    scheduleItem.RecurrenceInterval = $('#monthlyEveryXMonths').val();
                }
                else {
                    scheduleItem.RecurrenceType = "MonthlyDOW";
                    scheduleItem.WeekOfMonth = $("#monthlyDOWWeek").val();
                    scheduleItem.DayOfWeek = $("#monthlyDOWDay").val();
                    scheduleItem.RecurrenceInterval = $('#monthlyDOWEveryXMonths').val();
                }
                break;
            case "Yearly":
                scheduleItem.RecurrenceInterval = $('#everyXYears').val();
                if ($('#yearly').data('ejRadioButton').model.checked) {
                    scheduleItem.RecurrenceType = "Yearly";
                    scheduleItem.DaysOfMonth = $('#yearlyDay').val();
                    scheduleItem.MonthOfYear = $('#yearlyMonth').val();
                }
                else {
                    scheduleItem.RecurrenceType = "YearlyDOW";
                    scheduleItem.WeekOfMonth = $("#yearlyDOWWeek").val();
                    scheduleItem.DayOfWeek = $("#yearlyDOWDay").val();
                    scheduleItem.MonthOfYear = $('#yearlyDOWMonth').val();
                }
                break;
        }
        scheduleItem.IsEnabled = $("#enableSchedule").data("ejCheckBox").model.checked;
        scheduleItem.StartDate = $('#StartDate').val();

        switch ($('input[name=EndType]:checked', '#scheduleEndType').val().toString()) {
            case "never":
                scheduleItem.ScheduleEndType = "NoEnd";
                break;
            case "endAfter":
                scheduleItem.ScheduleEndType = "EndAfter";
                scheduleItem.RecurrenceFactor = $("#occurenceCount").val();
                break;
            case "endBy":
                scheduleItem.ScheduleEndType = "EndBy";
                scheduleItem.EndDate = $('#EndDate').val();
                break;
        }

        var selectedItems = $(".selected-recipients").children();

        var userlist = Array();
        var grouplist = Array();
        var a = 0, b = 0, c = 0;
        for (var i = 0; i < selectedItems.length; i++) {
            if (selectedItems[i].getAttribute("data-searchtype") == "userSearch") {
                if (a != -1) {
                    userlist[a] = selectedItems[i].id;
                    a++;
                }

            }
            if (selectedItems[i].getAttribute("data-searchtype") == "groupSearch") {
                if (b != -1) {
                    grouplist[b] = selectedItems[i].id;
                    b++;
                }

            }
        }
        if (selectedItems.length) {

            $.ajax({
                type: "POST",
                url: parent.baseurl + "/scheduler/UpdateSchedule",
                data: { scheduleList: JSON.stringify({ ScheduleItem: scheduleItem, UserList: userlist, GroupList: grouplist }) },
                beforeSend: function () {
                    parent.$("#PopupContainer_wrapper").ejWaitingPopup("show");
                },
                success: function (data) {
                    parent.$("#PopupContainer_wrapper").ejWaitingPopup("hide");
                    var scheduleName = $(".SchedulePopupTitle").text();
                    closePopupContainer();
                    parent.messageBox("su-calendar-1", scheduleName, "Schedule has been updated successfully.", "success", function () {
                        parent.onCloseMessageBox();
                        parent.refreshScheduleGrid();
                    });
                }
            });
        }
        else {
            $("#selectedUsersValidation").css("visibility", "visible");
        }
    });

    $(".share-popup .bs-deselect-all").after("<div class='bs-select-all-custom'><span>Select All</span><span class='bs-select-custom-tick glyphicon glyphicon-ok'></span></div>");
    $("#userSearch_container").on('click', '.bs-select-all-custom', function (e) {
        $("#userSearch_container").addClass("valueChanged");
        $('#UserSearch').data("selectpicker").selectAll();
        $(this).removeClass('bs-select-all-custom').addClass('bs-deselect-all-custom');
        $($(this).children("span")[0]).text("Clear All");
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#groupSearch_container").on('click', '.bs-select-all-custom', function (e) {
        $("#groupSearch_container").addClass("valueChanged");
        $('#GroupSearch').data("selectpicker").selectAll();
        $(this).removeClass('bs-select-all-custom').addClass('bs-deselect-all-custom');
        $($(this).children("span")[0]).text("Clear All");
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#userSearch_container").on('click', '.bs-deselect-all-custom', function (e) {
        $("#userSearch_container").addClass("valueChanged");
        $('#UserSearch').data("selectpicker").deselectAll();
        $(this).removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        $($(this).children("span")[0]).text("Select All");
        $(".SelectedShareItems[data-searchtype='userSearch']").remove();
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#groupSearch_container").on('click', '.bs-deselect-all-custom', function (e) {
        $("#groupSearch_container").addClass("valueChanged");
        $('#GroupSearch').data("selectpicker").deselectAll();
        $(this).removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        $($(this).children("span")[0]).text("Select All");
        $(".SelectedShareItems[data-searchtype='groupSearch']").remove();
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#userSearch_container").on('click', '.bootstrap-select li a', function (e) {
        $("#userSearch_container").addClass("valueChanged");;
        var selectedCount = $("#userSearch_container .bootstrap-select li.selected").length;
        var allListCount = $("#userSearch_container .bootstrap-select li").length;

        if (selectedCount == allListCount) {
            $($('#userSearch_container div.bs-select-all-custom').children("span")[0]).text("Clear All");
            $('#userSearch_container div.bs-select-all-custom').removeClass("bs-select-all-custom").addClass("bs-deselect-all-custom");
        }
        if ($(this).parent().hasClass("selected")) {
            var selectedUser = $("#UserSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            var userTile = $("<div>").attr("id", $(selectedUser).val()).attr("data-searchtype", "userSearch").addClass("SelectedShareItems");
            userTile.html("<div class='InstantSearch'><span class='details'>" + $(selectedUser).text() + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
            $("#SelectedUsers").append(userTile);
        }
        else {
            var selectedUser = $("#UserSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            $(".SelectedShareItems[id='" + $(selectedUser).val() + "']").remove();
            $($('#userSearch_container .bs-deselect-all-custom').children("span")[0]).text("Select All");
            $("#userSearch_container .bs-deselect-all-custom").removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        }
        $(this).parent().addClass("active");
        selectedItemsCount();
        e.stopPropagation();
    });

    $("#groupSearch_container").on('click', '.bootstrap-select .dropdown-menu .selectpicker li a', function (e) {
        $("#groupSearch_container").addClass("valueChanged");;
        var selectedCount = $("#groupSearch_container .bootstrap-select li.selected").length;
        var allListCount = $("#groupSearch_container .bootstrap-select li").length;
        if (selectedCount == allListCount) {
            $($('#groupSearch_container div.bs-select-all-custom').children("span")[0]).text("Clear All");
            $('#groupSearch_container div.bs-select-all-custom').removeClass("bs-select-all-custom").addClass("bs-deselect-all-custom");
        }

        if ($(this).parent().hasClass("selected")) {
            var selectedGroup = $("#GroupSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            var groupTile = $("<div>").attr("id", $(selectedGroup).val()).attr("data-searchtype", "groupSearch").addClass("SelectedShareItems");
            groupTile.html("<div class='InstantSearch'><span class='details'>" + $(selectedGroup).text() + "</span><div style='width:auto' class='instant-cancel'><span class='su su-close i-selected-cancel'/></div></div>");
            $("#SelectedUsers").append(groupTile);
        }
        else {
            var selectedGroup = $("#GroupSearch").find("option")[parseInt($(this).parent().attr("data-original-index"))];
            $(".SelectedShareItems").filter("[data-searchtype='groupSearch']").filter("#" + $(selectedGroup).val()).remove();
            $($('#groupSearch_container .bs-deselect-all-custom').children("span")[0]).text("Select All");
            $("#groupSearch_container .bs-deselect-all-custom").removeClass('bs-deselect-all-custom').addClass('bs-select-all-custom');
        }
        $(this).parent().addClass("active");
        selectedItemsCount();
        e.stopPropagation();
    });

}


function scheduleNameCheck(scheduleId, scheduleName) {
    $('#scheduleNameErrorContainer').css('display', 'none');
    $("#scheduleNameErrorContainer").parent().append($("<span class='col-sm-4 no-padding loader-gif' style='display:block;margin-left:-73px'></span>"));
    $.ajax({
        type: "POST",
        url: parent.baseurl + "/scheduler/CheckScheduleNameExist",
        data: { scheduleId: scheduleId, scheduleName: scheduleName },
        success: function (data) {
            if (data.Result) {
                $('span.loader-gif').remove();
                $("#schedule_name").closest('div').addClass("has-error");
                $('#scheduleNameErrorContainer').css('display', 'block');
                $('#scheduleNameErrorContainer').css('margin-left', '-30px');
                $('#scheduleNameValidator').html("Schedule name already exists");
            } else {
                $("#schedule_name").closest('div').removeClass("has-error");
                $('span.loader-gif').remove();
                $('#scheduleNameErrorContainer').css('display', 'none');
            }
        }
    });
}

function getAllStaticData() {
    if (window.userList == undefined) {
        $.ajax({
            type: "POST",
            url: parent.baseurl + "/home/getallactivegroupsandusers",
            data: {},
            async: false,
            success: function (result) {
                window.userList = result.data.users;
                window.groupList = result.data.groups;
            }
        });
    }
}

function refreshScheduleGridItem(scheduleId) {
    //change the loading icon to play icon
    var scheduleGridObj = $("#scheduleGrid").data("ejGrid");
    for (var i = 0; i < scheduleGridObj.model.currentViewData.length; i++) {
        if (scheduleGridObj.model.currentViewData[i].Id == scheduleId) {
            $('span span[data-scheduleid =' + scheduleId + ']').removeClass('loader-gif').addClass('su-play-folder');
        }
    }
}

function selectedItemsCount() {
    $("#selectedUsersInfo").html("<span class='pull-left'>" + $(".SelectedShareItems[data-searchtype='userSearch']").length + " User(s) " + $(".SelectedShareItems[data-searchtype='groupSearch']").length + " Group(s)<span>").css({ "padding-top": "10px", "padding-left": "15px", "margin-top": "0" });
}

function enableScheduleOption() {
    $(".subscribe-popup-body").css("display", "none");
    $(".schedule-popup-body").fadeIn();
    $('#next_Container').css("display", "none");
    $('#submit_Container').css("display", "none");
    $('#next_Container').css("display", "block");
}

function enableSubscribeOption() {
    $(".schedule-popup-body").css("display", "none");
    $(".subscribe-popup-body").fadeIn();
    $('#next_Container').css("display", "none");
    $('#submit_Container').css("display", "block");
}

function validateSchedule() {
    var startDateTimeObj = $('#StartDate').data("ejDateTimePicker");
    var scheduleName = $('#schedule_name').val();
    if (!($('#scheduleNameErrorContainer').css('display') == "block") && !($('body .loader-gif').length)) {
        if (!scheduleName) {
            $('#scheduleNameErrorContainer').css('display', 'block');
            $('#scheduleNameErrorContainer').css('margin-left', '-44px');
            $('#scheduleNameValidator').html("Please enter schedule name");
            return false;
        }
        else if (!parent.IsValidName("name", scheduleName)) {
            $('#scheduleNameErrorContainer').css('display', 'block');
            $('#scheduleNameErrorContainer').css('margin-left', '-44px');
            $('#scheduleNameValidator').html("Please avoid special characters");
            return false;
        }
        else {
            $('#scheduleNameErrorContainer').css('display', 'none');
        }

        switch ($('#recurrenceType').val().toString()) {
            case "Daily":
                break;
            case "Weekly":
                if (!$('#daysCheckBox input[type="checkbox"]').is(':checked')) {
                    $('#weeklyDayErrorContainer').css('display', 'block');
                    $('#weeklyDaysValidator').html("Please select at least one day.");
                    return false;
                }
                else {
                    $('#weeklyDayErrorContainer').css('display', 'none');
                }
                break;
            case "Monthly":
                break;
            case "Yearly":
                var currentMonth = $('#yearlyMonth').val().toString();
                var day = parseInt($('#yearlyDay').val());
                var dayObject = $("#yearlyDay").data("ejNumericTextbox");
                switch (currentMonth) {
                    case "February":
                        if (day > 28) {
                            dayObject.option("value", 28);
                        }
                        break;

                    case "April":
                    case "June":
                    case "September":
                    case "November":
                        if (day > 30) {
                            dayObject.option("value", 30);
                        }
                        break;

                    case "January":
                    case "March":
                    case "May":
                    case "July":
                    case "August":
                    case "October":
                    case "December":
                        if (day > 31) {
                            dayObject.option("value", 31);
                        }
                        break;
                }
                break;
        }

        if (!startDateTimeObj.model.dateTimeFormat == "M/d/yyyy h:mm tt") {
            $('#startDateErrorContainer').css('display', 'block');
            $('#startDateValidator').html("Please enter schedule name");
            return false;
        }
        else {
            $('#startDateErrorContainer').css('display', 'none');
        }

        switch ($('input[name=EndType]:checked', '#scheduleEndType').val().toString()) {
            case "endBy":
                var endDateTimeObj = $('#EndDate').data("ejDateTimePicker");
                if (!endDateTimeObj.model.dateTimeFormat == "M/d/yyyy h:mm tt") {
                    $('#endDateErrorContainer').css('display', 'block');
                    $('#endDateValidator').html("Please enter schedule name");
                    return false;
                }
                else {
                    $('#endDateErrorContainer').css('display', 'none');
                }
                break;
        }
        return true;
    } else {
        return false;
    }
}

function validateSubscriber() {
    if (!$("#SelectedUsers").children().length > 0) {
        $('#selectedRecipientErrorContainer').css('display', 'block');
        $('#selectedRecipientValidator').html("Please select atleast one recipient");
        return false;
    } else {
        $('#selectedRecipientErrorContainer').css('display', 'none');
    }
}

function validateDateTimePicker() {
    var startDate = $('#StartDate').data("ejDateTimePicker");
    var endDate = $('#EndDate').data("ejDateTimePicker");
    var startValue = $('#StartDate').data("ejDateTimePicker").getValue();
    var mindateVal = new Date(startValue);
    $('#EndDate').ejDateTimePicker({ minDateTime: new Date(mindateVal.getFullYear(), mindateVal.getMonth(), mindateVal.getDate()), value: new Date(startValue) });
}

function DateTimeParser(dateTime) {
    if (dateTime == undefined) {
        return "";
    }
    var pattern = /Date\(([^)]+)\)/;
    var resultStrs = pattern.exec(dateTime);

    if (resultStrs != null) {
        var dtObj = new Date(parseInt(resultStrs[1]));
        return dtObj;
    }
    else {
        return dateTime;
    }
}

function enableScheduleWaitingPopup(id) {
    ShowWaitingProgress(id, "show");
}

function disableScheduleWaitingPopup() {
    $(".reports-waitingPopup").remove();
}

$(document).on('focusout', "#schedule_name", function (event) {
    var scheduleName = $("#schedule_name").val();   
    var scheduleId = $("#schedule_Submit").attr("data-schedule-id");
    if ($.trim(scheduleName) != '') {
        $("#schedule_name").closest('div').removeClass("has-error");
        $('#scheduleNameErrorContainer').css('display', 'none');
        if (scheduleName) {
            scheduleNameCheck(scheduleId, scheduleName);
        } else {
            $('#scheduleNameErrorContainer').css('display', 'none');
        }
    }
    else {
        $("#schedule_name").closest('div').addClass("has-error");
        $('#scheduleNameErrorContainer').css('display', 'block');
        $('#scheduleNameErrorContainer').css('margin-left', '-30px');
        $('#scheduleNameValidator').html("Please enter schedule name");
    }
});

$(document).on('keyup', '#schedule_name', function (event) {
    if($.trim($("#schedule_name").val())!='')
    {
        $("#schedule_name").closest('div').removeClass("has-error");
        $('#scheduleNameErrorContainer').css('display', 'none');
    }
    else {
        $("#schedule_name").closest('div').addClass("has-error");
        $('#scheduleNameErrorContainer').css('display', 'block');
        $('#scheduleNameErrorContainer').css('margin-left', '-30px');
        $('#scheduleNameValidator').html("Please enter schedule name");
    }
});


$(document).on('click', "#schedule_Next", function (event) {
    if (validateSchedule()) {
        enableSubscribeOption();
    }
});

$(document).on('click', "#schedule_Back", function (event) {
    $("#selectedUsersValidation").css("visibility", "hidden");
    enableScheduleOption();
});

$(document).on('change', '#recurrenceType', function () {
    var selected = $(this).find("option:selected").val();
    $('#dailyScheduleOption').css('display', 'none');
    $('#weeklyScheduleOption').css('display', 'none');
    $('#monthlyScheduleOption').css('display', 'none');
    $('#yearlyScheduleOption').css('display', 'none');
    switch (selected.toString()) {
        case "Daily":
            $('#dailyScheduleOption').css('display', 'block');
            break;
        case "Weekly":
            $('#weeklyScheduleOption').css('display', 'block');
            break;
        case "Monthly":
            $('#monthlyScheduleOption').css('display', 'block');
            break;
        case "Yearly":
            $('#yearlyScheduleOption').css('display', 'block');
            break;
    }
});

$(document).on("click", "#SubscribersPanel .i-selected-cancel", function (event) {
    var key = $(this).parents(".SelectedShareItems").attr("id");
    var searchType = $(this).parents(".SelectedShareItems").attr("data-searchtype");
    if (searchType == "userSearch") {
        currentElementIndex = $("#UserSearch").find("[value='" + key + "']").index();
        $("#userSearch_container .bootstrap-select li").filter("[data-original-index='" + currentElementIndex + "']").find("a").click();
    }
    else {
        currentElementIndex = $("#GroupSearch").find("[value='" + key + "']").index();
        $("#groupSearch_container .bootstrap-select li").filter("[data-original-index='" + currentElementIndex + "']").find("a").click();
    }
    selectedItemsCount();
});

$(document).on('hide.bs.dropdown', '#userSearch_container', function (e) {
    if ($("#userSearch_container").hasClass("valueChanged")) {
        $("#userSearch_container").removeClass("valueChanged");
        e.stopPropagation();
    }
});
$(document).on('hide.bs.dropdown', '#groupSearch_container', function (e) {
    if ($("#groupSearch_container").hasClass("valueChanged")) {
        $("#groupSearch_container").removeClass("valueChanged");
        e.stopPropagation();
    }
});
$(document).on("click", "#schedule_Submit_Cancel,#schedulePopup,#schedule_Next_Cancel", function (event) {
    closePopupContainer();
});
$(document).keyup(function (e) {
    if (e.keyCode == 27) closePopupContainer();
});
function closePopupContainer() {
    parent.$("#PopupContainer").ejDialog("close");
    parent.$("#PopupContainer_wrapper").ejWaitingPopup("hide");
}