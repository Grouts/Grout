var isKeyUp = false;
$(document).ready(function () {
    var custompath;
    var currentDate = $.now();
    var uploadFileName;
    var extension;
    $("#avatarUploadBox").ejDialog({
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "425px",
        width: "600px",
        enableModal: true,
        close: "DialogBoxClose",
        closeOnEscape: true,
        showHeader: false,
        showRoundedCorner: true
    });
    $("#userprofile_delete_confirmation").ejDialog({
        width: "400px",
        showOnInit: false,
        allowDraggable: false,
        enableResize: false,
        height: "187px",
        title: "Delete User",
        enableModal: true,
        close: "onDeleteDialogClose",
        closeOnEscape: false,
        open: "onDeleteDialogOpen"
    });

    $.validator.addMethod("isValidEmail", function (value, element) {
        if (isKeyUp)
            return true;
        else
            return IsEmail(value)
    }, "Invalid email");

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value)
    }, "Please avoid special characters");

    $('.user_profile_form').validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) { isKeyUp = true; $(element).valid(); isKeyUp = false } else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "email-address": {
                isRequired: true,
                isValidName: true,
                isValidEmail: true
            },
            "first-name": {
                isRequired: true,
                isValidName: true
            }

        },
        highlight: function (element) {
            $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').next("td").find("span").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').next("td").find("span").html(error.html()).css("display", "block");
        },
        messages: {
            "email-address": {
                isRequired: "Please enter email address"
            },
            "first-name": {
                isRequired: "Please enter first name"
            }
        }

    });

    $('.change_password_form').validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "new-password": {
                required: true
            },
            "confirm-password": {
                required: true
            }

        },
        highlight: function (element) {
            $(element).closest('td').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('td').removeClass('has-error');
            $(element).closest('td').next("td").find("span").html("");
        },
        errorPlacement: function (error, element) {
            $(element).closest('td').next("td").find("span").html(error.html());
        },
        messages: {
            "new-password": {
                required: "Please enter your new password"
            },
            "confirm-password": {
                required: "Please confirm your new password"
            }
        }

    });

    $(document).on('click', "#activateButtonClick", function () {
        $("#activateButtonClick").attr("disabled", true);
        var id = $(this).attr("id");
        $.ajax({
            type: "POST", url: "/UserManagement/ActivateUser", data: { username: $("#user_name").val(), email: $("#user_email").val(), firstname: $("#user_firstname").val() },
            success: function (data) {
                $("#activateButtonClick").attr("disabled", false);
                if ($.type(data) == "object") {
                    if (data.Data.result == "success") {
                        $("#" + id).hide();
                        $('<input class="btn btn-primary password_save_button" type="button" id="resendButtonClick" value="Resend Activation Code" title="" />').insertAfter('#' + id);
                        CheckMailSettingsAndNotify("Activation code has been generated successfully. As email settings are not configured we are not able to send activation email to the user. <a href='/administration/e-mail-settings'>Please click here to configure the email settings.</a>", $("#AlertMessage"), "Activation code has been sent successfully.");
                    }
                } else {

                }
            }
        });
    });

    $(document).on('click', '#resendButtonClick', function () {
        $("#resendButtonClick").attr("disabled", true);
        var id = $(this).attr("id");
        $.ajax({
            type: "POST", url: "/UserManagement/resendactivationcode", data: { username: $("#user_name").val(), email: $("#user_email").val(), firstname: $("#user_firstname").val() },
            success: function (data) {
                $("#resendButtonClick").attr("disabled", false);
                if ($.type(data) == "object") {
                    if (data.Data.result == "success") {
                        CheckMailSettingsAndNotify("Activation code has been generated successfully. As email settings are not configured we are not able to send activation email to the user. <a href='/administration/e-mail-settings'>Please click here to configure the email settings.</a>", $("#AlertMessage"), "Activation code has been resent successfully.");
                    }
                } else {

                }
            }
        });
    });

    $('#uploadImage').click(function () {
        var isUpdated = $(".img-container").children("img").attr("src");
        var userId = $("#userId").val();
        var isNewFile = false;

        if (isUpdated != "/Content/Images/Preview.jpg") {
            isNewFile = true;

            var data =
            {
                "selection.LeftOfCropArea": parseInt($('input[name=LeftOfCropArea]').val()),
                "selection.TopOfCropAea": parseInt($('input[name=TopOfCropAea]').val()),
                "selection.LeftToCropArea": parseInt($('input[name=LeftToCropArea]').val()),
                "selection.TopToCropArea": parseInt($('input[name=TopToCropArea]').val()),
                "selection.height": parseInt($('input[name=height]').val()),
                "selection.width": parseInt($('input[name=width]').val()),
                "selection.UserName": $("#user_name").val(),
                "selection.UserId": userId,
                "selection.ImageName": $("#Image").val(),
                "selection.IsNewFile": isNewFile
            };

            $.ajax({
                type: "POST",
                data: JSON.stringify(data),
                url: "/user/updateprofilepicture",
                contentType: "application/json; charset=utf-8",
                beforeSend: ShowWaitingProgress("#avatarUploadBox", "show"),
                dataType: "json",
                success: function (result) {
                    messageBox("su-open", "Change Profile picture", "Profile picture has been saved successfully.", "success", function () {
                        $("#profile-picture1").attr("src", "/user/avatar?username=" + $("#user_name").val() + "&imageSize=150");
                        $("#LastModified").html(result.formattedString);
                        $(".img-view-holder").append('<span class="su su-delete" id="avatarDeleteclick" title="Delete profile picture"></span>');
                        onCloseMessageBox();
                    });
                    $("#imagepath").val("browse image path");
                    $("#profile-picture").attr("src", "/Content/Images/Preview.jpg");
                    $('#uploadImage').attr("disabled", "disabled");
                    if ($(".img-container").children().hasClass("jcrop-active")) {
                        $('#profile-picture').data('Jcrop').destroy();
                    }
                    $("#avatarUploadBox").ejDialog("close");
                    ShowWaitingProgress("#avatarUploadBox", "hide");
                },
                error: function (result) {
                    parent.messageBox("su-open", "Change Profile picture", "Failed to update the Profile picture, try again later.", "error", function () {

                        parent.onCloseMessageBox();
                    });
                }
            });
        } else {

        }

    });

    $(document).on("click", "#avatarDeleteclick", function () {
        messageBox("su-delete", "Delete Profile Picture", "Are you sure you want to delete the profile picture?", "error", function () {
            deleteUserAvatar();
        });
    });

    $("#avatarButtonclick").click(function () {

        $("#imagepath").val("browse image path").removeClass("ValidationErrorImage");
        $("#imagepath").closest("div").removeClass("has-error");

        $("#avatarUploadBox").ejDialog("open");

        $("#cancelAvatarPopup").click(function () {
            $("#profile-picture").attr("src", "/Content/Images/Preview.jpg");
            $('#uploadImage').attr("disabled", "disabled");
            if ($(".img-container").children().hasClass("jcrop-active")) {
                $('#profile-picture').data('Jcrop').destroy();
            }
            $("#avatarUploadBox").ejDialog("close");
        });

    });

    $("#upload-picture").ejUploadbox({
        saveUrl: "/fileupload/Upload?imageType=profileimage&&userName=" + $("#user_name").val() + "&&timeStamp=" + currentDate,
        autoUpload: true,
        showFileDetails: false,
        fileSize: 31457280,
        extensionsAllow: ".PNG,.png,.jpg,.JPG,.jpeg,.JPEG",
        extensionsDeny: "",
        buttonText: { browse: "..." },
        begin: function () {
            ShowWaitingProgress("#avatarUploadBox", "show");
        },
        fileSelect: function (e) {
            extension = e[0].extension.toLowerCase();
            uploadFileName = e[0].name;
            this.model.saveUrl = "/fileupload/Upload?imageType=profileimage&&userName=" + $("#user_name").val() + "&&timeStamp=" + currentDate;
        },
        error: function (e) {
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg") {
                $("#imagepath").val("Invalid file format").addClass("ValidationErrorImage");
                $("#imagepath").closest("div").addClass("has-error");
                $(".e-uploadinput").val("");
            }
        },
        complete: function fileselect(e) {
            var filename = "profile_picture_" + currentDate + ".png";
            filename = filename.replace('"', '');
            $("#Image").val(filename);
            custompath = filename;
            $("#upload-picture").attr("data-filename", filename);
            $("#imagepath").val(uploadFileName).removeClass("ValidationErrorImage")
            $(".jcrop-selection.jcrop-current").children("button").css("background", "");
            $("#profile-picture").attr("src", "/content/images/ProfilePictures/" + $("#user_name").val() + "/" + filename + "?v=" + $.now());

            var cb, filter;

            jQuery(function ($) {
                var CircleSel = function () { };
                CircleSel.prototype = new $.Jcrop.component.Selection();

                $.extend(CircleSel.prototype, {
                    zoomscale: 1,
                    attach: function () {
                        this.frame.css({
                            background: 'url(' + $('#profile-picture')[0].src.replace('750', '750') + ')'
                        });
                    },
                    positionBg: function (b) {
                        var midx = (b.x + b.x2) / 2;
                        var midy = (b.y + b.y2) / 2;
                        var ox = (-midx * this.zoomscale) + (b.w / 2);
                        var oy = (-midy * this.zoomscale) + (b.h / 2);
                        this.frame.css({ backgroundPosition: -(b.x + 1) + 'px ' + (-b.y - 1) + 'px' });
                    },
                    redraw: function (b) {
                        $.Jcrop.component.Selection.prototype.redraw.call(this, b);

                        this.positionBg(this.last);
                        return this;
                    },
                    prototype: $.Jcrop.component.Selection.prototype
                });
                var jcrop_api;
                $('#profile-picture').Jcrop({

                    selectionComponent: CircleSel,
                    applyFilters: ['constrain', 'extent', 'backoff', 'ratio', 'round'],
                    aspectRatio: 1,
                    setSelect: [25, 25, 100, 100],
                    handles: ['n', 's', 'e', 'w'],

                    dragbars: [],
                    borders: [],
                    onChange: function (coordinates) {
                        onPictureCropEnd(coordinates);
                    }
                }, function () {
                    this.container.addClass('jcrop-circle-demo');
                    interface_load(this);
                    jcrop_api = this;
                });

                function interface_load(obj) {
                    cb = obj;
                    cb.container.append($('<div />').addClass('custom-shade'));

                    function random_coords() {
                        return [
                          Math.random() * 300,
                          Math.random() * 200,
                          (Math.random() * 540) + 50,
                          (Math.random() * 340) + 60
                        ];
                    }
                    $(document.body).on('click', '[data-setting]', function (e) {
                        var $targ = $(e.target),
                            setting = $targ.data('setting'),
                            value = $targ.data('value'),
                            opt = {};

                        opt[setting] = value;
                        cb.setOptions(opt);

                        $targ.closest('.btn-group').find('.active').removeClass('active');
                        $targ.addClass('active');

                        if ((setting == 'multi') && !value) {
                            var m = cb.ui.multi, s = cb.ui.selection;

                            for (var i = 0; i < m.length; i++)
                                if (s !== m[i]) m[i].remove();

                            cb.ui.multi = [s];
                            s.focus();
                        }

                        e.preventDefault();
                    });

                    $(document.body).on('click', '[data-action]', function (e) {
                        var $targ = $(e.target);
                        var action = $targ.data('action');

                        switch (action) {
                            case 'random-move':
                                cb.ui.selection.animateTo(random_coords());
                                break;
                        }

                        cb.ui.selection.refresh();

                    }).on('selectstart', function (e) {
                        e.preventDefault();
                    }).on('click', 'a[data-action]', function (e) {
                        e.preventDefault();
                    });
                }

            });
            ShowWaitingProgress("#avatarUploadBox", "hide");
            $('#uploadImage').removeAttr("disabled");
        }
    });


    $(".showHidePassword").on("mousedown", function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        }
        else {
            $(this).siblings("input").attr('type', 'password');
        }
    });

    $(".showHidePassword").on("mouseup", function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        }
        else {
            $(this).siblings("input").attr('type', 'password');
        }
    });
    $(".showHidePassword").mouseleave(function () {
        $(this).siblings("input").attr('type', 'password');
    });
});


function onChangePasswordClose() {
    $(".admin-password-change").val("");
    $("#changePasswordClickAction").show();
    $("#changePasswordSection").slideUp("slow");
}

function onChangePasswordClick() {
    if ($("#changePasswordSection").css("display") == "block") {
        $("#changePasswordSection").slideUp("slow");
        $(".arrow").children().addClass("su-arrow-next").removeClass("su-arrow-down");
    }
    else {
        $("#changePasswordSection").slideDown("slow");
        $(".arrow").children().removeClass("su-arrow-next").addClass("su-arrow-down");
    }
}

function onUserChangePasswordClick() {
    $("#password_updation_validation").html("");
    var userId = $("#userId").val();
    $("#confirm_password_validation").closest("td").prev("td").removeClass("has-error");
    var isValid = $('.change_password_form').valid();
    if (isValid) {
        if ($("#new_password").val() != $("#confirm_password").val()) {
            $("#confirm_password_validation").html("Passwords mismatch").css("display", "block");
            $("#confirm_password_validation").closest("td").prev("td").addClass("has-error");
        } else {
            var values = "userId=" + userId + "&newpassword=" + $("#new_password").val() + "&confirmpassword=" + $("#confirm_password").val();
            doAjaxPost("POST", "/UserManagement/UpdatePassword", values, function (result) {
                if (result.Data.status) {
                    $(".admin-password-change").val("");
                    $("#password_updation_validation").html(result.Data.value).css("display", "block");
                }
                else {
                    $(".admin-password-change").val("");
                    $("#password_updation_validation").html(result.Data.value).css("display", "block");
                }
            });
        }
    }
}

function SaveUserdetails() {
    var firstName = $("#user_firstname").val().trim();
    var emailid = $('#user_email').val().trim();
    $("#invalid_email").closest("td").prev("td").removeClass("has-error");
    var isValid = $('.user_profile_form').valid();

    if (isValid) {
        $(".userprofile_validation_messages").css("display", "none");

        var userStatus = $("#user_status").val();
        var isActive = (typeof userStatus != "undefined") ? userStatus : false;

        doAjaxPost('POST',
            '/usermanagement/UpdateUserProfile',
            {
                username: $("#user_name").val(),
                email: emailid,
                picturename: $("#upload-picture").attr("data-filename"),
                firstname: firstName,
                lastname: $("#user_lastname").val(),
                mobile: $("#contact_no").val(),
                status: isActive
            },
             function (result) {

                 if (result.Data.status) {
                     $("#LastModified").html(result.formattedString);
                     $("#updation_validation_message").html(result.Data.value).css("display", "block");
                 }
                 else if (!result.Data.status && result.Data.key == "email") {
                     $("#invalid_email").html(result.Data.value).css("display", "block");
                     $("#invalid_email").closest("td").prev("td").addClass("has-error");
                 }
                 else {
                     $("#updation_validation_message").html(result.Data.value).css("display", "block");
                 }
             }
        );
    }
}

function onPictureCropEnd(coordinates) {
    $("input[name=LeftOfCropArea]").val(coordinates.x);
    $("input[name=TopOfCropAea]").val(coordinates.y);
    $("input[name=LeftToCropArea]").val(coordinates.x2);
    $("input[name=TopToCropArea]").val(coordinates.y2);
    $("input[name=height]").val(coordinates.h);
    $("input[name=width]").val(coordinates.w);
}

function deleteSingleUser(userId) {
    doAjaxPost("POST", "/UserManagement/DeleteSingleFromUserList", "UserId=" + userId, function (data) {
        if (data.toLowerCase() == "true") {
            window.location.href = "/administration/user-management/users";
        } else {
            //validation msg
        }
    });
}

function onDeleteDialogClose() {
    $("#userprofile_delete_confirmation").ejDialog("close");
}

function onDeleteDialogOpen() {
    $("#userprofile_delete_confirmation").ejDialog("open");
}