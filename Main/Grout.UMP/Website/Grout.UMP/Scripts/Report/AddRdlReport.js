
$(function () {
    window.isEditPopup = false;
    $(document).on("click", "#save_rdl_with_datasource", function (e) {
        updateReportWithDataSource();
    });
    $(document).on("click", ".select-datasource-btns", function (e) {
        var selectedDataSource = "";
        if ($(this).val().toLowerCase() == "update") {
            selectedDataSource = $(this).siblings("span.selected-datasource").attr("data-key");
        }

        onSelectDataSourceClick($(this).data("name"), selectedDataSource);

    });

    $(document).on("keyup", "#file_name", function () {
        if (onSelectValidation()) {
            $("#publish_report").removeAttr("disabled");
        }
        else {
            $("#publish_report").attr("disabled", "disabled");
        }
    });

    $("#createReportPopupHolder input").keypress(function (e) {
        if (e.which == 13) {
            addRDLReport();
        }
    });

    $(document).on("keyup", ".edit-text-fields", function (e) {
        if (window.isEditPopup) {
            if ($(this).attr("id") === "file_name") {
                if (window.editReportData.ReportName !== $(this).val()) {
                    window.editReportData.IsReportNameChanged = true;
                }
                else {
                    window.editReportData.IsReportNameChanged = false;
                }
            }
            if ($(this).attr("id") === "file_description") {
                if (window.editReportData.ReportDescription !== $(this).val()) {
                    window.editReportData.IsReportDescriptionChanged = true;
                }
                else {
                    window.editReportData.IsReportDescriptionChanged = false;
                }
            }
            reportEditValidation();
        }
        else {
            (onSelectValidation()) ? $("#publish_report").removeAttr("disabled") : $("#publish_report").attr("disabled", "disabled");
        }
    });

    $(document).on("change", "#selected_category", function (e) {
        if (window.isEditPopup) {
            if (window.editReportData.CategoryId !== $(this).val()) {
                window.editReportData.IsCategoryChanged = true;
            }
            else {
                window.editReportData.IsCategoryChanged = false;
            }
            reportEditValidation();
        }
    });

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value);
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $(".add-report-form").validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true;},
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "file_name": {
                isRequired: true,
                isValidName: true
            }
            
        },
        messages: {
            "file_name": {
                isRequired: "Please enter report name"
            }
        },
        highlight: function (element) {
            $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').find("span.validation-messages").html("");
        },
        errorPlacement: function (error, element) {            
            $(element).closest('td').find("span.validation-messages").html(error.html());
        }
    });
    $("#editReportForm").validate({
        errorElement: 'span',
        onkeyup: function (element,event) { if (event.keyCode != 9) $(element).valid(); else true;},
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "file_name": {
                isRequired: true,
                isValidName: true
            }

        },
        messages: {
            "file_name": {
                isRequired: "Please enter report name"
            }
        },
        highlight: function (element) {
            $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').find("span.validation-messages").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').find("span.validation-messages").html(error.html());
        }
    });
});

function reportEditValidation() {
    if ((window.editReportData.IsReportNameChanged || window.editReportData.IsCategoryChanged || window.editReportData.IsReportDescriptionChanged || window.editReportData.IsDataSourceChanged) && !window.editReportData.IsFileChanged) {
        window.isEdited = true;
    }
    else if (window.editReportData.IsFileChanged && onSelectValidation()) {
        window.isEdited = true;
    } else {
        window.isEdited = false;
    }
    if (window.isEdited && $("#file_name").val() != "") {
        $("#publish_report").removeAttr("disabled");
    } else {
        $("#publish_report").attr("disabled", true);
    }
}

function addRDLReport() {
    if ($('#publish_report').attr("disabled")!==undefined) {
        return;
    }
    var isValidForm = true;
    var iframe_content = $("#report_upload_iframe").contents().find("body");
    var fileName = $("#file_name").val().trim();
    if ($("#selected_category").val() == null || $("#selected_category").val() == "") {
        $("#CategoryMessage").removeClass("hide").addClass("show");
        isValidForm = false;
    } else {
        $("#CategoryMessage").removeClass("show").addClass("hide");
    }
    if (!$(".add-report-form,#editReportForm").valid())
        isValidForm = false;
    if ($("#publishedFileName").attr("value") == "" || $("#publishedFileName").attr("value") == "none") {
        iframe_content.find("#upload_browse_file").closest('span').addClass("has-error");
        iframe_content.find("#browse_file").closest('div').addClass("error-file-upload");
        $("#uploadValidation").html("Please upload the report");
        isValidForm = false;
    } else {
        iframe_content.find("#upload_browse_file").closest('span').removeClass("has-error");
        $("#uploadValidation").html("");
    }

    if (isValidForm) {
        window.parent.$("#report_popup_wrapper").ejWaitingPopup("show");
        var isAlreadyExist = false;
        var postData = {};
        var dataSourceList;
        var dataSources = [];
        var dataSource = 0;
        var categoryId = $("#selected_category option:selected").val();
        if (window.isEditPopup) {
            if (window.editReportData.IsReportNameChanged) {
                $.ajax({
                    type: "POST",
                    url: "/reports/IsItemExistInSameCategory",
                    data: { categoryId: categoryId, itemName: fileName },
                    async: false,
                    success: function (data) {
                        if (data.Data) {
                            window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                            $("#report_name_validation_error").closest('td').addClass("has-error");
                            $("#report_name_validation_error").html("A report with the same name already exists in this category");
                            isAlreadyExist = true;
                            return;
                        }
                    }
                });
            }
            if (!isAlreadyExist) {
                $("#report_name_validation_error").closest('td').removeClass("has-error");
                postData.reportId = window.editReportData.ReportId;
                postData.isReportNameChanged = window.editReportData.IsReportNameChanged;
                if (window.editReportData.IsReportNameChanged) {
                    postData.fileName = $("#file_name").val();
                }
                postData.isCategoryChanged = window.editReportData.IsCategoryChanged;
                if (window.editReportData.IsCategoryChanged) {
                    postData.categoryId = categoryId;
                }
                postData.isReportDescriptionChanged = window.editReportData.IsReportDescriptionChanged;
                if (window.editReportData.IsReportDescriptionChanged) {
                    postData.fileDescription = $("#file_description").val();
                }
                postData.isFileChanged = window.editReportData.IsFileChanged;
                if (window.editReportData.IsFileChanged) {
                    postData.temporaryFileName = $("#publishedFileName").val();
                    dataSourceList = $("#datasource_list_table tr");
                    if (dataSourceList.length !== 0) {
                        for (dataSource = 0; dataSource < dataSourceList.length; dataSource++) {
                            dataSources.push({ Name: $(dataSourceList[dataSource]).attr("data-key"), DataSourceId: $(dataSourceList[dataSource]).find("span.selected-datasource").attr("data-key") });
                        }
                    }
                    postData.versionComment = $("#comment").val();
                }
                postData.isDataSourceChanged = window.editReportData.IsDataSourceChanged;
                if (!window.editReportData.IsFileChanged && window.editReportData.IsDataSourceChanged) {
                    dataSourceList = $("#datasource_list_table tr");
                    if (dataSourceList.length !== 0) {
                        for (dataSource = 0; dataSource < dataSourceList.length; dataSource++) {
                            if ($(dataSourceList[dataSource]).attr("data-exist-id") !== $(dataSourceList[dataSource]).find("span.selected-datasource").attr("data-key")) {
                                dataSources.push({ Name: $(dataSourceList[dataSource]).attr("data-key"), DataSourceId: $(dataSourceList[dataSource]).find("span.selected-datasource").attr("data-key") });
                            }
                        }
                    }
                }
                postData.dataSourceList = JSON.stringify(dataSources);
                $.ajax({
                    type: "POST",
                    url: "/reports/editreport",
                    data: postData,
                    success: function (data) {
                        window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                        if (data.result.Status) {
                            closeNewRDLAddPopup();
                            parent.messageBox("su-report", "Update Report", "Report has been updated successfully.", "success", function () {
                                var gridName = window.parent.$('#itemGridContainer').attr("data-grid-name");
                                if (gridName == "reports") {
                                    parent.RefreshCategoryListAfterAction(categoryId);
                                }
                                parent.onCloseMessageBox();
                            });
                        } else {
                            closeNewRDLAddPopup();
                            parent.messageBox("su-report", "Update Report", "Failed to update report. Please try again later.", "success");
                        }
                    },
                    error: function (e) {
                        window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                        closeNewRDLAddPopup();
                        parent.messageBox("su-report", "Update Report", "Failed to update report. Please try again later.", "success");
                    }
                });
            }
        } else {
            $.ajax({
                type: "POST",
                url: "/reports/IsItemExistInSameCategory",
                data: { categoryId: categoryId, itemName: $("#file_name").val() },
                async: false,
                success: function (data) {
                    if (data.Data) {
                        window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                        $("#report_name_validation_error").html("A report with the same name already exists in this category");
                        isAlreadyExist = true;
                        return;
                    }
                }
            });
            if (!isAlreadyExist) {
                dataSourceList = $("#datasource_list_table tr");
                if (dataSourceList.length !== 0) {
                    for (dataSource = 0; dataSource < dataSourceList.length; dataSource++) {
                        dataSources.push({ Name: $(dataSourceList[dataSource]).attr("data-key"), DataSourceId: $(dataSourceList[dataSource]).find("span.selected-datasource").attr("data-key") });
                    }
                    postData.dataSourceList = JSON.stringify(dataSources);
                }
                postData.fileName = $("#file_name").val();
                postData.description = $("#file_description").val();
                postData.selectedCategoryId = categoryId;
                postData.temporaryFileName = $("#publishedFileName").val();
                if (dataSourceList.length !== 0) {
                    $.ajax({
                        type: "POST",
                        url: "/reports/addsharedrdlreport",
                        data: postData,
                        success: function (data) {
                            window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                            if (data.result.Status) {
                                closeNewRDLAddPopup();
                                parent.messageBox("su-report", "Add Report", "Report has been added successfully.", "success", function () {
                                    var gridName = window.parent.$('#itemGridContainer').attr("data-grid-name");
                                    if (gridName == "reports") {
                                        parent.RefreshCategoryListAfterAction(categoryId);
                                    }
                                    parent.onCloseMessageBox();
                                });
                            } else {
                                closeNewRDLAddPopup();
                                parent.messageBox("su-report", "Add Report", "Failed to add report. Please try again later.", "success");
                            }
                        },
                        error: function (e) {
                            window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                            closeNewRDLAddPopup();
                            parent.messageBox("su-report", "Add Report", "Failed to add report. Please try again later.", "success");
                        }
                    });
                } else {
                    $.ajax({
                        type: "POST",
                        url: "/reports/addembeddedrdlreport",
                        data: postData,
                        success: function (data) {
                            window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                            if (data.result.Status) {
                                closeNewRDLAddPopup();
                                parent.messageBox("su-report", "Add Report", "Report has been added successfully.", "success", function () {
                                    var gridName = window.parent.$('#itemGridContainer').attr("data-grid-name");
                                    if (gridName == "reports") {
                                        parent.RefreshCategoryListAfterAction(categoryId);
                                    }
                                    parent.onCloseMessageBox();
                                });
                            } else {
                                closeNewRDLAddPopup();
                                parent.messageBox("su-report", "Add Report", "Failed to add report. Please try again later.", "success");
                            }
                        },
                        error: function (e) {
                            window.parent.$("#report_popup_wrapper").ejWaitingPopup("hide");
                            closeNewRDLAddPopup();
                            parent.messageBox("su-report", "Add Report", "Failed to add report. Please try again later.", "success");
                        }
                    });
                }
            }
        }
    }
}



function closeNewRDLAddPopup() {
    window.parent.$('#report_popup').ejDialog("close");
    if ($("#publishedFileName").val() != "none") {
        $.ajax({
            type: "POST",
            url: "/reports/deletetemporaryrdlreport",
            async: false,
            data: { fileName: $("#publishedFileName").val() },
            success: function (data) {
                $("#publishedFileName").val("");
            }
        });
    }
}

function onSelectDataSourceClick(sourceName, selectedDataSource) {
    parent.$("#select_datasource_popup").ejDialog("open");
    parent.$("#select_datasource_popup_wrapper").ejWaitingPopup("show");
    parent.$("#datasource_iframe").attr("src", "reports/reportselectdatasource?sourceName=" + sourceName + "&selectedDataSource=" + selectedDataSource);
}

function onSelectionOfDataSource(element, object) {
    $(element).find(".selected-datasource").attr("data-key", object.Id).html(object.Name);
    $(element).find(".select-datasource-btns").attr("value", "Update");
    if (window.isEditPopup) {
        if (!window.editReportData.IsFileChanged) {
            dataSourceSelectionValidationOnEdit();
        } else {
            reportEditValidation();
        }
    }
    else if (onSelectValidation()) {
        $("#publish_report").removeAttr("disabled");
    }
}

function dataSourceSelectionValidationOnEdit() {
    var dataSources = $("#datasource_list_table tr");
    for (var dataSource = 0; dataSource < dataSources.length; dataSource++) {
        if ($(dataSources[dataSource]).attr("data-exist-id") !== $(dataSources[dataSource]).find(".selected-datasource").attr("data-key")) {
            window.editReportData.IsDataSourceChanged = true;
            reportEditValidation();
            return;
        }
    }
    window.editReportData.IsDataSourceChanged = false;
    reportEditValidation();
}

function onSelectValidation() {
    if ($("#report_upload_iframe").contents().find("#rdl_file_upload_btn").attr("disabled") == undefined || $("#report_upload_iframe").contents().find("#upload_browse_file").val().toLowerCase() == "choose file") {
        return false;
    }
    if ($("#file_name").val() == "") {
        return false;
    }
    var dataSources = $(".select-datasource-btns");
    for (var dataSource = 0; dataSource < dataSources.length; dataSource++) {
        if ($(dataSources[dataSource]).attr("value").toLowerCase() == "select") {
            return false;
        }
    }
    return true;
}

function createEditRdlPopup(data) {
    //window.parent.$('#report_popup_wrapper').ejWaitingPopup("show");
    window.editReportData = data;
    window.isEditPopup = true;
    window.editReportData.IsReportNameChanged = false;
    window.editReportData.IsCategoryChanged = false;
    window.editReportData.IsReportDescriptionChanged = false;
    window.editReportData.IsFileChanged = false;
    window.editReportData.IsDataSourceChanged = false;
    $("#version_comment").show();
    $("#comment").attr("readonly", true);
    $("#file_name").val(data.ReportName);
    $("#file_description").val(data.ReportDescription);
    $("#rdl_filename").val(data.ReportFileName);
    $("#publishedFileName").val(data.ReportFileName);
    $("#report_upload_iframe").contents().find("#upload_browse_file").attr("value", data.ReportFileName);
    $('#selected_category').val(data.CategoryId.toLowerCase());
    $("#selected_category").selectpicker("refresh");
    if (data.DataSources.length != 0) {
        var html = "";
        for (var dataSource = 0; dataSource < data.DataSources.length; dataSource++) {
            html += '<tr data-key="' + data.DataSources[dataSource].DataSourceName + '" data-exist-id="' + data.DataSources[dataSource].DataSourceId + '"><td><span class="rdl-datasource-name">' + data.DataSources[dataSource].DataSourceName + '</span> : <span class="selected-datasource" data-key="' + data.DataSources[dataSource].DataSourceId + '">' + data.DataSources[dataSource].Name + '</span><input type="button" class="btn btn-info select-datasource-btns small-left-margin" value="Update" data-key="' + data.DataSources[dataSource].DataSourceId + '" data-name="' + data.DataSources[dataSource].DataSourceName + '"></td></tr>';
        }
        $("#datasource_list_table").html(html);
        $("#datasource_list").show();
    }
    //window.parent.$('#report_popup_wrapper').ejWaitingPopup("hide");
}

function reportUploadComplete(reportUploadStatus) {
    if (reportUploadStatus.Status) {
        $("#uploadValidation").html("<span class='ReportUploadSuccessMessage'>Report has been uploaded successfully.</span>");
        $("#publishedFileName").val(reportUploadStatus.UploadedReportName);
        if (reportUploadStatus.IsShared) {
            var html = "";
            for (var dataSource = 0; dataSource < reportUploadStatus.SharedDataSourceList.length; dataSource++) {
                html += '<tr data-key="' + reportUploadStatus.SharedDataSourceList[dataSource] + '"><td><span class="rdl-datasource-name">' + reportUploadStatus.SharedDataSourceList[dataSource] + '</span> : <span class="selected-datasource"></span><input type="button" class="btn btn-primary select-datasource-btns small-left-margin" value="Select" data-name="' + reportUploadStatus.SharedDataSourceList[dataSource] + '"></td></tr>';
            }
            $("#datasource_list_table").html(html);
            $("#datasource_list").show();
        }
        else {
            $("#datasource_list").hide();
            $("#publish_report").removeAttr("disabled");
        }
    }
    else {
        $("#uploadValidation").html("<span class='ErrorMessage'>Error while uploading report. Please try again.</span>");
    }
}

$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        $('.PopupClose').click();
        window.parent.$("#createButton").focus();
    }
});