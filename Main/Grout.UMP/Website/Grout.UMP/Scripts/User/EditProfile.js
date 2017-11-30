var isKeyUp = false;
$(document).ready(function () {
    var extension;
    var custompath;
    var currentDate = $.now();
    var uploadFileName;
    $("#avatarUploadBox").ejDialog({
        showOnInit: false,
        allowDraggable: true,
        enableResize: false,
        height: "425px",
        width: "600px",
        enableModal: true,
        showHeader: false,
        close: "DialogBoxClose",
        closeOnEscape: true
    });

    $.validator.addMethod("isValidEmail", function (value, element) {
        if (isKeyUp)
            return true;
        else
            return IsEmail(value)
    }, "Please enter a valid email address");

    $.validator.addMethod("isValidName", function (value, element) {
        return IsValidName("name", value)
    }, "Please avoid special characters");

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");


    $('.edit_profile_form').validate({
        errorElement: 'span',
        onkeyup: function (element, event) {
            if (event.keyCode != 9) {
                isKeyUp = true;               
                $(element).valid();
                isKeyUp = false
            } else true
        },
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
            $(element).closest('td').next("td").find("span").html(error.html());
        },
        messages: {
            "email-address": {
                isRequired: "Please enter your email address"
            },
            "first-name": {
                isRequired: "Please enter your first name"
            }
        }

    });

    $('.change_password_form').validate({
        errorElement: 'span',
        onkeyup: function (element, event) {
            if (event.keyCode != 9) {                
                $(element).valid();
            } else true;
        },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "old-password": {
                required: true
            },
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
            "old-password": {
                required: "Please enter your old password"
            },
            "new-password": {
                required: "Please enter your new password"
            },
            "confirm-password": {
                required: "Please confirm your new password"
            }
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
                    parent.messageBox("su-open", "Change Profile picture", "Profile picture has been saved successfully.", "success", function () {
                        parent.$("#profile-picture1").attr("src", "/user/avatar?username=" + $("#user_name").val() + "&imageSize=150");
                        parent.$("#profile-picture-menu").find("img").attr("src", "/user/avatar?username=" + $("#user_name").val() + "&imageSize=32");
                        parent.$("#LastModified").html(result.formattedString);
                        $(".img-view-holder").append('<span class="su su-delete" id="avatarDeleteclick" title="Delete profile picture"></span>');
                        parent.onCloseMessageBox();
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
            $("#profile-picture-menu").find("img").attr("src", "/user/avatar?username=" + $("#user_name").val() + "&imageSize=32");
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
        buttonText: { browse: "..." },
        begin: function () {
            ShowWaitingProgress("#avatarUploadBox", "show");
        },
        fileSelect: function (e) {
            extension = e[0].extension.toLowerCase();
            uploadFileName = e[0].name;
            this.model.saveUrl = "/fileupload/Upload?imageType=profileimage&&userName=" + $("#user_name").val() + "&&timeStamp=" + currentDate
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
            $("#imagepath").removeClass("ValidationErrorImage");
            $("#Image").removeClass("ValidationErrorImage").val(filename);
            custompath = filename;
            $("#upload-picture").attr("data-filename", filename);
            $("#imagepath").val(uploadFileName);
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

});


function editUser(fulldata) {
    var specficuserdetails = fulldata;
    $("#user_name").val(specficuserdetails.UserName);
    $("#user_email").val(specficuserdetails.Email);
    $("#UserHead").html(specficuserdetails.FirstName + " " + specficuserdetails.LastName);
    if (specficuserdetails.Avatar == "") {
        $("#profile-picture1").siblings("#avatarDeleteclick").remove();
    }
    $("#profile-picture1").attr('src', "/User/Avatar?Username=" + specficuserdetails.UserName + "&ImageSize=150");
    $("#upload-picture").attr("data-filename", specficuserdetails.Avatar.replace("Content//images//ProfilePictures//" + specficuserdetails.UserName + "//", ""));

    if (fulldata.FirstName != null && fulldata.FirstName != "") {
        $("#user_firstname").val(fulldata.FirstName);
    }
    else {
        $("#user_firstname").val(specficuserdetails.FullName);
    }
    if (fulldata.LastName != null && fulldata.LastName != "") {
        $("#user_lastname").val(fulldata.LastName);
    }
    if (fulldata.ContactNumber != null && fulldata.ContactNumber != "") {
        $("#contact_no").val(fulldata.ContactNumber);
    }
}

function onEditProfileClick() {
    $("#success_message").html("");
    $("#email_duplicate_validation").closest("td").prev("td").removeClass("has-error");
    var isValid = $('.edit_profile_form').valid();

    if (isValid) {
        ShowWaitingProgress("#UserProfileMaster", "show");
        doAjaxPost('POST',
            '/user/UpdateUserProfile',
            {
                username: $("#user_name").val(),
                email: $("#user_email").val(),
                picturename: $("#upload-picture").attr("data-filename"),
                firstname: $("#user_firstname").val(),
                lastname: $("#user_lastname").val(),
                mobile: $("#contact_no").val()
            },
            function (result) {
                ShowWaitingProgress("#UserProfileMaster", "hide");
                if (result.Data.status) {
                    $("#LastModified").html(result.formattedString);
                    $("#success_message").html("<span class='validate-success'>Profile has been updated successfully.</span>");
                    $("#profile_display_name").html($("#user_firstname").val() + " " + $("#user_lastname").val());
                    $("#UserHead").html($("#user_firstname").val() + " " + $("#user_lastname").val());
                } else if (!result.Data.status && result.Data.key == "email") {
                    $("#email_duplicate_validation").html(result.Data.value).css("display", "block");
                    $("#email_duplicate_validation").closest("td").prev("td").addClass("has-error");
                } else {
                    $("#success_message").html(result.Data.value);
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

function onChangePasswordClick() {
    $(".password-validate-holder").html("");
    $("#confirm_pasword_validate, #confirm_pasword_validate").closest("td").prev("td").removeClass("has-error");
    var isValid = true;
    var isValidForm = $('.change_password_form').valid();

    if (isValidForm && $("#new_password").val() != $("#confirm_password").val()) {
        $("#confirm_pasword_validate").html("Passwords mismatch");
        $("#confirm_pasword_validate").closest("td").prev("td").addClass("has-error");
        isValid = false;
    }

    if (isValid == false) {
        return;
    }

    ShowWaitingProgress("#UserProfileMaster", "show");
    doAjaxPost('POST', baseurl + '/user/updatepassword', { oldpassword: $("#old_password").val(), newpassword: $("#new_password").val(), confirmpassword: $("#confirm_password").val() },
                 function (result) {
                     $("input[type='password']").val("");
                     ShowWaitingProgress("#UserProfileMaster", "hide");
                     if (!result.Data.status && result.Data.key == "password") {
                         $("#old_pasword_validate").html(result.Data.value);
                         $("#old_pasword_validate").closest("td").prev("td").addClass("has-error");
                     }
                     else {
                         $("#password_validation").html("<span class='validate-success'>" + result.Data.value + "</span>").css("visibility", "visible");
                     }
                 }
            );
}

function onEditProfileCancel() {
    location.href = "/";
}


